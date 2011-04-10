using NantTasks;
using NUnit.Framework;

namespace NantTasksTests
{
    [TestFixture]
    public class AppPoolTaskTest
    {
        [Test]
        public void CreateAppPoolWithDefaultSettings()
        {
            var task = new AppPoolTask();
            task.Execute();
        }
    }
}