using System.Web.Mvc;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {

        }
        public ActionResult Index()
        {
            return View();
        }
    }
}