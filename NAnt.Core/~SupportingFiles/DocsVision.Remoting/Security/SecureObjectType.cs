//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Security
{
	public enum SecureObjectType
	{
		Unknown = 0,
		File,
		Service,
		Printer,
		RegistryKey,
		NetworkShare,
		KernelObject,
		WindowObject,
		DirectoryObject,
		DirectoryObjectAll,
		ProviderDefined,
		WMIObject,
		RegistryKey_WOW64,
	}
}