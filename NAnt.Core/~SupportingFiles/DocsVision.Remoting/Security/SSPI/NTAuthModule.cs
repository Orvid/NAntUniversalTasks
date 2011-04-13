//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Runtime.InteropServices;

using DocsVision.Util;

namespace DocsVision.Security.SSPI
{
	public sealed class NTAuthModule : IAuthModule
	{
		// Security package to use
		private SecurityPackage _secPackage;

		#region Constructors

		public NTAuthModule(SecurityPackage secPackage)
		{
			// parameters validation
			if (secPackage == null)
				throw new ArgumentNullException("secPackage");

			_secPackage = secPackage;
		}

		#endregion

		#region IAuthModule Members

		/// <summary>
		/// Acquires security credentials for a current security principal
		/// </summary>
		public SecurityCredentials AcquireSecurityCredentials(
			SecurityCredentialsType credType,
			IDictionary authData)
		{
			// prepare context usage
			uint credUsage = 0;
			if ((credType & SecurityCredentialsType.InBound) != 0)
				credUsage |= SSPINative.SECPKG_CRED_INBOUND;
			if ((credType & SecurityCredentialsType.OutBound) != 0)
				credUsage |= SSPINative.SECPKG_CRED_OUTBOUND;

			// prepare identity information
			SSPINative.SecWinNTAuthIdentity authIdentity = new SSPINative.SecWinNTAuthIdentity();
			if (authData != null)
			{
				string userName = (string)authData["user"];
				if (userName != null)
				{
					authIdentity.User = userName;
					authIdentity.UserLength = userName.Length;
				}
				string domain = (string)authData["domain"];
				if (domain != null)
				{
					authIdentity.Domain = domain;
					authIdentity.DomainLength = domain.Length;
				}
				string password = (string)authData["password"];
				if (password != null)
				{
					authIdentity.Password = password;
					authIdentity.PasswordLength = password.Length;
				}
			}

			// acquire credentials
			Int64 credHandle;
			Int64 credExpiry;

			int error = SSPINative.AcquireCredentialsHandle(
				null,
				_secPackage.Name,
				credUsage,
				IntPtr.Zero,
				authIdentity,
				IntPtr.Zero,
				IntPtr.Zero,
				out credHandle,
				out credExpiry);
			if (error < 0)
				throw new SSPIException(Marshal.GetLastWin32Error(), "Could not acquire credentials handle");

			// create credentials object
			return new SecurityCredentials(_secPackage, credHandle, credExpiry, credType);
		}

		/// <summary>
		/// Creates security context and generates client token
		/// </summary>
		public SecurityContext CreateSecurityContext(
			SecurityCredentials credentials,
			SecurityContextAttributes contextAttributes,
			string targetName,
			out byte[] outputToken)
		{
			// parameters validation
			if (credentials == null)
				throw new ArgumentNullException("credentials");

			// prepare requirements for context
			uint contextReq = GetContextRequirements(false, contextAttributes);

			// prepare buffers
			SecurityBuffers outputBuffers = new SecurityBuffers(1);
			outputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_TOKEN, _secPackage.MaxToken);

			// create context
			Int64 credHandle = credentials.Handle;
			Int64 newContextHandle;
			Int64 contextExpiry;
			uint contextAttribs;

			int error = SSPINative.InitializeSecurityContext(
				ref credHandle,
				IntPtr.Zero,
				targetName,
				contextReq,
				0,
				SSPINative.SECURITY_NETWORK_DREP,
				null,
				0,
				out newContextHandle,
				outputBuffers,
				out contextAttribs,
				out contextExpiry);

			// check context state
			bool continueNeeded = false;
			bool completeNeeded = false;

			switch (error)
			{
				case Win32.ERROR_SUCCESS:
					break;
				case SSPINative.SEC_I_CONTINUE_NEEDED:
					continueNeeded = true;
					break;
				case SSPINative.SEC_I_COMPLETE_NEEDED:
					completeNeeded = true;
					break;
				case SSPINative.SEC_I_COMPLETE_AND_CONTINUE:
					continueNeeded = true;
					completeNeeded = true;
					break;
				default:
					throw new SSPIException(error, "Could not create security context");
			}

			if (completeNeeded)
			{
				// complete context
				error = SSPINative.CompleteAuthToken(ref newContextHandle, outputBuffers);
				if (error < 0)
					throw new SSPIException(error, "Could not complete security context");
			}

			// get output token
			outputToken = outputBuffers.GetBuffer(0);
			outputBuffers.Dispose();

			// create context object
			SecurityContextState contextState = (continueNeeded ? SecurityContextState.ContinueNeeded : SecurityContextState.Completed);
			return new SecurityContext(credentials, newContextHandle, contextExpiry, SecurityContextType.Client, contextState);
		}

		/// <summary>
		/// Creates security context, proceeds client token and generates server token
		/// </summary>
		public SecurityContext AcceptSecurityContext(
			SecurityCredentials credentials,
			SecurityContextAttributes contextAttributes,
			byte[] inputToken,
			out byte[] outputToken)
		{
			// parameters validation
			if (credentials == null)
				throw new ArgumentNullException("credentials");
			if (inputToken == null)
				throw new ArgumentNullException("inputToken");

			// prepare requirements for context
			uint contextReq = GetContextRequirements(true, contextAttributes);

			// prepare buffers
			SecurityBuffers inputBuffers = new SecurityBuffers(1);
			inputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_TOKEN, inputToken);

			SecurityBuffers outputBuffers = new SecurityBuffers(1);
			outputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_TOKEN, _secPackage.MaxToken);

			// create context
			Int64 credHandle = credentials.Handle;
			Int64 newContextHandle;
			Int64 contextExpiry;
			uint contextAttribs;

			int error = SSPINative.AcceptSecurityContext(
				ref credHandle,
				IntPtr.Zero,
				inputBuffers,
				contextReq,
				SSPINative.SECURITY_NETWORK_DREP,
				out newContextHandle,
				outputBuffers,
				out contextAttribs,
				out contextExpiry);

			inputBuffers.Dispose();

			// check context state
			bool continueNeeded = false;
			bool completeNeeded = false;

			switch (error)
			{
				case Win32.ERROR_SUCCESS:
					break;
				case SSPINative.SEC_I_CONTINUE_NEEDED:
					continueNeeded = true;
					break;
				case SSPINative.SEC_I_COMPLETE_NEEDED:
					completeNeeded = true;
					break;
				case SSPINative.SEC_I_COMPLETE_AND_CONTINUE:
					continueNeeded = true;
					completeNeeded = true;
					break;
				default:
					throw new SSPIException(error, "Could not accept security context");
			}

			if (completeNeeded)
			{
				// complete context
				error = SSPINative.CompleteAuthToken(ref newContextHandle, outputBuffers);
				if (error < 0)
					throw new SSPIException(error, "Could not complete security context");
			}

			// get output token
			outputToken = outputBuffers.GetBuffer(0);
			outputBuffers.Dispose();

			// create context object
			SecurityContextState contextState = (continueNeeded ? SecurityContextState.ContinueNeeded : SecurityContextState.Completed);
			return new SecurityContext(credentials, newContextHandle, contextExpiry, SecurityContextType.Server, contextState);
		}

		/// <summary>
		/// Updates security context, proceeds input token and generates output token
		/// </summary>
		public void UpdateSecurityContext(
			SecurityContext context,
			SecurityContextAttributes contextAttributes,
			byte[] inputToken,
			out byte[] outputToken)
		{
			// parameters validation
			if (context == null)
				throw new ArgumentNullException("credentials");
			if (inputToken == null)
				throw new ArgumentNullException("inputToken");

			// prepare requirements for context
			uint contextReq = GetContextRequirements(context.Type == SecurityContextType.Server, contextAttributes);

			// prepare buffers
			SecurityBuffers inputBuffers = new SecurityBuffers(1);
			inputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_TOKEN, inputToken);

			SecurityBuffers outputBuffers = new SecurityBuffers(1);
			outputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_TOKEN, _secPackage.MaxToken);

			// update context
			Int64 credHandle = context.credentials.Handle;
			Int64 contextHandle = context.Handle;
			uint contextAttribs;
			int error;

			if (context.Type == SecurityContextType.Client)
			{
				error = SSPINative.InitializeSecurityContext(
					ref credHandle,
					ref contextHandle,
					null,
					contextReq,
					0,
					SSPINative.SECURITY_NETWORK_DREP,
					inputBuffers,
					0,
					IntPtr.Zero,
					outputBuffers,
					out contextAttribs,
					IntPtr.Zero);
			}
			else
			{
				error = SSPINative.AcceptSecurityContext(
					ref credHandle,
					ref contextHandle,
					inputBuffers,
					contextReq,
					SSPINative.SECURITY_NETWORK_DREP,
					IntPtr.Zero,
					outputBuffers,
					out contextAttribs,
					IntPtr.Zero);
			}

			inputBuffers.Dispose();

			// check context state
			bool continueNeeded = false;
			bool completeNeeded = false;

			switch (error)
			{
				case Win32.ERROR_SUCCESS:
					break;
				case SSPINative.SEC_I_CONTINUE_NEEDED:
					continueNeeded = true;
					break;
				case SSPINative.SEC_I_COMPLETE_NEEDED:
					completeNeeded = true;
					break;
				case SSPINative.SEC_I_COMPLETE_AND_CONTINUE:
					continueNeeded = true;
					completeNeeded = true;
					break;
				default:
					throw new SSPIException(error, "Could not update security context");
			}

			if (completeNeeded)
			{
				// complete context
				error = SSPINative.CompleteAuthToken(ref contextHandle, outputBuffers);
				if (error < 0)
					throw new SSPIException(error, "Could not complete security context");
			}

			// get output token
			outputToken = outputBuffers.GetBuffer(0);
			outputBuffers.Dispose();

			// update context object state
			if (!continueNeeded)
			{
				context.SetCompleted();
			}
		}

		/// <summary>
		/// Creates the signature of a message
		/// </summary>
		public byte[] CreateSignature(SecurityContext context, byte[] message)
		{
			// parameters validation
			if (context == null)
				throw new ArgumentNullException("context");
			if (message == null)
				throw new ArgumentNullException("message");

			// prepare buffers
			SecurityBuffers inputBuffers = new SecurityBuffers(2);
			inputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_DATA, message);
			inputBuffers.SetBuffer(1, (int)SSPINative.SECBUFFER_TOKEN, context.MaxSignature);

			// create signature
			Int64 contextHandle = context.Handle;
			int error = SSPINative.MakeSignature(
				ref contextHandle,
				0,
				inputBuffers,
				0);
			if (error < 0)
				throw new SSPIException(error, "Could not create signature");

			// get signature
			byte[] signature = inputBuffers.GetBuffer(1);
			inputBuffers.Dispose();

			return signature;
		}

		/// <summary>
		/// Verifies the signature of a message
		/// </summary>
		public void VerifySignature(SecurityContext context, byte[] message, byte[] signature)
		{
			// parameters validation
			if (context == null)
				throw new ArgumentNullException("context");
			if (message == null)
				throw new ArgumentNullException("message");
			if (signature == null)
				throw new ArgumentNullException("signature");

			// prepare buffers
			SecurityBuffers inputBuffers = new SecurityBuffers(2);
			inputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_DATA, message);
			inputBuffers.SetBuffer(1, (int)SSPINative.SECBUFFER_TOKEN, signature);

			// verify signature
			Int64 contextHandle = context.Handle;
			int error = SSPINative.VerifySignature(
				ref contextHandle,
				inputBuffers,
				0,
				0);

			if (error < 0)
			{
				switch (error)
				{
					case SSPINative.SEC_E_MESSAGE_ALTERED:
						throw new SSPIException(error, "The message or signature supplied for verification has been altered");
					case SSPINative.SEC_E_OUT_OF_SEQUENCE:
						throw new SSPIException(error, "The message supplied for verification is out of sequence");
					default:
						throw new SSPIException(error, "Could not verify message signature");
				};
			}
		}

		/// <summary>
		/// Encrypts message
		/// </summary>
		public byte[] EncryptMessage(SecurityContext context, byte[] message)
		{
			// parameters validation
			if (context == null)
				throw new ArgumentNullException("context");
			if (message == null)
				throw new ArgumentNullException("message");

			// prepare buffers
			SecurityBuffers inputBuffers = new SecurityBuffers(2);
			inputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_DATA, message);
			inputBuffers.SetBuffer(1, (int)SSPINative.SECBUFFER_TOKEN, context.SecurityTrailer);

			// encrypt message
			Int64 contextHandle = context.Handle;
			int error = SSPINative.EncryptMessage(
				ref contextHandle,
				0,
				inputBuffers,
				0);
			if (error < 0)
				throw new SSPIException(error, "Could not encrypt message");

			// get encrypted data and trailer
			byte[] encrypted = inputBuffers.GetBuffer(0);
			byte[] trailer = inputBuffers.GetBuffer(1);
			inputBuffers.Dispose();

			// create encrypted buffer
			return CreateEncryptedBuffer(encrypted, trailer);
		}

		/// <summary>
		/// Decrypts message
		/// </summary>
		public byte[] DecryptMessage(SecurityContext context, byte[] encBuffer)
		{
			// parameters validation
			if (context == null)
				throw new ArgumentNullException("context");
			if (encBuffer == null)
				throw new ArgumentNullException("encMessage");

			// parse encrypted buffer
			byte[] encrypted;
			byte[] trailer;
			ParseEncryptedBuffer(encBuffer, out encrypted, out trailer);

			// prepare buffers
			SecurityBuffers inputBuffers = new SecurityBuffers(2);
			inputBuffers.SetBuffer(0, (int)SSPINative.SECBUFFER_DATA, encrypted);
			inputBuffers.SetBuffer(1, (int)SSPINative.SECBUFFER_TOKEN, trailer);

			// encrypt message
			Int64 contextHandle = context.Handle;
			int error = SSPINative.DecryptMessage(
				ref contextHandle,
				inputBuffers,
				0,
				0);
			if (error < 0)
				throw new SSPIException(error, "Could not decrypt message");

			// get decrypted message
			byte[] message = inputBuffers.GetBuffer(0);
			inputBuffers.Dispose();

			return message;
		}

		#endregion

		/// <summary>
		/// Returns context requirements
		/// </summary>
		private static uint GetContextRequirements(bool serverContext, SecurityContextAttributes contextAttributes)
		{
			if (serverContext)
			{
				uint contextReq =
					SSPINative.ISC_REQ_USE_DCE_STYLE   |
					SSPINative.ISC_REQ_CONFIDENTIALITY |
					SSPINative.ISC_REQ_REPLAY_DETECT   |
					SSPINative.ISC_REQ_SEQUENCE_DETECT |
					SSPINative.ISC_REQ_CONNECTION;

				if ((contextAttributes & SecurityContextAttributes.Delegate) != 0)
					contextReq = contextReq | SSPINative.ISC_REQ_DELEGATE | SSPINative.ISC_REQ_MUTUAL_AUTH;

				if ((contextAttributes & SecurityContextAttributes.MutualAuthentication) != 0)
					contextReq = contextReq | SSPINative.ISC_REQ_MUTUAL_AUTH;

				if ((contextAttributes & SecurityContextAttributes.Identify) != 0)
					contextReq = contextReq | SSPINative.ISC_REQ_IDENTIFY;

				return contextReq;
			}
			else
			{
				uint contextReq =
					SSPINative.ASC_REQ_USE_DCE_STYLE   |
					SSPINative.ASC_REQ_CONFIDENTIALITY |
					SSPINative.ASC_REQ_REPLAY_DETECT   |
					SSPINative.ASC_REQ_SEQUENCE_DETECT |
					SSPINative.ASC_REQ_CONNECTION;

				if ((contextAttributes & SecurityContextAttributes.Delegate) != 0)
					contextReq = contextReq | SSPINative.ASC_REQ_DELEGATE | SSPINative.ASC_REQ_MUTUAL_AUTH;

				if ((contextAttributes & SecurityContextAttributes.MutualAuthentication) != 0)
					contextReq = contextReq | SSPINative.ASC_REQ_MUTUAL_AUTH;

				if ((contextAttributes & SecurityContextAttributes.Identify) != 0)
					contextReq = contextReq | SSPINative.ASC_REQ_IDENTIFY;

				return contextReq;
			}
		}

		/// <summary>
		/// Creates encrypted buffer in format MESSAGE_LENGTH|ENCRYPTED_MESSAGE|MESSAGE_TRAILER
		/// </summary>
		private static byte[] CreateEncryptedBuffer(byte[] encrypted, byte[] trailer)
		{
			int encryptedLen = encrypted.Length;
			int trailerLen = trailer.Length;

			byte[] encBuffer = new byte[4 + encryptedLen + trailerLen];
			GCHandle gcEncBuffer = GCHandle.Alloc(encBuffer, GCHandleType.Pinned);

			try
			{
				IntPtr sizePtr = gcEncBuffer.AddrOfPinnedObject();
				Marshal.WriteInt32(sizePtr, encryptedLen);

				IntPtr encryptedPtr = IntPtrHelper.Add(sizePtr, 4);
				Marshal.Copy(encrypted, 0, encryptedPtr, encryptedLen);

				IntPtr trailerPtr = IntPtrHelper.Add(encryptedPtr, encryptedLen);
				Marshal.Copy(trailer, 0, trailerPtr, trailerLen);

				return encBuffer;
			}
			finally
			{
				gcEncBuffer.Free();
			}
		}

		/// <summary>
		/// Parses encrypted buffer in format MESSAGE_LENGTH|ENCRYPTED_MESSAGE|MESSAGE_TRAILER
		/// </summary>
		private static void ParseEncryptedBuffer(byte[] encBuffer, out byte[] encrypted, out byte[] trailer)
		{
			GCHandle gcEncBuffer = GCHandle.Alloc(encBuffer, GCHandleType.Pinned);

			try
			{
				IntPtr sizePtr = gcEncBuffer.AddrOfPinnedObject();
				int encryptedLen = Marshal.ReadInt32(sizePtr);

				int trailerLen = encBuffer.Length - 4 - encryptedLen;
				if (trailerLen < 0)
					throw new ArgumentOutOfRangeException("encBuffer.Length");

				IntPtr encryptedPtr = IntPtrHelper.Add(sizePtr, 4);
				encrypted = new byte[encryptedLen];
				Marshal.Copy(encryptedPtr, encrypted, 0, encryptedLen);

				IntPtr trailerPtr = IntPtrHelper.Add(encryptedPtr, encryptedLen);
				trailer = new byte[trailerLen];
				Marshal.Copy(trailerPtr, trailer, 0, trailerLen);
			}
			finally
			{
				gcEncBuffer.Free();
			}
		}
	}
}