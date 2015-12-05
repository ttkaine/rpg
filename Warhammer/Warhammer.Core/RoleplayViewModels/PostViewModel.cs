namespace Warhammer.Core.RoleplayViewModels
{
	public class PostViewModel
	{
		public int ID { get; set; }
		public int PlayerId { get; set; }
		public string PlayerName { get; set; }
		public int CharacterId { get; set; }
		public string CharacterName { get; set; }

		public int PostType { get; set; }
		public string DatePosted { get; set; }
		public bool IsPostedByGm { get; set; }

		public bool IsPrivate { get; set; }
		public string TargetPlayerNames { get; set; }

		public PostViewModel()
		{
			ID = -1;
			PlayerId = -1;
			PlayerName = string.Empty;
			CharacterId = -1;
			CharacterName = string.Empty;

			DatePosted = string.Empty;
			TargetPlayerNames = string.Empty;
		}
	}
}
