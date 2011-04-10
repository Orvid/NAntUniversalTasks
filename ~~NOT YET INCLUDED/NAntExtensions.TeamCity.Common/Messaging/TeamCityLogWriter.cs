using System;
using System.Globalization;
using System.IO;

using NAnt.Core;

using NAntExtensions.TeamCity.Common.Helper;

namespace NAntExtensions.TeamCity.Common.Messaging
{
	public abstract class TeamCityLogWriter : TextWriter
	{
		Task _taskToUseForLogging;

		protected TeamCityLogWriter() : base(CultureInfo.InvariantCulture)
		{
		}

		protected internal Task TaskToUseForLogging
		{
			get
			{
				if (_taskToUseForLogging == null)
				{
					throw new InvalidOperationException(
						"The Task for the TeamCity log writer has not been assigned and therefore cannot be used. " +
						"Something has gone wrong while configuring the TeamCityMessageProvider instance.");
				}

				return _taskToUseForLogging;
			}
			set
			{
				Ensure.ArgumentIsNotNull(value, "value");
				_taskToUseForLogging = value;
			}
		}
	}
}