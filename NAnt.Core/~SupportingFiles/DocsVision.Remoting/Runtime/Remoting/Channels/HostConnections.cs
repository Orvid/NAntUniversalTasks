//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Runtime.Remoting.Channels
{
	internal sealed class HostConnections
	{
		// Connections stored in a simple linked list
		private sealed class CachedConnection
		{
			private IConnection _connection;
			private DateTime _lastUsed;
			private CachedConnection _next;

			#region Constructors

			public CachedConnection(IConnection connection, CachedConnection next)
			{
				_connection = connection;
				_lastUsed = DateTime.Now;
				_next = next;
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

			public TimeSpan InactiveTime
			{
				get
				{
					return DateTime.Now - _lastUsed;
				}
			}

			public CachedConnection Next
			{
				get
				{
					return _next;
				}
				set
				{
					_next = value;
				}
			}

			#endregion
		}

		private ConnectionCache _cache;
		private string _channelUri;
		private CachedConnection _connections;	// list

		#region Constructors

		public HostConnections(string channelUri, ConnectionCache cache)
		{
			_cache = cache;
			_channelUri = channelUri;
		}

		#endregion

		/// <summary>
		/// Returns cached connection
		/// </summary>
		public IConnection GetConnection()
		{
			lock (this)
			{
				if (_connections == null)
				{
					// cache is empty so create new connection
					return _cache.CreateConnection(_channelUri);
				}
				else
				{
					IConnection connection = _connections.Connection;
					_connections = _connections.Next;
					return connection;
				}
			}
		}

		/// <summary>
		/// Returns connection back to the cache
		/// </summary>
		public void StoreConnection(IConnection connection)
		{
			lock (this)
			{
				_connections = new CachedConnection(connection, _connections);
			}
		}

		/// <summary>
		/// Checks connection for timeout
		/// </summary>
		public void CheckForTimeout(TimeSpan keepAlive)
		{
			lock (this)
			{
				CachedConnection prev = null;
				CachedConnection current = _connections;

				while (current != null)
				{
					// see if it's lifetime has expired
					if (current.InactiveTime > keepAlive)
					{
						// close the connection
						current.Connection.Close();

						// remove connection from the list
						current = current.Next;
						if (prev == null)
						{
							_connections = current;
						}
						else
						{
							prev.Next = current;
						}
					}
					else
					{
						// move to the next
						prev = current;
						current = current.Next;
					}
				}
			}
		}
	}
}