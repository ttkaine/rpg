using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Warhammer.Core.RoleplayViewModels
{
	public class PlayerViewModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public bool IsGM { get; set; }

		public PlayerViewModel()
		{
			ID = -1;
			Name = string.Empty;
		}
	}
}
