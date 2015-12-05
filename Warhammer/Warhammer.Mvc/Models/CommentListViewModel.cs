using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Models
{
    public class CommentListViewModel
    {
        public int? PageId { get; set; }
        public string PlayerName { get; set; }

        public bool ShowCommentAddPanel
        {
            get { return PageId.HasValue; }
        }

        [DisplayName("Post As:")]
        public int? SelectedPerson { get; set; }

        public List<Comment> Comments { get; set; }
        public SelectList PostAs { get; set; }

        [AllowHtml]
        [DisplayName("Comment:")]
        public string AddedComment { get; set; }
    }
}