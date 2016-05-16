using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using Warhammer.Mvc.Abstract;
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
                                CurrentPlayer.IsGm
                               );
                    }

                    return View(page);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Index(Page page)
        {
            if (ModelState.IsValid)
            {
                ClearPageCache(page.Id);

                Page updatedPage = DataProvider.UpdatePageDetails(page.Id, page.ShortName, page.FullName, _linkGenerator.ResolveCreoleLinks(page.Description));
                if (updatedPage.ImageData == null)
                {
                    Image image = _imageProcessor.GetImageFromHtmlString(updatedPage.Description);
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
        public ActionResult DeleteLink(int id, int linkToDeleteId)
        {
            if (ModelState.IsValid)
            {
                DataProvider.RemoveLink(id, linkToDeleteId);
                return RedirectToAction("EditLinks", new { id = id });
            }
            return RedirectToAction("index", new { id = id });
        }

        [OutputCache(Duration = 3600, VaryByParam = "id", Location = OutputCacheLocation.ServerAndClient, NoStore = true)]
        public ActionResult Image(int id)
        {
            Page page = DataProvider.GetPage(id);
            var defaultDir = Server.MapPath("/Content/Images");

            if (page != null)
            {
                if (page.ImageData != null && page.ImageData.Length > 100 && !string.IsNullOrWhiteSpace(page.ImageMime))
                {
                    return File(page.ImageData, page.ImageMime);
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

                    if (log.Person.ImageData != null && log.Person.ImageData.Length > 100 && !string.IsNullOrWhiteSpace(log.Person.ImageMime))
                    {
                        return File(log.Person.ImageData, log.Person.ImageMime);
                    }

                    var personPath = Path.Combine(defaultDir, "page-log.png");
                    return File(personPath, "image/jpeg");
                }

            }



            var defaultImagePath = Path.Combine(defaultDir, "page-page.png");
            return File(defaultImagePath, "image/jpeg");
        }

        public ActionResult PageImage(int? id)
        {
            if (id.HasValue)
            {
                Page page = DataProvider.GetPage(id.Value);
                return PartialView(page);
            }
            return null;
        }

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


    }
}