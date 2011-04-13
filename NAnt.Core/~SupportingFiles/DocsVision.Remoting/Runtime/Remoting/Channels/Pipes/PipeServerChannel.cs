//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;

using DocsVision.Security;
using DocsVision.Net.Pipes;
using DocsVision.Runtime.Remoting.Transport;
using DocsVision.Runtime.Remoting.Transport.Pipes;

namespace DocsVision.Runtime.Remoting.Channels.Pipes
{
	public sealed class PipeServerChannel : IChannel, IChannelReceiver
	{
		// Channel information
		private PipeName _pipeName;
		private string _pipe = "ServerPipe";
		private string _channelName = "pipe";
		private int _channelPriority = 1;
		private ChannelDataStore _channelData;
		private SecurityDescriptor _securityDescriptor;

		// Transport sinks
		private IServerChannelSinkProvider _sinkProvider;
		private ServerTransportSink _transportSink;

		// Listener thread
		private TransportListener _transportListener;
		private Thread _listenerThread;
		private AutoResetEvent _listenerStartedSignal = new AutoResetEvent(false);
		private bool _isListening = false;
		private Exception _listeningException;

		// Callback that will process incoming requests
		private ProcessRequestCallback _requestHandler;

		#region Constructors

		/// <summary>
		/// Creates a new server channel
		/// </summary>
		public PipeServerChannel()
		{
			Initialize(null, null);
		}

		/// <summary>
		/// Creates a new server channel
		/// </summary>
		public PipeServerChannel(string pipeName)
		{
			Hashtable properties = new Hashtable();
			properties.Add("pipe", pipeName);
			Initialize(properties, null);
		}

		/// <summary>
		/// Creates a new server channel with the specified properties and sink provider
		/// </summary>
		public PipeServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider)
		{
			Initialize(properties, sinkProvider);
		}

		#endregion

		#region IChannel Members

		public string ChannelName
		{
			get
			{
				return _channelName;
			}
		}

		public int ChannelPriority
		{
			get
			{
				return _channelPriority;
			}
		}

		public string Parse(string url, out string objectUri)
		{
			return PipeChannel.ParseUrl(url, out objectUri);
		}

		#endregion

		#region IChannelReceiver Members

		public object ChannelData
		{
			get
			{
				return _channelData;
			}
		}

		public string[] GetUrlsForUri(string objectUri)
		{
			if (!objectUri.StartsWith(@"\"))
				objectUri = @"\" + objectUri;
			objectUri = ChannelUri + objectUri;
			return new string[1] { objectUri };
		}

		public void StartListening(object data)
		{
			if (!_listenerThread.IsAlive)
			{
				// start listener thread
				_listenerThread.Start();
				_listenerStartedSignal.WaitOne();

				if (_listeningException != null)
					throw _listeningException;
				_isListening = true;
			}
		}

		public void StopListening(object data)
		{
			_isListening = false;
			if (_transportListener != null)
			{
				// simply close the transport
				_transportListener.Stop();
			}
		}

		#endregion

		#region Properties

		public string ChannelUri
		{
			get
			{
				return _pipeName.ToString();
			}
		}

		#endregion

		private void Initialize(IDictionary properties, IServerChannelSinkProvider sinkProvider)
		{
			if (properties != null)
			{
				// read property values
				foreach (DictionaryEntry property in properties)
				{
					switch ((string)property.Key)
					{
						case "name": _channelName = Convert.ToString(property.Value); break;
						case "priority": _channelPriority = Convert.ToInt32(property.Value); break;
						case "pipe": _pipe = Convert.ToString(property.Value); break;
						case "securityDescriptor": _securityDescriptor = (property.Value as SecurityDescriptor); break;
					}
				}
			}

			// setup pipe name
			_pipeName = new PipeName(@"\\.\pipe\" + _pipe);

			// create the chain of the sink providers that will process all messages
			_sinkProvider = ChannelHelper.ServerChannelCreateSinkProviderChain(sinkProvider);
			_channelData = ChannelHelper.ServerChannelCreateDataStore(ChannelUri, _sinkProvider);

			// create transport sink
			IServerChannelSink nextSink = ChannelServices.CreateServerChannelSinkChain(_sinkProvider, this);
			_transportSink = new ServerTransportSink(nextSink);

			// create listener thread
			_transportListener = new TransportListener(ChannelUri, typeof(PipeTransport));
			_listenerThread = new Thread(new ThreadStart(ListenerStart));
			_listenerThread.IsBackground = true;

			_requestHandler = new ProcessRequestCallback(_transportSink.ProcessRequest);

			// start listening on the channel
			StartListening(null);
		}

		private void ListenerStart()
		{
			_listeningException = null;
			bool transportIsOk = true;

			try
			{
				// make transport to listen for the client connections
				_transportListener.Start(PipeOption.MaxConnections, _securityDescriptor);
			}
			catch (Exception ex)
			{
				_listeningException = ex;
				transportIsOk = false;
			}
			finally
			{
				// signal to main thread that listener started
				_listenerStartedSignal.Set();
			}

			while (transportIsOk)
			{
				try
				{
					// wait for the client connection
					BinaryConnection connection = new BinaryConnection(this, _transportListener.AcceptClient());

					// serve requests on this connection
					RequestQueue.DefaultQueue.AddConnection(connection, _requestHandler);
				}
				catch (Exception ex)
				{
					if (!_isListening)
					{
						// listener was stopped
						transportIsOk = false;
					}
					else
					{
						// store exception for diagnostics usage
						_listeningException = ex;
					}
				}
			}
		}
	}
}