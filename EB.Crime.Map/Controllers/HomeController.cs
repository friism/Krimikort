using System;
using System.Linq;
using System.Web.Mvc;
using EB.Crime.DB;
using EB.Crime.DB.Rep;
using MvcContrib.ActionResults;

namespace EB.Crime.Map.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        [OutputCache(Duration = 600, VaryByParam = "*")]
        public ActionResult Index()
        {
            var db = new DatabaseDataContext();

            var mostrecentevent = db.Events.
                Where(e => e.IncidentTime <= DateTime.Now && e.Lat != null).
                Select(e => e.IncidentTime).
                OrderByDescending(d => d).First().Value;

            var earliestevent = db.Events.
                Where(e => e.IncidentTime <= DateTime.Now && e.Lat != null).
                Select(e => e.IncidentTime).
                OrderBy(d => d).First().Value;

            ViewData["Earliest"] = earliestevent.Year < 2008 ? new DateTime(2008,1,1) : earliestevent;
            ViewData["MostRecent"] = mostrecentevent;

            ViewData["Categories"] = (new CategoryRepository()).GetCategories();

            return View();
        }

        public XmlResult KML()
        {
            return new XmlResult(null);
        }
    }
}
