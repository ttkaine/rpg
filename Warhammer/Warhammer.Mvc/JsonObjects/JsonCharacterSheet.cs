using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Warhammer.Mvc.JsonObjects
{
	[Serializable]
	public class JsonCharacterSheet
	{
		public int CharacterSheetStyle { get; set; }
		public Dictionary<string, string> Skills { get; set; }
		public Dictionary<string, string> Traits { get; set; }
		public Dictionary<string, string> Specialities { get; set; }
		public Dictionary<string, string> Other { get; set; }

		public int BaseHitCapacity { get; set; }
		public string CurrentDamage { get; set; }

		public Dictionary<string, string> MinorWounds { get; set; }
		public Dictionary<string, string> SeriousWounds { get; set; }

		public string XP { get; set; }
		public string Chips { get; set; }
		public string Insanity { get; set; }

		public bool IsPrivate { get; set; }

		public JsonCharacterSheet()
		{
			Skills = new Dictionary<string, string>();
			Traits = new Dictionary<string, string>();
			Specialities = new Dictionary<string, string>();
			Other = new Dictionary<string, string>();

			BaseHitCapacity = 6;
			MinorWounds = new Dictionary<string, string>();
			SeriousWounds = new Dictionary<string, string>();

			CurrentDamage = string.Empty;
			XP = string.Empty;
			Chips = string.Empty;
			Insanity = string.Empty;
		}
	}
}
