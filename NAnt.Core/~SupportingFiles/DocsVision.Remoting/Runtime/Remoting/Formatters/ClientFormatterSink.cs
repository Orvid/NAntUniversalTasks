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
	internal sealed class ClientFormatterSink : IChannelSinkBase, IClientFormatterSink, IMessageSink
	{
		// Formatter instance
		private IWireFormatter _formatter;

		// Next sink in the chain
		private IClientChannelSink _nextSink;

		#region Constructors

		public ClientFormatterSink(IWireFormatter formatter, IClientChannelSink nextSink)
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

		#region IClientChannelSink Members

		public IClientChannelSink NextChannelSink
		{
			get
			{
				return _nextSink;
			}
		}

		public Stream GetRequestStream(IMessage requestMsg, ITransportHeaders requestHeaders)
		{
			// never called because the client formatter sink is always first in the chain
			throw new NotSupportedException();
		}

		public void ProcessMessage(IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			// never called because the client formatter sink is always first in the chain
			throw new NotSupportedException();
		}

		public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream)
		{
			// never called because the client formatter sink is always first in the chain
			throw new NotSupportedException();
		}

		public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders responseHeaders, Stream responseStream)
		{
			// parameters validation
			if (sinkStack == null)
				throw new ArgumentNullException("sinkStack");
			if (responseHeaders == null)
				throw new ArgumentNullException("responseHeaders");
			if (responseStream == null)
				throw new ArgumentNullException("responseStream");

			IMethodCallMessage methodCall = state as IMethodCallMessage;
			if (methodCall == null)
				throw new NotSupportedException();

			IMethodReturnMessage methodReturn;
			try
			{
				// deserialize response
				methodReturn = _formatter.DeserializeResponse(methodCall, responseHeaders, responseStream);
				responseStream.Close();
			}
			catch (Exception ex)
			{
				methodReturn = new ReturnMessage(ex, methodCall);
			}

			// dispatch response
			sinkStack.DispatchReplyMessage(methodReturn);
		}

		#endregion

		#region IMessageSink Members

		public IMessageSink NextSink
		{
			get
			{
				// there are only one message sink in the chain
				return null;
			}
		}

		public IMessage SyncProcessMessage(IMessage requestMsg)
		{
			// parameters validation
			IMethodCallMessage methodCall = (requestMsg as IMethodCallMessage);
			if (methodCall == null)
				throw new NotSupportedException();

			Stream requestStream = null;
			Stream responseStream = null;
			IMethodReturnMessage methodReturn;

			try
			{
				// serialize request
				ITransportHeaders requestHeaders;
				SerializeRequest(methodCall, out requestHeaders, out requestStream);

				// call next sink to dispatch method call
				ITransportHeaders responseHeaders;
				_nextSink.ProcessMessage(requestMsg, requestHeaders, requestStream,
					out responseHeaders, out responseStream);

				// deserialize response
				methodReturn = _formatter.DeserializeResponse(methodCall, responseHeaders, responseStream);
				responseStream.Close();
			}
			catch (Exception ex)
			{
				methodReturn = new ReturnMessage(ex, methodCall);
			}

			return methodReturn;
		}

		public IMessageCtrl AsyncProcessMessage(IMessage requestMsg, IMessageSink replySink)
		{
			// parameters validation
			IMethodCallMessage methodCall = (requestMsg as IMethodCallMessage);
			if (methodCall == null)
				throw new NotSupportedException();

			try
			{
				// serialize request
				ITransportHeaders requestHeaders;
				Stream requestStream;
				SerializeRequest(methodCall, out requestHeaders, out requestStream);

				// create sink stack for async request processing
				ClientChannelSinkStack sinkStack = new ClientChannelSinkStack(replySink);
				sinkStack.Push(this, requestMsg);	// save request message as state

				// call next sink to dispatch method call
				_nextSink.AsyncProcessRequest(sinkStack, requestMsg, requestHeaders, requestStream);
			}
			catch (Exception ex)
			{
				if (replySink != null)
				{
					// process exception synchronously
					replySink.SyncProcessMessage(new ReturnMessage(ex, methodCall));
				}
			}

			return null;
		}

		#endregion

		private void SerializeRequest(IMethodCallMessage methodCall, out ITransportHeaders requestHeaders, out Stream requestStream)
		{
			// get request stream
			requestHeaders = new TransportHeaders();
			requestStream = _nextSink.GetRequestStream(methodCall, requestHeaders);
			if (requestStream == null)
			{
				requestStream = new ChunkedMemoryStream();
			}

			// serialize request
			_formatter.SerializeRequest(methodCall, requestHeaders, requestStream);
		}
	}
}