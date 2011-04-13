//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Security.SSPI
{
	[Flags]
	public enum SecurityContextAttributes
	{
		None = 0,
		Delegate = 1,
		Identify = 2,
		MutualAuthentication = 4
	}
}