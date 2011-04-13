//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.InteropServices;

namespace DocsVision.Util
{
	internal sealed class IntPtrHelper
	{
		private IntPtrHelper()
		{
			// this class is non creatable
		}

		public static IntPtr Add(IntPtr ptr1, IntPtr ptr2)
		{
			return (IntPtr)(ptr1.ToInt64() + ptr2.ToInt64());
		}

		public static IntPtr Add(IntPtr ptr1, Int64 ptr2)
		{
			return (IntPtr)(ptr1.ToInt64() + ptr2);
		}

		public static IntPtr Add(IntPtr ptr1, Int32 ptr2)
		{
			return (IntPtr)(ptr1.ToInt32() + ptr2);
		}

		public static IntPtr Add(Int64 ptr1, IntPtr ptr2)
		{
			return (IntPtr)(ptr1 + ptr2.ToInt64());
		}

		public static IntPtr Add(Int32 ptr1, IntPtr ptr2)
		{
			return (IntPtr)(ptr1 + ptr2.ToInt32());
		}
	}
}