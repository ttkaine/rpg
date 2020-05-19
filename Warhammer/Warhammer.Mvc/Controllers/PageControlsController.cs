using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Models;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Models;

namespace Warhammer.Mvc.Controllers
{
    public class PageControlsController : BaseController
    {
        private readonly IViewModelFactory _factory;

        private readonly ICharacterAttributeManager _attributeManager;
        // GET: PageControls
        public ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                if (IsEditMode)
                {
                    PageControlsViewModel model = GetModel(id.Value);

                    if (model != null)
                    {
                        return PartialView(model);
                    }
                }
            }
            return null;
        }

        private GmReadModePageControlsViewModel GetGmReadOnlyModel(int id)
        {
            Page page = DataProvider.GetPage(id);

            if (page != null)
            {
                List<PageLinkModel> sessionsToOffer = DataProvider.RecentSessionsToLink(id);

                GmReadModePageControlsViewModel model = new GmReadModePageControlsViewModel
                {
                    Id = id,
                    SessionLinksToOffer = sessionsToOffer
                };

                return model;
            }

            return null;
        }

        private PageControlsViewModel GetModel(int id)
        {
            Page page = DataProvider.GetPage(id);
            if (page != null)
            {
                bool playerIsGm = DataProvider.CurrentPlayerIsGm;
                int currentPlayerId = DataProvider.CurrentPlayerId;
                bool isAdmin = DataProvider.CurrentUserIsAdmin;               
                List<Player> players = DataProvider.GetAllPlayers();
                PageControlsViewModel model =
                    _factory.MakePageControlsViewModel(page, isAdmin, playerIsGm, currentPlayerId, players);
                return model;
            }
            return null;
        }

        public PageControlsController(IAuthenticatedDataProvider data, IViewModelFactory factory, ICharacterAttributeManager attributeManager) : base(data)
        {
            _factory = factory;
            _attributeManager = attributeManager;
        }

        public ActionResult KillPerson(int id)
        {
            Person person = DataProvider.GetPerson(id);
            if (person == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(person);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult KillPerson(Person person)
        {
            if (ModelState.IsValid)
            {
                if (person.IsDead)
                {
                    DataProvider.ResurrectPerson(person.Id);
                }
                else
                {
                    DataProvider.KillPerson(person.Id, person.Obiturary, person.CauseOfDeath);
                }
                return RedirectToAction("Index", "Page", new { id = person.Id });
            }
            return View(person);
        }

        [HttpPost]
        public ActionResult ResetPersonAttributes(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.PersonAttributes))
            {
                PageControlsViewModel model = GetModel(id);

                if (model != null)
                {
                    if (model.CanResetPersonAttributes)
                    {
                        _attributeManager.ResetAttributes(id);
                    }
                    return PartialView("Index", model);
                }


            }
            return null;
        }

        public ActionResult ManageAwards(int id)
        {
            PageControlsViewModel controlModel = GetModel(id);

            if (controlModel != null && controlModel.CanManageAwards)
            {

                Person person = DataProvider.GetPerson(id);

                if (person == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    List<Trophy> trophies = DataProvider.Trophies().ToList();
                    List<Session> sessions = person.Sessions.ToList();
                    ManageAwardsViewModel model = _factory.MakeManageAwardsViewModel(trophies, person, sessions);
                    
                    return View(model);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult ManageAwards(ManageAwardsViewModel model)
        {
            if (ModelState.IsValid)
            {
                PageControlsViewModel controlModel = GetModel(model.PersonId);

                if (controlModel != null && controlModel.CanManageAwards)
                {
                    if (model.SelectedTrophy.HasValue)
                    {
                        DataProvider.AwardTrophy(model.PersonId, model.SelectedTrophy.Value, model.Reason, null, model.SelectedSessionId);
                    }
                    if (model.Awards != null)
                    {
                        foreach (Award award in model.Awards)
                        {
                            if (award.Remove)
                            {
                                DataProvider.RemoveAward(award.PersonId, award.Id);
                            }
                            else
                            {
                                DataProvider.UpdateAward(award.Id, award.Reason, award.SessionId);
                            }

                        }
                    }
                    return RedirectToAction("ManageAwards", "PageControls", new {id = model.PersonId});
                }
                else
                {
                    return null;
                }
            }
            return RedirectToAction("ManageAwards", "PageControls", new { id = model.PersonId });
        }

        [HttpPost]
        public ActionResult ToggleTextSession(int? id)
        {
            if (id.HasValue)
            {
                PageControlsViewModel model = GetModel(id.Value);
                if (model != null && model.CanUseSessionControls)
                {
                    DataProvider.ToggleSetAsTextSession(id.Value);
                    model = GetModel(id.Value);
                    return PartialView("Index", model);
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult ToggleSessionPrivicy(int? id)
        {
            if (id.HasValue)
            {
                PageControlsViewModel model = GetModel(id.Value);
                if (model != null && model.CanUseSessionControls)
                {
                    DataProvider.ToggleSessionPrivacy(id.Value);
                    model = GetModel(id.Value);
                    return PartialView("Index", model);
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult OpenCloseTextSession(int? id)
        {
            if (id.HasValue)
            {
                PageControlsViewModel model = GetModel(id.Value);
                if (model != null && model.CanUseSessionControls)
                {
                    DataProvider.OpenOrCloseTextSession(id.Value);
                    model = GetModel(id.Value);
                    return PartialView("Index", model);
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult SetGm(int? id, int? selectedGm)
        {
            if (id.HasValue)
            {
                PageControlsViewModel model = GetModel(id.Value);
                if (model != null && model.CanUseSessionControls)
                {
                    DataProvider.SetSessionGm(id.Value, selectedGm);
                    model = GetModel(id.Value);
                    model.GmJustSet = true;
                    return PartialView("Index", model);
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult TogglePin(int? id)
        {
            if (id.HasValue)
            {
                PageControlsViewModel model = GetModel(id.Value);
                if (model != null && (model.CanPin || model.CanUnpin))
                {
                    DataProvider.TogglePagePin(id.Value);
                    model = GetModel(id.Value);
                    return PartialView("Index", model);
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult ApplyShift(int? id)
        {
            if (id.HasValue)
            {
                PageControlsViewModel model = GetModel(id.Value);
                if (model != null && (model.CanApplyShift))
                {
                    DataProvider.ApplyShift(id.Value);
                    model = GetModel(id.Value);
                    model.ShiftJustApplied = true;
                    return PartialView("Index", model);
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult LinkToSession(int id, int sessionid)
        {
            if (CurrentPlayerIsGm)
            {
                DataProvider.AddLink(id, sessionid);
                GmReadModePageControlsViewModel gmModel = GetGmReadOnlyModel(id);
                if (gmModel != null)
                {
                    return PartialView("GmReadOnlyPageControls", gmModel);
                }
            }
            return null;
        }

        public ActionResult GmReadOnlyPageControls(int? id)
        {
            if (id.HasValue && CurrentPlayerIsGm)
            {
                GmReadModePageControlsViewModel gmModel = GetGmReadOnlyModel(id.Value);
                if (gmModel != null)
                {
                    return PartialView("GmReadOnlyPageControls", gmModel);
                }
            }
            return null;
        }

        [HttpGet]
        public ActionResult ManagePersonAttributes(int id)
        {
            PageControlsViewModel controlModel = GetModel(id);
            if (controlModel.CanManagePersonAttributes || DataProvider.CurrentUserIsAdmin)
            {
                ManagePersonAttributesViewModel model = _factory.MakeManagePersonAttributesViewModel(id);
                return View(model);
            }

            return RedirectToAction("Index", "Page", new { id });
        }


        [HttpPost]
        public ActionResult ManagePersonAttributes(ManagePersonAttributesViewModel model)
        {
            if (ModelState.IsValid)
            {
                PageControlsViewModel controlModel = GetModel(model.PersonId);

                if (controlModel != null && (controlModel.CanManagePersonAttributes || DataProvider.CurrentUserIsAdmin))
                {
                    if (model.NewAttributeIsPopulated && model.NewAttributeType.HasValue)
                    {
                        DataProvider.AddPersonAttribute(model.PersonId, model.NewAttributeType.Value, model.NewAttributeName, model.NewAttributeDescription, model.NewAttributeValue, model.NewAttributeIsPrivate);
                    }
                    if (model.Attributes != null)
                    {
                        foreach (PersonAttribute attribute in model.Attributes)
                        {
                            if (attribute.Remove)
                            {
                                DataProvider.RemovePersonAttribute(attribute.PersonId, attribute.Id);
                            }
                            else
                            {
                                DataProvider.UpdatePersonAttribute(attribute.Id, attribute.CurrentValue, attribute.Name, attribute.Description, attribute.AttributeType, attribute.IsPrivate);
                            }

                        }
                    }
                    return RedirectToAction("ManagePersonAttributes", "PageControls", new { id = model.PersonId });
                }
                else
                {
                    return null;
                }
            }
            return RedirectToAction("ManagePersonAttributes", "PageControls", new { id = model.PersonId });
        }
    }
}