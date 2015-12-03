using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.UI;
using Warhammer.Core;
using Warhammer.Core.Abstract;
using Warhammer.Core.Concrete;
using Warhammer.Core.Entities;
using Warhammer.Core.RoleplayViewModels;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.HtmlBuilders;
using Warhammer.Mvc.JsonObjects;
using Warhammer.Mvc.Models;
using Page = Warhammer.Core.Entities.Page;

namespace Warhammer.Mvc.Controllers
{
	//[Authorize]
    public class RoleplayController : BaseController
    {
		public IViewModelFactory ViewModelFactory { get; set; }
		public IPostManager PostManager { get; set; }
		public ILogGenerator LogGenerator { get; set; }
		//public ICharacterManager CharacterManager { get; set; }



        public RoleplayController(IAuthenticatedDataProvider data, IViewModelFactory viewModelFactory, IPostManager postManager, ILogGenerator logGenerator) : base(data)
        {
	        ViewModelFactory = viewModelFactory;
	        PostManager = postManager;
	        LogGenerator = logGenerator;
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
						List<Session> sessionsForCurrentPlayer = DataProvider.TextSessionsContainingMyCharacters();
						if (session.IsPrivate && sessionsForCurrentPlayer.All(s => s.Id != session.Id))
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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}

		}

		protected JsonResponseWithPostCollection GetRecentPostsForSession(int sessionId, int lastPostId, DateTime lastUpdateTime)
		{
			JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
			bool playerIsGm = false;
			int playerId = -1;
			List<PostViewModel> posts = ViewModelFactory.GetPostsForCurrentUserInSessionSinceLast(sessionId, lastPostId, out playerId, out playerIsGm);
			posts = (from p in posts
					 orderby p.ID ascending
					 select p).ToList();


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
				List<PostViewModel> editedPosts = ViewModelFactory.GetEditedPostsForCurrentUserInSessionSinceLast(sessionId, lastUpdateTime);
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

				postCollection.DeletedPosts = ViewModelFactory.GetDeletedPostIdsForCurrentUserInSessionSinceLast(sessionId, lastUpdateTime);
				postCollection.DeletedCount = postCollection.DeletedPosts.Count;
			}

			return postCollection;
		}

		public JsonResult GetCharacterList(int sessionId)
		{			
			if (DataProvider.IsLoggedIn())
			{
				List<CharacterViewModel> characters = ViewModelFactory.GetCharactersForCurrentUserInSession(sessionId);
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



		public JsonResult MakeTextPost(int sessionId, int characterId, int lastPostId, bool isOoc, string text, string lastUpdateTime, string recipientString)
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

				PostResult result = PostManager.CreateTextPostForUser(sessionId, characterId, isOoc, text, recipientString);
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();

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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();

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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}

		}

		public  JsonResult GetCharacterDetails(int sessionId, int characterId)
		{
			CharacterViewModel character = ViewModelFactory.GetCharacter(characterId);
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
						PlayerViewModel player = ViewModelFactory.GetPlayerForCurrentUser();
						SessionViewModel session = ViewModelFactory.GetSession(sessionId);

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
			SessionViewModel session = ViewModelFactory.GetSession(sessionId);

			string title = "Session Title";
			if (session != null)
			{
				title = session.Title;
			}

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return Json(serializer.Serialize(title));
		}

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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
		}

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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();

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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
				postCollection.IsError = true;
				postCollection.ErrorMessage = "Session timeout";
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				return Json("[" + serializer.Serialize(postCollection) + "]");
			}
		}

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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
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
				JsonResponseWithPostCollection postCollection = new JsonResponseWithPostCollection();
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
				List<PlayerViewModel> players = ViewModelFactory.GetPlayersForSessionExcludingUser(sessionId, out gmId);

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
		//		CharacterViewModel character = ViewModelFactory.GetCharacterForCurrentUser(characterId);
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
		//		CharacterViewModel character = ViewModelFactory.GetCharacterForCurrentUser(characterId);
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

		[OutputCache(Duration = 3600, VaryByParam = "id", Location = OutputCacheLocation.ServerAndClient, NoStore = true)]
		public ActionResult Image(int id)
		{
			CharacterViewModel character = ViewModelFactory.GetCharacter(id);
			var defaultDir = Server.MapPath("/Content/Images/RoleplayForum");

			if (character != null)
			{
				if (character.Image != null && character.Image.Length > 100 && !string.IsNullOrWhiteSpace(character.ImageMimeType))
				{
					return File(character.Image, character.ImageMimeType);
				}
			}

			var defaultImagePath = Path.Combine(defaultDir, "no-image.jpg");
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
					SessionTranscriptViewModel sessionTranscript = new SessionTranscriptViewModel();
				    sessionTranscript.Id = page.Id;
				    sessionTranscript.FullName = page.FullName;
				    sessionTranscript.ShortName = page.ShortName;
					sessionTranscript.Transcript = LogGenerator.GenerateHtmlLog(id, true);
				
					return View(sessionTranscript);
				}
		    }

		    return RedirectToAction("Index", "Home");
	    }
	}	
}