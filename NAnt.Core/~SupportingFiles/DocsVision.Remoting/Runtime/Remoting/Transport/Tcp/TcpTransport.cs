//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;

using DocsVision.Security;

namespace DocsVision.Runtime.Remoting.Transport.Tcp
{
	public class TcpTransport : ITransport, IAsyncTransport
	{
		// Address format: tcp:\\<host>:<port>\<requestUri>
		//                 <-- channel uri -->
		private static Regex s_TcpAddress = new Regex(@"tcp://([^:]+):([0-9]+)/?(.*)");
		private Socket _socket;
		private string _url;

		#region Constructors

		public TcpTransport() : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
		{
		}

		public TcpTransport(Socket socket)
		{
			// parameters validation
			if (socket == null)
				throw new ArgumentNullException("socket");
			if ((socket.AddressFamily != AddressFamily.InterNetwork) || socket.ProtocolType != ProtocolType.Tcp)
				throw new NotSupportedException("Only TCP connections supported");

			_socket = socket;

			// disable nagle delays
			_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

			// set linger option
			LingerOption lingerOption = new LingerOption(true, 3);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
		}

		#endregion

		#region ITransport Members

		public bool IsConnected
		{
			get
			{
				return _socket.Connected;
			}
		}

		public bool IsLocal
		{
			get
			{
				return (IPAddress.Loopback.Equals(((IPEndPoint)_socket.RemoteEndPoint).Address));
			}
		}

		public string Url
		{
			get
			{
				return _url;
			}
		}

		public IPrincipal ClientPrincipal
		{
			get
			{
				// not supported
				return null;
			}
		}

		public void Connect(string url)
		{
			_socket.Connect(GetSocketAddress(url));
			_url = url;
		}

		public void Bind(string url)
		{
			_socket.Bind(GetSocketAddress(url));
			_url = url;
		}

		public void BindAuth(string url, SecurityDescriptor securityDescriptor)
		{
			throw new NotSupportedException();
		}

		public void Listen(int backLog)
		{
			_socket.Listen(backLog);
		}

		public ITransport Accept()
		{
			return new TcpTransport(_socket.Accept());
		}

		public void Send(byte[] buffer, int offset, int size)
		{
			_socket.Send(buffer, offset, size, SocketFlags.None);
		}

		public int Receive(byte[] buffer, int offset, int size)
		{
			return _socket.Receive(buffer, offset, size, SocketFlags.None);
		}

		public int Peek(byte[] buffer, int offset, int size)
		{
			return _socket.Receive(buffer, offset, size, SocketFlags.Peek);
		}

		public void Flush()
		{
			// nothing to do
		}

		public void Close()
		{
			_socket.Close();
		}

		#endregion

		#region IAsyncTransport Members

		public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return _socket.BeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
		}

		public int EndReceive(IAsyncResult result)
		{
			return _socket.EndReceive(result);
		}

		public IAsyncResult BeginSend(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return _socket.BeginSend(buffer, offset, size, SocketFlags.None, callback, state);
		}

		public void EndSend(IAsyncResult result)
		{
			_socket.EndSend(result);
		}

		#endregion

		public Socket Socket
		{
			get
			{
				return _socket;
			}
		}

		public static bool ParseUrl(string url, out IDictionary parts)
		{
			parts = new Hashtable(4);
			Match m = s_TcpAddress.Match(url);
			if (m.Success)
			{
				parts["schema"] = "tcp";
				parts["host"] = m.Groups[1].Value;
				parts["port"] = m.Groups[2].Value;
				parts["requestUri"] = m.Groups[3].Value;
			}
			return m.Success;
		}

		private static IPEndPoint GetSocketAddress(string url)
		{
			IDictionary parts = null;
			if (!ParseUrl(url, out parts))
				throw new ArgumentException("Invalid TCP address specified");

			IPHostEntry hostInfo = Dns.Resolve((string)parts["host"]);
			string port = (string)parts["port"];

			IPEndPoint socketAddress = new IPEndPoint(hostInfo.AddressList[0], port == null ? 0 : int.Parse(port));
			return socketAddress;
		}
	}
}