using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly IAuthenticatedDataProvider _data;

        protected bool IsEditMode
        {
            get { return ViewBag.EditMode; }
        }

        protected IAuthenticatedDataProvider DataProvider
        {
            get { return _data; }
        }

        protected BaseController(IAuthenticatedDataProvider data)
        {
            _data = data;
        }

        protected Player CurrentPlayer
        {
            get { return _data.MyPlayer(); }
        }
    }
}