using System;
using System.Collections.Generic;
using Warhammer.Core.RoleplayViewModels;

namespace Warhammer.Core.Abstract
{
    public interface IModelFactory
    {
		PlayerViewModel GetPlayerForCurrentUser();
		PlayerViewModel GetPlayer(int playerId);
		List<PlayerViewModel> GetPlayersForSessionExcludingUser(int sessionId, out int gmId);

		//List<CharacterViewModel> GetCharactersForCurrentUser();
		CharacterViewModel GetCharacterForCurrentUser(int characterId);
		CharacterViewModel GetCharacter(int characterId);
		//List<CharacterViewModel> GetCharactersForCurrentUserInCampaign(int campaignId);
		//List<CharacterViewModel> GetCharactersForCurrentUserNotInCampaign(int campaignId);
		List<CharacterViewModel> GetCharactersForCurrentUserInSession(int sessionId);

		//List<CampaignViewModel> GetCurrentCampaignsForUser(Guid currentUserId);
		//List<CampaignViewModel> GetAvailableCampaignsForUser(Guid currentUserId);
		//List<CampaignViewModel> GetCompletedCampaignsForUser(Guid currentUserId);
		//CampaignViewModel GetCampaign(int campaignId);

		//List<SessionViewModel> GetCurrentSessionsForUserInCampaign();
		//List<SessionViewModel> GetCompletedSessionsForUserInCampaign();
		//SessionViewModel GetSessionForCurrentUser(int sessionId);
		SessionViewModel GetSession(int sessionId);

		List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId);
		List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId, out int playerId, out bool playerIsGm);
		List<PostViewModel> GetEditedPostsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate);
		List<int> GetDeletedPostIdsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate);
    }
}
