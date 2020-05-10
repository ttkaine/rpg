using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Warhammer.Core.Extensions;

namespace Warhammer.Core.Entities
{
    public partial class Comment
    {
        public bool IsPlayerComment
        {
            get { return !PersonId.HasValue; }
           
        }

        [AllowHtml] 
        public string FormattedContent => Description.ToDisplayFormat();

    }
}
