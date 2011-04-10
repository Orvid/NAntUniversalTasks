using System;
using System.DirectoryServices;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NantTasks
{
    [TaskName("createapppool")]
    public class AppPoolTask : Task
    {
        public AppPoolTask()
        {
            Server = "Localhost";
            MaxProcess = 1;
            MemoryThreshold = 2097152;
            RequestsThreshold = 35000;
        }

        [TaskAttribute("password")]
        public string Password { get; set; }

        [TaskAttribute("username")]
        public string Username { get; set; }

        [TaskAttribute("name", Required = true)]
        public string PoolName { get; set; }

        [TaskAttribute("max-memory")]
        public int MemoryThreshold { get; set; }

        [TaskAttribute("max-requests")]
        public int RequestsThreshold { get; set; }

        [TaskAttribute("max-processes")]
        public int MaxProcess { get; set; }

        [TaskAttribute("server")]
        public string Server { get; set; }

        protected override void ExecuteTask()
        {
            string metabasePath = "IIS://" + Server + "/W3SVC/AppPools";
            Console.WriteLine("\nCreating application pool named {0}/{1}:", metabasePath, PoolName);

            var apppools = new DirectoryEntry(metabasePath);
            DirectoryEntry newpool = apppools.Children.Add(PoolName, "IIsApplicationPool");

            newpool.InvokeSet("MaxProcesses", new Object[] {MaxProcess});
            newpool.InvokeSet("IdleTimeout", new Object[] {0});
            newpool.InvokeSet("PeriodicRestartTime", new Object[] {0});
            newpool.InvokeSet("PeriodicRestartRequests", new Object[] {RequestsThreshold});
            newpool.InvokeSet("PeriodicRestartMemory", new Object[] {MemoryThreshold});
            newpool.InvokeSet("AppPoolIdentityType", new Object[] {3});
            newpool.InvokeSet("WAMUserName", new Object[] {Username});
            newpool.InvokeSet("WAMUserPass", new Object[] {Password});
            newpool.Invoke("SetInfo", null);

            newpool.CommitChanges();
        }
    }
}