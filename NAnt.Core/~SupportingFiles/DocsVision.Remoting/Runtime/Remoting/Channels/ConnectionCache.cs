//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Threading;

namespace DocsVision.Runtime.Remoting.Channels
{
	// Delegate to method that will fabricate the appropriate transport connection
	internal delegate IConnection ConnectionFactory(string channelUri);

	internal sealed class ConnectionCache
	{
		// Connections stay open some amount of time before closing
		private TimeSpan _keepAlive;

		// Connections cached per host
		private Hashtable _hosts = new Hashtable();

		// This delegate will be used to create new transport connection
		private ConnectionFactory _connectionFactory;

		// We use thread pool to schedule method that checks for connection timeout
		private RegisteredWaitHandle _registeredTimeoutWaitHandle;
		private WaitOrTimerCallback _timeoutDelegate;
		private AutoResetEvent _timeoutWaitHandle;

		#region Constructors

		public ConnectionCache(int keepAlive, ConnectionFactory connectionFactory)
		{
			// parameters validation
			if (connectionFactory == null)
				throw new ArgumentNullException("connectionFactory");
			if (keepAlive < 1)
				throw new ArgumentOutOfRangeException("keepAlive");

			_connectionFactory = connectionFactory;
			_keepAlive = TimeSpan.FromSeconds(keepAlive);

			// Schedule timeout method
			_timeoutDelegate = new WaitOrTimerCallback(OnTimerElapsed);
			_timeoutWaitHandle = new AutoResetEvent(false);
			_registeredTimeoutWaitHandle = ThreadPool.RegisterWaitForSingleObject(
				_timeoutWaitHandle, _timeoutDelegate, null, _keepAlive, true);
		}

		#endregion

		/// <summary>
		/// Creates new connection
		/// </summary>
		public IConnection CreateConnection(string channelUri)
		{
			return _connectionFactory(channelUri);
		}

		/// <summary>
		/// Returns cached connection
		/// </summary>
		public IConnection GetConnection(string channelUri)
		{
			return GetHost(channelUri).GetConnection();
		}

		/// <summary>
		/// Returns connection back to the cache
		/// </summary>
		public void StoreConnection(string channelUri, IConnection connection)
		{
			GetHost(channelUri).StoreConnection(connection);
		}

		private void OnTimerElapsed(Object state, Boolean signalled)
		{
			lock (_hosts)
			{
				foreach (HostConnections host in _hosts.Values)
				{
					host.CheckForTimeout(_keepAlive);
				}
			}

			// schedule timeout method again
			_registeredTimeoutWaitHandle.Unregister(null);
			_registeredTimeoutWaitHandle = ThreadPool.RegisterWaitForSingleObject(
				_timeoutWaitHandle, _timeoutDelegate, null, _keepAlive, true);
		}

		private HostConnections GetHost(string channelUri)
		{
			lock (_hosts)
			{
				HostConnections host = (HostConnections)_hosts[channelUri];
				if (host == null)
				{
					host = new HostConnections(channelUri, this);
					_hosts[channelUri] = host;
				}
				return host;
			}
		}
	}
}