using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Tools;

namespace Warhammer.Core.Concrete
{
	public class PostManager : IPostManager
	{
	    private readonly IAuthenticatedDataProvider _data;
        private readonly IAzureProvider _azureProvider;

        private int GetGmId(int sessionId)
		{
			return _data.GetGmId(sessionId);
		}

	    private readonly IRepository _repository;
	    private IRepository Repo
	    {
		    get { return _repository;  }
	    }

		private readonly IAuthenticatedUserProvider _userProvider;
		private IAuthenticatedUserProvider UserProvider
		{
			get { return _userProvider; }
		}

		private readonly DiceRoller _diceRoller;
		private DiceRoller DiceRoller { get { return _diceRoller; } }

		public PostManager(IRepository repository, IAuthenticatedUserProvider userProvider, IAuthenticatedDataProvider data, IAzureProvider azureProvider)
		{
			_repository = repository;
			_userProvider = userProvider;
		    _data = data;
            _azureProvider = azureProvider;
            _diceRoller = DiceRoller.Instance;
		}

		private Player GetCurrentPlayer()
		{
			return Repo.Players().FirstOrDefault(p => p.UserName == UserProvider.UserName);			
		}

		private bool CharacterIsInSession(Session session, int characterId)
		{
			return characterId == 0 || characterId == -1 || session.People.Where(p => p.Id == characterId).LongCount() > 0;
		}

		public PostResult CreateTextPostForUser(int sessionId, int characterId, bool isOoc, string text, string recipientString)
		{
			Player player = GetCurrentPlayer();
            int gmId = GetGmId(sessionId);

            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
			Person character = Repo.People().FirstOrDefault(p => p.Id == characterId && (p.PlayerId == player.Id || (p.PlayerId == null && player.Id == gmId)));

			if (player == null)
			{
				return PostResult.InvalidPlayer;
			}
			if (!isOoc && character == null && characterId != 0 && characterId != -1)
			{
				return PostResult.InvalidCharacter;
			}
			if (session == null)
			{
				return PostResult.InvalidSession;
			}
			if (!isOoc && characterId == 0 && player.Id != gmId)
			{
				return PostResult.InvalidCharacter;
			}
			if (!isOoc && characterId == -1 && player.Id == gmId)
			{
				return PostResult.InvalidCharacter;
			}
			if (!CharacterIsInSession(session, characterId) && !isOoc)
			{
				return PostResult.CharacterNotInCampaign;
			}
			if (session.IsClosed)
			{
				return PostResult.SessionClosed;
			}

			Post post = new Post()
			{
				CharacterId = characterId > 0 ? (int?)characterId : null,
				OriginalContent = text,
				RevisedContent = string.Empty,
				DatePosted = DateTime.UtcNow,
				DieCount = 0,
				DieSize = 0,
				PlayerId = player.Id,
				PostType = (isOoc ? (int)PostType.OutOfCharacter : (characterId == 0 ? (int)PostType.GmInCharacter : (int)PostType.InCharacter)),
				ReRollMaximums = false,
				RollTarget = 0,
				RollValues = string.Empty,
				RollType = 0,
				SessionId = session.Id,
				TargetPlayerIds = recipientString.Trim().Length > 0 ? recipientString.Trim() : null,
				LastEdited = null
			};

			Repo.Save(post);
			if (!isOoc && player.Id != gmId)
			{
				SetTurnOverForUser(sessionId);
			}

			return PostResult.Success;
		}

		public PostResult CreateDiceRollPostForUser(int sessionId, int characterId, int dieSize, int dieCount, int rollType, int rollTarget, bool reRollMaximums, bool isPrivate)
		{
			Player player = GetCurrentPlayer();
            int gmId = GetGmId(sessionId);
            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
			Person character = Repo.People().FirstOrDefault(p => p.Id == characterId && (p.PlayerId == player.Id || (p.PlayerId == null && player.Id == gmId)));

			if (player == null)
			{
				return PostResult.InvalidPlayer;
			}
			if (character == null && characterId != 0)
			{
				return PostResult.InvalidCharacter;
			}
			if (session == null)
			{
				return PostResult.InvalidSession;
			}
            // commented out so that Players can do dice rolls as Environment, may need more elegant fixings 
			//if (characterId == 0 && player.Id != gmId)
			//{
			//	return PostResult.InvalidCharacter;
			//}
			if (!CharacterIsInSession(session, characterId))
			{
				return PostResult.CharacterNotInCampaign;
			}
			if (session.IsClosed)
			{
				return PostResult.SessionClosed;
			}
            if (rollType < 1 || rollType > 4)
            {
                return PostResult.InvalidDiceRoll;
            }
            if (rollType != (int)RollType.FUDGE && dieSize != 4 && dieSize != 6 && dieSize != 8 && dieSize != 10 && dieSize != 12 && dieSize != 20 && dieSize != 100)
			{
				return PostResult.InvalidDiceRoll;
			}
            if (rollType == (int)RollType.FUDGE && dieSize != 3)
            {
                return PostResult.InvalidDiceRoll;
            }
			if (rollType == (int)RollType.DicePool10 && (dieSize != 10 || rollTarget < 2 || rollTarget > 10))
			{
				return PostResult.InvalidDiceRoll;
			}
            if (rollType == (int)RollType.DicePool6 && (dieSize != 6 || rollTarget < 2 || rollTarget > 6))
            {
                return PostResult.InvalidDiceRoll;
            }
            if (dieCount < 1 || dieCount > 30)
			{
				return PostResult.InvalidDiceRoll;
			}

			List<int> rolls = (rollType == (int)RollType.FUDGE) ? DiceRoller.RollFudgeDice(dieCount) : DiceRoller.RollDice(dieSize, dieCount, rollType, rollTarget, reRollMaximums);
			StringBuilder rollValues = new StringBuilder();
			foreach (int roll in rolls)
			{
				if (rollValues.Length > 0)
				{
					rollValues.Append(",");
				}
				rollValues.Append(roll);
			}

			string targetPlayerIds = null;
			if (isPrivate)
			{
				targetPlayerIds = player.Id.ToString();
				if (player.Id != gmId)
				{
					targetPlayerIds += "," + gmId;
				}
			}

			Post post = new Post()
			{
				CharacterId = characterId > 0 ? (int?)characterId : null,
				OriginalContent = string.Empty,
				RevisedContent = string.Empty,
				DatePosted = DateTime.UtcNow,
				DieCount = dieCount,
				DieSize = dieSize,
				PlayerId = player.Id,
				PostType = (int)PostType.DiceRoll,
				ReRollMaximums = reRollMaximums,
				RollTarget = rollTarget,
				RollValues = rollValues.ToString(),
				RollType = (int)rollType,
				SessionId = session.Id,
				TargetPlayerIds = null,
				LastEdited = null
			};

			Repo.Save(post);

			return PostResult.Success;
		}

	    public PostResult CreateImagePostForUser(int sessionId, byte[] imageBytes, string fileName)
	    {
            Player player = GetCurrentPlayer();
            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);

            if (player == null)
            {
                return PostResult.InvalidPlayer;
            }
            if (session == null)
            {
                return PostResult.InvalidSession;
            }
            if (session.IsClosed)
            {
                return PostResult.SessionClosed;
            }
	        if (imageBytes.Length < 1 || imageBytes.Length > 4194304)
	        {
	            return PostResult.ImageError;
	        }

            PageImage pageImage = new PageImage()
	        {
                CampaignId = session.CampaignId,
                FileIdentifier = _azureProvider.CreateImageBlob(imageBytes),
                IsPrimary = false,
                PageId = session.Id
	        };

	        int imageId = Repo.Save(pageImage);

	        if (imageId <= 0)
	        {
	            return PostResult.ImageError;
	        }

            Post post = new Post()
            {
                CharacterId = null,
                OriginalContent = fileName,
                RevisedContent = string.Empty,
                DatePosted = DateTime.UtcNow,
                DieCount = 0,
                DieSize = 0,
                PlayerId = player.Id,
                PostType = (int)PostType.Image,
                ReRollMaximums = false,
                RollTarget = 0,
                RollValues = string.Empty,
                RollType = 0,
                SessionId = session.Id,
                TargetPlayerIds = null,
                LastEdited = null,
                ImageId = imageId
            };

            Repo.Save(post);

            return PostResult.Success;
        }

	    public bool DeletePostForUser(int postId)
		{
			Player player = GetCurrentPlayer();
			Post post = Repo.Posts().FirstOrDefault(p => p.Id == postId);

			if (post != null && player != null)
			{
				Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == post.SessionId);
				if (session != null)
				{
					if (!session.IsClosed)
					{
						if (player.Id == GetGmId(post.SessionId) || player.Id == post.PlayerId)
						{
							post.IsDeleted = true;
							post.DeletedDate = DateTime.UtcNow;
							Repo.Save(post);
							return true;
						}
					}
				}
			}

			return false;
		}

		public PostResult EditTextPostForUser(int postId, string text)
		{
			Player player = GetCurrentPlayer();
			Post post = Repo.Posts().FirstOrDefault(p => p.Id == postId);

			if (post == null)
			{
				return PostResult.InvalidPost;
			}

			if (player != null)
			{
				Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == post.SessionId);
				if (session != null)
				{
					if (session.IsClosed)
					{
						return PostResult.SessionClosed;
					}

					if (player.Id == GetGmId(post.SessionId) || post.PlayerId == player.Id)
					{
						post.RevisedContent = text;
						post.IsRevised = true;
						post.LastEdited = DateTime.UtcNow;
						Repo.Save(post);

						return PostResult.Success;
					}
					else
					{
						return PostResult.InvalidPlayer;
					}
				}
				else
				{
					return PostResult.InvalidSession;
				}
			}
			else
			{
				return PostResult.InvalidPlayer;
			}
		}

		public bool RevertPostForUser(int postId)
		{
			Player player = GetCurrentPlayer();
			Post post = Repo.Posts().FirstOrDefault(p => p.Id == postId);

			if (post != null && player != null)
			{
				Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == post.SessionId);
				if (session != null)
				{
					if (!session.IsClosed)
					{
						if (player.Id == GetGmId(post.SessionId))
						{
							post.IsRevised = false;
							post.LastEdited = DateTime.UtcNow;
							Repo.Save(post);
							return true;
						}
					}
				}
			}

			return false;	
		}

		public bool MarkPostOoc(int postId)
		{
			Player player = GetCurrentPlayer();
			Post post = Repo.Posts().FirstOrDefault(p => p.Id == postId);

			if (post != null && player != null)
			{
				Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == post.SessionId);
				if (session != null)
				{
					if (!session.IsClosed)
					{
						if (player.Id == GetGmId(post.SessionId) || player.Id == post.PlayerId)
						{
							post.PostType = (int) PostType.OutOfCharacter;
							post.IsRevised = true;
							post.LastEdited = DateTime.UtcNow;
							Repo.Save(post);
							return true;
						}
					}
				}
			}

			return false;
		}

		public bool SetTurnOverForUser(int sessionId)
	    {
            Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
            Player player = GetCurrentPlayer();
	        if (session != null)
	        {
	            PostOrder order = session.PostOrders.FirstOrDefault(p => p.PlayerId == player.Id);
	            if (order != null)
	            {
	                order.LastTurnEnded = DateTime.UtcNow;
	            }

	            int gmId = GetGmId(sessionId);
	            if (player.Id == gmId || session.GmIsSuspended)
	            {
	                session.IsGmTurn = false;
	            }
	            if (player.Id != gmId && !session.GmIsSuspended)
	            {
	                session.IsGmTurn = true;
	            }

                _repository.Save(session);

	            _data.NotifyTurn(sessionId);
                return true;
            }
	        return false;
	    }
	}
}
