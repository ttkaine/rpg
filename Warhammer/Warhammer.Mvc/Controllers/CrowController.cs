using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Core.Models.Crow;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class CrowController : BaseController
    {
        private readonly IViewModelFactory _factory;
        private readonly ICrowCharacterManager _characterManager;

        public CrowController(IAuthenticatedDataProvider data, IViewModelFactory factory, ICrowCharacterManager characterManager) : base(data)
        {
            _factory = factory;
            _characterManager = characterManager;
        }

        // GET: Crow
        public ActionResult TopStats(int id)
        { 

            if (DataProvider.SiteHasFeature(Feature.CrowTopStatsPanel) && DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                CrowCharacterAttributesModel model = _characterManager.GetCharacterAttributes(id);
                return PartialView(model);
            }
            return null;

        }

        public ActionResult CharacterGeneration(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.CharacterGeneration) && DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                CrowCharacterAttributesModel model = _characterManager.GetCharacterAttributes(id);
                return PartialView(model);
            }
            return null;
        }

    }
}