//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Threading;
using System.Security.Principal;
using System.Runtime.Remoting.Messaging;

namespace DocsVision.Runtime.Remoting
{
	public sealed class RemotingService
	{
		private RemotingService()
		{
			// this class is non creatable
		}

		public static IPrincipal ClientPrincipal
		{
			get
			{
				return (CallContext.GetData("__CurrentPrincipal") as IPrincipal);
			}
			set
			{
				CallContext.SetData("__CurrentPrincipal", value);
			}
		}
	}
}