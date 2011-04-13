//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.ComponentModel;

using DocsVision.Util;

namespace DocsVision.Security
{
	public sealed class SecureObject
	{
		/// <summary>
		/// Returns security descriptor for given object
		/// </summary>
		public static SecurityDescriptor GetSecurity(
			string objectName,
			SecureObjectType objectType,
			SecurityInformationType securityInfo)
		{
			// parameters validation
			if (objectName == null)
				throw new ArgumentNullException("objectName");

			// obtain security descriptor
			IntPtr pSd;
			int error = SecurityNative.GetNamedSecurityInfo(
				objectName,
				(uint)objectType,
				(uint)securityInfo,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero,
				out pSd);

			if (error != Win32.ERROR_SUCCESS)
				throw new Win32Exception(error);

			// create security descriptor object
			return new SecurityDescriptor(pSd, false);
		}

		/// <summary>
		/// Returns security descriptor for given object
		/// </summary>
		public static SecurityDescriptor GetSecurity(
			IntPtr objectHandle,
			SecureObjectType objectType,
			SecurityInformationType securityInfo)
		{
			// parameters validation
			if (objectHandle == IntPtr.Zero)
				throw new ArgumentNullException("objectHandle");

			// obtain security descriptor
			IntPtr pSd;
			int error = SecurityNative.GetSecurityInfo(
				objectHandle,
				(uint)objectType,
				(uint)securityInfo,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero,
				out pSd);

			if (error != Win32.ERROR_SUCCESS)
				throw new Win32Exception(error);

			// create security descriptor object
			return new SecurityDescriptor(pSd, false);
		}

		/// <summary>
		/// Sets security descriptor for given object
		/// </summary>
		public static void SetSecurity(
			string objectName,
			SecureObjectType objectType,
			SecurityInformationType securityInfo,
			SecurityDescriptor securityDescriptor)
		{
			// parameters validation
			if (objectName == null)
				throw new ArgumentNullException("objectName");
			if (securityDescriptor == null)
				throw new ArgumentNullException("securityDescriptor");

			// set security descriptor
			int error = SecurityNative.SetNamedSecurityInfo(
				objectName,
				(uint)objectType,
				(uint)securityInfo,
				securityDescriptor.Owner == null ? IntPtr.Zero : securityDescriptor.Owner.Handle,
				securityDescriptor.Group == null ? IntPtr.Zero : securityDescriptor.Group.Handle,
				securityDescriptor.DiscretionaryAcl == null ? IntPtr.Zero : securityDescriptor.DiscretionaryAcl.Handle,
				securityDescriptor.SystemAcl == null ? IntPtr.Zero : securityDescriptor.SystemAcl.Handle);

			if (error != Win32.ERROR_SUCCESS)
				throw new Win32Exception(error);
		}

		/// <summary>
		/// Sets security descriptor for given object
		/// </summary>
		public static void SetSecurity(
			IntPtr objectHandle,
			SecureObjectType objectType,
			SecurityInformationType securityInfo,
			SecurityDescriptor securityDescriptor)
		{
			// parameters validation
			if (objectHandle == IntPtr.Zero)
				throw new ArgumentNullException("objectHandle");
			if (securityDescriptor == null)
				throw new ArgumentNullException("securityDescriptor");

			// set security descriptor
			int error = SecurityNative.SetSecurityInfo(
				objectHandle,
				(uint)objectType,
				(uint)securityInfo,
				securityDescriptor.Owner == null ? IntPtr.Zero : securityDescriptor.Owner.Handle,
				securityDescriptor.Group == null ? IntPtr.Zero : securityDescriptor.Group.Handle,
				securityDescriptor.DiscretionaryAcl == null ? IntPtr.Zero : securityDescriptor.DiscretionaryAcl.Handle,
				securityDescriptor.SystemAcl == null ? IntPtr.Zero : securityDescriptor.SystemAcl.Handle);

			if (error != Win32.ERROR_SUCCESS)
				throw new Win32Exception(error);
		}
	}
}