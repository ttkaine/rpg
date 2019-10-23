namespace Warhammer.Core.RoleplayViewModels
{
	public class CharacterViewModel
	{
		public int ID { get; set; }
		public int PlayerId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
        public string CharacterSheet { get; set; }
        public string ImageUrl { get; set; }

        public CharacterViewModel()
		{
			ID = -1;
			Name = string.Empty;
			Description = string.Empty;
		}
	}
}
