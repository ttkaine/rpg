namespace Warhammer.Core
{
	public enum PostType
	{
		InCharacter = 1,
		OutOfCharacter = 2,
		GmInCharacter = 3,
		DiceRoll = 4,
        Image = 5
	}

	public enum RollType
	{
		DicePool10 = 1,
		Totalled = 2,
        DicePool6 = 3,
        FUDGE = 4
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
		InvalidPost = 9,
        ImageError = 10
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

    public enum AttributeType
    {
        Stat = 0,
        Skill = 1,
        Role = 2,
        Descriptor = 3,
        Wear = 4,
        Harm = 5,
        Edge = 6,
        Magic = 7,
        MagicItem = 8,
        Discipline = 9,
        Resilience = 10,
        Resolve = 11
    }

    public enum FixedWearAndHarmLevels
    {
        Trivial = 1,
        Slight = 2,
        Significant = 4,
        Serious = 6,
        Critical = 8
    }

    public enum PageLinkType
    {
        Other = 0,
        Person = 1,
        Place = 2,
        Session = 3,
        SessionLog = 4        
    }

    public enum DateAddType
    {
        Day,
        Week,
        Month        
    }
}
