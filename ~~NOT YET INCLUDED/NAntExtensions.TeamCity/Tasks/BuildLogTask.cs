#region License
// Copyright (c) 2008, Bluewire Technologies Ltd.
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the Bluewire Technologies Ltd. nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS 
// BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.IO;
using System.Xml;

using NAnt.Core.Attributes;

using NAntExtensions.TeamCity.Common.BuildEnvironment;

namespace NAntExtensions.TeamCity.Tasks
{
	/// <summary>
	/// The base class for all tasks that operate on the teamcity-info.xml file.
	/// </summary>
	public abstract class BuildLogTask : TeamCityTask
	{
		const string TeamCityInfoFileName = "teamcity-info.xml";
		string _teamCityInfoPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="BuildLogTask"/> class.
		/// </summary>
		/// <param name="buildEnvironment">The build environment.</param>
		protected BuildLogTask(IBuildEnvironment buildEnvironment) : base(buildEnvironment)
		{
		}

		/// <summary>
		/// Allows you to override the path to the <c>teamcity-info.xml</c> file. By default, the path to the file is detected
		/// automatically.
		/// </summary>
		[TaskAttribute("teamcity-info-path")]
		public string TeamCityInfoPath
		{
			get
			{
				if (String.IsNullOrEmpty(_teamCityInfoPath))
				{
					_teamCityInfoPath = GetDefaultTeamCityInfoPath();
				}
				return _teamCityInfoPath;
			}
			set { _teamCityInfoPath = value; }
		}

		/// <summary>
		/// Creates a statistic node with a key and a value.
		/// </summary>
		/// <param name="doc">The teamcity-info.xml document to create the node in.</param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected static XmlElement CreateStatisticValueNode(XmlDocument doc, string key, string value)
		{
			XmlElement element = FindOrCreateElement(doc, key);
			element.SetAttribute("key", key);
			element.SetAttribute("value", value);
			return element;
		}

		static XmlElement FindOrCreateElement(XmlDocument doc, string key)
		{
			XmlNode existingNodeForKey = doc.SelectSingleNode(String.Format("/build/statisticValue[@key='{0}']", key));

			if (existingNodeForKey != null && existingNodeForKey.NodeType == XmlNodeType.Element)
			{
				return (XmlElement) existingNodeForKey;
			}

			return doc.CreateElement("statisticValue");
		}

		/// <summary>
		/// Returns the build node of the document.
		/// </summary>
		/// <param name="doc">The teamcity-info.xml document.</param>
		/// <returns></returns>
		protected static XmlElement GetBuildNode(XmlDocument doc)
		{
			XmlElement documentElement = doc.DocumentElement;
			if (documentElement == null)
			{
				documentElement = doc.CreateElement("build");
				doc.AppendChild(documentElement);
			}
			return documentElement;
		}

		/// <summary>
		/// Returns the default path to teamcity-info.xml.
		/// </summary>
		/// <returns></returns>
		protected string GetDefaultTeamCityInfoPath()
		{
			if (Properties == null || !Properties.Contains("teamcity.build.checkoutDir"))
			{
				return TeamCityInfoFileName;
			}

			string checkoutDir = Properties["teamcity.build.checkoutDir"];
			return Path.Combine(checkoutDir, TeamCityInfoFileName);
		}

		/// <summary>
		/// Loads the teamcity-info.xml document.
		/// </summary>
		/// <returns>An <see cref="XmlDocument"/> representing the current teamcity-info.xml document.</returns>
		protected XmlDocument LoadTeamCityInfo()
		{
			XmlDocument document = new XmlDocument();
			if (File.Exists(TeamCityInfoPath))
			{
				document.Load(TeamCityInfoPath);
			}
			return document;
		}

		/// <summary>
		/// Saves the teamcity-info.xml document.
		/// </summary>
		/// <param name="teamCityInfoXml">The teamcity-info.xml document to save.</param>
		protected void SaveTeamCityInfo(XmlDocument teamCityInfoXml)
		{
			teamCityInfoXml.Save(TeamCityInfoPath);
		}
	}
}