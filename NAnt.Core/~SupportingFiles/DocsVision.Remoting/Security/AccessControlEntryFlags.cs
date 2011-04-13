//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Security
{
	[Flags]
	public enum AccessControlEntryFlags
	{
		Normal             = 0x00,
		ObjectInherit      = 0x01,
		ContainerInherit   = 0x02,
		NoPropogateInherit = 0x04,
		InheritOnly        = 0x08,
		Inherited          = 0x10,
		ValidInherit       = 0x1F,
		SuccessfullAccess  = 0x40,
		FailedAccess       = 0x80,
	}
}