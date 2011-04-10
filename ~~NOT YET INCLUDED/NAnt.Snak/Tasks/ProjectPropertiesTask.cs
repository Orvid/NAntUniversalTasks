using System;
using System.Reflection;
using NAnt.Core;
using NAnt.Core.Attributes;

using Snak.Core;
using Snak.Utilities;

namespace Snak.Tasks
{
	/// <summary>
	/// Loads NAnt properties with properties from a Visual Studio project
	/// </summary>
	[TaskName("projectproperties")]
	public class ProjectPropertiesTask : ProjectTask
	{
		private string _propertyName;
		/// <summary>
		/// Caches the list of all properties on VSProjectInfo
		/// </summary>
		readonly static PropertyInfo[] ProjectInfoProperties = typeof(IProjectInfo).GetProperties(BindingFlags.Instance | BindingFlags.Public);

		/// <summary>
		/// The property prefix that's used to load up all the project properties
		/// </summary>
		[TaskAttribute("property", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string PropertyName 
		{
			get { return _propertyName; }
			set { _propertyName = value; }
		}

		protected override void ExecuteTask()
		{
			IProjectInfo project = GetProject();
			SetupProperties(project, PropertyName);
		}

		/// <summary>
		/// Loop through all the properties on the project and set their values
		/// up as NAnt properties (using the prefix provided)
		/// </summary>
		internal void SetupProperties(IProjectInfo project, string name) 
		{
			string nantPropertyName;
			foreach (PropertyInfo property in ProjectInfoProperties) 
			{
				nantPropertyName = name + "." + TaskUtils.LowerFirst(property.Name);
				Properties[nantPropertyName] = TaskUtils.SafeToString(property.GetValue(project, new object[]{}));
				Log(Level.Verbose, "Setting {0} to {1}", nantPropertyName, Properties[nantPropertyName]);
			}
		}

#if (UNITTEST)
		[NUnit.Framework.TestFixture]
		public class ProjectPropertiesTaskTester
		{

		}
#endif
	}
}
