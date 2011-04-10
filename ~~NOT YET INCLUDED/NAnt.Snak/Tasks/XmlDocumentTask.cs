using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using System.Xml;
using System.IO;
using NAnt.Core.Types;
using NAnt.Core.Attributes;
using Snak.Common.Utilities;

namespace Snak.Tasks
{
    public abstract class XmlDocumentTask : Task
    {
        #region Private Instance Fields

        private FileInfo _xmlFile;
        private XmlNamespaceCollection _namespaces = new XmlNamespaceCollection();
        private Boolean _ignoreReadonly;
        private XmlNamespaceManager _nsManager;
        protected Level LogLevelForSelectMatch = Level.Info;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The name of the file that contains the XML document that is going 
        /// to be poked.
        /// </summary>
        [TaskAttribute("file", Required = true)]
        public FileInfo XmlFile
        {
            get { return _xmlFile; }
            set { _xmlFile = value; }
        }

        /// <summary>
        /// Namespace definitions to resolve prefixes in the XPath expression.
        /// </summary>
        [BuildElementCollection("namespaces", "namespace")]
        public XmlNamespaceCollection Namespaces
        {
            get { return _namespaces; }
            set { _namespaces = value; }
        }

        /// <summary>
        /// Determines whether the readonly status of the target file is ignored
        /// </summary>
        [TaskAttribute("ignoreReadonly", Required = false)]
        public Boolean IgnoreReadonly
        {
            get { return _ignoreReadonly; }
            set { _ignoreReadonly = value; }
        }

        #endregion Public Instance Properties

        /// <summary>
		/// Executes the task.
		/// </summary>
		protected override void ExecuteTask() 
		{
			// ensure the specified xml file exists
			if (!XmlFile.Exists) 
			{
				throw new BuildException(string.Format("{0} not found", XmlFile.FullName), Location);
			}

			try 
			{
				XmlDocument document = LoadDocument(XmlFile.FullName);

				_nsManager = new XmlNamespaceManager(document.NameTable);
				foreach (XmlNamespace xmlNamespace in Namespaces) 
				{
					if (xmlNamespace.IfDefined && !xmlNamespace.UnlessDefined) 
					{
						_nsManager.AddNamespace(xmlNamespace.Prefix, xmlNamespace.Uri);
					}
				}

                bool updated = ProcessDocument(document, _nsManager);
				if (updated) 
					SaveDocument(document, XmlFile.FullName);
			} 
			catch (BuildException) 
			{
				throw;
			} 
			catch (Exception ex) 
			{
				throw new BuildException("Failed", Location, ex);
			}
		}

        protected abstract bool ProcessDocument(XmlDocument document, XmlNamespaceManager nsManager);

        #region Private Instance Methods

        /// <summary>
        /// Loads an XML document from a file on disk.
        /// </summary>
        /// <param name="fileName">
        /// The file name of the file to load the XML document from.
        /// </param>
        /// <returns>
        /// An <see cref="System.Xml.XmlDocument" /> containing
        /// the document object model representing the file.
        /// </returns>
        private XmlDocument LoadDocument(string fileName)
        {
            XmlDocument document = null;

            try
            {
                Log(Level.Verbose, "Attempting to load XML document"
                    + " in file '{0}'.", fileName);

                document = new XmlDocument();
                document.Load(fileName);

                Log(Level.Verbose, "XML document in file '{0}' loaded"
                    + " successfully.", fileName);
                return document;
            }
            catch (Exception ex)
            {
                throw new BuildException("Failed to load file " + fileName, Location, ex);
            }
        }

        /// <summary>
        /// Given an XML document and an expression, returns a list of nodes
        /// which match the expression criteria.
        /// </summary>
        /// <param name="xpath">
        /// The XPath expression used to select the nodes.
        /// </param>
        /// <param name="document">
        /// The XML document that is searched.
        /// </param>
        /// <param name="nsMgr">
        /// An <see cref="XmlNamespaceManager" /> to use for resolving namespaces 
        /// for prefixes in the XPath expression.
        /// </param>
        /// <returns>
        /// An <see cref="XmlNodeList" /> containing references to the nodes 
        /// that matched the XPath expression.
        /// </returns>
        protected XmlNodeList SelectNodes(string xpath, XmlDocument document, XmlNamespaceManager nsMgr)
        {
            XmlNodeList nodes = null;

            try
            {
                Log(Level.Verbose, "Selecting nodes with XPath"
                    + " expression '{0}'.", xpath);

                nodes = document.SelectNodes(xpath, nsMgr);

                // report back how many we found if any. If not then
                // log a message saying we didn't find any.
                if (nodes.Count != 0)
                {
                    Log(LogLevelForSelectMatch, "Found '{0}' nodes matching"
                        + " XPath expression '{1}'.", nodes.Count, xpath);
                }
                else
                {
                    Log(Level.Warning, "No matching nodes were found"
                        + " with XPath expression '{0}'.", xpath);
                }
                return nodes;
            }
            catch (Exception ex)
            {
                throw new BuildException("Failed to select nodes", Location, ex);
            }
        }

        /// <summary>
        /// Saves the XML document to a file.
        /// </summary>
        /// <param name="document">The XML document to be saved.</param>
        /// <param name="fileName">The file name to save the XML document under.</param>
        private void SaveDocument(XmlDocument document, string fileName)
        {
            try
            {
                Log(Level.Verbose, "Attempting to save XML document"
                    + " to '{0}'.", fileName);

                if (_ignoreReadonly)
                    using (new WritableFileScope(new FileInfo(fileName)))
                        document.Save(fileName);
                else
                    try
                    {
                        document.Save(fileName);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        string message = (IgnoreReadonly)
                            ? ex.Message
                            : ex.Message + " [try again with ignoreReadonly=true]";

                        throw new BuildException(message, Location, ex);
                    }

                Log(Level.Verbose, "XML document successfully saved"
                    + " to '{0}'.", fileName);
            }
            catch (Exception ex)
            {
                throw new BuildException("Failed to save file " + fileName, Location, ex);
            }
        }

        #endregion Private Instance Methods
    }
}
