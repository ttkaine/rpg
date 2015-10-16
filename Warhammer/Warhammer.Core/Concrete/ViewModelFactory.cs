using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.RoleplayViewModels;

namespace Warhammer.Core.Concrete
{
    public class ViewModelFactory : IViewModelFactory
    {
	    private bool PlayerIsGm(Player player)
	    {
		    return player.Id == 1;
	    }

	    private int GetGmId()
	    {
		    return 1;
	    }

	    private readonly IAuthenticatedDataProvider _dataProvider;
	    private IAuthenticatedDataProvider DataProvider
	    {
		    get { return _dataProvider; }
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

	    public ViewModelFactory(IAuthenticatedDataProvider dataProvider, IAuthenticatedUserProvider userProvider, IRepository repository)
	    {
		    _userProvider = userProvider;
		    _dataProvider = dataProvider;
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
			    viewModel.IsGM = PlayerIsGm(player);

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
				viewModel.IsGM = PlayerIsGm(player);

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
			gmId = -1;
		    Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
		    PlayerViewModel currentPlayer = GetPlayerForCurrentUser();

			if (session != null && currentPlayer != null)
			{
				gmId = GetGmId();
				foreach (Player player in Repo.Players())
				{
					if (player.Id != currentPlayer.ID)
					{
						PlayerViewModel viewModel = new PlayerViewModel();
						viewModel.ID = player.Id;
						viewModel.Name = player.DisplayName;
						viewModel.IsGM = PlayerIsGm(player);
						viewModels.Add(viewModel);
					}
				}
			}

			return viewModels;
	    }

	    public CharacterViewModel GetCharacterForCurrentUser(int characterId)
	    {
			PlayerViewModel currentPlayer = GetPlayerForCurrentUser();
		    Person character = Repo.People().FirstOrDefault(p => p.Id == characterId && p.PlayerId == currentPlayer.ID);

			if (character != null)
			{
				CharacterViewModel viewModel = new CharacterViewModel();
				viewModel.ID = character.Id;
				Page characterPage = Repo.Pages().OfType<Person>().FirstOrDefault(p => p.Id == character.Id);
				if (characterPage != null)
				{
					viewModel.Description = characterPage.RawText;
				}
				viewModel.Name = character.ShortName;
				viewModel.Image = character.ImageData;
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
			Page characterPage = Repo.Pages().OfType<Person>().FirstOrDefault(p => p.Id == character.Id);
			if (characterPage != null)
			{
				viewModel.Description = characterPage.RawText;
			}
			viewModel.Name = character.ShortName;
			viewModel.Image = character.ImageData;
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
					viewModels.Add(new CharacterViewModel() { ID = 0, Name = "GM" });
				}
				foreach (Person character in session.PlayerCharacters)
				{
					viewModels.Add(GetCharacterViewModel(character));
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
				viewModel.IsClosed = false; // TODO: Add status to DB
				viewModel.StartDate = session.DateTime.GetValueOrDefault();
				viewModel.Title = session.FullName;
				viewModel.GmId = GetGmId();

				return viewModel;
			}

			return null;
	    }

	    public List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId)
	    {
		    throw new NotImplementedException();
	    }

	    public List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId, out int playerId, out bool playerIsGm)
	    {
		    throw new NotImplementedException();
	    }

	    public List<PostViewModel> GetEditedPostsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate)
	    {
		    throw new NotImplementedException();
	    }

	    public List<int> GetDeletedPostIdsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate)
	    {
		    throw new NotImplementedException();
	    }
    }
}
