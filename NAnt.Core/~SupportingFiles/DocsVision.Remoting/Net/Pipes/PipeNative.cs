//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Security;
using System.Runtime.InteropServices;

using DocsVision.Util;

namespace DocsVision.Net.Pipes
{
	[SuppressUnmanagedCodeSecurityAttribute()]
	internal sealed class PipeNative
	{
		private PipeNative()
		{
			// this class is non creatable
		}

		#region Constants

		//
		// Define the dwOpenMode values for CreateNamedPipe
		//

		public const int PIPE_ACCESS_INBOUND         = 0x00000001;
		public const int PIPE_ACCESS_OUTBOUND        = 0x00000002;
		public const int PIPE_ACCESS_DUPLEX          = 0x00000003;

		//
		// Define the dwPipeMode values for CreateNamedPipe
		//

		public const int PIPE_WAIT                   = 0x00000000;
		public const int PIPE_NOWAIT                 = 0x00000001;
		public const int PIPE_READMODE_BYTE          = 0x00000000;
		public const int PIPE_READMODE_MESSAGE       = 0x00000002;
		public const int PIPE_TYPE_BYTE              = 0x00000000;
		public const int PIPE_TYPE_MESSAGE           = 0x00000004;

		//
		// Define the well known values for CreateNamedPipe nMaxInstances
		//

		public const int PIPE_UNLIMITED_INSTANCES    = 255;

		//
		// Define the well known values for WaitNamedPipe nTimeOut
		//

		public const int NMPWAIT_WAIT_FOREVER        = -1;
		public const int NMPWAIT_NOWAIT              = 0x00000001;
		public const int NMPWAIT_USE_DEFAULT_WAIT    = 0x00000000;


		//
		// Define the dwShareMode values for CreateFile
		//

		public const int FILE_SHARE_NONE             = 0x00000000;
		public const int FILE_SHARE_READ             = 0x00000001;
		public const int FILE_SHARE_WRITE            = 0x00000002;
		public const int FILE_SHARE_DELETE           = 0x00000004;

		//
		// Define the dwCreationDisposition values for CreateFile
		//

		public const int CREATE_NEW                  = 1;
		public const int CREATE_ALWAYS               = 2;
		public const int OPEN_EXISTING               = 3;
		public const int OPEN_ALWAYS                 = 4;
		public const int TRUNCATE_EXISTING           = 5;

		//
		// Define the dwFlagsAndAttributes values for CreateFile
		//

		public const int FILE_FLAG_WRITE_THROUGH            = unchecked((int)0x80000000);
		public const int FILE_FLAG_OVERLAPPED               = 0x40000000;
		public const int FILE_FLAG_NO_BUFFERING             = 0x20000000;
		public const int FILE_FLAG_RANDOM_ACCESS            = 0x10000000;
		public const int FILE_FLAG_SEQUENTIAL_SCAN          = 0x08000000;
		public const int FILE_FLAG_DELETE_ON_CLOSE          = 0x04000000;
		public const int FILE_FLAG_BACKUP_SEMANTICS         = 0x02000000;
		public const int FILE_FLAG_POSIX_SEMANTICS          = 0x01000000;
		public const int FILE_FLAG_OPEN_REPARSE_POINT       = 0x00200000;
		public const int FILE_FLAG_OPEN_NO_RECALL           = 0x00100000;
		public const int FILE_FLAG_FIRST_PIPE_INSTANCE      = 0x00080000;

		public const int FILE_ATTRIBUTE_READONLY            = 0x00000001;
		public const int FILE_ATTRIBUTE_HIDDEN              = 0x00000002;
		public const int FILE_ATTRIBUTE_SYSTEM              = 0x00000004;
		public const int FILE_ATTRIBUTE_DIRECTORY           = 0x00000010;
		public const int FILE_ATTRIBUTE_ARCHIVE             = 0x00000020;
		public const int FILE_ATTRIBUTE_DEVICE              = 0x00000040;
		public const int FILE_ATTRIBUTE_NORMAL              = 0x00000080;
		public const int FILE_ATTRIBUTE_TEMPORARY           = 0x00000100;
		public const int FILE_ATTRIBUTE_SPARSE_FILE         = 0x00000200;
		public const int FILE_ATTRIBUTE_REPARSE_POINT       = 0x00000400;
		public const int FILE_ATTRIBUTE_COMPRESSED          = 0x00000800;
		public const int FILE_ATTRIBUTE_OFFLINE             = 0x00001000;
		public const int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
		public const int FILE_ATTRIBUTE_ENCRYPTED           = 0x00004000;

		//
		// Define the Security Quality of Service bits to be passed into CreateFile
		//

		public const int SECURITY_ANONYMOUS          = 0 << 16;
		public const int SECURITY_IDENTIFICATION     = 1 << 16;
		public const int SECURITY_IMPERSONATION      = 2 << 16;
		public const int SECURITY_DELEGATION         = 3 << 16;

		public const int SECURITY_CONTEXT_TRACKING   = 0x00040000;
		public const int SECURITY_EFFECTIVE_ONLY     = 0x00080000;

		public const int SECURITY_SQOS_PRESENT       = 0x00100000;
		public const int SECURITY_VALID_SQOS_FLAGS   = 0x001F0000;

		//
		//  These are the generic rights
		//

		public const int GENERIC_READ                = unchecked((int)0x80000000);
		public const int GENERIC_WRITE               = 0x40000000;
		public const int GENERIC_EXECUTE             = 0x20000000;
		public const int GENERIC_ALL                 = 0x10000000;

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class SecurityAttributes
		{
			// Specifies the size, in bytes, of this structure
			public int Length = Marshal.SizeOf(typeof(SecurityAttributes));
			// Pointer to a security descriptor for the object that controls the sharing of it
			public IntPtr SecurityDescriptor = IntPtr.Zero;
			// Specifies whether the returned handle is inherited when a new process is created
			public bool InheritHandle = false;
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(SecurityAttributes));
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public class Overlapped
		{
			// Reserved for operating system use
			public IntPtr InternalLow = IntPtr.Zero;
			public IntPtr InternalHigh = IntPtr.Zero;
			// File position at which to start the transfer
			public int OffsetLow = 0;
			public int OffsetHigh = 0;
			// Handle to an event set to the signaled state when the operation has been completed
			public IntPtr hEvent = IntPtr.Zero;
			// Fileds offset in this structure
			public static readonly int InternalLowOffset = (int)Marshal.OffsetOf(typeof(Overlapped), "InternalLow");
			public static readonly int InternalHighOffset = (int)Marshal.OffsetOf(typeof(Overlapped), "InternalHigh");
			public static readonly int OffsetLowOffset = (int)Marshal.OffsetOf(typeof(Overlapped), "OffsetLow");
			public static readonly int OffsetHighOffset = (int)Marshal.OffsetOf(typeof(Overlapped), "OffsetHigh");
			public static readonly int hEventOffset = (int)Marshal.OffsetOf(typeof(Overlapped), "hEvent");
			// Size of this structure
			public static readonly int Size = Marshal.SizeOf(typeof(Overlapped));
		}

		#endregion

		#region Error Codes

		public const int ERROR_BAD_PIPE              = 230;
		public const int ERROR_PIPE_BUSY             = 231;
		public const int ERROR_NO_DATA               = 232;
		public const int ERROR_PIPE_NOT_CONNECTED    = 233;
		public const int ERROR_MORE_DATA             = 234;
		public const int ERROR_PIPE_CONNECTED        = 535;
		public const int ERROR_PIPE_LISTENING        = 536;
		public const int ERROR_IO_INCOMPLETE         = 996;
		public const int ERROR_IO_PENDING            = 997;

		#endregion

		#region Kernel32 imports

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern IntPtr CreateNamedPipe(
			[In]      string lpPipeName,               // pipe name
			[In]      int dwOpenMode,                  // pipe open mode
			[In]      int dwPipeMode,                  // pipe-specific modes
			[In]      int nMaxInstances,               // maximum number of instances
			[In]      int nOutBufferSize,              // output buffer size
			[In]      int nInBufferSize,               // input buffer size
			[In]      int nDefaultTimeOut,             // time-out interval
			[In]      SecurityAttributes attr          // security attributes
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool ConnectNamedPipe(
			[In]      IntPtr hNamedPipe,               // handle to named pipe
			[In]      IntPtr lpOverlapped              // overlapped structure
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool DisconnectNamedPipe(
			[In]      IntPtr hNamedPipe                // handle to pipe
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool PeekNamedPipe(
			[In]      IntPtr hNamedPipe,               // handle to pipe
			[In]      IntPtr lpBuffer,                 // data buffer
			[In]      int nBufferSize,                 // buffer size
			[Out]     out int lpBytesRead,             // number of bytes read
			[Out]     out int lpTotalBytesAvail,       // total number of bytes available in pipe
			[Out]     out int lpBytesLeftThisMessage   // total number of bytes available in current message
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool WaitNamedPipe(
			[In]      string lpNamedPipeName,          // pipe name
			[In]      int nTimeOut                     // time-out interval
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern IntPtr CreateFile(
			[In]      string lpFileName,               // file name
			[In]      int dwDesiredAccess,             // accessMode mode
			[In]      int dwShareMode,                 // share mode
			[In]      SecurityAttributes attr,         // security attributes
			[In]      int dwCreation,                  // how to create
			[In]      int dwFlagsAndAttributes,        // file attributes
			[In]      IntPtr hTemplateFile             // handle to template file
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool ReadFile(
			[In]      IntPtr hFile,                    // handle to file
			[In]      IntPtr lpBuffer,                 // data buffer
			[In]      int nNumberOfBytesToRead,        // number of bytes to read
			[Out]     out int lpNumberOfBytesRead,     // number of bytes read
			[In]      IntPtr lpOverlapped              // overlapped structure
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool ReadFile(
			[In]      IntPtr hFile,                    // handle to file
			[In]      IntPtr lpBuffer,                 // data buffer
			[In]      int nNumberOfBytesToRead,        // number of bytes to read
			[In]      IntPtr lpNumberOfBytesRead,      // number of bytes read
			[In]      IntPtr lpOverlapped              // overlapped structure
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool WriteFile(
			[In]      IntPtr hFile,                    // handle to file
			[In]      IntPtr lpBuffer,                 // data buffer
			[In]      int nNumberOfBytesToWrite,       // number of bytes to write
			[Out]     out int lpNumberOfBytesWritten,  // number of bytes read
			[In]      IntPtr lpOverlapped              // overlapped structure
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool WriteFile(
			[In]      IntPtr hFile,                    // handle to file
			[In]      IntPtr lpBuffer,                 // data buffer
			[In]      int nNumberOfBytesToWrite,       // number of bytes to write
			[In]      IntPtr lpNumberOfBytesWritten,   // number of bytes written
			[In]      IntPtr lpOverlapped              // overlapped structure
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool FlushFileBuffers(
			[In]      IntPtr hFile                     // handle to file
			);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool GetOverlappedResult(
			[In]      IntPtr hFile,                    // handle to file
			[In]      IntPtr lpOverlapped,             // overlapped structure
			[Out]     out int lpNumberOfBytes,         // number of bytes transferred
			[In]      bool bWait                       // wait for completion
			);

		#endregion

		#region AdvApi32 imports

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool ImpersonateNamedPipeClient(
			[In]      IntPtr hNamedPipe                // handle to pipe
			);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
		public static extern bool RevertToSelf();

		#endregion

		public static int HackedGetOverlappedResult(IntPtr pOverlapped, out int totalBytes)
		{
			//
			// read IO asynchronous status from Overlapped structure
			//
			int status = Marshal.ReadInt32(IntPtrHelper.Add(pOverlapped, Overlapped.InternalLowOffset));
			if (status == 0)
			{
				//
				// the Async IO call completed
				//
				totalBytes = Marshal.ReadInt32(IntPtrHelper.Add(pOverlapped, Overlapped.InternalHighOffset));
			}
			else
			{
				//
				// the Async IO call failed or still pending
				//
				totalBytes = -1;
			}

			return status;
		}
	}
}