//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Security.Principal;

namespace DocsVision.Runtime.Remoting.Transport
{
	public class TransportClient : IDisposable
	{
		// Associated transport
		private Type _transportType;
		private ITransport _transport;
		private string _url;

		// Stream to work this transport data
		private TransportStream _stream;

		// Object state
		private bool _disposed;

		#region Constructors

		public TransportClient(Type transportType)
		{
			// parameters validation
			if (transportType == null)
				throw new ArgumentNullException("Transport type must be specified");

			_transportType = transportType;
		}

		public TransportClient(string url, Type transportType)
		{
			// parameters validation
			if (transportType == null)
				throw new ArgumentNullException("Transport type must be specified");

			_transportType = transportType;
			Connect(url);
		}

		internal TransportClient(string url, ITransport transport)
		{
			// parameters validation
			if (transport == null)
				throw new ArgumentNullException("Transport must be specified");
			if (!transport.IsConnected)
				throw new ArgumentNullException("Transport must be connected");

			_transportType = transport.GetType();
			_transport = transport;
			_url = url;
		}

		#endregion

		#region Disposing

		~TransportClient()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (_stream == null)
			{
				try
				{
					// if stream is not created we must close transport manually
					_transport.Close();
				}
				catch
				{
					// it seems reasonable to ignore this error
				}
			}

			_transport = null;
			_stream = null;
			_disposed = true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Indicates the url that client is connected to
		/// </summary>
		public string Url
		{
			get
			{
				return _url;
			}
		}

		/// <summary>
		/// Underlying network transport
		/// </summary>
		public ITransport Transport
		{
			get
			{
				return _transport;
			}
		}

		/// <summary>
		/// Indicates that a connection has been made
		/// </summary>
		public bool Active
		{
			get
			{
				return (_transport != null);
			}
		}

		/// <summary>
		/// Client principal
		/// </summary>
		public IPrincipal ClientPrincipal
		{
			get
			{
				return (_transport != null ? _transport.ClientPrincipal : null);
			}
		}

		#endregion

		/// <summary>
		/// Creates a new transport instance
		/// </summary>
		protected ITransport CreateTransport()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			ITransport transport = Activator.CreateInstance(_transportType) as ITransport;
			if (transport == null)
				throw new ArgumentException("Incorrect transport type was specified");

			return transport;
		}

		/// <summary>
		/// Connects the client to the specified url
		/// </summary>
		public void Connect(string url)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			if (_transport == null)
			{
				_transport = CreateTransport();
				_transport.Connect(url);
				_url = url;
			}
		}

		/// <summary>
		/// Returns the stream used to read and write data to the remote host
		/// </summary>
		public TransportStream GetStream()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			if (_stream == null)
			{
				_stream = new TransportStream(_transport, true);
			}

			return _stream;
		}

		/// <summary>
		/// Disposes the connection
		/// </summary>
		public void Close()
		{
			((IDisposable)this).Dispose();
		}
	}
}