using System;
using System.Text;

using NAnt.Core;

using NAntExtensions.TeamCity.Common.BuildEnvironment;
using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.TeamCity.Common.Messaging
{
	internal class DefaultTeamCityLogWriter : TeamCityLogWriter
	{
		static UnicodeEncoding UnicodeEncoding;
		readonly StringBuilder _builder = new StringBuilder();
		IBuildEnvironment _buildEnvironment;
		bool _isOpen;

		public DefaultTeamCityLogWriter(IBuildEnvironment buildEnvironment)
		{
			BuildEnvironment = buildEnvironment;

			_isOpen = true;
		}

		bool UseTaskLogger
		{
			get { return BuildEnvironment.IsRunningWithTeamCityNAntRunner; }
		}

		public override Encoding Encoding
		{
			get
			{
				if (UnicodeEncoding == null)
				{
					UnicodeEncoding = new UnicodeEncoding(false, false);
				}
				return UnicodeEncoding;
			}
		}

		IBuildEnvironment BuildEnvironment
		{
			get { return _buildEnvironment; }
			set
			{
				Ensure.ArgumentIsNotNull(value, "value");
				_buildEnvironment = value;
			}
		}

		public override void Close()
		{
			if (_isOpen)
			{
				Flush();
			}
			Dispose(true);
		}

		protected override void Dispose(bool disposing)
		{
			_isOpen = false;
			base.Dispose(disposing);
		}

		public override void WriteLine()
		{
			Flush();
		}

		public override void WriteLine(string value)
		{
			Write(value);
			Flush();
		}

		public override void Write(char value)
		{
			CheckWriterIsOpened();

			_builder.Append(value);
		}

		public override void Write(string value)
		{
			CheckWriterIsOpened();

			if (value != null)
			{
				_builder.Append(value);
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			CheckWriterIsOpened();

			Ensure.ArgumentIsNotNull(buffer, "buffer");
			Ensure.That<ArgumentOutOfRangeException>(index>=0, "The index must be greater or equal to 0");
			Ensure.That<ArgumentOutOfRangeException>(count>=0, "The count must be greater or equal to 0");
			Ensure.That<ArgumentException>(buffer.Length - index >= count, "Invalid combination of offset and length");

			_builder.Append(buffer, index, count);
		}

		public override void Flush()
		{
			CheckWriterIsOpened();

			if (_builder.Length > 0)
			{
				if (UseTaskLogger)
				{
					TaskToUseForLogging.Log(Level.Info, _builder.ToString());
				}
				else
				{
					Console.WriteLine(_builder.ToString());
				}

				_builder.Length = 0;
			}
		}

		void CheckWriterIsOpened()
		{
			if (!_isOpen)
			{
				throw new ObjectDisposedException(null, "Writer is closed");
			}
		}
	}
}