//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Security.Principal;

using DocsVision.Security;
using DocsVision.Net.Pipes;

namespace DocsVision.Runtime.Remoting.Transport.Pipes
{
	public class PipeTransport : ITransport, IAsyncTransport
	{
		// Address format: \\<host>\pipe\<pipe_name>\<requestUri>
		//                 <----- channel uri ----->
		private static Regex s_PipeName = new Regex(@"\\\\([^\\]*)\\pipe\\([^\\]*)\\?(.*)");
		private Pipe _pipe;
		private string _url;

		#region Constructors

		public PipeTransport() : this(new Pipe())
		{
		}

		public PipeTransport(Pipe pipe)
		{
			// parameters validation
			if (pipe == null)
				throw new ArgumentNullException("pipe");

			_pipe = pipe;
		}

		#endregion

		#region ITransport Members

		public bool IsConnected
		{
			get
			{
				return _pipe.IsConnected;
			}
		}

		public bool IsLocal
		{
			get
			{
				return _pipe.Name.IsLocal;
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
				return _pipe.ClientPrincipal;
			}
		}

		public void Connect(string url)
		{
			_pipe.Connect(GetPipeName(url));
			_url = url;
		}

		public void Bind(string url)
		{
			BindAuth(url, null);
		}

		public void BindAuth(string url, SecurityDescriptor securityDescriptor)
		{
			_pipe.Bind(GetPipeName(url), securityDescriptor);
			_url = url;
		}

		public void Listen(int backLog)
		{
			_pipe.Listen(backLog);
		}

		public ITransport Accept()
		{
			return new PipeTransport(_pipe.Accept());
		}

		public void Send(byte[] buffer, int offset, int size)
		{
			_pipe.Send(buffer, offset, size);
		}

		public int Receive(byte[] buffer, int offset, int size)
		{
			return _pipe.Receive(buffer, offset, size);
		}

		public int Peek(byte[] buffer, int offset, int size)
		{
			return _pipe.Peek(buffer, offset, size);
		}

		public void Flush()
		{
			_pipe.Flush();
		}

		public void Close()
		{
			_pipe.Close();
		}

		#endregion

		#region IAsyncTransport Members

		public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return _pipe.BeginReceive(buffer, offset, size, callback, state);
		}

		public int EndReceive(IAsyncResult result)
		{
			return _pipe.EndReceive(result);
		}

		public IAsyncResult BeginSend(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return _pipe.BeginSend(buffer, offset, size, callback, state);
		}

		public void EndSend(IAsyncResult result)
		{
			_pipe.EndSend(result);
		}

		#endregion

		public Pipe Pipe
		{
			get
			{
				return _pipe;
			}
		}

		public static bool ParseUrl(string url, out IDictionary parts)
		{
			parts = new Hashtable(4);
			Match m = s_PipeName.Match(url);
			if (m.Success)
			{
				parts["schema"] = "pipe";
				parts["host"] = m.Groups[1].Value;
				parts["pipe"] = m.Groups[2].Value;
				parts["requestUri"] = m.Groups[3].Value;
			}
			return m.Success;
		}

		private static PipeName GetPipeName(string url)
		{
			IDictionary parts = null;
			if (!ParseUrl(url, out parts))
				throw new ArgumentException("Invalid pipe address specified");

			PipeName pipeName = new PipeName((string)parts["host"], (string)parts["pipe"]);
			return pipeName;
		}
	}
}