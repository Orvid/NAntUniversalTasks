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
	internal sealed class ChunkedMemoryStream : Stream
	{
		// Chunks stored in a simple linked list
		private sealed class MemoryChunk
		{
			public byte[] Buffer;
			public MemoryChunk Next;
		}

		// We use buffer pool for chunk allocations to save some time to GC
		private IByteBufferPool _bufferPool;

		private MemoryChunk _chunks;	// list head
		private MemoryChunk _readChunk;
		private MemoryChunk _writeChunk;
		private int _readOffset;
		private int _writeOffset;

		// Object state
		private bool _closed;

		#region Constructors

		public ChunkedMemoryStream() : this(ByteBufferPool.DefaultBufferPool)
		{
		}

		public ChunkedMemoryStream(IByteBufferPool bufferPool)
		{
			// parameters validation
			if (bufferPool == null)
				throw new ArgumentNullException("bufferPool");

			_bufferPool = bufferPool;
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

		public override bool CanSeek
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
				return true;
			}
		}

		public override long Length
		{
			get
			{
				// check object state
				if (_closed)
					throw new IOException("Stream is closed");
				if (_chunks == null)
					return 0;	// there is no data available

				// total size is filled_chunks_count * chunk_size + used_bytes_in_last_chunk
				return (GetChunksCount() - 1) * GetChunkSize() + _writeOffset;
			}
		}

		public override long Position
		{
			get
			{
				// check object state
				if (_closed)
					throw new IOException("Stream is closed");
				if (_readChunk == null)
					return 0;	// no data was readed

				// position is current_chunk_index * chunk_size + read_offset_in_current_chunk
				return GetChunkIndex(_readChunk) * GetChunkSize() + _readOffset;
			}
			set
			{
				// check object state
				if (_closed)
					throw new IOException("Stream is closed");
				if (value < 0)
					throw new ArgumentOutOfRangeException("value");

				MemoryChunk chunk = GetChunk((int)value / GetChunkSize());
				if (chunk == null)
					throw new ArgumentOutOfRangeException("value");

				_readChunk = chunk;
				_readOffset = (int)value % GetChunkSize();
			}
		}

		#endregion

		#region Stream Operations

		public override void Close()
		{
			if (_chunks != null)
			{
				ReleaseMemoryChunks(_chunks);
			}

			_chunks = null;
			_readChunk = null;
			_writeChunk = null;

			_closed = true;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			// nothing to do...
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			// check object state
			if (_closed)
				throw new IOException("Stream is closed");

			switch (origin)
			{
				case SeekOrigin.Begin:
					Position = offset;
					break;

				case SeekOrigin.Current:
					Position = Position + offset;
					break;

				case SeekOrigin.End:
					Position = Length + offset;
					break;
			}

			return Position;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			// check object state
			if (_closed)
				throw new IOException("Stream is closed");

			if (_readChunk == null)
			{
				if (_chunks == null)
					return 0;	// there is no data available
				_readChunk = _chunks;
				_readOffset = 0;
			}

			byte[] chunkBuffer = _readChunk.Buffer;
			int chunkSize = (_readChunk.Next == null ? _writeOffset : chunkBuffer.Length);
			int bytesRead = 0;

			while (count > 0)
			{
				if (_readOffset == chunkSize)
				{
					if (_readChunk.Next != null)
					{
						// jump to the next chunk
						_readChunk = _readChunk.Next;
						_readOffset = 0;

						chunkBuffer = _readChunk.Buffer;
						chunkSize = (_readChunk.Next == null ? _writeOffset : chunkBuffer.Length);
					}
					else
					{
						// there are no more chunks are currently available
						break;
					}
				}

				// read chunk content
				int readCount = chunkSize - _readOffset;
				if (readCount > count) readCount = count;
				Buffer.BlockCopy(chunkBuffer, _readOffset, buffer, offset, readCount);
				offset += readCount;
				count -= readCount;
				_readOffset += readCount;
				bytesRead += readCount;
			}

			return bytesRead;
		}

		public override int ReadByte()
		{
			// check object state
			if (_closed)
				throw new IOException("Stream is closed");

			if (_readChunk == null)
			{
				if (_chunks == null)
					return 0;	// there is no data available
				_readChunk = _chunks;
				_readOffset = 0;
			}

			byte[] chunkBuffer = _readChunk.Buffer;
			int chunkSize = (_readChunk.Next == null ? _writeOffset : chunkBuffer.Length);

			if (_readOffset == chunkSize)
			{
				if (_readChunk.Next != null)
				{
					// jump to the next chunk
					_readChunk = _readChunk.Next;
					_readOffset = 0;

					chunkBuffer = _readChunk.Buffer;
				}
				else
				{
					// there are no more chunks are currently available
					return -1;
				}
			}

			return chunkBuffer[_readOffset++];
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			// check object state
			if (_closed)
				throw new IOException("Stream is closed");

			// parameters validation
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || count > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("count");

			if (_chunks == null)
			{
				// allocate first chunk
				_chunks = AllocateMemoryChunk();
				_writeChunk = _chunks;
				_writeOffset = 0;
			}

			byte[] chunkBuffer = _writeChunk.Buffer;
			int chunkSize = chunkBuffer.Length;

			while (count > 0)
			{
				if (_writeOffset == chunkSize)
				{
					// allocate a new chunk if the current one is full
					_writeChunk.Next = AllocateMemoryChunk();
					_writeChunk = _writeChunk.Next;
					_writeOffset = 0;

					chunkBuffer = _writeChunk.Buffer;
					chunkSize = chunkBuffer.Length;
				}

				// write chunk content
				int writeCount = chunkSize - _writeOffset;
				if (writeCount > count) writeCount = count;
				Buffer.BlockCopy(buffer, offset, chunkBuffer, _writeOffset, writeCount);
				offset += writeCount;
				count -= writeCount;
				_writeOffset += writeCount;
			}
		}

		public override void WriteByte(byte value)
		{
			// check object state
			if (_closed)
				throw new IOException("Stream is closed");

			if (_chunks == null)
			{
				// allocate first chunk
				_chunks = AllocateMemoryChunk();
				_writeChunk = _chunks;
				_writeOffset = 0;
			}

			byte[] chunkBuffer = _writeChunk.Buffer;
			int chunkSize = chunkBuffer.Length;

			if (_writeOffset == chunkSize)
			{
				// allocate a new chunk if the current one is full
				_writeChunk.Next = AllocateMemoryChunk();
				_writeChunk = _writeChunk.Next;
				_writeOffset = 0;

				chunkBuffer = _writeChunk.Buffer;
			}

			chunkBuffer[_writeOffset++] = value;
		}

		#endregion

		#region Chunk Routines

		private int GetChunkSize()
		{
			return _bufferPool.GetBufferSize();
		}

		private int GetChunksCount()
		{
			int chunksCount = 0;
			for (MemoryChunk chunk = _chunks; chunk != null; chunk = chunk.Next, ++chunksCount);
			return chunksCount;
		}

		private int GetChunkIndex(MemoryChunk chunkToFind)
		{
			int index = 0;
			for (MemoryChunk chunk = _chunks; chunk != null && chunk != chunkToFind; chunk = chunk.Next, ++index);
			return index;
		}

		private MemoryChunk GetChunk(int index)
		{
			MemoryChunk chunk =_chunks;
			for (; chunk != null && index > 0; chunk = chunk.Next, --index);
			return chunk;
		}

		private MemoryChunk AllocateMemoryChunk()
		{
			MemoryChunk chunk = new MemoryChunk();
			chunk.Buffer = _bufferPool.GetBuffer();
			return chunk;
		}

		private void ReleaseMemoryChunks(MemoryChunk chunk)
		{
			if (_bufferPool is FakeByteBufferPool)
				return;	// until we are using buffer pool there is nothing to do

			while (chunk != null)
			{
				_bufferPool.ReturnBuffer(chunk.Buffer);
				chunk = chunk.Next;
			}
		}

		#endregion

		public void WriteTo(Stream stream)
		{
			// check object state
			if (_closed)
				throw new IOException("Stream is closed");

			// parameters validation
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (_readChunk == null)
			{
				if (_chunks == null)
					return;	// there is no data available
				_readChunk = _chunks;
				_readOffset = 0;
			}

			byte[] chunkBuffer = _readChunk.Buffer;
			int chunkSize = (_readChunk.Next == null ? _writeOffset : chunkBuffer.Length);

			while (true)
			{
				if (_readOffset == chunkSize)
				{
					if (_readChunk.Next != null)
					{
						// jump to the next chunk
						_readChunk = _readChunk.Next;
						_readOffset = 0;

						chunkBuffer = _readChunk.Buffer;
						chunkSize = (_readChunk.Next == null ? _writeOffset : chunkBuffer.Length);
					}
					else
					{
						// there is no more chunks currently available
						break;
					}
				}

				int writeCount = chunkSize - _readOffset;
				stream.Write(chunkBuffer, _readOffset, writeCount);
				_readOffset += writeCount;
			}
		}

		public byte[] ToArray()
		{
			int size = (int)Length;	// will throw if stream is closed
			byte[] copy = new byte[size];

			MemoryChunk backupReadChunk = _readChunk;
			int backupReadOffset = _readOffset;

			_readChunk = _chunks;
			_readOffset = 0;
			Read(copy, 0, size);

			_readChunk = backupReadChunk;
			_readOffset = backupReadOffset;

			return copy;
		}
	}
}