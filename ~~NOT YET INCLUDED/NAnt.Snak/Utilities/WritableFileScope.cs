using System;
using System.IO;

namespace Snak.Common.Utilities
{
	/// <summary>
	/// Provides a block-scope during which time a file is made writable
	/// </summary>
	public class WritableFileScope : IDisposable
	{
		private FileInfo _file;
		private FileAttributes _originalAttributes;
        private bool _exists;

		public WritableFileScope(FileInfo file)
		{
			_file = file;
            _exists = file.Exists;
            if (_exists)    // Do nothing if the file doesn't exist
            {
                _originalAttributes = _file.Attributes;
                _file.Attributes = FileAttributes.Normal;
            }
		}

		public void Dispose()
		{
		    if (_exists)
    			_file.Attributes = _originalAttributes;
		}
	}
}
