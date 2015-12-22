using System.Text;
using Warhammer.Core;
using Warhammer.Core.RoleplayViewModels;

namespace Warhammer.Mvc.HtmlBuilders
{
	public class PostBuilder
	{
		//private Dictionary<PostType, string> Templates { get; set; }

		//public PostBuilder(string path)
		//{
		//    SetupTemplates(path);
		//}

		//protected void SetupTemplates(string path)
		//{
		//    Templates = new Dictionary<PostType, string>();

		//    Templates[PostType.OutOfCharacter] = GetTemplate(path + "\\OutOfCharacter.txt");
		//    Templates[PostType.InCharacter] = GetTemplate(path + "\\InCharacter.txt");
		//    Templates[PostType.GmInCharacter] = GetTemplate(path + "\\GmInCharacter.txt");
		//    Templates[PostType.DiceRoll] = GetTemplate(path + "\\DiceRoll.txt");
		//}

		//protected string GetTemplate(string fileName)
		//{
		//    TextReader tr = new StreamReader(fileName);
		//    string template = tr.ReadToEnd();
		//    tr.Close();
		//    tr.Dispose();

		//    return template.Trim();
		//}

		public PostBuilder()
		{
		}

		public string GetHtmlForPost(PostViewModel post)
		{
			return GetHtmlForPost(post, false, -1, false);
		}

		public string GetHtmlForPost(PostViewModel post, bool includeEditControls, int playerId, bool playerIsGm)
		{
			string html = string.Empty;
			switch (post.PostType)
			{
				case (int)PostType.OutOfCharacter:
					if (post is TextPostViewModel)
					{
						html = GetHtmlForOutOfCharacterPost((TextPostViewModel)post, includeEditControls, playerId, playerIsGm);	
					}
					break;

				case (int)PostType.InCharacter:
					if (post is TextPostViewModel)
					{
						if (post.IsPostedByGm)
						{
							html = GetHtmlForGmInCharacterPost((TextPostViewModel)post, includeEditControls, playerId, playerIsGm);	
						}
						else
						{
							html = GetHtmlForInCharacterPost((TextPostViewModel)post, includeEditControls, playerId, playerIsGm);	
						}						
					}
					break;

				case (int)PostType.GmInCharacter:
					if (post is TextPostViewModel)
					{
						html = GetHtmlForGmInCharacterPost((TextPostViewModel)post, includeEditControls, playerId, playerIsGm);	
					}
					break;

				case (int)PostType.DiceRoll:
					if (post is DiceRollPostViewModel)
					{
						html = GetHtmlForDiceRollPost((DiceRollPostViewModel)post);
					}
					break;
			}
			return html;
		}

		private string GetHtmlForInCharacterPost(TextPostViewModel post, bool includeEditControls, int playerId, bool playerIsGm)
		{
			StringBuilder html = new StringBuilder();

			html.Append("<div class=\"Post\"><div id=\"post");
			html.Append(post.ID);
			html.Append("\" class=\"InCharacterPost\"><div class=\"PostHeader\"><span class=\"PostNumber\">");
			//{PostNumber}
			html.Append("</span><span class=\"PostCharacter\">");
			html.Append(post.CharacterName);
			html.Append("</span>");
			//if (post.CharacterId > 0)
			//{
			//	html.Append("<span class=\"ViewCharacterIcon\" onclick=\"viewCharacter(");
			//	html.Append(post.CharacterId);
			//	html.Append(", '");
			//	html.Append(post.CharacterName);
			//	html.Append("');\"></span>");
			//}
			html.Append("<span class=\"PostPlayer\">");
			//html.Append(post.PlayerName);
			html.Append("</span><div class=\"Clear\"></div></div><div class=\"PostInfo\"><span class=\"CharacterPicture\">");
			html.Append("<a onkeypress=\"window.open(this.href); return false;\" onclick=\"window.open(this.href); return false;\" href=\"/Page/Index/");
			html.Append(post.CharacterId);
			html.Append("\"><img src=\"/Roleplay/Image/");
			html.Append(post.CharacterId);
			html.Append("\" /></a>");
			html.Append("</span><span class=\"PostedDate\">");
			html.Append(post.DatePosted);
			html.Append("</span><div class=\"Clear\"></div></div>");
			if (post.TargetPlayerNames.Length > 0)
			{
				html.Append("<div class=\"PrivateMessageRecipients\"><strong>Recipients:</strong>&nbsp;&nbsp;&nbsp;");
				html.Append(post.TargetPlayerNames);
				html.Append("</div>");
			}			
			html.Append("<div id=\"postContent");
			html.Append(post.ID);
			html.Append("\" class=\"PostContent\">");			
			html.Append(post.Content);
			html.Append("</div><div class=\"Clear\"></div>");
			if (includeEditControls && (playerId == post.PlayerId || playerIsGm))
			{
				html.Append("<div class=\"PostEditControls\">");

				if (playerIsGm)
				{
					html.Append("<input id=\"btnDeletePost");
					html.Append(post.ID);
					html.Append("\" type=\"button\" value=\"DELETE\" class=\"PostEditButton\" onclick=\"deletePost(");
					html.Append(post.ID);
					html.Append(");\" />");
				}

				html.Append("<input id=\"btnEditPost");
				html.Append(post.ID);
				html.Append("\" type=\"button\" value=\"EDIT\" class=\"PostEditButton\" onclick=\"editPost(");
				html.Append(post.ID);
				html.Append(");\" />");

				if (playerIsGm && post.IsRevised)
				{
					html.Append("<input id=\"btnRevertPost");
					html.Append(post.ID);
					html.Append("\" type=\"button\" value=\"REVERT\" class=\"PostEditButton\" onclick=\"revertPost(");
					html.Append(post.ID);
					html.Append(");\" />");
				}

				if (post.IsRevised && post.LastEdited != null)
				{
					html.Append("<span class=\"EditedDate\">Last Edited: ");
					html.Append(post.LastEdited);
					html.Append("</span>");
				}

				html.Append("</div>");
			}
			html.Append("</div>");
			html.Append("<div id=\"coverpost");
			html.Append(post.ID);
			html.Append("\" class=\"PostCover\"></div>");
			html.Append("</div>");

			return html.ToString();
		}

		private string GetHtmlForOutOfCharacterPost(TextPostViewModel post, bool includeEditControls, int playerId, bool playerIsGm)
		{
			StringBuilder html = new StringBuilder();

			html.Append("<div class=\"Post\"><div id=\"post");
			html.Append(post.ID);
			html.Append("\" class=\"OutOfCharacterPost\"><div class=\"PostHeader\"><span class=\"PostPlayer\">");
			html.Append(post.PlayerName);
			if (post.IsPostedByGm)
			{
				html.Append(" (GM)");
			}
			html.Append("</span><span class=\"PostCharacter\">OOC</span><div class=\"Clear\"></div></div>");
			if (post.TargetPlayerNames.Length > 0)
			{
				html.Append("<div class=\"PrivateMessageRecipients\"><strong>Recipients:</strong>&nbsp;&nbsp;&nbsp;");
				html.Append(post.TargetPlayerNames);
				html.Append("</div>");
			}
			html.Append("<div id=\"postContent");
			html.Append(post.ID);
			html.Append("\" class=\"PostContent\">");
			html.Append(post.Content);
			html.Append("</div><span class=\"PostedDate\">");
			html.Append(post.DatePosted);
			html.Append("</span><div class=\"Clear\"></div>");
			if (includeEditControls && (playerId == post.PlayerId || playerIsGm))
			{
				html.Append("<div class=\"PostEditControls\">");

				if (playerIsGm)
				{
					html.Append("<input id=\"btnDeletePost");
					html.Append(post.ID);
					html.Append("\" type=\"button\" value=\"DELETE\" class=\"PostEditButton\" onclick=\"deletePost(");
					html.Append(post.ID);
					html.Append(");\" />");
				}

				html.Append("<input id=\"btnEditPost");
				html.Append(post.ID);
				html.Append("\" type=\"button\" value=\"EDIT\" class=\"PostEditButton\" onclick=\"editPost(");
				html.Append(post.ID);
				html.Append(");\" />");

				if (playerIsGm && post.IsRevised)
				{
					html.Append("<input id=\"btnRevertPost");
					html.Append(post.ID);
					html.Append("\" type=\"button\" value=\"REVERT\" class=\"PostEditButton\" onclick=\"revertPost(");
					html.Append(post.ID);
					html.Append(");\" />");
				}

				if (post.IsRevised && post.LastEdited != null)
				{
					html.Append("<span class=\"EditedDate\">Last Edited: ");
					html.Append(post.LastEdited);
					html.Append("</span>");
				}

				html.Append("</div>");
			}
			html.Append("</div>");
			html.Append("<div id=\"coverpost");
			html.Append(post.ID);
			html.Append("\" class=\"PostCover\"></div>");
			html.Append("</div>");

			return html.ToString();
		}

		private string GetHtmlForGmInCharacterPost(TextPostViewModel post, bool includeEditControls, int playerId, bool playerIsGm)
		{
			StringBuilder html = new StringBuilder();

			html.Append("<div class=\"Post\"><div id=\"post");
			html.Append(post.ID);
			html.Append("\" class=\"GmInCharacterPost\"><div class=\"PostHeader\"><span class=\"PostNumber\">");
			//{PostNumber}
			html.Append("</span><span class=\"PostCharacter\">");
			html.Append(post.CharacterName);
			html.Append("</span>");
			//if (post.CharacterId > 0)
			//{
			//	html.Append("<span class=\"ViewCharacterIcon\" onclick=\"viewCharacter(");
			//	html.Append(post.CharacterId);
			//	html.Append(", '");
			//	html.Append(post.CharacterName);
			//	html.Append("');\"></span>");
			//}		
			html.Append("<span class=\"PostPlayer\">");
			//html.Append(post.PlayerName);
			html.Append("</span><div class=\"Clear\"></div></div><div class=\"PostInfo\"><span class=\"CharacterPicture\">");
			if (post.CharacterName == "GM")
			{
				html.Append("<img src=\"/content/images/roleplayforum/gm.jpg\" />");
			}
			else
			{
				html.Append("<a onkeypress=\"window.open(this.href); return false;\" onclick=\"window.open(this.href); return false;\" href=\"/Page/Index/");
				html.Append(post.CharacterId);
				html.Append("\"><img src=\"/Roleplay/Image/");
				html.Append(post.CharacterId);
				html.Append("\" /></a>");			
			}
			html.Append("</span><span class=\"PostedDate\">");
			html.Append(post.DatePosted);
			html.Append("</span><div class=\"Clear\"></div></div>");
			if (post.TargetPlayerNames.Length > 0)
			{
				html.Append("<div class=\"PrivateMessageRecipients\"><strong>Recipients:</strong>&nbsp;&nbsp;&nbsp;");
				html.Append(post.TargetPlayerNames);
				html.Append("</div>");
			}
			html.Append("<div id=\"postContent");
			html.Append(post.ID);
			html.Append("\" class=\"PostContent\">");
			html.Append(post.Content);
			html.Append("</div><div class=\"Clear\"></div>");
			if (includeEditControls && (playerId == post.PlayerId || playerIsGm))
			{
				html.Append("<div class=\"PostEditControls\">");

				if (playerIsGm)
				{
					html.Append("<input id=\"btnDeletePost");
					html.Append(post.ID);
					html.Append("\" type=\"button\" value=\"DELETE\" class=\"PostEditButton\" onclick=\"deletePost(");
					html.Append(post.ID);
					html.Append(");\" />");
				}

				html.Append("<input id=\"btnEditPost");
				html.Append(post.ID);
				html.Append("\" type=\"button\" value=\"EDIT\" class=\"PostEditButton\" onclick=\"editPost(");
				html.Append(post.ID);
				html.Append(");\" />");

				if (playerIsGm && post.IsRevised)
				{
					html.Append("<input id=\"btnRevertPost");
					html.Append(post.ID);
					html.Append("\" type=\"button\" value=\"REVERT\" class=\"PostEditButton\" onclick=\"revertPost(");
					html.Append(post.ID);
					html.Append(");\" />");
				}

				if (post.IsRevised && post.LastEdited != null)
				{
					html.Append("<span class=\"EditedDate\">Last Edited: ");
					html.Append(post.LastEdited);
					html.Append("</span>");
				}

				html.Append("</div>");
			}
			html.Append("</div>");
			html.Append("<div id=\"coverpost");
			html.Append(post.ID);
			html.Append("\" class=\"PostCover\"></div>");
			html.Append("</div>");

			return html.ToString();
		}

		private string GetHtmlForDiceRollPost(DiceRollPostViewModel post)
		{
			StringBuilder html = new StringBuilder();

			html.Append("<div class=\"Post\"><div id=\"post");
			html.Append(post.ID);
			html.Append("\" class=\"DiceRollPost\"><div class=\"PostHeader\"><span class=\"PostPlayer\">");
			html.Append(post.PlayerName);
			if (post.IsPostedByGm)
			{
				html.Append(" (GM)");
			}
			html.Append("</span><span class=\"PostCharacter\">");
			html.Append(post.CharacterName);
			html.Append("</span><div class=\"Clear\"></div></div>"); 
			html.Append("<div class=\"PostContent\"><span class=\"RollDescription\">");
			html.Append(post.DieCount);
			html.Append("D");
			html.Append(post.DieSize);
			html.Append(" = ");
			bool previousRollWasMax = false;
			for (int i = 0; i < post.RollValues.Count; i++)
			{
				if (i > 0)
				{
					html.Append(", ");
				}
				if (previousRollWasMax && post.ReRollMaximums)
				{
					html.Append("<span class=\"BonusDie\">");
				}
				if (post.RollValues[i] == post.DieSize)
				{
					html.Append("<strong>");
				}
				html.Append(post.RollValues[i]);
				if (post.RollValues[i] == post.DieSize)
				{
					html.Append("</strong>");
				}
				if (previousRollWasMax && post.ReRollMaximums)
				{
					html.Append("</span>");
				}

				previousRollWasMax = (post.RollValues[i] == post.DieSize);
			}
			html.Append("</span><span class=\"RollResult\">");
			if (post.RollType == (int)RollType.DicePool)
			{
				html.Append("Difficulty: ");
				html.Append(post.RollTarget);
				html.Append("<br />");
				html.Append("Successes: ");
				int successes = 0;
				foreach (int roll in post.RollValues)
				{
					if (roll >= post.RollTarget)
					{
						successes++;
					}
				}
				html.Append(successes);
			}
			else
			{
				int total = 0;
				foreach (int roll in post.RollValues)
				{
					total += roll;
				}
				html.Append("Total: ");
				html.Append(total);
			}
			html.Append("</span></div><div class=\"Clear\"></div></div>");
			html.Append("<div id=\"coverpost");
			html.Append(post.ID);
			html.Append("\" class=\"PostCover\"></div>");
			html.Append("</div>");

			return html.ToString();
		}
	}
}
