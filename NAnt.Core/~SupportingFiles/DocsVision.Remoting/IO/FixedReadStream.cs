//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;

namespace DocsVision.IO
{
	internal sealed class FixedReadStream : Stream
	{
		// Underlying stream
		private Stream _stream;

		// Fixed stream length
		private int _length;
		private int _bytesLeft;

		#region Constructors

		public FixedReadStream(Stream stream, int contentLength)
		{
			// parameters validation
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (contentLength <= 0)
				throw new ArgumentOutOfRangeException("contentLength");

			_stream = stream;
			_length = contentLength;
			_bytesLeft = contentLength;
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
				return _length;
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

			if (_bytesLeft == 0)
				throw new IOException("No more data available");

			int bytesRead = _stream.Read(buffer, offset, (_bytesLeft < size ? _bytesLeft : size));
			_bytesLeft -= bytesRead;

			return bytesRead;
		}

		public override int ReadByte()
		{
			// check object state
			if (_stream == null)
				throw new IOException("Stream is closed");

			if (_bytesLeft == 0)
				throw new IOException("No more data available");

			byte value = (byte)_stream.ReadByte();
			--_bytesLeft;

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
	}
}