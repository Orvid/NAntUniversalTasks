#region Copyright and Licensing
//
// NAntCollections
// Copyright © 2007 Justin Kohlhepp
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301 USA
//
// Justin Kohlhepp (justin@vsxp.com)
//
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAntCollections.Utility;
#endregion

namespace NAntCollections.Dictionary
{
	/// <summary>
	/// Outputs the contents of a dictionary.
	/// </summary>
    /// <remarks>
    /// <para>
    /// Primarily used for debugging.
    /// </para>
    /// <para>
    /// Will limit output of an individual item to one line, with keys having
    /// preference.  Therefore, if keys are too long it could end up that
    /// values are not displayed at all.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Output the contents of MyDict.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <dict-dump dictionary="MyDict"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("dict-dump")]
	public class DictionaryDumpTask : DictionaryBase
    {
        #region Constants
        private const int    COL_BUFFER_LENGTH = 2;
        private const char   DIVIDER_CHAR      = '-';
        private const string KEY_HEADER        = "Key";
        private const string VAL_HEADER        = "Value";
        #endregion

        #region Private Fields
        private int _maxKeyLength = KEY_HEADER.Length;
        private int _maxValueLength = VAL_HEADER.Length;
        private int _usableWidth;
        #endregion

        #region Private Methods
        /// <summary>
        /// Loop through the dictionary and examine the console to figure
        /// out the right widths for formatting output.
        /// </summary>
        private void DetermineProperFormattingWidths()
        {
            // Determine usable width for output
            Log(Level.Verbose, "Console width:    {0}", NAntUtility.ConsoleWidth);
            Log(Level.Verbose, "Indentation size: {0}", Project.IndentationSize);
            _usableWidth = NAntUtility.ConsoleWidth - Project.IndentationSize;
            Log(Level.Verbose, "Usable width:     {0}", _usableWidth);

            // Determine max lengths of keys and values
            foreach (string key in Dictionary.Keys)
            {
                string val = Dictionary[key];
                if (key.Length > _maxKeyLength)   _maxKeyLength   = key.Length;
                if (val.Length > _maxValueLength) _maxValueLength = val.Length;
            }
            Log(Level.Verbose, "Max key length:   {0}", _maxKeyLength);
            Log(Level.Verbose, "Max value length: {0}", _maxValueLength);

            // See if we have enough room to fit everything, and if
            // we don't, set the lengths so we do
            if ((_maxKeyLength + COL_BUFFER_LENGTH + _maxValueLength) > _usableWidth)
            {
                // If max value length ends up <= 0 then it doesn't get displayed
                int prevMaxValLen = _maxValueLength;
                _maxValueLength = _usableWidth - _maxKeyLength - COL_BUFFER_LENGTH;
                if (prevMaxValLen > _maxValueLength)
                    Log(Level.Warning, "Warning: Some values will be truncated because of length");
                if (_maxValueLength <= 0)
                    Log(Level.Warning, "Warning: Cannot output values of dictionary because keys are too long");

                // If max key length is larger than the available buffer 
                // we can only display up to the end of the buffer
                if (_maxKeyLength > _usableWidth)
                {
                    _maxKeyLength = _usableWidth;
                    Log(Level.Warning, "Warning: Some key names will be truncated because of length");
                }
            }
            Log(Level.Verbose, "Adjusted max key length:   {0}", _maxKeyLength);
            Log(Level.Verbose, "Adjusted max value length: {0}", _maxValueLength);
        }

        /// <summary>
        /// Output a line for specified key and value.
        /// </summary>
        private void WriteLine(string key, string val)
        {
            StringBuilder line = new StringBuilder();

            // Pad left up to indentation size
            line.Append(new String(' ', Project.IndentationSize));

            // Add the key
            if (key.Length > _maxKeyLength)
                line.Append(key.Substring(0, _maxKeyLength));
            else
                line.Append(key.PadRight(_maxKeyLength));

            // Make sure we have the room to output the value
            if (_maxValueLength > 0)
            {
                // Add the column buffer
                line.Append(new String(' ', COL_BUFFER_LENGTH));

                // Add the value
                if (val.Length > _maxValueLength)
                    line.Append(val.Substring(0, _maxValueLength));
                else
                    line.Append(val.PadRight(_maxValueLength));
            }

            Console.Out.WriteLine(line.ToString());
        }
        #endregion

        #region Execution
        /// <summary>
        /// Execute the task.
        /// </summary>
		protected override void ExecuteTask()
		{
			base.ExecuteTask();

            DetermineProperFormattingWidths();

            // Write the dictionary name
			Log(Level.Info, "Contents of dictionary [{0}]:", DictionaryName);

            // Write the header labels
            string keyDivider = new string(DIVIDER_CHAR, _maxKeyLength);
            string valDivider = (_maxValueLength <= 0) ? "" : new string(DIVIDER_CHAR, _maxValueLength);
            WriteLine(KEY_HEADER, VAL_HEADER);
            WriteLine(keyDivider, valDivider);

            // Write the items
			foreach (string key in Dictionary.Keys)
                WriteLine(key, Dictionary[key]);
		}
		#endregion
	}
}
