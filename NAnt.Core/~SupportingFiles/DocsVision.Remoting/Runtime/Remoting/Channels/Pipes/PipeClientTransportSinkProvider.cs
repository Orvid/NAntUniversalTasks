//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.Remoting.Channels;

namespace DocsVision.Runtime.Remoting.Channels.Pipes
{
	internal sealed class PipeClientTransportSinkProvider : IClientChannelSinkProvider
	{
		#region Constructors

		public PipeClientTransportSinkProvider()
		{
		}

		#endregion

		#region IClientChannelSinkProvider Members

		public IClientChannelSinkProvider Next
		{
			get
			{
				// we are always last one in the chain
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public IClientChannelSink CreateSink(IChannelSender channel, string url, object data)
		{
			// parameters validation
			PipeClientChannel pipeChannel = channel as PipeClientChannel;
			if (pipeChannel == null)
				throw new NotSupportedException();

			// parse object url
			string objectUri;
			string channelUri = pipeChannel.Parse(url, out objectUri);
			if (channelUri == null)
				throw new NotSupportedException();

			// create transport sink
			return new ClientTransportSink(channelUri, pipeChannel.ConnectionCache);
		}

		#endregion
	}
}