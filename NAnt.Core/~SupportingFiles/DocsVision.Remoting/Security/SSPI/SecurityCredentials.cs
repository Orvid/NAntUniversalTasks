//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Runtime.InteropServices;

namespace DocsVision.Security.SSPI
{
	public sealed class SecurityCredentials : IDisposable
	{
		// credentials handle
		private Int64 _credHandle;

		// security package
		private SecurityPackage _secPackage;

		// credentials properties
		private Int64 _credExpiry;
		private SecurityCredentialsType _credType;
		private string _userName;

		// object state
		private bool _disposed = false;

		#region Constructors

		internal SecurityCredentials(
			SecurityPackage secPackage,
			Int64 credHandle,
			Int64 credExpiry,
			SecurityCredentialsType credType)
		{
			// parameters validation
			if (secPackage == null)
				throw new ArgumentNullException("secPackage");
			if (credHandle == 0)
				throw new ArgumentNullException("credHandle");

			_secPackage = secPackage;

			_credHandle = credHandle;
			_credExpiry = credExpiry;
			_credType = credType;
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
				if (_credHandle != 0)
				{
					SSPINative.FreeCredentialsHandle(ref _credHandle);
					_credHandle = 0;
				}

				_disposed = true;
			}
		}

		~SecurityCredentials()
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
				return _secPackage;
			}
		}

		/// <summary>
		/// credentials handle
		/// </summary>
		public Int64 Handle
		{
			get { return _credHandle; }
		}

		/// <summary>
		/// credentials expiration time
		/// </summary>
		public Int64 Expiry
		{
			get
			{
				return _credExpiry;
			}
		}

		/// <summary>
		/// credentials type
		/// </summary>
		public SecurityCredentialsType Type
		{
			get
			{
				return _credType;
			}
		}

		/// <summary>
		/// Principal name
		/// </summary>
		public string UserName
		{
			get
			{
				if (_userName == null)
				{
					SSPINative.SecPkgCredentials_Names credNames = new SSPINative.SecPkgCredentials_Names();
					GCHandle gcNames = GCHandle.Alloc(credNames, GCHandleType.Pinned);

					int error = SSPINative.QueryCredentialsAttributes(
						ref _credHandle,
						SSPINative.SECPKG_CRED_ATTR_NAMES,
						gcNames.AddrOfPinnedObject());
					gcNames.Free();

					if (error < 0)
						throw new SSPIException(error, "Could not query credentials attributes");

					_userName = Marshal.PtrToStringUni(credNames.UserName);
					SSPINative.FreeContextBuffer(credNames.UserName);
				}

				return _userName;
			}
		}

		#endregion
	}
}