// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

// Gerry Shaw (gerry_shaw@yahoo.com)
// Gert Driesen (driesen@users.sourceforge.net)

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

using NAnt.Core.Attributes;
using NAnt.Core.Extensibility;

namespace NAnt.Core {
    public class TaskBuilder : ExtensionBuilder {
        #region Public Instance Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TaskBuilder" /> class
        /// for the specified <see cref="Task" /> class in the specified
        /// <see cref="Assembly" />.
        /// </summary>
        /// <remarks>
        /// An <see cref="ExtensionAssembly" /> for the specified <see cref="Assembly" />
        /// is cached for future use.
        /// </remarks>
        /// <param name="assembly">The <see cref="Assembly" /> containing the <see cref="Task" />.</param>
        /// <param name="className">The class representing the <see cref="Task" />.</param>
        public TaskBuilder (Assembly assembly, string className)
            : this (ExtensionAssembly.Create (assembly), className) {
        }

        #endregion Public Instance Constructors

        #region Internal Instance Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TaskBuilder" /> class
        /// for the specified <see cref="Task" /> class in the specified
        /// <see cref="ExtensionAssembly" />.
        /// </summary>
        /// <param name="extensionAssembly">The <see cref="ExtensionAssembly" /> containing the <see cref="Task" />.</param>
        /// <param name="className">The class representing the <see cref="Task" />.</param>
        internal TaskBuilder(ExtensionAssembly extensionAssembly, string className) : base (extensionAssembly) {
            _className = className;
        }

        #endregion Internal Instance Constructors

        #region Public Instance Properties

        /// <summary>
        /// Gets the name of the <see cref="Task" /> class that can be created
        /// using this <see cref="TaskBuilder" />.
        /// </summary>
        /// <value>
        /// The name of the <see cref="Task" /> class that can be created using
        /// this <see cref="TaskBuilder" />.
        /// </value>
        public string ClassName {
            get { return _className; }
        }

        /// <summary>
        /// Gets the name of the task which the <see cref="TaskBuilder" />
        /// can create.
        /// </summary>
        /// <value>
        /// The name of the task which the <see cref="TaskBuilder" /> can 
        /// create.
        /// </value>
        public string TaskName {
            get {
                if (_taskName == null) {
                    TaskNameAttribute taskNameAttribute = (TaskNameAttribute) 
                        Attribute.GetCustomAttribute(Assembly.GetType(ClassName), 
                        typeof(TaskNameAttribute));
                    _taskName = taskNameAttribute.Name;
                }
                return _taskName;
            }
        }

        #endregion Public Instance Properties

        #region Public Instance Methods

        [ReflectionPermission(SecurityAction.Demand, Flags=ReflectionPermissionFlag.NoFlags)]
        public Task CreateTask() {
            Task task = (Task) Assembly.CreateInstance(
                ClassName, 
                true, 
                BindingFlags.Public | BindingFlags.Instance,
                null,
                null,
                CultureInfo.InvariantCulture,
                null);
            IPluginConsumer pluginConsumer = task as IPluginConsumer;
            if (pluginConsumer != null) {
                TypeFactory.PluginScanner.RegisterPlugins(pluginConsumer);
            }
            return task;
        }

        #endregion Public Instance Methods

        #region Private Instance Fields

        private readonly string _className;
        private string _taskName;

        #endregion Private Instance Fields
    }
}
