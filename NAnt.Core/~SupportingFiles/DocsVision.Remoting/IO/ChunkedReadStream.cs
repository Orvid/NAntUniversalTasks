//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;

namespace DocsVision.IO
{
	internal sealed class ChunkedReadStream : Stream
	{
		// Underlying stream
		private Stream _stream;
		private int _bytesLeft;
		private bool _foundEnd;

		#region Constructors

		public ChunkedReadStream(Stream stream)
		{
			// parameters validation
			if (stream == null)
				throw new ArgumentNullException("stream");

			_stream = stream;

			// read first chunk length
			_bytesLeft = ReadChunkLength();
			if (_bytesLeft == 0)
			{
				_foundEnd = true;
			}
		}

		#endregion

		#region Stream Properties

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
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
			// check object state
			if (_stream == null)
				throw new IOException("Stream is closed");

			int totalBytes = 0;

			while (size > 0)
			{
				if (_foundEnd)
					throw new IOException("No more data available");

				// read chunk data
				int bytesToRead = (_bytesLeft < size ? _bytesLeft : size);
				int bytesRead = _stream.Read(buffer, offset, bytesToRead);
				if (bytesRead != bytesToRead)
					throw new IOException("Stream chunk is invalid");

				totalBytes += bytesRead;
				offset += bytesRead;
				size -= bytesRead;
				_bytesLeft -= bytesRead;

				if (_bytesLeft == 0)
				{
					// read next chunk length
					_bytesLeft = ReadChunkLength();
					if (_bytesLeft == 0)
					{
						_foundEnd = true;
					}
				}
			}

			return totalBytes;
		}

		public override int ReadByte()
		{
			// check object state
			if (_stream == null)
				throw new IOException("Stream is closed");

			if (_foundEnd)
				throw new IOException("No more data available");

			// read byte
			byte value = (byte)_stream.ReadByte();
			--_bytesLeft;

			if (_bytesLeft == 0)
			{
				// read next chunk length
				_bytesLeft = ReadChunkLength();
				if (_bytesLeft == 0)
				{
					_foundEnd = true;
				}
			}

			return value;
		}

		public override void Close()
		{
			_stream = null;
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			throw new NotSupportedException();
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

		private int ReadChunkLength()
		{
			int count = 0;
			int shift = 0;
			byte b;
			do
			{
				b = (byte)_stream.ReadByte();
				count |= (b & 0x7F) << shift;
				shift += 7;
			} while ((b & 0x80) != 0);
			return count;
		}
	}
}