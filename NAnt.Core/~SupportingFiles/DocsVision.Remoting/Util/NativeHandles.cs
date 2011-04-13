//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;

namespace DocsVision.Util
{
	/// <summary>
	/// Base class for native resources management
	/// </summary>
	internal abstract class IntPtrWrapper : IDisposable
	{
		// Wrapped native handle
		protected IntPtr _handle;

		#region Contructors

		public IntPtrWrapper(IntPtr handle)
		{
			_handle = handle;
		}

		#endregion

		#region IDisposable Members

		~IntPtrWrapper()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected abstract void Dispose(bool disposing);

		#endregion

		/// <summary>
		/// Conversions operator
		/// </summary>
		public static implicit operator IntPtr(IntPtrWrapper obj)
		{
			return obj._handle;
		}
	}

	/// <summary>
	/// LocalAlloc/LocalFree wrapper
	/// </summary>
	internal class NativeHandle : IntPtrWrapper
	{
		#region Constructors

		public NativeHandle(IntPtr handle) : base(handle)
		{
		}

		#endregion

		/// <summary>
		/// Frees handle
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (_handle != IntPtr.Zero)
			{
				Win32.CloseHandle(_handle);
				_handle = IntPtr.Zero;
			}
		}
	}

	/// <summary>
	/// LocalAlloc/LocalFree wrapper
	/// </summary>
	internal class LocalAllocHandle : IntPtrWrapper
	{
		#region Constructors

		public LocalAllocHandle(IntPtr handle) : base(handle)
		{
		}

		public LocalAllocHandle(int size) : base(Allocate(size))
		{
		}

		#endregion

		/// <summary>
		/// Returns true, if memory was allocated
		/// </summary>
		public bool Allocated
		{
			get
			{
				return _handle != IntPtr.Zero;
			}
		}

		/// <summary>
		/// Frees memory
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (_handle != IntPtr.Zero)
			{
				Win32.LocalFree(_handle);
				_handle = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Allocates memory
		/// </summary>
		private static IntPtr Allocate(int size)
		{
			IntPtr handle = Win32.LocalAlloc(Win32.LMEM_ZEROINIT, (uint)size);
			if (handle == IntPtr.Zero)
				throw new OutOfMemoryException();

			return handle;
		}
	}
}