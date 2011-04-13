//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

using DocsVision.Security;

namespace DocsVision.Runtime.Remoting.Transport
{
	public class TransportListener : IDisposable
	{
		// Associated transport
		private Type _transportType;
		private ITransport _transport;
		private string _url;

		// Object state
		private bool _disposed;

		#region Constructors

		public TransportListener(string url, Type transportType)
		{
			// parameters validation
			if (transportType == null)
				throw new ArgumentNullException("pipe", "Transport type must be specified");

			_transportType = transportType;
			_url = url;
		}

		#endregion

		#region Disposing

		~TransportListener()
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

			try
			{
				// Stop listening
				Stop();
			}
			catch
			{
				// it seems reasonable to ignore this error
			}

			_disposed = true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Indicates the url that listener is bound to
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
		/// Indicates that the listener has been bound to a url and started listening
		/// </summary>
		public bool Active
		{
			get
			{
				return (_transport != null);
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
		/// Starts listening to network requests
		/// </summary>
		public void Start(int backLog)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			if (_transport == null)
			{
				ITransport transport = CreateTransport();
				transport.Bind(_url);
				transport.Listen(backLog);
				_transport = transport;
			}
		}

		/// <summary>
		/// Starts listening to network requests
		/// </summary>
		public void Start(int backLog, SecurityDescriptor securityDescriptor)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			if (_transport == null)
			{
				ITransport transport = CreateTransport();
				transport.BindAuth(_url, securityDescriptor);
				transport.Listen(backLog);
				_transport = transport;
			}
		}

		/// <summary>
		/// Closes the network connection
		/// </summary>
		public void Stop()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			if (_transport != null)
			{
				_transport.Close();
				_transport = null;
			}
		}

		/// <summary>
		/// Accepts a pending connection request and returns underlying network transport
		/// </summary>
		public ITransport Accept()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (_transport == null)
				throw new InvalidOperationException("Listener was stopped");

			return _transport.Accept();
		}

		/// <summary>
		/// Accepts a pending connection request and returns TransportClient object
		/// </summary>
		public TransportClient AcceptClient()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			return new TransportClient(_url, Accept());
		}
	}
}