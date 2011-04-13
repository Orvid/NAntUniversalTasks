//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting;

using DocsVision.IO;

namespace DocsVision.Runtime.Remoting.Channels
{
	// Delegate to method that will process incoming request
	internal delegate void ProcessRequestCallback(IConnection connection);

	internal sealed class RequestQueue
	{
		// Default request queue
		public static readonly RequestQueue DefaultQueue = new RequestQueue(250, 5, 2, 10);

		// Incoming requests
		private Queue _requests = new Queue();
		private int _requestQueueSize = 0;
		private int _workItemsCount = 0;

		// Queue constraints
		private TimeSpan _queueTimeout;
		private int _maxRequestQueueSize;
		private int _maxWorkItemsCount;
		private int _minFreeThreads;

		// Callback for new requests
		private DataAvailableCallback _requestAvailableCallback;

		// Callback for new work items
		private WaitCallback _workItemCallback;

		private sealed class QueuedRequest
		{
			private IConnection _connection;
			private ProcessRequestCallback _requestHandler;

			#region Constructors

			public QueuedRequest(IConnection connection, ProcessRequestCallback requestHandler)
			{
				_connection = connection;
				_requestHandler = requestHandler;
			}

			#endregion

			#region Properties

			public IConnection Connection
			{
				get
				{
					return _connection;
				}
			}

			public ProcessRequestCallback RequestHandler
			{
				get
				{
					return _requestHandler;
				}
			}

			#endregion
		}

		#region Constructors

		public RequestQueue(int maxQueueSize, int queueTimeout, int maxActiveWorkItems, int minFreeThreads)
		{
			// parameters validation
			if (maxQueueSize < 1)
				throw new ArgumentOutOfRangeException("maxQueueSize");
			if (queueTimeout < 1)
				throw new ArgumentOutOfRangeException("minFreeThreads");
			if (maxActiveWorkItems < 1)
				throw new ArgumentOutOfRangeException("maxActiveWorkItems");

			_maxRequestQueueSize = maxQueueSize;
			_queueTimeout = TimeSpan.FromSeconds(queueTimeout);
			_maxWorkItemsCount = maxActiveWorkItems;
			_minFreeThreads = minFreeThreads;

			_requestAvailableCallback = new DataAvailableCallback(OnDataAvailable);
			_workItemCallback = new WaitCallback(OnNewWorkItem);
		}

		#endregion

		public void AddConnection(IConnection connection, ProcessRequestCallback requestHandler)
		{
			// parameters validation
			if (connection == null)
				throw new ArgumentNullException("connection");
			if (requestHandler == null)
				throw new ArgumentNullException("requestHandler");

			// wait for requests on this connection
			BeginWaitForRequest(new QueuedRequest(connection, requestHandler));
		}

		private void BeginWaitForRequest(QueuedRequest request)
		{
			int workerThreads, ioThreads;
			ThreadPool.GetAvailableThreads(out workerThreads, out ioThreads);
			int freeThreads = (ioThreads > workerThreads) ? workerThreads : ioThreads;

			if (freeThreads > _minFreeThreads)
			{
				// schedule new thread to wait for the request
				request.Connection.BeginReceive(_requestAvailableCallback, request);
			}
			else
			{
				// use request queue...
				EnqueueRequest(request);
			}
		}

		private void EnqueueRequest(QueuedRequest request)
		{
			if (_workItemsCount < _maxWorkItemsCount)
			{
				// schedule new work item to handle request
				Interlocked.Increment(ref _workItemsCount);
				ThreadPool.QueueUserWorkItem(_workItemCallback, request);
			}
			else
			{
				lock (this)
				{
					if (_requestQueueSize > _maxRequestQueueSize)
					{
						// wait for the place in the queue
						if (!Monitor.Wait(this, _queueTimeout))
						{
							// so, we can't handle requests now... :(
							ProcessErrorAndClose(request.Connection, new RemotingException("Server was over-burdened"));
							return;
						}
					}

					// request will wait for the next available work item
					_requests.Enqueue(request);
					++_requestQueueSize;
				}
			}
		}

		private QueuedRequest DequeueRequest()
		{
			QueuedRequest request = null;

			lock (this)
			{
				if (_requestQueueSize > 0)
				{
					request = (QueuedRequest)_requests.Dequeue();
					--_requestQueueSize;
				}
			}

			return request;
		}

		private void ProcessRequest(QueuedRequest request)
		{
			try
			{
				// process request
				request.RequestHandler(request.Connection);

				// wait for the next request on this connection
				BeginWaitForRequest(request);
			}
			catch (Exception ex)
			{
				// process error message
				ProcessErrorAndClose(request.Connection, ex);
			}
		}

		private void ProcessErrorAndClose(IConnection connection, Exception ex)
		{
			try
			{
				// send error details
				connection.SendErrorResponse(ex);
			}
			catch
			{
				// mmm, what can we do now?
			}

			try
			{
				// close connection
				connection.Close();
			}
			catch
			{
				// it seems reasonable to ignore this error
			}
		}

		private void OnDataAvailable(Exception ex, object state)
		{
			QueuedRequest request = (QueuedRequest)state;

			if (ex == null)
			{
				// process request on this thread
				ProcessRequest(request);
			}
			else
			{
				// process error message
				ProcessErrorAndClose(request.Connection, ex);
			}
		}

		private void OnNewWorkItem(object state)
		{
			QueuedRequest request = (QueuedRequest)state;

			while (request != null)
			{
				// process request on this thread
				ProcessRequest(request);

				// get next request
				request = DequeueRequest();
			}

			Interlocked.Decrement(ref _workItemsCount);
		}
	}
}