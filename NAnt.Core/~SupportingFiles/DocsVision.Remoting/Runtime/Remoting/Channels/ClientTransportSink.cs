//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace DocsVision.Runtime.Remoting.Channels
{
	internal sealed class ClientTransportSink : IChannelSinkBase, IClientChannelSink
	{
		// Channel information
		private string _channelUri;

		// Connection cache
		private ConnectionCache _connectionCache;
		private IConnection _connection;

		#region Constructors

		public ClientTransportSink(string channelUri, ConnectionCache connectionCache)
		{
			// parameters validation
			if (channelUri == null)
				throw new ArgumentNullException("channelUri");
			if (connectionCache == null)
				throw new ArgumentNullException("connectionCache");

			_channelUri = channelUri;
			_connectionCache = connectionCache;
		}

		#endregion

		#region IChannelSinkBase Members

		public IDictionary Properties
		{
			get
			{
				// we have no properties
				return null;
			}
		}

		#endregion

		#region IClientChannelSink Members

		public IClientChannelSink NextChannelSink
		{
			get
			{
				// we are always last in the chain
				return null;
			}
		}

		public Stream GetRequestStream(IMessage requestMsg, ITransportHeaders requestHeaders)
		{
			// parameters validation
			if (requestHeaders == null)
				throw new ArgumentNullException("requestHeaders");

			IMethodCallMessage methodCall = requestMsg as IMethodCallMessage;
			if (methodCall == null)
				throw new NotSupportedException();

			if (_connection != null)
			{
				// close connection as it is probably not valid
				_connection.Close();
			}

			// get connection from the cache
			_connection = _connectionCache.GetConnection(_channelUri);

			try
			{
				// return stream for writing request
				return _connection.GetRequestStream(methodCall, requestHeaders);
			}
			catch
			{
				// close connection as it is probably not valid
				_connection.Close();
				_connection = null;
				throw;
			}
		}

		public void ProcessMessage(IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			// parameters validation
			if (requestHeaders == null)
				throw new ArgumentNullException("requestHeaders");
			if (requestStream == null)
				throw new ArgumentNullException("requestStream");

			IMethodCallMessage methodCall = requestMsg as IMethodCallMessage;
			if (methodCall == null)
				throw new NotSupportedException();

			if (_connection == null)
			{
				// get connection from the cache
				_connection = _connectionCache.GetConnection(_channelUri);
			}

			try
			{
				// send request
				_connection.SendRequest(methodCall, requestHeaders, requestStream);

				if (!RemotingServices.IsOneWay(methodCall.MethodBase))
				{
					// receive response
					_connection.ReceiveResponse(out responseHeaders, out responseStream);
				}
				else
				{
					responseHeaders = null;
					responseStream = null;
				}

				// return connection to the cache
				_connectionCache.StoreConnection(_channelUri, _connection);
				_connection = null;
			}
			catch
			{
				// close connection as it is probably not valid
				_connection.Close();
				_connection = null;
				throw;
			}
		}

		public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream)
		{
			// UNDONE:
			throw new NotImplementedException();
		}

		public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders responseHeaders, Stream responseStream)
		{
			// never called because the client transport sink is always last in the chain
			throw new NotSupportedException();
		}

		#endregion
	}
}