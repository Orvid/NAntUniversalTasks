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

namespace NAntCollections.List
{
    /// <summary>
    /// Outputs the contents of a list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily used for debugging.
    /// </para>
    /// <para>
    /// Will truncate values if necessary so that a given item fits on one line.
    /// </para>
    /// </remarks>
    /// <example>
    ///   <para>
    ///   Output the contents of MyList.
    ///   </para>
    ///   <code>
    ///     <![CDATA[
    /// <list-dump list="MyList"/>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("list-dump")]
    public class ListDump : ListBase
    {
        #region Constants
        private const int    COL_BUFFER_LENGTH = 2;
        private const char   DIVIDER_CHAR      = '-';
        private const string POS_HEADER        = "#";
        private const string VAL_HEADER        = "Value";
        #endregion

        #region Private Fields
        private int _maxPositionLength = POS_HEADER.Length;
        private int _maxValueLength = VAL_HEADER.Length;
        private int _usableWidth;
        #endregion

        #region Private Methods
        private void DetermineProperFormattingWidths()
        {
            // Determine usable width for output
            Log(Level.Verbose, "Console width:    {0}", NAntUtility.ConsoleWidth);
            Log(Level.Verbose, "Indentation size: {0}", Project.IndentationSize);
            _usableWidth = NAntUtility.ConsoleWidth - Project.IndentationSize;
            Log(Level.Verbose, "Usable width:     {0}", _usableWidth);

            // Determine max length of position column
            _maxPositionLength = List.Count.ToString().Length;

            // Determine max length of values
            foreach (string val in List)
                if (val.Length > _maxValueLength) _maxValueLength = val.Length;
            Log(Level.Verbose, "Max value length: {0}", _maxValueLength);

            // Truncate value length if need be
            if (_maxValueLength + _maxPositionLength + COL_BUFFER_LENGTH > _usableWidth)
            {
                _maxValueLength = _usableWidth - _maxPositionLength - COL_BUFFER_LENGTH;
                Log(Level.Warning, "Warning: Some values will be truncated because of length");
            }
        }

        private void WriteLine(string pos, string val)
        {
            StringBuilder line = new StringBuilder();

            // Pad left up to indentation size
            line.Append(new String(' ', Project.IndentationSize));

            // Add the key
            if (pos.Length > _maxPositionLength)
                line.Append(pos.Substring(0, _maxPositionLength));
            else
                line.Append(pos.PadLeft(_maxPositionLength));

            // Add the column buffer
            line.Append(new String(' ', COL_BUFFER_LENGTH));

            // Add the value
            if (val.Length > _maxValueLength)
                line.Append(val.Substring(0, _maxValueLength));
            else
                line.Append(val.PadRight(_maxValueLength));

            Console.Out.WriteLine(line.ToString());
        }
        #endregion

        #region Execution
        /// <summary>
        /// Task execution.
        /// </summary>
        protected override void ExecuteTask()
        {
            base.ExecuteTask();

            DetermineProperFormattingWidths();

            if (_maxValueLength <= 0)
            {
                Log(Level.Error, "Not enough usable width to display contents");
                return;
            }

            // Write the list name
            Log(Level.Info, "Contents of list [{0}]:", ListName);

            WriteLine(POS_HEADER, VAL_HEADER);
            WriteLine(new string(DIVIDER_CHAR, _maxPositionLength), new string(DIVIDER_CHAR, _maxValueLength));

            // Write the items
            int i = 0;
            foreach (string val in List)
            {
                int pos = i++;
                WriteLine(pos.ToString(), val);
            }
        }
        #endregion
    }
}
