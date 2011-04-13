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

using DocsVision.IO;

namespace DocsVision.Runtime.Remoting.Formatters
{
	internal sealed class ServerFormatterSink : IChannelSinkBase, IServerChannelSink
	{
		// Formatter instance
		private IWireFormatter _formatter;

		// Next sink in the chain
		private IServerChannelSink _nextSink;

		#region Constructors

		public ServerFormatterSink(IWireFormatter formatter, IServerChannelSink nextSink)
		{
			// parameters validation
			if (formatter == null)
				throw new ArgumentNullException("formatter");
			if (nextSink == null)
				throw new ArgumentNullException("nextSink");

			_formatter = formatter;
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
			// never called because the server formatter sink is always last in the chain
			throw new NotSupportedException();
		}

		public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			if (requestMsg != null)
			{
				// the message has already been deserialized so delegate to the next sink
				return _nextSink.ProcessMessage(
					sinkStack,
					requestMsg, requestHeaders, requestStream,
					out responseMsg, out responseHeaders, out responseStream);
			}

			// parameters validation
			if (sinkStack == null)
				throw new ArgumentNullException("sinkStack");
			if (requestHeaders == null)
				throw new ArgumentNullException("requestHeaders");
			if (requestStream == null)
				throw new ArgumentNullException("requestStream");

			// deserialize request
			IMethodCallMessage methodCall = _formatter.DeserializeRequest(requestHeaders, requestStream);
			requestStream.Close();

			// prepare stack for request processing
			sinkStack.Push(this, null);

			// process request
			ServerProcessing processing;
			IMethodReturnMessage methodReturn = null;

			try
			{
				// call next sink to dispatch method call
				processing = _nextSink.ProcessMessage(
					sinkStack,
					methodCall, requestHeaders, null,
					out responseMsg, out responseHeaders, out responseStream);

				if (processing == ServerProcessing.Complete)
				{
					// response headers and stream must be null at this point!
					if (responseHeaders != null)
						throw new NotSupportedException();
					if (responseStream != null)
						throw new NotSupportedException();

					// check response
					methodReturn = responseMsg as IMethodReturnMessage;
					if (methodReturn == null)
						throw new NotSupportedException();
				}
			}
			catch (Exception ex)
			{
				processing = ServerProcessing.Complete;
				methodReturn = new ReturnMessage(ex, methodCall);

				responseMsg = methodReturn;
				responseHeaders = null;
				responseStream = null;
			}

			// handle response
			switch (processing)
			{
				case ServerProcessing.Complete:
				{
					// call proceeded synchronously - serialize response
					sinkStack.Pop(this);
					SerializeResponse(sinkStack, methodReturn, out responseHeaders, out responseStream);
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
					sinkStack.Store(this, null);
					break;
				}
				default:
					throw new NotSupportedException();
			}

			return processing;
		}

		public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage responseMsg, ITransportHeaders responseHeaders, Stream responseStream)
		{
			// parameters validation
			if (sinkStack == null)
				throw new ArgumentNullException("sinkStack");
			if (responseHeaders == null)
				throw new ArgumentNullException("responseHeaders");
			if (responseStream == null)
				throw new ArgumentNullException("responseStream");

			IMethodReturnMessage methodReturn = responseMsg as IMethodReturnMessage;
			if (methodReturn == null)
				throw new NotSupportedException();

			// serialize response
			_formatter.SerializeResponse(methodReturn, responseHeaders, responseStream);

			// process response asynchronously
			sinkStack.AsyncProcessResponse(responseMsg, responseHeaders, responseStream);
		}

		#endregion

		private void SerializeResponse(IServerChannelSinkStack sinkStack, IMethodReturnMessage methodReturn, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			// get response stream
			responseHeaders = new TransportHeaders();
			responseStream = sinkStack.GetResponseStream(methodReturn, responseHeaders);
			if (responseStream == null)
			{
				responseStream = new ChunkedMemoryStream();
			}

			// serialize response
			_formatter.SerializeResponse(methodReturn, responseHeaders, responseStream);
		}
	}
}