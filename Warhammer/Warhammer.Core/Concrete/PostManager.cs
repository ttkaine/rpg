﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Tools;

namespace Warhammer.Core.Concrete
{
	public class PostManager : IPostManager
	{
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

		public PostManager(IRepository repository, IAuthenticatedUserProvider userProvider)
		{
			_repository = repository;
			_userProvider = userProvider;
			_diceRoller = DiceRoller.Instance;
		}

		private Player GetCurrentPlayer()
		{
			return Repo.Players().FirstOrDefault(p => p.UserName == UserProvider.UserName);			
		}

		private bool CharacterIsInSession(Session session, int characterId)
		{
			return characterId == 0 || session.People.Where(p => p.Id == characterId).LongCount() > 0;
		}

		public PostResult CreateTextPostForUser(int sessionId, int characterId, bool isOoc, string text, string recipientString)
		{
			Player player = GetCurrentPlayer();
			Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
			Person character = Repo.People().FirstOrDefault(p => p.Id == characterId && (p.PlayerId == player.Id || (p.PlayerId == null && player.IsGm)));

			if (player == null)
			{
				return PostResult.InvalidPlayer;
			}
			if (!isOoc && character == null && characterId != 0)
			{
				return PostResult.InvalidCharacter;
			}
			if (session == null)
			{
				return PostResult.InvalidSession;
			}
			if (!isOoc && characterId == 0 && !player.IsGm)
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
				DatePosted = DateTime.Now,
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

			return PostResult.Success;
		}

		public PostResult CreateDiceRollPostForUser(int sessionId, int characterId, int dieSize, int dieCount, int rollType, int rollTarget, bool reRollMaximums, bool isPrivate)
		{
			Player player = GetCurrentPlayer();
			Session session = Repo.Pages().OfType<Session>().FirstOrDefault(s => s.Id == sessionId);
			Person character = Repo.People().FirstOrDefault(p => p.Id == characterId && (p.PlayerId == player.Id || (p.PlayerId == null && player.IsGm)));

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
			if (characterId == 0 && !player.IsGm)
			{
				return PostResult.InvalidCharacter;
			}
			if (!CharacterIsInSession(session, characterId))
			{
				return PostResult.CharacterNotInCampaign;
			}
			if (session.IsClosed)
			{
				return PostResult.SessionClosed;
			}
			if (dieSize != 4 && dieSize != 6 && dieSize != 8 && dieSize != 10 && dieSize != 12 && dieSize != 20 && dieSize != 100)
			{
				return PostResult.InvalidDiceRoll;
			}
			if (rollType < 1 || rollType > 2)
			{
				return PostResult.InvalidDiceRoll;
			}
			if (rollType == (int)RollType.DicePool && (dieSize != 10 || rollTarget < 2 || rollTarget > 10))
			{
				return PostResult.InvalidDiceRoll;
			}
			if (dieCount < 1 || dieCount > 30)
			{
				return PostResult.InvalidDiceRoll;
			}

			List<int> rolls = DiceRoller.RollDice(dieSize, dieCount, rollType, rollTarget, reRollMaximums);
			StringBuilder rollValues = new StringBuilder();
			foreach (int roll in rolls)
			{
				if (rollValues.Length > 0)
				{
					rollValues.Append(",");
				}
				rollValues.Append(roll);
			}

			int gmId = GetGmId();
			string targetPlayerIds = null;
			if (isPrivate)
			{
				targetPlayerIds = player.Id.ToString();
				if (!player.IsGm)
				{
					targetPlayerIds += "," + gmId;
				}
			}

			Post post = new Post()
			{
				CharacterId = characterId > 0 ? (int?)characterId : null,
				OriginalContent = string.Empty,
				RevisedContent = string.Empty,
				DatePosted = DateTime.Now,
				DieCount = dieCount,
				DieSize = dieSize,
				PlayerId = player.Id,
				PostType = (int)PostType.DiceRoll,
				ReRollMaximums = reRollMaximums,
				RollTarget = rollTarget,
				RollValues = rollValues.ToString(),
				RollType = (int)rollType,
				SessionId = session.Id,
				TargetPlayerIds = targetPlayerIds,
				LastEdited = null
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
						if (player.IsGm || player.Id == post.PlayerId)
						{
							post.IsDeleted = true;
							post.DeletedDate = DateTime.Now;
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

					if (player.IsGm || post.PlayerId == player.Id)
					{
						post.RevisedContent = text;
						post.IsRevised = true;
						post.LastEdited = DateTime.Now;
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
						if (player.IsGm)
						{
							post.IsRevised = false;
							post.LastEdited = DateTime.Now;
							Repo.Save(post);
							return true;
						}
					}
				}
			}

			return false;	
		}
	}
}
