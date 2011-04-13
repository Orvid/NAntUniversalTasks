//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.Text.RegularExpressions;

namespace DocsVision.Net.Pipes
{
	public class PipeName
	{
		private static Regex s_PipeName = new Regex(@"\\\\([^\\]*)\\pipe\\(.*)");
		private string _name = @"\\.\pipe\";
		private string _host = ".";
		private string _pipe = string.Empty;

		#region Constructors

		public PipeName(string host, string pipe)
		{
			_name = @"\\" + host + @"\pipe\" + pipe;
			_host = host;
			_pipe = pipe;
		}

		public PipeName(string pipeName)
		{
			Match m = s_PipeName.Match(pipeName);
			if (!m.Success)
				throw new ArgumentException("Invalid pipe name");

			_name = pipeName;
			_host = m.Groups[1].Value;
			_pipe = m.Groups[2].Value;
		}

		#endregion

		#region Properties

		public string Host
		{
			get {	return _host; }
		}

		public string Pipe
		{
			get {	return _pipe; }
		}

		public bool IsLocal
		{
			get { return (_host == "."); }
		}

		#endregion

		public override string ToString()
		{
			return _name;
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}
	}
}