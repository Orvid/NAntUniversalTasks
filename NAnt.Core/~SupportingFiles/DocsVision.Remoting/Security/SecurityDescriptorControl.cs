//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Security
{
	[Flags]
	public enum SecurityDescriptorControl
	{
		OwnerDefaulted        = 0x0001,
		GroupDefaulted        = 0x0002,
		DaclPresent           = 0x0004,
		DaclDefaulted         = 0x0008,
		SaclPresent           = 0x0010,
		SaclDefaulted         = 0x0020,
		DaclAutoInheritReq    = 0x0100,
		SaclAutoInheritReq    = 0x0200,
		DaclAutoInherited     = 0x0400,
		SaclAutoInherited     = 0x0800,
		DaclProtected         = 0x1000,
		SaclProtected         = 0x2000,
		RmControlValid        = 0x4000,
		SelfRelative          = 0x8000,

		DaclInheritenceMask   = (DaclAutoInheritReq | DaclAutoInherited | DaclProtected),
		SaclInheritenceMask   = (SaclAutoInheritReq | SaclAutoInherited | SaclProtected),
		InheritenceMask       = (DaclInheritenceMask | SaclInheritenceMask),
	}
}