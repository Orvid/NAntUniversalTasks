//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

using DocsVision.IO;
using DocsVision.Util;
using DocsVision.Runtime.Remoting.Transport;

namespace DocsVision.Runtime.Remoting.Channels
{
	internal sealed class BinaryConnection : IConnection
	{
		// Connection counter
		private static Int64 _connectionCounter = 0;
		private Int64 _connectionId;

		// Channel
		private IChannel _channel;

		// Network transport
		private ITransport _transport;

		// We use buffered wrapper to improve performance
		private BufferedStreamEx _bufferedStream;
		private bool _directAccess;

		// We use BinaryReader/Writer for typed IO
		private BinaryReader _reader;
		private BinaryWriter _writer;

		#region Constructors

		public BinaryConnection(IChannel channel, TransportClient client)
		{
			_connectionId = Interlocked.Increment(ref _connectionCounter);
			_channel = channel;
			_transport = client.Transport;
			_bufferedStream = new BufferedStreamEx(client.GetStream());
			_reader = new BinaryReader(_bufferedStream, Encoding.UTF8);
			_writer = new BinaryWriter(_bufferedStream, Encoding.UTF8);
		}

		#endregion

		#region IConnection Members

		public void BeginReceive(DataAvailableCallback callback, object state)
		{
			// begin asynchronous receive operation
			_bufferedStream.FillBuffer(callback, state);
		}

		public void ReceiveRequest(out ITransportHeaders requestHeaders, out Stream requestStream)
		{
			// transport signature
			if (!MatchPreamble())
				BinaryWireProtocol.ThrowException(BinaryWireProtocol.StatusCode.InvalidMessageFormat);
			// operation opcode
			byte operation = _reader.ReadByte();
			if (operation != BinaryWireProtocol.OperationType.Request && operation != BinaryWireProtocol.OperationType.OneWayRequest)
				BinaryWireProtocol.ThrowException(BinaryWireProtocol.StatusCode.InvalidMessageFormat);
			// content length
			int contentLength = _reader.ReadInt32();
			// request uri
			string requestUri = _reader.ReadString();
			if (!CheckRequestUri(requestUri))
				BinaryWireProtocol.ThrowException(BinaryWireProtocol.StatusCode.InvalidRequestUri);
			// request headers
			requestHeaders = ReadHeaders();
			// set special headers
			requestHeaders[BinaryWireProtocol.WellKnownHeaders.ConnectionId] = _connectionId;
			requestHeaders[BinaryWireProtocol.WellKnownHeaders.RequestUri] = requestUri;

			// create stream for request reading
			if (contentLength == -1)
			{
				requestStream = new ChunkedReadStream(_bufferedStream);
			}
			else
			{
				requestStream = new FixedReadStream(_bufferedStream, contentLength);
			}

			// set client principal
			RemotingService.ClientPrincipal = _transport.ClientPrincipal;
		}

		public void ReceiveResponse(out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			// transport signature
			if (!MatchPreamble())
				BinaryWireProtocol.ThrowException(BinaryWireProtocol.StatusCode.InvalidMessageFormat);
			// operation opcode
			byte operation = _reader.ReadByte();
			if (operation != BinaryWireProtocol.OperationType.Reply)
				BinaryWireProtocol.ThrowException(BinaryWireProtocol.StatusCode.InvalidMessageFormat);
			// content length
			int contentLength = _reader.ReadInt32();
			// response headers
			responseHeaders = ReadHeaders();
			// set special headers
			responseHeaders[BinaryWireProtocol.WellKnownHeaders.ConnectionId] = _connectionId;

			// create stream for response reading
			if (contentLength == -1)
			{
				responseStream = new ChunkedReadStream(_bufferedStream);
			}
			else
			{
				responseStream = new FixedReadStream(_bufferedStream, contentLength);
			}
		}

		public Stream GetRequestStream(IMethodCallMessage requestMsg, ITransportHeaders requestHeaders)
		{
			// transport signature
			_writer.Write(BinaryWireProtocol.ClientNameAndVersion);
			// operation opcode
			_writer.Write(RemotingServices.IsOneWay(requestMsg.MethodBase) ? BinaryWireProtocol.OperationType.OneWayRequest: BinaryWireProtocol.OperationType.Request);
			// content length
			_writer.Write((Int32)(-1));
			// request uri
			_writer.Write(GetRequestUri(requestMsg));
			// request headers
			WriteHeaders(requestHeaders);

			// create stream for writing request
			_directAccess = true;
			return new ChunkedWriteStream(_bufferedStream);
		}

		public void SendRequest(IMethodCallMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream)
		{
			if (!_directAccess)
			{
				// transport signature
				_writer.Write(BinaryWireProtocol.ClientNameAndVersion);
				// operation opcode
				_writer.Write(RemotingServices.IsOneWay(requestMsg.MethodBase) ? BinaryWireProtocol.OperationType.OneWayRequest: BinaryWireProtocol.OperationType.Request);
				// content length
				_writer.Write((Int32)requestStream.Length);
				// request uri
				_writer.Write(GetRequestUri(requestMsg));
				// request headers
				WriteHeaders(requestHeaders);
				// request content
				StreamHelper.CopyStream(requestStream, _bufferedStream);
			}
			else
			{
				_directAccess = false;
			}

			requestStream.Close();
			_bufferedStream.Flush();
		}

		public Stream GetResponseStream(IMethodReturnMessage responseMsg, ITransportHeaders responseHeaders)
		{
			// transport signature
			_writer.Write(BinaryWireProtocol.ClientNameAndVersion);
			// operation opcode
			_writer.Write(BinaryWireProtocol.OperationType.Reply);
			// content length
			_writer.Write((Int32)(-1));
			// status code
			_writer.Write(BinaryWireProtocol.HeaderType.StatusCode);
			_writer.Write(responseMsg.Exception == null ? BinaryWireProtocol.StatusCode.Success : BinaryWireProtocol.StatusCode.ServerError);
			// response headers
			WriteHeaders(responseHeaders);

			// create stream for writing response
			_directAccess = true;
			return new ChunkedWriteStream(_bufferedStream);
		}

		public void SendResponse(IMethodReturnMessage responseMsg, ITransportHeaders responseHeaders, Stream responseStream)
		{
			if (!_directAccess)
			{
				// transport signature
				_writer.Write(BinaryWireProtocol.ClientNameAndVersion);
				// operation opcode
				_writer.Write(BinaryWireProtocol.OperationType.Reply);
				// content length
				_writer.Write((Int32)responseStream.Length);
				// status code
				_writer.Write(BinaryWireProtocol.HeaderType.StatusCode);
				_writer.Write(responseMsg.Exception == null ? BinaryWireProtocol.StatusCode.Success : BinaryWireProtocol.StatusCode.ServerError);
				// response headers
				WriteHeaders(responseHeaders);
				// response content
				StreamHelper.CopyStream(responseStream, _bufferedStream);
			}
			else
			{
				_directAccess = false;
			}

			responseStream.Close();
			_bufferedStream.Flush();
		}

		public void SendErrorResponse(Exception ex)
		{
			// transport signature
			_writer.Write(BinaryWireProtocol.ClientNameAndVersion);
			// operation opcode
			_writer.Write(BinaryWireProtocol.OperationType.Reply);
			// content length
			_writer.Write((Int32)0);
			// status code
			_writer.Write(BinaryWireProtocol.HeaderType.StatusCode);
			if (ex is RemotingExceptionEx)
			{
				_writer.Write((ex as RemotingExceptionEx).StatusCode);
			}
			else
			{
				_writer.Write(BinaryWireProtocol.StatusCode.InternalError);
			}
			// error message
			_writer.Write(BinaryWireProtocol.HeaderType.ErrorMessage);
			_writer.Write(ex.Message);
			// end-of-headers marker
			_writer.Write(BinaryWireProtocol.HeaderType.EndOfHeaders);

			_bufferedStream.Flush();
		}

		public void Close()
		{
			// close transport stream
			_bufferedStream.Close();
		}

		#endregion

		/// <summary>
		/// Reads and verifies transport preamble
		/// </summary>
		private bool MatchPreamble()
		{
			for (int i = 0; i < BinaryWireProtocol.ClientNameAndVersion.Length; ++i)
			{
				if (BinaryWireProtocol.ClientNameAndVersion[i] != _reader.ReadByte())
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns request uri from method call object
		/// </summary>
		private string GetRequestUri(IMethodCallMessage methodCall)
		{
			string objectUri;
			string channelUri = _channel.Parse(methodCall.Uri, out objectUri);
			return (objectUri == null ? methodCall.Uri : objectUri);
		}

		/// <summary>
		/// Validates request URI
		/// </summary>
		private bool CheckRequestUri(string requestUri)
		{
			if (requestUri == null)
				return false;	// something wrong on the network

			Type serverType = RemotingServices.GetServerTypeForUri(requestUri);
			if (serverType == null)
				return false;	// bad request or server type if not registered

			return true;
		}

		/// <summary>
		/// Writes transport headers
		/// </summary>
		private void WriteHeaders(ITransportHeaders headers)
		{
			if (headers != null)
			{
				// proceed headers
				foreach (DictionaryEntry header in headers)
				{
					string headerName = (string)header.Key;
					if (!BinaryWireProtocol.IsLocalHeader(headerName))
					{
						// check for well-known header
						byte headerType = BinaryWireProtocol.GetWellKnownHeaderType(headerName);

						// write header type
						_writer.Write(headerType);

						if (headerType == BinaryWireProtocol.HeaderType.Custom)
						{
							// write custom header name
							_writer.Write(headerName);
						}

						// write header value
						_writer.Write(Convert.ToString(header.Value));
					}
				}
			}

			// end-of-headers marker
			_writer.Write(BinaryWireProtocol.HeaderType.EndOfHeaders);
		}

		/// <summary>
		/// Reads transport headers
		/// </summary>
		private ITransportHeaders ReadHeaders()
		{
			TransportHeaders headers = new TransportHeaders();

			// for error-header procesing
			bool errorHeader = false;
			string errorMessage = null;
			byte statusCode = BinaryWireProtocol.StatusCode.InternalError;

			// proceed headers
			string headerName = null;
			bool endOfHeaders = false;

			while (!endOfHeaders)
			{
				// read header type
				byte headerType = _reader.ReadByte();

				switch (headerType)
				{
					case BinaryWireProtocol.HeaderType.EndOfHeaders:
						// end-of-headers marker
						endOfHeaders = true;
						continue;
					case BinaryWireProtocol.HeaderType.ErrorMessage:
						// error header
						errorHeader = true;
						errorMessage = _reader.ReadString();
						continue;
					case BinaryWireProtocol.HeaderType.StatusCode:
						// status code
						statusCode = _reader.ReadByte();
						continue;
					case BinaryWireProtocol.HeaderType.Custom:
						// custom header
						headerName = _reader.ReadString();
						break;
					default:
						// well-known header
						headerName = BinaryWireProtocol.GetWellKnownHeader(headerType);
						break;
				}

				if (headerName == null)
					BinaryWireProtocol.ThrowException(BinaryWireProtocol.StatusCode.InvalidMessageFormat);

				// read header value
				headers[headerName] = _reader.ReadString();
			}

			if (errorHeader)
				BinaryWireProtocol.ThrowException(statusCode, "Internal server error: " + errorMessage);

			return headers;
		}
	}
}