//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.ComponentModel;

namespace DocsVision.Security
{
	public enum SecurityInformationType
	{
		Owner = 0x01,
		Group = 0x02,
		Dacl  = 0x04,
		Sacl  = 0x08,
		All   = 0x0F,
	}
}