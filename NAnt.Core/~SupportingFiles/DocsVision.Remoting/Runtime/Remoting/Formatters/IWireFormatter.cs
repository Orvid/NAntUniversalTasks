//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace DocsVision.Runtime.Remoting.Formatters
{
	internal interface IWireFormatter
	{
		/// <summary>
		/// Serializes request message
		/// </summary>
		void SerializeRequest(IMethodCallMessage message, ITransportHeaders requestHeaders, Stream requestStream);

		/// <summary>
		/// Deserializes request message
		/// </summary>
		IMethodCallMessage DeserializeRequest(ITransportHeaders requestHeaders, Stream requestStream);

		/// <summary>
		/// Serializes response message
		/// </summary>
		void SerializeResponse(IMethodReturnMessage message, ITransportHeaders responseHeaders, Stream responseStream);

		/// <summary>
		/// Deserializes response message
		/// </summary>
		IMethodReturnMessage DeserializeResponse(IMethodCallMessage message, ITransportHeaders responseHeaders, Stream responseStream);
	}
}