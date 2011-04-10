using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using System.Collections;
using NAnt.Core.Attributes;

namespace Snak.Tasks
{
    [TaskName("dumpproperties")]
    public class DumpPropertiesTask : Task
    {
        protected override void ExecuteTask()
        {
            foreach (DictionaryEntry item in Properties)
                if (!item.Key.ToString().StartsWith("nant."))
                    Log(Level.Info, "{0}={1}", item.Key, item.Value);
        }
    }
}
