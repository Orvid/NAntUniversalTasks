//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace DocsVision.Runtime.Remoting
{
	[Serializable]
	public class RemotingExceptionEx : RemotingException
	{
		private byte _statusCode = 0;

		public RemotingExceptionEx(byte statusCode, string message) : base(message)
		{
			_statusCode = statusCode;
		}

		protected RemotingExceptionEx(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public byte StatusCode
		{
			get
			{
				return _statusCode;
			}
		}
	}
}