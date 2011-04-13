//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using DocsVision.Util;

namespace DocsVision.Security
{
	public sealed class SecurityIdentifier
	{
		public struct WellKnown
		{
			#region Universal SIDs

			public static readonly SecurityIdentifier Null = new SecurityIdentifier(SecurityNative.NullSidAuthority, new uint[] {SecurityNative.SECURITY_NULL_RID});
			public static readonly SecurityIdentifier World = new SecurityIdentifier(SecurityNative.WorldSidAuthority, new uint[] {SecurityNative.SECURITY_WORLD_RID});
			public static readonly SecurityIdentifier Local = new SecurityIdentifier(SecurityNative.LocalSidAuthority, new uint[] {SecurityNative.SECURITY_LOCAL_RID});
			public static readonly SecurityIdentifier CreatorOwner = new SecurityIdentifier(SecurityNative.CreatorSidAuthority, new uint[] {SecurityNative.SECURITY_CREATOR_OWNER_RID});
			public static readonly SecurityIdentifier CreatorGroup = new SecurityIdentifier(SecurityNative.CreatorSidAuthority, new uint[] {SecurityNative.SECURITY_CREATOR_GROUP_RID});
			public static readonly SecurityIdentifier CreatorOwnerServer = new SecurityIdentifier(SecurityNative.CreatorSidAuthority, new uint[] {SecurityNative.SECURITY_CREATOR_OWNER_SERVER_RID});
			public static readonly SecurityIdentifier CreatorGroupServer = new SecurityIdentifier(SecurityNative.CreatorSidAuthority, new uint[] {SecurityNative.SECURITY_CREATOR_GROUP_SERVER_RID});

			#endregion

			#region NT Authority SIDs

			public static readonly SecurityIdentifier Dialup = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_DIALUP_RID});
			public static readonly SecurityIdentifier Network = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_NETWORK_RID});
			public static readonly SecurityIdentifier Batch = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BATCH_RID});
			public static readonly SecurityIdentifier Interactive = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_INTERACTIVE_RID});
			public static readonly SecurityIdentifier Service = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_SERVICE_RID});
			public static readonly SecurityIdentifier AnonymousLogon = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_ANONYMOUS_LOGON_RID});
			public static readonly SecurityIdentifier Proxy = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_PROXY_RID});
			public static readonly SecurityIdentifier ServerLogon = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_SERVER_LOGON_RID});
			public static readonly SecurityIdentifier Self = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_PRINCIPAL_SELF_RID});
			public static readonly SecurityIdentifier AuthenticatedUser = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_AUTHENTICATED_USER_RID});
			public static readonly SecurityIdentifier RestrictedCode = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_RESTRICTED_CODE_RID});
			public static readonly SecurityIdentifier TerminalServer = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_TERMINAL_SERVER_RID});
			public static readonly SecurityIdentifier RemoteLogon = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_REMOTE_LOGON_RID});
			public static readonly SecurityIdentifier LocalSystem = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_LOCAL_SYSTEM_RID});
			public static readonly SecurityIdentifier LocalService = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_LOCAL_SERVICE_RID});
			public static readonly SecurityIdentifier NetworkService = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_NETWORK_SERVICE_RID});

			#endregion

			#region NT Authority\BUILTIN SIDs

			public static readonly SecurityIdentifier Admins = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_ADMINS});
			public static readonly SecurityIdentifier Users = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_USERS});
			public static readonly SecurityIdentifier Guests = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_GUESTS});
			public static readonly SecurityIdentifier PowerUsers = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_POWER_USERS});
			public static readonly SecurityIdentifier AccountOps = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_ACCOUNT_OPS});
			public static readonly SecurityIdentifier SystemOps = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_SYSTEM_OPS});
			public static readonly SecurityIdentifier PrintOps = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_PRINT_OPS});
			public static readonly SecurityIdentifier BackupOps = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_BACKUP_OPS});
			public static readonly SecurityIdentifier Replicator = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_REPLICATOR});
			public static readonly SecurityIdentifier RasServers = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_RAS_SERVERS});
			public static readonly SecurityIdentifier PreW2KAccess = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_PREW2KCOMPACCESS});
			public static readonly SecurityIdentifier RemoteDesktopUsers = new SecurityIdentifier(SecurityNative.NTAuthority, new uint[] {SecurityNative.SECURITY_BUILTIN_DOMAIN_RID, SecurityNative.DOMAIN_ALIAS_RID_REMOTE_DESKTOP_USERS});

			#endregion
		}

		// Pointer to security identifier
		private LocalAllocHandle _pSid;

		// SID properties
		private string _stringSid;
		private string _accountName;
		private int _size;

		#region Constructors

		public SecurityIdentifier(byte[] authorityIdentifier, uint[] rids)
		{
			// parameters validation
			if (authorityIdentifier == null)
				throw new ArgumentNullException("authorityIdentifier");
			if (rids == null)
				throw new ArgumentNullException("rids");

			// get SID size
			byte subAuthCount = (byte)rids.Length;
			_size = SecurityNative.GetSidLengthRequired(subAuthCount);

			// allocate memory for security identifier
			_pSid = new LocalAllocHandle(_size);

			// initialize security identifier
			GCHandle gcAuthId = GCHandle.Alloc(authorityIdentifier, GCHandleType.Pinned);
			if (!SecurityNative.InitializeSid(_pSid, gcAuthId.AddrOfPinnedObject(), subAuthCount))
				throw new Win32Exception(Marshal.GetLastWin32Error());
			gcAuthId.Free();

			// initialize subauthorities
			for (byte i = 0; i < subAuthCount; ++i)
			{
				Marshal.WriteInt32(SecurityNative.GetSidSubAuthority(_pSid, i), (int)rids[i]);
			}
		}

		public SecurityIdentifier(string sid)
		{
			// parameters validation
			if (sid == null)
				throw new ArgumentNullException("sid");

			// convert string to SID
			IntPtr pSid;
			if (!SecurityNative.ConvertStringSidToSid(sid, out pSid))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			// store pointer
			_pSid = new LocalAllocHandle(pSid);

			// get SID size
			_size = SecurityNative.GetLengthSid(pSid);
		}

		internal SecurityIdentifier(IntPtr pSid, bool copy)
		{
			// parameters validation
			if (pSid == IntPtr.Zero)
				throw new ArgumentNullException("pSid");

			// get SID size
			_size = SecurityNative.GetLengthSid(pSid);

			if (copy)
			{
				// allocate memory for security identifier
				_pSid = new LocalAllocHandle(_size);

				// copy security identifier
				if (!SecurityNative.CopySid((uint)_size, (IntPtr)_pSid, pSid))
					throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			else
			{
				// store pointer
				_pSid = new LocalAllocHandle(pSid);
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Native handle
		/// </summary>
		public IntPtr Handle
		{
			get
			{
				return _pSid;
			}
		}

		/// <summary>
		/// Account name
		/// </summary>
		public string AccountName
		{
			get
			{
				if (_accountName == null)
				{
					_accountName = LookupAccount(null, this);
				}

				return _accountName;
			}
		}

		/// <summary>
		/// Size in bytes
		/// </summary>
		public int Size
		{
			get
			{
				return _size;
			}
		}

		#endregion

		#region Lookup account methods

		/// <summary>
		/// Returns account SID for the name
		/// </summary>
		public static SecurityIdentifier LookupAccount(string systemName, string accountName)
		{
			// parameters validation
			if (accountName == null)
				throw new ArgumentNullException("accountName");

			uint cbAccountSid = 256;
			IntPtr pAccountSid = IntPtr.Zero;

			uint cbDomainName = 256;
			IntPtr pDomainName = IntPtr.Zero;

			try
			{
				while (true)
				{
					// allocate buffers
					pAccountSid = Win32.LocalAlloc(Win32.LMEM_FIXED, cbAccountSid);
					pDomainName = Win32.LocalAlloc(Win32.LMEM_FIXED, cbDomainName);

					if ((pAccountSid == IntPtr.Zero) || (pDomainName == IntPtr.Zero))
						throw new Win32Exception(Marshal.GetLastWin32Error());

					// lookup for account
					int accountType;
					if (SecurityNative.LookupAccountName(
						systemName,
						accountName,
						pAccountSid,
						ref cbAccountSid,
						pDomainName,
						ref cbDomainName,
						out accountType))
					{
						return new SecurityIdentifier(pAccountSid, false);
					}
					else
					{
						int error = Marshal.GetLastWin32Error();
						if (error != Win32.ERROR_OUTOFMEMORY)
							throw new Win32Exception(error);
					}
				}
			}
			finally
			{
				if (pAccountSid != IntPtr.Zero)
					Win32.LocalFree(pAccountSid);
				if (pDomainName != IntPtr.Zero)
					Win32.LocalFree(pDomainName);
			}
		}

		/// <summary>
		/// Returns account name for the SID
		/// </summary>
		public static string LookupAccount(string systemName, SecurityIdentifier accountSid)
		{
			// parameters validation
			if (accountSid == null)
				throw new ArgumentNullException("accountSid");

			uint cbAccountName = 256;
			IntPtr pAccountName = IntPtr.Zero;

			uint cbDomainName = 256;
			IntPtr pDomainName = IntPtr.Zero;

			try
			{
				while (true)
				{
					// allocate buffers
					pAccountName = Win32.LocalAlloc(Win32.LMEM_FIXED, cbAccountName);
					pDomainName = Win32.LocalAlloc(Win32.LMEM_FIXED, cbDomainName);

					if ((pAccountName == IntPtr.Zero) || (pDomainName == IntPtr.Zero))
						throw new Win32Exception(Marshal.GetLastWin32Error());

					// lookup for account
					int accountType;
					if (SecurityNative.LookupAccountSid(
						systemName,
						accountSid.Handle,
						pAccountName,
						ref cbAccountName,
						pDomainName,
						ref cbDomainName,
						out accountType))
					{
						return Marshal.PtrToStringUni(pDomainName) + "\\" + Marshal.PtrToStringUni(pAccountName);
					}
					else
					{
						int error = Marshal.GetLastWin32Error();
						if (error != Win32.ERROR_OUTOFMEMORY)
							throw new Win32Exception(error);
					}
				}
			}
			finally
			{
				if (pAccountName != IntPtr.Zero)
					Win32.LocalFree(pAccountName);
				if (pDomainName != IntPtr.Zero)
					Win32.LocalFree(pDomainName);
			}
		}

		#endregion

		/// <summary>
		/// Copies SID
		/// </summary>
		public SecurityIdentifier Copy()
		{
			return new SecurityIdentifier(_pSid, true);
		}

		/// <summary>
		/// Compares two SIDs
		/// </summary>
		public bool Compare(SecurityIdentifier sid)
		{
			return SecurityNative.EqualSid(_pSid, sid.Handle);
		}

		/// <summary>
		/// Compares two SID prefixes
		/// </summary>
		public bool ComparePrefix(SecurityIdentifier sid)
		{
			return SecurityNative.EqualPrefixSid(_pSid, sid.Handle);
		}

		/// <summary>
		/// Returns security identifier string representation
		/// </summary>
		public override string ToString()
		{
			if (_stringSid == null)
			{
				// convert SID to string
				IntPtr pStringSd;
				if (!SecurityNative.ConvertSidToStringSid(_pSid, out pStringSd))
					throw new Win32Exception(Marshal.GetLastWin32Error());

				_stringSid = Marshal.PtrToStringUni(pStringSd);
				Win32.LocalFree(pStringSd);
			}

			return _stringSid;
		}
	}
}