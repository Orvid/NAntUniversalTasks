//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;

namespace DocsVision.Security.SSPI
{
	public interface IAuthModule
	{
		/// <summary>
		/// Acquires security credentials for a current security principal
		/// </summary>
		SecurityCredentials AcquireSecurityCredentials(
			SecurityCredentialsType credType,
			IDictionary authData);

		/// <summary>
		/// Creates security context and generates client token
		/// </summary>
		SecurityContext CreateSecurityContext(
			SecurityCredentials credentials,
			SecurityContextAttributes contextAttributes,
			string targetName,
			out byte[] outputToken);

		/// <summary>
		/// Creates security context, proceeds client token and generates server token
		/// </summary>
		SecurityContext AcceptSecurityContext(
			SecurityCredentials credentials,
			SecurityContextAttributes contextAttributes,
			byte[] inputToken,
			out byte[] outputToken);

		/// <summary>
		/// Updates security context, proceeds input token and generates output token
		/// </summary>
		void UpdateSecurityContext(
			SecurityContext context,
			SecurityContextAttributes contextAttributes,
			byte[] inputToken,
			out byte[] outputToken);

		/// <summary>
		/// Creates the signature of a message
		/// </summary>
		byte[] CreateSignature(SecurityContext context, byte[] message);

		/// <summary>
		/// Verifies the signature of a message
		/// </summary>
		void VerifySignature(SecurityContext context, byte[] message, byte[] signature);

		/// <summary>
		/// Encrypts message
		/// </summary>
		byte[] EncryptMessage(SecurityContext context, byte[] message);

		/// <summary>
		/// Decrypts message
		/// </summary>
		byte[] DecryptMessage(SecurityContext context, byte[] encBuffer);
	}
}