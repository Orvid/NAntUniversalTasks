//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;

using DocsVision.IO;

namespace DocsVision.Util
{
	internal sealed class StreamHelper
	{
		private StreamHelper()
		{
			// this class is non creatable
		}

		public static void CopyStream(Stream source, Stream target)
		{
			if (source == null || target == null)
				throw new ArgumentNullException();

			if (source is MemoryStream)
			{
				(source as MemoryStream).WriteTo(target);
			}
			else if (source is ChunkedMemoryStream)
			{
				(source as ChunkedMemoryStream).WriteTo(target);
			}
			else
			{
				byte[] buffer = ByteBufferPool.DefaultBufferPool.GetBuffer();
				int bufferSize = buffer.Length;
				int readCount = source.Read(buffer, 0, bufferSize);

				while (readCount > 0)
				{
					target.Write(buffer, 0, readCount);
					readCount = source.Read(buffer, 0, bufferSize);
				}

				ByteBufferPool.DefaultBufferPool.ReturnBuffer(buffer);
			}
		}
	}
}