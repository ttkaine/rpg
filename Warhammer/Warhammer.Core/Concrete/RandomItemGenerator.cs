using System;
using Warhammer.Core.Abstract;
using Warhammer.Core.Models;

namespace Warhammer.Core.Concrete
{
    public class RandomItemGenerator : IRandomItemGenerator
    {
        private readonly Random _dice = new Random();
        private int Roll(int sides = 6, int count = 1)
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total = total + _dice.Next(1, sides + 1);
            }
            return total;
        }

        public RandomItemResult PersonAge()
        {
            int age = Roll(50) + Roll(6, 4);
            return new RandomItemResult { Name = "Age", Content = age.ToString()};
        }

        public RandomItemResult PersonSex()
        {
            int roll = Roll(2);
            string sex = roll == 1 ? "Male" : "Female";
            return new RandomItemResult { Name = "Sex", Content = sex };
        }

        public RandomItemResult PersonOrientation()
        {
            int roll = Roll(20);
            string orientation;
            switch (roll)
            {
                case 20:
                    orientation = "Gay";
                    break;
                case 19:
                    orientation = "Bisexual";
                    break;
                default:
                    orientation = "Straight";
                    break;
            }

            return new RandomItemResult { Name = "Sex", Content = $"{orientation} ({roll})" };
        }
    }
}