//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

using DocsVision.IO;

namespace DocsVision.Runtime.Remoting.Channels
{
	internal interface IConnection
	{
		/// <summary>
		/// Initiates receive operation
		/// </summary>
		void BeginReceive(DataAvailableCallback callback, object state);

		/// <summary>
		/// Receives client request
		/// </summary>
		void ReceiveRequest(out ITransportHeaders requestHeaders, out Stream requestStream);

		/// <summary>
		/// Receives server response
		/// </summary>
		void ReceiveResponse(out ITransportHeaders responseHeaders, out Stream responseStream);

		/// <summary>
		/// Returns stream for writing request
		/// </summary>
		Stream GetRequestStream(IMethodCallMessage requestMsg, ITransportHeaders requestHeaders);

		/// <summary>
		/// Sends request to the server
		/// </summary>
		void SendRequest(IMethodCallMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream);

		/// <summary>
		/// Returns stream for writing response
		/// </summary>
		Stream GetResponseStream(IMethodReturnMessage responseMsg, ITransportHeaders responseHeaders);

		/// <summary>
		/// Sends response to the client
		/// </summary>
		void SendResponse(IMethodReturnMessage responseMsg, ITransportHeaders responseHeaders, Stream responseStream);

		/// <summary>
		/// Sends error response to the client
		/// </summary>
		void SendErrorResponse(Exception ex);

		/// <summary>
		/// Closes connection
		/// </summary>
		void Close();
	}
}