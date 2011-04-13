//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Security
{
	public enum AccessControlEntryType
	{
		AccessAllowed = 0,
		AccessDenied,
		Audit,
		Alarm,
	}
}