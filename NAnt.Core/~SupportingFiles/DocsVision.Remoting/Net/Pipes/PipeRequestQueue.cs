//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Threading;

namespace DocsVision.Net.Pipes
{
	internal sealed class PipeRequestQueue
	{
		// Pipe instance pool
		private PipeInstancePool _instancePool;

		// Request queue
		private Queue _requests = new Queue();
		private int _backlog;

		// Background listener thread
		private static Thread _listenerThread;
		private bool _isListening;

		#region Constructors

		public PipeRequestQueue(PipeInstancePool instancePool, int backlog)
		{
			// parameters validation
			if (instancePool == null)
				throw new ArgumentNullException("instancePool");
			if (backlog < 0)
				throw new ArgumentOutOfRangeException("backlog");

			_instancePool = instancePool;
			_backlog = backlog;
		}

		#endregion

		#region Properties

		public bool IsListening
		{
			get
			{
				return _isListening;
			}
		}

		#endregion

		public void StartListening()
		{
			if (_listenerThread == null)
			{
				// start listener thread
				_listenerThread = new Thread(new ThreadStart(ListenerStart));
				_listenerThread.IsBackground = true;
				_listenerThread.Start();
			}

			_isListening = true;
		}

		public void StopListening()
		{
			if (_listenerThread != null)
			{
				// terminate listener thread
				_listenerThread.Abort();
				_listenerThread = null;
			}

			_isListening = false;
		}

		public PipeInstance GetRequest()
		{
			lock (_requests)
			{
				while (true)
				{
					// notify the ListenerStart thread that a request is needed
					Monitor.Pulse(_requests);

					if (_requests.Count > 0)
					{
						// return request from the queue
						return (PipeInstance)_requests.Dequeue();
					}

					// wait for incoming request
					Monitor.Wait(_requests);
				}
			}
		}

		private void ListenerStart()
		{
			while (_isListening)
			{
				PipeInstance instance = null;
				try
				{
					// get an instance from the pool
					instance = _instancePool.GetInstance();

					// wait for incoming client request
					instance.WaitForClientConnection();
				}
				catch
				{
					if (instance != null)
					{
						// close the pipe as it likely is not valid
						instance.Close();
						instance = null;
					}
				}

				if (instance != null)
				{
					lock (_requests)
					{
						while (_requests.Count >= _backlog)
						{
							// we are out of space in the queue, so wait for it
							Monitor.Wait(_requests);
						}

						// add request to the queue
						_requests.Enqueue(instance);

						// notify the GetRequest thread that there are available requests
						Monitor.Pulse(_requests);
					}
				}
			}
		}
	}
}