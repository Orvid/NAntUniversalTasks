//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

using DocsVision.Security;
using DocsVision.Util;

namespace DocsVision.Net.Pipes
{
	public class Pipe : IDisposable
	{
		// Parent Pipe object
		private Pipe _parent;

		// Pipe instance assotiated with object
		private PipeInstance _instance;

		// Object state
		private bool _disposed;

		// Incoming requests queue
		private PipeRequestQueue _clientRequests;

		// Pipe instance pool
		private PipeInstancePool _instancePool;
		private int _poolSize = 10;

		// Security descriptor describing access rights to the pipe
		private SecurityDescriptor _securityDescriptor;

		// Client principal
		private WindowsPrincipal _clientPrincipal;

		#region Constructors

		public Pipe()
		{
		}

		internal Pipe(PipeInstance instance, Pipe parent)
		{
			_instance = instance;
			_parent = parent;
		}

		#endregion

		#region Disposing

		~Pipe()
		{
			Dispose(false);
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			try
			{
				if (_parent == null)
				{
					if (_clientRequests != null)
					{
						// stop the request queue
						_clientRequests.StopListening();
					}
				}
				else
				{
					if (_instance == _parent.Instance)
					{
						// LAME: Two Pipe objects can share the same pipe instance - that is
						// the first pipe instance. In this case we could not close pipe as
						// it can be the last one...
						_instance.DisconnectFromClient();
						_instance = null;
					}
					else if (!AppDomain.CurrentDomain.IsFinalizingForUnload())
					{
						// return instance back to the pool
						_parent.InstancePool.StoreInstance(_instance);
						_instance = null;
					}
				}
				if (_instance != null)
				{
					// close the pipe instance
					_instance.Close();
					_instance = null;
				}
			}
			catch
			{
				// it seems reasonable to ignore this error
			}

			_disposed = true;
		}

		#endregion

		#region Properties

		internal PipeInstance Instance
		{
			get
			{
				return _instance;
			}
		}

		internal PipeInstancePool InstancePool
		{
			get
			{
				return _instancePool;
			}
		}

		internal SecurityDescriptor SecurityDescriptor
		{
			get
			{
				return _securityDescriptor;
			}
			set
			{
				_securityDescriptor = value;
			}
		}

		public WindowsPrincipal ClientPrincipal
		{
			get
			{
				if (_clientPrincipal == null)
				{
					_clientPrincipal = _instance.GetClientPrincipal();
				}

				return _clientPrincipal;
			}
		}

		public PipeName Name
		{
			get
			{
				return (_instance != null ? _instance.Name : null);
			}
		}

		public bool IsConnected
		{
			get
			{
				return (_instance != null ? _instance.IsConnected : false);
			}
		}

		public int PoolSize
		{
			get
			{
				return _poolSize;
			}
			set
			{
				// parameters validation
				if (value < 1)
					throw new InvalidOperationException("PoolSize must be greater than zero");

				_poolSize = value;
			}
		}

		#endregion

		/// <summary>
		/// Creates pipe on the server side to allow client to connect to
		/// </summary>
		public void Bind(PipeName pipeName, SecurityDescriptor securityDescriptor)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance != null)
				throw new InvalidOperationException("Pipe is already connected");

			// store security descriptor
			_securityDescriptor = securityDescriptor;

			// create pipe
			_instance = PipeInstance.Create(pipeName, true, _securityDescriptor);
			_instancePool = new PipeInstancePool(this);
		}

		/// <summary>
		/// Places a pipe in a listening state
		/// </summary>
		public void Listen(int backlog)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");
			if (_clientRequests != null)
				throw new InvalidOperationException("Pipe is already listening");

			// start listening on the pipe
			_clientRequests = new PipeRequestQueue(_instancePool, backlog);
			_clientRequests.StartListening();
		}

		/// <summary>
		/// Creates a new pipe instance to proceed incoming request
		/// </summary>
		public Pipe Accept()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");
			if (_clientRequests == null)
				throw new InvalidOperationException("Pipe is not listening");

			// return the incoming request
			return new Pipe(_clientRequests.GetRequest(), this);
		}

		/// <summary>
		/// Establishes a connection to a remote system
		/// </summary>
		public void Connect(PipeName pipeName)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance != null)
				throw new InvalidOperationException("Pipe is already connected");

			// connect to the remote host
			_instance = PipeInstance.Connect(pipeName);
		}

		/// <summary>
		/// Receives data from the pipe
		/// </summary>
		public int Receive(byte[] buffer, int offset, int size)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");

			// parameters validation
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("size");

			// read data from the pipe
			GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			int bytesRead;

			bool error = PipeNative.ReadFile(
				_instance.Handle,
				Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset),
				size,
				out bytesRead,
				IntPtr.Zero);
			gcBuffer.Free();

			if (!error)
			{
				// error occured
				throw new PipeIOException(Marshal.GetLastWin32Error(), "Could not read data from the pipe: " + _instance.Name);
			}

			return bytesRead;
		}

		/// <summary>
		/// Initiates receive operation
		/// </summary>
		public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");

			// parameters validation
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("size");

			// read data from the pipe
			GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			PipeOverlappedAsyncResult asyncResult = new PipeOverlappedAsyncResult(gcBuffer, callback, state);

			int error = asyncResult.CheckForCompletion(PipeNative.ReadFile(
				_instance.Handle,
				Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset),
				size,
				IntPtr.Zero,
				asyncResult.OverlappedHandle));

			switch (error)
			{
				case Win32.ERROR_SUCCESS:
					// operation completed synchronously
					break;
				case PipeNative.ERROR_IO_PENDING:
					// async operation was pended
					break;
				default:
					// error occured
					asyncResult.Dispose();
					throw new PipeIOException(error, "Could not read data from the pipe: " + _instance.Name);
			}

			return asyncResult;
		}

		/// <summary>
		/// Completes receive operation
		/// </summary>
		public int EndReceive(IAsyncResult result)
		{
			// parameters validation
			if (result == null)
				throw new ArgumentNullException("result");

			// get async result
			PipeOverlappedAsyncResult asyncResult = (result as PipeOverlappedAsyncResult);
			if (asyncResult == null)
				throw new ArgumentException("result");

			// wait for completion
			int error = asyncResult.WaitForCompletion();
			if (error != Win32.ERROR_SUCCESS)
			{
				// error occured
				throw new PipeIOException(error, "Could not read data from the pipe: " + _instance.Name);
			}

			return asyncResult.TotalBytes;
		}

		/// <summary>
		/// Sends data to the pipe
		/// </summary>
		public void Send(byte[] buffer, int offset, int size)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");

			// parameters validation
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("size");

			// read data from the pipe
			GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			int bytesWritten;

			bool error = PipeNative.WriteFile(
				_instance.Handle,
				Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset),
				size,
				out bytesWritten,
				IntPtr.Zero);
			gcBuffer.Free();

			if (!error)
			{
				// error occured
				throw new PipeIOException(Marshal.GetLastWin32Error(), "Could not write data to the pipe: " + _instance.Name);
			}
		}

		/// <summary>
		/// Initiates send operation
		/// </summary>
		public IAsyncResult BeginSend(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");

			// parameters validation
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("size");

			// write data to the pipe
			GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			PipeOverlappedAsyncResult asyncResult = new PipeOverlappedAsyncResult(gcBuffer, callback, state);

			int error = asyncResult.CheckForCompletion(PipeNative.WriteFile(
				_instance.Handle,
				Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset),
				size,
				IntPtr.Zero,
				asyncResult.OverlappedHandle));

			switch (error)
			{
				case Win32.ERROR_SUCCESS:
					// operation completed synchronously
					break;
				case PipeNative.ERROR_IO_PENDING:
					// async operation was pended
					break;
				default:
					// error occured
					asyncResult.Dispose();
					throw new PipeIOException(error, "Could not write data to the pipe: " + _instance.Name);
			}

			return asyncResult;
		}

		/// <summary>
		/// Completes send operation
		/// </summary>
		public void EndSend(IAsyncResult result)
		{
			// parameters validation
			if (result == null)
				throw new ArgumentNullException("result");

			// get async result
			PipeOverlappedAsyncResult asyncResult = (result as PipeOverlappedAsyncResult);
			if (asyncResult == null)
				throw new ArgumentException("result");

			// wait for completion
			int error = asyncResult.WaitForCompletion();
			if (error != Win32.ERROR_SUCCESS)
			{
				// error occured
				throw new PipeIOException(error, "Could not write data to the pipe: " + _instance.Name);
			}
		}

		/// <summary>
		/// Copies data into a buffer without removing it from the pipe
		/// </summary>
		public int Peek(byte[] buffer, int offset, int size)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");

			// parameters validation
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("size");

			// read data from the pipe
			GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			int bytesRead;
			int totalBytes;
			int bytesLeftThisMessage;

			bool error = PipeNative.PeekNamedPipe(
				_instance.Handle,
				Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset),
				size,
				out bytesRead,
				out totalBytes,
				out bytesLeftThisMessage);
			gcBuffer.Free();

			if (!error)
			{
				// error occured
				throw new PipeIOException(Marshal.GetLastWin32Error(), "Could not read data from the pipe: " + _instance.Name);
			}

			return bytesRead;
		}

		/// <summary>
		/// Returns information about data in the pipe
		/// </summary>
		public int GetBytesAvailable()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");

			// check pipe buffer
			int bytesRead;
			int totalBytes;
			int bytesLeftThisMessage;

			bool error = PipeNative.PeekNamedPipe(
				_instance.Handle,
				IntPtr.Zero,
				0,
				out bytesRead,
				out totalBytes,
				out bytesLeftThisMessage);

			if (!error)
			{
				// error occured
				throw new PipeIOException(Marshal.GetLastWin32Error(), "Could not read data from the pipe: " + _instance.Name);
			}

			return totalBytes;
		}

		/// <summary>
		/// Flushes internal pipe buffers
		/// </summary>
		public void Flush()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_instance == null)
				throw new InvalidOperationException("Pipe is not connected");

			// flush buffers
			if (!PipeNative.FlushFileBuffers(_instance.Handle))
				throw new PipeIOException();
		}

		/// <summary>
		/// Close the pipe
		/// </summary>
		public void Close()
		{
			((IDisposable)this).Dispose();
		}
	}
}