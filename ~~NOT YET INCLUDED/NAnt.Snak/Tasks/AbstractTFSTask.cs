using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using System.Net;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Client;
using Snak.Types;
using Snak.Utilities;

namespace Snak.Tasks
{
    public abstract class AbstractTFSTask : Task
    {
        private string _server;

        /// <summary>
        ///   The name or URL of the team foundation server.  For example http://vstsb2:8080 or vstsb2 if it
        ///   has already been registered on the machine.
        /// </summary>
        [TaskAttribute("server", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        private TfsCredentials _tfsCredentials = null;

        [BuildElement("tfsCredentials", Required = false)]
        public TfsCredentials TfsCredentials
        {
            get { return _tfsCredentials; }
            set { _tfsCredentials = value; }
        }	

        private TeamFoundationServer _teamFoundationServer = null;

        /// <summary>
        /// locally cached instance of TeamFoundationServer.
        /// </summary>
        internal TeamFoundationServer TeamFoundationServer
        {
            get
            {
                if (null == _teamFoundationServer)
                {
                    Log(Level.Verbose, "Connecting to Team Foundation Server '" + this.Server + "' using the following credentials (if blank probably using CredentialCache.DefaultNetworkCredentials)- Username: '" + this.Credentials.UserName + "', Password: '" + this.Credentials.Password + "', Domain: '" + this.Credentials.Domain + "'");
                    _teamFoundationServer = new TeamFoundationServer(this.Server, this.Credentials);

                    try
                    {
                        Log(Level.Verbose, "Authenticating with the TFS at '{0}'", this.Server);
                        _teamFoundationServer.Authenticate();
                        Log(Level.Verbose, "Authenticated Successfully");
                    }
                    catch
                    {
                        Log(Level.Error, "Team Foundation Server Authentication failed");
                        _teamFoundationServer = null;

                        throw;
                    }
                }

                return _teamFoundationServer;
            }
            set
            {
                _teamFoundationServer = value;
            }
        }

        private NetworkCredential _networkCredential;

        internal NetworkCredential Credentials
        {
            get
            {
                if (null == _networkCredential)
                {
                    if (_tfsCredentials != null && _tfsCredentials.If)
                    {
                        Log(Level.Verbose, "Using supplied credentials for {0}", _tfsCredentials.Username);
                        if (!String.IsNullOrEmpty(_tfsCredentials.Domain))
                            _networkCredential = new NetworkCredential(_tfsCredentials.Username, _tfsCredentials.Password, _tfsCredentials.Domain);
                        else
                            _networkCredential = new NetworkCredential(_tfsCredentials.Username, _tfsCredentials.Password);
                    }
                    else
                    {
                        Log(Level.Verbose, "Using CredentialCache.DefaultNetworkCredentials");
                        _networkCredential = CredentialCache.DefaultNetworkCredentials;
                        //_networkCredential = CredentialCache.DefaultCredentials.GetCredential(new Uri(this.Server), "Negotiate");
                    }
                }
                return _networkCredential;
            }
            set
            {
                _networkCredential = value;
            }
        }

        private VersionControlServer _versionControlServer;

        internal VersionControlServer VersionControlServer
        {
            get
            {
                if (null == _versionControlServer)
                {
                    _versionControlServer = (VersionControlServer)this.TeamFoundationServer.GetService(typeof(VersionControlServer));
                }
                return _versionControlServer;
            }
            set
            {
                _versionControlServer = value;
            }
        }

        internal Workspace GetCurrentWorkspace()
        {
            try
            {
                return TfsUtils.GetWorkspace(Project.BaseDirectory, Server, Credentials, new NAntLoggingProxy(Log).Log);
            }
            catch (Exception err)
            {
                throw new BuildException(err.Message, Location, err);
            }
        }

        protected abstract override void ExecuteTask();
    }
}
