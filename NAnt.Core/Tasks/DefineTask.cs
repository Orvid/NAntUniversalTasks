using System;
using System.Reflection;
using System.Reflection.Emit;
using Define.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Util;

namespace Define.Tasks
{
    [TaskName("define")]
    public class DefineTask : Task
    {
        private string _taskName;

        [TaskAttribute("name", Required = true), StringValidator(AllowEmpty = false)]
        public string TaskName
        {
            get { return _taskName; }
            set { _taskName = StringUtils.ConvertEmptyToNull(value); }
        }

        protected override bool CustomXmlProcessing
        {
            get { return true; }
        }

        // In here, we're defining a class, foo_Task, derived from our DefinedTask class. This allows us to write the bulk of the code
        // in real C#, rather than attempting to build it here.
        // This means that we have Task <- DefinedTask <- foo_Task.
        // We also need a module and an assembly to hold the class, so we do that, too.
        protected override void ExecuteTask()
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = _taskName + "_Assembly";

            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(_taskName + "_Module");

            // Now we've got an assembly and a module for the task to live in, we can define the actual task class.
            TypeBuilder typeBuilder = moduleBuilder.DefineType(_taskName + "_Task", TypeAttributes.Public);
            typeBuilder.SetParent(typeof(DefinedTask));

            // It needs a [TaskName] attribute.
            ConstructorInfo taskNameAttributeConstructor =
                typeof(TaskNameAttribute).GetConstructor(new Type[] { typeof(string) });
            CustomAttributeBuilder taskNameAttributeBuilder =
                new CustomAttributeBuilder(taskNameAttributeConstructor, new object[] { _taskName });
            typeBuilder.SetCustomAttribute(taskNameAttributeBuilder);

            // We're done. Create it.
            Type taskType = typeBuilder.CreateType();

            // Stash the XML in our static. We'll need it in DefinedTask later.
            DefinedTaskDefinitions.Add(_taskName, XmlNode);

            // Hook that up into NAnt.
            TaskBuilder taskBuilder = new TaskBuilder(taskType.Assembly, taskType.FullName);
            TypeFactory.TaskBuilders.Add(taskBuilder);
        }
    }
}