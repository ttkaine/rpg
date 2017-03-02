﻿using System;

namespace Warhammer.Core.Extensions
{
    public enum OrdinalOption
    {
        First,
        Second,
        Third,
        Fourth,
        Fifth
    }

    public enum DayName
    {
        Festag,
        Wellentag,
        Aubentag,
        Marktag,
        Backertag,
        Bezahltag,
        Konistag    
    }

    public enum MonthName
    {
        Nachhexen,
        Jahrdrung,
        Pflugzeit,
        Sigmarzeit,
        Sommerzeit,
        Vorgeheim,
        Nachgeheim,
        Erntzeit,
        Brauzeit,
        Kaldezeit,
        Ulriczeit,
        Vorhexen
    }

    public static class DateTimeExtensions
    {
        public static string ToWarhammerDateString(this DateTime date)
        {
            MonthName month = (MonthName)date.Month;
            DayName day = (DayName)date.DayOfWeek;
            OrdinalOption ordinal = (OrdinalOption)(date.Day / 7);
            string year = NumberToWords(date.Year);

            return $"{ordinal} {day} of {month} in the Imperial year of Sigmar {year}";
        }


        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
    }
}