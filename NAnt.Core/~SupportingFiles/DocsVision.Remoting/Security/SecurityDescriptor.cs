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
	public sealed class SecurityDescriptor
	{
		// Pointer to security descriptor
		private LocalAllocHandle _pSd;

		// SD properties
		private int _revision;
		private SecurityDescriptorControl _control;
		private SecurityIdentifier _owner;
		private SecurityIdentifier _group;
		private AccessControlList _dacl;
		private AccessControlList _sacl;
		private string _stringSd;

		#region Constructors

		public SecurityDescriptor(AccessControlList dacl)
			: this(null, null, dacl, null, 0)
		{
		}

		public SecurityDescriptor(SecurityIdentifier owner, SecurityIdentifier group, AccessControlList dacl)
			: this(owner, group, dacl, null, 0)
		{
		}

		public SecurityDescriptor(SecurityIdentifier owner, SecurityIdentifier group, AccessControlList dacl, AccessControlList sacl)
			: this(owner, group, dacl, sacl, 0)
		{
		}

		public SecurityDescriptor(
			SecurityIdentifier owner,
			SecurityIdentifier group,
			AccessControlList dacl,
			AccessControlList sacl,
			SecurityDescriptorControl control)
		{
			// create security descriptor
			_pSd = CreateSecurityDescriptor(
				owner == null ? IntPtr.Zero : owner.Handle,
				group == null ? IntPtr.Zero : group.Handle,
				dacl == null ? IntPtr.Zero : dacl.Handle,
				sacl == null ? IntPtr.Zero : sacl.Handle,
				control);

			// query properties
			uint revision;
			uint controlFlags;
			if (!SecurityNative.GetSecurityDescriptorControl(_pSd, out controlFlags, out revision))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			_revision = (int)revision;
			_control = (SecurityDescriptorControl)controlFlags;
			_owner = owner;
			_group = group;
			_dacl = dacl;
			_sacl = sacl;
		}

		internal SecurityDescriptor(IntPtr pSd, bool copy)
		{
			// get security descriptor information
			IntPtr pOwner;
			IntPtr pGroup;
			IntPtr pDacl;
			IntPtr pSacl;
			SecurityDescriptorControl control;

			GetSecurityDescriptorInfo(
				pSd,
				out pOwner,
				out pGroup,
				out pDacl,
				out pSacl,
				out control);

			bool copyStructs;
			if (copy)
			{
				// create security descriptor
				_pSd = CreateSecurityDescriptor(pOwner, pGroup, pDacl, pSacl, control);
				copyStructs = true;
			}
			else
			{
				// store pointer
				_pSd = new LocalAllocHandle(pSd);
				copyStructs = (control & SecurityDescriptorControl.SelfRelative) != 0;
			}

			// query properties
			uint revision;
			uint controlFlags;
			if (!SecurityNative.GetSecurityDescriptorControl(_pSd, out controlFlags, out revision))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			_revision = (int)revision;
			_control = (SecurityDescriptorControl)controlFlags;

			if (pOwner != IntPtr.Zero)
				_owner = new SecurityIdentifier(pOwner, copyStructs);
			if (pGroup != IntPtr.Zero)
				_group = new SecurityIdentifier(pGroup, copyStructs);
			if (pDacl != IntPtr.Zero)
				_dacl = new AccessControlList(pDacl, copyStructs);
			if (pSacl != IntPtr.Zero)
				_sacl = new AccessControlList(pSacl, copyStructs);
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
				return _pSd;
			}
		}

		/// <summary>
		/// Revision number
		/// </summary>
		public int Revision
		{
			get
			{
				return _revision;
			}
		}

		/// <summary>
		/// Control flags
		/// </summary>
		public SecurityDescriptorControl Control
		{
			get
			{
				return _control;
			}
		}

		/// <summary>
		/// Owner security identifier
		/// </summary>
		public SecurityIdentifier Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Group security identifier
		/// </summary>
		public SecurityIdentifier Group
		{
			get
			{
				return _group;
			}
		}

		/// <summary>
		/// Security descriptor DACL
		/// </summary>
		public AccessControlList DiscretionaryAcl
		{
			get
			{
				return _dacl;
			}
		}

		/// <summary>
		/// Security descriptor SACL
		/// </summary>
		public AccessControlList SystemAcl
		{
			get
			{
				return _sacl;
			}
		}

		#endregion

		/// <summary>
		/// Creates security descriptor
		/// </summary>
		private static LocalAllocHandle CreateSecurityDescriptor(
			IntPtr pOwner,
			IntPtr pGroup,
			IntPtr pDacl,
			IntPtr pSacl,
			SecurityDescriptorControl control)
		{
			// correct control flags
			if (pDacl != IntPtr.Zero)
				control |= SecurityDescriptorControl.DaclPresent;
			if (pSacl != IntPtr.Zero)
				control |= SecurityDescriptorControl.SaclPresent;

			// allocate memory for security descriptor
			LocalAllocHandle pSd = new LocalAllocHandle(SecurityNative.SecurityDescriptor.Size);

			// initialize security descriptor
			if (!SecurityNative.InitializeSecurityDescriptor(pSd, SecurityNative.SECURITY_DESCRIPTOR_REVISION))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			// set security descriptor control
			if (!SecurityNative.SetSecurityDescriptorControl(pSd, (uint)SecurityDescriptorControl.InheritenceMask, (uint)(control & SecurityDescriptorControl.InheritenceMask)))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			// set SIDs
			if (!SecurityNative.SetSecurityDescriptorOwner(pSd, pOwner, (control & SecurityDescriptorControl.OwnerDefaulted) != 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());
			if (!SecurityNative.SetSecurityDescriptorGroup(pSd, pGroup, (control & SecurityDescriptorControl.GroupDefaulted) != 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			// set ACLs
			if (!SecurityNative.SetSecurityDescriptorDacl(pSd, (control & SecurityDescriptorControl.DaclPresent) != 0, pDacl, (control & SecurityDescriptorControl.DaclDefaulted) != 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());
			if (!SecurityNative.SetSecurityDescriptorSacl(pSd, (control & SecurityDescriptorControl.SaclPresent) != 0, pSacl, (control & SecurityDescriptorControl.SaclDefaulted) != 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return pSd;
		}

		/// <summary>
		/// Returns security descriptor information
		/// </summary>
		private static void GetSecurityDescriptorInfo(
			IntPtr pSd,
			out IntPtr pOwner,
			out IntPtr pGroup,
			out IntPtr pDacl,
			out IntPtr pSacl,
			out SecurityDescriptorControl control)
		{
			// parameters validation
			if (pSd == IntPtr.Zero)
				throw new ArgumentNullException("pSd");

			// query SIDs
			bool ownerDefaulted;
			if (!SecurityNative.GetSecurityDescriptorOwner(pSd, out pOwner, out ownerDefaulted))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			bool groupDefaulted;
			if (!SecurityNative.GetSecurityDescriptorGroup(pSd, out pGroup, out groupDefaulted))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			// query ACLs
			bool daclPresent;
			bool daclDefaulted;
			if (!SecurityNative.GetSecurityDescriptorDacl(pSd, out daclPresent, out pDacl, out daclDefaulted))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			bool saclPresent;
			bool saclDefaulted;
			if (!SecurityNative.GetSecurityDescriptorSacl(pSd, out saclPresent, out pSacl, out saclDefaulted))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			// query properties
			uint revision;
			uint controlFlags;
			if (!SecurityNative.GetSecurityDescriptorControl(pSd, out controlFlags, out revision))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			control = (SecurityDescriptorControl)controlFlags;
		}

		/// <summary>
		/// Copies SD
		/// </summary>
		public SecurityDescriptor Copy()
		{
			return new SecurityDescriptor(_pSd, true);
		}

		/// <summary>
		/// Returns security identifier string representation
		/// </summary>
		public override string ToString()
		{
			if (_stringSd == null)
			{
				// convert SD to string
				IntPtr pStringSd;
				int stringLen;

				if (!SecurityNative.ConvertSecurityDescriptorToStringSecurityDescriptor(
					_pSd,
					SecurityNative.SDDL_REVISION,
					(uint)SecurityInformationType.All,
					out pStringSd,
					out stringLen))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				_stringSd = Marshal.PtrToStringUni(pStringSd);
				Win32.LocalFree(pStringSd);
			}

			return _stringSd;
		}
	}
}