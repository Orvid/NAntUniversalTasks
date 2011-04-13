//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Runtime.Remoting.Transport
{
	public interface IAsyncTransport : ITransport
	{
		/// <summary>
		/// Receives data via connected transport
		/// </summary>
		IAsyncResult BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, object state);
		int EndReceive(IAsyncResult result);

		/// <summary>
		/// Sends a data via connected transport
		/// </summary>
		IAsyncResult BeginSend(byte[] buffer, int offset, int size, AsyncCallback callback, object state);
		void EndSend(IAsyncResult result);
	}
}