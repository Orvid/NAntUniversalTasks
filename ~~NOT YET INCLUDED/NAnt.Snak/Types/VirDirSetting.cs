using System;
using System.Collections.Generic;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.Core;
using Snak.Utilities;

namespace Snak.Types
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// there are really a lot of possible values we could expose here NAnt.Contrib.Tasks.Web.CreateVirtualDirectory
    /// </remarks>
    [Serializable()]
    public class VirDirSetting : Element
    {
        private string _projectName = String.Empty;
        private DotNetFrameworkInfo _dotNetFrameworkInfo = null;
        private bool _authAnonymous = true;
        private bool _authBasic;
        private bool _authNtlm;
        private bool _enableDirBrowsing;
        private string _defaultDoc = "Default.htm,Default.asp,index.htm,iisstart.asp,Default.aspx";
        private string _server;
        private string _vdirname;

        /// <summary>
        /// Specifies name of the project
        /// </summary>
        [TaskAttribute("projectName", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ProjectName
        {
            get { return _projectName; }
            set { _projectName = value; }
        }

        /// <summary>
        /// The .Net framework the web app is to run under, e.g v1.1.4322, v2.0.50727 etc
        /// </summary>
        [TaskAttribute("clrVersion", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ClrVersion
        {
            get { return (_dotNetFrameworkInfo.ClrVersion); }
            set
            {
                _dotNetFrameworkInfo = new DotNetFrameworkInfo(value);
            }
        }

        internal DotNetFrameworkVersion DotNetFrameworkVersion
        {
            get { return _dotNetFrameworkInfo.DotNetFrameworkVersion; }
            set
            {
                _dotNetFrameworkInfo = new DotNetFrameworkInfo(value);
            }
        }

        /// <summary>
        /// Specifies Anonymous authentication as one of the possible authentication
        /// schemes returned to clients as being available. The default is
        /// <see langword="true" />.
        /// </summary>
        [TaskAttribute("authanonymous")]
        public bool AuthAnonymous
        {
            get { return _authAnonymous; }
            set { _authAnonymous = value; }
        }

        /// <summary>
        /// Specifies Basic authentication as one of the possible authentication 
        /// schemes returned to clients as being available. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("authbasic")]
        public bool AuthBasic
        {
            get { return _authBasic; }
            set { _authBasic = value; }
        }

        /// <summary>
        /// Specifies Integrated Windows authentication as one of the possible 
        /// authentication schemes returned to clients as being available. The
        /// default is <see langword="false" />.
        /// </summary>
        [TaskAttribute("authntlm")]
        public bool AuthNtlm
        {
            get { return _authNtlm; }
            set { _authNtlm = value; }
        }

        /// <summary>
        /// Specifies whether directory browsing is enabled. The default is
        /// <see langword="false" />.
        /// </summary>
        [TaskAttribute("enabledirbrowsing")]
        public bool EnableDirBrowsing
        {
            get { return _enableDirBrowsing; }
            set { _enableDirBrowsing = value; }
        }

        /// <summary>
        /// One or more file names of default documents that will be returned to 
        /// the client if no file name is included in the client's request.
        /// </summary>
        [TaskAttribute("defaultdoc")]
        public string DefaultDoc
        {
            get { return _defaultDoc; }
            set { _defaultDoc = value; }
        }

        [TaskAttribute("iisserver")]
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        [TaskAttribute("vdirname")]
        public string VirtualDirectory
        {
            get { return _vdirname; }
            set{ _vdirname = value; }
        }
    }
}
