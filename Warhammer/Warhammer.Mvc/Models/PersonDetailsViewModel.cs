using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Configuration;
using System.Web.Mvc;

namespace Warhammer.Mvc.Models
{
    public class PersonDetailsViewModel
    {
        public bool HeightJustSet { get; set; }
        public bool AgeJustSet { get; set; }
        public bool MoneyJustSet { get; set; }

        public bool ShowAny => ShowAge || ShowHeight || ShowMoney;

        public bool ShowMoney { get; set; }
        public bool ShowAge { get; set; }
        public bool ShowHeight { get; set; }

        public int Crowns { get; set; }
        public int Shillings { get; set; }
        public int Pennies { get; set; }
        public int Upkeep { get; set; }
        public int TotalUpkeep { get; set; }

        public int TotalPennies { get; set; }
        public int Age { get; set; }
        public string DateOfBirthString { get; set; }
        public DateTime DateOfBirth { get; set; }

        [AllowHtml]
        public string Height { get; set; }

        [Required]
        public int PersonId { get; set; }

        public DateTime GameDate { get; set; }
        public bool AllowEdit { get; set; }

        public int SpendCrowns { get; set; }
        public int SpendShillings { get; set; }
        public int SpendPence { get; set; }
        public bool MoneySpent { get; set; }

        public int AddCrowns { get; set; }
        public int AddShillings { get; set; }
        public int AddPence { get; set; }
        public bool MoneyAdded { get; set; }

        public string MoneyDisplayString { get; set; }
    }
}