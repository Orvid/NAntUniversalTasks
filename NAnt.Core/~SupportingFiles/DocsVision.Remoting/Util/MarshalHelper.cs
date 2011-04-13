//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.InteropServices;

namespace DocsVision.Util
{
	internal sealed class MarshalHelper
	{
		private MarshalHelper()
		{
			// this class is non creatable
		}

		public static byte ReadByte(IntPtr ptr, Type type, string member)
		{
			return Marshal.ReadByte(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)));
		}

		public static void WriteByte(IntPtr ptr, Type type, string member, byte value)
		{
			Marshal.WriteByte(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)), value);
		}

		public static Int16 ReadInt16(IntPtr ptr, Type type, string member)
		{
			return Marshal.ReadInt16(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)));
		}

		public static void WriteInt16(IntPtr ptr, Type type, string member, Int16 value)
		{
			Marshal.WriteInt16(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)), value);
		}

		public static Int32 ReadInt32(IntPtr ptr, Type type, string member)
		{
			return Marshal.ReadInt32(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)));
		}

		public static void WriteInt32(IntPtr ptr, Type type, string member, Int32 value)
		{
			Marshal.WriteInt32(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)), value);
		}

		public static Int64 ReadInt64(IntPtr ptr, Type type, string member)
		{
			return Marshal.ReadInt64(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)));
		}

		public static void WriteInt64(IntPtr ptr, Type type, string member, Int64 value)
		{
			Marshal.WriteInt64(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)), value);
		}

		public static IntPtr ReadIntPtr(IntPtr ptr, Type type, string member)
		{
			return Marshal.ReadIntPtr(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)));
		}

		public static void WriteIntPtr(IntPtr ptr, Type type, string member, IntPtr value)
		{
			Marshal.WriteIntPtr(IntPtrHelper.Add(ptr, Marshal.OffsetOf(type, member)), value);
		}

		public static string ReadString(IntPtr ptr, Type type, string member)
		{
			return Marshal.PtrToStringUni(ReadIntPtr(ptr, type, member));
		}

		public static byte[] ReadBytes(IntPtr ptr, Type type, string member, int count)
		{
			IntPtr source = ReadIntPtr(ptr, type, member);
			if (source != IntPtr.Zero)
			{
				byte[] data = new byte[count];
				Marshal.Copy(source, data, 0, count);
				return data;
			}

			return null;
		}
	}
}