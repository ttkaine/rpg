using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warhammer.Mvc.JsonObjects
{
	public class JsonCharacterDetails
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string CharacterSheet { get; set; }

		public JsonCharacterDetails()
		{
			Name = "Unknown";
			Description = "No Description Available";
			CharacterSheet = "No Character Sheet Available";
		}
	}
}