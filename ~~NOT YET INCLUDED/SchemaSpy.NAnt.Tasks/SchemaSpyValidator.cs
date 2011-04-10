//--------------------------------------------------------------------------
// <copyright file="SchemaSpyValidator.cs" company="James Eggers">
//  Copyright (c) James Eggers All rights reserved.
// </copyright>
// <author> James Eggers </author>
// <description>
//  This file contains the code used by the SchemaSpy task to validate the attributes
//  beyond what the NAnt.Core validations offers.
// </description>
//--------------------------------------------------------------------------
using System.IO;

namespace NAnt.SchemaSpy.Tasks
{
    /// <summary>
    /// This class holds the validation rules for the SchemaSpy Task.
    /// </summary>
    internal class SchemaSpyValidator
    {
        /// <summary>
        /// Validates the Jar file for SchemaSpy exists.
        /// </summary>
        /// <param name="path">The Path to the SchemaSpy JAR file.</param>
        /// <returns>True if the jar file is found, else false.</returns>
        internal bool ValidateJarPath(string path)
        {
            // Verify the jar file exists.
            if (!File.Exists(path))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that the directory for the Database Driver path specified is provided.
        /// </summary>
        /// <param name="directory">The Diretory that holds the Database Driver.</param>
        /// <returns>True if the directory exists, else false.</returns>
        internal bool ValidateDriverPath(string directory)
        {
            // Verify the driver path exists.
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the Logon Credentials to ensure either sso or a username AND password are provided.
        /// </summary>
        /// <param name="singleSignOn">A value indicating whether to use the Single Sign On option or not.</param>
        /// <param name="userName">The Login User Name to use.</param>
        /// <param name="password">The Login Password to use.</param>
        /// <returns>True if the credential options are setup properly, else false. </returns>
        internal bool ValidateLogonCredentials(bool singleSignOn, string userName, string password)
        {
            // Can have SSO OR Username/password but not all or none.
            if ((!singleSignOn && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password)) ||
                (singleSignOn && (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(password))))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a schema option is present amongst the options.
        /// </summary>
        /// <param name="allSchemas">A boolean value indicating whether or not all schemas will be analyzed and documented.</param>
        /// <param name="schemaName">A string value of the individual schema to be analyzed and documented.</param>
        /// <param name="schemaCollection">A collection of schemas provided in a multi-schema selection</param>
        /// <returns>True if a valid schema option is present or not.</returns>
        internal bool ValidateSchemaOptions(bool allSchemas, string schemaName, SchemaCollection schemaCollection)
        {
            if (!allSchemas && string.IsNullOrEmpty(schemaName) && schemaCollection.Count <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
