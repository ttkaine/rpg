using System.Web.Mvc;

namespace Warhammer.Mvc.Controllers
{
    public class PersonController : Controller
    {
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(int personId, string fullName, string shortName, string description, string saveAction)
        {
            if (saveAction == "Save")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");              
            }
        }
    }
}