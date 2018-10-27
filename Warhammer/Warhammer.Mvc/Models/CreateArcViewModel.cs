using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class CreateArcViewModel
    {
        public Arc Arc { get; set; }
        public string StartDate { get; set; }
        public string CurrentDate { get; set; }
    }
}