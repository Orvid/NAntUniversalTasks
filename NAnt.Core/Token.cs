
using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAnt.ToDo
{
	[ElementName("Token")]
	public class Token : Element
	{

		[TaskAttribute("Value")]
		[StringValidator(AllowEmpty = false)]
		public string Value { get; set; }
	}
}
