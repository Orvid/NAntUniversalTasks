//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Net;

namespace DocsVision.Util
{
	internal sealed class NetHelper
	{
		private static string s_HostName;
		private static string s_MachineName;
		private static string s_MachineIp;

		private NetHelper()
		{
			// this class is non creatable
		}

		public static string GetHostName()
		{
			if (s_HostName == null)
			{
				s_HostName = Dns.GetHostName();
				if (s_HostName == null)
				{
					throw new ArgumentNullException("hostName");
				}
			}

			return s_HostName;
		}

		public static string GetMachineName()
		{
			if (s_MachineName == null)
			{
				string machineName = GetHostName();
				if (machineName != null)
				{
					IPHostEntry host = Dns.GetHostByName(machineName);
					if (host != null)
						s_MachineName = host.HostName;
				}
				if (s_MachineName == null)
				{
					throw new ArgumentNullException("machineName");
				}
			}

			return s_MachineName;
		}

		public static string GetMachineIp()
		{
			if (s_MachineIp == null)
			{
				string hostName = GetMachineName();
				IPHostEntry ipEntries = Dns.GetHostByName(hostName);
				if ((ipEntries != null) && (ipEntries.AddressList.Length > 0))
				{
					s_MachineIp = ipEntries.AddressList[0].ToString();
				}
				if (s_MachineIp == null)
				{
					throw new ArgumentNullException("machineIp");
				}
			}

			return s_MachineIp;
		}
	}
}