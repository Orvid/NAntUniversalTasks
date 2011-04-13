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
	public sealed class TransportAsyncWrapper : ITransport, IAsyncTransport
	{
		// Wrapped transport
		private ITransport _transport;

		#region Constructors

		private TransportAsyncWrapper(ITransport transport)
		{
			_transport = transport;
		}

		#endregion

		#region ITransport Members

		public bool IsConnected
		{
			get
			{
				return _transport.IsConnected;
			}
		}

		public bool IsLocal
		{
			get
			{
				return _transport.IsLocal;
			}
		}

		public string Url
		{
			get
			{
				return _transport.Url;
			}
		}

		public IPrincipal ClientPrincipal
		{
			get
			{
				return _transport.ClientPrincipal;
			}
		}

		public void Connect(string url)
		{
			_transport.Connect(url);
		}

		public void Bind(string url)
		{
			_transport.Bind(url);
		}

		public void BindAuth(string url, SecurityDescriptor securityDescriptor)
		{
			_transport.BindAuth(url, securityDescriptor);
		}

		public void Listen(int backLog)
		{
			_transport.Listen(backLog);
		}

		public ITransport Accept()
		{
			return GetWrapper(_transport.Accept());
		}

		public void Send(byte[] buffer, int offset, int size)
		{
			_transport.Send(buffer, offset, size);
		}

		public int Receive(byte[] buffer, int offset, int size)
		{
			return _transport.Receive(buffer, offset, size);
		}

		public int Peek(byte[] buffer, int offset, int size)
		{
			return _transport.Peek(buffer, offset, size);
		}

		public void Flush()
		{
			_transport.Flush();
		}

		public void Close()
		{
			_transport.Close();
		}

		#endregion

		#region IAsyncTransport Members

		public IAsyncResult BeginConnect(string url, AsyncCallback callback, object state)
		{
			return TransportAsyncWorker.Perform(_transport, TransportAsyncWorker.OperationType.Connect,
				url, callback, state);
		}

		public void EndConnect(IAsyncResult result)
		{
			TransportAsyncResult asyncResult = (TransportAsyncResult)result;
			asyncResult.AsyncWaitHandle.WaitOne();
			Exception ex = asyncResult.Worker.Exception;
			if (ex != null)
				throw ex;
		}

		public IAsyncResult BeginAccept(AsyncCallback callback, object state)
		{
			return TransportAsyncWorker.Perform(_transport, TransportAsyncWorker.OperationType.Accept,
				callback, state);
		}

		public ITransport EndAccept(IAsyncResult result)
		{
			TransportAsyncResult asyncResult = (TransportAsyncResult)result;
			asyncResult.AsyncWaitHandle.WaitOne();
			Exception ex = asyncResult.Worker.Exception;
			if (ex != null)
				throw ex;
			return asyncResult.Worker.AcceptedConnection;
		}

		public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return TransportAsyncWorker.Perform(_transport, TransportAsyncWorker.OperationType.Receive,
				buffer, offset, size, callback, state);
		}

		public int EndReceive(IAsyncResult result)
		{
			TransportAsyncResult asyncResult = (TransportAsyncResult)result;
			asyncResult.AsyncWaitHandle.WaitOne();
			Exception ex = asyncResult.Worker.Exception;
			if (ex != null)
				throw ex;
			return asyncResult.Worker.BytesReceived;
		}

		public IAsyncResult BeginSend(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return TransportAsyncWorker.Perform(_transport, TransportAsyncWorker.OperationType.Send,
				buffer, offset, size, callback, state);
		}

		public void EndSend(IAsyncResult result)
		{
			TransportAsyncResult asyncResult = (TransportAsyncResult)result;
			asyncResult.AsyncWaitHandle.WaitOne();
			Exception ex = asyncResult.Worker.Exception;
			if (ex != null)
				throw ex;
		}

		#endregion

		public static IAsyncTransport GetWrapper(ITransport transport)
		{
			// determine whether transport supports async operations and construct wrapper if not
			return transport is IAsyncTransport
				? (IAsyncTransport)transport
				: new TransportAsyncWrapper(transport);
		}
	}
}