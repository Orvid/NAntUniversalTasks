//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.InteropServices;

using DocsVision.Util;

namespace DocsVision.Security.SSPI
{
	internal sealed class SecurityBuffers : SSPINative.SecBufferDesc, IDisposable
	{
		// object state
		private bool _disposed = false;

		#region Constructors

		public SecurityBuffers(int count)
		{
			// parameters validation
			if (count < 1)
				throw new ArgumentOutOfRangeException("count");

			// create array of buffer descriptors
			BuffersCount = count;
			pvBuffers = Win32.LocalAlloc(Win32.LMEM_ZEROINIT, (uint)(SSPINative.SecBuffer.Size * count));
			if (pvBuffers == IntPtr.Zero)
				throw new OutOfMemoryException();
		}

		#endregion

		#region IDisposable Members

		~SecurityBuffers()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				// free buffers
				FreeBuffers();

				// free array of buffer descriptors
				Win32.LocalFree(pvBuffers);
				pvBuffers = IntPtr.Zero;

				_disposed = true;
			}
		}

		#endregion

		internal void FreeBuffers()
		{
			IntPtr secBuffer = pvBuffers;
			for (int i = 0; i < BuffersCount; ++i)
			{
				IntPtr bufferData = MarshalHelper.ReadIntPtr(secBuffer, typeof(SSPINative.SecBuffer), "pvBuffer");
				if (bufferData != IntPtr.Zero)
				{
					Win32.LocalFree(bufferData);
				}

				secBuffer = IntPtrHelper.Add(pvBuffers, SSPINative.SecBuffer.Size);
			}

			Win32.RtlZeroMemory(pvBuffers, (uint)(SSPINative.SecBuffer.Size * BuffersCount));
		}

		/// <summary>
		/// Initializes buffer
		/// </summary>
		public void SetBuffer(int index, int type, int size)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// parameters validation
			if ((index < 0) || (index >= BuffersCount))
				throw new ArgumentOutOfRangeException("index");
			if (size < 1)
				throw new ArgumentOutOfRangeException("size");

			// allocate buffer
			IntPtr bufferData = Win32.LocalAlloc(Win32.LMEM_ZEROINIT, (uint)size);
			if (bufferData == IntPtr.Zero)
				throw new OutOfMemoryException();

			try
			{
				// create buffer descriptor
				IntPtr secBuffer = IntPtrHelper.Add(pvBuffers, index * SSPINative.SecBuffer.Size);
				MarshalHelper.WriteInt32(secBuffer, typeof(SSPINative.SecBuffer), "BufferSize", size);
				MarshalHelper.WriteInt32(secBuffer, typeof(SSPINative.SecBuffer), "BufferType", type);
				MarshalHelper.WriteIntPtr(secBuffer, typeof(SSPINative.SecBuffer), "pvBuffer", bufferData);
			}
			catch
			{
				Win32.LocalFree(bufferData);
				throw;
			}
		}

		/// <summary>
		/// Initializes buffer with data
		/// </summary>
		public void SetBuffer(int index, int type, byte[] buffer)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// parameters validation
			if ((index < 0) || (index >= BuffersCount))
				throw new ArgumentOutOfRangeException("index");
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (buffer.Length == 0)
				throw new ArgumentOutOfRangeException("buffer.Length");

			// allocate buffer
			int size = buffer.Length;
			IntPtr bufferData = Win32.LocalAlloc(Win32.LMEM_ZEROINIT, (uint)size);
			if (bufferData == IntPtr.Zero)
				throw new OutOfMemoryException();

			try
			{
				// copy buffer data
				Marshal.Copy(buffer, 0, bufferData, size);

				// create buffer descriptor
				IntPtr secBuffer = IntPtrHelper.Add(pvBuffers, index * SSPINative.SecBuffer.Size);
				MarshalHelper.WriteInt32(secBuffer, typeof(SSPINative.SecBuffer), "BufferSize", size);
				MarshalHelper.WriteInt32(secBuffer, typeof(SSPINative.SecBuffer), "BufferType", type);
				MarshalHelper.WriteIntPtr(secBuffer, typeof(SSPINative.SecBuffer), "pvBuffer", bufferData);
			}
			catch
			{
				Win32.LocalFree(bufferData);
				throw;
			}
		}

		/// <summary>
		/// Returns buffer data
		/// </summary>
		public byte[] GetBuffer(int index)
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// parameters validation
			if ((index < 0) || (index >= BuffersCount))
				throw new ArgumentOutOfRangeException("count");

			// get buffer descriptor
			IntPtr secBuffer = IntPtrHelper.Add(pvBuffers, index * SSPINative.SecBuffer.Size);

			// return buffer
			int size = MarshalHelper.ReadInt32(secBuffer, typeof(SSPINative.SecBuffer), "BufferSize");
			if (size > 0)
			{
				return MarshalHelper.ReadBytes(secBuffer, typeof(SSPINative.SecBuffer), "pvBuffer", size);
			}
			else
			{
				return null;
			}
		}
	}
}