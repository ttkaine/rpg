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

        private Player _gm;
        private CampaignDetail _campaign;

        public CampaignDetail Campaign
        {
            get
            {
                if (_campaign == null)
                {
                    _campaign = Repo.CampaignDetails().Single();
                }
                return _campaign;
            }
        }

        public Player Gm
        {
            get
            {
                if (_gm == null)
                {

                    if (Campaign != null)
                    {
                        _gm = Repo.Players().Single(p => p.Id == Campaign.GmId);
                    }
                }
                return _gm;
            }
        }

	    private int GetGmId()
	    {
	        return Gm?.Id ?? 0;
	    }

        public string GenerateTextLog(int sessionId, bool includeOoc, IPostFormatter postFormatter)
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
                    if (post.RollType == (int)RollType.FUDGE)
                    {
                        output.Append("FATE ROLL = ");
                    }
                    else
                    {
                        output.Append(post.DieCount);
                        output.Append("D");
                        output.Append(post.DieSize);
                        output.Append(" = ");
                    }
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
						    if (post.PlayerId == Gm?.Id && Gm != null)
						    {
						        output.AppendLine("GM:");
						    }
						    else
						    {
						        if (post.PostType == (int) PostType.Image)
						        {
						            output.Append(post.Player.DisplayName);
						            output.AppendLine(":");
						        }
						        else
						        {
						            output.AppendLine("Environment:");
						        }
						    }
						}
					}
                    if (post.IsRevised && !string.IsNullOrWhiteSpace(post.RevisedContent))
                    {
                        output.AppendLine(postFormatter.RemoveHtmlAndMarkdown(post.RevisedContent).Replace("{CR}", "\r\n"));
                    }
                    else
                    {
                        output.AppendLine(postFormatter.RemoveHtmlAndMarkdown(post.OriginalContent).Replace("{CR}", "\r\n"));
                    }
                }

				output.AppendLine();
			}

			return output.ToString().Replace("[b]", string.Empty).Replace("[/b]", string.Empty).Replace("[i]", string.Empty).Replace("[/i]", string.Empty);
		}

		public string GenerateHtmlLog(int sessionId, bool includeOoc, IPostFormatter postFormatter)
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
                else if (post.PostType == (int) PostType.Image)
                {
                    if (post.PlayerId == Gm?.Id && Gm != null)
                    {
                        output.AppendLine("<h4>GM</h4>");
                    }
                    else
                    {
                        output.Append("<h4>");
                        output.Append(post.Player.DisplayName);
                        output.AppendLine("</h4>");
                    }

                    output.Append("<div class=\"LogImage\">");
                    output.Append("<img src=\"/Roleplay/ImagePost/");
                    output.Append(post.ImageId ?? 0);
                    output.Append("\" alt=\"");
                    output.Append(post.OriginalContent);
                    output.Append("\" />");
                    output.Append("</div>");
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
						    if (post.PlayerId == Gm?.Id && Gm != null)
						    {
						        output.AppendLine("<h4>GM</h4>");
						    }
						    else
						    {
						        output.AppendLine("<h4>Environment</h4>");
						    }
						}
					}
					output.Append("<p>");
					if (post.IsRevised && !string.IsNullOrWhiteSpace(post.RevisedContent))
					{
						output.AppendLine(postFormatter.ApplyPostFormatting(post.RevisedContent.Replace("{CR}", "</p><p>"), true));
					}
					else
					{
						output.AppendLine(postFormatter.ApplyPostFormatting(post.OriginalContent.Replace("{CR}", "</p><p>"), true));
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


    }
}
