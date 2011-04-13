//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Collections;
using System.Threading;

namespace DocsVision.Net.Pipes
{
	internal sealed class PipeInstancePool
	{
		// Pipe wich instances are pooled
		private Pipe _pipe;

		// Instances pool
		private ArrayList _instances = new ArrayList();

		#region Constructors

		public PipeInstancePool(Pipe pipe)
		{
			// parameters validation
			if (pipe == null)
				throw new ArgumentNullException("pipe");

			_pipe = pipe;

			// first pipe instance is the first item in the pool
			StoreInstance(_pipe.Instance);
		}

		#endregion

		public PipeInstance GetInstance()
		{
			lock (_instances)
			{
				if (_instances.Count == 0)
				{
					// create a new pipe instance
					return PipeInstance.Create(_pipe.Name, false, _pipe.SecurityDescriptor);
				}
				else
				{
					// reuse pooled instance (newer one)
					PipeInstance instance = (PipeInstance)_instances[_instances.Count - 1];
					_instances.Remove(instance);
					return instance;
				}
			}
		}

		public void StoreInstance(PipeInstance instance)
		{
			// parameters validation
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (instance.IsConnected)
			{
				try
				{
					// we can reuse this instance later, so disconnect a client
					// from the pipe to allow another client to connect
					instance.DisconnectFromClient();
				}
				catch
				{
					// we lose this instance...
					instance.Close();
					return;
				}
			}

			lock (_instances)
			{
				if (_instances.Count < _pipe.PoolSize)
				{
					// store pipe instance for future use
					_instances.Add(instance);
				}
				else
				{
					// we are reach a maximum pool size, so simply close this instance
					instance.Close();
				}
			}
		}
	}
}