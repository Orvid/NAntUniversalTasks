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

namespace DocsVision.Runtime.Remoting.Channels.Pipes
{
	public sealed class PipeChannel : IChannelSender, IChannelReceiver
	{
		private const string PipeSchema = @"\pipe\";

		private IChannelSender _clientChannel;
		private IChannelReceiver _serverChannel;

		#region Constructors

		/// <summary>
		/// Creates a new client channel
		/// </summary>
		public PipeChannel()
		{
			Initialize(null, null, null);
		}

		/// <summary>
		/// Creates a new server channel
		/// </summary>
		public PipeChannel(string pipeName)
		{
			Hashtable properties = new Hashtable();
			properties.Add("pipe", pipeName);
			Initialize(properties, null, null);
		}

		/// <summary>
		/// Creates a new channel with the specified properties and transport sinks
		/// </summary>
		public PipeChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)
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
			_clientChannel = new PipeClientChannel(properties, clientSinkProvider);
			if ((properties != null) && (properties["pipe"] != null))
			{
				_serverChannel = new PipeServerChannel(properties, serverSinkProvider);
			}
		}

		internal static string ParseUrl(string url, out string objectUri)
		{
			string channelUri = null;
			objectUri = null;

			// find the starting slash
			if (url.StartsWith(@"\"))
			{
				// find \pipe\ entry
				int start = url.IndexOf(PipeSchema);
				if (start > 0)
				{
					// find next slash
					int separator = url.IndexOf('\\', start + PipeSchema.Length);
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
			}

			return channelUri;
		}
	}
}