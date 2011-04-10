using System;
using System.Collections.Generic;
using System.Text;

namespace Snak.Core
{
    /// <summary>
    /// 
    /// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\8.0\Policy\SupportedProjects
    /// </summary>
    /// <see cref="http://msdn2.microsoft.com/en-us/library/hb23x61k.aspx"/>
    internal enum ProjectType
    {
        SolutionFolder, 
        VisualBasic,
        VisualCSharp,
        WebProject
    }
}
