using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;

using Snak.Utilities;
using Snak.Tasks;

namespace Snak.Core.Whidbey
{
	/// <summary>
	/// Represents a project (and a particular configuration) within a Visual Studio 2005 solution
	/// </summary>
	public class ProjectInfo : IProjectInfo
	{
		#region Declarations
        const string MSBuild_Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

		private readonly FileInfo _projectFile;
		private readonly string _config;
        private readonly ProjectConfiguration _configuration;
		private readonly string _assemblyName;
		private readonly ProjectOutputType _outputType;
		private readonly string _defineConstants;
		private readonly string _outputPath;
		private readonly string _warningLevel;
		private readonly ProjectLanguage _projectLanguage;
		private const string WEB_DEPLOYMENT_PROJECT_STRING = "WebDeployment";
		private XmlDocument _projectDoc = null;
		private XmlNamespaceManager _projectFileXMLNamespaceManager = null;
        LoggingHandler _log;

		/// <summary>
		/// The project type (Local or Web, or the value of WEB_DEPLOYMENT_PROJECT_STRING)
		/// </summary>
		/// <remarks>It appears that the 2005 web deployment projects (.wdproj) don’t have a project type, given this we assign the type manually here</remarks>
		private readonly string _projectType;

		#endregion

        /// <summary>
        /// Factory method to create an instance
        /// </summary>
        internal static ProjectInfo CreateNew(FileInfo projectFilePath, string config)
        {
            return CreateNew(projectFilePath, config, NullLogger.Log);
        }

        /// <summary>
        /// Factory method to create an instance and attach it's logger
        /// </summary>
        internal static ProjectInfo CreateNew(FileInfo projectFilePath, string config, LoggingHandler log)
        {
            return new ProjectInfo(projectFilePath, config, log);
        }

		/// <summary>
		/// Creates an instance given a path to the project file,
		/// and a build configuration
		/// </summary>
		/// <param name="projectFile">The project file (.csproj / .vbproj)</param>
		/// <param name="config">eg "Debug|AnyCPU", "Release|AnyCPU" [CASE SENSITIVE]</param>
		public ProjectInfo(string projectFile, string config)
			:this(new FileInfo(projectFile), config, NullLogger.Log)
		{
			
		}

		/// <summary>
		/// Creates an instance given the <see cref="FileInfo"/> to the project file,
		/// and a build configuration
		/// </summary>
		/// <param name="projectFile">The project file (.csproj / .vbproj)</param>
		/// <param name="config">eg "Debug|AnyCPU", "Release|AnyCPU" [CASE SENSITIVE]</param>
		public ProjectInfo(FileInfo projectFile, string config, LoggingHandler log) 
		{
			if (projectFile == null)
				throw new ArgumentNullException("projectFile");
			if (config == null || config == string.Empty)
				throw new ArgumentNullException("config");

			if (!projectFile.Exists)
				throw new FileNotFoundException("Failed to find project file", projectFile.FullName);
			if (!projectFile.Extension.EndsWith("proj"))
				throw new ArgumentException(string.Format("{0} is not a valid project file", projectFile.FullName), "projectFile");

            try
            {
                _projectFile = projectFile;
                _config = config;
                _configuration = ProjectConfiguration.Parse(config);
                _log = log;

                // load the project file up so we can start setting properties 
                _projectDoc = LoadXmlDocument(_projectFile.FullName);
                // we need the XmlNamespaceManager throughout this class in order to get values from _projectDoc
                _projectFileXMLNamespaceManager = new XmlNamespaceManager(_projectDoc.NameTable);
                _projectFileXMLNamespaceManager.AddNamespace("ms", MSBuild_Namespace);

                // sanity check the project file we've been given
                if (_projectDoc.DocumentElement.NamespaceURI != MSBuild_Namespace)
                    throw new InvalidOperationException(string.Format("{0} can only parse MSBuild-style project files", GetType().FullName));

                _projectType = GetProjectType();

                //HACK - proper place might be /Project/Import/@Project=*Microsoft.<language>.targets", but language is still subtly different to the enum
                _projectLanguage = (projectFile.Extension.ToLower() == ".vbproj") ? ProjectLanguage.VisualBasic : ProjectLanguage.CSHARP;   //(ProjectLanguage) Enum.Parse(typeof (ProjectLanguage), projectTypeNode.Name);

                //XmlElement projectSettings = (XmlElement) projectDoc.SelectSingleNode("//Build/Settings");
                _assemblyName = GetPropertySettingFor("AssemblyName");

                if (_projectType == WEB_DEPLOYMENT_PROJECT_STRING)
                    _outputType = ProjectOutputType.Deployment;
                else
                {
                    string outputType = GetPropertySettingFor("OutputType");
                    try
                    {
                        _outputType = (ProjectOutputType)Enum.Parse(typeof(ProjectOutputType), outputType);
                    }
                    catch (Exception err)
                    {
                        throw new InvalidOperationException(string.Format("OutputType of '{1}' not recognized", projectFile.Name, outputType), err);
                    }
                }

                _defineConstants = GetPropertySettingFor("DefineConstants");
                _outputPath = GetPropertySettingFor("OutputPath");
                _warningLevel = GetPropertySettingFor("WarningLevel");
            }
            catch (Exception err)
            {
                throw new InvalidOperationException(string.Format("Failed loading {0}: {1}", projectFile.Name, err.Message), err);
            }
		}


		private string GetProjectType()
		{
			string projectType = String.Empty;

			// there seems to be some inconsistencies regarding how to determine the 'project type' given the project file
			// some project files have a ProjectType element, some don’t have a ProjectType but have a ProjectTypeGuids element.
			// Some such as the .wdproj dont have either of the above mentioned.
			//
			// Given this we end up with a few areas were we can figure out the project type. At this point we really only care about
			// Web projects, Deployment projects and others (which we will label Local). (In 2003 we had Web or Local).
			//
			// At this point we can get the info from one of 3 places.
			//
			// 1: for project file that have a ProjectType element we can simply get that value, if that fails
			//
			// 2: for project file that have a ProjectTypeGuids element we can look in the registry, 
			// we could sniff around in the registry using code here but since we only care about a couple of project types I'll just 
			// hard code the guids as they are unlikely to change....
			//
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
			//
			// 3: for project file that don’t have ProjectType or ProjectTypeGuids element we can look at the file extension
			//
			// 4: if that fails we will just give it a ProjectType of Local
			//

			projectType = GetPropertySettingFor("ProjectType"); 

			if (projectType == String.Empty)
			{
				string guid = String.Empty;
				string guids = GetPropertySettingFor("ProjectTypeGuids");

				if (guids != null && guids != String.Empty)
				{
					if (guids.IndexOf(";", 0, guids.Length) > -1)
						guid = guids.Split(';')[0];
					else
						guid = guids;

					if (String.Compare(guid, "{349c5851-65df-11da-9384-00065b846f21}", true) == 0)
					{
						// Web Application Project Factory (Web application project)
						projectType = "Web";
					}
					else if (String.Compare(guid, "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}", true) == 0)
					{
						// Test Project
						projectType = "Test";
					}
					else
					{
						// do nothing as the projectType value will get set below.
					}
				}


				if (projectType == String.Empty && _projectFile.Extension == ".wdproj")
					projectType = WEB_DEPLOYMENT_PROJECT_STRING;

				if (projectType == String.Empty)
				{
					//Console.WriteLine("Could not find a project type for '" + _projectFile.FullName + "' using default value of 'Local'");
					projectType = "Local";
				}
			}

			return projectType;
		}

		/// <summary>
		/// Attempts to find a Property element with given name,
        /// first under the specific build config property group,
        /// then the generic/global property group.
		/// </summary>
        /// <remarks>The behaviour here has been extended to cope with build files
        /// where the condition is only partially specified, ie build files that
        /// say <![CDATA[
        ///   <PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">
        /// ]]></remarks>
		/// <param name="propertyName"></param>
		/// <param name="projectDoc"></param>
		/// <returns></returns>
		private string GetPropertySettingFor(string propertyName)
		{
            ProjectConfiguration config = _configuration;   // NB: Value type, so this is a copy!

			string propertyGroupXPath = string.Format("/ms:Project/ms:PropertyGroup[@Condition]");

			XmlElement propertyElement = null; 
			XmlNodeList propertyGroups = _projectDoc.SelectNodes(propertyGroupXPath, _projectFileXMLNamespaceManager);

            // TODO: Should be able to clean all this up quite a bit and do all the work in the XPath
            // however just a pain attempting to format the XPath (so many quotes everywhere - never seems to work)

            // First pass - attempt to find the property within the property group with the complete Condition
            // eg:   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
            string conditionExpected = config.GetMSBuildConditionAttribute().Replace(" ","");
            propertyElement = GetPropertyElementFromPropertyGroupList(propertyGroups, conditionExpected, propertyName);

            if (propertyElement == null)
            {
                // Second pass - attempt to find the property within the property group with a Configuration only condition
                // (ie no platform specified)
                // eg   <PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">

                config.Platform = string.Empty;
                conditionExpected = config.GetMSBuildConditionAttribute().Replace(" ", "");
                propertyElement = GetPropertyElementFromPropertyGroupList(propertyGroups, conditionExpected, propertyName);

                if (propertyElement == null)
                {
                    // Third pass - attempt to find the property within the non-specific property group
                    // Format of this is a bit different
                    //<PropertyGroup>
                    //  <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
                    //  <ProductVersion>8.0.50727</ProductVersion>
                    // However on the basis there's only ever one of these, we can just hit the property straight off
                    // NB: We still restrict the match to the default group - so won't accidentally pick up debug settings

                    config.Configuration = string.Empty;
                    conditionExpected = config.GetMSBuildConditionAttribute().Replace(" ", "");
                    string defaultPropertyGroupXPath = string.Format("/ms:Project/ms:PropertyGroup[ms:Configuration/@Condition]/ms:{0}", propertyName);
                    propertyElement = (XmlElement)_projectDoc.SelectSingleNode(defaultPropertyGroupXPath, _projectFileXMLNamespaceManager);
                }
            }

            if (propertyElement != null)
            {
                _log(LogLevel.Debug, "ProjectProperty {0}={1} [matched on Condition='{2}']", propertyName, propertyElement.InnerText, conditionExpected);
                return propertyElement.InnerText;
            }
            else
            {
                // Last chance: fall over to 'find me the first one you can find' match
                // This is handled seperately to the others because the log message is different
                // If we fall through to here really we need to do some more work, because this
                // is a very non-specific match, and can easily pickup settings from the wrong bit
                // (ie pickup Debug settings etc... rather than 'non-specific' settings)
                string propertyValueXPath = string.Format("/ms:Project/ms:PropertyGroup/ms:{0}", propertyName);
                propertyElement = (XmlElement)_projectDoc.SelectSingleNode(propertyValueXPath, _projectFileXMLNamespaceManager);
                if (propertyElement != null)
                {
                    _log(LogLevel.Warning, "ProjectProperty {0}={1} [matched using fallover match]", propertyName, propertyElement.InnerText);
                    return propertyElement.InnerText;
                }

                _log(LogLevel.Debug, "ProjectProperty {0}=String.Empty", propertyName);
                return string.Empty;
            }
		}

        private XmlElement GetPropertyElementFromPropertyGroupList(XmlNodeList propertyGroups, string conditionExpected, string propertyName)
        {
            XmlElement propertyFound = null;
            foreach (XmlElement propertyGroup in propertyGroups)
            {
                string conditionAttr = propertyGroup.GetAttribute("Condition");
                if (!string.IsNullOrEmpty(conditionAttr) && conditionAttr.Replace(" ", "") == conditionExpected)
                    propertyFound = (XmlElement)propertyGroup.SelectSingleNode("ms:" + propertyName, _projectFileXMLNamespaceManager);

                if (propertyFound != null) return propertyFound;
            }
            return null;
        }

		/// <summary>
		/// The name of the project
		/// </summary>
		public string Name {
			get { return Path.GetFileNameWithoutExtension(_projectFile.Name); }
		}

		/// <summary>
		/// The name of the project
		/// </summary>
		public string ProjectName {
			get { return Name; }

		}

		public FileInfo ProjectFile {
			get { return _projectFile; }
		}

		/// <summary>
		/// The project file's file extension (csproj / vbproj)
		/// </summary>
		public string ProjectFileExtension {
			get { return _projectFile.Extension; }
		}

		/// <summary>
		/// The file extension for code files for this project type (eg .cs 
		/// </summary>
		public string CodeFileExtension {
			get {
				switch (this.ProjectLanguage) {
					case ProjectLanguage.CSHARP:
						return ".cs";
					case ProjectLanguage.VisualBasic:
						return ".vb";
					default:
						throw new InvalidEnumArgumentException("Unknown project language " + ProjectLanguage);
				}
			}
		}

		/// <summary>
		/// The path to the project root
		/// </summary>
		public string ProjectDir {
			get { return _projectFile.DirectoryName; }
		}

		/// <summary>
		/// The configuration that this project's settings are being read from (Debug, Release etc...)
		/// </summary>
		public string Config {
			get { return _config; }
		}

		/// <summary>
		/// The language that the project is written in
		/// </summary>
		public ProjectLanguage ProjectLanguage {
			get { return _projectLanguage; }
		}

		/// <summary>
		/// The (simple) name of the project's output assembly (no file extension)
		/// </summary>
		public string AssemblyName {
			get { return _assemblyName; }
		}

		/// <summary>
		/// The name of the project's output assembly with the file extension
		/// </summary>
		public string AssemblyFileName {
			get {
				switch (this.OutputType) {
					case ProjectOutputType.Exe:
					case ProjectOutputType.WinExe:
					default:
						return AssemblyName + ".exe";

					case ProjectOutputType.Library:
						return AssemblyName + ".dll";
				}
			}
		}

		/// <summary>
		/// The complete path to the project's output assembly
		/// </summary>
		public string AssemblyPath {
			get { return this.OutputFilePath; }
		}

		/// <summary>
		/// The type of project it is (dll / exe)
		/// </summary>
		public ProjectOutputType OutputType {
			get { return _outputType; }
		}

		/// <summary>
		/// Any build constants setup for this project configuration
		/// </summary>
		public string DefineConstants {
			get { return _defineConstants; }
		}

		/// <summary>
		/// The relative path to the project's output folder (for this configuration)
		/// </summary>
		public string OutputPath {
			get { return _outputPath; }
		}


		/// <summary>
		/// The full path to the project output folder (for this configuration)
		/// </summary>
		public string OutputPathFull {
			get { return Path.Combine(ProjectDir, _outputPath); }
		}

		/// <summary>
		/// The complete path to the project's output assembly
		/// </summary>
		public string OutputFilePath {
			get { return Path.Combine(OutputPathFull, AssemblyFileName); }
		}


		public string WarningLevel {
			get { return _warningLevel; }
		}

		public bool IsWebProject
		{
			get{ return ProjectType == "Web"; }
		}

		public bool IsWebDeploymentProject
		{
			get{ return ProjectType == WEB_DEPLOYMENT_PROJECT_STRING; } 
		}

		public string ProjectType
		{
			get{ return _projectType;	}
		}

		public DotNetFrameworkVersion DotNetFrameworkVersion
		{
			get{ return DotNetFrameworkVersion.v2_0 ; }
		}

		/// <summary>
		/// The project's configuration file (web.config or app.config)
		/// </summary>
		public string ConfigFile 
		{ 
			get {
				if (IsWebProject)
					return "web.config";
				else
					return "app.config";
			  } 
		}

		/// <summary>
		/// Either a specially named 'nunit.config' in the project root,
		/// or the same as the project's configuration file, or
		/// </summary>
		public string TestingConfigFile 
		{ 
			get	{ 
				string testingConfig = Path.Combine(ProjectDir, "nunit.config");
				string binConfig = OutputFilePath + ".config";
				string binConfigParent = Path.Combine(this.OutputPathFull, "..\\") + this.AssemblyFileName + ".config";

				// fall through a couple of attempts

				if (File.Exists(binConfig))
					return binConfig;	// bin/assemblyname.dll.config
				else if (File.Exists(binConfigParent))
					return binConfigParent;	// assemblyname.dll.config (or bin/assemblyname.dll.config if above was in bin/debug)
				else if (File.Exists(testingConfig))
					return testingConfig;	// nunit.config
				else	// app.config / web.config
					return Path.Combine(ProjectDir, ConfigFile);
			} 
		}

		/// <summary>
		/// Gets an array of all the files contained within the project
		/// All paths are project-relative
		/// </summary>
		public string[] GetProjectFiles()
		{
			XmlDocument projectXml = LoadXmlDocument(_projectFile.FullName);
			XmlNodeList fileNodes = projectXml.SelectNodes("//Files/Include/File");
			return GetProjectFiles(fileNodes);
		}

		/// <summary>
		/// Gets an array of all the files contained within the project
		/// that match the content type provided (eg 'Content' for web pages).
		/// All paths are project-relative
		/// </summary>
		public string[] GetProjectFiles(ProjectFileType contentType)
		{
			string contentFilesXPath = string.Format("/ms:Project/ms:ItemGroup/ms:{0}", contentType);

			XmlNodeList matchingNodes = _projectDoc.SelectNodes(contentFilesXPath, _projectFileXMLNamespaceManager);

			string[] files = new string[matchingNodes.Count];

			for (int i = 0; i < matchingNodes.Count; i ++ )
			{
				string includeAttribute = ((XmlElement)matchingNodes[i]).GetAttribute("Include");
				
				if (includeAttribute.Length > 0)
					files[i] = includeAttribute;
				else
					files[i] = String.Empty;	
			}

			return files;
		}

		private string[] GetProjectFiles(XmlNodeList fileNodes)
		{
			ArrayList files = new ArrayList(fileNodes.Count);
			foreach (XmlElement fileNode in fileNodes)
			{
				files.Add(fileNode.GetAttribute("RelPath"));
			}
			return (string[])files.ToArray(typeof(string));
		}

		private static XmlDocument LoadXmlDocument(string documentPath)
		{
			XmlDocument projectDoc = new XmlDocument();
			using(FileStream projectFileStream = File.OpenRead(documentPath))
				projectDoc.Load(projectFileStream);
			return projectDoc;
		}

        public struct ProjectConfiguration
        {
            public string Configuration;
            public string Platform;

            public static ProjectConfiguration Parse(string projectConfiguration)
            {
                ProjectConfiguration config = new ProjectConfiguration();

                string[] buildParts = projectConfiguration.Replace(" ", "").Split('|');
                if (buildParts.Length > 0)
                    config.Configuration = buildParts[0];
                if (buildParts.Length > 1)
                    config.Platform = buildParts[1];

                return config;
            }

            /// <summary>
            /// Attempts to provide the string used in the Condition attribute of the MSBuild file
            /// for this <see cref="ProjectConfiguration"/>.
            /// </summary>
            /// <remarks>I wouldn't rely on the whitespacing to be the same. It is
            /// in all MSBuild files I've seen created by VS, but almost certainally
            /// MSBuild doesn't care about the whitespace, so in locating matching
            /// configurations we shouldn't either</remarks>
            public string GetMSBuildConditionAttribute()
            {
                if (string.IsNullOrEmpty(Configuration))
                    return " '$(Configuration)' == '' ";

                string configForXPath = Configuration.Replace(" ", "");
                if (string.IsNullOrEmpty(Platform))
                    return string.Format(" '$(Configuration)' == '{0}' ", configForXPath);

                string platformForXPath = Platform.Replace(" ", "");
                return string.Format(" '$(Configuration)|$(Platform)' == '{0}|{1}' ", configForXPath, platformForXPath);
            }
        }

#if( UNITTEST )
		[NUnit.Framework.TestFixture]
		public class ProjectInfoTester
		{
			private static string projectRoot;
			private const string PROJECT_NAME = "Snak.csproj";

			[NUnit.Framework.TestFixtureSetUp]
			public void Init()
			{
				projectRoot = AppDomain.CurrentDomain.BaseDirectory;
				while (!File.Exists(Path.Combine(projectRoot, PROJECT_NAME)))
					projectRoot = Path.GetDirectoryName(projectRoot);
	
				projectRoot = projectRoot.ToLower();
			}

			[NUnit.Framework.Test(Description="")]
			public void TestLoadDebugProject() {
				ProjectInfo project = new ProjectInfo(Path.Combine(projectRoot, PROJECT_NAME), "Debug");

				AssertEqualIgnoreCase("Snak", project.Name);
				AssertEqualIgnoreCase("Debug", project.Config);
				AssertEqualIgnoreCase("Snak.dll", project.AssemblyFileName);
				AssertEqualIgnoreCase("Snak", project.AssemblyName);
				AssertEqualIgnoreCase(".csproj", project.ProjectFileExtension);
				AssertEqualIgnoreCase(".cs", project.CodeFileExtension);
				AssertEqualIgnoreCase(projectRoot, project.ProjectDir);
				AssertEqualIgnoreCase(@"bin\debug\", project.OutputPath);
				AssertEqualIgnoreCase(Path.Combine(projectRoot, @"bin\debug\"), project.OutputPathFull);
				AssertEqualIgnoreCase(Path.Combine(projectRoot, @"bin\debug\Snak.dll"), project.OutputFilePath);
				NUnit.Framework.Assert.AreEqual(ProjectOutputType.Library, project.OutputType);
			}

            [NUnit.Framework.Test(Description = "")]
            public void TestLoadReleaseProject()
            {
                ProjectInfo project = new ProjectInfo(Path.Combine(projectRoot, PROJECT_NAME), "Release|Any CPU");

                AssertEqualIgnoreCase("Snak", project.Name);
                AssertEqualIgnoreCase("Release|Any CPU", project.Config);   // Not sure if this property should trim after the pipe or not
                AssertEqualIgnoreCase("Snak.dll", project.AssemblyFileName);
                AssertEqualIgnoreCase("Snak", project.AssemblyName);
                AssertEqualIgnoreCase(".csproj", project.ProjectFileExtension);
                AssertEqualIgnoreCase(".cs", project.CodeFileExtension);
                AssertEqualIgnoreCase(projectRoot, project.ProjectDir);
                AssertEqualIgnoreCase(@"bin\release\", project.OutputPath);
                AssertEqualIgnoreCase(Path.Combine(projectRoot, @"bin\release\"), project.OutputPathFull);
                AssertEqualIgnoreCase(Path.Combine(projectRoot, @"bin\release\Snak.dll"), project.OutputFilePath);
                NUnit.Framework.Assert.AreEqual(ProjectOutputType.Library, project.OutputType);
            }

			private void AssertEqualIgnoreCase(string expected, string actual) {
				NUnit.Framework.Assert.AreEqual(expected.ToLower(), actual.ToLower());
			}

			[NUnit.Framework.Test]
			public void TestGetProjectFiles()
			{
				Hashtable expectedFiles = new Hashtable();
				expectedFiles.Add(ProjectFileType.Compile, "Properties\\AssemblyInfo.cs");
				expectedFiles.Add(ProjectFileType.Content, "ContentItem.txt");
				expectedFiles.Add(ProjectFileType.EmbeddedResource, "EmbeddedItem.bmp");
				expectedFiles.Add(ProjectFileType.None, "NoneItem.txt");

				// Get the resource stream out and stick it on disk for the test
				string projectName = "TestCSharpWinformsProject2005.csproj";

                ProjectInfo projectInfo;
                
                projectInfo = GetProjectInfoFromResourceStream(projectName, "Debug|Any CPU");
                NUnit.Framework.Assert.AreEqual(@"bin\Debug\", projectInfo.OutputPath, "OutputPath");

				foreach (DictionaryEntry expectedFile in expectedFiles)
				{
					ProjectFileType type = (ProjectFileType)expectedFile.Key;
					string[] compileItems = projectInfo.GetProjectFiles(type);

					switch(type)
					{
						case ProjectFileType.EmbeddedResource:
							NUnit.Framework.Assert.AreEqual(2, compileItems.Length, "Length of items for '{0}'", type);
							NUnit.Framework.Assert.AreEqual(expectedFile.Value, compileItems[1], "Value of item for '{0}'", type);
							break;
						case ProjectFileType.Compile:
							NUnit.Framework.Assert.AreEqual(3, compileItems.Length, "Length of items for '{0}'", type);
							NUnit.Framework.Assert.AreEqual(expectedFile.Value, compileItems[0], "Value of item for '{0}'", type);
							break;
						case ProjectFileType.None:
							NUnit.Framework.Assert.AreEqual(3, compileItems.Length, "Length of items for '{0}'", type);
							NUnit.Framework.Assert.AreEqual(expectedFile.Value, compileItems[2], "Value of item for '{0}'", type);
							break;
						default:
							NUnit.Framework.Assert.AreEqual(1, compileItems.Length, "Length of items for '{0}'", type);
							NUnit.Framework.Assert.AreEqual(expectedFile.Value, compileItems[0], "Value of item for '{0}'", type);
							break;
					}
				}

                projectInfo = GetProjectInfoFromResourceStream(projectName, "Release|Any CPU");
                NUnit.Framework.Assert.AreEqual(@"bin\Release\", projectInfo.OutputPath, "OutputPath");
            }

			[NUnit.Framework.Test]
			[NUnit.Framework.ExpectedException(typeof(ArgumentException))]
			public void TestGetBadProject()
			{
				ProjectInfo.CreateNew(new FileInfo(this.GetType().Assembly.Location), "Debug");
			}

            [NUnit.Framework.Test(Description = "This test reflects a real-world WF generated project file, that doesn't specify the platform configuration in the MSBuild file")]
            public void TestGetBuildDetailsFromNonPlatformSepecificProjectFile()
            {
                // This project file doesn't specify the 'Any CPU' condition in it's build conditions
                string projectName = "TestCSharpWinformsProject2005_NonPlatformSpecificBuilds.csproj";

                ProjectInfo projectInfo;

                projectInfo = GetProjectInfoFromResourceStream(projectName, "Debug|Any CPU");
                NUnit.Framework.Assert.AreEqual(@".\bin\Debug\", projectInfo.OutputPath, "OutputPath");
                NUnit.Framework.Assert.AreEqual(@"FER.Workflow", projectInfo.AssemblyName, "AssemblyName");

                projectInfo = GetProjectInfoFromResourceStream(projectName, "Release|Any CPU");
                NUnit.Framework.Assert.AreEqual(@".\bin\Release\", projectInfo.OutputPath, "OutputPath");
                NUnit.Framework.Assert.AreEqual(@"FER.Workflow", projectInfo.AssemblyName, "AssemblyName");
            }


            private ProjectInfo GetProjectInfoFromResourceStream(string projectName, string buildConfigAndPlatform)
            {
                string tempFile = Path.Combine(Path.GetTempPath(), projectName);
                if (File.Exists(tempFile)) File.Delete(tempFile);

                XmlDocument projectDocument = new XmlDocument();
                projectDocument.Load(ResourceUtils.GetResourceStream(GetType(), projectName));
                projectDocument.Save(tempFile);

                FileInfo fileInfo = new FileInfo(tempFile);
                ProjectInfo projectInfo = new ProjectInfo(fileInfo, buildConfigAndPlatform, ConsoleLogger.Log);
                return projectInfo;
            }
		}
#endif
    }
}