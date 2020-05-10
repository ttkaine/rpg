﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNet.SignalR;
using Warhammer.Core;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Core.Extensions;
using Warhammer.Core.Models;
using Warhammer.Mvc.Abstract;
using Warhammer.Mvc.Concrete;
using Warhammer.Mvc.Models;
using Page = Warhammer.Core.Entities.Page;

namespace Warhammer.Mvc.Controllers
{
    public class PageController : BaseController
    {
        private readonly IImageProcessor _imageProcessor;
        private readonly ILinkGenerator _linkGenerator;

        // GET: Page
        public PageController(IAuthenticatedDataProvider data, IImageProcessor imageProcessor, ILinkGenerator linkGenerator) : base(data)
        {
            _imageProcessor = imageProcessor;
            _linkGenerator = linkGenerator;
        }

        public ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                Page page = DataProvider.GetPage(id.Value);
                if (page != null)
                {
                    DataProvider.MarkAsSeen(page.Id);
                    if (!IsEditMode)
                    {
                        page.Description = _linkGenerator.CreoleLinksToHtml(page.Description);
                    }

                    if (page is Session)
                    {
                        Session session = page as Session;
                        ViewBag.ShowTextSessionLink =
                            session.IsTextSession
                            && (
                                !session.IsPrivate
                                ||
                                session.PlayerCharacters.Any(c => c.PlayerId == CurrentPlayer.Id)
                                ||
                                CurrentPlayerIsGm
                                );
                    }

                    page.PlayerIsGm = CurrentPlayerIsGm;
                    if (DataProvider.SiteHasFeature(Feature.GmNotes))
                    {
                        page.ShowGmNotes = CurrentPlayerIsGm && page.CampaignId == DataProvider.CurrentCampaignId;
                    }

                    if (DataProvider.SiteHasFeature(Feature.PlayerSecrets))
                    {
                        page.ShowPlayerSecrets = true;
                        page.CurrentPlayerId = CurrentPlayer.Id;
                        page.PlayerIsGm = CurrentPlayerIsGm;
                    }
                    return View(page);
                }
            }

            return RedirectToAction("Index", "Home");
        }



        [HttpPost, ValidateInput(false)]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult Index(Page page)
        {
            if (ModelState.IsValid)
            {
                List<ExtractedImage> images = _imageProcessor.GetImagesFromHtmlString(page.Description);

                foreach (ExtractedImage image in images)
                {
                    byte[] imageData = _imageProcessor.GetJpegFromImage(image.Image);
                    PageImage pageImage = DataProvider.SaveImage(page.Id, imageData);
                    string linkUrl =
                        $"{DataProvider.ImageUrlBase}{pageImage.FileIdentifier}"; //Url.Action("ShowImage", "Home", new {id = pageImage.Id});

                    int width = image.Image.Width / 5;
                    if (width > 100)
                    {
                        width = 100;
                    }

                    page.Description = page.Description.Replace(image.OriginalSrc, $"src='{linkUrl}' width='{width}%'");
                }

                foreach (PlayerSecret playerSecret in page.VisibleSecrets)
                {
                    DataProvider.UpdatePlayerSecret(playerSecret.PlayerId, playerSecret.PageId, playerSecret.Details);
                }

                Page updatedPage = DataProvider.UpdatePageDetails(page.Id, page.ShortName, page.FullName,
                    _linkGenerator.ResolveCreoleLinks(page.Description));

                if (CurrentPlayerIsGm)
                {
                    if (page.GmNotes != null)
                    {
                        List<ExtractedImage> notesImages = _imageProcessor.GetImagesFromHtmlString(page.GmNotes);

                        foreach (ExtractedImage image in notesImages)
                        {
                            byte[] imageData = _imageProcessor.GetJpegFromImage(image.Image);
                            PageImage pageImage = DataProvider.SaveImage(page.Id, imageData);
                            string linkUrl =
                                $"{DataProvider.ImageUrlBase}{pageImage.FileIdentifier}"; //Url.Action("ShowImage", "Home", new {id = pageImage.Id});
                            page.GmNotes = page.GmNotes.Replace(image.OriginalSrc, $"src='{linkUrl}'");
                        }

                        DataProvider.SaveGmNotes(page.Id, page.GmNotes);
                    }
                }

                if (!updatedPage.HasImage)
                {
                    ExtractedImage primaryImage = images.FirstOrDefault();
                    Image image = primaryImage?.Image;
                    if (image != null)
                    {
                        image = _imageProcessor.ResizeImage(image, new Size {Height = 200, Width = 200});
                        byte[] imageData = _imageProcessor.GetJpegFromImage(image);

                        DataProvider.ChangePicture(updatedPage.Id, imageData, "Image/Jpeg");
                    }
                }

                updatedPage.PlayerIsGm = CurrentPlayerIsGm;

                if (DataProvider.SiteHasFeature(Feature.GmNotes))
                {
                    updatedPage.ShowGmNotes = CurrentPlayerIsGm;
                }

              //  return View(updatedPage);
            }

            return RedirectToAction("index", new {id = page.Id});
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult DeleteLink(int id, int linkToDeleteId)
        {
            if (ModelState.IsValid)
            {
                DataProvider.RemoveLink(id, linkToDeleteId);
                return RedirectToAction("EditLinks", new {id = id});
            }
            return RedirectToAction("index", new {id = id});
        }


        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult ChangeImage(int? id)
        {
            if (id.HasValue)
            {
                Page page = DataProvider.GetPage(id.Value);
                if (page != null)
                {
                    return View(page);
                }
            }


            return RedirectToAction("index", "home");
        }

        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult MangePeople(int? id)
        {
            if (DataProvider.SiteHasFeature(Feature.ManageSessionPeople))
            {
                if (id.HasValue)
                {
                    Page page = DataProvider.GetPage(id.Value);
                    if (page != null)
                    {
                        ManageSessionPeopleViewModel model = new ManageSessionPeopleViewModel();
                        List<PageToggleModel> people = DataProvider.GetAllPeopleForSession(id.Value);
                        model.People = people.OrderByDescending(p => p.Selected).ThenBy(p => p.FullName).ToList();
                        model.SessionId = id.Value;
                        model.SessionName = page.FullName;
                        return View(model);
                    }
                }
            }

            return RedirectToAction("index", "home");           
        }

        [System.Web.Mvc.Authorize(Roles = "Player")]
        [HttpPost]
        public ActionResult MangePeople(ManageSessionPeopleViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.ManageSessionPeople))
            {
                if (ModelState.IsValid)
                {
                    foreach (var pageToggleModel in model.People.Where(p => p.InitialState != p.Selected))
                    {
                        if (pageToggleModel.Selected)
                        {
                            DataProvider.AddLink(model.SessionId, pageToggleModel.PageId);
                        }
                        else
                        {
                            DataProvider.RemoveLink(model.SessionId, pageToggleModel.PageId);
                        }
                    }

                    return RedirectToAction("Index", "Page", new {id = model.SessionId});
                }
            }

            return RedirectToAction("index", "home");
        }

        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult EditLinks(int? id)
        {
            if (id.HasValue)
            {
                Page page = DataProvider.GetPage(id.Value);
                if (page != null)
                {
                    EditLinksViewModel model = new EditLinksViewModel
                    {
                        Page = page,
                        CurrentLinks = page.Related.ToList(),
                        LinkToList = new SelectList(DataProvider.PossibleLinks(page.Id), "Id", "ShortName")
                    };


                    return View(model);
                }
            }
            return RedirectToAction("index", "home");
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult EditLinks(EditLinksViewModel model)
        {
            if (ModelState.IsValid)
            {
                Page page = DataProvider.GetPage(model.Page.Id);

                if (page != null)
                {
                    DataProvider.AddLink(model.Page.Id, model.AddLinkTo);
                    model.Page = page;
                    model.CurrentLinks = page.Related.ToList();
                    model.LinkToList = new SelectList(DataProvider.PossibleLinks(page.Id), "Id", "ShortName");
                    return View(model);
                }
            }
            return RedirectToAction("index", "home");
        }


        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        [ValidateInput(false)]
        public ActionResult ChangeImage(string saveAction, int id, HttpPostedFileBase profileImageFile, double? y1, double? x1, double? h, double? w)
        {
            if (ModelState.IsValid)
            {
                if (saveAction == "Save")
                {
                    Rectangle cropArea = GetCropArea(y1, x1, h, w);
                    if (profileImageFile != null)
                    {
                        Image theImage = System.Drawing.Image.FromStream(profileImageFile.InputStream, true, true);
                        Image croppedImage = _imageProcessor.Crop(theImage, cropArea);
                        croppedImage = _imageProcessor.ResizeImage(croppedImage, new Size {Height = 200, Width = 200});
                        byte[] imageData = _imageProcessor.GetJpegFromImage(croppedImage);

                        DataProvider.ChangePicture(id, imageData, "Image/Jpeg");
                        return RedirectToAction("Index", new {id = id});
                    }
                }
                if (saveAction == "Remove Image")
                {
                    DataProvider.RemoveProfileImage(id);
                    return RedirectToAction("Index", new {id = id});
                }
            }
            // there is something wrong with the data values
            Page model = DataProvider.GetPage(id);
            return View(model);
        }

        private Rectangle GetCropArea(double? y1, double? x1, double? h, double? w)
        {
            if (y1.HasValue && x1.HasValue && h.HasValue && w.HasValue)
            {
                return new Rectangle((int) x1.Value, (int) y1.Value, (int) w.Value, (int) h.Value);
            }

            return new Rectangle();
        }

        public ActionResult PlayerSessionControls(int? id)
        {
            if (id.HasValue)
            {
                Page page = DataProvider.GetPage(id.Value);
                if (page is Session)
                {
                    int currentGmId = DataProvider.GetGmId(page.Id);
                    bool currentPlayerIsSessionGm = currentGmId == CurrentPlayer.Id;
                    PlayerSessionControlsViewModel model = ModelFactory.MakePlayerSessionControlsViewModel((Session) page, CurrentPlayer, currentPlayerIsSessionGm);
                    return PartialView(model);
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult UnSuspendPlayer(int playerId, int sessionId)
        {
            Page page = DataProvider.GetPage(sessionId);
            if (page is Session)
            {
                int currentGmId = DataProvider.GetGmId(sessionId);
                bool currentPlayerIsSessionGm = currentGmId == CurrentPlayer.Id;

                if (playerId == CurrentPlayer.Id || currentPlayerIsSessionGm)
                {
                    if (playerId == CurrentPlayer.Id && currentPlayerIsSessionGm)
                    {
                        DataProvider.SetGmSuspended(page.Id, false);
                    }
                    else
                    {
                        DataProvider.SetPlayerSuspended(page.Id, playerId, false);
                    }
                    UpdateRoleplayHub();
                }

                PlayerSessionControlsViewModel model = ModelFactory.MakePlayerSessionControlsViewModel((Session) page, CurrentPlayer, currentPlayerIsSessionGm);
                return PartialView("PlayerSessionControls", model);
            }
            return null;
        }

        [HttpPost]
        public ActionResult SuspendPlayer(int playerId, int sessionId)
        {
            Page page = DataProvider.GetPage(sessionId);
            if (page is Session)
            {
                int currentGmId = DataProvider.GetGmId(sessionId);
                bool currentPlayerIsSessionGm = currentGmId == CurrentPlayer.Id;

                if (playerId == CurrentPlayer.Id || currentPlayerIsSessionGm)
                {
                    if (playerId == CurrentPlayer.Id && currentPlayerIsSessionGm)
                    {
                        DataProvider.SetGmSuspended(page.Id, true);
                    }
                    else
                    {
                        DataProvider.SetPlayerSuspended(page.Id, playerId, true);
                    }
                    UpdateRoleplayHub();
                }

                PlayerSessionControlsViewModel model = ModelFactory.MakePlayerSessionControlsViewModel((Session) page, CurrentPlayer, currentPlayerIsSessionGm);
                return PartialView("PlayerSessionControls", model);
            }
            return null;
        }

        private void UpdateRoleplayHub()
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<RoleplayHub>();
            hubContext.Clients.All.updateSession();
            hubContext.Clients.All.updateTextSessionsOnHomePage();

        }

        public ActionResult RelatedLinksPanel(int id)
        {
            return PartialView(id);
        }

        public ActionResult RelatedLinks(int id)
        {
            Page page = DataProvider.GetPage(id, true);
            List<PageLinkModel> linkedPages = DataProvider.GetRelatedPages(id);
            List<PageLinkModel> sessionLogs = DataProvider.SessionLogs(id);
            return PartialView(new RelatedLinksModel(linkedPages, page, sessionLogs));
        }

        public ActionResult GetShowLocalButton(int id)
        {
            if (!DataProvider.IsMasterDomain)
            {
                return null;
            }
            CampaignDetail campaignDetail = DataProvider.GetCampaginDetailsForPage(id);

            if (campaignDetail == null)
            {
                return null;
            }

            if (campaignDetail.CampaignId == DataProvider.CurrentCampaignId)
            {
                return null;
            }

            ExternalLinkModel model = new ExternalLinkModel
            {
                Url = $"https://{campaignDetail.Url}/page/index/{id}",
                TypeOfLink = ExternalLinkModel.LinkType.InfoButton,
                Text = $"Show in {campaignDetail.DisplayName}"
            };
            return PartialView(model);
        }


        public ActionResult ArcSessions(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.SessionArcs))
            {
                Arc arc = DataProvider.GetArc(id);
                if (arc != null)
                {
                    ArcSessionsViewModel model = ModelFactory.MakeArcSessionsViewModel(arc);
                    foreach (SessionListItemViewModel session in model.Sessions)
                    {
                        session.LogButtonPerson =
                            session.People.FirstOrDefault(
                                p =>
                                    p.PlayerId.HasValue && p.SessionLogs.All(l => l.SessionId != session.Id) &&
                                    p.Player.UserName == User.Identity.Name);
                    }

                    return PartialView(model);
                }
            }
            return null;
        }

        public ActionResult GameDate(int id, bool isStartDate = false)
        {
            if (DataProvider.SiteHasFeature(Feature.ShowGameDate))
            {
                Page page = DataProvider.GetPage(id);
                if ((page != null && (page is Session || page is Arc)) || id == 0)
                {
                    GameDateViewModel model = new GameDateViewModel();

                    if (page != null)
                    {
                        if (page is Session)
                        {
                            model.Title = "Session Date";
                            model.Date = ((Session) page).GameDate;
                        }
                        if (page is Arc)
                        {
                            if (isStartDate)
                            {
                                model.Title = "Arc Start Date";
                                model.Date = ((Arc) page).StartGameDate;
                            }
                            else
                            {
                                model.Title = "Current Arc Date";
                                model.Date = ((Arc) page).CurrentGameDate;
                            }
                        }
                    }
                    else
                    {
                        model.Title = "Current Game Date";
                    }

                    if (model.Date == null)
                    {
                        CampaignDetail campaign = DataProvider.GetCampaginDetails();
                        DateTime date = campaign.CurrentGameDate ?? new DateTime(2520, 09, 01);
                        model.Date = new GameDate()
                        {
                            Year = date.Year,
                            Month = date.Month,
                            Day = date.Day,
                            Comment = "From Campaign Date"
                        };
                    }

                    if (DataProvider.SiteHasFeature(Feature.WarhammerDate))
                    {
                        model.UseWarhammerDate = true;
                    }

                    return PartialView(model);
                }
            }
            return null;
        }

        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult EditGameDate(int id, bool isStartDate = false)
        {
            if (DataProvider.SiteHasFeature(Feature.ShowGameDate))
            {
                Page page = DataProvider.GetPage(id);
                if (page != null && (page is Session || page is Arc))
                {
                    EditDateViewModel model = new EditDateViewModel();
                    model.PageId = page.Id;
                    model.IsStartDate = isStartDate;

                    if (page is Session)
                    {
                        model.Title = "Session Date";
                        model.DisplayDate = ((Session) page).GameDate;
                    }

                    if (page is Arc)
                    {
                        if (isStartDate)
                        {
                            model.Title = "Arc Start Date";
                            model.DisplayDate = ((Arc) page).StartGameDate;
                        }
                        else
                        {
                            model.Title = "Current Arc Date";
                            model.DisplayDate = ((Arc) page).CurrentGameDate;
                        }
                    }

                    if (model.DisplayDate == null)
                    {
                        CampaignDetail campaign = DataProvider.GetCampaginDetails();
                        DateTime date = campaign.CurrentGameDate ?? new DateTime(2520, 09, 01);
                        model.DisplayDate = new GameDate()
                        {
                            Year = date.Year,
                            Month = date.Month,
                            Day = date.Day,
                            Comment = "From Campaign Date"
                        };
                    }

                    model.IsWarhammerDate = DataProvider.SiteHasFeature(Feature.WarhammerDate);
                    model.EditableDate = model.DisplayDate.ToShortDateString(true);

                    return PartialView(model);
                }
            }

            return null;
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult UpdateGameDate(EditDateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.AddDay)
                {
                    DataProvider.AddDayToDate(model.PageId, model.IsStartDate);    
                }
                else if (model.AddMonth)
                {
                    DataProvider.AddMonthToDate(model.PageId, model.IsStartDate);
                }
                else if (model.AddWeek)
                {
                    DataProvider.AddWeekToDate(model.PageId, model.IsStartDate);
                }
                else
                {
                    DataProvider.SaveDate(model.PageId, model.EditableDate.ToWarhammerGameDate(), model.IsStartDate);
                }
            }

            return RedirectToAction("EditGameDate", new { id = model.PageId, isStartDate = model.IsStartDate });
        }

        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult EditArcSessions(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.SessionArcs))
            {
                if (IsEditMode)
                {
                    Arc arc = DataProvider.GetArc(id);
                    if (arc != null)
                    {
                        List<Session> sessions = DataProvider.AllSessions();
                        EditArcSessionsViewModel model = ModelFactory.MakeEditArcSessionsViewModel(arc, sessions);

                        return View(model);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult EditArcSessions(EditArcSessionsViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.SessionArcs))
            {
                if (IsEditMode)
                {
                    if (ModelState.IsValid)
                    {
                        Arc arc = DataProvider.GetArc(model.ArcId);
                        Page sessionToAdd = DataProvider.GetPage(model.AddSessionId ?? 0);
                        if (arc != null && sessionToAdd != null && sessionToAdd is Session &&
                            arc.Sessions.All(s => s.Id != sessionToAdd.Id))
                        {
                            DataProvider.SetSessionArc(arc.Id, sessionToAdd.Id);
                        }
                    }

                    return RedirectToAction("EditArcSessions", new {id = model.ArcId});
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult RemoveSessionFromArc(int id, int sessionId)
        {
            if (DataProvider.SiteHasFeature(Feature.SessionArcs))
            {
                if (IsEditMode)
                {
                    Page session = DataProvider.GetPage(sessionId);
                    if (session is Session && session.Id == sessionId)
                    {
                        DataProvider.SetSessionArc(null, session.Id);
                    }

                    return RedirectToAction("EditArcSessions", new {id});
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult EditSessionArc(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.SessionArcs))
            {
                Page session = DataProvider.GetPage(id);
                if (session != null && session is Session)
                {
                    List<Arc> arcs = DataProvider.Arcs().ToList();
                    EditSessionArcViewModel model = ModelFactory.MakeEditSessionArcViewModel((Session) session, arcs);

                    return PartialView(model);
                }
            }
            return null;
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult UpdateSessionArc(EditSessionArcViewModel model)
        {
            if (DataProvider.SiteHasFeature(Feature.SessionArcs))
            {
                if (IsEditMode && ModelState.IsValid)
                {
                    Page session = DataProvider.GetPage(model.SessionId);
                    if (session != null && session is Session)
                    {
                        Arc arc = DataProvider.GetArc(model.SelectedArcId);
                        DataProvider.SetSessionArc(arc?.Id, session.Id);
                    }
                }
            }
            return RedirectToAction("EditSessionArc", new { id = model.SessionId });
        }

        public ActionResult ArcPanel(int id)
        {
            if (DataProvider.SiteHasFeature(Feature.SessionArcs))
            {
                SessionArcSummaryModel session = DataProvider.GetSessionArcSummary(id);
                
                if(session != null)
                {
                    return PartialView(session);
                }
                
            }
            return null;
        }

    }
}