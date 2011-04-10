//--------------------------------------------------------------------------
// <copyright file="Schema.cs" company="James Eggers">
//  Copyright (c) James Eggers All rights reserved.
// </copyright>
// <author> James Eggers </author>
// <description>
//  This file contains the code associated with the Schema element used by 
//  the SchemaSpy Task for NAnt.
// </description>
//--------------------------------------------------------------------------
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.SchemaSpy.Tasks
{
    /// <summary>
    /// An element for each schema to analyze and document.
    /// </summary>
    [ElementName("schema")]
    public class Schema : Element
    {
        /// <summary>
        /// Gets or sets the name of the schema to analyze and document.
        /// </summary>
        [TaskAttribute("schemaName")]
        [StringValidator(AllowEmpty = false)]
        public string SchemaName { get; set; }
    }
}