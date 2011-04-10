using System;
using System.Globalization;
using System.IO;
using System.Xml;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

using Snak.Utilities;
using Snak.Common.Utilities;

namespace Snak.Tasks
{
	/// <summary>
	/// A simple way of deleting XML nodes from a document,
	/// like XmlPoke if it allowed setting empty strings...
	/// </summary>
	/// <remarks>This class gutted from nant's XmlPoke task</remarks>
	[TaskName("xmldelete")]
	public class XmlDeleteTask : XmlDocumentTask
	{
		#region Private Instance Fields
		private string _xPathExpression;
		#endregion Private Instance Fields

		#region Public Instance Properties

		/// <summary>
		/// The XPath expression used to select which nodes are to be modified.
		/// </summary>
		[TaskAttribute("xpath", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string XPath 
		{
			get { return _xPathExpression; }
			set { _xPathExpression = value; }
		}

		#endregion Public Instance Properties

        protected override bool ProcessDocument(XmlDocument document, XmlNamespaceManager nsManager)
        {
            XmlNodeList nodes = SelectNodes(XPath, document, nsManager);

            // don't bother trying to update any nodes or save the
            // file if no nodes were found in the first place.
            if (nodes.Count > 0)
            {
                UpdateNodes(nodes);
                return true;
            }
            return false;
        }

		/// <summary>
		/// Given a node list, removes the nodes from the XML document
		/// </summary>
		/// <param name="nodes">
		/// The list of nodes to replace the contents of.
		/// </param>
		private void UpdateNodes(XmlNodeList nodes) 
		{
			Log(Level.Verbose, "Deleting nodes");
                
			int index = 0;
			foreach (XmlNode node in nodes) 
			{
				Log(Level.Verbose, "Deleting node '{0}'.", index);
				XmlAttribute attribute = node as XmlAttribute;
				if (attribute!=null)
				{
					attribute.OwnerElement.RemoveAttributeNode(attribute);
				}
				else
				{
					node.ParentNode.RemoveChild(node);
				}
				index ++;
			}

			Log( Level.Verbose, "Updated all nodes successfully.");
		}
    }
}
