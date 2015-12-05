using System;

namespace Warhammer.Mvc.JsonObjects
{
	[Serializable]
	public class JsonCharacterListItem
	{
		public int ID { get; set; }
		public string Name { get; set; }

		public JsonCharacterListItem()
		{
		}
	}
}