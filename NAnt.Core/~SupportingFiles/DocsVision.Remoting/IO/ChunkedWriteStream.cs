//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;

using DocsVision.Util;

namespace DocsVision.IO
{
	internal sealed class ChunkedWriteStream : Stream
	{
		// We use buffer pool for chunk allocations to save some time to GC
		private IByteBufferPool _bufferPool;

		// Underlying stream
		private Stream _stream;

		// Buffer for chunked write
		private byte[] _buffer;
		private int _bufferSize;
		private int _writePos;

		#region Constructors

		public ChunkedWriteStream(Stream stream) : this(stream, ByteBufferPool.DefaultBufferPool)
		{
		}

		public ChunkedWriteStream(Stream stream, IByteBufferPool bufferPool)
		{
			// parameters validation
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (bufferPool == null)
				throw new ArgumentNullException("bufferPool");

			_stream = stream;
			_bufferPool = bufferPool;
			_buffer =  bufferPool.GetBuffer();
			_bufferSize = bufferPool.GetBufferSize();
		}

		#endregion

		#region Stream Properties

		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		#endregion

		#region Stream Methods

		public override int Read(byte[] buffer, int offset, int size)
		{
			throw new NotSupportedException();
		}

		public override int ReadByte()
		{
			throw new NotSupportedException();
		}

		public override void Close()
		{
			if (_writePos > 0)
			{
				// write last chunk
				FlushWrite();
			}

			// write NULL chunk
			WriteChunkLength(0);
			_stream = null;

			if (_buffer != null)
			{
				_bufferPool.ReturnBuffer(_buffer);
				_buffer = null;
			}
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
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
					WriteChunkLength(size);
					_stream.Write(buffer, offset, size);
				}
			}
		}

		public override void WriteByte(byte value)
		{
			if (_writePos == _bufferSize)
			{
				// buffer is full, so flush it to the stream
				FlushWrite();
			}

			_buffer[_writePos++] = value;
		}

		public override void Flush()
		{
			if (_writePos > 0)
			{
				FlushWrite();
				_stream.Flush();
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long length)
		{
			throw new NotSupportedException();
		}

		#endregion

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

		private void FlushWrite()
		{
			WriteChunkLength(_writePos);
			_stream.Write(_buffer, 0, _writePos);
			_writePos = 0;
		}

		private void WriteChunkLength(int value)
		{
			uint v = (uint) value;   // support negative numbers
			while (value >= 0x80)
			{
				_stream.WriteByte((byte) (value | 0x80));
				value >>= 7;
			}
			_stream.WriteByte((byte)value);
		}
	}
}