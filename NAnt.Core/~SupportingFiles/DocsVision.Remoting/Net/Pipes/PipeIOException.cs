//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace DocsVision.Net.Pipes
{
	[Serializable]
	public class PipeIOException : Win32Exception
	{
		public PipeIOException() : base(Marshal.GetLastWin32Error())
		{
		}

		public PipeIOException(int errorCode) : base(errorCode)
		{
		}

		public PipeIOException(int errorCode, string message) : base(errorCode, message)
		{
		}

		protected PipeIOException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}