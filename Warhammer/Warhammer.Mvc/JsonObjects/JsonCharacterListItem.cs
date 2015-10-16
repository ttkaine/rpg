using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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