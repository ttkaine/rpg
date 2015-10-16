using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warhammer.Core
{
	public enum PostType
	{
		InCharacter = 1,
		OutOfCharacter = 2,
		GmInCharacter = 3,
		DiceRoll = 4
	}

	public enum RollType
	{
		DicePool = 1,
		Totalled = 2
	}

	public enum PostResult
	{
		Success = 1,
		PlayerNotInCampaign = 2,
		CharacterNotInCampaign = 3,
		InvalidCharacter = 4,
		InvalidPlayer = 5,
		InvalidSession = 6,
		InvalidDiceRoll = 7,
		SessionClosed = 8,
		InvalidPost = 9
	}

	public enum MediaSaveResult
	{
		Success = 0,
		Failure = 1,
		FileTooBig = 2,
		WrongFormat = 3,
		FileExists = 4
	}

	public enum CharacterSheetStyle
	{
		So82013Standard = 1,
		So82013Fantasy = 2
	}
}
