//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

namespace DocsVision.Runtime.Remoting.Channels.Tcp
{
	public sealed class TcpChannel : IChannel, IChannelSender, IChannelReceiver
	{
		private const string TcpSchema = "tcp://";

		private IChannelSender _clientChannel;
		private IChannelReceiver _serverChannel;

		#region Constructors

		/// <summary>
		/// Creates a new client channel
		/// </summary>
		public TcpChannel()
		{
			Initialize(null, null, null);
		}

		/// <summary>
		/// Creates a new server channel
		/// </summary>
		public TcpChannel(int port)
		{
			Hashtable properties = new Hashtable();
			properties.Add("port", port);
			Initialize(properties, null, null);
		}

		/// <summary>
		/// Creates a new channel with the specified properties and transport sinks
		/// </summary>
		public TcpChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)
		{
			Initialize(properties, clientSinkProvider, serverSinkProvider);
		}

		#endregion

		#region IChannel Members

		public string ChannelName
		{
			get
			{
				return _clientChannel.ChannelName;
			}
		}

		public int ChannelPriority
		{
			get
			{
				return _clientChannel.ChannelPriority;
			}
		}

		public string Parse(string url, out string objectUri)
		{
			return ParseUrl(url, out objectUri);
		}

		#endregion

		#region IChannelSender Members

		public IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectUri)
		{
			return _clientChannel.CreateMessageSink(url, remoteChannelData, out objectUri);
		}

		#endregion

		#region IChannelReceiver Members

		public object ChannelData
		{
			get
			{
				return (_serverChannel != null ? _serverChannel.ChannelData : null);
			}
		}

		public string[] GetUrlsForUri(string uri)
		{
			return (_serverChannel != null ? _serverChannel.GetUrlsForUri(uri) : null);
		}

		public void StartListening(object data)
		{
			if (_serverChannel != null)
			{
				_serverChannel.StartListening(data);
			}
		}

		public void StopListening(object data)
		{
			if (_serverChannel != null)
			{
				_serverChannel.StopListening(data);
			}
		}

		#endregion

		private void Initialize(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)
		{
			_clientChannel = new TcpClientChannel(properties, clientSinkProvider);
			if ((properties != null) && (properties["port"] != null))
			{
				_serverChannel = new TcpServerChannel(properties, serverSinkProvider);
			}
		}

		internal static string ParseUrl(string url, out string objectUri)
		{
			string channelUri = null;
			objectUri = null;

			// find the starting point of tcp://
			if (url.StartsWith(TcpSchema))
			{
				// find next slash (after end of scheme)
				int separator = url.IndexOf('/', TcpSchema.Length);
				if (separator > 0)
				{
					channelUri = url.Substring(0, separator);
					objectUri = url.Substring(separator + 1);
				}
				else
				{
					channelUri = url;
					objectUri = string.Empty;
				}
			}

			return channelUri;
		}
	}
}