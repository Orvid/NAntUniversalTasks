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
	internal sealed class ServerTransportSink : IChannelSinkBase, IServerChannelSink
	{
		// Next sink in the chain
		private IServerChannelSink _nextSink;

		#region Constructors

		public ServerTransportSink(IServerChannelSink nextSink)
		{
			// parameters validation
			if (nextSink == null)
				throw new ArgumentNullException("nextSink");

			_nextSink = nextSink;
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

		#region IServerChannelSink Members

		public IServerChannelSink NextChannelSink
		{
			get
			{
				return _nextSink;
			}
		}

		public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage responseMsg, ITransportHeaders responseHeaders)
		{
			// parameters validation
			if (responseHeaders == null)
				throw new ArgumentNullException("responseHeaders");

			IMethodReturnMessage methodReturn = responseMsg as IMethodReturnMessage;
			if (methodReturn == null)
				throw new NotSupportedException();

			IConnection connection = state as IConnection;
			if (connection == null)
				throw new NotSupportedException();

			// return stream for writing response
			return connection.GetResponseStream(methodReturn, responseHeaders);
		}

		public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			// never called because the server transport sink is always first in the chain
			throw new NotSupportedException();
		}

		public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage responseMsg, ITransportHeaders responseHeaders, Stream responseStream)
		{
			// UNDONE:
			throw new NotImplementedException();
		}

		#endregion

		public void ProcessRequest(IConnection connection)
		{
			// parameters validation
			if (connection == null)
				throw new ArgumentNullException("connection");

			// receive request
			ITransportHeaders requestHeaders;
			Stream requestStream;
			connection.ReceiveRequest(out requestHeaders, out requestStream);

			// create sink stack for request processing
			ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
			sinkStack.Push(this, connection);	// save connection as state

			// process request
			ServerProcessing processing;
			IMethodReturnMessage methodReturn = null;
			ITransportHeaders responseHeaders = null;
			Stream responseStream = null;

			try
			{
				// call next sink to dispatch method call
				IMessage responseMsg;
				processing = _nextSink.ProcessMessage(sinkStack,
					null, requestHeaders, requestStream,
					out responseMsg, out responseHeaders, out responseStream);

				if (processing == ServerProcessing.Complete)
				{
					// response headers and stream can not be null at this point!
					if (responseHeaders == null)
						throw new ArgumentNullException("responseHeaders");
					if (responseStream == null)
						throw new ArgumentNullException("responseStream");

					// check response
					methodReturn = responseMsg as IMethodReturnMessage;
					if (methodReturn == null)
						throw new NotSupportedException();
				}
			}
			catch (Exception ex)
			{
				processing = ServerProcessing.Complete;
				methodReturn = new ReturnMessage(ex, null);
			}

			// handle response
			switch (processing)
			{
				case ServerProcessing.Complete:
				{
					// call completed synchronously - send response
					sinkStack.Pop(this);
					connection.SendResponse(methodReturn, responseHeaders, responseStream);
					break;
				}
				case ServerProcessing.OneWay:
				{
					// no response needed
					sinkStack.Pop(this);
					break;
				}
				case ServerProcessing.Async:
				{
					// call proceeded asynchronously
					sinkStack.StoreAndDispatch(this, connection);
					break;
				}
				default:
					throw new NotSupportedException();
			}
		}
	}
}