//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace DocsVision.Runtime.Remoting.Formatters.Simple
{
	public sealed class SimpleClientFormatterSinkProvider : IClientFormatterSinkProvider
	{
		// Formatter used for serialization
		private static SimpleWireFormatter s_Formatter = new SimpleWireFormatter();

		// Next provider in the chain
		private IClientChannelSinkProvider _nextProvider;

		#region Constructors

		public SimpleClientFormatterSinkProvider()
		{
		}

		#endregion

		#region IClientChannelSinkProvider Members

		public IClientChannelSinkProvider Next
		{
			get
			{
				return _nextProvider;
			}
			set
			{
				_nextProvider = value;
			}
		}

		public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
		{
			IClientChannelSink nextSink = null;
			if (_nextProvider != null)
			{
				nextSink = _nextProvider.CreateSink(channel, url, remoteChannelData);
			}

			if (nextSink != null)
			{
				return new ClientFormatterSink(s_Formatter, nextSink);
			}

			return null;
		}

		#endregion
	}
}