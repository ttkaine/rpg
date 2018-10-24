using System;
using System.Collections.Generic;
using System.Globalization;
using Warhammer.Core.Entities;

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

    public enum ImperialDayName
    {
        Festag,
        Wellentag,
        Aubentag,
        Marktag,
        Backertag,
        Bezahltag,
        Konistag    
    }

    public enum ImperialMonthName
    {
        Nachhexen = 1,
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
            ImperialMonthName month = (ImperialMonthName)date.Month;
            ImperialDayName day = (ImperialDayName)date.DayOfWeek;
            OrdinalOption ordinal = (OrdinalOption)(date.Day / 7);
            string year = NumberToWords(date.Year);

            return $"{ordinal} {day} of {month} in the Imperial year of Sigmar {year}";
        }

        public static string ToWarhammerDateString(this GameDate date)
        {
            ImperialMonthName month = (ImperialMonthName)date.Month;
            ImperialDayName day = (ImperialDayName)date.DayOfWeek();
            OrdinalOption ordinal = (OrdinalOption)(date.Day / 7);

            if (date.Year > 0)
            {
                string year = NumberToWords(date.Year);
                return $"{ordinal} {day} of {month} in the Imperial year of Sigmar {year}";
            }
            else
            {
                string year = NumberToWords(-date.Year);
                return $"{ordinal} {day} of {month} in the Pre-Imperial year {year}";
            }
        }

        public static string ToRealDateString(this GameDate date)
        {
            if (date.Month < 1)
            {
                date.Month = 1;
            }
            if (date.Month > 12)
            {
                date.Month = 12;
            }
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[date.Month - 1];
            string dayName = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[date.DayOfWeek()];
            
            if (date.Year > 0)
            {
                return $"{dayName} {date.Day} {monthName} {date.Year}";
            }
            else
            {
                return $"{dayName} {date.Day} {monthName} {-date.Year} BCE";
            }
        }

        public static int DayOfWeek(this GameDate date)
        {
            int years = (date.Year < 0 ? -date.Year : date.Year) - 1;
            int leapYearRem;
            Math.DivRem(date.Year < 0 ? -date.Year : date.Year, 4, out leapYearRem);

            int dayOfYear = ((date.Month - 1)*31) + (date.Day - 1);
            if (date.Month > 2)
            {
                dayOfYear -= 2;
                if (leapYearRem != 0)
                {
                    dayOfYear--;
                }
            }
            if (date.Month > 4)
            {
                dayOfYear--;
            }
            if (date.Month > 6)
            {
                dayOfYear--;
            }
            if (date.Month > 9)
            {
                dayOfYear--;
            }
            if (date.Month > 11)
            {
                dayOfYear--;
            }            
            if (date.Year < 0)
            {
                dayOfYear = (leapYearRem == 0 ? 365 - dayOfYear : 364 - dayOfYear);
            }

            int totalDays = (years*365) + (years / 4) + dayOfYear;

            int dayOfWeek;
            Math.DivRem(totalDays, 7, out dayOfWeek);

            if (date.Year < 1)
            {
                dayOfWeek = 6 - dayOfWeek;
            }

            return dayOfWeek;
        }

        public static GameDate ToWarhammerGameDate(this string date)
        {
            string[] dateParts = date.Split(new string[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
            if (dateParts.Length == 3)
            {
                int year;
                int month;
                int day;
                if (!int.TryParse(dateParts[0], out year))
                {
                    return null;
                }
                if (!int.TryParse(dateParts[1], out month))
                {
                    return null;
                }
                if (!int.TryParse(dateParts[2], out day))
                {
                    return null;
                }
                if (year == 0)
                {
                    year = 1;
                }
                if (month < 1)
                {
                    month = 1;
                }
                if (month > 12)
                {
                    month = 12;
                }
                if (day < 1)
                {
                    day = 1;
                }
                if (day > 31)
                {
                    day = 31;
                }
                if (month == 2 && day > 29)
                {
                    day = 28;
                }
                if ((month == 4 || month == 6 || month == 9 || month == 11) && day > 30)
                {
                    day = 30;
                }

                return new GameDate() {Year = year, Month = month, Day = day};
            }
            return null;
        }

        public static string ToShortDateString(this GameDate date, bool editorFormat = false)
        {
            string day = date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString();
            string month = date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString();

            if (editorFormat)
            {
                return $"{date.Year}/{month}/{day}";
            }
            else
            {
                return $"{day}/{month}/{date.Year}";
            }
            
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