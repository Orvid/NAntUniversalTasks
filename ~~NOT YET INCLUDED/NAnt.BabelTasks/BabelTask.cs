using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Tasks;
using NAnt.Core.Types;
using NAnt.DotNet.Types;

using NAnt.Babel.Types;

namespace NAnt.Babel.Tasks
{
	[TaskName("babel")]
	public class BabelTask : ExternalProgramBase
	{
		private string _programArgs;
		private DirectoryInfo _babeldirectory;
		private FileInfo _inputFile;
		private FileInfo _keyFile;
		private string _keyPwd;
		private string _keyContainer;
		private FileInfo _outputFile;
		private FileInfo _logFile;
		private FileSet _rulesfiles;
		private FileInfo _mapOutFile;
		private FileSet _mapInFiles;
		private RegexItemCollection _takeFiles;
		private RegexItemCollection _skipFiles;
		private AssemblyFileSet _embedassemblies;
		private AssemblyFileSet _mergeassemblies;
		private string _stringencryptionalgorithm;
		private RegexItemCollection _noWarnings;
		private RegexItem[] _msilEncryption;

		private Hashtable _bag;

		public override string ProgramArguments
		{
			get 
			{ 
				return _programArgs; 
			}
		}

		[TaskAttribute("babeldirectory")]
		public DirectoryInfo BabelDirectory
		{
			get { return _babeldirectory; }
			set { _babeldirectory = value; }
		}

		[TaskAttribute("inputfile", Required = true)]
		public FileInfo InputFile
		{
			get { return _inputFile; }
			set { _inputFile = value; }
		}

		[TaskAttribute("keyfile")]
		public FileInfo KeyFile
		{
			get { return _keyFile; }
			set { _keyFile = value; }
		}

		[TaskAttribute("keypwd")]
		public string KeyPwd
		{
			get { return _keyPwd; }
			set { _keyPwd = value; }
		}

		[TaskAttribute("keycontainer")]
		[StringValidator(AllowEmpty = false)]
		public string KeyContainer
		{
			get { return _keyContainer; }
			set { _keyContainer = value; }
		}

		[TaskAttribute("outputfile")]
		public FileInfo OutputFile
		{
			get { return _outputFile; }
			set { _outputFile = value; }
		}

		[TaskAttribute("logfile")]
		public FileInfo LogFile
		{
			get { return _logFile; }
			set { _logFile = value; }
		}

		[BuildElement("rulesfiles")]
		public FileSet RulesFiles
		{
			get { return _rulesfiles; }
			set { _rulesfiles = value; }
		}

		[TaskAttribute("mapoutfile")]
		public FileInfo MapOutFile
		{
			get { return _mapOutFile; }
			set { _mapOutFile = value; }
		}

		[BuildElement("mapinfiles")]
		public FileSet MapInFiles
		{
			get { return _mapInFiles; }
			set { _mapInFiles = value; }
		}

		[BuildElementCollection("takefiles", "takefile")]
		public RegexItemCollection TakeFiles
		{
			get { return _takeFiles; }
		}

		[BuildElementCollection("skipfiles", "skipfile")]
		public RegexItemCollection SkipFiles
		{
			get { return _skipFiles; }			
		}

		[BuildElementCollection("nowarnings", "nowarning")]
		public RegexItemCollection NoWarnings
		{
			get { return _noWarnings; }			
		}

		[TaskAttribute("showlogo")]
		public bool ShowLogo
		{
			get { return (bool)_bag["showlogo"]; }
			set { _bag["showlogo"] = value; }
		}

		[TaskAttribute("verboselevel")]
		public int VerboseLevel
		{
			get { return (int)_bag["verboselevel"]; }
			set { _bag["verboselevel"] = value; }
		}

		[TaskAttribute("xapcompressionlevel")]
		public int XapCompressionLevel
		{
			get { return (int)_bag["xapcompressionlevel"]; }
			set { _bag["xapcompressionlevel"] = value; }
		}

		[TaskAttribute("showstatistics")]
		public bool ShowStatistics
		{
			get { return (bool)_bag["showstatistics"]; }
			set { _bag["showstatistics"] = value; }
		}

		[TaskAttribute("suppressildasm")]
		public bool SuppressIldasm
		{
			get { return (bool)_bag["suppressildasm"]; }
			set { _bag["suppressildasm"] = value; }
		}

		[TaskAttribute("deadcodeelimination")]
		public bool DeadCodeElimination
		{
			get { return (bool)_bag["deadcodeelimination"]; }
			set { _bag["deadcodeelimination"] = value; }
		}

		[BuildElement("embedassemblies")]
		public AssemblyFileSet EmbedAssemblies
		{
			get { return _embedassemblies; }
			set { _embedassemblies = value; }
		}

		[BuildElement("mergeassemblies")]
		public AssemblyFileSet MergeAssemblies
		{
			get { return _mergeassemblies; }
			set { _mergeassemblies = value; }
		}

		[TaskAttribute("internalize")]
		public bool Internalize
		{
			get { return (bool)_bag["internalize"]; }
			set { _bag["internalize"] = value; }
		}

		[TaskAttribute("copyattributes")]
		public bool CopyAttributes
		{
			get { return (bool)_bag["copyattributes"]; }
			set { _bag["copyattributes"] = value; }
		}

		[TaskAttribute("enableobfuscationagent")]
		public bool EnableObfuscationAgent
		{
			get { return (bool)_bag["enableobfuscationagent"]; }
			set { _bag["enableobfuscationagent"] = value; }
		}

		[TaskAttribute("flattennamespaces")]
		public bool FlattenNamespaces
		{
			get { return (bool)_bag["flattennamespaces"]; }
			set { _bag["flattennamespaces"] = value; }
		}

		[TaskAttribute("unicodenormalization")]
		public bool UnicodeNormalization
		{
			get { return (bool)_bag["unicodenormalization"]; }
			set { _bag["unicodenormalization"] = value; }
		}

		[TaskAttribute("obfuscatetypes")]
		public bool ObfuscateTypes
		{
			get { return (bool)_bag["obfuscatetypes"]; }
			set { _bag["obfuscatetypes"] = value; }
		}
		[TaskAttribute("obfuscateevents")]
		public bool ObfuscateEvents
		{
			get { return (bool)_bag["obfuscateevents"]; }
			set { _bag["obfuscateevents"] = value; }
		}
		[TaskAttribute("obfuscatemethods")]
		public bool ObfuscateMethods
		{
			get { return (bool)_bag["obfuscatemethods"]; }
			set { _bag["obfuscatemethods"] = value; }
		}
		[TaskAttribute("obfuscateproperties")]
		public bool ObfuscateProperties
		{
			get { return (bool)_bag["obfuscateproperties"]; }
			set { _bag["obfuscateproperties"] = value; }
		}
		[TaskAttribute("obfuscatefields")]
		public bool ObfuscateFields
		{
			get { return (bool)_bag["obfuscatefields"]; }
			set { _bag["obfuscatefields"] = value; }
		}

		[TaskAttribute("virtualfunctions")]
		public bool VirtualFunctions
		{
			get { return (bool)_bag["virtualfunctions"]; }
			set { _bag["virtualfunctions"] = value; }
		}

		[TaskAttribute("overloadedrenaming")]
		public bool OverloadedRenaming
		{
			get { return (bool)_bag["overloadedrenaming"]; }
			set { _bag["overloadedrenaming"] = value; }
		}

		[TaskAttribute("stringencryption")]
		public bool StringEncryption
		{
			get { return (bool)_bag["stringencryption"]; }
			set { _bag["stringencryption"] = value; }
		}

		[TaskAttribute("stringencryptionalgorithm")]
		[StringValidator(AllowEmpty = false)]
		public string StringEncryptionAlgorithm
		{
			get { return _stringencryptionalgorithm; }
			set { _stringencryptionalgorithm = value; }
		}

		[TaskAttribute("controlflowobfuscation")]
		public bool ControlFlowObfuscation
		{
			get { return (bool)_bag["controlflowobfuscation"]; }
			set { _bag["controlflowobfuscation"] = value; }
		}

		[TaskAttribute("emitinvalidopcodes")]
		public string EmitInvalidOpcodes
		{
			get { return (string)_bag["emitinvalidopcodes"]; }
			set { _bag["emitinvalidopcodes"] = value; }
		}

		[TaskAttribute("iliterations")]
		public int ILIterations
		{
			get { return (int)_bag["iliterations"]; }
			set { _bag["iliterations"] = value; }
		}

		[BuildElementArray("msilencryption")]
		public RegexItem[] MsilEncryption
		{
			get { return _msilEncryption; }
			set { _msilEncryption = value; }
		}

		[TaskAttribute("resourceencryption")]
		public bool ResourceEncryption
		{
			get { return (bool)_bag["resourceencryption"]; }
			set { _bag["resourceencryption"] = value; }
		}

		[TaskAttribute("suppressreflection")]
		public bool SuppressReflection
		{
			get { return (bool)_bag["suppressreflection"]; }
			set { _bag["suppressreflection"] = value; }
		}

		private string StandardBabelDirectory
		{
			get { return "Babel"; }
		}

		public override string ExeName
		{
			get
			{
				return GenerateFullPathToTool();
			}
			set
			{
				base.ExeName = value;
			}
		}

		public BabelTask()
		{
			_bag = new Hashtable();
			_takeFiles = new RegexItemCollection();
			_skipFiles = new RegexItemCollection();
			_noWarnings = new RegexItemCollection();
		}

		protected override void ExecuteTask()
		{
			GenerateCommandLineCommands();
			base.ExecuteTask();
		}

		private void GenerateCommandLineCommands()
		{
			StringWriter writer = new StringWriter();
			try
			{
				if (!this.InputFile.Exists)
					throw new BuildException(String.Format("Input file '{0}' does not exists.", this.InputFile.FullName));

				AppendFileNameIfNotNull(writer, this.InputFile.FullName);

				this.Log(Level.Info, "Babel Input File {0}", this.InputFile.FullName);

				StringEnumerator enumerator;
				AssemblyFileSet mergeAssemblies = this.MergeAssemblies;
 				if ((mergeAssemblies != null) && (mergeAssemblies.FileNames.Count > 0))
 				{
 					enumerator = mergeAssemblies.FileNames.GetEnumerator();
 					while (enumerator.MoveNext())
 					{
 						string path = enumerator.Current;
						AppendFileNameIfNotNull(writer, path);
 					}
 				}

				AssemblyFileSet embedAssemblies = this.EmbedAssemblies;
				if ((embedAssemblies != null) && (embedAssemblies.FileNames.Count > 0))
				{
					AppendSwitchFileCollection(writer, "--embed ", embedAssemblies.FileNames);
				}

				AppendSwitchFileIfNotNull(writer, "--out ", this.OutputFile);

				if (_bag.ContainsKey("xapcompressionlevel"))
					AppendSwitch(writer, "--compress ", this.XapCompressionLevel);

				AppendSwitchCollection(writer, "--take ", this.TakeFiles);
				AppendSwitchCollection(writer, "--skip ", this.SkipFiles);

				AppendSwitchFileIfNotNull(writer, "--logfile ", this.LogFile);

				FileSet rulesFiles = this.RulesFiles;
 				if ((rulesFiles != null) && (rulesFiles.FileNames.Count > 0))
 				{
					AppendSwitchFileCollection(writer, "--rules ", rulesFiles.FileNames);
 				}

				FileSet mapInFiles = this.MapInFiles;
 				if ((mapInFiles != null) && (mapInFiles.FileNames.Count > 0))
 				{
					AppendSwitchFileCollection(writer, "--mapin ", mapInFiles.FileNames);
 				}

				AppendSwitchFileIfNotNull(writer, "--mapout ", this.MapOutFile);

				if (_bag.ContainsKey("verboselevel"))
					AppendSwitch(writer, "-v ", this.VerboseLevel);

				AppendSwitchCollection(writer, "--nowarn ", this.NoWarnings);

				string no = "no";

				StringBuilder sbNo = new StringBuilder();
				StringBuilder sbYes = new StringBuilder();
				
				// Use command line option short form
				if (_bag.ContainsKey("enableobfuscationagent"))
					(EnableObfuscationAgent ? sbYes : sbNo).Append("a");
				if (_bag.ContainsKey("deadcodeelimination"))
					(DeadCodeElimination ? sbYes : sbNo).Append("d");
				if (_bag.ContainsKey("obfuscatetypes"))
					(ObfuscateTypes ? sbYes : sbNo).Append("t");
				if (_bag.ContainsKey("obfuscateevents"))
					(ObfuscateEvents ? sbYes : sbNo).Append("e");
				if (_bag.ContainsKey("obfuscatemethods"))
					(ObfuscateMethods ? sbYes : sbNo).Append("m");
				if (_bag.ContainsKey("obfuscateproperties"))
					(ObfuscateProperties ? sbYes : sbNo).Append("p");
				if (_bag.ContainsKey("obfuscatefields"))
					(ObfuscateFields ? sbYes : sbNo).Append("f");
				if (_bag.ContainsKey("unicodenormalization"))
					(UnicodeNormalization ? sbYes : sbNo).Append("u");
				if (_bag.ContainsKey("flattennamespaces"))
					(FlattenNamespaces ? sbYes : sbNo).Append("n");

				if (sbYes.Length > 0)
					AppendSwitch(writer, "-" + sbYes.ToString());

				if (sbNo.Length > 0)
					AppendSwitch(writer, "-" + no + sbNo.ToString());

				if (_bag.ContainsKey("showlogo"))
					AppendSwitch(writer, String.Format("--{0}logo", this.ShowLogo ? String.Empty : no));
				if (_bag.ContainsKey("virtualfunctions"))
					AppendSwitch(writer, String.Format("--{0}virtual", this.VirtualFunctions ? String.Empty : no));
				if (_bag.ContainsKey("overloadedrenaming"))
					AppendSwitch(writer, String.Format("--{0}overloaded", this.OverloadedRenaming ? String.Empty : no));
				if (_bag.ContainsKey("controlflowobfuscation"))
					AppendSwitch(writer, String.Format("--{0}controlflow", this.ControlFlowObfuscation ? String.Empty : no));

				if (this.EmitInvalidOpcodes != null)
				{
					bool emitInvalidEnabled = false;
					if (Boolean.TryParse(this.EmitInvalidOpcodes, out emitInvalidEnabled))
						AppendSwitch(writer, String.Format("--{0}invalidopcodes", emitInvalidEnabled ? String.Empty : no));
					else
						AppendSwitchIfNotNull(writer, "--invalidopcodes ", this.EmitInvalidOpcodes);
				}

				if (_bag.ContainsKey("resourceencryption"))
					AppendSwitch(writer, String.Format("--{0}resourceencryption", this.ResourceEncryption ? String.Empty : no));
				if (_bag.ContainsKey("suppressreflection"))
					AppendSwitch(writer, String.Format("--{0}reflection", this.SuppressReflection ? String.Empty : no));

				if (_bag.ContainsKey("iliterations"))
					AppendSwitch(writer, "--iliterations ", ILIterations);

				if (_bag.ContainsKey("internalize"))
					AppendSwitch(writer, String.Format("--{0}internalize", Internalize ? String.Empty : no));

				if (_bag.ContainsKey("copyattributes"))
					AppendSwitch(writer, String.Format("--{0}copyattrs", CopyAttributes ? String.Empty : no));

				if (_bag.ContainsKey("stringencryption"))
				{
					string algorithm = StringEncryptionAlgorithm;
					AppendSwitch(writer, String.Format("--{0}stringencryption{1}", 
						this.StringEncryption ? String.Empty : no,
						algorithm != null ? " " + algorithm : String.Empty));
				}

				RegexItem[] msilEncryption = this.MsilEncryption;
				if (msilEncryption != null)
				{
					foreach (RegexItem msilRegEx in msilEncryption)
					{
						bool msilEncryptionEnabled = false;
						if (msilRegEx != null && Boolean.TryParse(msilRegEx.Value, out msilEncryptionEnabled))
							AppendSwitch(writer, String.Format("--{0}msilencryption", msilEncryptionEnabled ? String.Empty : no));
						else
							AppendSwitchIfNotNull(writer, "--msilencryption ", msilRegEx.Value);
					}
				}

				if (_bag.ContainsKey("suppressildasm"))
					AppendSwitch(writer, String.Format("--{0}ildasm", this.SuppressIldasm ? String.Empty : no));
            
				AppendSwitchFileIfNotNull(writer, "--keyfile ", this.KeyFile);
				AppendSwitchIfNotNull(writer, "--keyname ", this.KeyContainer);
				AppendSwitchIfNotNull(writer, "--keypwd ", this.KeyPwd);

				if (_bag.ContainsKey("showstatistics"))
					AppendSwitch(writer, String.Format("--{0}stat", this.ShowStatistics ? String.Empty : no));

				_programArgs = writer.ToString();

				this.Log(Level.Debug, "Arguments: {0}", _programArgs);
			}
			finally
			{
				writer.Close();
			}
		}

		private string GenerateFullPathToTool()
		{
			DirectoryInfo babelDir = this.BabelDirectory;
			string path = null;
			if (babelDir != null)
				path = babelDir.FullName;

			if ((path == null) || (path.Length == 0))
			{
				string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				path = Path.Combine(programFiles, StandardBabelDirectory);
				if (!Directory.Exists(path))
				{
					programFiles = Environment.GetEnvironmentVariable("ProgramW6432");
					if (programFiles.Length == 0)
						path = Path.Combine(programFiles, StandardBabelDirectory);
				}
			}
			return Path.Combine(path, "babel.exe");
		}

		private void AppendSwitch(StringWriter writer, string value)
		{
			if (value != null)
				writer.Write(" " + value);
		}
		
		private void AppendSwitch(StringWriter writer, string name, int value)
		{
			writer.Write(String.Format(" {0}{1}", name, value));
		}

		private void AppendSwitchCollection(StringWriter writer, string name, StringCollection collection)
		{
			if (collection != null)
			{
				StringEnumerator enumerator = collection.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string item = enumerator.Current;
					AppendSwitchIfNotNull(writer, name, item);
				}
			}
		}

		private void AppendSwitchCollection(StringWriter writer, string name, RegexItemCollection collection)
		{
			if (collection != null)
			{
				RegexItemEnumerator enumerator = collection.GetEnumerator();
				while (enumerator.MoveNext())
				{
					RegexItem item = enumerator.Current;
					AppendSwitchIfNotNull(writer, name, item.Value);
				}
			}			
		}

		private void AppendSwitchFileCollection(StringWriter writer, string name, StringCollection collection)
		{
			if (collection != null)
			{
				StringEnumerator enumerator = collection.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string item = enumerator.Current;
					AppendSwitchFileIfNotNull(writer, name, item);
				}
			}
		}

		private void AppendFileNameIfNotNull(StringWriter writer, string file)
		{
			if (file != null)
			{
				writer.Write(String.Format(" \"{0}\"", file));
			}
		}

		private void AppendSwitchIfNotNull(StringWriter writer, string name, string value)
		{
			if (value != null)
			{
				writer.Write(String.Format(" {0}{1}", name, value));
			}
		}

		private void AppendSwitchFileIfNotNull(StringWriter writer, string name, string value)
		{
			if (value != null)
			{
				writer.Write(String.Format(" {0}\"{1}\"", name, value));
			}
		}

		private void AppendSwitchFileIfNotNull(StringWriter writer, string name, FileInfo value)
		{
			if (value != null)
			{
				writer.Write(String.Format(" {0} \"{1}\"", name, value.FullName));
			}
		}
	}
}
