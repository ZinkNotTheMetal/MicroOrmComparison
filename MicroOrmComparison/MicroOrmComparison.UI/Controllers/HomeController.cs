using System.Web.Mvc;

namespace MicroOrmComparison.UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AboutMicroOrms()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Entity()
        {
            return View();
        }

        public ActionResult Handrolled()
        {
            return View();
        }

        public ActionResult Dapper()
        {
            return View();
        }

        public ActionResult InsightDatabase()
        {
            return View();
        }

        public ActionResult OrmLite()
        {
            return View();
        }

        public ActionResult SimpleData()
        {
            return View();
        }

        public ActionResult PerformanceTesting()
        {
            return View();
        }
    }
}