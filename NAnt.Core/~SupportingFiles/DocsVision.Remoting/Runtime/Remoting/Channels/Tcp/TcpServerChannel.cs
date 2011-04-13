//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Channels;
using System.Net;
using System.Net.Sockets;

using DocsVision.Runtime.Remoting.Transport;
using DocsVision.Runtime.Remoting.Transport.Tcp;
using DocsVision.Util;

namespace DocsVision.Runtime.Remoting.Channels.Tcp
{
	public sealed class TcpServerChannel : IChannel, IChannelReceiver
	{
		// Channel information
		private string _channelName = "tcp";
		private int _channelPriority = 1;
		private string _machineName;
		private int _port = -1;
		private IPAddress _bindToAddr = IPAddress.Any;
		private bool _useIpAddress = true;
		private ChannelDataStore _channelData;

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
		public TcpServerChannel(int port)
		{
			Hashtable properties = new Hashtable();
			properties.Add("port", port);
			Initialize(properties, null);
		}

		/// <summary>
		/// Creates a new server channel with specified name
		/// </summary>
		public TcpServerChannel(int port, string channelName)
		{
			Hashtable properties = new Hashtable();
			properties.Add("name", channelName);
			properties.Add("port", port);
			Initialize(properties, null);
		}

		/// <summary>
		/// Creates a new server channel with specified properties
		/// </summary>
		public TcpServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider)
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
			return TcpChannel.ParseUrl(url, out objectUri);
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
			if (!objectUri.StartsWith("/"))
				objectUri = "/" + objectUri;
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
				return "tcp://" + _machineName + ":" + _port;
			}
		}

		private string ListenerUri
		{
			get
			{
				return "tcp://" + _bindToAddr.ToString() + ":" + _port;
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
						case "port": _port = Convert.ToInt32(property.Value); break;
						case "bindTo": _bindToAddr = IPAddress.Parse(Convert.ToString(property.Value)); break;
						case "machineName": _machineName = Convert.ToString(property.Value); break;
						case "useIpAddress": _useIpAddress = Convert.ToBoolean(property.Value); break;
						case "rejectRemoteRequests": if (Convert.ToBoolean(property.Value)) _bindToAddr = IPAddress.Loopback; break;
					}
				}
			}

			if (_machineName == null)
			{
				// setup machine name
				if (_useIpAddress)
				{
					if (_bindToAddr == IPAddress.Any)
						_machineName = NetHelper.GetMachineIp();
					else
						_machineName = _bindToAddr.ToString();
				}
				else
				{
					_machineName = NetHelper.GetMachineName();
				}
			}

			// create the chain of the sink providers that will process all messages
			_sinkProvider = ChannelHelper.ServerChannelCreateSinkProviderChain(sinkProvider);
			_channelData = ChannelHelper.ServerChannelCreateDataStore(ChannelUri, _sinkProvider);

			// create transport sink
			IServerChannelSink nextSink = ChannelServices.CreateServerChannelSinkChain(_sinkProvider, this);
			_transportSink = new ServerTransportSink(nextSink);

			// create listener thread
			_transportListener = new TransportListener(ListenerUri, typeof(TcpTransport));
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
				_transportListener.Start((int)SocketOptionName.MaxConnections);
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