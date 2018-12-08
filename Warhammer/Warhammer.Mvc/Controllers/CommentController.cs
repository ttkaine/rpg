using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class CommentController : BaseController
    {
        // GET: Comment
        public ActionResult Index(int? id)
        {
            CommentListViewModel model = CommentListViewModel(id);

            return PartialView(model);
        }

        private CommentListViewModel CommentListViewModel(int? id)
        {
            CommentListViewModel model = new CommentListViewModel {PageId = id};

            if (id.HasValue)
            {
                model.Comments = DataProvider.GetCommentsForPage(id.Value).OrderBy(p => p.Created).ToList();

                Player player = CurrentPlayer;
                List<PageLinkModel> people;
                if (CurrentPlayerIsGm)
                {
                    people = DataProvider.AllNpcLinks().OrderBy(p => p.FullName).ToList();
                }
                else
                {
                    people = DataProvider.MyPeople().OrderBy(p => p.FullName).ToList();
                }
                model.PlayerName = player.DisplayName;
                model.PostAs = new SelectList(people, "Id", "FullName");
            }
            else
            {
                model.Comments = DataProvider.RecentComments();
            }
            return model;
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult EditComment(int? pageId, int commentId, string comment)
        {
            DataProvider.EditComment(commentId, comment);
            ModelState.Clear();
            CommentListViewModel fullModel = CommentListViewModel(pageId);
            return PartialView("Index", fullModel);
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        //    [ValidateInput(false)]
        public ActionResult PostComment(CommentListViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (model.PageId.HasValue)
                {
                    if (model.SelectedPerson.HasValue)
                    {
                        DataProvider.AddComment(model.PageId.Value, model.AddedComment, model.SelectedPerson.Value);
                    }
                    else
                    {
                        DataProvider.AddComment(model.PageId.Value, model.AddedComment);
                    }        
                }
            }
            ModelState.Clear();
            CommentListViewModel fullModel = CommentListViewModel(model.PageId);
            return PartialView("Index", fullModel);
        }

        public CommentController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        [HttpPost]
        [Authorize(Roles = "Player")]
        public ActionResult Delete(int commentId, int? pageId)
        {
            DataProvider.DeleteComment(commentId);
            ModelState.Clear();

            CommentListViewModel fullModel = CommentListViewModel(pageId);
            return PartialView("Index", fullModel); ;
        }
    }
}