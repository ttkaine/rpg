using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.RoleplayViewModels;

namespace Warhammer.Core.Concrete
{
    public class ModelFactory : IModelFactory
    {
		//private bool PlayerIsGm(Player player)
		//{
		//	return player.Id == 1;
		//}

	    private int GetGmId()
	    {
		    Player player = Repo.Players().FirstOrDefault(p => p.IsGm);
		    if (player != null)
		    {
			    return player.Id;
		    }
		    else
		    {
			    return 0;
		    }
	    }

	    private readonly IAuthenticatedUserProvider _userProvider;
	    private IAuthenticatedUserProvider UserProvider
	    {
		    get { return _userProvider; }
	    }

	    private readonly IRepository _repository;
	    private IRepository Repo
	    {
		    get { return _repository;  }
	    }

	    public ModelFactory()
	    {		    
	    }

	    public ModelFactory(IAuthenticatedUserProvider userProvider, IRepository repository)
	    {
		    _userProvider = userProvider;
		    _repository = repository;
	    }

	    public PlayerViewModel GetPlayerForCurrentUser()
	    {
		    Player player = Repo.Players().FirstOrDefault(p => p.UserName == UserProvider.UserName);

		    if (player != null)
		    {
			    PlayerViewModel viewModel = new PlayerViewModel();
			    viewModel.ID = player.Id;
			    viewModel.Name = player.DisplayName;
			    viewModel.IsGM = player.IsGm;

			    return viewModel;
		    }
		    else
		    {
			    return null;
		    }
	    }

	    public PlayerViewModel GetPlayer(int playerId)
	    {
			Player player = Repo.Players().FirstOrDefault(p => p.Id == playerId);

			if (player != null)
			{
				PlayerViewModel viewModel = new PlayerViewModel();
				viewModel.ID = player.Id;
				viewModel.Name = player.DisplayName;
				viewModel.IsGM = player.IsGm;

				return viewModel;
			}
			else
			{
				return null;
			}
		}

	    public List<PlayerViewModel> GetPlayersForSessionExcludingUser(int sessionId, out int gmId)
	    {
			List<PlayerViewModel> viewModels = new List<PlayerViewModel>();
		    Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
		    PlayerViewModel currentPlayer = GetPlayerForCurrentUser();

			if (session != null && currentPlayer != null)
			{
				foreach (Player player in Repo.Players())
				{
					if (player.Id != currentPlayer.ID)
					{
						PlayerViewModel viewModel = new PlayerViewModel();
						viewModel.ID = player.Id;
						viewModel.Name = player.DisplayName;
						viewModel.IsGM = player.IsGm;
						viewModels.Add(viewModel);
					}
				}
			}

		    gmId = GetGmId();

			return viewModels;
	    }

	    public CharacterViewModel GetCharacterForCurrentUser(int characterId)
	    {
			PlayerViewModel currentPlayer = GetPlayerForCurrentUser();
		    Person character = Repo.People().FirstOrDefault(p => p.Id == characterId && (p.PlayerId == currentPlayer.ID || (p.PlayerId == null && currentPlayer.IsGM)));

			if (character != null)
			{
				CharacterViewModel viewModel = new CharacterViewModel();
				viewModel.ID = character.Id;
				viewModel.Description = character.RawText;
				viewModel.Name = character.ShortName;
				viewModel.Image = character.PrimaryImage;
				viewModel.ImageMimeType = character.ImageMime;
				viewModel.PlayerId = character.PlayerId.GetValueOrDefault();
				viewModel.CharacterSheet = string.Empty;
				
				return viewModel;
			}

			return null;
	    }

	    private CharacterViewModel GetCharacterViewModel(Person character)
	    {
			CharacterViewModel viewModel = new CharacterViewModel();
			viewModel.ID = character.Id;
			viewModel.Description = character.RawText;
			viewModel.Name = character.ShortName;
			viewModel.Image = character.PrimaryImage;
		    viewModel.ImageMimeType = character.ImageMime;
			viewModel.PlayerId = character.PlayerId.GetValueOrDefault();
			viewModel.CharacterSheet = string.Empty;

			return viewModel;		    
	    }

	    public CharacterViewModel GetCharacter(int characterId)
	    {
			Person character = Repo.People().FirstOrDefault(p => p.Id == characterId);

			if (character != null)
			{
				return GetCharacterViewModel(character);
			}

			return null;
	    }

	    public List<CharacterViewModel> GetCharactersForCurrentUserInSession(int sessionId)
	    {
			Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
			PlayerViewModel player = GetPlayerForCurrentUser();

			List<CharacterViewModel> viewModels = new List<CharacterViewModel>();
			if (player != null && session != null)
			{
				if (player.IsGM)
				{
					viewModels.Add(new CharacterViewModel() {ID = 0, Name = "GM"});
					viewModels.AddRange(session.Npcs.Select(GetCharacterViewModel));
				}
				else
				{
					viewModels.AddRange(from character in session.PlayerCharacters where character.PlayerId == player.ID select GetCharacterViewModel(character));
					viewModels.Add(new CharacterViewModel() { ID = -1, Name = "Environment" });
				}
			}

			return viewModels;
		}

	    public SessionViewModel GetSession(int sessionId)
	    {
			//SessionModel session = Data.GetSession(sessionId);
			Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);

			if (session != null)
			{
				SessionViewModel viewModel = new SessionViewModel();
				viewModel.Description = session.Description;
				viewModel.IsClosed = session.IsClosed;
				viewModel.StartDate = session.DateTime.GetValueOrDefault();
				viewModel.Title = session.ShortName;
				viewModel.GmId = GetGmId();
			    viewModel.CurrentPlayerId = GetCurrentPlayerId(session);
				return viewModel;
			}

			return null;
	    }

		public PostViewModel GetPost(int postId, out int playerId, out bool playerIsGm)
	    {
			PlayerViewModel player = GetPlayerForCurrentUser();
			Post post = Repo.Posts().FirstOrDefault(p => p.Id == postId);
			int gmId = GetGmId();
			playerId = player.ID;
			playerIsGm = player.IsGM;

		    if (post != null)
		    {
				List<int> playerIds = new List<int>();
				StringBuilder names = new StringBuilder();
				if (post.TargetPlayerIds != null)
				{
					playerIds.AddRange(GetIntsFromString(post.TargetPlayerIds));
					foreach (int id in playerIds)
					{
						PlayerViewModel targetPlayer = GetPlayer(id);
						if (targetPlayer != null)
						{
							if (names.Length > 0)
							{
								names.Append(", ");
							}
							names.Append(targetPlayer.Name);
							if (targetPlayer.IsGM)
							{
								names.Append(" (GM)");
							}
						}
					}
				}
				else
				{
					playerIds.Add(player.ID);
				}

				if (post.PlayerId == player.ID || playerIds.Contains(player.ID) || (player.ID == gmId && post.PostType == (int)PostType.DiceRoll))
				{
					PostViewModel viewModel = GetPostViewModelForPost(post, gmId);
					if (viewModel != null)
					{
						viewModel.TargetPlayerNames = names.ToString();
					}
					return viewModel;
				}			    
		    }

		    return null;
	    }

	    private int GetCurrentPlayerId(Session session)
        {
            if (!session.IsGmTurn)
            { 
                PostOrder order = session.PostOrders.OrderBy(p => p.LastTurnEnded).FirstOrDefault();
                if (order != null)
                {
                    return order.PlayerId;
                }
            }
            return GetGmId();
        }

        public List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId)
	    {
			bool playerIsGm;
			int playerId;
			return GetPostsForCurrentUserInSessionSinceLast(sessionId, lastPostId, out playerId, out playerIsGm);
	    }

		private List<int> GetIntsFromString(string intString)
		{
			List<int> ints = new List<int>();
			string[] ids = intString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string id in ids)
			{
				int value;
				if (int.TryParse(id, out value))
				{
					ints.Add(value);
				}
			}
			return ints;
		}

	    public List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId, out int playerId, out bool playerIsGm)
	    {
		    playerIsGm = false;
			playerId = -1;
		    int gmId = GetGmId();
			//PlayerModel player = Data.GetPlayerDataForUserId(currentUserId);
			//SessionModel session = Data.GetSession(sessionId);
			Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
			PlayerViewModel player = GetPlayerForCurrentUser();

			List<PostViewModel> viewModels = new List<PostViewModel>();
			if (player != null && session != null)
			{
				playerId = player.ID;
				List<Post> posts = Repo.Posts().Where(p => p.SessionId == session.Id && p.Id > lastPostId && !p.IsDeleted).ToList();

				foreach (Post post in posts)
				{
					List<int> playerIds = new List<int>();
					StringBuilder names = new StringBuilder();
					if (post.TargetPlayerIds != null)
					{
						playerIds.AddRange(GetIntsFromString(post.TargetPlayerIds));
						foreach (int id in playerIds)
						{
							PlayerViewModel targetPlayer = GetPlayer(id);
							if (targetPlayer != null)
							{
								if (names.Length > 0)
								{
									names.Append(", ");
								}
								names.Append(targetPlayer.Name);
								if (targetPlayer.IsGM)
								{
									names.Append(" (GM)");
								}
							}
						}
					}
					else
					{
						playerIds.Add(player.ID);
					}

					if (post.PlayerId == player.ID || playerIds.Contains(player.ID) || (player.ID == gmId && post.PostType == (int)PostType.DiceRoll))
					{
						PostViewModel viewModel = GetPostViewModelForPost(post, gmId);
						if (viewModel != null)
						{
							viewModel.TargetPlayerNames = names.ToString();
							viewModels.Add(viewModel);
						}
					}
					
				}
				playerIsGm = player.IsGM;
			}

		    
			return viewModels;
	    }

		private PostViewModel GetPostViewModelForPost(Post post, int gmId)
		{
			PostViewModel viewModel;
			if (post.PostType == (int)PostType.DiceRoll)
			{
				DiceRollPostViewModel diceRollViewModel = new DiceRollPostViewModel();
				diceRollViewModel.DieCount = post.DieCount;
				diceRollViewModel.DieSize = post.DieSize;
				diceRollViewModel.RollTarget = post.RollTarget;
				diceRollViewModel.RollType = post.RollType;
				diceRollViewModel.RollValues.AddRange(GetIntsFromString(post.RollValues));
				diceRollViewModel.ReRollMaximums = post.ReRollMaximums;

				viewModel = diceRollViewModel;
			}
			else
			{
				TextPostViewModel textViewModel = new TextPostViewModel();
				if (!post.IsRevised)
				{
					textViewModel.Content = post.OriginalContent.Replace("{CR}", "<br />");
					textViewModel.LastEdited = null;
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(post.RevisedContent))
					{
						textViewModel.Content = post.RevisedContent.Replace("{CR}", "<br />");
						textViewModel.CanRevert = true;
					}
					else
					{
						textViewModel.Content = post.OriginalContent.Replace("{CR}", "<br />");
					}					
					textViewModel.LastEdited = post.LastEdited.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm:ss");
					textViewModel.IsRevised = true;
				}

				textViewModel.IsOoc = post.PostType == (int)PostType.OutOfCharacter;

				viewModel = textViewModel;
			}

			viewModel.ID = post.Id;
			viewModel.DatePosted = post.DatePosted.ToString("dd/MM/yyyy HH:mm:ss");
			viewModel.IsPostedByGm = post.PlayerId == gmId;
			viewModel.PlayerId = post.PlayerId;
			viewModel.IsPrivate = post.TargetPlayerIds != null;

			if (post.CharacterId != null)
			{
				Person character = Repo.People().FirstOrDefault(p => p.Id == post.CharacterId);
				if (character != null)
				{
					viewModel.CharacterId = character.Id;
					viewModel.CharacterName = character.ShortName;
				}
				else
				{
					if (viewModel.IsPostedByGm)
					{
						viewModel.CharacterName = "GM";
					}
					else
					{
						viewModel.CharacterName = "Unknown";
					}

				}
			}
			else
			{
				if (viewModel.IsPostedByGm)
				{
					viewModel.CharacterName = "GM";
				}
				else
				{
					viewModel.CharacterName = "Environment";
				}
			}

			PlayerViewModel player = GetPlayer(post.PlayerId);
			if (player != null)
			{
				viewModel.PlayerName = player.Name;
			}

			viewModel.PostType = post.PostType;

			return viewModel;
		}

	    public List<PostViewModel> GetEditedPostsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate)
	    {
			//PlayerModel player = Data.GetPlayerDataForUserId(currentUserId);
			//SessionModel session = Data.GetSession(sessionId);
			Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
			PlayerViewModel player = GetPlayerForCurrentUser();
		    int gmId = GetGmId();

			List<PostViewModel> viewModels = new List<PostViewModel>();
			if (player != null && session != null)
			{
				List<Post> posts = Repo.Posts().Where(p => p.SessionId == session.Id && p.LastEdited >= lastUpdate && !p.IsDeleted).ToList();
				foreach (Post post in posts)
				{
					List<int> playerIds = new List<int>();
					StringBuilder names = new StringBuilder();
					if (post.TargetPlayerIds != null)
					{
						playerIds.AddRange(GetIntsFromString(post.TargetPlayerIds));
						foreach (int id in playerIds)
						{
							PlayerViewModel targetPlayer = GetPlayer(id);
							if (targetPlayer != null)
							{
								if (names.Length > 0)
								{
									names.Append(", ");
								}
								names.Append(targetPlayer.Name);
								if (targetPlayer.IsGM)
								{
									names.Append(" (GM)");
								}
							}
						}
					}
					else
					{
						playerIds.Add(player.ID);
					}

					if (post.PlayerId == player.ID || playerIds.Contains(player.ID) || (player.IsGM && post.PostType == (int)PostType.DiceRoll))
					{
						PostViewModel viewModel = GetPostViewModelForPost(post, gmId);
						if (viewModel != null)
						{
							viewModel.TargetPlayerNames = names.ToString();
							viewModels.Add(viewModel);
						}
					}
				}
			}
			return viewModels;		
	    }

	    public List<int> GetDeletedPostIdsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate)
	    {
			List<Post> posts = Repo.Posts().Where(p => p.SessionId == sessionId && p.DeletedDate >= lastUpdate && p.IsDeleted).ToList();

			List<int> ids = new List<int>();
			foreach (Post post in posts)
			{
				ids.Add(post.Id);
			}

			return ids;
	    }
    }
}
