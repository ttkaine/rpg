using System;
using System.Collections.Generic;
using Warhammer.Core.RoleplayViewModels;

namespace Warhammer.Core.Abstract
{
    public interface IModelFactory
    {
		PlayerViewModel GetPlayerForCurrentUser(int sessionId);
		PlayerViewModel GetPlayer(int playerId, int sessionId);
		List<PlayerViewModel> GetPlayersForSessionExcludingUser(int sessionId, out int gmId);
		CharacterViewModel GetCharacterForCurrentUser(int characterId, int sessionId);
		CharacterViewModel GetCharacter(int characterId);
		List<CharacterViewModel> GetCharactersForCurrentUserInSession(int sessionId);
		SessionViewModel GetSession(int sessionId);
		PostViewModel GetPost(int postId, out int playerId, out bool playerIsGm);
		List<PostViewModel> GetPostsForCurrentUserInSessionSinceLast(int sessionId, int lastPostId, out int playerId, out bool playerIsGm);
		List<PostViewModel> GetEditedPostsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate);
		List<int> GetDeletedPostIdsForCurrentUserInSessionSinceLast(int sessionId, DateTime lastUpdate);

        PostedImageViewModel GetPostedImage(int imageId);
    }
}
