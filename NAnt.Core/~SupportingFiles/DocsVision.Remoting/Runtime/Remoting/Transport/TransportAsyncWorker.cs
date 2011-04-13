//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Threading;

namespace DocsVision.Runtime.Remoting.Transport
{
	internal sealed class TransportAsyncWorker
	{
		// Async operation types
		public enum OperationType
		{
			Connect,
			Accept,
			Send,
			Receive,
		}

		// Async objects
		private TransportAsyncResult _result;
		private AsyncCallback _callback;

		// Transport
		private ITransport _transport;

		// Operation data
		private OperationType _operation;
		private string _url;
		private byte[] _buffer;
		private int _offset;
		private int _size;

		// Operation result
		private Exception _transportException;
		private ITransport _acceptedConnection;
		private int _bytesReceived = 0;

		#region Constructors

		private TransportAsyncWorker()
		{
			// this class does not created directly
		}

		// For accept
		private TransportAsyncWorker(ITransport transport, OperationType operation, AsyncCallback callback, object state)
		{
			_transport = transport;
			_operation = operation;

			_callback = callback;
			_result = new TransportAsyncResult(this, state);
		}

		// For connect
		private TransportAsyncWorker(ITransport transport, OperationType operation, string url, AsyncCallback callback, object state) : this(transport, operation, callback, state)
		{
			_url = url;
		}

		// For send/receive
		private TransportAsyncWorker(ITransport transport, OperationType operation, byte[] buffer, int offset, int size, AsyncCallback callback, object state) : this(transport, operation, callback, state)
		{
			_buffer = buffer;
			_offset = offset;
			_size = size;
		}

		#endregion

		#region Properties

		public int BytesReceived
		{
			get
			{
				return _bytesReceived;
			}
		}

		public ITransport AcceptedConnection
		{
			get
			{
				return _acceptedConnection;
			}
		}

		public Exception Exception
		{
			get
			{
				return _transportException;
			}
		}

		#endregion

		#region Worker Methods

		private void Connect(object state)
		{
			lock (_result)
			{
				try
				{
					_transport.Connect(_url);
				}
				catch (Exception ex)
				{
					_transportException = ex;
				}
				End();
			}
		}

		private void Accept(object state)
		{
			lock (_result)
			{
				try
				{
					_acceptedConnection = _transport.Accept();
				}
				catch (Exception ex)
				{
					_transportException = ex;
				}
				End();
			}
		}

		private void Send(object state)
		{
			lock (_result)
			{
				try
				{
					_transport.Send(_buffer, _offset, _size);
				}
				catch (Exception ex)
				{
					_transportException = ex;
				}
				End();
			}
		}

		private void Receive(object state)
		{
			lock (_result)
			{
				try
				{
					_bytesReceived = _transport.Receive(_buffer, _offset, _size);
				}
				catch (Exception ex)
				{
					_transportException = ex;
				}
				End();
			}
		}

		private void End()
		{
			try
			{
				_result.SetCompleted();

				if (_callback != null)
				{
					_callback(_result);
				}
			}
			catch (Exception ex)
			{
				_transportException = ex;
			}
		}

		#endregion

		public static TransportAsyncResult Perform(ITransport transport, OperationType operation, AsyncCallback callback, object state)
		{
			return Perform(new TransportAsyncWorker(transport, operation, callback, state));
		}

		public static TransportAsyncResult Perform(ITransport transport, OperationType operation, string url, AsyncCallback callback, object state)
		{
			return Perform(new TransportAsyncWorker(transport, operation, url, callback, state));
		}

		public static TransportAsyncResult Perform(ITransport transport, OperationType operation, byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return Perform(new TransportAsyncWorker(transport, operation, buffer, offset, size, callback, state));
		}

		private static TransportAsyncResult Perform(TransportAsyncWorker worker)
		{
			WaitCallback workItem;

			switch (worker._operation)
			{
				case OperationType.Connect:
				{
					workItem = new WaitCallback(worker.Connect);
					break;
				}
				case OperationType.Accept:
				{
					workItem = new WaitCallback(worker.Accept);
					break;
				}
				case OperationType.Send:
				{
					workItem = new WaitCallback(worker.Send);
					break;
				}
				case OperationType.Receive:
				{
					workItem = new WaitCallback(worker.Receive);
					break;
				}
				default:
				{
					throw new ArgumentException("Invalid operation type", "operation");
				}
			}

			// start worker thread
			ThreadPool.QueueUserWorkItem(workItem);
			return worker._result;
		}
	}
}