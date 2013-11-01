using System;
using System.Runtime.Serialization;

namespace ZUMOAPPNAME
{
	public class ToDoItem
	{
		public int Id { get; set; }

		[DataMember (Name = "text")]
		public string Text { get; set; }

		[DataMember (Name = "complete")]
		public bool Complete { get; set; }
	}
}

