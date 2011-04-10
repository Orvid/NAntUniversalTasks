using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;
using Snak.Utilities;

namespace Snak.Core.Everett
{
	/// <summary>
	/// Represents a project (and a particular configuration) within a Visual Studio 2003 solution
	/// </summary>
	public class ProjectInfo : IProjectInfo
	{
		#region Declarations

		private readonly FileInfo _projectFile;
		private readonly string _config;
		private readonly string _assemblyName;
		private readonly ProjectOutputType _outputType;
		private readonly string _defineConstants;
		private readonly string _outputPath;
		private readonly string _warningLevel;
		private readonly ProjectLanguage _projectLanguage;
		/// <summary>
		/// The project type (Local or Web)
		/// </summary>
		private readonly string _projectType;

		#endregion

		/// <summary>
		/// Creates an instance given a path to the project file,
		/// and a build configuration
		/// </summary>
		/// <param name="projectFile">The project file (.csproj / .vbproj)</param>
		/// <param name="config">eg "Debug", "Release" [CASE SENSITIVE]</param>
		public ProjectInfo(string projectFile, string config)
			:this(new FileInfo(projectFile), config)
		{
			
		}

		/// <summary>
		/// Creates an instance given the <see cref="FileInfo"/> to the project file,
		/// and a build configuration
		/// </summary>
		/// <param name="projectFile">The project file (.csproj / .vbproj)</param>
		/// <param name="config">eg "Debug", "Release" [CASE SENSITIVE]</param>
		public ProjectInfo(FileInfo projectFile, string config) {
			if (projectFile == null)
				throw new ArgumentNullException("projectFile");
			if (config == null || config == string.Empty)
				throw new ArgumentNullException("config");

			if (!projectFile.Exists)
				throw new FileNotFoundException("Failed to find project file", projectFile.FullName);
			if (!projectFile.Extension.EndsWith("proj"))
				throw new ArgumentException(string.Format("{0} is not a valid project file", projectFile.FullName), "projectFile");

			_projectFile = projectFile;
			_config = config;

			// load the project file up so we can start setting properties 
			XmlDocument projectDoc = LoadXmlDocument(_projectFile.FullName);

			XmlElement projectTypeNode = (XmlElement)projectDoc.SelectSingleNode("/*/*");
			_projectType = projectTypeNode.GetAttribute("ProjectType");
			_projectLanguage = (ProjectLanguage) Enum.Parse(typeof (ProjectLanguage), projectTypeNode.Name);

			XmlElement projectSettings = (XmlElement) projectDoc.SelectSingleNode("//Build/Settings");
			_assemblyName = projectSettings.GetAttribute("AssemblyName");
            if (_projectLanguage == ProjectLanguage.BIZTALK)
                _outputType = ProjectOutputType.Library;
            else
            {
                _outputType = (ProjectOutputType)Enum.Parse(typeof(ProjectOutputType), projectSettings.GetAttribute("OutputType"));
            }

			string findBuildConfigXPath = string.Format("//Build/Settings/Config[@Name='{0}']", config);
			XmlElement configSettings = (XmlElement) projectDoc.SelectSingleNode(findBuildConfigXPath);
            if (configSettings == null)
                throw new Exception(string.Format("Failed to locate project config settings for config {0}", config));

			_defineConstants = configSettings.GetAttribute("DefineConstants");
			_outputPath = configSettings.GetAttribute("OutputPath");
			_warningLevel = configSettings.GetAttribute("WarningLevel");
		}

		private ProjectInfo(XmlDocument projectDoc, FileInfo projectFile, string config)
		{
			_projectFile = projectFile;
			_config = config;

			// TODO: Eliminate nasty duplication without removing READONLY flag
			XmlElement projectTypeNode = (XmlElement)projectDoc.SelectSingleNode("/*/*");
			_projectType = projectTypeNode.GetAttribute("ProjectType");
			_projectLanguage = (ProjectLanguage) Enum.Parse(typeof (ProjectLanguage), projectTypeNode.Name);

			XmlElement projectSettings = (XmlElement) projectDoc.SelectSingleNode("//Build/Settings");
			_assemblyName = projectSettings.GetAttribute("AssemblyName");
			_outputType = (ProjectOutputType) Enum.Parse(typeof (ProjectOutputType), projectSettings.GetAttribute("OutputType"));

			string findBuildConfigXPath = string.Format("//Build/Settings/Config[@Name='{0}']", config);
			XmlElement configSettings = (XmlElement) projectDoc.SelectSingleNode(findBuildConfigXPath);
			_defineConstants = configSettings.GetAttribute("DefineConstants");
			_outputPath = configSettings.GetAttribute("OutputPath");
			_warningLevel = configSettings.GetAttribute("WarningLevel");
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
			get{ return false; } // 2003 doesn't have a web deployment project
		}

		public string ProjectType
		{
			get{ return _projectType;	}
		}

		public DotNetFrameworkVersion DotNetFrameworkVersion
		{
			get{ return DotNetFrameworkVersion.v1_1 ; }
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
			XmlDocument projectXml = LoadXmlDocument(_projectFile.FullName);
			XmlNodeList fileNodes = projectXml.SelectNodes(string.Format("//Files/Include/File[@BuildAction='{0}']", contentType));
			return GetProjectFiles(fileNodes);
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

#if( UNITTEST )
		[NUnit.Framework.TestFixture]
		public class ProjectInfoTester
		{
            // this assumes that the tests are always going to be run from the \bin\debug directory
			private const string PROJECT_NAME = @"..\..\..\TestArtifacts\SampleApplications\v1.1\C#\ClassLibrary\ClassLibrary1\ClassLibrary1.csproj";
            private string testProjectRoot = String.Empty;

			[NUnit.Framework.TestFixtureSetUp]
			public void Init()
			{
                testProjectRoot = Path.GetFullPath(Path.GetDirectoryName(PROJECT_NAME)).ToLower();   
			}

			[NUnit.Framework.Test(Description="")]
			public void TestLoadProject() {
				ProjectInfo project = new ProjectInfo(Path.GetFullPath(PROJECT_NAME), "Debug");

				AssertEqualIgnoreCase("ClassLibrary1", project.Name);
				AssertEqualIgnoreCase("Debug", project.Config);
                AssertEqualIgnoreCase("ClassLibrary1.dll", project.AssemblyFileName);
                AssertEqualIgnoreCase("ClassLibrary1", project.AssemblyName);
				AssertEqualIgnoreCase(".csproj", project.ProjectFileExtension);
				AssertEqualIgnoreCase(".cs", project.CodeFileExtension);
                AssertEqualIgnoreCase(testProjectRoot, project.ProjectDir);
				AssertEqualIgnoreCase(@"bin\debug\", project.OutputPath);
				AssertEqualIgnoreCase(Path.Combine(testProjectRoot, @"bin\debug\"), project.OutputPathFull);
                AssertEqualIgnoreCase(Path.Combine(testProjectRoot, @"bin\debug\ClassLibrary1.dll"), project.OutputFilePath);
				NUnit.Framework.Assert.AreEqual(ProjectOutputType.Library, project.OutputType);
			}

			private void AssertEqualIgnoreCase(string expected, string actual) {
				NUnit.Framework.Assert.AreEqual(expected.ToLower(), actual.ToLower());
			}

			[NUnit.Framework.Test]
			public void TestGetProjectFiles()
			{
				Hashtable expectedFiles = new Hashtable();
				expectedFiles.Add(ProjectFileType.Compile, @"AssemblyInfo.cs");
				expectedFiles.Add(ProjectFileType.Content, "ContentItem.txt");
				expectedFiles.Add(ProjectFileType.EmbeddedResource, "EmbeddedItem.bmp");
				expectedFiles.Add(ProjectFileType.None, "NoneItem.txt");

				// Get the resource stream out and stick it on disk for the test
				string projectName = "TestCSharpWinformsProject2003.csproj";
				string tempFile = Path.Combine(Path.GetTempPath(), projectName);
				XmlDocument projectDocument = new XmlDocument();
				projectDocument.Load(ResourceUtils.GetResourceStream(GetType(), projectName));
				projectDocument.Save(tempFile);

				ProjectInfo otherProject = new ProjectInfo(tempFile, "Debug");

				foreach (DictionaryEntry expectedFile in expectedFiles)
				{
					ProjectFileType type = (ProjectFileType)expectedFile.Key;
					string[] compileItems = otherProject.GetProjectFiles(type);

					if (type == ProjectFileType.None)
						NUnit.Framework.Assert.AreEqual(2, compileItems.Length);
					else
						NUnit.Framework.Assert.AreEqual(1, compileItems.Length);
					NUnit.Framework.Assert.AreEqual(expectedFile.Value, compileItems[0]);
				}
			}

			[NUnit.Framework.Test]
			[NUnit.Framework.ExpectedException(typeof(ArgumentException))]
			public void TestGetBadProject()
			{
				new ProjectInfo(new FileInfo(this.GetType().Assembly.Location), "Debug");
			}
		}

#endif
	}
}