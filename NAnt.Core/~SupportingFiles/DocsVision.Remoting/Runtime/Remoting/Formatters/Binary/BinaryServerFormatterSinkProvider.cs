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

namespace DocsVision.Runtime.Remoting.Formatters.Binary
{
	public sealed class BinaryServerFormatterSinkProvider : IServerFormatterSinkProvider
	{
		// Formatter used for serialization
		private static BinaryWireFormatter s_Formatter = new BinaryWireFormatter();

		// Next provider in the chain
		private IServerChannelSinkProvider _nextProvider;

		#region Constructors

		public BinaryServerFormatterSinkProvider()
		{
		}

		#endregion

		#region IServerFormatterSinkProvider Members

		public IServerChannelSinkProvider Next
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

		public void GetChannelData(IChannelDataStore channelData)
		{
			// no idea why we need this
		}

		public IServerChannelSink CreateSink(IChannelReceiver channel)
		{
			IServerChannelSink nextSink = null;
			if (_nextProvider != null)
			{
				nextSink = _nextProvider.CreateSink(channel);
			}

			if (nextSink != null)
			{
				return new ServerFormatterSink(s_Formatter, nextSink);
			}

			return null;
		}

		#endregion
	}
}