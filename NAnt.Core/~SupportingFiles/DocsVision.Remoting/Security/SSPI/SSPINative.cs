//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Security;
using System.Runtime.InteropServices;

namespace DocsVision.Security.SSPI
{
	[SuppressUnmanagedCodeSecurityAttribute()]
	internal sealed class SSPINative
	{
		private SSPINative()
		{
			// this class is non creatable
		}

		#region Constants

		//
		//  Security Package Capabilities
		//

		public const uint SECPKG_FLAG_INTEGRITY              = 0x00000001;
		public const uint SECPKG_FLAG_PRIVACY                = 0x00000002;
		public const uint SECPKG_FLAG_TOKEN_ONLY             = 0x00000004;
		public const uint SECPKG_FLAG_DATAGRAM               = 0x00000008;
		public const uint SECPKG_FLAG_CONNECTION             = 0x00000010;
		public const uint SECPKG_FLAG_MULTI_REQUIRED         = 0x00000020;
		public const uint SECPKG_FLAG_CLIENT_ONLY            = 0x00000040;
		public const uint SECPKG_FLAG_EXTENDED_ERROR         = 0x00000080;
		public const uint SECPKG_FLAG_IMPERSONATION   	     = 0x00000100;
		public const uint SECPKG_FLAG_ACCEPT_WIN32_NAME      = 0x00000200;
		public const uint SECPKG_FLAG_STREAM          	     = 0x00000400;
		public const uint SECPKG_FLAG_NEGOTIABLE      	     = 0x00000800;
		public const uint SECPKG_FLAG_GSS_COMPATIBLE  	     = 0x00001000;
		public const uint SECPKG_FLAG_LOGON           	     = 0x00002000;
		public const uint SECPKG_FLAG_ASCII_BUFFERS   	     = 0x00004000;
		public const uint SECPKG_FLAG_FRAGMENT        	     = 0x00008000;
		public const uint SECPKG_FLAG_MUTUAL_AUTH     	     = 0x00010000;
		public const uint SECPKG_FLAG_DELEGATION      	     = 0x00020000;
		public const uint SECPKG_FLAG_READONLY_WITH_CHECKSUM = 0x00040000;

		//
		//  Security credentials Use Flags
		//
		public const uint SECPKG_CRED_INBOUND             = 0x00000001;
		public const uint SECPKG_CRED_OUTBOUND            = 0x00000002;
		public const uint SECPKG_CRED_BOTH                = 0x00000003;
		public const uint SECPKG_CRED_DEFAULT             = 0x00000004;
		public const uint SECPKG_CRED_RESERVED            = 0xF0000000;

		//
		//  Security credentials Attributes
		//

		public const uint SECPKG_CRED_ATTR_NAMES          = 1;
		public const uint SECPKG_CRED_ATTR_SSI_PROVIDER   = 2;

		//
		//  Security Context Attributes
		//

		public const uint SECPKG_ATTR_SIZES               = 0;
		public const uint SECPKG_ATTR_NAMES               = 1;
		public const uint SECPKG_ATTR_LIFESPAN            = 2;
		public const uint SECPKG_ATTR_DCE_INFO            = 3;
		public const uint SECPKG_ATTR_STREAM_SIZES        = 4;
		public const uint SECPKG_ATTR_KEY_INFO            = 5;
		public const uint SECPKG_ATTR_AUTHORITY           = 6;
		public const uint SECPKG_ATTR_PROTO_INFO          = 7;
		public const uint SECPKG_ATTR_PASSWORD_EXPIRY     = 8;
		public const uint SECPKG_ATTR_SESSION_KEY         = 9;
		public const uint SECPKG_ATTR_PACKAGE_INFO        = 10;
		public const uint SECPKG_ATTR_USER_FLAGS          = 11;
		public const uint SECPKG_ATTR_NEGOTIATION_INFO    = 12;
		public const uint SECPKG_ATTR_NATIVE_NAMES        = 13;
		public const uint SECPKG_ATTR_FLAGS               = 14;
		public const uint SECPKG_ATTR_USE_VALIDATED       = 15;
		public const uint SECPKG_ATTR_credentials_NAME     = 16;
		public const uint SECPKG_ATTR_TARGET_INFORMATION  = 17;
		public const uint SECPKG_ATTR_ACCESS_TOKEN        = 18;
		public const uint SECPKG_ATTR_TARGET              = 19;
		public const uint SECPKG_ATTR_AUTHENTICATION_ID   = 20;

		//
		//  InitializeSecurityContext Requirement and return flags
		//

		public const uint ISC_REQ_DELEGATE                = 0x00000001;
		public const uint ISC_REQ_MUTUAL_AUTH             = 0x00000002;
		public const uint ISC_REQ_REPLAY_DETECT           = 0x00000004;
		public const uint ISC_REQ_SEQUENCE_DETECT         = 0x00000008;
		public const uint ISC_REQ_CONFIDENTIALITY         = 0x00000010;
		public const uint ISC_REQ_USE_SESSION_KEY         = 0x00000020;
		public const uint ISC_REQ_PROMPT_FOR_CREDS        = 0x00000040;
		public const uint ISC_REQ_USE_SUPPLIED_CREDS      = 0x00000080;
		public const uint ISC_REQ_ALLOCATE_MEMORY         = 0x00000100;
		public const uint ISC_REQ_USE_DCE_STYLE           = 0x00000200;
		public const uint ISC_REQ_DATAGRAM                = 0x00000400;
		public const uint ISC_REQ_CONNECTION              = 0x00000800;
		public const uint ISC_REQ_CALL_LEVEL              = 0x00001000;
		public const uint ISC_REQ_FRAGMENT_SUPPLIED       = 0x00002000;
		public const uint ISC_REQ_EXTENDED_ERROR          = 0x00004000;
		public const uint ISC_REQ_STREAM                  = 0x00008000;
		public const uint ISC_REQ_INTEGRITY               = 0x00010000;
		public const uint ISC_REQ_IDENTIFY                = 0x00020000;
		public const uint ISC_REQ_NULL_SESSION            = 0x00040000;
		public const uint ISC_REQ_MANUAL_CRED_VALIDATION  = 0x00080000;
		public const uint ISC_REQ_RESERVED1               = 0x00100000;
		public const uint ISC_REQ_FRAGMENT_TO_FIT         = 0x00200000;

		public const uint ISC_RET_DELEGATE                = 0x00000001;
		public const uint ISC_RET_MUTUAL_AUTH             = 0x00000002;
		public const uint ISC_RET_REPLAY_DETECT           = 0x00000004;
		public const uint ISC_RET_SEQUENCE_DETECT         = 0x00000008;
		public const uint ISC_RET_CONFIDENTIALITY         = 0x00000010;
		public const uint ISC_RET_USE_SESSION_KEY         = 0x00000020;
		public const uint ISC_RET_USED_COLLECTED_CREDS    = 0x00000040;
		public const uint ISC_RET_USED_SUPPLIED_CREDS     = 0x00000080;
		public const uint ISC_RET_ALLOCATED_MEMORY        = 0x00000100;
		public const uint ISC_RET_USED_DCE_STYLE          = 0x00000200;
		public const uint ISC_RET_DATAGRAM                = 0x00000400;
		public const uint ISC_RET_CONNECTION              = 0x00000800;
		public const uint ISC_RET_INTERMEDIATE_RETURN     = 0x00001000;
		public const uint ISC_RET_CALL_LEVEL              = 0x00002000;
		public const uint ISC_RET_EXTENDED_ERROR          = 0x00004000;
		public const uint ISC_RET_STREAM                  = 0x00008000;
		public const uint ISC_RET_INTEGRITY               = 0x00010000;
		public const uint ISC_RET_IDENTIFY                = 0x00020000;
		public const uint ISC_RET_NULL_SESSION            = 0x00040000;
		public const uint ISC_RET_MANUAL_CRED_VALIDATION  = 0x00080000;
		public const uint ISC_RET_RESERVED1               = 0x00100000;
		public const uint ISC_RET_FRAGMENT_ONLY           = 0x00200000;

		//
		//  AcceptSecurityContext Requirement and return flags
		//

		public const uint ASC_REQ_DELEGATE                = 0x00000001;
		public const uint ASC_REQ_MUTUAL_AUTH             = 0x00000002;
		public const uint ASC_REQ_REPLAY_DETECT           = 0x00000004;
		public const uint ASC_REQ_SEQUENCE_DETECT         = 0x00000008;
		public const uint ASC_REQ_CONFIDENTIALITY         = 0x00000010;
		public const uint ASC_REQ_USE_SESSION_KEY         = 0x00000020;
		public const uint ASC_REQ_ALLOCATE_MEMORY         = 0x00000100;
		public const uint ASC_REQ_USE_DCE_STYLE           = 0x00000200;
		public const uint ASC_REQ_DATAGRAM                = 0x00000400;
		public const uint ASC_REQ_CONNECTION              = 0x00000800;
		public const uint ASC_REQ_CALL_LEVEL              = 0x00001000;
		public const uint ASC_REQ_EXTENDED_ERROR          = 0x00008000;
		public const uint ASC_REQ_STREAM                  = 0x00010000;
		public const uint ASC_REQ_INTEGRITY               = 0x00020000;
		public const uint ASC_REQ_LICENSING               = 0x00040000;
		public const uint ASC_REQ_IDENTIFY                = 0x00080000;
		public const uint ASC_REQ_ALLOW_NULL_SESSION      = 0x00100000;
		public const uint ASC_REQ_ALLOW_NON_USER_LOGONS   = 0x00200000;
		public const uint ASC_REQ_ALLOW_CONTEXT_REPLAY    = 0x00400000;
		public const uint ASC_REQ_FRAGMENT_TO_FIT         = 0x00800000;
		public const uint ASC_REQ_FRAGMENT_SUPPLIED       = 0x00002000;
		public const uint ASC_REQ_NO_TOKEN                = 0x01000000;

		public const uint ASC_RET_DELEGATE                = 0x00000001;
		public const uint ASC_RET_MUTUAL_AUTH             = 0x00000002;
		public const uint ASC_RET_REPLAY_DETECT           = 0x00000004;
		public const uint ASC_RET_SEQUENCE_DETECT         = 0x00000008;
		public const uint ASC_RET_CONFIDENTIALITY         = 0x00000010;
		public const uint ASC_RET_USE_SESSION_KEY         = 0x00000020;
		public const uint ASC_RET_ALLOCATED_MEMORY        = 0x00000100;
		public const uint ASC_RET_USED_DCE_STYLE          = 0x00000200;
		public const uint ASC_RET_DATAGRAM                = 0x00000400;
		public const uint ASC_RET_CONNECTION              = 0x00000800;
		public const uint ASC_RET_CALL_LEVEL              = 0x00002000;
		public const uint ASC_RET_THIRD_LEG_FAILED        = 0x00004000;
		public const uint ASC_RET_EXTENDED_ERROR          = 0x00008000;
		public const uint ASC_RET_STREAM                  = 0x00010000;
		public const uint ASC_RET_INTEGRITY               = 0x00020000;
		public const uint ASC_RET_LICENSING               = 0x00040000;
		public const uint ASC_RET_IDENTIFY                = 0x00080000;
		public const uint ASC_RET_NULL_SESSION            = 0x00100000;
		public const uint ASC_RET_ALLOW_NON_USER_LOGONS   = 0x00200000;
		public const uint ASC_RET_ALLOW_CONTEXT_REPLAY    = 0x00400000;
		public const uint ASC_RET_FRAGMENT_ONLY           = 0x00800000;
		public const uint ASC_RET_NO_TOKEN                = 0x01000000;

		//
		// SecWinNTAuthIdentity Flags
		//

		public const int SEC_WINNT_AUTH_IDENTITY_ANSI     = 0x1;
		public const int SEC_WINNT_AUTH_IDENTITY_UNICODE  = 0x2;

		//
		//  Data Representation Constant
		//
		public const uint SECURITY_NETWORK_DREP           = 0x00000000;
		public const uint SECURITY_NATIVE_DREP            = 0x00000010;

		//
		// Security buffer type
		//

		public const uint SECBUFFER_EMPTY                    = 0;
		public const uint SECBUFFER_DATA                     = 1;
		public const uint SECBUFFER_TOKEN                    = 2;
		public const uint SECBUFFER_PKG_PARAMS               = 3;
		public const uint SECBUFFER_MISSING                  = 4;
		public const uint SECBUFFER_EXTRA                    = 5;
		public const uint SECBUFFER_STREAM_TRAILER           = 6;
		public const uint SECBUFFER_STREAM_HEADER            = 7;
		public const uint SECBUFFER_NEGOTIATION_INFO         = 8;
		public const uint SECBUFFER_PADDING                  = 9;
		public const uint SECBUFFER_STREAM                   = 10;
		public const uint SECBUFFER_MECHLIST                 = 11;
		public const uint SECBUFFER_MECHLIST_SIGNATURE       = 12;
		public const uint SECBUFFER_TARGET                   = 13;
		public const uint SECBUFFER_CHANNEL_BINDINGS         = 14;

		public const uint SECBUFFER_ATTRMASK          	     = 0xF0000000;
		public const uint SECBUFFER_READONLY          	     = 0x80000000;
		public const uint SECBUFFER_READONLY_WITH_CHECKSUM   = 0x10000000;
		public const uint SECBUFFER_RESERVED          	     = 0x60000000;

		#endregion

		#region Error codes

		public const int SEC_E_INVALID_HANDLE        = unchecked((int)0x80090301);
		public const int SEC_E_INTERNAL_ERROR        = unchecked((int)0x80090304);
		public const int SEC_E_SECPKG_NOT_FOUND      = unchecked((int)0x80090305);
		public const int SEC_E_INVALID_TOKEN         = unchecked((int)0x80090308);
		public const int SEC_E_NO_IMPERSONATION      = unchecked((int)0x8009030B);
		public const int SEC_E_LOGON_DENIED          = unchecked((int)0x8009030C);
		public const int SEC_E_UNKNOWN_CREDENTIALS   = unchecked((int)0x8009030D);
		public const int SEC_E_NO_CREDENTIALS        = unchecked((int)0x8009030E);
		public const int SEC_E_MESSAGE_ALTERED       = unchecked((int)0x8009030F);
		public const int SEC_E_OUT_OF_SEQUENCE       = unchecked((int)0x80090310);
		public const int SEC_E_BAD_PKGID             = unchecked((int)0x80090316);

		public const int SEC_I_CONTINUE_NEEDED       = unchecked((int)0x00090312);
		public const int SEC_I_COMPLETE_NEEDED       = unchecked((int)0x00090313);
		public const int SEC_I_COMPLETE_AND_CONTINUE = unchecked((int)0x00090314);

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecWinNTAuthIdentity
		{
			// String that contains the user name
			public string User = null;
			// The length, in characters, of the user string
			public int UserLength = 0;
			// String that contains the domain name or the workgroup name
			public string Domain = null;
			// The length, in characters, of the domain string
			public int DomainLength = 0;
			// String that contains the password of the user in the domain or workgroup
			public string Password= null;
			// The length, in characters, of the password string
			public int PasswordLength = 0;
			// Flags
			public int Flags = SEC_WINNT_AUTH_IDENTITY_UNICODE;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecWinNTAuthIdentity));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecBuffer
		{
			// Buffer size in bytes
			public int BufferSize = 0;
			// Buffer type
			public int BufferType = 0;
			// Pointer to a buffer
			public IntPtr pvBuffer = IntPtr.Zero;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecBuffer));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecBufferDesc
		{
			// Version number of this structure
			public int Version = 0;
			// Number of buffers
			public int BuffersCount = 0;
			// Pointer to an array of SecBuffer structures
			public IntPtr pvBuffers = IntPtr.Zero;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecBufferDesc));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecPkgInfo
		{
			// Capabilities of the security package
			public int Capabilities = 0;
			// Version of the package protocol
			public short Version = 1;
			// DCE RPC identifier
			public short RPCID = -1;
			// Maximum token size in bytes
			public int MaxToken = 0;
			// Package name
			public string Name = null;
			// Package description
			public string Comment = null;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecPkgInfo));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecPkgCredentials_Names
		{
			// Name of the user represented by the credentials
			public IntPtr UserName = IntPtr.Zero;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecPkgCredentials_Names));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecPkgContext_Sizes
		{
			// Specifies the maximum size of the security token
			public int MaxToken = 0;
			// Specifies the maximum size of the signature
			public int MaxSignature = 0;
			// Specifies the preferred integral size of the messages
			public int BlockSize = 0;
			// Size of the security trailer to be appended to messages
			public int SecurityTrailer = 0;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecPkgContext_Sizes));
		}

		#endregion

		#region Security imports

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int EnumerateSecurityPackages(
			[Out]     out uint pcPackages,             // number of packages
			[Out]     out IntPtr ppPackageInfo         // security packages information
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int QuerySecurityPackageInfo(
			[In]      string pszPackageName,           // package name
			[Out]     out IntPtr ppPackageInfo         // security package information
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int FreeContextBuffer(
			[In]      IntPtr pvContextBuffer           // pointer to memory allocated by the security package
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int AcquireCredentialsHandle(
			[In]      string pszPrincipal,             // name of the principal
			[In]      string pszPackage,               // name of the security package
			[In]      uint fCredentialsUse,            // crdentials usage
			[In]      IntPtr pvLogonID,                // LUID that identifies the user
			[In]      IntPtr pvAuthData,               // package-specific data
			[In]      IntPtr pGetKeyFn,                // function that retrieves the key for the function
			[In]      IntPtr pvGetKeyArgument,         // function parameters
			[Out]     out Int64 phCredentials,         // credentials handle
			[Out]     out Int64 ptsExpiry              // expiration time
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int AcquireCredentialsHandle(
			[In]      string pszPrincipal,             // name of the principal
			[In]      string pszPackage,               // name of the security package
			[In]      uint fCredentialsUse,            // crdentials usage
			[In]      IntPtr pvLogonID,                // LUID that identifies the user
			[In]      SecWinNTAuthIdentity pAuthData,  // package-specific data
			[In]      IntPtr pGetKeyFn,                // function that retrieves the key for the function
			[In]      IntPtr pvGetKeyArgument,         // function parameters
			[Out]     out Int64 phCredentials,         // credentials handle
			[Out]     out Int64 ptsExpiry              // expiration time
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int QueryCredentialsAttributes(
			[In]      ref Int64 phCredentials,         // credentials handle
			[In]      uint ulAttribute,                // attribute of the credentials to be returned
			[In]      IntPtr pBuffer                   // pointer to a structure that receives the attributes
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int FreeCredentialsHandle(
			[In]      ref Int64 phCredentials          // credentials handle
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int InitializeSecurityContext(
			[In]      ref Int64 phCredentials,         // credentials handle
			[In]      IntPtr phContext,                // security context handle
			[In]      string pszTargetName,            // target of the context
			[In]      uint fContextReq,                // requirements of the context
			[In]      uint Reserved1,                  // not used
			[In]      uint TargetDataRep,              // data representation
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint Reserved2,                  // not used
			[Out]     out Int64 phNewContext,          // created security context
			[In]      SecBufferDesc pOutput,           // output buffers
			[Out]     out uint pfContextAttr,          // created context attributes
			[Out]     out Int64 ptsExpiry              // expiration time
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int InitializeSecurityContext(
			[In]      ref Int64 phCredentials,         // credentials handle
			[In]      ref Int64 phContext,             // security context handle
			[In]      string pszTargetName,            // target of the context
			[In]      uint fContextReq,                // requirements of the context
			[In]      uint Reserved1,                  // not used
			[In]      uint TargetDataRep,              // data representation
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint Reserved2,                  // not used
			[In]      IntPtr phNewContext,             // created security context
			[In]      SecBufferDesc pOutput,           // output buffers
			[Out]     out uint pfContextAttr,          // created context attributes
			[In]      IntPtr ptsExpiry                 // expiration time
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int AcceptSecurityContext(
			[In]      ref Int64 phCredentials,         // credentials handle
			[In]      IntPtr phContext,                // security context handle
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint fContextReq,                // requirements of the context
			[In]      uint TargetDataRep,              // data representation
			[Out]     out Int64 phNewContext,          // created security context
			[In]      SecBufferDesc pOutput,           // output buffers
			[Out]     out uint pfContextAttr,          // created context attributes
			[Out]     out Int64 ptsExpiry              // expiration time
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int AcceptSecurityContext(
			[In]      ref Int64 phCredentials,         // credentials handle
			[In]      ref Int64 phContext,             // security context handle
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint fContextReq,                // requirements of the context
			[In]      uint TargetDataRep,              // data representation
			[In]      IntPtr phNewContext,             // created security context
			[In]      SecBufferDesc pOutput,           // output buffers
			[Out]     out uint pfContextAttr,          // created context attributes
			[In]      IntPtr ptsExpiry                 // expiration time
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int CompleteAuthToken(
			[In]      ref Int64 phContext,             // security context handle
			[In]      SecBufferDesc pInput             // input buffers
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int QuerySecurityContextToken(
			[In]      ref Int64 phContext,             // security context handle
			[Out]     out IntPtr hToken                // context token
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int QueryContextAttributes(
			[In]      ref Int64 phContext,             // security context handle
			[In]      uint ulAttribute,                // attribute of the context to be returned
			[In]      IntPtr pBuffer                   // pointer to a structure that receives the attributes
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int ImpersonateSecurityContext(
			[In]      ref Int64 phContext              // security context handle
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int RevertSecurityContext(
			[In]      ref Int64 phContext              // security context handle
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int DeleteSecurityContext(
			[In]      ref Int64 phContext              // security context handle
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int EncryptMessage(
			[In]      ref Int64 phContext,             // security context handle
			[In]      uint qualityOfProtection,        // package-specific flags
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint sequenceNumber              // sequence number
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int DecryptMessage(
			[In]      ref Int64 phContext,             // security context handle
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint sequenceNumber,             // sequence number
			[In]      uint qualityOfProtection         // package-specific flags
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int MakeSignature(
			[In]      ref Int64 phContext,             // security context handle
			[In]      uint qualityOfProtection,        // package-specific QOP flags
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint sequenceNumber              // sequence number
			);

		[DllImport("security.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern int VerifySignature(
			[In]      ref Int64 phContext,             // security context handle
			[In]      SecBufferDesc pInput,            // input buffers
			[In]      uint qualityOfProtection,        // package-specific QOP flags
			[In]      uint sequenceNumber              // sequence number
			);

		#endregion
	}
}