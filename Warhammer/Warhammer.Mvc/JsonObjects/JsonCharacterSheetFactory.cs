using System.Collections.Generic;
using Warhammer.Core;

namespace Warhammer.Mvc.JsonObjects
{
	public static class JsonCharacterSheetFactory
	{
		public static JsonCharacterSheet GetCharacterSheet(CharacterSheetStyle style)
		{
			switch (style)
			{
				case CharacterSheetStyle.So82013Standard:
					return GetSo82013StandardCharacterSheet();					
				case CharacterSheetStyle.So82013Fantasy:
					return GetSo82013FantasyCharacterSheet();
					
				default:
					return GetSo82013StandardCharacterSheet();				
			}
		}

		private static JsonCharacterSheet GetSo82013CoreCharacterSheet()
		{
			JsonCharacterSheet sheet = new JsonCharacterSheet();

			sheet.Traits["Agile"] = string.Empty;
			sheet.Traits["Beautiful"] = string.Empty;
			sheet.Traits["Big"] = string.Empty;
			sheet.Traits["Charming"] = string.Empty;
			sheet.Traits["Cunning"] = string.Empty;
			sheet.Traits["Dextrous"] = string.Empty;
			sheet.Traits["Intelligent"] = string.Empty;
			sheet.Traits["Perceptive"] = string.Empty;
			sheet.Traits["Quick"] = string.Empty;
			sheet.Traits["Robust"] = string.Empty;
			sheet.Traits["Strong"] = string.Empty;
			sheet.Traits["Wilful"] = string.Empty;

			sheet.MinorWounds["Head"] = string.Empty;
			sheet.MinorWounds["Body"] = string.Empty;
			sheet.MinorWounds["Right Arm"] = string.Empty;
			sheet.MinorWounds["Left Arm"] = string.Empty;
			sheet.MinorWounds["Right Leg"] = string.Empty;
			sheet.MinorWounds["Left Leg"] = string.Empty;

			sheet.SeriousWounds["Head"] = string.Empty;
			sheet.SeriousWounds["Body"] = string.Empty;
			sheet.SeriousWounds["Right Arm"] = string.Empty;
			sheet.SeriousWounds["Left Arm"] = string.Empty;
			sheet.SeriousWounds["Right Leg"] = string.Empty;
			sheet.SeriousWounds["Left Leg"] = string.Empty;

			return sheet;
		}

		private static JsonCharacterSheet GetSo82013StandardCharacterSheet()
		{
			JsonCharacterSheet sheet = GetSo82013CoreCharacterSheet();
			sheet.CharacterSheetStyle = (int)CharacterSheetStyle.So82013Standard;

			sheet.Skills["Academics"] = string.Empty;
			sheet.Skills["Animal Handling"] = string.Empty;
			sheet.Skills["Athletics"] = string.Empty;
			sheet.Skills["Awareness"] = string.Empty;
			sheet.Skills["Chicanery"] = string.Empty;
			sheet.Skills["Coercion"] = string.Empty;
			sheet.Skills["Computing"] = string.Empty;
			sheet.Skills["Construction"] = string.Empty;
			sheet.Skills["Deception"] = string.Empty;
			sheet.Skills["Diplomacy"] = string.Empty;
			sheet.Skills["Driving"] = string.Empty;
			sheet.Skills["Investigation"] = string.Empty;
			sheet.Skills["Medicine"] = string.Empty;
			sheet.Skills["Melee"] = string.Empty;
			sheet.Skills["Occult"] = string.Empty;
			sheet.Skills["Performance"] = string.Empty;
			sheet.Skills["Shooting"] = string.Empty;
			sheet.Skills["Stealth"] = string.Empty;
			sheet.Skills["Streetwise"] = string.Empty;
			sheet.Skills["Survival"] = string.Empty;
			sheet.Skills["Tenacity"] = string.Empty;

			return sheet;
		}

		private static JsonCharacterSheet GetSo82013FantasyCharacterSheet()
		{
			JsonCharacterSheet sheet = GetSo82013CoreCharacterSheet();
			sheet.CharacterSheetStyle = (int)CharacterSheetStyle.So82013Fantasy;

			sheet.Skills["Academics"] = string.Empty;
			sheet.Skills["Animal Handling"] = string.Empty;
			sheet.Skills["Athletics"] = string.Empty;
			sheet.Skills["Awareness"] = string.Empty;
			sheet.Skills["Chicanery"] = string.Empty;
			sheet.Skills["Coercion"] = string.Empty;
			sheet.Skills["Construction"] = string.Empty;
			sheet.Skills["Deception"] = string.Empty;
			sheet.Skills["Diplomacy"] = string.Empty;
			sheet.Skills["Investigation"] = string.Empty;
			sheet.Skills["Medicine"] = string.Empty;
			sheet.Skills["Melee"] = string.Empty;
			sheet.Skills["Occult"] = string.Empty;
			sheet.Skills["Performance"] = string.Empty;
			sheet.Skills["Seamanship"] = string.Empty;
			sheet.Skills["Shooting"] = string.Empty;
			sheet.Skills["Stealth"] = string.Empty;
			sheet.Skills["Streetwise"] = string.Empty;
			sheet.Skills["Survival"] = string.Empty;
			sheet.Skills["Tenacity"] = string.Empty;

			return sheet;
		}

		public static JsonCharacterSheet RefreshCharacterSheetFromSource(JsonCharacterSheet sheet)
		{
			JsonCharacterSheet newSheet = GetCharacterSheet((CharacterSheetStyle)sheet.CharacterSheetStyle);
			JsonCharacterSheet source = GetCharacterSheet((CharacterSheetStyle)sheet.CharacterSheetStyle);

			foreach (KeyValuePair<string, string> skill in source.Skills)
			{
				if (sheet.Skills.ContainsKey(skill.Key))
				{
					newSheet.Skills[skill.Key] = sheet.Skills[skill.Key];
				}
			}

			foreach (KeyValuePair<string, string> trait in source.Traits)
			{
				if (sheet.Traits.ContainsKey(trait.Key))
				{
					newSheet.Traits[trait.Key] = sheet.Traits[trait.Key];
				}
			}

			newSheet.Specialities = sheet.Specialities;
			newSheet.Other = sheet.Other;
			newSheet.CharacterSheetStyle = sheet.CharacterSheetStyle;
			newSheet.XP = sheet.XP;
			newSheet.Chips = sheet.Chips;
			newSheet.Insanity = sheet.Insanity;
			newSheet.BaseHitCapacity = sheet.BaseHitCapacity;
			newSheet.CurrentDamage = sheet.CurrentDamage;
			newSheet.IsPrivate = sheet.IsPrivate;

			foreach (KeyValuePair<string,string> wound in source.MinorWounds)
			{
				if (sheet.MinorWounds.ContainsKey(wound.Key))
				{
					newSheet.MinorWounds[wound.Key] = sheet.MinorWounds[wound.Key];
				}
			}

			foreach (KeyValuePair<string, string> wound in source.SeriousWounds)
			{
				if (sheet.SeriousWounds.ContainsKey(wound.Key))
				{
					newSheet.SeriousWounds[wound.Key] = sheet.SeriousWounds[wound.Key];
				}
			}

			return newSheet;
		}

	}
}
