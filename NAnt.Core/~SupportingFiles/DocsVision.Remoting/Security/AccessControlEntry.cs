//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.InteropServices;

using DocsVision.Util;

namespace DocsVision.Security
{
	public sealed class AccessControlEntry
	{
		// ACE properties
		private AccessControlEntryType _aceType;
		private AccessControlEntryFlags _aceFlags;
		private AccessMask _accessMask;
		private SecurityIdentifier _trustee;
		private int _size;

		#region Constructors

		public AccessControlEntry() : this(SecurityIdentifier.WellKnown.World, AccessControlEntryType.AccessAllowed, AccessMask.GenericAll)
		{
		}

		public AccessControlEntry(SecurityIdentifier trustee, AccessControlEntryType aceType, AccessMask accessMask)
		{
			// parameters validation
			if (trustee == null)
				throw new ArgumentNullException("trustee");

			_aceType = aceType;
			_aceFlags = AccessControlEntryFlags.Normal;
			_accessMask = accessMask;
			Trustee = trustee;
		}

		internal AccessControlEntry(IntPtr pAce)
		{
			// parameters validation
			if (pAce == IntPtr.Zero)
				throw new ArgumentNullException("pAce");

			// read ACE properties
			_aceType = (AccessControlEntryType)MarshalHelper.ReadByte(pAce, typeof(SecurityNative.ACE), "AceType");
			_aceFlags = (AccessControlEntryFlags)MarshalHelper.ReadByte(pAce, typeof(SecurityNative.ACE), "AceFlags");
			_accessMask = (AccessMask)MarshalHelper.ReadInt32(pAce, typeof(SecurityNative.ACE), "AccessMask");

			// read SID
			IntPtr pSid = IntPtrHelper.Add(pAce, Marshal.OffsetOf(typeof(SecurityNative.ACE), "SidStart"));
			Trustee = new SecurityIdentifier(pSid, true);
		}

		#endregion

		#region Properties

		/// <summary>
		/// ACE type
		/// </summary>
		public AccessControlEntryType Type
		{
			get
			{
				return _aceType;
			}
			set
			{
				_aceType = value;
			}
		}

		/// <summary>
		/// ACE flags
		/// </summary>
		public AccessControlEntryFlags Flags
		{
			get
			{
				return _aceFlags;
			}
			set
			{
				_aceFlags = value;
			}
		}

		/// <summary>
		/// ACE access mask
		/// </summary>
		public AccessMask AccessMask
		{
			get
			{
				return _accessMask;
			}
			set
			{
				_accessMask = value;
			}
		}


		/// <summary>
		/// ACE size in bytes
		/// </summary>
		public int Size
		{
			get
			{
				return _size;
			}
		}

		/// <summary>
		/// ACE trustee
		/// </summary>
		public SecurityIdentifier Trustee
		{
			get
			{
				return _trustee;
			}
			set
			{
				// parameters validation
				if (value == null)
					throw new ArgumentNullException("value");

				_trustee = value;
				_size = SecurityNative.ACE.Size - 4 + _trustee.Size;
			}
		}

		#endregion

		/// <summary>
		/// Writes ACE to native buffer
		/// </summary>
		internal void UnsafeWrite(IntPtr pAce)
		{
			// write ACE properties
			MarshalHelper.WriteByte(pAce, typeof(SecurityNative.ACE), "AceType", (byte)_aceType);
			MarshalHelper.WriteByte(pAce, typeof(SecurityNative.ACE), "AceFlags", (byte)_aceFlags);
			MarshalHelper.WriteInt16(pAce, typeof(SecurityNative.ACE), "AceSize", (short)_size);
			MarshalHelper.WriteInt32(pAce, typeof(SecurityNative.ACE), "AccessMask", (int)_accessMask);

			// write SID
			IntPtr pSid = IntPtrHelper.Add(pAce, Marshal.OffsetOf(typeof(SecurityNative.ACE), "SidStart"));
			Win32.CopyMemory(pSid, _trustee.Handle, (uint)_trustee.Size);
		}
	}
}