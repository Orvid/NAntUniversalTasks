using System;
using System.Text;

namespace Snak.Utilities
{
	/// <summary>
	/// provides a useful tool for building command line commands.
	/// </summary>
	internal class CommandLineBuilder
	{
		private StringBuilder commandStringBuilder = new StringBuilder();

		internal CommandLineBuilder() { }

        /// <summary>
        /// adds a switch to the command. eg. /verbose
        /// </summary>
        /// <param name="switchName"></param>
        /// <example>
        /// <![CDATA[
        /// CommandLineBuilder builder = new CommandLineBuilder();
        /// builder.AppendSwitch ( "/nologo" );
        /// ]]>
        /// </example>
		internal void AppendSwitch(string switchName)
		{
			AddSwitchName(switchName);
		}

        /// <summary>
        /// Appends the given switch if the switchValue is not null or empty
        /// </summary>
        /// <param name="switchName"></param>
        /// <param name="switchValue"></param>
        /// <example>
        /// <![CDATA[
        /// CommandLineBuilder builder = new CommandLineBuilder();
        /// builder.AppendSwitchIfNotNullOrEmpty ( "/test:" ,  "a string that is obviously not null or empty" );
        /// ]]>
        /// </example>
		internal void AppendSwitchIfNotNullOrEmpty(string switchName, string switchValue )
		{
			if (switchValue != null && switchValue != String.Empty)
			{
				AddSwitchName(switchName);
				AppendToCommand(switchValue);
			}
		}

        /// <summary>
        /// Appends a switch with mutiple values to the command
        /// </summary>
        /// <param name="switchName"></param>
        /// <param name="switchValues"></param>
        /// <param name="delimeter"></param>
        /// <example>
        /// <![CDATA[
        /// CommandLineBuilder builder = new CommandLineBuilder();
        /// builder.AppendSwitchesIfNotNullOrEmpty( "/test:" ,  new string[] {"a", "b", "c"}, ",");
        /// builder.GetCommand();
        /// // would produce /test:a,b,c
        /// ]]>
        /// </example>
		internal void AppendSwitchesIfNotNullOrEmpty(string switchName, string[] switchValues, string delimeter )
		{
			if (switchValues.Length == 0)
				throw new InvalidOperationException("You cannot pass an array of empty values to " + System.Reflection.MethodInfo.GetCurrentMethod().Name);

			if (switchValues[0] != null && switchValues[0] != String.Empty)
			{
				AddSwitchName(switchName);

				for (int i = 0; i < switchValues.Length; i++)
				{
					if (switchValues[i] != null && switchValues[i] != String.Empty)
					{ 
						AppendToCommand(switchValues[i]);
				
						if (i < switchValues.Length && (switchValues[i+1] != null && switchValues[i+1] != String.Empty))
							AppendToCommand(delimeter);
					}
				}
			}
		}

		private void AddSwitchName(string switchName)
		{
			AppendToCommand(" ");
			AppendToCommand(switchName);
		}

		private void AppendToCommand(string theValue)
		{
			commandStringBuilder.Append(theValue);
		}

		public string GetCommand()
		{
			return commandStringBuilder.ToString();
		}
	}
}
