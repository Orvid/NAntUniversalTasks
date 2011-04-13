//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace DocsVision.Runtime.Remoting.Formatters.Binary
{
	internal sealed class BinaryWireFormatter : IWireFormatter
	{
		// Class that used to pass uri to the binary serializer
		private class UriHeaderHandler
		{
			private string _uri;

			#region Constructors

			public UriHeaderHandler(string uri)
			{
				_uri = uri;
			}

			#endregion

			public object HeaderHandler(Header[] Headers)
			{
				return _uri;
			}
		}

		// Formatter instance
		private BinaryFormatter _serializer;
		private BinaryFormatter _deserializer;

		#region Constructors

		public BinaryWireFormatter()
		{
			StreamingContext context = new StreamingContext(StreamingContextStates.Other);
			RemotingSurrogateSelector selector = new RemotingSurrogateSelector();

			_serializer = new BinaryFormatter(selector, context);
			_serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;

			_deserializer = new BinaryFormatter(null, context);
			_deserializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
		}

		#endregion

		#region IWireFormatter Members

		public void SerializeRequest(IMethodCallMessage message, ITransportHeaders requestHeaders, Stream requestStream)
		{
			_serializer.Serialize(requestStream, message, null);
		}

		public IMethodCallMessage DeserializeRequest(ITransportHeaders requestHeaders, Stream requestStream)
		{
			string uri = (string)requestHeaders["__RequestUri"];
			UriHeaderHandler uriHH = new UriHeaderHandler(uri);

			return (IMethodCallMessage)_deserializer.Deserialize(requestStream, new HeaderHandler(uriHH.HeaderHandler));
		}

		public void SerializeResponse(IMethodReturnMessage message, ITransportHeaders responseHeaders, Stream responseStream)
		{
			_serializer.Serialize(responseStream, message, null);
		}

		public IMethodReturnMessage DeserializeResponse(IMethodCallMessage message, ITransportHeaders responseHeaders, Stream responseStream)
		{
			return (IMethodReturnMessage)_deserializer.DeserializeMethodResponse(responseStream, null, message);
		}

		#endregion
	}
}