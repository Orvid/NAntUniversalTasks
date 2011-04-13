//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Threading;

namespace DocsVision.Util
{
	internal interface IByteBufferPool
	{
		int GetBufferSize();
		byte[] GetBuffer();
		void ReturnBuffer(byte[] buffer);
	}

	internal sealed class FakeByteBufferPool : IByteBufferPool
	{
		public static readonly IByteBufferPool DefaultBufferPool = new FakeByteBufferPool(4096);

		private int _bufferSize;

		public FakeByteBufferPool(int bufferSize)
		{
			_bufferSize = bufferSize;
		}

		public int GetBufferSize()
		{
			return _bufferSize;
		}

		public byte[] GetBuffer()
		{
			return new byte[_bufferSize];
		}

		public void ReturnBuffer(byte[] buffer)
		{
		}
	}

	internal sealed class ByteBufferPool : IByteBufferPool
	{
		public static readonly IByteBufferPool DefaultBufferPool = new ByteBufferPool(4096, 10);

		private byte[][] _bufferPool;
		private int _bufferSize;
		private int _maxPoolSize;

		private int _current; // -1 for none
		private int _last;

		private int _controlCookie = 1;

		public ByteBufferPool(int bufferSize, int maxPoolSize)
		{
			_bufferSize = bufferSize;
			_maxPoolSize = maxPoolSize;
			_bufferPool = new byte[_maxPoolSize][];

			_current = -1;
			_last = -1;
		}

		public int GetBufferSize()
		{
			return _bufferSize;
		}

		public byte[] GetBuffer()
		{
			byte[] buffer = null;

			// we use Interlocked.Exchange as a "light weight" thread syncronization
			int cookie = Interlocked.Exchange(ref _controlCookie, 0);
			if (cookie == 1)
			{
				try
				{
					if (_current == -1)
					{
						// no pooled buffers available, create a new buffer
						buffer = new byte[_bufferSize];
					}
					else
					{
						// grab next available buffer
						buffer = _bufferPool[_current];
						_bufferPool[_current] = null;

						// update "current" index
						if (_current == _last)
						{
							_current = -1;
						}
						else
						{
							_current = (_current + 1) % _maxPoolSize;
						}
					}
				}
				catch
				{
					// reset pool to the "known good" configuration
					_current = -1;
					_last = -1;
					throw;
				}
				finally
				{
					// restore cookie
					_controlCookie = 1;
				}
			}
			else
			{
				// we don't have the control cookie, so just create a new buffer
				buffer = new byte[_bufferSize];
			}

			return buffer;
		}

		public void ReturnBuffer(byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			// we use Interlocked.Exchange as a "light weight" thread syncronization
			int cookie = Interlocked.Exchange(ref _controlCookie, 0);
			if (cookie == 1)
			{
				try
				{
					if (_current == -1)
					{
						// this is the first buffer in the pool
						_bufferPool[0] = buffer;
						_current = 0;
						_last = 0;
					}
					else
					{
						int newLast = (_last + 1) % _maxPoolSize;
						if (newLast != _current)
						{
							// the pool isn't full so store this buffer
							_last = newLast;
							_bufferPool[_last] = buffer;
						}
					}
				}
				catch
				{
					// reset pool to the "known good" configuration
					_current = -1;
					_last = -1;
					throw;
				}
				finally
				{
					// restore cookie
					_controlCookie = 1;
				}
			}
		}
	}
}