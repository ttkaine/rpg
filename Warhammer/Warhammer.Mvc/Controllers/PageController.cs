using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
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

                    return View(page);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Index(Page page, string saveAction)
        {
            if (ModelState.IsValid && saveAction == "Save")
            {
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
            }

            var defaultImagePath = Path.Combine(defaultDir, "no-image.jpg");
            Response.Cache.SetExpires(DateTime.Now.AddYears(1));
            Response.Cache.SetCacheability(HttpCacheability.Public);
            return File(defaultImagePath, "image/jpeg"); 
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