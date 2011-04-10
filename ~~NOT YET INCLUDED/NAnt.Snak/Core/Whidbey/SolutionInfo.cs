using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NAnt.VSNet.Types;
using Snak.Tasks;
using Snak.Utilities;
using System.Collections.Generic;

namespace Snak.Core.Whidbey
{
	/// <summary>
	/// Represents a Visual Studio 2005 solution file, and provides access to it's projects
	/// </summary>
	/// <remarks>
	/// <example>
	///   <para>Some example</para>
	///   <code>
	///     <![CDATA[
	///     some code here
	///     ]]>
	///   </code>
	/// </example>
	/// </remarks>
	public class SolutionInfo : ISolutionInfo
	{
		#region Declarations
		private readonly FileInfo _solutionFile;
        private readonly string[] _buildTypes;
        private readonly ProjectLink[] _projects;
        private readonly Dictionary<Guid, ProjectLink> _projectsByGuid;
        private readonly Dictionary<string, ProjectLinkWithConfig[]> _projectBySolutionConfiguration;
		private readonly NAnt.VSNet.Types.WebMapCollection _projectWebMaps;
		private const string PROJECT_PATTERN = @"Project\(""([^""]+)""\)\s*=\s*""([^""]+)"",\s*""([^""]+)"",\s""([^""]+)";
		// private const string PROJECT_BUILD_PATTERN = @"^*?(?<projectGuid>\{[^\}]*?\})\.\bSOLUTIONCONFIG\b\.(?<configMetaData>[^\s]*?)\s=\s(?<projectConfig>[^\r\n]*)"; //";
		// If you look in the solution file you may see an entry for a project for a given configuration that look like this
		// {26A46113-403B-4F0E-A187-F27928956CD6}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		// {26A46113-403B-4F0E-A187-F27928956CD6}.Debug|Any CPU.Build.0 = Debug|Any CPU
        // We are only going to grab the configuration related to Build.0, the regexp above this (thats commented out) will allow you to get access to the 
		// values of ActiveCfg or Build.0 if you want to take this further...
        private const string PROJECT_BUILD_PATTERN = @"^*?(?<projectGuid>\{[^\}]*?\})\.\bSOLUTIONCONFIG\b\.Build\.0\s=\s(?<projectConfig>[^\r\n]*)"; //";
		private const string BUILD_TYPES_CONFIGURATION_SECTION_PATTERN = @"GlobalSection\(SolutionConfigurationPlatforms\)[^\n]*\n(?<configValues>(.|\n)*?)EndGlobalSection";
		private const string BUILD_TYPES_PATTERN = @"^(?<entireConfigString>(?<configValue>.*?)\|(?<platformValue>.*?))=.*?$";
        private LoggingHandler _log;
        #endregion

        /// <summary>
        /// Factory method to create an instance and attach it's logger
        /// </summary>
        public static SolutionInfo CreateNew(FileInfo solutionFile, LoggingHandler log)
	    {
            if (!solutionFile.Exists)
                throw new FileNotFoundException(solutionFile.Name);
            if (solutionFile.Extension != ".sln")
                throw new ArgumentException(string.Format("{0} is not a valid solution file", solutionFile.FullName), "solutionFile");

            string solutionFileContents = File.ReadAllText(solutionFile.FullName);

            return new SolutionInfo(solutionFile, solutionFileContents, log);
	    }
	    
		#region Constructor(s)
        private SolutionInfo(FileInfo solutionFile, string solutionFileContents, LoggingHandler log)
		{
			_solutionFile = solutionFile;
            _log = log;
            log(LogLevel.Verbose, "Loading solution {0} as {1}", solutionFile.Name, GetType().FullName);

			// parse the solution file and create a list of all the projects within
            _buildTypes = ExtractBuildTypes(solutionFileContents);
            _projects = ExtractProjectLinks(solutionFileContents, log);
			_projectsByGuid = new Dictionary<Guid, ProjectLink>(_projects.Length);
			_projectWebMaps = new NAnt.VSNet.Types.WebMapCollection();
			foreach(ProjectLink project in _projects)
				_projectsByGuid.Add(project.ProjectGuid, project);

			_projectBySolutionConfiguration = ExtractProjectsByBuild(solutionFileContents, _projectsByGuid);
		}

		#endregion

		public FileInfo SolutionFile {
			get { return _solutionFile; }
		}

		/// <summary>
		/// A hashtable of maps between URI's (or URI prefixes) and local paths,
		/// to be used when resolving HTTP references for projects
		/// </summary>
		public NAnt.VSNet.Types.WebMapCollection WebMaps{
			get { return _projectWebMaps; }
		}

        /// <summary>
        /// An array of all the active build configurations for the solution
        /// (from which project configurations are mapped)
        /// One of these values is the input into <see cref="GetProjectsFor"/>
        /// </summary>
        public string[] Configurations
        {
            get { return _buildTypes; }
        }

		/// <summary>
		/// Retrieves an array of all the projects to be built in the specified solution configuration
		/// </summary>
		/// <param name="solutionConfiguration">A solution configuration (Debug, Release etc...)
		/// This is mapped back to project configurations based on what's configured within the solution,
		/// and doesn't neccesarily match any one project configuration name</param>
		/// <returns></returns>
		public IProjectInfo[] GetProjectsFor(string solutionConfiguration) 
		{
            ProjectLinkWithConfig[] links = GetProjectLinksFor(solutionConfiguration);
            if (links == null || links.Length==0) return new ProjectInfo[0];

			IProjectInfo[] projects = new IProjectInfo[links.Length];
			for (int i = 0; i < projects.Length; i++) {
                _log(LogLevel.Verbose, "Getting project {0} [{1}]", links[i].ProjectLink.Name, links[i].Config);
				projects[i] = GetProject(links[i]);
			}
			return projects;
		}

        /// <summary>
        /// Retrieves an array of all the projects to be built in the specified solution configuration
        /// </summary>
        /// <param name="solutionConfiguration">A solution configuration (Debug, Release etc...)
        /// This is mapped back to project configurations based on what's configured within the solution,
        /// and doesn't neccesarily match any one project configuration name</param>
        /// <returns></returns>
        internal ProjectLinkWithConfig[] GetProjectLinksFor(string solutionConfiguration)
        {
            try
            {
                ProjectLinkWithConfig[] links = (ProjectLinkWithConfig[])_projectBySolutionConfiguration[solutionConfiguration];
                return (links == null) ? new ProjectLinkWithConfig[0] : links;
            }
            catch (Exception err)
            {
                string message = string.Format("Solution configuration '{0}' not located",solutionConfiguration);
                throw new InvalidOperationException(message, err);
            }
        }
	    
		IProjectInfo[] ISolutionInfo.GetProjectsFor(string solutionConfiguration) {
			return GetProjectsFor(solutionConfiguration);
		}

		private IProjectInfo GetProject(ProjectLinkWithConfig link)
		{
            FileInfo projectLocation = link.GetProjectLocation(SolutionFile.Directory, _projectWebMaps);
			return ProjectFactory.GetProject(projectLocation, link.Config, _log);
		}

		private static ProjectLink[] ExtractProjectLinks(string solutionFileContents, LoggingHandler log)
		{
			Regex projectMatcher = new Regex(PROJECT_PATTERN);
			MatchCollection matches = projectMatcher.Matches(solutionFileContents);
			List<ProjectLink> projects = new List<ProjectLink>(matches.Count);

			foreach(Match match in matches){

                string projectTypeGuid = match.Groups[1].Value;
				string name = match.Groups[2].Value;
				string location = match.Groups[3].Value;
				string projectGuid = match.Groups[4].Value;
                ProjectType projectType = ProjectTypeFactory.GetProjectType(projectTypeGuid);

                if (projectType == ProjectType.WebProject)
                {
                    // as web projects dont have project files we need to get the project info from the solution
                    // throw new NotImplementedException("TODO: file based web project support in .net 2.0");
                    log(LogLevel.Warning, "Skipping project {0}: file-based websites are not currently supported. Guid={1}", name, projectGuid);
                }
                else
                {
                    projects.Add(new ProjectLink(name, location, new Guid(projectGuid), new Guid(projectTypeGuid)));
                }
			}
			return projects.ToArray();
		}

        private Dictionary<string, ProjectLinkWithConfig[]> ExtractProjectsByBuild(string solutionFileContents, Dictionary<Guid, ProjectLink> projectsByGuid) 
		{
            Guard.ArgumentStringIsNotNullOrEmpty(solutionFileContents, "solutionFileContents");

			// Now create a hashtable that matches projects by the solution configurations they're enabled in
			Dictionary<string, ProjectLinkWithConfig[]> projectsByConfig = new Dictionary<string, ProjectLinkWithConfig[]>(_buildTypes.Length);
			foreach(string solutionConfig in _buildTypes)
			{
                _log(LogLevel.Verbose, "Locating project configurations for solution configuration " + solutionConfig);
				string matchPattern = PROJECT_BUILD_PATTERN.Replace("SOLUTIONCONFIG", solutionConfig.Replace("|", "\\|"));  //HACK - need to escape pipes....
				Regex projectConfigMatcher = new Regex(matchPattern);
				MatchCollection projectMatches = projectConfigMatcher.Matches(solutionFileContents);
				List<ProjectLinkWithConfig> projectsForThisConfig = new List<ProjectLinkWithConfig>(projectMatches.Count);

                foreach (Match projectConfigMatch in projectMatches)
                {

                    // look at the details for the project
                    // string projectGuid = projectConfigMatch.Groups[1].Value;
                    // string projectConfig = projectConfigMatch.Groups[2].Value;

                    string projectGuid = projectConfigMatch.Groups["projectGuid"].Value;
                    string configMetaData = projectConfigMatch.Groups["configMetaData"].Value;
                    string projectConfig = projectConfigMatch.Groups["projectConfig"].Value;
                    Guid projectGuidTyped = new Guid(projectGuid);

                    // pull the project for that guid back out of the projects hashtable
                    ProjectLink projectLink = null;
                    projectsByGuid.TryGetValue(projectGuidTyped, out projectLink);
                    if (projectLink != null)
                    {
                        // ...and add it into the array for this configuration
                        _log(LogLevel.Verbose, "Adding {0} [{1}]", projectLink.Name, projectConfig);
                        projectsForThisConfig.Add(new ProjectLinkWithConfig(projectLink, projectConfig));
                    }
                    else
                    {
                        // At the moment it *is* possible to find a project/config mapping that
                        // we can't find the corresponding ProjectLink for: this means it's
                        // a project type that we don't support yet.
                        // So rather than throwing (like we used to)...
                        //      Guard.IsNotNull(projectLink, "No project link found for project with id " + projectGuid);
                        // ...we now just skip silently unless the link exists
                        _log(LogLevel.Verbose, "Skipping project {0} [{1}]: ProjectLink not present",
                            projectGuid, projectConfig);
                    }
                }

				// finally, add all the projects active in this configuration into the hashtable
				projectsByConfig.Add(solutionConfig, projectsForThisConfig.ToArray());
                _log(LogLevel.Verbose, "Located {0} project configurations for solution configuration {1}",
                    projectsForThisConfig.Count, solutionConfig);
            }
			return projectsByConfig;
		}

		private static string[] ExtractBuildTypes(string solutionFileContents) {
            Guard.ArgumentStringIsNotNullOrEmpty(solutionFileContents, "solutionFileContents");

			Regex regex = new Regex(BUILD_TYPES_CONFIGURATION_SECTION_PATTERN, RegexOptions.IgnoreCase);
			MatchCollection matches = regex.Matches(solutionFileContents);

			if (matches.Count == 0)
			{
				throw new ApplicationException("Could not find a GlobalSection 'SolutionConfigurationPlatforms' in the solution file");
			}

			regex = new Regex(BUILD_TYPES_PATTERN, RegexOptions.Multiline);
			matches = regex.Matches(matches[0].Groups["configValues"].Value);

			if (matches.Count == 0)
			{
				throw new ApplicationException("Couldn't find any values in the solution file GlobalSection 'SolutionConfigurationPlatforms'");
			}

			// Create a list of all the types of build (Debug, Release)
			// ... ie all the solution configurations
			string[] buildTypes = new string[matches.Count];
			for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++) 
			{
				// in 2005 there is now also a platform build type that gets assigned to one of the build types:
				// eg 
				//Debug|.NET = Debug|.NET
				//Debug|Any CPU = Debug|Any CPU
				//Debug|Mixed Platforms = Debug|Mixed Platforms
				//Quick Run|.NET = Quick Run|.NET
				//Quick Run|Any CPU = Quick Run|Any CPU
				//Quick Run|Mixed Platforms = Quick Run|Mixed Platforms

				// matches[0].Groups["configValue"].Value.Trim() : will get the first part 
				// matches[0].Groups["platformValue"].Value.Trim() : will get the second part 

				// initially we were getting the 2 parts separately (as commented out above) but we 
				// we actually need the whole string as shown below
				buildTypes[matchIndex] = matches[matchIndex].Groups["entireConfigString"].Value.Trim();
			}

			return buildTypes;
		}

		private static string ReplaceLeft(string value, string prefix, string newPrefix)
		{
			if (!value.StartsWith(prefix))
				throw new InvalidOperationException("Value must start with prefix to begin with");

			return newPrefix + value.Substring(prefix.Length);
		}

		internal class ProjectLinkWithConfig
		{
			private ProjectLink _projectLink;
			private string _config;

			public ProjectLinkWithConfig(){}

			public ProjectLinkWithConfig(ProjectLink projectLink, string config)
			{
				_projectLink = projectLink;
				_config = config;
			}

			public ProjectLink ProjectLink 
			{
				get { return _projectLink; }
				set { _projectLink = value; }
			}

			public string Config 
			{
				get { return _config; }
				set { _config = value; }
			}

			public FileInfo GetProjectLocation(DirectoryInfo solutionRoot, NAnt.VSNet.Types.WebMapCollection webProjectMappings) 
			{
				if (ProjectLink.IsWebProject)
				{
					string mappedPath = webProjectMappings.FindBestMatch(ProjectLink.Location);
					if (mappedPath!=null)
					{
						FileInfo mappedFile = new FileInfo(mappedPath);
						AssertFileExists(mappedFile, "Unable to locate web project {0} after mapping to {1}", ProjectLink.Location, mappedPath); 
						return mappedFile;
					}

					// Attempt to find the project directly under the solution
					string projectFileName = Path.GetFileName(ProjectLink.Location);
					string projectDirectory = Path.GetFileName(Path.GetDirectoryName(ProjectLink.Location));
					string pathAttempted = Path.Combine(Path.Combine(solutionRoot.FullName, projectDirectory), projectFileName);
					FileInfo inferredFile = new FileInfo(pathAttempted);
					AssertFileExists(inferredFile, "Failed to infer path to web project {0} at {1}", ProjectLink.Location, inferredFile);
					return inferredFile;
				}
				else
					return new FileInfo(Path.Combine(solutionRoot.FullName, ProjectLink.Location));
			}

			private static void AssertFileExists(FileInfo fileInfo, string message, params object[] args)
			{
				if (!fileInfo.Exists)
				{
					//System.Diagnostics.Debugger.Launch();
					throw new FileNotFoundException(string.Format(message, args), fileInfo.FullName);
				}
			}
		}

#if(UNITTEST)
		[NUnit.Framework.TestFixture]
		public class SolutionInfoTester{
            private const string SOLUTION_NAME = @"..\..\..\TestArtifacts\SampleApplications\v2.0\C#\SolutionWithMultipleDifferingProjects.sln";
            private string testSolutionRoot = String.Empty;

			#region Setup test and mock objects
			SolutionInfo solutionInfo;
			string solutionFileContents;

			[NUnit.Framework.TestFixtureSetUp]
			public void Init(){
                testSolutionRoot = Path.GetFullPath(Path.GetDirectoryName(SOLUTION_NAME)).ToLower();
                solutionInfo = SolutionInfo.CreateNew(new FileInfo(Path.GetFullPath(SOLUTION_NAME)), NullLogger.Log);
				// don't map for the first WebApplication1 project - this is left to be inferred
				WebMap map = new WebMap();
                map.Url = "http://localhost/Webapplication1";
                map.Path = new FileInfo(Path.Combine(testSolutionRoot, @"AspNetWebApplication\WebApplication1\WebApplication1.csproj"));
                solutionInfo.WebMaps.Add(map);

				using(TextReader reader = solutionInfo.SolutionFile.OpenText())
					solutionFileContents = reader.ReadToEnd();

				if (solutionFileContents==null)
					throw new FileLoadException("Failed to load solution contents");
			}

			[NUnit.Framework.SetUp]
			public void Setup()
			{
			}
			#endregion

			[NUnit.Framework.Test()]
			public void TestReplacePrefix()
			{
				NUnit.Framework.Assert.AreEqual("Something", ReplaceLeft("Nothing", "No", "Some"));
				NUnit.Framework.Assert.AreEqual("Nothing", ReplaceLeft("Something", "Some", "No"));
			}

			[NUnit.Framework.Test(Description="")]
			public void TestParseSolutionFile(){
				string solutionFileDemo = @"Microsoft Visual Studio Solution File, Format Version 8.00
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""SNAK"", ""SNAK\SNAK.csproj"", ""{3CFFC105-BE5E-4968-A74D-D0A9EC7CE340}""
Project(""{00a0a0C0-301F-11D3-BF4B-00C04F79a0a0}"") = ""Other"", ""Other\Other.csproj"", ""{3C0a0a0a-BE5E-4968-A74D-000000000001}""
	ProjectSection(ProjectDependencies) = postProject
	...etc...";

				ProjectLink[] projects = ExtractProjectLinks(solutionFileDemo, NullLogger.Log);

				NUnit.Framework.Assert.IsNotNull(projects, "ExtractProjectInfos returned null");
				NUnit.Framework.Assert.AreEqual(2, projects.Length);
				NUnit.Framework.Assert.AreEqual("SNAK", projects[0].Name);
				NUnit.Framework.Assert.AreEqual("Other", projects[1].Name);
				NUnit.Framework.Assert.AreEqual("SNAK\\SNAK.csproj".ToLower(), projects[0].Location);
				NUnit.Framework.Assert.AreEqual("3CFFC105-BE5E-4968-A74D-D0A9EC7CE340".ToLower(), projects[0].ProjectGuid.ToString());
			}

			[NUnit.Framework.Test]
			public void TestExtractBuildTypes()
			{
				string[] buildTypes = ExtractBuildTypes(solutionFileContents);

				NUnit.Framework.Assert.AreEqual(2, buildTypes.Length);
				NUnit.Framework.Assert.AreEqual("Debug|Any CPU", buildTypes[0]);
                NUnit.Framework.Assert.AreEqual("Release|Any CPU", buildTypes[1]);
			}

			[NUnit.Framework.Test]
			public void TestSolutionInfo()
			{
				IProjectInfo[] projects;

                projects = solutionInfo.GetProjectsFor("Debug|Any CPU");
				NUnit.Framework.Assert.AreEqual(7, projects.Length);
			
				projects = solutionInfo.GetProjectsFor("Release|Any CPU");
				NUnit.Framework.Assert.AreEqual(6, projects.Length);
			}

			public void SpikeProjectBuildPatternRegex()
			{
				Init();
				Regex regex = new Regex(PROJECT_BUILD_PATTERN.Replace("SOLUTIONCONFIG", "Debug"));
				NUnit.Framework.Assert.IsTrue(regex.IsMatch(solutionFileContents));
				Console.WriteLine(regex.Match(solutionFileContents).Value);
			}

			[NUnit.Framework.Test]
			[NUnit.Framework.ExpectedException(typeof(ArgumentException))]
			public void TestGetBadProject()
			{
                SolutionInfo.CreateNew(new FileInfo(this.GetType().Assembly.Location), NullLogger.Log);
			}

		    [NUnit.Framework.Test]
		    public void TestProjectsExcludedFromSolutionBuildNotReturned()
            {
                #region SOLUTION_WITH_EXCLUDED_BUILD
            /// <summary>
            /// A solution file where one project (ClassLibrary1) is not present in the debug build
            /// </summary>
            const string SOLUTION_WITH_EXCLUDED_BUILD = @"
Microsoft Visual Studio Solution File, Format Version 9.00
" + @"# Visual Studio 2005
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""ClassLibrary1"", ""ClassLibrary\ClassLibrary1.csproj"", ""{A49EF1A3-5F78-44F5-A66D-AD51B84A5B28}""
EndProject
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""ConsoleApplication1"", ""ConsoleApplication\ConsoleApplication1.csproj"", ""{D5720673-3207-4F4C-810A-C8E6F0838CF4}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{A49EF1A3-5F78-44F5-A66D-AD51B84A5B28}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{A49EF1A3-5F78-44F5-A66D-AD51B84A5B28}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{A49EF1A3-5F78-44F5-A66D-AD51B84A5B28}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{A49EF1A3-5F78-44F5-A66D-AD51B84A5B28}.Release|Any CPU.Build.0 = Release|Any CPU
		{D5720673-3207-4F4C-810A-C8E6F0838CF4}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{D5720673-3207-4F4C-810A-C8E6F0838CF4}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{D5720673-3207-4F4C-810A-C8E6F0838CF4}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
";
		        #endregion
		    
                FileInfo fileInfo = new FileInfo(@"C:\temp\somesolution.sln");
                SolutionInfo solution = new SolutionInfo(fileInfo, SOLUTION_WITH_EXCLUDED_BUILD, NullLogger.Log);
		        
		        ProjectLinkWithConfig[] debugProjects = solution.GetProjectLinksFor("Debug|Any CPU");
                NUnit.Framework.Assert.IsTrue(debugProjects.Length > 0, "Failed to get *any* projects in Debug build");
                if (ContainsProject(debugProjects, "ConsoleApplication1"))
                    NUnit.Framework.Assert.Fail("Expected ConsoleApplication1 to be excluded from Debug build");

                ProjectLinkWithConfig[] releaseProjects = solution.GetProjectLinksFor("Release|Any CPU");
                NUnit.Framework.Assert.IsTrue(releaseProjects.Length > 0, "Failed to get *any* projects in Release build");
                if (!ContainsProject(releaseProjects, "ConsoleApplication1"))
                    NUnit.Framework.Assert.Fail("Expected ConsoleApplication1 to be included in Release build");
            }

            private static bool ContainsProject(ProjectLinkWithConfig[] projects, string projectName)
		    {
                foreach (ProjectLinkWithConfig project in projects)
		            if (project.ProjectLink.Name.ToLowerInvariant() == projectName.ToLowerInvariant())
                        return true;
		        return false;
		    }
		}
#endif

	}
}
