//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace DocsVision.Security.SSPI
{
	public sealed class SecurityContext : IDisposable
	{
		// context handle
		private Int64 _contextHandle;

		// associated credentials
		private SecurityCredentials _credentials;

		// context properties
		private Int64 _contextExpiry;
		private SecurityContextType _contextType;
		private SecurityContextState _contextState;
		private SSPINative.SecPkgContext_Sizes _contextSizes;
		private WindowsIdentity _contextIdentity;

		// object state
		private bool _disposed = false;

		#region Constructors

		internal SecurityContext(
			SecurityCredentials credentials,
			Int64 contextHandle,
			Int64 contextExpiry,
			SecurityContextType contextType,
			SecurityContextState contextState)
		{
			// parameters validation
			if (credentials == null)
				throw new ArgumentNullException("credentials");
			if (contextHandle == 0)
				throw new ArgumentNullException("contextHandle");

			_credentials = credentials;

			_contextHandle = contextHandle;
			_contextExpiry = contextExpiry;
			_contextType = contextType;
			_contextState = contextState;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (_contextHandle != 0)
				{
					SSPINative.DeleteSecurityContext(ref _contextHandle);
					_contextHandle = 0;
				}

				_disposed = true;
			}
		}

		~SecurityContext()
		{
			Dispose(false);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Security package
		/// </summary>
		public SecurityPackage Package
		{
			get
			{
				return _credentials.Package;
			}
		}

		/// <summary>
		/// Assotiated credentials
		/// </summary>
		public SecurityCredentials credentials
		{
			get
			{
				return _credentials;
			}
		}

		/// <summary>
		/// Context handle
		/// </summary>
		public Int64 Handle
		{
			get
			{
				return _contextHandle;
			}
		}

		/// <summary>
		/// credentials expiration time
		/// </summary>
		public Int64 Expiry
		{
			get
			{
				return _contextExpiry;
			}
		}

		/// <summary>
		/// Context type
		/// </summary>
		public SecurityContextType Type
		{
			get
			{
				return _contextType;
			}
		}

		/// <summary>
		/// Context state
		/// </summary>
		public SecurityContextState State
		{
			get
			{
				return _contextState;
			}
		}

		/// <summary>
		/// Maximum token size
		/// </summary>
		public int MaxToken
		{
			get
			{
				if (_contextSizes == null)
				{
					_contextSizes = GetContextSizes();
				}

				return _contextSizes.MaxToken;
			}
		}

		/// <summary>
		/// Maximum signature size
		/// </summary>
		public int MaxSignature
		{
			get
			{
				if (_contextSizes == null)
				{
					_contextSizes = GetContextSizes();
				}

				return _contextSizes.MaxSignature;
			}
		}

		/// <summary>
		/// Preferred block size
		/// </summary>
		public int BlockSize
		{
			get
			{
				if (_contextSizes == null)
				{
					_contextSizes = GetContextSizes();
				}

				return _contextSizes.BlockSize;
			}
		}

		/// <summary>
		/// Preferred block size
		/// </summary>
		public int SecurityTrailer
		{
			get
			{
				if (_contextSizes == null)
				{
					_contextSizes = GetContextSizes();
				}

				return _contextSizes.SecurityTrailer;
			}
		}

		/// <summary>
		/// Context identity
		/// </summary>
		public WindowsIdentity ClientIdentity
		{
			get
			{
				if (_contextIdentity == null)
				{
					_contextIdentity = GetContextIdentity();
				}

				return _contextIdentity;
			}
		}

		#endregion

		/// <summary>
		/// Marks context state as comleted
		/// </summary>
		internal void SetCompleted()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// set state
			_contextState = SecurityContextState.Completed;
		}

		/// <summary>
		/// Impersonates security context on current thread
		/// </summary>
		public void ImpersonateContext()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// impersonate context
			int error = SSPINative.ImpersonateSecurityContext(ref _contextHandle);
			if (error < 0)
				throw new SSPIException(error, "Could not impersonate security context");
		}

		/// <summary>
		/// Reverts impersonation of a security context
		/// </summary>
		public void RevertContext()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// revert impersonation
			int error = SSPINative.RevertSecurityContext(ref _contextHandle);
			if (error < 0)
				throw new SSPIException(error, "Could not revert security context impersonation");
		}

		/// <summary>
		/// Returns security context access token
		/// </summary>
		private WindowsIdentity GetContextIdentity()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// query context token
			IntPtr accessToken = IntPtr.Zero;
			int error = SSPINative.QuerySecurityContextToken(
				ref _contextHandle,
				out accessToken);
			if (error < 0)
				throw new SSPIException(error, "Could not determine security context identity");

			return new WindowsIdentity(accessToken);
		}

		/// <summary>
		/// Queries for context properties
		/// </summary>
		private SSPINative.SecPkgContext_Sizes GetContextSizes()
		{
			// check object state
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);

			// query context properties
			SSPINative.SecPkgContext_Sizes contextSizes = new SSPINative.SecPkgContext_Sizes();
			GCHandle gcSizes = GCHandle.Alloc(contextSizes, GCHandleType.Pinned);

			int error = SSPINative.QueryContextAttributes(
				ref _contextHandle,
				SSPINative.SECPKG_ATTR_SIZES,
				gcSizes.AddrOfPinnedObject());
			gcSizes.Free();

			if (error < 0)
				throw new SSPIException(error, "Could not query security context information");

			return contextSizes;
		}
	}
}