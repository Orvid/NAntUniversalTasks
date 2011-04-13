//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;

namespace DocsVision.Runtime.Remoting.Transport
{
	public class TransportStream : Stream, IDisposable
	{
		// Associated transport
		private IAsyncTransport _transport;
		private bool _ownsTransport;

		// Transport access mode
		private FileAccess _accessMode;

		// Timeout
		private int _timeout;

		// Object state
		private bool _disposed;

		#region Constructors

		public TransportStream(ITransport transport) : this(transport, FileAccess.ReadWrite, false)
		{
		}

		public TransportStream(ITransport transport, bool ownsTransport) : this(transport, FileAccess.ReadWrite, ownsTransport)
		{
		}

		public TransportStream(ITransport transport, FileAccess accessMode) : this(transport, accessMode, false)
		{
		}

		public TransportStream(ITransport transport, FileAccess accessMode, bool ownsTransport)
		{
			// parameters validation
			if (transport == null)
				throw new ArgumentNullException("pipe", "Transport can not be null");
			if (!transport.IsConnected)
				throw new ArgumentException("Transport not connected", "transport");

			_accessMode = accessMode;
			_ownsTransport = ownsTransport;
			_transport = TransportAsyncWrapper.GetWrapper(transport);
		}

		#endregion

		#region Disposing

		~TransportStream()
		{
			Dispose(false);
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		new protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (_transport != null)
			{
				if (_ownsTransport)
				{
					// close transport connection
					_transport.Close();
				}

				_transport = null;
			}

			_disposed = true;
		}

		#endregion

		#region Stream Properties

		public override bool CanRead
		{
			get
			{
				return (_accessMode == FileAccess.ReadWrite || _accessMode == FileAccess.Read);
			}
		}

		public override bool CanWrite
		{
			get
			{
				return (_accessMode == FileAccess.ReadWrite || _accessMode == FileAccess.Write);
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

		public TimeSpan Timeout
		{
			get
			{
				return TimeSpan.FromMilliseconds(_timeout);
			}
			set
			{
				_timeout = (int)value.TotalMilliseconds;
			}
		}

		#endregion

		#region Stream Methods

		public override int Read(byte[] buffer, int offset, int size)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			if (_timeout == 0)
			{
				return _transport.Receive(buffer, offset, size);
			}
			else
			{
				IAsyncResult result = _transport.BeginReceive(buffer, offset, size, null, null);
				if (!result.CompletedSynchronously)
				{
					result.AsyncWaitHandle.WaitOne(_timeout, false);
					if (!result.IsCompleted)
						throw new IOException("Operation timeout expired");
				}
				return _transport.EndReceive(result);
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			return _transport.BeginReceive(buffer, offset, size, callback, state);
		}

		public override int EndRead(IAsyncResult result)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			return _transport.EndReceive(result);
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			_transport.Send(buffer, offset, size);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			return _transport.BeginSend(buffer, offset, size, callback, state);
		}

		public override void EndWrite(IAsyncResult result)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			_transport.EndSend(result);
		}

		public override void Close()
		{
			((IDisposable)this).Dispose();
		}

		public override void Flush()
		{
			_transport.Flush();
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

		/// <summary>
		/// Underlying network transport
		/// </summary>
		public ITransport Transport
		{
			get
			{
				return _transport;
			}
		}
	}
}