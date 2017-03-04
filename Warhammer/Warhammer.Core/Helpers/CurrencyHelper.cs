using System;

namespace Warhammer.Core.Helpers
{
    public static class CurrencyHelper
    {
        public static int Crowns(int pennies)
        {
            return (int)Math.Floor((decimal)pennies / 240);
        }
        public static int Crowns(decimal pennies)
        {
            return (int)Math.Floor(pennies / 240);
        }
        public static int Shillings(decimal pennies)
        {
            return ((int)Math.Floor(pennies / 12)) - (Crowns(pennies) * 20);
        }

        public static int Pence(decimal pennies)
        {
            return ((int)Math.Floor(pennies)) - (Crowns(pennies) * 240) - (Shillings(pennies) * 12);
        }

        public static string DisplayString(decimal pennies)
        {
            bool isNegative = false;
            if (pennies < 0)
            {
                pennies = 0 - pennies;
                isNegative = true;
            }


            string display = "";
            if (Crowns(pennies) > 0)
            {
                display = $"{Crowns(pennies)}GC";
            }
            if (Shillings(pennies) > 0)
            {
                display = $"{display} {Shillings(pennies)}/";
                if (Pence(pennies) > 0)
                {
                    display = $"{display}{Pence(pennies)}";
                }
                else
                {
                    display = display + "-";
                }
            }
            else
            {
                if (Crowns(pennies) > 0 && Pence(pennies) > 0)
                {
                    display = $"{display} -/{Pence(pennies)}";
                }
                if (Pence(pennies) > 0)
                {
                    if (pennies - Pence(pennies) > 0)
                    {
                        if ((pennies - Pence(pennies)) == 0.25m)
                        {
                            display = $"{Pence(pennies)}&#188;d";
                        }
                        else if ((pennies - Pence(pennies)) == 0.5m)
                        {
                            display = $"{Pence(pennies)}&#189;d";
                        }
                        else if ((pennies - Pence(pennies)) == 0.75m)
                        {
                            display = $"{Pence(pennies)}&#190;d";
                        }
                        else
                        {
                            display = $"{(int)Math.Round(pennies, 0)}d";
                        }
                    }
                    else
                    {
                        display = $"{(int)Math.Round(pennies, 0)}d";
                    }

                }
            }

            if (isNegative)
            {
                return $"-{display}";
            }
            return display;
        }
    }
}