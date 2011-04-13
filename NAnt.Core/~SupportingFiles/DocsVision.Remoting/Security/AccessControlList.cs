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
	public sealed class AccessControlList
	{
		// Pointer to access control list
		private LocalAllocHandle _pAcl;

		// ACL properties
		private int _revision;
		private int _size;
		private int _count;
		private AccessControlEntry[] _aces;

		#region Constructors

		public AccessControlList(AccessControlEntry[] aces)
		{
			// parameters validation
			if (aces == null)
				throw new ArgumentNullException("aces");

			// create aces list
			int listSize;
			LocalAllocHandle pAceList = CreateAceList(aces, out listSize);

			// allocate memory for ACL
			int aclSize = SecurityNative.ACL.Size + listSize;
			_pAcl = new LocalAllocHandle(aclSize);

			// intialize ACL
			if (!SecurityNative.InitializeAcl(_pAcl, (uint)aclSize, SecurityNative.ACL_REVISION))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			// add aces to ACL
			if (!SecurityNative.AddAce(_pAcl, SecurityNative.ACL_REVISION, 0, pAceList, (uint)listSize))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			_revision = (int)SecurityNative.ACL_REVISION;
			_size = aclSize;
			_count = aces.Length;
			_aces = aces;
		}

		internal AccessControlList(IntPtr pAcl, bool copy)
		{
			// parameters validation
			if (pAcl == IntPtr.Zero)
				throw new ArgumentNullException("pAcl");

			// read properties
			_revision = MarshalHelper.ReadByte(pAcl, typeof(SecurityNative.ACL), "Revision");
			_size = MarshalHelper.ReadInt16(pAcl, typeof(SecurityNative.ACL), "AclSize");
			_count = MarshalHelper.ReadInt16(pAcl, typeof(SecurityNative.ACL), "AceCount");

			if (copy)
			{
				// copy access list
				_pAcl = new LocalAllocHandle(_size);
				Win32.CopyMemory(_pAcl, pAcl, (uint)_size);
			}
			else
			{
				// store pointer
				_pAcl = new LocalAllocHandle(pAcl);
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
				return _pAcl;
			}
		}

		/// <summary>
		/// Revision numbre
		/// </summary>
		public int Revision
		{
			get
			{
				return _revision;
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

		/// <summary>
		/// Entries count
		/// </summary>
		public int Count
		{
			get
			{
				return _count;
			}
		}

		/// <summary>
		/// List entries
		/// </summary>
		public AccessControlEntry[] Entries
		{
			get
			{
				if (_aces == null)
				{
					_aces = GetAces(_pAcl, _count);
				}

				return _aces;
			}
		}

		#endregion

		/// <summary>
		/// Copies ACL
		/// </summary>
		public AccessControlList Copy()
		{
			return new AccessControlList(_pAcl, true);
		}

		/// <summary>
		/// Creates ACE list in memory
		/// </summary>
		private static LocalAllocHandle CreateAceList(AccessControlEntry[] aces, out int listSize)
		{
			// parameters validation
			if (aces == null)
				throw new ArgumentNullException("aces");

			// calculate list size
			listSize = 0;
			for (int i = 0; i < aces.Length; ++i)
			{
				listSize += aces[i].Size;
			}

			// allocate buffer for aces
			LocalAllocHandle pAceList = new LocalAllocHandle(listSize);

			// write aces to buffer
			IntPtr pAce = pAceList;
			for (int i = 0; i < aces.Length; ++i)
			{
				aces[i].UnsafeWrite(pAce);
				pAce = IntPtrHelper.Add(pAce, aces[i].Size);
			}

			return pAceList;
		}

		/// <summary>
		/// Read ACEs from ACL in memory
		/// </summary>
		private static AccessControlEntry[] GetAces(IntPtr pAcl, int count)
		{
			AccessControlEntry[] aces = new AccessControlEntry[count];

			IntPtr pAce;
			for (int i = 0; i < count; ++i)
			{
				if (!SecurityNative.GetAce(pAcl, (uint)i, out pAce))
					throw new Win32Exception(Marshal.GetLastWin32Error());

				aces[i] = new AccessControlEntry(pAce);
			}

			return aces;
		}
	}
}