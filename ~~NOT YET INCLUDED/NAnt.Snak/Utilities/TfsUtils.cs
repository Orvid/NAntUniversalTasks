using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using Snak.Core;
using System.Net;
using Microsoft.TeamFoundation.Client;
using System.Threading;

namespace Snak.Utilities
{
    public static class TfsUtils
    {
        public static Workspace GetWorkspace(string path, string serverName, NetworkCredential credentials, LoggingHandler log)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(serverName)) throw new ArgumentNullException("serverName");
            if (credentials == null) throw new ArgumentNullException("credentials");
            if (string.IsNullOrEmpty(credentials.UserName)) throw new ArgumentException("credentials has no UserName specified");

            log(LogLevel.Verbose, "Getting the current workspace.");
            Workstation workstation = Workstation.Current;

            TeamFoundationServer server = new TeamFoundationServer(serverName, credentials);
            server.Authenticate();
            VersionControlServer versionControlServer = (VersionControlServer)server.GetService(typeof(VersionControlServer));

            // Update workspace cache first: work round an issue I had where the lack of
            // the local cache prevented the mapping detection from working at all
            // PW
            string userName = (credentials == CredentialCache.DefaultNetworkCredentials) 
                ? Thread.CurrentPrincipal.Identity.Name
                : credentials.UserName;
            workstation.UpdateWorkspaceInfoCache(versionControlServer, userName);

            // Now use the cache to try and locate the workstation that the path is mapped to
            WorkspaceInfo workspaceInfo = workstation.GetLocalWorkspaceInfo(path);
            if (workspaceInfo==null)
                throw new Exception("The directory '" + path + "' is not mapped to any TFS workspace");

            Workspace workspace = workspaceInfo.GetWorkspace(server);
            if (workspace==null)
                throw new Exception("The directory '" + path + "' is not mapped to a TFS workspace on the designated server " + serverName);

            log(LogLevel.Verbose, "Directory {0} mapped [Workspace={1}; Owner={2}; Server={3}]", 
                path, workspace.Name, workspace.OwnerName, server.Name);

            return workspace;
        }
    }
}
