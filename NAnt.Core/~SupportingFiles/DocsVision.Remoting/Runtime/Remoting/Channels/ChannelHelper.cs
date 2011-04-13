//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

using DocsVision.Runtime.Remoting.Formatters.Binary;

namespace DocsVision.Runtime.Remoting.Channels
{
	internal sealed class ChannelHelper
	{
		private ChannelHelper()
		{
			// this class is non creatable
		}

		public static IClientChannelSinkProvider ClientChannelCreateSinkProviderChain(
			IClientChannelSinkProvider formatterSinkProvider,
			IClientChannelSinkProvider transportSinkProvider)
		{
			// we use MSFT BinaryFormatter by default for maximum compatibility
			if (formatterSinkProvider == null)
			{
				formatterSinkProvider = new BinaryClientFormatterSinkProvider();
			}

			// set transport provider to be the last in the chain
			IClientChannelSinkProvider sinkProvider = formatterSinkProvider;
			while (sinkProvider.Next != null)
			{
				sinkProvider = sinkProvider.Next;
			}
			sinkProvider.Next = transportSinkProvider;

			return formatterSinkProvider;
		}

		public static IMessageSink ClientChannelCreateMessageSink(
			IChannelSender channel,
			IClientChannelSinkProvider sinkProviderChain,
			string url,
			object remoteChannelData,
			out string objectUri)
		{
			objectUri = null;
			string channelUri = null;

			if (url != null)
			{
				// parse returns null if this is not one of our url's
				channelUri = channel.Parse(url, out objectUri);
			}
			else if (remoteChannelData != null)
			{
				IChannelDataStore cds = remoteChannelData as IChannelDataStore;
				if (cds != null)
				{
					// see if this is a valid uri
					string simpleChannelUri = channel.Parse(cds.ChannelUris[0], out objectUri);
					if (simpleChannelUri != null)
						channelUri = cds.ChannelUris[0];
				}
			}

			if (channelUri != null)
			{
				// create message sink
				return sinkProviderChain.CreateSink(channel,
					(url != null ? url : channelUri), remoteChannelData) as IMessageSink;
			}

			// could not connect
			return null;
		}

		public static IServerChannelSinkProvider ServerChannelCreateSinkProviderChain(
			IServerChannelSinkProvider formatterSinkProvider)
		{
			if (formatterSinkProvider == null)
			{
				// we use MSFT BinaryFormatter by default for maximum compatibility
				formatterSinkProvider = new BinaryServerFormatterSinkProvider();
			}

			return formatterSinkProvider;
		}

		public static ChannelDataStore ServerChannelCreateDataStore(
			string channelUri,
			IServerChannelSinkProvider sinkProviderChain)
		{
			string[] uris = new string[1] { channelUri };
			ChannelDataStore channelData = new ChannelDataStore(uris);

			// walk throw the chain of sink providers and collect their data
			IServerChannelSinkProvider sinkProvider = sinkProviderChain;
			while (sinkProvider != null)
			{
				sinkProvider.GetChannelData(channelData);
				sinkProvider = sinkProvider.Next;
			}

			return channelData;
		}
	}
}