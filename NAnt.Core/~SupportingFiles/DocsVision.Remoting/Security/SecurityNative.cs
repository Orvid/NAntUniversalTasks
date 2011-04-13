//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Security;
using System.Runtime.InteropServices;

namespace DocsVision.Security
{
	[SuppressUnmanagedCodeSecurityAttribute()]
	internal sealed class SecurityNative
	{
		private SecurityNative()
		{
			// this class is non creatable
		}

		#region Constants

		//
		//  Revisions
		//

		public const uint SECURITY_DESCRIPTOR_REVISION    = 1;
		public const uint SDDL_REVISION                   = 1;
		public const uint ACL_REVISION                    = 2;

		//
		//  Sid Identifier Authority
		//

		public static readonly byte[] NullSidAuthority    = new byte[] {0,0,0,0,0,0};
		public static readonly byte[] WorldSidAuthority   = new byte[] {0,0,0,0,0,1};
		public static readonly byte[] LocalSidAuthority   = new byte[] {0,0,0,0,0,2};
		public static readonly byte[] CreatorSidAuthority = new byte[] {0,0,0,0,0,3};
		public static readonly byte[] NonUniqueAuthority  = new byte[] {0,0,0,0,0,4};
		public static readonly byte[] NTAuthority         = new byte[] {0,0,0,0,0,5};
		public static readonly byte[] ResManagerAuthority = new byte[] {0,0,0,0,0,9};

		//
		//  Universal well-known Sids
		//

		public const uint SECURITY_NULL_RID                 = 0x00000000;
		public const uint SECURITY_WORLD_RID                = 0x00000000;
		public const uint SECURITY_LOCAL_RID                = 0x00000000;

		public const uint SECURITY_CREATOR_OWNER_RID        = 0x00000000;
		public const uint SECURITY_CREATOR_GROUP_RID        = 0x00000001;

		public const uint SECURITY_CREATOR_OWNER_SERVER_RID = 0x00000002;
		public const uint SECURITY_CREATOR_GROUP_SERVER_RID = 0x00000003;

		//
		//  NT Authority well-known Sids
		//

		public const uint SECURITY_DIALUP_RID               = 0x00000001;
		public const uint SECURITY_NETWORK_RID              = 0x00000002;
		public const uint SECURITY_BATCH_RID                = 0x00000003;
		public const uint SECURITY_INTERACTIVE_RID          = 0x00000004;

		public const uint SECURITY_SERVICE_RID              = 0x00000006;
		public const uint SECURITY_ANONYMOUS_LOGON_RID      = 0x00000007;
		public const uint SECURITY_PROXY_RID                = 0x00000008;
		public const uint SECURITY_SERVER_LOGON_RID         = 0x00000009;
		public const uint SECURITY_PRINCIPAL_SELF_RID       = 0x0000000A;
		public const uint SECURITY_AUTHENTICATED_USER_RID   = 0x0000000B;
		public const uint SECURITY_RESTRICTED_CODE_RID      = 0x0000000C;
		public const uint SECURITY_TERMINAL_SERVER_RID      = 0x0000000D;
		public const uint SECURITY_REMOTE_LOGON_RID         = 0x0000000E;

		public const uint SECURITY_LOCAL_SYSTEM_RID         = 0x00000012;
		public const uint SECURITY_LOCAL_SERVICE_RID        = 0x00000013;
		public const uint SECURITY_NETWORK_SERVICE_RID      = 0x00000014;

		public const uint SECURITY_BUILTIN_DOMAIN_RID       = 0x00000020;

		//
		//  NT Authority well-known Sids
		//

		public const uint DOMAIN_ALIAS_RID_ADMINS           = 0x00000220;
		public const uint DOMAIN_ALIAS_RID_USERS            = 0x00000221;
		public const uint DOMAIN_ALIAS_RID_GUESTS           = 0x00000222;
		public const uint DOMAIN_ALIAS_RID_POWER_USERS      = 0x00000223;
		public const uint DOMAIN_ALIAS_RID_ACCOUNT_OPS      = 0x00000224;
		public const uint DOMAIN_ALIAS_RID_SYSTEM_OPS       = 0x00000225;
		public const uint DOMAIN_ALIAS_RID_PRINT_OPS        = 0x00000226;
		public const uint DOMAIN_ALIAS_RID_BACKUP_OPS       = 0x00000227;
		public const uint DOMAIN_ALIAS_RID_REPLICATOR       = 0x00000228;
		public const uint DOMAIN_ALIAS_RID_RAS_SERVERS      = 0x00000229;
		public const uint DOMAIN_ALIAS_RID_PREW2KCOMPACCESS = 0x0000022A;
		public const uint DOMAIN_ALIAS_RID_REMOTE_DESKTOP_USERS = 0x0000022B;

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecurityDescriptor
		{
			// Security descriptor revision
			public byte Revision = (byte)SECURITY_DESCRIPTOR_REVISION;
			// Padding
			public byte Sbz1 = 0;
			// Control flags
			public short Control = 0;
			// Security descriptor owner
			public IntPtr Owner = IntPtr.Zero;
			// Security descriptor group
			public IntPtr Group = IntPtr.Zero;
			// Security descriptor system access control list
			public IntPtr Sacl = IntPtr.Zero;
			// Security descriptor access control list
			public IntPtr Dacl = IntPtr.Zero;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecurityDescriptor));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class ACL
		{
			// ACL revision
			public byte Revision = (byte)ACL_REVISION;
			// Padding
			public byte Sbz1 = 0;
			// ACL size
			public short AclSize = 0;
			// ACE count
			public short AceCount = 0;
			// Padding
			public short Sbz2 = 0;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(ACL));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class ACE
		{
			// ACE type
			public byte AceType = 0;
			// ACE flags
			public byte AceFlags = 0;
			// ACE size
			public ushort AceSize = 0;
			// ACE access mask
			public int AccessMask = 0;
			// ACE trustee
			public int SidStart = 0;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(ACE));
		}

		#endregion

		#region AdvApi32 imports

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool InitializeSecurityDescriptor(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[In]      uint dwRevision                  // revision level
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern bool IsValidSecurityDescriptor(
			[In]      IntPtr pSecurityDescriptor       // security descriptor
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int GetSecurityInfo(
			[In]      IntPtr ObjectHandle,             // handle of the object
			[In]      uint ObjectType,                 // object type
			[In]      uint SecurityInfo,               // type of security information to get
			[In]      IntPtr ppSidOwner,               // owner SID
			[In]      IntPtr ppSidGroup,               // primary group SID
			[In]      IntPtr ppDacl,                   // DACL
			[In]      IntPtr ppSacl,                   // SACL
			[Out]     out IntPtr pSecurityDescriptor   // security descriptor
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int SetSecurityInfo(
			[In]      IntPtr ObjectHandle,             // handle of the object
			[In]      uint ObjectType,                 // object type
			[In]      uint SecurityInfo,               // type of security information to set
			[In]      IntPtr pSidOwner,                // owner SID
			[In]      IntPtr pSidGroup,                // primary group SID
			[In]      IntPtr pDacl,                    // DACL
			[In]      IntPtr pSacl                     // SACL
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int GetNamedSecurityInfo(
			[In]      string ObjectName,               // name of the object
			[In]      uint ObjectType,                 // object type
			[In]      uint SecurityInfo,               // type of security information to get
			[In]      IntPtr ppSidOwner,               // owner SID
			[In]      IntPtr ppSidGroup,               // primary group SID
			[In]      IntPtr ppDacl,                   // DACL
			[In]      IntPtr ppSacl,                   // SACL
			[Out]     out IntPtr pSecurityDescriptor   // security descriptor
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int SetNamedSecurityInfo(
			[In]      string ObjectName,               // name of the object
			[In]      uint ObjectType,                 // object type
			[In]      uint SecurityInfo,               // type of security information to get
			[In]      IntPtr ppSidOwner,               // owner SID
			[In]      IntPtr ppSidGroup,               // primary group SID
			[In]      IntPtr ppDacl,                   // DACL
			[In]      IntPtr ppSacl                    // SACL
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool GetSecurityDescriptorControl(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[Out]     out uint pControl,               // security descriptor control
			[Out]     out uint pRevision               // security descriptor revision
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool SetSecurityDescriptorControl(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[In]      uint ControlMask,                // security descriptor control mask
			[In]      uint ControlBits                 // security descriptor control
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool GetSecurityDescriptorOwner(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[Out]     out IntPtr pOwnerSid,            // security descriptor owner
			[Out]     out bool bOwnerDefaulted         // default owner flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool SetSecurityDescriptorOwner(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[In]      IntPtr pOwnerSid,                // security descriptor owner
			[In]      bool bOwnerDefaulted             // default owner flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool GetSecurityDescriptorGroup(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[Out]     out IntPtr pGroupSid,            // security descriptor group
			[Out]     out bool bGroupDefaulted         // default group flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool SetSecurityDescriptorGroup(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[In]      IntPtr pGroupSid,                // security descriptor group
			[In]      bool bGroupDefaulted             // default group flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool GetSecurityDescriptorDacl(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[Out]     out bool bDaclPresent,           // DACL presence flag
			[Out]     out IntPtr pDacl,                // DACL
			[Out]     out bool bDaclDefaulted          // default DACL flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool SetSecurityDescriptorDacl(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[In]      bool bDaclPresent,               // DACL present flag
			[In]      IntPtr pDacl,                    // DACL
			[In]      bool bDaclDefaulted              // default DACL flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool GetSecurityDescriptorSacl(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[Out]     out bool bSaclPresent,           // SACL presence flag
			[Out]     out IntPtr pSacl,                // SACL
			[Out]     out bool bSaclDefaulted          // default SACL flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool SetSecurityDescriptorSacl(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[In]      bool bSaclPresent,               // SACL present flag
			[In]      IntPtr pSacl,                    // SACL
			[In]      bool bSaclDefaulted              // default SACL flag
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool ConvertSecurityDescriptorToStringSecurityDescriptor(
			[In]      IntPtr pSecurityDescriptor,      // security descriptor
			[In]      uint dwRevision,                 // revision level
			[In]      uint SecurityInfo,               // type of security information
			[Out]     out IntPtr pStringSd,            // string representation
			[Out]     out int stringLen                // string length
		);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool InitializeSid(
			[In]      IntPtr pSid,                     // security identifier
			[In]      IntPtr pIdentifierAuthority,     // identifier authority
			[In]      byte nSubAuthorityCount          // number of subauthorities
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern bool IsValidSid(
			[In]      IntPtr pSid                      // security identifier
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool CopySid(
			[In]      uint nDestinationSidLength,      // buffer size
			[In]      IntPtr pSid1,                    // destination security identifier
			[In]      IntPtr pSid2                     // source security identifier
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern int GetSidLengthRequired(
			[In]      byte nSubAtuhorityCount          // number of subauthorities
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern int GetLengthSid(
			[In]      IntPtr pSid                      // security identifier
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern byte GetSidSubAuthorityCount(
			[In]      IntPtr pSid                      // security identifier
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern IntPtr GetSidSubAuthority(
			[In]      IntPtr pSid,                     // security identifier
			[In]      uint nSubAuthority               // subauthority index
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern bool EqualSid(
			[In]      IntPtr pSid1,                    // security identifier #1
			[In]      IntPtr pSid2                     // security identifier #2
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern bool EqualPrefixSid(
			[In]      IntPtr pSid1,                    // security identifier #1
			[In]      IntPtr pSid2                     // security identifier #2
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool LookupAccountName(
			[In]      string SystemName,               // target computer
			[In]      string AccountName,              // account name
			[In]      IntPtr pAccountSid,              // buffer for security identifier
			[In, Out] ref uint cbAccountSid,           // buffer size
			[In]      IntPtr pDomainName,              // buffer for domain name
			[In, Out] ref uint cbDomainName,           // buffer size
			[Out]     out int peUse                    // type of the account
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool LookupAccountSid(
			[In]      string SystemName,               // target computer
			[In]      IntPtr pAccountSid,              // security identifier
			[In]      IntPtr pAccountName,             // buffer for account name
			[In, Out] ref uint cbAccountName,          // buffer size
			[In]      IntPtr pDomainName,              // buffer for domain name
			[In, Out] ref uint cbDomainName,           // buffer size
			[Out]     out int peUse                    // type of the account
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool ConvertSidToStringSid(
			[In]      IntPtr pSid,                     // security identifier
			[Out]     out IntPtr pStringSid            // string representation
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool ConvertStringSidToSid(
			[In]      string StringSid,                // string representation
			[Out]     out IntPtr pSid                  // security identifier
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool InitializeAcl(
			[In]      IntPtr pAcl,                     // access control list
			[In]      uint nAclLength,                 // ACL size in bytes
			[In]      uint dwAclRevision               // ACL revision number
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool AddAce(
			[In]      IntPtr pAcl,                     // access control list
			[In]      uint dwAclRevision,              // ACL revision number
			[In]      uint dwStartingAceIndex,         // ACE start index
			[In]      IntPtr pAceList,                 // ACEs list
			[In]      uint nAceListLength              // ACEs list size in bytes
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool GetAce(
			[In]      IntPtr pAcl,                     // access control list
			[In]      uint dwAceIndex,                 // ACE index
			[Out]     out IntPtr pAce                  // ACE
			);

		#endregion
	}
}