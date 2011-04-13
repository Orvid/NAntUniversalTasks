using System;
using System.Xml;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Define.Tasks
{
    public class DefinedTask : Task
    {
        protected DefinedTask()
        {
        }

        protected override void InitializeXml(XmlNode elementNode, PropertyDictionary properties,
                                              FrameworkInfo framework)
        {
            // Remember this for later, but don't do anything right now.
            XmlNode = elementNode;
        }

        protected override void ExecuteTask()
        {
            Log(Level.Info, "Executing defined task.");

            // When you call a defined task, you can define any XML attributes that you want.
            // We take those, and convert them to properties called "this.whatever". This makes
            // them available to the enclosed XML with predictable names.
            foreach (XmlAttribute attr in XmlNode.Attributes)
            {
                string thisPropertyName = "this." + attr.Name;
                string thisPropertyValue = Properties.ExpandProperties(attr.Value, Location);

                Properties.Add(thisPropertyName, thisPropertyValue);
            }

            // DefineTask created a class derived from DefinedTask. Pick up the [TaskName] attribute from that derived class.
            TaskNameAttribute taskNameAttribute =
                (TaskNameAttribute)Attribute.GetCustomAttribute(GetType(), typeof(TaskNameAttribute));

            // We stashed the enclosed XML node in the DefintedTaskDefinitions global collection; pick it up now.
            XmlNode originalDefinitionNode = DefinedTaskDefinitions.Find(taskNameAttribute.Name);

            // NAnt conveniently already has a way to nest tasks: TaskContainer. This is how <if>, <foreach/do>, etc., work.
            // We need to define our own derived class to get access to some of the protected properties.
            DefinedTaskContainer containedTasks = DefinedTaskContainer.Create(this, originalDefinitionNode);

            try
            {
                containedTasks.Execute();
            }
            finally
            {
                foreach (XmlAttribute attr in XmlNode.Attributes)
                {
                    Properties.Remove("this." + attr.Name);
                }
            }
        }
    }
}