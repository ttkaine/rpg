using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class GameDateViewModel
    {
        public string Title { get; set; }
        public GameDate Date { get; set; }
    }
}