using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Warhammer.Core.RoleplayViewModels
{
	public class CharacterViewModel
	{
		public int ID { get; set; }
		public int PlayerId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public byte[] Image { get; set; }
		public string CharacterSheet { get; set; }

		public CharacterViewModel()
		{
			ID = -1;
			Name = string.Empty;
			Description = string.Empty;
		}
	}
}
