//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace DocsVision.Security.SSPI
{
	[Serializable]
	public class SSPIException : Win32Exception
	{
		public SSPIException() : this(Marshal.GetLastWin32Error())
		{
		}

		public SSPIException(int errorCode) : base(errorCode)
		{
		}

		public SSPIException(int errorCode, string message) : base(errorCode, message)
		{
		}

		protected SSPIException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}