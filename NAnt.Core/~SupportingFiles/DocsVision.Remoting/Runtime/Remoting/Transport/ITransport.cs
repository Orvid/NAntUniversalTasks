//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Security.Principal;

using DocsVision.Security;

namespace DocsVision.Runtime.Remoting.Transport
{
	public interface ITransport
	{
		/// <summary>
		/// Connection status
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// Determines if this is local connection
		/// </summary>
		bool IsLocal { get; }

		/// <summary>
		/// Connection url
		/// </summary>
		string Url { get; }

		/// <summary>
		/// Client principal
		/// </summary>
		IPrincipal ClientPrincipal { get; }

		/// <summary>
		/// Connects to the server with the specified url
		/// </summary>
		void Connect(string url);

		/// <summary>
		/// Associates a transport with the specified url
		/// </summary>
		void Bind(string url);

		/// <summary>
		/// Associates a transport with the specified url
		/// </summary>
		void BindAuth(string url, SecurityDescriptor securityDescriptor);

		/// <summary>
		/// Places a transport in a listening state to allow clients to connect
		/// </summary>
		void Listen(int backLog);

		/// <summary>
		/// Establishes connection with the client and returns connected transport
		/// </summary>
		ITransport Accept();

		/// <summary>
		/// Sends a data via connected transport
		/// </summary>
		void Send(byte[] buffer, int offset, int size);

		/// <summary>
		/// Receives data via connected transport
		/// </summary>
		int Receive(byte[] buffer, int offset, int size);

		/// <summary>
		/// Copies data into a buffer without removing it from the tranport
		/// </summary>
		int Peek(byte[] buffer, int offset, int size);

		/// <summary>
		/// Flushes internal transport buffers
		/// </summary>
		void Flush();

		/// <summary>
		/// Closes the transport
		/// </summary>
		void Close();
	}
}