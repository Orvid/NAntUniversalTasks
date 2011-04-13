
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.Specialized;

using NAnt.Core;
using NAnt.Core.Attributes;

using Debug = System.Diagnostics.Debug;

namespace NAnt.ToDo
{

	[TaskName("ToDo")]
	public class ToDoTask : Task
	{
	
		private TokenCollection _Tokens = new TokenCollection();
		[BuildElementCollection("Tokens","Token", Required = false)]
		public TokenCollection Tokens
		{
			get { return _Tokens; }
			set { _Tokens = value; }
		}
		
		private string _SourceFolder;
		[TaskAttribute("source", Required = true)]
		public string SourceFolder
		{
			get{ return _SourceFolder; }
			set{ _SourceFolder = value; }
		}
		
		private string _OutputFile;
		[TaskAttribute("output", Required = true)]
		public string OutputFile
		{
			get{ return _OutputFile; }
			set{ _OutputFile = value; }
		}
		
		private string _SearchPattern;
		/// <summary>
		/// The search string to match against the names of files in path. 
		/// </summary>
		[TaskAttribute("searchpattern", Required = true)]
		public string SearchPattern
		{
			get{ return _SearchPattern; }
			set{ _SearchPattern = value; }
		}

		public ToDoTask()
		{
			Tokens.Add(new Token(){ Value = "TODO"});
			Tokens.Add(new Token(){ Value = "HACK"});
			Tokens.Add(new Token(){ Value = "FIXME"});
		}
		
		protected override void ExecuteTask()
		{
			//TODO: First Test to do Item
			//FIXME: First Fix me Item
			//HACK: First Hack Item

			Project.Log(Level.Info, "Using Source Folder " + _SourceFolder);
			ToDo todo = new ToDo();
			foreach(Token token in Tokens)
			{			
				try
				{
					string[] patterns =  _SearchPattern.Split(';');
					foreach(string pattern in patterns)
					{
						String[] files = Directory.GetFiles(_SourceFolder,pattern,SearchOption.AllDirectories);
						foreach(string file in files)
						{
                            Project.Log(Level.Debug, file);
                            string[] lines = File.ReadAllLines(file);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                string szRegex = string.Format(@"\/\/\s{{0,1}}{0}:(?<comment>.*)", token.Value);

                                Regex re = new Regex(szRegex, RegexOptions.IgnoreCase);
                                MatchCollection matchCol = re.Matches(lines[i]);
                                if (matchCol.Count > 0)
                                {
                                    foreach (Match match in matchCol)
                                    {
                                        string todoMessage = match.Groups["comment"].Value.Trim();
                                        todo.Items.Add(new ToDoItem() { Message = todoMessage, File = file, Type = token.Value, Line = i + 1 });
                                    }
                                }
                            }
						}
					}		
					try
					{
						if(File.Exists(_OutputFile))
						   File.Delete(_OutputFile);
						using(StreamWriter writer = File.CreateText(_OutputFile))
						{
							writer.Write(SerializeObject(todo,typeof(ToDo)));
						}
					}
					catch(Exception ex)
					{
						Project.Log(Level.Error,"Failed to write to output file");
						Project.Log(Level.Error,ex.Message);	
						Project.Log(Level.Error,ex.StackTrace);
					}
				}
				catch(Exception ex)
				{
					Project.Log(Level.Error,ex.Message);
					Project.Log(Level.Error,ex.StackTrace);
					if(ex.InnerException != null)
					{
						Project.Log(Level.Error,ex.InnerException.Message);
						Project.Log(Level.Error,ex.InnerException.StackTrace);
					}
				}
			}		
		}
		/// <summary>
        /// Serialize Object to a string
        /// </summary>
        /// <param name="pObject">Input Object</param>
        /// <param name="type">Type of Object</param>
        /// <returns>
        ///     string of serialize object
        /// </returns>
        public static string SerializeObject(Object pObject, Type type)
        {
            try
            {
                string XmlizedString = string.Empty;

                MemoryStream memoryStream = new MemoryStream();
                XmlSerializer xs = new XmlSerializer(type);
				//NonXsiTextWriter xmlTextWriter = new NonXsiTextWriter(memoryStream, Encoding.GetEncoding("ISO-8859-1"));
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.GetEncoding("ISO-8859-1"));
                xs.Serialize(xmlTextWriter, pObject);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                StreamReader reader = new StreamReader(memoryStream, Encoding.GetEncoding("ISO-8859-1"));
                XmlizedString = reader.ReadToEnd();
#if(DEBUG)
                //Debug.WriteLine(XmlizedString);
#endif
				return XmlizedString;
            }
#if(DEBUG)
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return string.Empty;
            }
#else
            catch
            {
                return string.Empty;
            }
#endif
        }
	}
}
