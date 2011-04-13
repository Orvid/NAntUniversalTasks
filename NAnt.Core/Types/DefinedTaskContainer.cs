using System.Xml;
using NAnt.Core;

namespace Define.Tasks
{
    sealed internal class DefinedTaskContainer : TaskContainer
    {
        public static DefinedTaskContainer Create(Task parent, XmlNode taskNode)
        {
            DefinedTaskContainer container = new DefinedTaskContainer();
            container.Parent = parent;
            container.Project = parent.Project;
            container.NamespaceManager = parent.NamespaceManager;
            container.XmlNode = taskNode;

            return container;
        }
    }
}