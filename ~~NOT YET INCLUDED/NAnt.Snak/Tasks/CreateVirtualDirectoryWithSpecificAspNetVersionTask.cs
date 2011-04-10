using System;
using NAnt.Contrib.Tasks.Web;

using NAnt.Core;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

using Snak.Core;
using Snak.Utilities;

namespace Snak.Tasks
{
	/// <summary>
	/// 
	/// </summary>
	[TaskName("mkiisdirWithSpecificAspNetVersion")]
	public class CreateVirtualDirectoryWithSpecificAspNetVersionTask : CreateVirtualDirectory
	{
		private DotNetFrameworkInfo _dotNetFrameworkInfo = null;
		private string _propertyName = String.Empty;

		internal string AppRoot
		{
			get { return (this.ServerPath + this.VdirPath); }
		}

		internal Uri Uri
		{
			get { return new Uri("http://" + this.Server + this.VdirPath); }
		}

		internal DotNetFrameworkVersion DotNetFrameworkVersion
		{
			get{ return _dotNetFrameworkInfo.DotNetFrameworkVersion; }
			set
			{ 
				_dotNetFrameworkInfo = new DotNetFrameworkInfo(value);
			}
		}

		/// <summary>
		/// The property prefix that's used to load up all the project properties
		/// </summary>
		[TaskAttribute("property", Required=false)]
		[StringValidator(AllowEmpty=false)]
		public string PropertyName 
		{
			get { return _propertyName; }
			set { _propertyName = value; }
		}

		/// <summary>
        /// The .Net framework to change to, e.g v1.1.4322, v2.0.50727 etc
		/// </summary>
		[TaskAttribute("clrVersion", Required=true)]
		[StringValidator(AllowEmpty=false)]
		public string ClrVersion
		{
			get { return ( _dotNetFrameworkInfo.ClrVersion ); }
			set 
			{
				_dotNetFrameworkInfo = new DotNetFrameworkInfo(value);
			}
		}

		public CreateVirtualDirectoryWithSpecificAspNetVersionTask() : base() { }

		protected override void ExecuteTask()
		{
			base.ExecuteTask();

			ChangeWebSiteAspNetVersionTask changeWebSiteAspNetVersionTask = new ChangeWebSiteAspNetVersionTask(this);
			this.CopyTo(changeWebSiteAspNetVersionTask);
			changeWebSiteAspNetVersionTask.DotNetFrameworkInfo = new DotNetFrameworkInfo(this._dotNetFrameworkInfo.DotNetFrameworkVersion);
			changeWebSiteAspNetVersionTask.RecursivelyApplyChange = true;
			changeWebSiteAspNetVersionTask.Execute();

            string propertyName = ((this._propertyName == String.Empty) ? "Url" : this._propertyName + ".Url");
            
            this.Properties[propertyName] = Uri.ToString();

            // TODO: this has no test, what happens if you create a 2.0 virtual dir with a 1.1 app pool (inherited setting from parent), 
            // if the 1.1. apppool has threads in 1.1. its going to blow up... write a test 4 this
		}
	}
}
