using System;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NAnt.ToDo
{
	[XmlRoot("ToDo")]
	public class ToDo
	{
		[XmlArray("Items")]
		[XmlArrayItem("Item")]
		public List<ToDoItem> Items { get; set; }
		
		public ToDo()
		{
			this.Items = new List<ToDoItem>();
		}
	}
}
