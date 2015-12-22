using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
	public class LogGenerator : ILogGenerator
	{
		private readonly IRepository _repository;
		private IRepository Repo
		{
			get { return _repository; }
		}

		public LogGenerator(IRepository repository)
		{
			_repository = repository;
		}


		public string GenerateTextLog(int sessionId, bool includeOoc)
		{
			List<Post> posts = (from p in Repo.Posts()
				where
					p.SessionId == sessionId && !p.IsDeleted &&
					(p.PostType == (int) PostType.InCharacter || p.PostType == (int) PostType.GmInCharacter || includeOoc)
				orderby p.DatePosted
				select p).ToList();

			StringBuilder output = new StringBuilder();
			foreach (Post post in posts)
			{
				if (post.PostType == (int) PostType.DiceRoll)
				{
					output.Append("DICE ROLL - ");
					Person character = Repo.People().FirstOrDefault(p => p.Id == post.CharacterId);
					if (character != null)
					{
						output.Append(character.ShortName);
						output.AppendLine(":");
					}
					else
					{
						output.AppendLine("GM:");
					}
					output.Append(post.DieCount);
					output.Append("D");
					output.Append(post.DieSize);
					output.Append(" = ");
					output.AppendLine(post.RollValues.Replace(",", ", "));					
				}
				else
				{
					if (post.PostType == (int) PostType.OutOfCharacter)
					{
						output.Append("OOC - ");
						output.Append(post.Player.DisplayName);
						output.AppendLine(":");
					}
					else
					{
						Person character = Repo.People().FirstOrDefault(p => p.Id == post.CharacterId);
						if (character != null)
						{
							output.Append(character.ShortName);
							output.AppendLine(":");
						}
						else
						{
							output.AppendLine("GM:");
						}
					}
					if (post.IsRevised)
					{
						output.AppendLine(post.RevisedContent.Replace("{CR}", "\r\n"));
					}
					else
					{
						output.AppendLine(post.OriginalContent.Replace("{CR}", "\r\n"));
					}
				}

				output.AppendLine();
			}

			return output.ToString().Replace("[b]", string.Empty).Replace("[/b]", string.Empty).Replace("[i]", string.Empty).Replace("[/i]", string.Empty);
		}

		public string GenerateHtmlLog(int sessionId, bool includeOoc)
		{
			List<Post> posts = (from p in Repo.Posts()
								where
									p.SessionId == sessionId && !p.IsDeleted &&
									(p.PostType == (int)PostType.InCharacter || p.PostType == (int)PostType.GmInCharacter || includeOoc)
								orderby p.DatePosted
								select p).ToList();

			StringBuilder output = new StringBuilder();
			foreach (Post post in posts)
			{
				if (post.PostType == (int)PostType.DiceRoll)
				{
					output.Append("<div class=\"DiceRoll\">");
					output.Append("<span class=\"Title\">DICE ROLL - ");
					Person character = Repo.People().FirstOrDefault(p => p.Id == post.CharacterId);
					if (character != null)
					{
						output.Append(character.ShortName);
						output.AppendLine("</span>");
					}
					else
					{
						output.AppendLine("GM</span>");
					}
					output.Append("<span class=\"Content\">");
					output.Append(post.DieCount);
					output.Append("D");
					output.Append(post.DieSize);
					output.Append(" = ");
					output.AppendLine(post.RollValues.Replace(",", ", "));
					output.AppendLine("</span></div>");
				}
				else
				{
					if (post.PostType == (int)PostType.OutOfCharacter)
					{
						output.Append("<div class=\"OocComment\">");
						output.Append("<span class=\"Title\">OOC - ");
						output.Append(post.Player.DisplayName);
						output.AppendLine("</span>");
						output.Append("<span class=\"Content\">");
					}
					else
					{
						Person character = Repo.People().FirstOrDefault(p => p.Id == post.CharacterId);
						if (character != null)
						{
							output.Append("<h4>");
							output.Append(character.ShortName);
							output.Append("</h4>");
						}
						else
						{
							output.AppendLine("<h4>GM</h4>");
						}
					}
					output.Append("<p>");
					if (post.IsRevised)
					{
						output.AppendLine(ApplyPostFormatting(post.RevisedContent.Replace("{CR}", "</p><p>")));
					}
					else
					{
						output.AppendLine(ApplyPostFormatting(post.OriginalContent.Replace("{CR}", "</p><p>")));
					}
					output.Append("</p>");
					if (post.PostType == (int) PostType.OutOfCharacter)
					{
						output.AppendLine("</span></div>");
					}
				}

				output.AppendLine();
			}

			return output.ToString();
		}

		private string ApplyPostFormatting(string postContent)
		{
			Regex bold = new Regex(@"\[b\](.*)\[/b\]");
			Regex italic = new Regex(@"\[i\](.*)\[/i\]");

			MatchCollection boldMatches = bold.Matches(postContent);
			foreach (Match match in boldMatches)
			{
				if (match.Groups.Count > 1)
				{
					string content = match.Groups[1].Value.Replace("[b]", string.Empty).Replace("[/b]", string.Empty);
					postContent = postContent.Replace(match.Value, string.Format("<strong>{0}</strong>", content));
				}
			}

			MatchCollection italicMatches = italic.Matches(postContent);
			foreach (Match match in italicMatches)
			{
				if (match.Groups.Count > 1)
				{
					string content = match.Groups[1].Value.Replace("[i]", string.Empty).Replace("[/i]", string.Empty);
					postContent = postContent.Replace(match.Value, string.Format("<em>{0}</em>", content));
				}
			}

			postContent = postContent.Replace("[b]", string.Empty).Replace("[/b]", string.Empty).Replace("[i]", string.Empty).Replace("[/i]", string.Empty);

			return postContent;
		}
	}
}
