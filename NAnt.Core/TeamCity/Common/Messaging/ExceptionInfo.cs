using System;

namespace NAntExtensions.TeamCity.Common.Messaging
{
	public class ExceptionInfo
	{
		public string Type
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public string Source
		{
			get;
			set;
		}

		public string StackTrace
		{
			get;
			set;
		}

		public ExceptionInfo InnerException
		{
			get;
			set;
		}

		public static ExceptionInfo FromException(Exception exception)
		{
			if (exception == null)
			{
				return null;
			}

			ExceptionInfo result = new ExceptionInfo
			                       {
			                       	Type = exception.GetType().FullName,
			                       	Message = exception.Message,
			                       	Source = exception.Source,
			                       	StackTrace = exception.StackTrace
			                       };

			if (exception.InnerException != null)
			{
				result.InnerException = FromException(exception.InnerException);
			}

			return result;
		}
	}
}