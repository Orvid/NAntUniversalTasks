//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Threading;
using System.Runtime.InteropServices;

using DocsVision.Util;

namespace DocsVision.Net.Pipes
{
	internal sealed class PipeOverlappedAsyncResult : IDisposable, IAsyncResult
	{
		// Operation state
		private object _asyncState;
		private ManualResetEvent _asyncEvent;
		private AsyncCallback _asyncCallback;
		private int _completed;
		private bool _completedSynchronously;
		private int _totalBytes;
		private int _result;
		private GCHandle _gcBuffer;

		// Unmanaged structures
		private LocalAllocHandle _pOverlapped;

		#region Constructor

		public PipeOverlappedAsyncResult()
		{
			InitializeOverlapped();
		}

		public PipeOverlappedAsyncResult(GCHandle gcBuffer, AsyncCallback asyncCallback, object asyncState)
		{
			_gcBuffer = gcBuffer;
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;

			InitializeOverlapped();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (_pOverlapped.Allocated)
			{
				_pOverlapped.Dispose();
			}
			if (_gcBuffer.IsAllocated)
			{
				_gcBuffer.Free();
			}
		}

		#endregion

		#region IAsyncResult Members

		public object AsyncState
		{
			get
			{
				return _asyncState;
			}
		}

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				return _asyncEvent;
			}
		}

		public bool IsCompleted
		{
			get
			{
				return (_completed == 1);
			}
		}

		public bool CompletedSynchronously
		{
			get
			{
				return _completedSynchronously;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// User callback
		/// </summary>
		public AsyncCallback AsyncCallback
		{
			get
			{
				return _asyncCallback;
			}
		}

		/// <summary>
		/// Pointer to Overlapped structure
		/// </summary>
		public IntPtr OverlappedHandle
		{
			get
			{
				return _pOverlapped;
			}
		}

		/// <summary>
		/// Total bytes transferred
		/// </summary>
		public int TotalBytes
		{
			get
			{
				return _totalBytes;
			}
		}

		#endregion

		/// <summary>
		/// Initializes unmanaged Overlapped structure
		/// </summary>
		private void InitializeOverlapped()
		{
			_pOverlapped = new LocalAllocHandle(PipeNative.Overlapped.Size);
			_asyncEvent = new ManualResetEvent(false);
			Marshal.WriteIntPtr(_pOverlapped, PipeNative.Overlapped.hEventOffset, _asyncEvent.Handle);
		}

		/// <summary>
		/// Check async operation status
		/// </summary>
		public int CheckForCompletion(bool result)
		{
			int error = Win32.ERROR_SUCCESS;
			if (result)
			{
				// operation completed synchronously
				SetCompleted(true);

				if (_asyncCallback != null)
				{
					// queue user callback function
					ThreadPool.QueueUserWorkItem(
						new WaitCallback(OnSyncCompletion),
						this);
				}
			}
			else
			{
				error = Marshal.GetLastWin32Error();
				if (error == PipeNative.ERROR_IO_PENDING)
				{
					// async operation was pended
					ThreadPool.RegisterWaitForSingleObject(
						_asyncEvent,
						new WaitOrTimerCallback(OnAsyncCompletion),
						this,
						-1,
						true);
				}
			}

			return error;
		}

		/// <summary>
		/// Waits for async operation completion
		/// </summary>
		public int WaitForCompletion()
		{
			if (_completed != 1)
			{
				_asyncEvent.WaitOne();
				SetCompleted(false);
			}

			return _result;
		}

		private static void OnSyncCompletion(object state)
		{
			PipeOverlappedAsyncResult asyncResult = (PipeOverlappedAsyncResult)state;

			// invoke client callback
			asyncResult.AsyncCallback(asyncResult);
		}

		private static void OnAsyncCompletion(object state, bool signaled)
		{
			PipeOverlappedAsyncResult asyncResult = (PipeOverlappedAsyncResult)state;
			asyncResult.SetCompleted(false);

			if (asyncResult.AsyncCallback != null)
			{
				// invoke client callback
				asyncResult.AsyncCallback(asyncResult);
			}
		}

		private void SetCompleted(bool completedSynchronously)
		{
			if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0)
			{
				// store result
				_result = PipeNative.HackedGetOverlappedResult(_pOverlapped, out _totalBytes);
				_completedSynchronously = completedSynchronously;

				if (completedSynchronously)
				{
					// signal completion
					_asyncEvent.Set();
				}

				// free unmanaged resources
				Dispose();
			}
		}
	}
}