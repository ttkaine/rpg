using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using Warhammer.Core;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.RoleplayViewModels;
using Warhammer.Mvc.HtmlBuilders;
using Warhammer.Mvc.JsonObjects;
using Warhammer.Mvc.Models;
using Page = Warhammer.Core.Entities.Page;

namespace Warhammer.Mvc.Controllers
{
	//[Authorize]
    public class RoleplayController : BaseController
    {
		public new IModelFactory ModelFactory { get; set; }
		public IPostManager PostManager { get; set; }
		public ILogGenerator LogGenerator { get; set; }
        public ICurrentCampaignProvider CampaignProvider { get; set; }
		//public ICharacterManager CharacterManager { get; set; }

        public RoleplayController(IAuthenticatedDataProvider data, IModelFactory modelFactory, IPostManager postManager, ILogGenerator logGenerator, ICurrentCampaignProvider campaignProvider) : base(data)
        {
	        ModelFactory = modelFactory;
	        PostManager = postManager;
	        LogGenerator = logGenerator;
            CampaignProvider = campaignProvider;
            //CharacterManager = new CharacterManager(new DataAccess());
        }   
		
		//
        // GET: /Roleplay/
		public ActionResult Index(int? id)
		{
			if (id.HasValue)
			{
				Page page = DataProvider.GetPage(id.Value);
				if (page != null)
				{
					Session session = page as Session;
					if (session != null && session.IsTextSession)
					{
						if (!CurrentPlayerIsGm && session.IsPrivate && session.PlayerCharacters.All(c => c.PlayerId != CurrentPlayer.Id))
						{
							return RedirectToAction("Index", "Page", new {id = id});
						}
						else
						{
							if (session.IsClosed)
							{
								return RedirectToAction("HtmlLog", "Roleplay", new {id = id});
							}
							else
							{
								DataProvider.EnsurePostOrders(session.Id);
								ViewBag.SessionId = session.Id;
								ViewBag.Title = session.FullName;

								return View();
							}
						}
					}
				}
			}

			return RedirectToAction("Index", "Home");
		}




		public JsonResult GetLatestPostsForSession(int sessionId, int lastPostId, string lastUpdateTime)
		{
			DateTime lastUpdate;
			if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
			{
				lastUpdate = DateTime.MinValue;
			}

			if (DataProvider.IsLoggedIn())
			{
				JsonResponseWithPostCollection postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
			else
			{
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}

		}

		protected JsonResponseWithPostCollection GetRecentPostsForSession(int sessionId, int lastPostId, DateTime lastUpdateTime)
		{
			JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
			bool playerIsGm = false;
			int playerId = -1;
			List<PostViewModel> posts = ModelFactory.GetPostsForCurrentUserInSessionSinceLast(sessionId, lastPostId, out playerId, out playerIsGm);
			posts = (from p in posts
					 orderby p.ID ascending
					 select p).ToList();

			Player player = DataProvider.PlayerToPostInSession(sessionId);
			string name = "Nobody's";
			if (player != null)
			{
				name = player.Id == CurrentPlayer.Id ? "YOUR" : player.DisplayName + "'s";
			}

			postCollection.PlayerTurnMessage = string.Format("It is currently {0} turn", name);
			postCollection.IsCurrentPlayerTurn = (player != null && player.Id == CurrentPlayer.Id);

			PostBuilder postBuilder = new PostBuilder();
			if (posts.Count > 0)
			{
				foreach (PostViewModel post in posts)
				{
					JsonPost jsonPost = new JsonPost();
					jsonPost.ID = "post" + post.ID;
					jsonPost.Content = (postBuilder.GetHtmlForPost(post, true, playerId, playerIsGm));
					if (jsonPost.Content.Trim().Length > 0)
					{
						postCollection.Posts.Add(jsonPost);
					}
				}

				postCollection.LatestPostId = posts[posts.Count - 1].ID;
				postCollection.Count = postCollection.Posts.Count;
			}
			else
			{
				postCollection.LatestPostId = lastPostId;
			}

			if (lastPostId > 0)
			{
				List<PostViewModel> editedPosts = ModelFactory.GetEditedPostsForCurrentUserInSessionSinceLast(sessionId, lastUpdateTime);
				if (editedPosts.Count > 0)
				{
					foreach (PostViewModel post in editedPosts)
					{
						JsonPost jsonPost = new JsonPost();
						jsonPost.ID = "post" + post.ID;
						jsonPost.Content = (postBuilder.GetHtmlForPost(post, true, playerId, playerIsGm));
						if (jsonPost.Content.Trim().Length > 0)
						{
							postCollection.EditedPosts.Add(jsonPost);
						}
					}

					postCollection.EditedCount = postCollection.EditedPosts.Count;
				}

				postCollection.DeletedPosts = ModelFactory.GetDeletedPostIdsForCurrentUserInSessionSinceLast(sessionId, lastUpdateTime);
				postCollection.DeletedCount = postCollection.DeletedPosts.Count;
			}

            DataProvider.MarkAsSeen(sessionId);

			return postCollection;
		}

		public JsonResult GetCharacterList(int sessionId)
		{			
			if (DataProvider.IsLoggedIn())
			{
				List<CharacterViewModel> characters = ModelFactory.GetCharactersForCurrentUserInSession(sessionId);
				List<JsonCharacterListItem> characterListItems = new List<JsonCharacterListItem>();
				foreach (CharacterViewModel character in characters)
				{
					JsonCharacterListItem characterListItem = new JsonCharacterListItem()
					{
						ID = character.ID,
						Name = character.Name
					};
					characterListItems.Add(characterListItem);
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json(serializer.Serialize(characterListItems));
			}
			else
			{
				throw new Exception("Session timeout");
			}
		}

        [Authorize(Roles = "Player")]
        public JsonResult MakeTextPost(int sessionId, int characterId, int lastPostId, bool isOoc, string text, string lastUpdateTime, string recipientString)
		{
			DateTime lastUpdate;
			if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
			{
				lastUpdate = DateTime.MinValue;
			}
			if (DataProvider.IsLoggedIn())
			{
				PostResult result = PostResult.Success;
				if (text.Trim().Length > 0)
				{
					text = StripTagsFromString(text);
					text = text.Replace("\n", "{CR}");
					text = text.Replace("&quote;", "\"");

					result = PostManager.CreateTextPostForUser(sessionId, characterId, isOoc, text, recipientString);
				}
				else
				{
					Player playerToPost = DataProvider.PlayerToPostInSession(sessionId);
					if (playerToPost != null && playerToPost.Id == CurrentPlayer.Id)
					{
						PostManager.SetTurnOverForUser(sessionId);
					}
				}
                //CallSignalRUpdate();

				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);

				if (result == PostResult.Success)
				{
					postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);
				}
				else
				{
					postCollection.IsError = true;
					switch (result)
					{
						case PostResult.CharacterNotInCampaign:
							postCollection.ErrorMessage = "Character not assigned to this campaign.";
							break;

						case PostResult.InvalidCharacter:
							postCollection.ErrorMessage = "Invalid character";
							break;

						case PostResult.InvalidPlayer:
							postCollection.ErrorMessage = "Invalid player";
							break;

						case PostResult.InvalidSession:
							postCollection.ErrorMessage = "Invalid session";
							break;

						case PostResult.PlayerNotInCampaign:
							postCollection.ErrorMessage = "You are not part of this campaign.";
							break;

						case PostResult.SessionClosed:
							postCollection.ErrorMessage = "Posting not allowed.  The GM has closed the session.";
							break;

						default:
							postCollection.ErrorMessage = "Unknown Error";
							break;
					}
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");	
			}
			else
			{
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}

		}

        [Authorize(Roles = "Player")]
        public JsonResult MakeImagePost(int sessionId, int lastPostId, string lastUpdateTime, string recipientString)
        {
            DateTime lastUpdate;
            if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
            {
                lastUpdate = DateTime.MinValue;
            }
            if (DataProvider.IsLoggedIn() && Request.Files["imageUpload"] != null)
            {
                HttpPostedFileBase file = Request.Files["imageUpload"];
                string fileName = file.FileName;

                PostResult result = PostResult.Success;

                //CallSignalRUpdate();
                JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);

                if (result == PostResult.Success)
                {
                    postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);
                }
                else
                {
                    postCollection.IsError = true;
                    switch (result)
                    {
                        case PostResult.CharacterNotInCampaign:
                            postCollection.ErrorMessage = "Character not assigned to this campaign.";
                            break;

                        case PostResult.InvalidPlayer:
                            postCollection.ErrorMessage = "Invalid player";
                            break;

                        case PostResult.InvalidSession:
                            postCollection.ErrorMessage = "Invalid session";
                            break;

                        case PostResult.PlayerNotInCampaign:
                            postCollection.ErrorMessage = "You are not part of this campaign.";
                            break;

                        case PostResult.SessionClosed:
                            postCollection.ErrorMessage = "Posting not allowed.  The GM has closed the session.";
                            break;

                        case PostResult.ImageError:
                            postCollection.ErrorMessage = "Image Error";
                            break;

                        default:
                            postCollection.ErrorMessage = "Unknown Error";
                            break;
                    }
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return Json("[" + serializer.Serialize(postCollection) + "]");
            }
            else
            {
                JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
                postCollection.IsError = true;
                postCollection.ErrorMessage = "Session timeout";
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return Json("[" + serializer.Serialize(postCollection) + "]");
            }
        }


        private string StripTagsFromString(string source)
		{
			return Regex.Replace(source, "<.*?>", string.Empty);
		}

        [Authorize(Roles = "Player")]
        public JsonResult MakeDiceRollPost(int sessionId, int characterId, int lastPostId, int dieSize, int dieCount, int rollType, int rollTarget, bool reRollMaximum, string lastUpdateTime)
		{
			DateTime lastUpdate;
			if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
			{
				lastUpdate = DateTime.MinValue;
			}
			if (DataProvider.IsLoggedIn())
			{

				PostResult result = PostManager.CreateDiceRollPostForUser(sessionId, characterId, dieSize, dieCount, rollType, rollTarget, reRollMaximum, true);

                //CallSignalRUpdate();
                JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);

				if (result == PostResult.Success)
				{
					postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);
				}
				else
				{
					postCollection.IsError = true;
					switch (result)
					{
						case PostResult.CharacterNotInCampaign:
							postCollection.ErrorMessage = "Character not assigned to this campaign.";
							break;

						case PostResult.InvalidCharacter:
							postCollection.ErrorMessage = "Invalid character";
							break;

						case PostResult.InvalidPlayer:
							postCollection.ErrorMessage = "Invalid player";
							break;

						case PostResult.InvalidSession:
							postCollection.ErrorMessage = "Invalid session";
							break;

						case PostResult.InvalidDiceRoll:
							postCollection.ErrorMessage = "Invalid dice roll";
							break;

						case PostResult.PlayerNotInCampaign:
							postCollection.ErrorMessage = "You are not part of this campaign.";
							break;

						case PostResult.SessionClosed:
							postCollection.ErrorMessage = "Posting not allowed.  The GM has closed the session.";
							break;

						default:
							postCollection.ErrorMessage = "Unknown Error";
							break;
					}
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
			else
			{
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}

		}

		public  JsonResult GetCharacterDetails(int sessionId, int characterId)
		{
			CharacterViewModel character = ModelFactory.GetCharacter(characterId);
			JsonCharacterDetails jsonCharacter = new JsonCharacterDetails();
			JavaScriptSerializer serializer = new JavaScriptSerializer();

			if (character != null)
			{
				jsonCharacter.ID = character.ID;
				jsonCharacter.Name = character.Name;
				if (character.Description.Trim().Length > 0)
				{
					jsonCharacter.Description = character.Description;
				}

				if (!string.IsNullOrWhiteSpace(character.CharacterSheet))
				{
					if (DataProvider.IsLoggedIn())
					{
						PlayerViewModel player = ModelFactory.GetPlayerForCurrentUser(sessionId);
						SessionViewModel session = ModelFactory.GetSession(sessionId);

						if (player != null && session != null)
						{
							try
							{
								JsonCharacterSheet characterSheet = (JsonCharacterSheet) serializer.Deserialize(character.CharacterSheet, typeof (JsonCharacterSheet));
								characterSheet = JsonCharacterSheetFactory.RefreshCharacterSheetFromSource(characterSheet);
								if (!characterSheet.IsPrivate || (characterSheet.IsPrivate && (player.ID == character.PlayerId || player.IsGM)))
								{
									CharacterSheetBuilder sheetBuilder = CharacterSheetBuilder.Instance;
									jsonCharacter.CharacterSheet = sheetBuilder.GetDisplayCharacterSheet(characterSheet);
								}
								else
								{
									jsonCharacter.CharacterSheet = "<p>Unable to view this character sheet, as the player has set it to private.</p>";
								}
							}
							catch
							{
							}
						}
					}
				}
			}

			return Json("[" + serializer.Serialize(jsonCharacter) + "]");
		}

		public JsonResult GetSessionTitle(int sessionId)
		{
			SessionViewModel session = ModelFactory.GetSession(sessionId);

			string title = "Session Title";
			if (session != null)
			{
				title = session.Title;
			}

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return Json(serializer.Serialize(title));
		}

	    public JsonResult GetCurrentPlayerToPost(int sessionId)
	    {
		    Player player = DataProvider.PlayerToPostInSession(sessionId);
		    string name = "Nobody's";
		    if (player != null)
		    {
			    name = player.Id == CurrentPlayer.Id ? "YOUR" : player.DisplayName + "'s";
		    }
		    
		    var returnObject = new
		    {
				Message = string.Format("It is currently {0} turn", name),
				IsCurrentPlayer = (player != null && player.Id == CurrentPlayer.Id)
		    };

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return Json("[" + serializer.Serialize(returnObject) + "]");		    
	    }

        [Authorize(Roles = "Player")]
        public JsonResult DeletePost(int sessionId, int postId, int lastPostId, string lastUpdateTime)
		{
			DateTime lastUpdate;
			if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
			{
				lastUpdate = DateTime.MinValue;
			}

			if (DataProvider.IsLoggedIn())
			{
				bool deleted = PostManager.DeletePostForUser(postId);
                //CallSignalRUpdate();
                JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				if (!deleted)
				{
					postCollection.IsError = true;
					postCollection.ErrorMessage = "Unable to delete post.";
				}
				else
				{
					postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);					
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
			else
			{
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
		}

        [Authorize(Roles = "Player")]
        public JsonResult MakePostOoc(int sessionId, int postId, int lastPostId, string lastUpdateTime)
		{
			DateTime lastUpdate;
			if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
			{
				lastUpdate = DateTime.MinValue;
			}

			if (DataProvider.IsLoggedIn())
			{
				bool markedOoc = PostManager.MarkPostOoc(postId);
                //CallSignalRUpdate();
                JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				if (!markedOoc)
				{
					postCollection.IsError = true;
					postCollection.ErrorMessage = "Unable to set post OOC.";
				}
				else
				{
					postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
			else
			{
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
		}

        [Authorize(Roles = "Player")]
        public JsonResult EditTextPost(int sessionId, int postId, int lastPostId, string text, string lastUpdateTime)
		{
			DateTime lastUpdate;
			if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
			{
				lastUpdate = DateTime.MinValue;
			}

			if (DataProvider.IsLoggedIn())
			{
				text = StripTagsFromString(text);
				text = text.Replace("\n", "{CR}");
				text = text.Replace("&quote;", "\"");

				PostResult result = PostManager.EditTextPostForUser(postId, text);
                //CallSignalRUpdate();
                JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);

				if (result == PostResult.Success)
				{
					postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);
				}
				else
				{
					postCollection.IsError = true;
					switch (result)
					{
						case PostResult.CharacterNotInCampaign:
							postCollection.ErrorMessage = "Character not assigned to this campaign.";
							break;

						case PostResult.InvalidCharacter:
							postCollection.ErrorMessage = "Invalid character";
							break;

						case PostResult.InvalidPlayer:
							postCollection.ErrorMessage = "Invalid player";
							break;

						case PostResult.InvalidSession:
							postCollection.ErrorMessage = "Invalid session";
							break;

						case PostResult.PlayerNotInCampaign:
							postCollection.ErrorMessage = "You are not part of this campaign.";
							break;

						case PostResult.SessionClosed:
							postCollection.ErrorMessage = "Posting not allowed.  The GM has closed the session.";
							break;

						case PostResult.InvalidPost:
							postCollection.ErrorMessage = "Invalid post";
							break;

						default:
							postCollection.ErrorMessage = "Unknown Error";
							break;
					}
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
			else
			{
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
		}

        [Authorize(Roles = "Player")]
        public JsonResult RevertPost(int sessionId, int postId, int lastPostId, string lastUpdateTime)
		{
			DateTime lastUpdate;
			if (!DateTime.TryParse(lastUpdateTime, out lastUpdate))
			{
				lastUpdate = DateTime.MinValue;
			}

			if (DataProvider.IsLoggedIn())
			{
				bool reverted = PostManager.RevertPostForUser(postId);
                //CallSignalRUpdate();
                JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				if (!reverted)
				{
					postCollection.IsError = true;
					postCollection.ErrorMessage = "Unable to revert post.";
				}
				else
				{
					postCollection = GetRecentPostsForSession(sessionId, lastPostId, lastUpdate);
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
			else
			{
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection(CampaignProvider.CurrentCampaignId);
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
		}

		public JsonResult GetRecipientList(int sessionId, string recipientString)
		{
			if (DataProvider.IsLoggedIn())
			{
				int gmId;
				List<PlayerViewModel> players = ModelFactory.GetPlayersForSessionExcludingUser(sessionId, out gmId);

				List<int> checkedIds = new List<int>();
				string[] checkedIdItems = recipientString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string checkedIdItem in checkedIdItems)
				{
					int checkedId;
					if (int.TryParse(checkedIdItem.Trim(), out checkedId))
					{
						checkedIds.Add(checkedId);
					}
				}

				List<JsonRecipient> recipients = new List<JsonRecipient>();
				foreach (PlayerViewModel player in players)
				{
					bool isChecked = checkedIds.Contains(player.ID);
					if (player.ID == gmId)
					{
						player.Name += " (GM)";
					}
					recipients.Add(new JsonRecipient(player.ID, player.Name, isChecked));
				}

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json(serializer.Serialize(recipients));
			}
			else
			{
				throw new Exception("Session timeout");
			}
		}

        //public JsonResult GetEditableCharacterSheet(int characterId)
        //{
        //	if (DataProvider.IsLoggedIn())
        //	{
        //		CharacterViewModel character = ModelFactory.GetCharacterForCurrentUser(characterId);
        //		JavaScriptSerializer serializer = new JavaScriptSerializer();

        //		if (character != null)
        //		{
        //			JsonCharacterSheet characterSheet; 
        //			if (!string.IsNullOrWhiteSpace(character.CharacterSheet))
        //			{
        //				try
        //				{
        //					characterSheet = (JsonCharacterSheet)serializer.Deserialize(character.CharacterSheet, typeof(JsonCharacterSheet));
        //					characterSheet = JsonCharacterSheetFactory.RefreshCharacterSheetFromSource(characterSheet);
        //				}
        //				catch
        //				{
        //					characterSheet = JsonCharacterSheetFactory.GetCharacterSheet(CharacterSheetStyle.So82013Fantasy);
        //				}
        //			}
        //			else
        //			{
        //				characterSheet = JsonCharacterSheetFactory.GetCharacterSheet(CharacterSheetStyle.So82013Fantasy);
        //			}

        //			CharacterSheetBuilder sheetBuilder = CharacterSheetBuilder.Instance;
        //			string characterSheetHtml = sheetBuilder.GetCharacterSheetEditorHtml(characterSheet);
        //			return Json(serializer.Serialize(characterSheetHtml));
        //		}
        //		else
        //		{
        //			return Json(serializer.Serialize("<p>You are not allowed to edit this character.</p>"));					
        //		}
        //	}
        //	else
        //	{
        //		throw new Exception("Session timeout");
        //	}
        //}

        //public JsonResult SubmitCharacterSheet(int characterId, string jsonSheet)
        //{
        //	if (DataProvider.IsLoggedIn())
        //	{
        //		CharacterViewModel character = ModelFactory.GetCharacterForCurrentUser(characterId);
        //		JavaScriptSerializer serializer = new JavaScriptSerializer();

        //		if (character != null)
        //		{
        //			character.CharacterSheet = jsonSheet;
        //			CharacterManager.SaveCharacter(character);

        //			return Json(serializer.Serialize("Character sheet updated."));
        //		}
        //		else
        //		{
        //			return Json(serializer.Serialize("Unable to update character sheet for this character."));
        //		}
        //	}
        //	else
        //	{
        //		throw new Exception("Session timeout");
        //	}
        //}

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        public ActionResult Image(int id)
		{
			CharacterViewModel character = ModelFactory.GetCharacter(id);
			var defaultDir = Server.MapPath("/Content/Images/RoleplayForum");

		    if (character?.Image != null && character.Image.Length > 100)
		    {
		        return File(character.Image, "image/jpeg");
		    }

		    var defaultImagePath = Path.Combine(defaultDir, "default_character.jpg");
			return File(defaultImagePath, "image/jpeg");
		}

	    public FileContentResult TextLog(int id)
	    {
		    string textLog = LogGenerator.GenerateTextLog(id, false);

			return File(Encoding.UTF8.GetBytes(textLog), "text/plain", string.Format("{0}.txt", "session_log"));
	    }

	    public ActionResult HtmlLog(int id)
	    {
			Page page = DataProvider.GetPage(id);
		    if (page != null)
		    {
			    DataProvider.MarkAsSeen(page.Id);

			    if (page is Session)
			    {
				    Session session = (Session) page;
                    //List<Session> sessionsForCurrentPlayer = DataProvider.TextSessionsContainingMyCharacters();
                    if (!CurrentPlayerIsGm && session.IsPrivate && session.PlayerCharacters.All(c => c.PlayerId != CurrentPlayer.Id))
                    {
                        return RedirectToAction("Index", "Page", new {id = id});
				    }
				    else
				    {

					    SessionTranscriptViewModel sessionTranscript = new SessionTranscriptViewModel();
					    sessionTranscript.Id = page.Id;
					    sessionTranscript.FullName = page.FullName;
					    sessionTranscript.ShortName = page.ShortName;
					    sessionTranscript.Transcript = LogGenerator.GenerateHtmlLog(id, true);

					    return View(sessionTranscript);
				    }
			    }
		    }

		    return RedirectToAction("Index", "Home");
	    }

        //public void CallSignalRUpdate()
        //{
        //    var context = GlobalHost.ConnectionManager.GetHubContext<RoleplayHub>();
        //    context.Clients.All.update();
        //}
    }	
}