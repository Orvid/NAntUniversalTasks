//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

using DocsVision.Security;
using DocsVision.Util;

namespace DocsVision.Net.Pipes
{
	internal sealed class PipeInstance
	{
		// Internal pipe buffers size
		private const int InBufferSize = 512;
		private const int OutBufferSize = 512;

		// Instance handle
		private NativeHandle _handle;

		// Instance properties
		private PipeName _pipeName;
		private bool _isConnected;

		#region Constructors

		public PipeInstance(PipeName pipeName, IntPtr handle, bool isConnected)
		{
			// parameters validation
			if (pipeName == null)
				throw new ArgumentNullException("pipeName");
			if (handle == Win32.InvalidHandle)
				throw new ArgumentException("Invalid pipe handle", "handle");

			_pipeName = pipeName;
			_handle = new NativeHandle(handle);
			_isConnected = isConnected;
		}

		#endregion

		#region Properties

		public PipeName Name
		{
			get
			{
				return _pipeName;
			}
		}

		public IntPtr Handle
		{
			get
			{
				return _handle;
			}
		}

		public bool IsConnected
		{
			get
			{
				return _isConnected;
			}
		}

		#endregion

		public static PipeInstance Connect(PipeName pipeName)
		{
			// parameters validation
			if (pipeName == null)
				throw new ArgumentNullException("pipeName", "Pipe name must be specified");

			while (true)
			{
				// try to connect to the pipe
				IntPtr handle = PipeNative.CreateFile(pipeName.ToString(),
					PipeNative.GENERIC_READ | PipeNative.GENERIC_WRITE,
					PipeNative.FILE_SHARE_NONE,
					null,
					PipeNative.OPEN_EXISTING,
					PipeNative.FILE_FLAG_OVERLAPPED,
					IntPtr.Zero);

				if (handle == Win32.InvalidHandle)
				{
					int errorCode = Marshal.GetLastWin32Error();
					if (errorCode != PipeNative.ERROR_PIPE_BUSY)
					{
						throw new PipeIOException(errorCode, "Could not open pipe: " + pipeName);
					}
					if (!PipeNative.WaitNamedPipe(pipeName.ToString(), PipeNative.NMPWAIT_USE_DEFAULT_WAIT))
					{
						throw new PipeIOException(errorCode, "Specified pipe was over-burdened: " + pipeName);
					}
				}
				else
				{
					return new PipeInstance(pipeName, handle, true);
				}
			}
		}

		public static PipeInstance Create(PipeName pipeName, bool firstInstance, SecurityDescriptor securityDescriptor)
		{
			// parameters validation
			if (pipeName == null)
				throw new ArgumentNullException("pipeName", "Pipe name must be specified");
			if (!pipeName.IsLocal)
				throw new ArgumentException("Could not bind to the remote pipe");

			PipeNative.SecurityAttributes secAttrs = new PipeNative.SecurityAttributes();
			secAttrs.SecurityDescriptor = (securityDescriptor == null ? IntPtr.Zero : securityDescriptor.Handle);
			secAttrs.InheritHandle = true;

			// try to create pipe
			IntPtr handle = PipeNative.CreateNamedPipe(pipeName.ToString(),
				PipeNative.PIPE_ACCESS_DUPLEX | PipeNative.FILE_FLAG_OVERLAPPED | (firstInstance ? PipeNative.FILE_FLAG_FIRST_PIPE_INSTANCE : 0),
				PipeNative.PIPE_TYPE_BYTE | PipeNative.PIPE_READMODE_BYTE | PipeNative.PIPE_WAIT,
				PipeNative.PIPE_UNLIMITED_INSTANCES,
				OutBufferSize,
				InBufferSize,
				PipeNative.NMPWAIT_USE_DEFAULT_WAIT,
				secAttrs);

			if (handle == Win32.InvalidHandle)
			{
				throw new PipeIOException(Marshal.GetLastWin32Error(), "Could not create pipe: " + pipeName);
			}
			else
			{
				return new PipeInstance(pipeName, handle, false);
			}
		}

		public void WaitForClientConnection()
		{
			// check object state
			if (_isConnected)
				throw new InvalidOperationException("Pipe is already connected");

			// connect to the client
			PipeOverlappedAsyncResult asyncResult = new PipeOverlappedAsyncResult();
			int error = asyncResult.CheckForCompletion(PipeNative.ConnectNamedPipe(
				_handle,
				asyncResult.OverlappedHandle));

			switch (error)
			{
				case Win32.ERROR_SUCCESS:
					// operation completed synchronously
					break;
				case PipeNative.ERROR_PIPE_CONNECTED:
					// client already connected
					break;
				case PipeNative.ERROR_IO_PENDING:
					// async operation was pended
					asyncResult.WaitForCompletion();
					break;
				default:
					// error occured
					throw new PipeIOException(error, "Unable to connect client to the pipe: " + _pipeName);
			}

			_isConnected = true;
		}

		public WindowsPrincipal GetClientPrincipal()
		{
			// check object state
			if (!_isConnected)
				throw new InvalidOperationException("Pipe is not connected");

			// impersonate client
			if (!PipeNative.ImpersonateNamedPipeClient(_handle))
				throw new PipeIOException(Marshal.GetLastWin32Error(), "Could not impersonate client");

			try
			{
				// get client identity
				return new WindowsPrincipal(WindowsIdentity.GetCurrent());
			}
			finally
			{
				// undo impersonation
				PipeNative.RevertToSelf();
			}
		}

		public void DisconnectFromClient()
		{
			// check object state
			if (!_isConnected)
				throw new InvalidOperationException("Pipe is not connected");

			// flush internal buffers
			if (!PipeNative.FlushFileBuffers(_handle))
				throw new PipeIOException();

			// disconnect from the client
			if (!PipeNative.DisconnectNamedPipe(_handle))
				throw new PipeIOException();

			_isConnected = false;
		}

		public void Close()
		{
			if (_isConnected)
			{
				// flush the pipe to allow the client to read the pipe's contents
				// before disconnecting
				PipeNative.FlushFileBuffers(_handle);
				PipeNative.DisconnectNamedPipe(_handle);
			}

			// close handle
			_handle.Dispose();
		}
	}
}