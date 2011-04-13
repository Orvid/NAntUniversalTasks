//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Security.SSPI
{
	[Flags]
	public enum SecurityCredentialsType
	{
		InBound = 1,
		OutBound = 2,
		Both = 3
	}
}