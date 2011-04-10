using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Snak.Core;
using NUnit.Framework;

namespace Snak.BuildDrop
{
    public class CommonDirectoryDropNamingStrategy : AbstractDropNamingStrategy
    {
        public override DirectoryInfo DropDirectory
        {
            get 
            {
                return this.ArtefactDirectory; // this this stragegy we drop everything into the Artefact directory
            }
        }

        public override DirectoryInfo DropDirectoryCurrent
        {
            get
            {
                // this strategy always drops everything to the same artefacts directory so there 
                // should never be a current artefact directory, there can however, be current 
                // directories for projects and common assembly output, use another method of this class to get those
                return null; 
            }
        }

        public override DirectoryInfo DropDirectoryLast
        {
            get 
            {
                return null; // this stragegy always drops everything to the same artefacts directory so there should never be a 'last' directory
            }
        }

        public override DirectoryInfo GetPackageDropDirectory(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            return
                GetDropDirectory(packageName, dropToOwnDirectory,
                    delegate()
                    {
                        string name = packageName;

                        if (packageIsPartOfSolution)
                            name += "_" + MakeSafeForPath(this.SolutionConfiguration);

                        name += "_" + this.BuildVersion.BuildVersionString;

                        return Path.Combine(this.DropDirectory.FullName, name);
                    }
                );
        }

        public override DirectoryInfo GetPackageDropDirectoryCurrent(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            return
                GetDropDirectoryCurrent(packageName, dropToOwnDirectory,
                    delegate()
                    {
                        string name = packageName;

                        if (packageIsPartOfSolution)
                            name += "_" + MakeSafeForPath(this.SolutionConfiguration);

                        name +=  "_Current";

                        return Path.Combine(this.DropDirectory.FullName, name);
                    }
                );      
        }

        public override DirectoryInfo GetPackageLastDropDirectoryLast(string packageName, bool dropToOwnDirectory, bool packageIsPartOfSolution)
        {
            return TryGetDropDirectoryElseGetNull(packageName, dropToOwnDirectory,
                 delegate()
                 {
                     int lastVersionNumber = this.BuildVersion.BuildVersionNumber - 1;

                     string name = packageName;

                     if (packageIsPartOfSolution)
                         name += "_" + MakeSafeForPath(this.SolutionConfiguration);

                     name += "_" + lastVersionNumber.ToString();

                     return Path.Combine(this.DropDirectory.FullName, name);
                 }
             ); 
        }

        public CommonDirectoryDropNamingStrategy(string solutionName, DirectoryInfo artefactDirectory, BuildVersion buildVersion, string solutionConfiguration)
            : base(solutionName, artefactDirectory, buildVersion, solutionConfiguration)
        {
            string commonSolutionDropDirNameWithoutBuildNumberPart = Path.Combine(this.DropDirectory.FullName, this.SolutionName.Replace(".sln", "") + "_Assemblies_" + MakeSafeForPath(this.SolutionConfiguration) + "_");

            this.PackagesCommonDropDirectory =  new DirectoryInfo(commonSolutionDropDirNameWithoutBuildNumberPart + this.BuildVersion.BuildVersionString);
            this.PackagesCommonDropDirectoryCurrent = new DirectoryInfo(commonSolutionDropDirNameWithoutBuildNumberPart + "Current");
            this.PackagesCommonDropDirectoryLast = TryFigureOutPreviousDir(commonSolutionDropDirNameWithoutBuildNumberPart);
        }

        private DirectoryInfo TryFigureOutPreviousDir(string dropDirNameWithoutBuildNumberPart)
        {
            DirectoryInfo previousDropDir = null;

            // try figure out the name that would have been given to the last drop location, 
            if (this.BuildVersion.BuildVersionType == BuildVersionType.Numeric)
            {
                int previousBuildLabel = this.BuildVersion.BuildVersionNumber - 1;
                previousDropDir = new DirectoryInfo(dropDirNameWithoutBuildNumberPart + previousBuildLabel.ToString());

                if (!previousDropDir.Exists)
                    previousDropDir = null;
            }
            else if (this.BuildVersion.BuildVersionType == BuildVersionType.VersionNumber)
            {
                throw new NotImplementedException("Cannot figure out PreviousDropDir when the build label is a version (e.g. 1.0.0.0), currently we only support numerical build labels");
                // GetFirstDirectoryNameBasedOnSortTask getFirstDirectoryNameBasedOnSortTask = new GetFirstDirectoryNameBasedOnSortTask();
            }

            return previousDropDir;
        }

        public override void DeleteDropDirectories()
        {
            // we dont want to delete the drop directoy for this implementatino beacuse its the common artifacts.. for all builds.
        }

        //internal override void CreateDropDirectories()
        //{
        //    CreateDropDirectory(_dropDir, true);
        //    CreateDropDirectory(_dropDirCurrent);
        //}

        //internal override void DeleteDropDirectories()
        //{
        //    DeleteDropDirectory(_dropDir);
        //    DeleteDropDirectory(_dropDirCurrent);
        //}

#if(UNITTEST)

        [NUnit.Framework.TestFixture]
        public class DropPackagesToCommonDirectoryStrategyTester : AbstractDropNamingStrategyTester
        {
            [Test]
            public void CheckBuildDropDirectoryNames()
            {
                ISolutionInfo solution = Snak.Core.Whidbey.SolutionInfo.CreateNew(new FileInfo(@"..\..\..\TestArtifacts\SampleApplications\v2.0\C#\SolutionWithMultipleDifferingProjects.sln"), NullLogger.Log);
                DirectoryInfo artifactDir = new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#");

                IDropNamingStrategy packageNamingStrategy = new CommonDirectoryDropNamingStrategy("SolutionWithMultipleDifferingProjects.sln", artifactDir, new BuildVersion("65"), "Debug|Any CPU");
                BuildDrop buildDropLocations = new BuildDrop(packageNamingStrategy, solution, "Debug|Any CPU");

                // IProjectInfo[] projects = solution.GetProjectsFor("Debug|Any CPU");
                Dictionary<string, IProjectInfo> projectsInSolution = new Dictionary<string, IProjectInfo>();

                foreach (IProjectInfo project in solution.GetProjectsFor("Debug|Any CPU"))
                {
                    projectsInSolution.Add(project.Name, project);
                }

                CheckDirectoryName(
                    buildDropLocations.CommonAssembliesDropLocation.DropDir,
                    new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU_65"),
                    "SolutionWithMultipleDifferingProjects.sln (Solution)"
                );

                CheckDirectoryName(
                    buildDropLocations.CommonAssembliesDropLocation.DropDirCurrent,
                    new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU_Current"),
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
                            new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\" + keyValuePair.Key + "_Debug-AnyCPU_65"),
                            project.Name
                        );

                        CheckDirectoryName(
                            keyValuePair.Value.DropDirCurrent,
                            new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\" + keyValuePair.Key + "_Debug-AnyCPU_Current"),
                            project.Name
                        );
                    }
                    else
                    {
                        CheckDirectoryName(
                            keyValuePair.Value.DropDir,
                            new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU_65"),
                            project.Name
                        );

                        CheckDirectoryName(
                            keyValuePair.Value.DropDirCurrent,
                            new DirectoryInfo(@"..\..\..\TestArtifacts\BuildDrops\v2.0\C#\SolutionWithMultipleDifferingProjects_Assemblies_Debug-AnyCPU_Current"),
                            project.Name
                        );
                    }
                }
            }
        }

#endif    
    }
}
