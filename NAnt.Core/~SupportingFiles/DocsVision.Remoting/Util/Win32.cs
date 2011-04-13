//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Security;
using System.Runtime.InteropServices;

namespace DocsVision.Util
{
	[SuppressUnmanagedCodeSecurityAttribute()]
	internal sealed class Win32
	{
		private Win32()
		{
			// this class is non creatable
		}

		#region Constants

		//
		//  Invalid handle value
		//

		public static readonly IntPtr InvalidHandle   = new IntPtr(-1);

		//
		//  Local Memory Flags
		//

		public const uint LMEM_FIXED                      = 0x0000;
		public const uint LMEM_MOVEABLE                   = 0x0002;
		public const uint LMEM_NOCOMPACT                  = 0x0010;
		public const uint LMEM_NODISCARD                  = 0x0020;
		public const uint LMEM_ZEROINIT                   = 0x0040;
		public const uint LMEM_MODIFY                     = 0x0080;
		public const uint LMEM_DISCARDABLE                = 0x0F00;
		public const uint LMEM_VALID_FLAGS                = 0x0F72;
		public const uint LMEM_INVALID_HANDLE             = 0x8000;
		public const uint LMEM_DISCARDED                  = 0x4000;
		public const uint LMEM_LOCKCOUNT                  = 0x00FF;

		#endregion

		#region Error Codes

		public const int ERROR_SUCCESS               = 0;
		public const int ERROR_OUTOFMEMORY           = 14;

		#endregion

		#region Kernel32 imports

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern bool CloseHandle(
			[In]      IntPtr hObject                   // handle
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern IntPtr LocalAlloc(
			[In]      uint nFlags,                     // allocation type
			[In]      uint nSize                       // bytes to allocate
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern IntPtr LocalFree(
			[In]      IntPtr pBuffer                   // pointer to buffer
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern void RtlZeroMemory(
			[In]      IntPtr pBuffer,                  // pointer to buffer
			[In]      uint nSize                       // buffer length
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=false)]
		public static extern void CopyMemory(
			[In]      IntPtr pDestBuffer,              // pointer to destination buffer
			[In]      IntPtr pSourceBuffer,            // pointer to source buffer
			[In]      uint nSize                       // buffer length
			);

		#endregion
	}
}