using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNet.SignalR;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
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
                ClearPageCache(page.Id);

                List<ExtractedImage> images = _imageProcessor.GetImagesFromHtmlString(page.Description);

                foreach (ExtractedImage image in images)
                {
                    byte[] imageData = _imageProcessor.GetJpegFromImage(image.Image);
                    PageImage pageImage = DataProvider.SaveImage(page.Id, imageData);
                    string linkUrl = Url.Action("ShowImage", "Home", new {id = pageImage.Id});
                    page.Description = page.Description.Replace(image.OriginalSrc, $"src='{linkUrl}'");
                }

                Page updatedPage = DataProvider.UpdatePageDetails(page.Id, page.ShortName, page.FullName, _linkGenerator.ResolveCreoleLinks(page.Description));

                
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
                return View(updatedPage);
            }
            return RedirectToAction("index", new { id = page.Id });
        }

        private void ClearPageCache(int id)
        {
            string path = Url.Action("Image", new { id });

            if (path != null)
            {
                Response.RemoveOutputCacheItem(path);
            }
            
            path = Url.Action("Index", new { id });

            if (path != null)
            {
                Response.RemoveOutputCacheItem(path);
            }

            path = Url.Action("CharacterLeague", "Home");

            if (path != null)
            {
                Response.RemoveOutputCacheItem(path);
            }
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Player")]
        public ActionResult DeleteLink(int id, int linkToDeleteId)
        {
            if (ModelState.IsValid)
            {
                DataProvider.RemoveLink(id, linkToDeleteId);
                return RedirectToAction("EditLinks", new { id = id });
            }
            return RedirectToAction("index", new { id = id });
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        public ActionResult Image(int id)
        {
            Page page = DataProvider.GetPage(id);
            var defaultDir = Server.MapPath("/Content/Images");

            if (page != null)
            {
                if (page.HasImage)
                {
                    return File(page.PrimaryImage, "image/jpeg");
                }

                if (page is Session)
                {
                    var personPath = Path.Combine(defaultDir, "page-session.png");
                    return File(personPath, "image/jpeg");
                }

                if (page is Place)
                {
                    var personPath = Path.Combine(defaultDir, "page-place.png");
                    return File(personPath, "image/jpeg");
                }

                if (page is Person)
                {
                    var personPath = Path.Combine(defaultDir, "page-person.png");
                    return File(personPath, "image/jpeg");
                }

                if (page is SessionLog)
                {
                    SessionLog log = page as SessionLog;

                    if (log.Person.HasImage)
                    {
                        return File(log.Person.PrimaryImage, "image/jpeg");
                    }

                    var personPath = Path.Combine(defaultDir, "page-log.png");
                    return File(personPath, "image/jpeg");
                }

            }



            var defaultImagePath = Path.Combine(defaultDir, "page-page.png");
            return File(defaultImagePath, "image/jpeg");
        }

        [OutputCache(Duration = 360000, VaryByParam = "id", Location = OutputCacheLocation.Downstream)]
        public ActionResult PageImage(int? id)
        {
            if (id.HasValue)
            {
                Page page = DataProvider.GetPage(id.Value);
                return PartialView(page);
            }
            return null;
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
                    ClearPageCache(id);
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
                    return RedirectToAction("Index", new { id = id });
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
                return new Rectangle((int)x1.Value, (int)y1.Value, (int)w.Value, (int)h.Value);
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
                    PlayerSessionControlsViewModel model = ModelFactory.MakePlayerSessionControlsViewModel((Session)page, CurrentPlayer, CurrentPlayerIsGm);

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
                if (playerId == CurrentPlayer.Id || CurrentPlayerIsGm)
                {
                    if (playerId == CurrentPlayer.Id && CurrentPlayerIsGm)
                    {
                        DataProvider.SetGmSuspended(page.Id, false);
                    }
                    else
                    {
                        DataProvider.SetPlayerSuspended(page.Id, playerId, false);
                    }
                    UpdateRoleplayHub();
                }

                PlayerSessionControlsViewModel model = ModelFactory.MakePlayerSessionControlsViewModel((Session)page, CurrentPlayer, CurrentPlayerIsGm);
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
                if (playerId == CurrentPlayer.Id || CurrentPlayerIsGm)
                {
                    if (playerId == CurrentPlayer.Id && CurrentPlayerIsGm)
                    {
                        DataProvider.SetGmSuspended(page.Id, true);
                    }
                    else
                    {
                        DataProvider.SetPlayerSuspended(page.Id, playerId, true);
                    }
                    UpdateRoleplayHub();
                }

                PlayerSessionControlsViewModel model = ModelFactory.MakePlayerSessionControlsViewModel((Session)page, CurrentPlayer, CurrentPlayerIsGm);
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

        public ActionResult RelatedLinks(int id)
        {
            List<PageLinkModel> linkedPages = DataProvider.GetRelatedPages(id);
            return PartialView(linkedPages);
        }
    }
}