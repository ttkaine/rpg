using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Warhammer.Mvc.Models
{
    public class PersonDetailsViewModel
    {
        public bool ShowAny => ShowAge || ShowHeight || ShowMoney;

        public bool ShowMoney { get; set; }
        public bool ShowAge { get; set; }
        public bool ShowHeight { get; set; }

        public int Crowns { get; set; }
        public int Shillings { get; set; }
        public int Pennies { get; set; }

        public int TotalPennies { get; set; }
        public int Age { get; set; }
        public string DateOfBirthString { get; set; }
        public DateTime DateOfBirth { get; set; }

        [AllowHtml]
        public string Height { get; set; }

        [Required]
        public int PersonId { get; set; }

        public DateTime GameDate { get; set; }
    }
}