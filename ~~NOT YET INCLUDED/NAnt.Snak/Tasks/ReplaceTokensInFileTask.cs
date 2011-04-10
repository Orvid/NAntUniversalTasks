using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core.Attributes;
using NAnt.Core;
using System.IO;
using NAnt.Core.Types;
using NAnt.Core.Filters;

namespace Snak.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// <replaceTokensInFile>
    ///   <fileset basedir=".">
    /// 	<include name="*.config"/>
    ///   </fileset>		
    ///   <filterchain>
    ///     <replacestring from="REPLACE_TOKEN_1" to="replacedToken1"/>
    ///     <replacestring from="REPLACE_TOKEN_2" to="replacedToken2"/>
    ///   </filterchain>			
    /// </replaceTokensInFile>
    /// ]]>
    /// </example>
    [TaskName("replaceTokensInFile")]
    public class ReplaceTokensInFileTask : Task
    {
        private bool _makeFileWritableIfNecessary = true;
        private FileSet _ReplaceFileSet;
        private FilterChain _filters;

        [TaskAttribute("makeFileWritableIfNecessary", Required = false)]
        public bool MakeFileWritableIfNecessary
        {
            get { return _makeFileWritableIfNecessary; }
            set { _makeFileWritableIfNecessary = value; }
        }

        /// <summary>
        /// The set of files in which to replace token with value.
        /// </summary>
        [BuildElement("fileset", Required = true)]
        public FileSet ReplaceFileSet
        {
            get { return _ReplaceFileSet; }
            set { _ReplaceFileSet = value; }
        }

        /// <summary>
        /// Chain of filters used to alter the file's content as it is moved.
        /// </summary>
        [BuildElement("filterchain")]
        public FilterChain Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        protected override void ExecuteTask()
        {
            foreach (string file in ReplaceFileSet.FileNames)
            {
                // first make a copy of the file, during the call to CopyFile below nant will uses that 
                // as it applies its filters as aposed to the original.
                // This prevents nant form locking the file we want to update.
                FileInfo theFile = new FileInfo(file);
                string tempFileName = file + ".temp";
                theFile.IsReadOnly = ! _makeFileWritableIfNecessary;
                theFile.CopyTo(tempFileName);
                NAnt.Core.Util.FileUtils.CopyFile(tempFileName, file, this.Filters, null, null);
                File.Delete(tempFileName);
            }
        }
    }
}
