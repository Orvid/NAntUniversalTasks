using System.Collections;
using System.Xml;

namespace Define.Tasks
{
    internal class DefinedTaskDefinitions
    {
        private static readonly Hashtable _inner = new Hashtable();

        public static void Add(string taskName, XmlNode taskNode)
        {
            _inner.Add(taskName, taskNode);
        }

        public static XmlNode Find(string taskName)
        {
            return (XmlNode)_inner[taskName];
        }
    }
}