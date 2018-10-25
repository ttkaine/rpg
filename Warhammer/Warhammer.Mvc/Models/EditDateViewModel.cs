using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class EditDateViewModel
    {
        public int PageId { get; set; }
        public string Title { get; set; }
        public bool IsStartDate { get; set; }

        public GameDate DisplayDate;
        public string EditableDate { get; set; }
    }
}