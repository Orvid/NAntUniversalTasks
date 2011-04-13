using System;
using System.DirectoryServices;
using System.Globalization;
using System.Text;
using NAnt.Contrib.Tasks.Web;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NantTasks
{
    /// <summary>
    /// This task adds or replaces a custom application mapping to an IIS virtual directory.  It does not yet add script maps to a root web site.
    /// It extends Nant.Contrib.Tasks.Web.WebBase.  
    /// </summary>
    [TaskName("iisappmap")]
    public class IisApplicationMapping : WebBase
    {
        private bool checkFileExists = true;
        private string executable;
        private string extension;
        private string verbs = string.Empty;

        /// <summary>
        /// The file extension you are mapping.  e.g. ".jpg" or ".php"
        /// </summary>
        [TaskAttribute("extension")]
        [StringValidator(AllowEmpty = false,
            Expression = @"\.[a-zA-Z0-9]+",
            ExpressionErrorMessage = "extension must be a file extension such as .pl or .jpg")]
        public string Extension
        {
            get { return extension; }
            set { extension = value; }
        }

        /// <summary>
        /// The full path to the application executable you are mapping.  e.g. "C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll"
        /// </summary>
        [TaskAttribute("executable")]
        [StringValidator(AllowEmpty = false)]
        public string Executable
        {
            get { return executable; }
            set { executable = value; }
        }

        /// <summary>
        /// The verbs to which this mapping will respond.  This is a custom string, for future flexibility, but typically contains things 
        /// like GET, POST, PUT, DELETE, TRACE.  
        /// <b>Note: </b> Exclude or leave blank to include ALL verbs.
        /// </summary>
        [TaskAttribute("verbs")]
        public string Verbs
        {
            get { return verbs; }
            set { verbs = value; }
        }

        /// <summary>
        /// True/false whether IIS will execute application even if file does not exist.  Default: <b>true</b>
        /// </summary>
        [TaskAttribute("checkfileexists")]
        [BooleanValidator]
        public bool CheckFileExists
        {
            get { return checkFileExists; }
            set { checkFileExists = value; }
        }

        private string ScriptMapping
        {
            get
            {
                StringBuilder map = new StringBuilder();
                map.Append(Extension);
                map.Append(",");
                map.Append(Executable);
                map.Append(",");
                map.Append(CheckFileExists ? "5" : "1");
                map.Append(",");
                map.Append(Verbs);
                return map.ToString();
            }
        }

        protected override void ExecuteTask()
        {
            Log(Level.Info, "Adding Application Mapping on virtual directory '{0}' on '{1}' (website: {2}):\n\"{3}\"",
                new object[] {base.VirtualDirectory, base.Server, base.Website, ScriptMapping});
            base.CheckIISSettings();
            try
            {
                DirectoryEntry vdir;
                if (base.DirectoryEntryExists(base.ServerPath + base.VdirPath))
                {
                    vdir = new DirectoryEntry(base.ServerPath + base.VdirPath);
                }
                else
                {
                    Log(Level.Error, string.Format("Could not find web site: {0}{1}", base.ServerPath, base.VdirPath));
                    throw new ArgumentException(
                        string.Format("Could not find web site: {0}{1}", base.ServerPath, base.VdirPath));
                }
                vdir.RefreshCache();
                vdir.Properties["ScriptMaps"].Add(ScriptMapping);
                vdir.CommitChanges();
                vdir.Close();
            }
            catch (Exception exception)
            {
                throw new BuildException(
                    string.Format(CultureInfo.InvariantCulture,
                                  "Error adding script mapping to virtual directory '{0}' on '{1}' (website: {2}).",
                                  new object[] {base.VirtualDirectory, base.Server, base.Website}), Location, exception);
            }
        }
    }
}