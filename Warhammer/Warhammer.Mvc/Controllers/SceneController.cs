using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class SceneController : BaseController
    {
        // GET: Scene
        protected SceneController(IAuthenticatedDataProvider data) : base(data)
        {
        }

        public ActionResult Index()
        {
            List<Scene> scenes = DataProvider.MyScenes().ToList();
            return View();
        }
        
        
    }
}