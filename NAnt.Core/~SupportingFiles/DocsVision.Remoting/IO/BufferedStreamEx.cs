//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Threading;

namespace DocsVision.IO
{
	// This delegate is used in async data buffering
	internal delegate void DataAvailableCallback(Exception ex, object state);

	internal sealed class BufferedStreamEx : Stream
	{
		// Intermediate buffer for buffered operations
		private static int _bufferSize = 4096;
		private byte[] _buffer = new byte[_bufferSize];
		private int _dataCount;
		private int _readPos;
		private int _writePos;

		// Underlying stream
		private Stream _stream;

		// Callback for async data buffering
		private AsyncCallback _dataAvailableCallback;
		private object _dataAvailableState;

		#region Constructors

		public BufferedStreamEx(Stream dataStream)
		{
			// parameters validation
			if (dataStream == null)
				throw new ArgumentNullException("dataStream");

			_stream = dataStream;
			_dataAvailableCallback = new AsyncCallback(OnDataAvailable);
		}

		#endregion

		#region Stream Properties

		public override bool CanRead
		{
			get
			{
				return _stream.CanRead;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _stream.CanWrite;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return _stream.CanSeek;
			}
		}

		public override long Length
		{
			get
			{
				if (_writePos > 0)
				{
					// flush buffered data to the stream
					FlushWrite();
				}

				return _stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return _stream.Position - _dataCount + _writePos;
			}
			set
			{
				if (_writePos > 0)
				{
					// flush buffered data to the stream
					FlushWrite();
				}

				if (_stream.CanSeek)
				{
					_stream.Seek(value, SeekOrigin.Begin);
				}

				_readPos = 0;
				_dataCount = 0;
			}
		}

		#endregion

		#region Stream Methods

		public override int Read(byte[] buffer, int offset, int size)
		{
			int totalBytesRead = 0;

			if (_writePos > 0)
			{
				// flush buffered data to the stream
				FlushWrite();
			}
			else
			{
				// read from internal buffer first
				totalBytesRead += ReadFromBuffer(buffer, ref offset, ref size);
			}

			while (size > 0)
			{
				if (size < _bufferSize)
				{
					// if size is less than buffer length, we will buffer more data
					BufferMoreData();

					// read data from buffer
					totalBytesRead += ReadFromBuffer(buffer, ref offset, ref size);
				}
				else
				{
					// the internal buffer is guaranteed to be empty at this point,
					// so just read directly into the array given
					int bytesRead = _stream.Read(buffer, offset, size);
					if (bytesRead <= 0)
						throw new IOException("No data available");

					size -= bytesRead;
					offset += bytesRead;
					totalBytesRead += bytesRead;
				}
			}

			return totalBytesRead;
		}

		public override int ReadByte()
		{
			if (_writePos > 0)
			{
				// flush buffered data to the stream
				FlushWrite();
			}
			if (_dataCount == 0)
			{
				// buffer more data
				BufferMoreData();
			}

			--_dataCount;
			return _buffer[_readPos++];
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			if (_dataCount > 0)
			{
				// synchronize stream position
				FlushRead();
			}

			// fill internal buffer first
			WriteToBuffer(buffer, ref offset, ref size);

			if (size > 0)
			{
				// buffer is full, so flush it to the stream
				FlushWrite();

				if (size < _bufferSize)
				{
					// if size is less than buffer length, we will buffer more data
					WriteToBuffer(buffer, ref offset, ref size);
				}
				else
				{
					// the internal buffer is guaranteed to be empty at this point,
					// so just write directly into the stream
					_stream.Write(buffer, offset, size);
				}
			}
		}

		public override void WriteByte(byte value)
		{
			if (_dataCount > 0)
			{
				// synchronize stream position
				FlushRead();
			}
			else if (_writePos == _bufferSize)
			{
				// buffer is full, so flush it to the stream
				FlushWrite();
			}

			_buffer[_writePos++] = value;
		}

		public override void Flush()
		{
			if (_dataCount > 0)
			{
				// synchronize stream position
				FlushRead();
			}
			else if (_writePos > 0)
			{
				// flush buffered data to the stream
				FlushWrite();
				_stream.Flush();
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long length)
		{
			_stream.SetLength(length);
		}

		public override void Close()
		{
			Flush();
			_stream.Close();
		}

		#endregion

		/// <summary>
		/// Returns true, if there is some data in the buffer
		/// </summary>
		public bool DataAvailable
		{
			get
			{
				return _dataCount > 0;
			}
		}

		/// <summary>
		/// Asynchronously reads data from stream to fill buffer
		/// </summary>
		public void FillBuffer(DataAvailableCallback callback, object state)
		{
			if (_dataCount > 0)
			{
				// schedule client callback execution
				_dataAvailableState = state;
				ThreadPool.QueueUserWorkItem(new WaitCallback(InvokeCallback), callback);
			}
			else
			{
				if (_writePos > 0)
				{
					// flush buffered data to the stream
					FlushWrite();
				}

				// begin read operation
				_dataAvailableState = state;
				_stream.BeginRead(_buffer, 0, _bufferSize, _dataAvailableCallback, callback);
			}
		}

		private void InvokeCallback(object state)
		{
			// invoke client callback
			DataAvailableCallback callback = (DataAvailableCallback)state;
			callback(null, _dataAvailableState);
		}

		private void OnDataAvailable(IAsyncResult result)
		{
			DataAvailableCallback callback = (DataAvailableCallback)result.AsyncState;
			Exception readException = null;

			try
			{
				// complete read operation
				_readPos = 0;
				_dataCount = _stream.EndRead(result);

				if (_dataCount <= 0)
					readException = new IOException("No data available");
			}
			catch (Exception ex)
			{
				// unable to read data, connection is broken?
				readException = ex;
			}

			// invoke client callback
			callback(readException, _dataAvailableState);
		}

		private void BufferMoreData()
		{
			_readPos = 0;
			_dataCount = _stream.Read(_buffer, 0, _bufferSize);
			if (_dataCount <= 0)
				throw new IOException("No data available");
		}

		private int ReadFromBuffer(byte[] buffer, ref int offset, ref int size)
		{
			int bytesRead = (_dataCount < size ? _dataCount : size);
			if (bytesRead > 0)
			{
				Buffer.BlockCopy(_buffer, _readPos, buffer, offset, bytesRead);
				_readPos += bytesRead;
				_dataCount -= bytesRead;
				size -= bytesRead;
				offset += bytesRead;
			}

			return bytesRead;
		}

		private void WriteToBuffer(byte[] buffer, ref int offset, ref int size)
		{
			int bytesAvailable = _bufferSize - _writePos;
			int bytesWrite = (bytesAvailable < size ? bytesAvailable : size);
			if (bytesWrite > 0)
			{
				Buffer.BlockCopy(buffer, offset, _buffer, _writePos, bytesWrite);
				_writePos += bytesWrite;
				size -= bytesWrite;
				offset += bytesWrite;
			}
		}

		private void FlushRead()
		{
			if (_stream.CanSeek)
			{
				_stream.Seek(_dataCount, SeekOrigin.Current);
			}

			_readPos = 0;
			_dataCount = 0;
		}

		private void FlushWrite()
		{
			_stream.Write(_buffer, 0, _writePos);
			_writePos = 0;
		}
	}
}