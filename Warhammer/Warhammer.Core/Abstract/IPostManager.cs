namespace Warhammer.Core.Abstract
{
	public interface IPostManager
	{
		PostResult CreateTextPostForUser(int sessionId, int characterId, bool isOoc, string text, string recipientString);
		PostResult CreateDiceRollPostForUser(int sessionId, int characterId, int dieSize, int dieCount, int rollType, int rollTarget, bool reRollMaximums, bool isPrivate);
		bool DeletePostForUser(int postId);
		PostResult EditTextPostForUser(int postId, string text);
		bool RevertPostForUser(int postId);
	    bool SetTurnOverForUser(int sessionId);
	}
}
