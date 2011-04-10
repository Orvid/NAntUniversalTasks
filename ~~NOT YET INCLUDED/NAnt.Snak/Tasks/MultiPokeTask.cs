using System;
using System.Text;
using NAnt.Core;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;
using NAnt.Core.Attributes;
using System.Diagnostics;

namespace Snak.Tasks
{
    /// <summary>
    /// Multipoke provides a way of mapping multiple nant properties onto
    /// related nodes in an xml document (eg a config file) without having
    /// to resort to long, error-prone lists of xml pokes.
    /// </summary>
    /// <remarks>Properties that match a given regex pattern are
    /// substituted into the provided xpath pattern, and the nodes that
    /// are matched by the result are updated with the value of the original
    /// nant property. Provision is given for the name of the nant property
    /// to have a prefix (or suffix) that's not present in the node name.
    /// <example>
    /// <code>
    /// Some examples will help. For the following app.config:
    ///	<configuration><appSettings>
    ///	 <add key="Settings.LogPath" value="C:\temp" />
    ///	 <add key="Settings.LogPathB" value="C:\otherfolder" />
    ///	 <add key="ErrorMessage" value="Bad something" />
    ///	 <add key="ErrorMessageB" value="Bad something Else" />
    /// </appSettings></configuration>
    /// 
    /// Simple case - matching nant properties directly against appSettings values of the same name:
    /// <code><![CDATA[
    /// <multipoke file="somexml.config" propertyPattern="Settings\.[\w]+" xpath="\\appSettings\add[@Name='{0}']\Value"/>
    /// ]]></code>
    /// For the example file above, the preceding example would replace the value of Settings.LogPath with the
    /// nant property 'Settings.LogPath', and Settings.LogPathB with the value of Settings.LogPathB.
    /// Note that only NAnt properties that match the regex supplied are mapped, so even if we had
    /// a nant property named 'ErrorMessage', in this example the config for 'ErrorMessage' would be untouched.
    /// 
    /// Variation - matching without the nant property prefix (or suffix):
    /// Commonly, the prefix on the nant property wouldn't be present in the actual xml node name,
    /// for example if you had a nant property Settings.ErrorMessage that you wanted to replace
    /// the 'ErrorMessage' node in the example above.
    /// In this case can provide a capture match within your regex to specify the substring of the property name
    /// to use in the xpath
    /// eg
    /// <code><![CDATA[
    /// <multipoke file="somexml.config" propertyPattern="Setting\.([\w]+)" xpath="\\appSettings\add[@Name='{0}']\Value"/>
    /// ]]></code>   
    /// When a capture match is present (as above) it is used in the xpath *in place of the full property name*,
    /// so in this case we're replacing ErrorMessage / ErrorMessageB
    /// with the value of nant properties Settings.ErrorMessage / Settings.ErrorMessageB
    /// </remarks>
    [TaskName("multipoke")]
    public class MultiPokeTask : XmlDocumentTask
    {
        private string _xpath;
        private string _propertyPattern;

        [TaskAttribute("xpath", Required = true)]
        public string XPath
        {
            get { return _xpath; }
            set { _xpath = value; }
        }

        [TaskAttribute("propertyPattern", Required = true)]
        public string PropertyPattern
        {
            get { return _propertyPattern; }
            set { _propertyPattern = value; }
        }

        protected override bool ProcessDocument(XmlDocument document, XmlNamespaceManager nsManager)
        {
            LogLevelForSelectMatch = Level.Verbose;

            Regex propertyNameMatcher = new Regex(PropertyPattern);
            bool anyNodesUpdated = false;

            foreach (DictionaryEntry property in this.Properties)
            {
                string propertyName = property.Key.ToString();
                Match match = propertyNameMatcher.Match(propertyName);
                if (match.Success)
                {
                    //Debugger.Break();
                    string propertyNameInDocument = (match.Groups.Count > 1) ? match.Groups[1].Value : propertyName;   // TODO: This should be the first capture if present
                    string xpath = String.Format(_xpath, propertyNameInDocument);
                    Log(Level.Verbose, "Matching nodes with xpath {0}", xpath);
                    XmlNodeList nodes = this.SelectNodes(xpath, document, nsManager);
                    foreach (XmlNode node in nodes)
                    {
                        if (node.NodeType == XmlNodeType.Element || node.NodeType == XmlNodeType.Attribute)
                        {
                            // If this is the first match, write out which file we're updating
                            if (!anyNodesUpdated)
                                Log(Level.Info, "Updating nodes in {0}", this.XmlFile.Name);

                            string newValue = RenderPropertyValue(property.Value);
                            Log(Level.Info, "\tSetting {0} -> '{1}'", xpath, newValue);
                            node.Value = RenderPropertyValue(newValue);
                            anyNodesUpdated = true;
                        }
                    }
                }
            }

            return anyNodesUpdated;
        }

        private string RenderPropertyValue(object value)
        {
            if (value == null) return string.Empty;
            return value.ToString();    // Not very sophisticated, but will do for now
        }
    }
}
      
