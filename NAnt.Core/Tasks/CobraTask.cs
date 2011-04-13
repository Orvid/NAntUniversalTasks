using System.Globalization;
using System.Text.RegularExpressions;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;
using NAnt.DotNet.Types;
using System;
using System.IO;

namespace NAnt.DotNet.Tasks
{
	[TaskName("cobra")]
	class CobraTask : CompilerBase
	{
		protected bool embedRuntime;
        [TaskAttributeAttribute("embedRuntime")]
        [BooleanValidator()]
        public bool EmbedRuntime
        {
            get { return embedRuntime; }
            set { embedRuntime = value; }
        }
		
        protected bool optimize;
        [TaskAttributeAttribute("optimize")]
        [BooleanValidator()]
        public bool Optimize
        {
            get { return optimize; }
            set { optimize = value; }
        }
        
		protected override Regex ClassNameRegex
		{
			get
			{
				return new Regex("class\\s+(?<class>\\w+");
			}
		}
			
		protected override Regex NamespaceRegex
		{
			get
			{
				return new Regex("namespace\\s+(?<namespace>\\w+");
			}
		}
		
		public override String Extension
		{
			get
			{
				return "cobra";
			}
		}
		
		protected String _programArguments;
		public override String ProgramArguments
		{
			get
			{
				return _programArguments;
			}
		}
		
        private String otherParams;
		[TaskAttributeAttribute("otherParams")]
        public String OtherParams
        {
            get { return otherParams; }
            set { otherParams = value; }
        }
        
		protected override void WriteOptions(TextWriter writer)
		{
            WriteArgument(writer, "-t:\"" + base.OutputTarget + "\"");
            if (base.OutputTarget == "exe" || base.OutputTarget == "winexe")
                WriteArgument(writer, "-compile");
            if (base.Debug)
				WriteArgument(writer, "-d");
            if (this.Optimize)
                WriteArgument(writer, "-turbo");
            if (base.OutputFile != null)
					WriteArgument(writer, "-out:\"" + base.OutputFile.FullName + "\"");
            if (this.EmbedRuntime)
            {
                WriteArgument(writer, "-ert:yes");
            }
            else
            {
                WriteArgument(writer, "-ert:no");
            }
            if (this.OtherParams != "")
                WriteArgument(writer, this.OtherParams);
            if (base.References != null)
            {
                foreach(String i in base.References.FileNames)
                {
                    WriteArgument(writer, "-ref:\"" + i +"\"");
                }
            }
            if (base.Sources != null)
            {
                foreach(String i in base.Sources.FileNames)
                {
                    WriteArgument(writer, i);
                }
            }
            _programArguments = writer.ToString();


		}
		
        void WriteArgument(TextWriter writer, String message)
        {
            writer.Write(message + " ");
        }			
	}	
}