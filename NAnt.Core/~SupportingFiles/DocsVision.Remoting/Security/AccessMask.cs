//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Security
{
	[Flags]
	public enum AccessMask : uint
	{
		None                   = 0x00000000,
		// Specific rights
		SpecificAll            = 0x0000FFFF,
		// Standard rights
		StandardDelete         = 0x00010000,
		StandardReadControl    = 0x00020000,
		StandardWriteDAC       = 0x00040000,
		StandardWriteOwner     = 0x00080000,
		StandardRequired       = 0x000F0000,
		StandardSynchronize    = 0x00100000,
		StandardAll            = 0x001F0000,
		// AccessSystemAcl access type
		AccessSystemSecurity   = 0x01000000,
		// MaximumAllowed access type
		MaximumAllowed         = 0x02000000,
		// Generic rights
		GenericAll             = 0x10000000,
		GenericExecute         = 0x20000000,
		GenericWrite           = 0x40000000,
		GenericRead            = 0x80000000,

		SpecificMask           = 0x0000FFFF,
		StandardMask           = 0x00FF0000,
		SpecialMask            = 0x0F000000,
		GenericMask            = 0xF0000000,
	}
}