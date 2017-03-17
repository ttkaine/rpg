using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Concrete;

namespace Warhammer.Mvc.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly IAuthenticatedDataProvider _data;
        private IViewModelFactory _factory;


        protected bool CurrentPlayerIsGm
        {
            get { return _data.CurrentPlayerIsGm; }
        }

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

        protected IViewModelFactory ModelFactory
        {
            get
            {
                return _factory ??
                       (_factory = new ViewModelFactory(Url, _data));
            }
        }
    }
}