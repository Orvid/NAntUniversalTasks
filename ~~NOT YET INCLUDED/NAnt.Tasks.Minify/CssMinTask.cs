using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

namespace NAnt.Tasks.Minify
{
    [TaskName("cssmin")]
    public class CssMinTask : Task
    {
        #region Private Fields

        private DirectoryInfo _toDirectory;
        private bool _flatten;
        private FileSet _cssFiles = new FileSet();

        #endregion

        #region Public Properties

        [TaskAttribute("todir", Required = true)]
        public virtual DirectoryInfo ToDirectory
        {
            get { return _toDirectory; }
            set { _toDirectory = value; }
        }

        [TaskAttribute("flatten")]
        [BooleanValidator()]
        public virtual bool Flatten
        {
            get { return _flatten; }
            set { _flatten = value; }
        }

        [BuildElement("fileset", Required = true)]
        public virtual FileSet CssFiles
        {
            get { return _cssFiles; }
            set { _cssFiles = value; }
        }

        #endregion
        protected override void InitializeTask(XmlNode taskNode)
        {
            if (_toDirectory == null)
                throw new BuildException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The 'todir' attribute must be set to specify the output directory of the minified JS files."),
                    Location);

            if (!_toDirectory.Exists)
                _toDirectory.Create();

            if (_cssFiles == null)
                throw new BuildException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The <fileset> element must be used to specify the JS files to minify."),
                    Location);
        }
        private string GetDestPath(DirectoryInfo srcBase, FileInfo srcFile)
        {
            string destPath = string.Empty;

            if (_flatten)
            {
                destPath = Path.Combine(_toDirectory.FullName, srcFile.Name);
            }
            else
            {
                if (srcFile.FullName.IndexOf(srcBase.FullName, 0) != -1)
                    destPath = srcFile.FullName.Substring(srcBase.FullName.Length);
                else
                    destPath = srcFile.Name;

                if (destPath[0] == Path.DirectorySeparatorChar)
                    destPath = destPath.Substring(1);

                destPath = Path.Combine(_toDirectory.FullName, destPath);
            }

            return destPath;
        }
        protected override void ExecuteTask()
        {
            if (_cssFiles.BaseDirectory == null)
                _cssFiles.BaseDirectory = new DirectoryInfo(Project.BaseDirectory);

            Log(Level.Info, "Minifying {0} Css file(s) to '{1}'.", _cssFiles.FileNames.Count, _toDirectory.FullName);

            foreach (string srcPath in _cssFiles.FileNames)
            {
                FileInfo srcFile = new FileInfo(srcPath);

                if (srcFile.Exists)
                {
                    string destPath = GetDestPath(_cssFiles.BaseDirectory, srcFile);

                    DirectoryInfo destDir = new DirectoryInfo(Path.GetDirectoryName(destPath));

                    if (!destDir.Exists)
                        destDir.Create();

                    Log(Level.Verbose, "Minifying '{0}' to '{1}'.", srcPath, destPath);

                    if (srcPath.Equals(destPath))
                    {
                        string tmpFile = Path.GetTempFileName();
                        if (File.Exists(tmpFile))
                        {
                            File.Delete(tmpFile);
                        }
                        File.Copy(srcPath, tmpFile);
                        //new JavaScriptMinifier().Minify(tmpFile, destPath);
                        File.WriteAllText(destPath, CssMinifier.CssMinify(File.ReadAllText(tmpFile)));
                        File.Delete(tmpFile);
                    }
                    else
                    {
                        File.WriteAllText(destPath, CssMinifier.CssMinify(File.ReadAllText(srcPath)));

                    }
                }
                else
                {
                    throw new BuildException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Could not find file '{0}' to minify.",
                            srcFile.FullName),
                        Location);
                }
            }
        }
    }
}
