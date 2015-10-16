using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warhammer.Core.Tools
{
	public class DiceRoller
	{
		private Random RandomNumberGenerator { get; set; }

		private static readonly DiceRoller _instance = new DiceRoller();

		static DiceRoller()
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
		}

		public DiceRoller()
		{
			RandomNumberGenerator = new Random();
		}

		public static DiceRoller Instance
		{
			get
			{
				return _instance;
			}
		}

		public List<int> RollDice(int dieSize, int dieCount, int rollType, int rollTarget, bool reRollMaximums)
		{
			List<int> rolls = new List<int>();
			for (int i = 0; i < dieCount; i++)
			{
				int roll = RandomNumberGenerator.Next(dieSize) + 1;
				rolls.Add(roll);
				if (reRollMaximums)
				{
					while (roll == dieSize)
					{
						roll = RandomNumberGenerator.Next(dieSize) + 1;
						rolls.Add(roll);
					}
				}
			}

			return rolls;
		}
	}
}

