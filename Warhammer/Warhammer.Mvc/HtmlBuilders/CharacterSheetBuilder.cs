using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Warhammer.Core;
using Warhammer.Mvc.JsonObjects;

namespace Warhammer.Mvc.HtmlBuilders
{
	public class CharacterSheetBuilder
	{
		Dictionary<CharacterSheetStyle, string> EditableCharacterSheetBases { get; set; }
		Dictionary<CharacterSheetStyle, string> DisplayCharacterSheetBases { get; set; }

		private static readonly CharacterSheetBuilder _instance = new CharacterSheetBuilder();

		static CharacterSheetBuilder()
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
		}

		public CharacterSheetBuilder()
		{
			LoadCharacterSheetBases();	
		}

		public static CharacterSheetBuilder Instance
		{
			get
			{
				return _instance;
			}
		}

		protected void LoadCharacterSheetBases()
		{
			HttpContext context = HttpContext.Current;

			EditableCharacterSheetBases = new Dictionary<CharacterSheetStyle, string>();
			EditableCharacterSheetBases[CharacterSheetStyle.So82013Standard] = GetCharacterSheetBase(context.Server.MapPath("~/HtmlBits/So82013StandardEditable.txt"));
			EditableCharacterSheetBases[CharacterSheetStyle.So82013Fantasy] = GetCharacterSheetBase(context.Server.MapPath("~/HtmlBits/So82013FantasyEditable.txt"));

			DisplayCharacterSheetBases = new Dictionary<CharacterSheetStyle, string>();
			DisplayCharacterSheetBases[CharacterSheetStyle.So82013Standard] = GetCharacterSheetBase(context.Server.MapPath("~/HtmlBits/So82013Standard.txt"));
			DisplayCharacterSheetBases[CharacterSheetStyle.So82013Fantasy] = GetCharacterSheetBase(context.Server.MapPath("~/HtmlBits/So82013Fantasy.txt"));

		}

		protected string GetCharacterSheetBase(string fileName)
		{
			TextReader tr = new StreamReader(fileName);
			string sheet = tr.ReadToEnd();
			tr.Close();
			tr.Dispose();

			return sheet.Trim();
		}

		public string GetCharacterSheetEditorHtml(JsonCharacterSheet sheet)
		{
			//{Private}
			//{XP}
			//{Chips}
			//{SkillRows}
			//{TraitRows}
			//{SpecialityRows}
			//{OtherRows}
			//{BaseHitCapacity}
			//{CurrentDamage}
			//{MinorHitCapacity}
			//{SeriousHitCapacity}
			//{CriticalHitCapacity}
			//{WoundRows}
			//{Insanity}

			string sheetHtml = EditableCharacterSheetBases[(CharacterSheetStyle)sheet.CharacterSheetStyle];
			int hitCapacity = sheet.BaseHitCapacity + GetTraitModifierToHitCapacity(sheet.Traits["Big"]) + GetTraitModifierToHitCapacity(sheet.Traits["Robust"]);

			sheetHtml = sheetHtml.Replace("{Private}", sheet.IsPrivate ? "checked=\"checked\"" : string.Empty);
			sheetHtml = sheetHtml.Replace("{XP}", sheet.XP);
			sheetHtml = sheetHtml.Replace("{Chips}", sheet.Chips);
			sheetHtml = sheetHtml.Replace("{SkillRows}", GetEditableSkillsHtml(sheet.Skills));
			sheetHtml = sheetHtml.Replace("{TraitRows}", GetEditableTraitsHtml(sheet.Traits));
			sheetHtml = sheetHtml.Replace("{SpecialityRows}", GetEditableSpecialitiesHtml(sheet.Specialities));
			sheetHtml = sheetHtml.Replace("{OtherRows}", GetEditableOthersHtml(sheet.Other));
			sheetHtml = sheetHtml.Replace("{BaseHitCapacity}", sheet.BaseHitCapacity.ToString());
			sheetHtml = sheetHtml.Replace("{CurrentDamage}", sheet.CurrentDamage);
			sheetHtml = sheetHtml.Replace("{MinorHitCapacity}", hitCapacity.ToString());
			sheetHtml = sheetHtml.Replace("{SeriousHitCapacity}", (hitCapacity * 2).ToString());
			sheetHtml = sheetHtml.Replace("{CriticalHitCapacity}", (hitCapacity * 3).ToString());
			sheetHtml = sheetHtml.Replace("{WoundRows}", GetEditableWoundsHtml(sheet.MinorWounds, sheet.SeriousWounds));
			sheetHtml = sheetHtml.Replace("{Insanity}", sheet.Insanity);

			return sheetHtml;
		}

		public string GetDisplayCharacterSheet(JsonCharacterSheet sheet)
		{
			//{Private}
			//{XP}
			//{Chips}
			//{SkillRows}
			//{TraitRows}
			//{SpecialityRows}
			//{OtherRows}
			//{BaseHitCapacity}
			//{CurrentDamage}
			//{MinorHitCapacity}
			//{SeriousHitCapacity}
			//{CriticalHitCapacity}
			//{WoundRows}
			//{Insanity}

			string sheetHtml = DisplayCharacterSheetBases[(CharacterSheetStyle)sheet.CharacterSheetStyle];
			int hitCapacity = sheet.BaseHitCapacity + GetTraitModifierToHitCapacity(sheet.Traits["Big"]) + GetTraitModifierToHitCapacity(sheet.Traits["Robust"]);

			sheetHtml = sheetHtml.Replace("{Private}", sheet.IsPrivate ? "checked=\"checked\"" : string.Empty);
			sheetHtml = sheetHtml.Replace("{XP}", sheet.XP);
			sheetHtml = sheetHtml.Replace("{Chips}", sheet.Chips);
			sheetHtml = sheetHtml.Replace("{SkillRows}", GetGenericRowHtml("skills", sheet.Skills, false));
			sheetHtml = sheetHtml.Replace("{TraitRows}", GetGenericRowHtml("traits", sheet.Traits, false));
			sheetHtml = sheetHtml.Replace("{SpecialityRows}", GetGenericRowHtml("specialities", sheet.Specialities, true));
			sheetHtml = sheetHtml.Replace("{OtherRows}", GetGenericRowHtml("other items", sheet.Other, true));
			sheetHtml = sheetHtml.Replace("{BaseHitCapacity}", sheet.BaseHitCapacity.ToString());
			sheetHtml = sheetHtml.Replace("{CurrentDamage}", sheet.CurrentDamage);
			sheetHtml = sheetHtml.Replace("{MinorHitCapacity}", hitCapacity.ToString());
			sheetHtml = sheetHtml.Replace("{SeriousHitCapacity}", (hitCapacity * 2).ToString());
			sheetHtml = sheetHtml.Replace("{CriticalHitCapacity}", (hitCapacity * 3).ToString());
			sheetHtml = sheetHtml.Replace("{WoundRows}", GetWoundsHtml(sheet.MinorWounds, sheet.SeriousWounds));
			sheetHtml = sheetHtml.Replace("{Insanity}", sheet.Insanity);

			return sheetHtml;
		}

		private int GetTraitModifierToHitCapacity(string trait)
		{
			int modifier = 0;
			if (trait != null)
			{
				int.TryParse(trait, out modifier);
				if (modifier < 1 || modifier > 3)
				{
					modifier = 0;
				}
			}

			return modifier;
		}

		public string GetEditableSkillsHtml(Dictionary<string, string> skills)
		{
			StringBuilder html = new StringBuilder();

			foreach (KeyValuePair<string, string> skill in skills)
			{
				html.Append("<div class=\"SkillRow\">");
				html.Append("<span class=\"SkillRowTitle\">");
				html.Append(skill.Key);
				html.Append("</span>");
				html.Append("<span class=\"SkillRowContent\">");
				html.Append("<input type=\"text\" class=\"SkillTextBox\" value=\"");
				html.Append(skill.Value);
				html.Append("\" maxlength=\"3\" />");
				html.Append("</span><div class=\"Clear\"></div></div>");
			}

			return html.ToString();
		}

		public string GetEditableTraitsHtml(Dictionary<string, string> traits)
		{
			StringBuilder html = new StringBuilder();

			foreach (KeyValuePair<string, string> trait in traits)
			{
				html.Append("<div class=\"TraitRow\">");
				html.Append("<span class=\"TraitRowTitle\">");
				html.Append(trait.Key);
				html.Append("</span>");
				html.Append("<span class=\"TraitRowContent\">");
				html.Append("<div class=\"DotSelectorContainer\">");
				for (int i = 1; i <= 3; i++)
				{
					int traitValue = 0;
					int.TryParse(trait.Value.Trim(), out traitValue);

					html.Append("<div class=\"DotSelector");
					html.Append(i <= traitValue ? "Checked" : "Unchecked");
					html.Append("\" trait_name=\"");
					html.Append(trait.Key);
					html.Append("\" isHitCapacityMultiplier=\"");
					html.Append((trait.Key == "Big" || trait.Key == "Robust").ToString().ToLower());
					html.Append("\" trait_value=\"");
					html.Append(i);
					html.Append("\" dot_selected=\"");
					html.Append((traitValue == i).ToString().ToLower());
					html.Append("\" onclick=\"updateDotSelector('");
					html.Append(i);
					html.Append("', '");
					html.Append(trait.Key);
					html.Append("');\"></div>");
				}
				html.Append("</div></span></div>");
			}

			return html.ToString();
		}

		public string GetEditableSpecialitiesHtml(Dictionary<string, string> specialities)
		{
			StringBuilder html = new StringBuilder();

			foreach (KeyValuePair<string, string> speciality in specialities)
			{
				html.Append("<div class=\"SpecialityRow\">");
				html.Append("<span class=\"SpecialityRowTitle\">");
				html.Append("<input type=\"text\" class=\"SpecialityNameTextBox\" value=\"");
				html.Append(speciality.Key);
				html.Append("\" /></span>");
				html.Append("<span class=\"SpecialityRowContent\">");
				html.Append("<input type=\"text\" class=\"SpecialityValueTextBox\" value=\"");
				html.Append(speciality.Value);
				html.Append("\" maxlength=\"3\" /></span>");
                html.Append("<span class=\"RemoveSpecialityRow\" onclick=\"removeSpecialityRow(this);\"></span>");
				html.Append("<div class=\"Clear\"></div></div>");
			}

			return html.ToString();
		}

		public string GetEditableOthersHtml(Dictionary<string, string> others)
		{
			StringBuilder html = new StringBuilder();

			foreach (KeyValuePair<string, string> other in others)
			{
				html.Append("<div class=\"OtherRow\">");
				html.Append("<span class=\"OtherRowTitle\">");
				html.Append("<input type=\"text\" class=\"OtherNameTextBox\" value=\"");
				html.Append(other.Key);
				html.Append("\" /></span>");
				html.Append("<span class=\"OtherRowContent\">");
				html.Append("<input type=\"text\" class=\"OtherValueTextBox\" value=\"");
				html.Append(other.Value);
				html.Append("\" maxlength=\"3\" /></span>");
				html.Append("<span class=\"RemoveOtherRow\" onclick=\"removeOtherRow(this);\"></span>");
				html.Append("<div class=\"Clear\"></div></div>");
			}

			return html.ToString();
		}

		public string GetEditableWoundsHtml(Dictionary<string, string> minorWounds, Dictionary<string,string> seriousWounds)
		{
			StringBuilder html = new StringBuilder();

			foreach (KeyValuePair<string, string> wound in minorWounds)
			{
                html.Append("<tr class=\"BodyRow\" wound_row=\"");
				html.Append(wound.Key);
				html.Append("\"><td>");
				html.Append(wound.Key);
                html.Append("</td>");
				html.Append("<td><span><input type=\"text\" class=\"MinorWoundTextBox\" value=\"");
				html.Append(wound.Value);
				html.Append("\" maxlength=\"5\" /></span></td>");
                html.Append("<td><span><input type=\"text\" class=\"SeriousWoundTextBox\" value=\"");
				html.Append(seriousWounds[wound.Key]);
				html.Append("\" maxlength=\"5\" /></span></td></tr>");
			}

			return html.ToString();
		}

		public string GetGenericRowHtml(string name, Dictionary<string, string> items, bool includeBlanks)
		{
			StringBuilder html = new StringBuilder();

			foreach (KeyValuePair<string, string> item in items)
			{
				if (item.Key.Trim().Length > 0 && (item.Value.Trim().Length > 0 || includeBlanks))
				{
					html.Append("<div class=\"GenericRow\">");
					html.Append("<span class=\"GenericRowTitle\">");
					html.Append(item.Key);
					html.Append("</span>");
					html.Append("<span class=\"GenericRowContent\">");
					html.Append(item.Value);
					html.Append("</span><div class=\"Clear\"></div></div>");
				}
			}

			if (html.Length == 0)
			{
				html.Append("<span class=\"GenericEmpty\">No ");
				html.Append(name);
				html.Append(" entered.</span>");
			}

			return html.ToString();
		}

		public string GetWoundsHtml(Dictionary<string, string> minorWounds, Dictionary<string, string> seriousWounds)
		{
			StringBuilder html = new StringBuilder();

			foreach (KeyValuePair<string, string> wound in minorWounds)
			{
				html.Append("<tr class=\"BodyRow\"><td>");
				html.Append(wound.Key);
				html.Append("</td>");
				html.Append("<td><span class=\"WoundDisplay\">");
				html.Append(wound.Value);
				html.Append("</span></td>");
				html.Append("<td><span class=\"WoundDisplay\">");
				html.Append(seriousWounds[wound.Key]);
				html.Append("</span></td></tr>");
			}

			return html.ToString();
		}

	}
}