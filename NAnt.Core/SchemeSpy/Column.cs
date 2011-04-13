//--------------------------------------------------------------------------
// <copyright file="Column.cs" company="James Eggers">
//  Copyright (c) James Eggers All rights reserved.
// </copyright>
// <author> James Eggers </author>
// <description>
//  This file contains the code associated with the Column element used by 
//  the SchemaSpy Task for NAnt.
// </description>
//--------------------------------------------------------------------------
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.SchemaSpy.Tasks
{
    /// <summary>
    /// This class defines the data element for the column node in the build file.
    /// </summary>
    [ElementName("column")]
    public class Column : Element
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        [TaskAttribute("name", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the name of the table the column belongs.
        /// </summary>
        [TaskAttribute("tableName", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string TableName { get; set; }
    }
}
