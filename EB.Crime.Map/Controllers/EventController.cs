using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EB.Crime.DB;

namespace EB.Crime.Map.Controllers
{
	public class EventController : Controller
	{
		[OutputCache(Duration = 600, VaryByParam = "*")]
		public ActionResult EventDetails(string slug, int eventid)
		{
			var db = new DatabaseDataContext();

			var currentevent = 
				db.Events.Single(_ => _.EventId == eventid);
			
			ViewData["CurrentEvent"] = currentevent;

			var mostrecentevent = db.Events.
				Where(e => e.IncidentTime <= DateTime.Now && e.Lat != null).
				Select(e => e.IncidentTime).
				OrderByDescending(d => d).First().Value;

			var earliestevent = db.Events.
				Where(e => e.IncidentTime <= DateTime.Now && e.Lat != null).
				Select(e => e.IncidentTime).
				OrderBy(d => d).First().Value;

			ViewData["Earliest"] = earliestevent.Year < 2008 ? new DateTime(2008, 1, 1) : earliestevent;
			ViewData["MostRecent"] = mostrecentevent;

			ViewData["Categories"] =
				db.Categories.ToList().Concat(
					new List<Category>() { new Category { DisplayName = "Andet", CategoryId = 42 } });

			return View("../Home/Index");
		}
	}
}
