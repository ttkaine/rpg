using System;

namespace Warhammer.Mvc.JsonObjects
{
	[Serializable]
	public class JsonPost
	{
		public string ID { get; set; }
		public string Content { get; set; }

		public JsonPost()
		{
			Content = string.Empty;
		}
	}
}