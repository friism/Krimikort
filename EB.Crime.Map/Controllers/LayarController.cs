using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EB.Crime.DB;

namespace EB.Crime.Map.Controllers
{
	public class LayarController : Controller
	{
		public ActionResult GetPOIs(string lat, string lon, 
			string requestedPoiId, string pageKey)
		{
			var db = new DatabaseDataContext();

			int? page = null;
			if (!string.IsNullOrEmpty(pageKey))
			{
				page = int.Parse(pageKey);
			}

			var eventssp = db.FindNearestEvents(
				float.Parse(lat, NumberStyles.Float, CultureInfo.InvariantCulture),
				float.Parse(lon, NumberStyles.Float, CultureInfo.InvariantCulture),
				20, page ?? 0);

			var events = eventssp.Select(e => new POI()
			{
				lat = e.Lat.Value.ToLayarCoord(),
				lon = e.Lng.Value.ToLayarCoord(),
				distance = e.Distance.Value,
				id = e.EventId,
				title = 
					e.Title + " (" + e.IncidentTime.Value.ToString("d. MMMM yyyy kl. HH:mm") + ")",
				line2 = e.BodyText,
				attribution = "Ekstra Bladet Krimikort"
			}).ToList();

			return this.Json(
				new Response
				{
					radius = (int)(events.Max(e => e.distance) * 1000),
					nextPageKey = page != null ? page + 1 : 1,
					morePages = events.Count() == 20,
					hotspots = events,
				}, JsonRequestBehavior.AllowGet
				);
		}
	}

	public class Response
	{
		public string layer { get { return "krimikort"; } }
		public int errorCode { get { return 0; } }
		public string errorString { get { return "ok"; } }
		public IEnumerable<POI> hotspots { get; set; }
		public int radius { get; set; }
		public int? nextPageKey { get; set; }
		public bool morePages { get; set; }
	}

	public class POI
	{
		public object[] actions { get { return new object[] { }; } }
		public string attribution { get; set; }
		public float distance { get; set; }
		public int id { get; set; }
		public string imageUrl { get; set; }
		public int lat { get; set; }
		public int lon { get; set; }
		public string line2 { get; set; }
		public string line3 { get; set; }
		public string line4 { get; set; }
		public string title { get; set; }
		public int type { get; set; }
	}

	public static class Extensions
	{
		public static int ToLayarCoord(this double coord)
		{
			return (int)(coord * 1000000);
		}
	}
}
