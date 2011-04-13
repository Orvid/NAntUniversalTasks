//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Threading;

namespace DocsVision.Runtime.Remoting.Transport
{
	internal sealed class TransportAsyncResult : IAsyncResult
	{
		// Worker that perform operation
		private TransportAsyncWorker _worker;

		// Operation state
		private object _state;
		private ManualResetEvent _completedSignal = new ManualResetEvent(false);
		private bool _isCompleted;

		#region Constructors

		public TransportAsyncResult(TransportAsyncWorker worker, object state)
		{
			_state = state;
			_worker = worker;
		}

		#endregion

		#region IAsyncResult Members

		public object AsyncState
		{
			get
			{
				return _state;
			}
		}

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				return _completedSignal;
			}
		}

		public bool CompletedSynchronously
		{
			get
			{
				return false;
			}
		}

		public bool IsCompleted
		{
			get
			{
				return _isCompleted;
			}
		}

		#endregion

		public TransportAsyncWorker Worker
		{
			get
			{
				return _worker;
			}
		}

		public void SetCompleted()
		{
			_completedSignal.Set();
			_isCompleted = true;
		}
	}
}