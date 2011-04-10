using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Snak.Core;
using NUnit.Framework;

namespace Snak.BuildDrop
{
    public class LabelDirectoryDropNamingStrategy : AbstractDropNamingStrategy
    {
        public override DirectoryInfo GetPackageDropDirectory(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            return 
                GetDropDirectory(packageName, dropToOwnDirectory, 
                    delegate() 
                    {
                        return Path.Combine(this.DropDirectory.FullName, GetPackageName(packageName, packageIsPartOfSolution)); 
                    }
                );
        }

        public override DirectoryInfo GetPackageDropDirectoryCurrent(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            return
                GetDropDirectoryCurrent(packageName, dropToOwnDirectory, 
                    delegate() 
                    {
                        return Path.Combine(this.DropDirectoryCurrent.FullName, GetPackageName(packageName, packageIsPartOfSolution)); 
                    }
                );  
        }

        public override DirectoryInfo GetPackageLastDropDirectoryLast(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            return TryGetDropDirectoryElseGetNull(packageName, dropToOwnDirectory, 
                delegate() 
                {
                    if (this.DropDirectoryLast != null)
                    {
                        return Path.Combine(this.DropDirectoryLast.FullName, GetPackageName(packageName, packageIsPartOfSolution));
                    }
                    else
                        return null;
                }
            );   
        }

        public LabelDirectoryDropNamingStrategy(string solutionName, DirectoryInfo artefactDirectory, BuildVersion buildVersion, string solutionConfiguration)
            : base(solutionName, artefactDirectory, buildVersion, solutionConfiguration)
        {
            string buildLabeledDropDir = GetLabeledBuildFolder(this.ArtefactDirectory.FullName, this.BuildVersion.BuildVersionNumber);

            this.DropDirectory = new DirectoryInfo(buildLabeledDropDir);
            this.DropDirectoryCurrent = new DirectoryInfo(Path.Combine(this.ArtefactDirectory.FullName, "Build_Current"));
            this.DropDirectoryLast = TryFigureOutPreviousDropDir();

            string assemblyDropDir = this.SolutionName.Replace(".sln", "") + "_Assemblies_" + MakeSafeForPath(this.SolutionConfiguration);

            string commonAssembliesDropLocationName = Path.Combine(this.DropDirectory.FullName, assemblyDropDir);
            string commonAssembliesDropLocationCurrent = Path.Combine(this.DropDirectoryCurrent.FullName, assemblyDropDir);

            this.PackagesCommonDropDirectory = new DirectoryInfo(commonAssembliesDropLocationName);
            this.PackagesCommonDropDirectoryCurrent = new DirectoryInfo(commonAssembliesDropLocationCurrent);

            if (DropDirectoryLast != null)
                this.PackagesCommonDropDirectoryLast = TryFigureOutPreviousCommonDropLocation(DropDirectoryLast, commonAssembliesDropLocationName);
        }

        private DirectoryInfo TryFigureOutPreviousDropDir()
        {
            string expectedLocation = GetLabeledBuildFolder(this.ArtefactDirectory.FullName, this.BuildVersion.BuildVersionNumber - 1);

            DirectoryInfo expectedDirectoryInfo = new DirectoryInfo(expectedLocation);

            return (expectedDirectoryInfo.Exists) ? expectedDirectoryInfo : null;
        }

        private DirectoryInfo TryFigureOutPreviousCommonDropLocation(DirectoryInfo _dropDirectoryLast, string expectedCommonAssembliesDropLocationName)
        {
            DirectoryInfo expectedDirectoryInfo = new DirectoryInfo(Path.Combine(_dropDirectoryLast.FullName, expectedCommonAssembliesDropLocationName));

            return (expectedDirectoryInfo.Exists) ? expectedDirectoryInfo : null; 
        }

        private string GetLabeledBuildFolder(string artefactDirectory, int buildVersionNumber)
        {
            return Path.Combine(artefactDirectory, "Build_" + buildVersionNumber.ToString());
        }

        private string GetPackageName(string packageName, bool packageIsPartOfSolution)
        {
            string name = packageName;

            if (packageIsPartOfSolution)
                name += "_" + MakeSafeForPath(this.SolutionConfiguration);

            return name;
        }

#if(UNITTEST)

        [NUnit.Framework.TestFixture]
        public class DropPackagesToLabelDirectoryStrategyTester : AbstractDropNamingStrategyTester
        {
            [Test]
            public void CheckBuildDropDirectoryNames()
            {
                try
                {
                    ISolutionInfo solution = Snak.Core.Whidbey.SolutionInfo.CreateNew(new FileInfo(@"..\..\..\TestArtifacts\SampleApplications\v2.0\C#\SolutionWithMultipleDifferingProjects.sln"), NullLogger.Log);
                    DirectoryInfo artifactDir = new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#");

                    IDropNamingStrategy packageNamingStrategy = new LabelDirectoryDropNamingStrategy("SolutionWithMultipleDifferingProjects.sln", artifactDir, new BuildVersion("65"), "Debug|Any CPU");
                    BuildDrop buildDropLocations = new BuildDrop(packageNamingStrategy, solution, "Debug|Any CPU");

                    // IProjectInfo[] projects = solution.GetProjectsFor("Debug|Any CPU");
                    Dictionary<string, IProjectInfo> projectsInSolution = new Dictionary<string, IProjectInfo>();

                    foreach (IProjectInfo project in solution.GetProjectsFor("Debug|Any CPU"))
                    {
                        projectsInSolution.Add(project.Name, project);
                    }

                    CheckDirectoryName(
                        buildDropLocations.CommonAssembliesDropLocation.DropDir,
                        new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\Build_65\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU"),
                        "SolutionWithMultipleDifferingProjects.sln (Solution)"
                    );

                    CheckDirectoryName(
                        buildDropLocations.CommonAssembliesDropLocation.DropDirCurrent,
                        new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\Build_Current\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU"),
                        "SolutionWithMultipleDifferingProjects.sln (Solution)"
                    );

                    foreach (KeyValuePair<string, DropLocation> keyValuePair in buildDropLocations.DropLocationsByProjectName)
                    {
                        IProjectInfo project = projectsInSolution[keyValuePair.Key];

                        bool deployToOwnDirectory = (project.IsWebProject || project.OutputType == ProjectOutputType.Exe || project.OutputType == ProjectOutputType.WinExe);

                        if (deployToOwnDirectory)
                        {
                            CheckDirectoryName(
                                keyValuePair.Value.DropDir,
                                new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\Build_65\" + keyValuePair.Key + "_Debug-AnyCPU"),
                                project.Name
                            );

                            CheckDirectoryName(
                                keyValuePair.Value.DropDirCurrent,
                                new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\Build_Current\" + keyValuePair.Key + "_Debug-AnyCPU"),
                                project.Name
                            );
                        }
                        else
                        {
                            CheckDirectoryName(
                                keyValuePair.Value.DropDir,
                                new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\Build_65\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU"),
                                project.Name
                            );

                            CheckDirectoryName(
                                keyValuePair.Value.DropDirCurrent,
                                new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\Build_Current\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU"),
                                project.Name
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

#endif    
    
    }
}
