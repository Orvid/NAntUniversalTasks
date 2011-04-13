//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

using DocsVision.Util;

namespace DocsVision.Security.SSPI
{
	public sealed class SecurityPackage
	{
		// All registered packages
		private static SecurityPackage[] s_Packages;

		// Standard security packages
		private static SecurityPackage s_NTLM;
		private static SecurityPackage s_Kerberos;
		private static SecurityPackage s_Negotiate;
		private static SecurityPackage s_Digest;
		private static SecurityPackage s_Schannel;

		// Package properties
		private int _capabilities;
		private int _version;
		private int _RPCID;
		private int _maxToken;
		private string _name;
		private string _comment;

		#region Constructors

		public SecurityPackage(string name)
		{
			// parameters validation
			if (name == null)
				throw new ArgumentNullException("name");

			// query package information
			IntPtr pkgInfo;
			int error = SSPINative.QuerySecurityPackageInfo(name, out pkgInfo);
			if (error < 0)
				throw new SSPIException(error, "Could not query security package information");

			try
			{
				// initialize package object
				Init(pkgInfo);
			}
			finally
			{
				SSPINative.FreeContextBuffer(pkgInfo);
			}
		}

		private SecurityPackage(IntPtr pkgInfo)
		{
			// parameters validation
			if (pkgInfo == IntPtr.Zero)
				throw new ArgumentNullException("pkgInfo");

			// initialize package object
			Init(pkgInfo);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Standard NTLM security package
		/// </summary>
		public static SecurityPackage NTLM
		{
			get
			{
				if (s_NTLM == null)
				{
					s_NTLM = new SecurityPackage("NTLM");
				}

				return s_NTLM;
			}
		}

		/// <summary>
		/// Standard Kerberos security package
		/// </summary>
		public static SecurityPackage Kerberos
		{
			get
			{
				if (s_Kerberos == null)
				{
					s_Kerberos = new SecurityPackage("Kerberos");
				}

				return s_Kerberos;
			}
		}

		/// <summary>
		/// Standard Negotiate security package
		/// </summary>
		public static SecurityPackage Negotiate
		{
			get
			{
				if (s_Negotiate == null)
				{
					s_Negotiate = new SecurityPackage("Negotiate");
				}

				return s_Negotiate;
			}
		}

		/// <summary>
		/// Standard Digest security package
		/// </summary>
		public static SecurityPackage Digest
		{
			get
			{
				if (s_Digest == null)
				{
					s_Digest = new SecurityPackage("Digest");
				}

				return s_Digest;
			}
		}

		/// <summary>
		/// Standard Schannel security package
		/// </summary>
		public static SecurityPackage Schannel
		{
			get
			{
				if (s_Schannel == null)
				{
					s_Schannel = new SecurityPackage("Schannel");
				}

				return s_Schannel;
			}
		}

		/// <summary>
		/// Registered SSPI packages
		/// </summary>
		public static SecurityPackage[] RegisteredPackages
		{
			get
			{
				if (s_Packages == null)
				{
					// enumerate packages
					uint count;
					IntPtr arrPackages;
					int error = SSPINative.EnumerateSecurityPackages(out count, out arrPackages);
					if (error < 0)
						throw new SSPIException(error, "Could not enumerate security packages");

					try
					{
						// create packages list
						SecurityPackage[] packages = new SecurityPackage[count];

						IntPtr ptr = arrPackages;
						for (uint i = 0; i < count; ++i)
						{
							packages[i] = new SecurityPackage(ptr);
							ptr = IntPtrHelper.Add(ptr, SSPINative.SecPkgInfo.Size);
						}

						s_Packages = packages;
					}
					finally
					{
						SSPINative.FreeContextBuffer(arrPackages);
					}
				}

				return s_Packages;
			}
		}

		/// <summary>
		/// Package capabilities
		/// </summary>
		public int Capabilities
		{
			get
			{
				return _capabilities;
			}
		}

		/// <summary>
		/// Package version
		/// </summary>
		public int Version
		{
			get
			{
				return _version;
			}
		}

		/// <summary>
		/// Package RPC unique identifier
		/// </summary>
		public int RPCID
		{
			get
			{
				return _RPCID;
			}
		}

		/// <summary>
		/// Maximum token size in bytes
		/// </summary>
		public int MaxToken
		{
			get
			{
				return _maxToken;
			}
		}

		/// <summary>
		/// Package name
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// Package description
		/// </summary>
		public string Comment
		{
			get
			{
				return _comment;
			}
		}

		#endregion

		private void Init(IntPtr pkgInfo)
		{
			_capabilities = MarshalHelper.ReadInt32(pkgInfo, typeof(SSPINative.SecPkgInfo), "Capabilities");
			_version = MarshalHelper.ReadInt16(pkgInfo, typeof(SSPINative.SecPkgInfo), "Version");
			_RPCID = MarshalHelper.ReadInt16(pkgInfo, typeof(SSPINative.SecPkgInfo), "RPCID");
			_maxToken = MarshalHelper.ReadInt32(pkgInfo, typeof(SSPINative.SecPkgInfo), "MaxToken");
			_name = MarshalHelper.ReadString(pkgInfo, typeof(SSPINative.SecPkgInfo), "Name");
			_comment = MarshalHelper.ReadString(pkgInfo, typeof(SSPINative.SecPkgInfo), "Comment");
		}
	}
}