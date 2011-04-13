//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

using DocsVision.Runtime.Remoting.Channels;
using DocsVision.Runtime.Remoting.Transport;
using DocsVision.Runtime.Remoting.Transport.Tcp;

namespace DocsVision.Runtime.Remoting.Channels.Tcp
{
	public sealed class TcpClientChannel : IChannel, IChannelSender
	{
		// Channel information
		private string _channelName = "tcp";
		private int _channelPriority = 1;

		// Channel sink providers chain
		private IClientChannelSinkProvider _sinkProvider;

		// Connection cache
		private ConnectionCache _connectionCache;
		private int _cachedConnections = 10;

		#region Constructors

		/// <summary>
		/// Creates a new client channel
		/// </summary>
		public TcpClientChannel()
		{
			Initialize(null, null);
		}

		/// <summary>
		/// Creates a new client channel with the specified name
		/// </summary>
		public TcpClientChannel(string channelName)
		{
			Hashtable properties = new Hashtable();
			properties.Add("name", channelName);
			Initialize(properties, null);
		}

		/// <summary>
		/// Creates a new client channel with the specified properties
		/// </summary>
		public TcpClientChannel(IDictionary properties, IClientChannelSinkProvider sinkProvider)
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

		#region IChannelSender Members

		public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectUri)
		{
			// create message sink
			return ChannelHelper.ClientChannelCreateMessageSink(this, _sinkProvider, url, remoteChannelData, out objectUri);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Connection cache
		/// </summary>
		internal ConnectionCache ConnectionCache
		{
			get
			{
				return _connectionCache;
			}
		}

		#endregion

		private void Initialize(IDictionary properties, IClientChannelSinkProvider sinkProvider)
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
						case "cachedConnections": _cachedConnections = Convert.ToInt32(property.Value); break;
					}
				}
			}

			// create the chain of the sink providers that will process all messages
			_sinkProvider = ChannelHelper.ClientChannelCreateSinkProviderChain(
				sinkProvider, new TcpClientTransportSinkProvider());

			//  create connection cache
			_connectionCache = new ConnectionCache(_cachedConnections, new ConnectionFactory(CreateConnection));
		}

		private IConnection CreateConnection(string channelUri)
		{
			return new BinaryConnection(this, new TransportClient(channelUri, typeof(TcpTransport)));
		}
	}
}