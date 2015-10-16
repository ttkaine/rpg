using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Warhammer.Core.RoleplayViewModels
{
	public class DiceRollPostViewModel : PostViewModel
	{
		public int DieSize { get; set; }
		public int DieCount { get; set; }
		public int RollType { get; set; }
		public int RollTarget { get; set; }
		public List<int> RollValues { get; set; }
		public bool ReRollMaximums { get; set; }

		public DiceRollPostViewModel()
		{
			RollValues = new List<int>();
		}

	}
}
