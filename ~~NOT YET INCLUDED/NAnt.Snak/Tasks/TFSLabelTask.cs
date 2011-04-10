#if(TFS)
using System;
using System.Collections.Generic;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.Contrib.Tasks.Web;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Net;
using Microsoft.TeamFoundation.Client;

namespace Snak.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Code bases on code from the VSTS plugins available at: http://vstsplugins.sourceforge.net/index.php/downloads/</remarks>
    [TaskName("tfsLabel")]
    public class TFSLabelTask : AbstractTFSTask
    {
        private string _projectPath;

        /// <summary>
        ///   The path to the project in source control, for example $\VSTSPlugins
        /// </summary>
        [TaskAttribute("projectPath", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ProjectPath
        {
            get { return _projectPath; }
            set { _projectPath = value; }
        }

        private string _labelText;

        /// <summary>
        ///  The label to apply
        /// </summary>
        /// <remarks>must be less than 64 characters, cannot end with a space or period, and cannot contain any of the following characters: "/:<>\|*?@ ---></remarks>
        [TaskAttribute("labelText", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string LabelText
        {
            get { return _labelText; }
            set { _labelText = value; }
        }

        private string _comment;

        /// <summary>
        /// The comment to apply with the label
        /// </summary>
        [TaskAttribute("comment", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        protected override void ExecuteTask()
        {
            CreateLabel(this.VersionControlServer, _labelText, this.ProjectPath, this._comment);
        }

        internal void CreateLabel(VersionControlServer versionControlServer, string labelText, string projectPath, string comment)
        {
            Log(Level.Info, "About to apply label '" + labelText + "' to project path '" + projectPath + "' with comment '" + comment + "'");

            VersionControlLabel label = new VersionControlLabel(versionControlServer, labelText, versionControlServer.AuthenticatedUser, projectPath, comment);

            // Create Label Item Spec.
            ItemSpec itemSpec = new ItemSpec(projectPath, RecursionType.Full);
            LabelItemSpec[] labelItemSpec = new LabelItemSpec[] {  
                new LabelItemSpec(itemSpec, new DateVersionSpec(DateTime.Now), false)
            };

            LabelResult[] results = versionControlServer.CreateLabel(label, labelItemSpec, LabelChildOption.Replace);

            // humm cant see any doco on what exactly CreateLabel() returns? for now Im assuming if it doesn’t throw it was successful

            Log(Level.Info, "Label applied sucessfully'");
        }
    }
}
#endif