//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Text;

namespace DocsVision.Runtime.Remoting.Channels
{
	internal sealed class BinaryWireProtocol
	{
		public static readonly byte[] ClientNameAndVersion = Encoding.ASCII.GetBytes("DV30");

		private BinaryWireProtocol()
		{
			// this class is non creatable
		}

		public struct OperationType
		{
			public const byte Request                = 0;   // this is client request
			public const byte OneWayRequest          = 1;   // this is client one-way request - there are no server response expected
			public const byte Reply                  = 2;   // this is server response
		}

		public struct StatusCode
		{
			public const byte Success                = 0;   // operation completed successfully
			public const byte InvalidMessageFormat   = 1;   // message format is invalid
			public const byte InvalidRequestUri      = 2;   // request uri is invalid
			public const byte AccessDenied           = 3;   // access denied
			public const byte ServerError            = 254; // server error
			public const byte InternalError          = 255; // internal server error
		}

		public struct HeaderType
		{
			public const byte ConnectionId           = 0;   // connection identifier follows
			public const byte RequestUri             = 1;   // request uri follows
			public const byte StatusCode             = 2;   // status code follows
			public const byte ErrorMessage           = 3;   // error message follows
			public const byte AuthToken              = 4;   // authentication data follows
			public const byte ContentType            = 5;   // content type follows
			public const byte Custom                 = 254; // custom header follows
			public const byte EndOfHeaders           = 255; // can appear once at end of headers
		}

		public struct WellKnownHeaders
		{
			public const string ConnectionId         = "__ConnectionId";
			public const string RequestUri           = "__RequestUri";
			public const string StatusCode           = "StatusCode";
			public const string ErrorMessage         = "ErrorMessage";
			public const string AuthToken            = "AuthToken";
			public const string ContentType          = "Content-Type";
		}

		private static readonly string[] s_WellKnownHeaders = new string[]
		{
			WellKnownHeaders.ConnectionId,
			WellKnownHeaders.RequestUri,
			WellKnownHeaders.StatusCode,
			WellKnownHeaders.ErrorMessage,
			WellKnownHeaders.AuthToken,
			WellKnownHeaders.ContentType,
		};

		private static string GetErrorMessage(byte statusCode)
		{
			switch (statusCode)
			{
				case StatusCode.AccessDenied: return "Access denied";
				case StatusCode.InvalidRequestUri: return "Request uri is invalid";
				case StatusCode.InvalidMessageFormat: return "Message format is invalid";
				case StatusCode.ServerError: return "Server error";
				case StatusCode.InternalError: return "Internal server error";
				default: return null;
			}
		}

		public static void ThrowException(byte statusCode)
		{
			throw new RemotingExceptionEx(statusCode, GetErrorMessage(statusCode));
		}

		public static void ThrowException(byte statusCode, string errorMessage)
		{
			throw new RemotingExceptionEx(statusCode, errorMessage);
		}

		public static bool IsLocalHeader(string headerName)
		{
			return headerName.StartsWith("__");
		}

		public static byte GetWellKnownHeaderType(string headerName)
		{
			for (byte headerType = 0; headerType < s_WellKnownHeaders.Length; ++headerType)
			{
				if (s_WellKnownHeaders[headerType] == headerName)
				{
					return headerType;
				}
			}

			return HeaderType.Custom;
		}

		public static string GetWellKnownHeader(byte headerType)
		{
			if (headerType < s_WellKnownHeaders.Length)
				return s_WellKnownHeaders[headerType];

			return null;	// custom header
		}
	}
}