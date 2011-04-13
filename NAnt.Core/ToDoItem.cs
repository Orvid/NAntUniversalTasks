
using System;
using System.Xml;
using System.Xml.Serialization;

namespace NAnt.ToDo
{
	[XmlRoot("ToDoItem")]
	public class ToDoItem
	{
		public string Message { get; set; }
		public string File { get; set; }
		public int Line { get; set; }
		public string Type { get; set; }
	}
}
