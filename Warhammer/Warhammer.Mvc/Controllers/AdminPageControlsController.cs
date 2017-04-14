using System.Collections.Generic;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class AdminPageControlsController : BaseController
    {
        private ICharacterAttributeManager _attributeManager;

        public AdminPageControlsController(IAuthenticatedDataProvider data, ICharacterAttributeManager attributeManager) : base(data)
        {
            _attributeManager = attributeManager;
        }

        public ActionResult Index(int? id)
        {
            if (id.HasValue && User.IsInRole("Admin"))
            {
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView(page);
                }
            }
            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ToggleTextSession(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.ToggleSetAsTextSession(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult OpenCloseTextSession(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.OpenOrCloseTextSession(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult PinPage(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.PinPage(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ToggleSessionPrivicy(int? id)
        {
            if (id.HasValue)
            {
                DataProvider.ToggleSessionPrivacy(id.Value);
                Page page = DataProvider.GetPage(id.Value);
                if (page != null && IsEditMode)
                {
                    return PartialView("Index", page);
                }
            }

            return null;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SessionXpControl(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.SimpleStats))
            {
                Page page = DataProvider.GetPage(id);
                SessionXpModel model = new SessionXpModel
                {
                    SessionId = id,
                    XpToAward = 1
                };
                return PartialView(model);
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SessionXpControl(SessionXpModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.SimpleStats))
            {
                if (ModelState.IsValid)
                {
                    DataProvider.AddXpForSession(model.SessionId, model.XpToAward);

                    int id = model.SessionId;

                    ModelState.Clear();
                    SessionXpModel updatedModel = new SessionXpModel
                    {
                        SessionId = id,
                        XpToAward = 1,
                        XpAwarded = model.XpToAward
                    };

                    return PartialView(updatedModel);
                } 
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ResetNpcStats(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.SimpleStats))
            {
                DataProvider.ResetNpcStats(id);
            }
            return null;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ResetNpcPersonAttributes(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                _attributeManager.ResetAttributes(id);
            }
            return null;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SessionGmControl(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.VariableSessionGm))
            {
                List<Player> players = DataProvider.GetAllPlayers();
                int gmId = DataProvider.GetGmId(id);
                SessionGmViewModel model = ModelFactory.MakeSessionGmViewModel(id, players, gmId);
                return PartialView(model);
            }
            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SessionGmControl(SessionGmViewModel postedModel)
        {
            if (DataProvider.SiteHasFeature(Feature.VariableSessionGm))
            {
                DataProvider.SetSessionGm(postedModel.SessionId, postedModel.SelectedGm);

                List<Player> players = DataProvider.GetAllPlayers();
                int gmId = DataProvider.GetGmId(postedModel.SessionId);
                SessionGmViewModel model = ModelFactory.MakeSessionGmViewModel(postedModel.SessionId, players, gmId);
                model.GmJustSet = true;
                return PartialView(model);
            }
            return null;
        }
    }
}