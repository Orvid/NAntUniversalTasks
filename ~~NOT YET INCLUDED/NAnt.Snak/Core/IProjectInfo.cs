using System.IO;

namespace Snak.Core
{
	public interface IProjectInfo
	{
		/// <summary>
		/// The name of the project
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The name of the project
		/// </summary>
		string ProjectName { get; }

		FileInfo ProjectFile { get; }

		/// <summary>
		/// The project file's file extension (csproj / vbproj)
		/// </summary>
		string ProjectFileExtension { get; }

		/// <summary>
		/// The file extension for code files for this project type (eg .cs 
		/// </summary>
		string CodeFileExtension { get; }

		/// <summary>
		/// The path to the project root
		/// </summary>
		string ProjectDir { get; }

		/// <summary>
		/// The configuration that this project's settings are being read from (Debug, Release etc...)
		/// </summary>
		string Config { get; }

		/// <summary>
		/// The language that the project is written in
		/// </summary>
		ProjectLanguage ProjectLanguage { get; }

		/// <summary>
		/// The (simple) name of the project's output assembly (no file extension)
		/// </summary>
		string AssemblyName { get; }

		/// <summary>
		/// The name of the project's output assembly with the file extension
		/// </summary>
		string AssemblyFileName { get; }

		/// <summary>
		/// The complete path to the project's output assembly
		/// </summary>
		string AssemblyPath { get; }

		/// <summary>
		/// The type of project it is (dll / exe)
		/// </summary>
		ProjectOutputType OutputType { get; }

		/// <summary>
		/// Any build constants setup for this project configuration
		/// </summary>
		string DefineConstants { get; }

		/// <summary>
		/// The relative path to the project's output folder (for this configuration)
		/// </summary>
		string OutputPath { get; }

		/// <summary>
		/// The full path to the project output folder (for this configuration)
		/// </summary>
		string OutputPathFull { get; }

		/// <summary>
		/// The complete path to the project's output assembly
		/// </summary>
		string OutputFilePath { get; }

		string WarningLevel { get; }

		/// <summary>
		/// Determines whether this project is a web project or not
		/// </summary>
		bool IsWebProject{ get; }

		/// <summary>
		/// Determines whether this project is a web deployment project
		/// </summary>
		/// <remarks>In VS 2005 these have a .wdproj ext</remarks>
		bool IsWebDeploymentProject{ get; }

		/// <summary>
		/// The project's configuration file (web.config or app.config)
		/// </summary>
		string ConfigFile { get; }

		/// <summary>
		/// Either a specially named 'nunit.config' in the project root,
		/// or the same as the project's configuration file, or
		/// </summary>
		string TestingConfigFile { get; }

		/// <summary>
		/// Gets an array of all the files contained within the project
		/// All paths are project-relative
		/// </summary>
		string[] GetProjectFiles();
		/// <summary>
		/// Gets an array of all the files contained within the project
		/// that match the content type provided (eg 'Content' for web pages).
		/// All paths are project-relative
		/// </summary>
		string[] GetProjectFiles(ProjectFileType contentType);

		/// <summary>
		/// The .net framework the project is targeted towards.
		/// </summary>
		DotNetFrameworkVersion DotNetFrameworkVersion{ get; }
	}
}