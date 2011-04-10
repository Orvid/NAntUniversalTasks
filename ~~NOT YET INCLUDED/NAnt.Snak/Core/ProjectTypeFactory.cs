using System;
using System.Collections.Generic;
using System.Text;

namespace Snak.Core
{
    internal class ProjectTypeFactory
    {
        // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\8.0\Projects
        // {FAE04EC0-301F-11d3-BF4B-00C04F79EFBC} csproj
        // {F184B08F-C81C-45f6-A57F-5ABD9991F28F} vbproj
        // {978C614F-708E-4E1A-B201-565925725DBA} Visual Studio Deployment Setup Project
        // {8BC9CEB9-8B4A-11D0-8D11-00A0C91BC942} Exe Projects
        // {54435603-DBB4-11D2-8724-00A0C9A8B90C} Visual Studio Deployment Project
        // {4fd007e8-1a56-7e75-70ca-0466484d4f98} VisualBasic Test Project
        // {3AC096D0-A1C2-E12C-1390-A8335801FDAB} Test Project
        // {39d444fd-b490-1554-5274-2d612a165298} CSharp Test Project
        // {2150E333-8FDC-42a3-9474-1A3956D46DE8} Solution Folder Project
        // {349c5851-65df-11da-9384-00065b846f21} Web Application Project Factory (Web application project)

        public static ProjectType GetProjectType(string projectTypeGuid)
        {
            switch (projectTypeGuid.ToUpperInvariant())
            {
                case "{2150E333-8FDC-42A3-9474-1A3956D46DE8}":
                case "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}": //?? Typo?
                    return ProjectType.SolutionFolder;

                case "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}": return ProjectType.VisualBasic;
                case "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}": return ProjectType.VisualCSharp;
                case "{E24C65DC-7377-472B-9ABA-BC803B73C61A}": return ProjectType.WebProject;

                default: return ProjectType.VisualCSharp;
            }
        }
    }
}
