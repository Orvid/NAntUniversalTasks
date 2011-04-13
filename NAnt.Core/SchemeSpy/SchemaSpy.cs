//--------------------------------------------------------------------------
// <copyright file="SchemaSpy.cs" company="James Eggers">
//  Copyright (c) James Eggers All rights reserved.
// </copyright>
// <author> James Eggers </author>
// <description>
//  This file contains the code associated with the SchemaSpy custom task for NAnt.
// </description>
//--------------------------------------------------------------------------
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;

namespace NAnt.SchemaSpy.Tasks
{
    /// <summary>
    /// A custom NAnt task for using the SchemaSpy database documentation application.
    /// </summary>
    [TaskName("schemaSpy")]
    public class SchemaSpy : ExternalProgramBase
    {
        #region Task Attributes/Properties

        /// <summary>
        /// Instantiates a default collection to add elements to.
        /// </summary>
        private SchemaCollection _schemaCollection = new SchemaCollection();

        /// <summary>
        /// Instantiates a default collection to add column elements to.
        /// </summary>
        private ColumnRelationshipExclusionCollection _excludedColumnCollection = new ColumnRelationshipExclusionCollection();

        /// <summary>
        /// Gets or sets the location of the SchemaSpy jar file.
        /// </summary>
        [TaskAttribute("jarPath", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string JarPath { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the database to document.
        /// </summary>
        [TaskAttribute("dbType", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string DbType { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the database server.
        /// </summary>
        [TaskAttribute("host", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the port used for communicating with the database.
        /// </summary>
        [TaskAttribute("port", Required = true)]
        [Int32Validator(1, 65535)]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to document.
        /// </summary>
        [TaskAttribute("dbName", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string DbName { get; set; }

        /// <summary>
        /// Gets or sets the user name to use when accessing the database if single sign on is not used.
        /// </summary>
        [TaskAttribute("userName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password to use when accessing the database if single sign on is not used.
        /// </summary>
        [TaskAttribute("password", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the schema name (i.e. dbo) of the database to document.
        /// </summary>
        [TaskAttribute("schemaName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the collection of schemas to use.
        /// </summary>
        [BuildElementCollection("schemas", "schema", Required = false)]
        public SchemaCollection Schemas 
        {
            get { return _schemaCollection; }
            set { _schemaCollection = value; }
        }

        /// <summary>
        /// Gets or sets the directory to place the documentation once it is generated.
        /// </summary>
        [TaskAttribute("outputDirectory", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the location for the Database Driver to use.
        /// </summary>
        [TaskAttribute("driverPath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string DriverPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the Single Sign On option or not.
        /// </summary>
        [TaskAttribute("singleSignOn", Required = false)]
        [BooleanValidator()]
        public bool SingleSignOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to analyze and document all schemas.
        /// </summary>
        [TaskAttribute("allSchemas", Required = false)]
        [BooleanValidator()]
        public bool AllSchemas { get; set; }

        /// <summary>
        /// Gets or sets a description that will appear at the top of each schema page.
        /// </summary>
        [TaskAttribute("description", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets additional information for connecting to the database.
        /// </summary>
        [TaskAttribute("connectionProperties", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ConnectionProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to generate Html documentation along with the analysis.
        /// </summary>
        [TaskAttribute("noHtml", Required = false)]
        [BooleanValidator()]
        public bool NoHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to obtain a rowcount for each table analyzed.
        /// </summary>
        [TaskAttribute("noRowCount", Required = false)]
        [BooleanValidator()]
        public bool NoRowCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to allow Html located in the commentary fields of a table to not be encoded.
        /// </summary>
        [TaskAttribute("allowHtmlComments", Required = false)]
        [BooleanValidator()]
        public bool AllowHtmlComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to generate High Quality images for the documentation.
        /// </summary>
        [TaskAttribute("highQualityImages", Required = false)]
        [BooleanValidator()]
        public bool HighQualityImages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to generate Low Quality images for the documentation
        /// </summary>
        [TaskAttribute("lowQualityImages", Required = false)]
        [BooleanValidator()]
        public bool LowQualityImages { get; set; }

        /// <summary>
        /// Gets or sets the collection of schemas to use.
        /// </summary>
        [BuildElementCollection("columnRelationshipExclusions", "column", Required = false)]
        public ColumnRelationshipExclusionCollection ColumnRelationshipExclusions
        {
            get { return _excludedColumnCollection; }
            set { _excludedColumnCollection = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to imply foreign key relations while analyzing and documenting the database.
        /// </summary>
        [TaskAttribute("implyForeignKeys", Required = false)]
        [BooleanValidator()]
        public bool ImplyForeignKeys { get; set; }

        /// <summary>
        /// Gets or sets the specific tables to document and analyze
        /// </summary>
        [TaskAttribute("includeTables", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string IncludeTables { get; set; }

        /// <summary>
        /// Gets or sets the path to the meta data file that provides additional information about the tables and databases being analyzed.
        /// </summary>
        [TaskAttribute("metaFile", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string MetaFile { get; set; }

        /// <summary>
        /// The Program Arguments listing used by the ExternalProgramBase class.
        /// </summary>
        public override string ProgramArguments
        {
            get { return BuildArgumentList(); }
        }

        #endregion

        /// <summary>
        /// Executes the task's operation for NAnt.
        /// </summary>
        protected override void ExecuteTask()
        {
            bool isValid = ValidateAttributes();

            if (!isValid)
            {
                Log(Level.Error, "Task Attributes are not valid.");
                return;
            }

            this.ExeName = "java.exe";
            base.ExecuteTask();
        }

        /// <summary>
        /// Builds the string of command arguments for the executed process.
        /// </summary>
        /// <returns>A string containing the command line arguments</returns>
        private string BuildArgumentList()
        {
            // Create a new string builder.
            StringBuilder sb = new StringBuilder();

            // Append the basic arguments
            sb.Append(BuildJarPathArgument());
            sb.Append(BuildDatabaseTypeArgument());
            sb.Append(BuildHostArgument());
            sb.Append(BuildPortArgument());
            sb.Append(BuildSignOnArgument());
            sb.Append(BuildDatabaseNameArgument());
            sb.Append(BuildSchemaArgument());
            sb.Append(BuildOutputDirectoryArgument());
            sb.Append(BuildDriverPathArgument());
            sb.Append(BuildDescriptionArgument());
            sb.Append(BuildConnectionPropertiesArgument());
            sb.Append(BuildNoHtmlArgument());
            sb.Append(BuildNoRowCountArgument());
            sb.Append(BuildAllowHtmlCommentsArgument());
            sb.Append(BuildImageQualityArgument());
            sb.Append(BuildColumnRelationshipExclusionArgument());
            sb.Append(BuildImplyForeignKeysArgument());
            sb.Append(BuildIncludeTablesArgument());
            sb.Append(BuildMetaFileArgument());

            LogVerbose("Argument List: " + sb.ToString());

            return sb.ToString();
        } 

        #region Argument Related Methods

        /// <summary>
        /// Builds the argument string associated with the jar file path.
        /// </summary>
        /// <returns>A string representing jar file path to use.</returns>
        private string BuildJarPathArgument()
        {
            // Address whitespace in the filepath
            if (this.JarPath.IndexOf(' ') >= 0)
            {
                this.JarPath = "\"" + this.JarPath + "\"";
            }

            return "-jar " + this.JarPath;
        }

        /// <summary>
        /// Builds the argument string associated with the database type.
        /// </summary>
        /// <returns>A string representing database type to use.</returns>
        private string BuildDatabaseTypeArgument()
        {
            return " -t " + this.DbType;
        }

        /// <summary>
        /// Builds the argument string associated with the host server.
        /// </summary>
        /// <returns>A string representing host server to use.</returns>
        private string BuildHostArgument()
        {
            return " -host " + this.Host;
        }

        /// <summary>
        /// Builds the argument string associated with the port.
        /// </summary>
        /// <returns>A string representing port to use.</returns>
        private string BuildPortArgument()
        {
            return " -port " + this.Port;
        }

        /// <summary>
        /// Builds the argument string associated with the Sign On arguments.
        /// </summary>
        /// <returns>A string representing sign on to use.</returns>
        private string BuildSignOnArgument()
        {
            string returnValue = string.Empty;

            // Addend the logon credentials bor either single sign on or provided username/password.
            if (this.SingleSignOn)
            {
                returnValue = " -sso";
            }
            else
            {
                returnValue = BuildUserNameArgument();
                returnValue += BuildPasswordArgument();
            }

            return returnValue;
        }

        /// <summary>
        /// Builds the argument string associated with the Schema(s) to analyze and document.
        /// </summary>
        /// <returns>A string representing which schema option to use.</returns>
        private string BuildSchemaArgument()
        {
            string returnValue = string.Empty;

            if (this.AllSchemas)
            {
                returnValue = " -all";
            }
            else if (this.Schemas.Count > 0)
            {
                returnValue = " -schemas \"";
                foreach (Schema s in this.Schemas)
                {
                    returnValue += s.SchemaName + ",";
                }

                returnValue = returnValue.Remove(returnValue.Length - 1) + "\"";
            }
            else
            {
                returnValue = " -s " + this.SchemaName;
            }

            return returnValue;
        }

        /// <summary>
        /// Builds the argument string associated with the database name to analyze and document.
        /// </summary>
        /// <returns>A string representing database name to use.</returns>
        private string BuildDatabaseNameArgument()
        {
            return " -db " + this.DbName;
        }

        /// <summary>
        /// Builds the argument string associated with the user name.
        /// </summary>
        /// <returns>A string representing user name to use.</returns>
        private string BuildUserNameArgument()
        {
            return " -u " + this.UserName;
        }

        /// <summary>
        /// Builds the argument string associated with the password.
        /// </summary>
        /// <returns>A string representing password to use.</returns>
        private string BuildPasswordArgument()
        {
            return " -p " + this.Password;
        }

        /// <summary>
        /// Builds the argument string associated with the output directory.
        /// </summary>
        /// <returns>A string representing output directory to use.</returns>
        private string BuildOutputDirectoryArgument()
        {
            if (this.OutputDirectory.IndexOf(' ') >= 0)
            {
                this.OutputDirectory = "\"" + this.OutputDirectory + "\"";
            }

            return " -o " + this.OutputDirectory;
        }

        /// <summary>
        /// Builds the argument string associated with the driver path.
        /// </summary>
        /// <returns>A string representing driver path to use.</returns>
        private string BuildDriverPathArgument()
        {
            string returnValue = string.Empty;

            if (!string.IsNullOrEmpty(this.DriverPath))
            {
                if (this.DriverPath.IndexOf(' ') >= 0)
                {
                    this.DriverPath = "\"" + this.DriverPath + "\"";
                }

                returnValue = " -dp " + this.DriverPath;
            }

            return returnValue;
        }

        /// <summary>
        /// Builds the argument string assocaited with the description that is appended to the documentation.
        /// </summary>
        /// <returns>A string representing the description argument for SchemaSpy</returns>
        private string BuildDescriptionArgument()
        {
            string returnValue = string.Empty;

            if (!string.IsNullOrEmpty(this.Description))
            {
                if (this.Description.IndexOf(' ') >= 0)
                {
                    this.Description = "\"" + this.Description + "\"";
                }

                returnValue = " -desc " + this.Description;
            }

            return returnValue;
        }

        /// <summary>
        /// Builds the argument string associated with the database type.
        /// </summary>
        /// <returns>A string representing database type to use.</returns>
        private string BuildConnectionPropertiesArgument()
        {
            return (this.ConnectionProperties != string.Empty) ? " -connprops " + this.DbType : string.Empty;
        }

        /// <summary>
        /// Builds the argument string used to not generate the HTML output of the analysis.
        /// </summary>
        /// <returns>A string to signify if the Html output should be rendered or not.</returns>
        private string BuildNoHtmlArgument()
        {
            return this.NoHtml ? " -nohtml" : string.Empty;
        }

        /// <summary>
        /// Builds the argument string used to indicate whether to include the row count or not.
        /// </summary>
        /// <returns>A string to signify if the row count should be done or not.</returns>
        private string BuildNoRowCountArgument()
        {
            return this.NoRowCount ? " -norows" : string.Empty;
        }

        /// <summary>
        /// Builds the argument string to allow Html found in database related comments to be rendered as Html or encoded.
        /// </summary>
        /// <returns>A string that will tell SchemaSpy to not encode the Html-based comments or not.</returns>
        private string BuildAllowHtmlCommentsArgument()
        {
            return this.AllowHtmlComments ? " -ahic" : string.Empty;
        }

        /// <summary>
        /// Builds the argument string to determine to use High or Low quality graphics when modeling the database.
        /// </summary>
        /// <returns>A string that tells SchemaSpy which level of quality graphics to generate.</returns>
        private string BuildImageQualityArgument()
        {
            string returnValue = string.Empty;

            if (this.HighQualityImages)
            {
                returnValue = " -hq";
            }

            if (this.LowQualityImages)
            {
                returnValue = " -lq";
            }

            return returnValue;
        }

        /// <summary>
        /// Builds the column relationship exclusion argument for SchemaSpy
        /// </summary>
        /// <returns>A string indicating what columns to exclude based on the columnRelationshipExclusion elements</returns>
        private string BuildColumnRelationshipExclusionArgument()
        {
            string returnValue = string.Empty;

            if (this.ColumnRelationshipExclusions.Count > 0)
            {
                returnValue = " -x \"";

                foreach (Column c in this.ColumnRelationshipExclusions)
                {
                    returnValue += "(" + c.TableName + "." + c.ColumnName + ")|";
                }

                returnValue = returnValue.Remove(returnValue.Length - 1) + "\"";
            }

            return returnValue;
        }

        /// <summary>
        /// Builds the argument for SchemaSpy to remove implied foreign key relationships from being rendered.
        /// </summary>
        /// <returns>A string argument to prevent implied foreign key relationships from being rendered.</returns>
        private string BuildImplyForeignKeysArgument()
        {
            return (!this.ImplyForeignKeys) ? " -noimplied" : string.Empty;
        }

        /// <summary>
        /// Builds an argument to inform SchemaSpy which Tables/Views to specify only.  If not provided, then all views/tables in the provided schema will be used.
        /// </summary>
        /// <returns>A string argument telling specifying which tables and views to analyze and document.</returns>
        private string BuildIncludeTablesArgument()
        {
            string returnValue = string.Empty;

            if (!string.IsNullOrEmpty(this.IncludeTables))
            {
                if (this.IncludeTables.IndexOf(' ') >= 0)
                {
                    this.IncludeTables = "\"" + this.IncludeTables + "\"";
                }

                returnValue = " -i " + this.IncludeTables;
            }

            return returnValue;
        }

        /// <summary>
        /// Builds the argument string for SchemaSpy to use a different metafile.
        /// </summary>
        /// <returns>A string argument to have SchemaSpy use the provided MetaFile Information.</returns>
        private string BuildMetaFileArgument()
        {
            string returnValue = string.Empty;

            if (!string.IsNullOrEmpty(this.MetaFile))
            {
                if (this.MetaFile.IndexOf(' ') >= 0)
                {
                    this.MetaFile = "\"" + this.MetaFile + "\"";
                }

                returnValue = " -meta " + this.MetaFile;
            }

            return returnValue;
        }

        #endregion

        /// <summary>
        /// Validates various attributes beyond what the NAnt core offers.
        /// </summary>
        /// <returns>True if the validations pass, else throws an error explaining what failed.</returns>
        private bool ValidateAttributes()
        {
            SchemaSpyValidator validator = new SchemaSpyValidator();

            // Verify the jar file exists.
            if (!validator.ValidateJarPath(JarPath))
            {
                throw new BuildException("Cannot find file: " + this.JarPath);
            }

            // Verify the driver path exists.
            if (!validator.ValidateDriverPath(DriverPath))
            {
                throw new BuildException("Cannot find folder: " + this.DriverPath);
            }

            // Can have SSO OR Username/password but not all or none.
            if (!validator.ValidateLogonCredentials(SingleSignOn, UserName, Password))
            {
                throw new BuildException("Either Single Sign On or Username and Password must be supplied; both cannot be set at the same time.");
            }

            // Validate that one of the two options are there.
            if (!validator.ValidateSchemaOptions(AllSchemas, SchemaName, Schemas))
            {
                throw new BuildException("No schema related options have been set.  Either use the 'schemaName' or 'SchemaCollection' options to indicate which schema(s) you wish to document and analyze or the 'allSchemas' option for documenting and analyzing all accessible schemas on the database.");
            }
                      
            return true;
        }
        
        #region Logging Methods

        /// <summary>
        /// Writes a message under information mode
        /// </summary>
        /// <param name="message">The message to output</param>
        private void LogInfo(string message)
        {
            Project.Log(Level.Info, message);
        }

        /// <summary>
        /// Writes a message under warning mode
        /// </summary>
        /// <param name="message">The message to output</param>
        private void LogWarnning(string message)
        {
            Project.Log(Level.Warning, message);
        }

        /// <summary>
        /// Writes a message under error mode
        /// </summary>
        /// <param name="message">The message to output</param>
        private void LogError(string message)
        {
            Project.Log(Level.Error, message);
        }

        /// <summary>
        /// Writes a message under debug mode
        /// </summary>
        /// <param name="message">The message to output</param>
        private void LogDebug(string message)
        {
            Project.Log(Level.Debug, message);
        }

        /// <summary>
        /// Writes a message under verbose mode
        /// </summary>
        /// <param name="message">The message to output</param>
        private void LogVerbose(string message)
        {
            Project.Log(Level.Verbose, message);
        }

        #endregion
    }
}
