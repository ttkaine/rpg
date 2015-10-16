using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Warhammer.Mvc.JsonObjects
{
	public class JsonRecipient
	{
		public string Content { get; set; }

		public JsonRecipient(int id, string name, bool isChecked)
		{
			Content = string.Format("<span class=\"RecipientListItem\"><input id=\"chkPlayer{0}\" player_id=\"{0}\" class=\"RecipientCheckBox\" player_name=\"{2}\" type=\"checkbox\" {1} />{2}</span>", id, isChecked ? "checked=\"checked\"" : string.Empty, name);
		}
	}
}