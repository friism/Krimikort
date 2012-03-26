using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EB.Crime.DB;
using EB.Crime.DB.Rep;
using EB.Crime.Map.Views;

namespace EB.Crime.Map.Controllers
{
	public class DataController : Controller
	{
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Data(
			string datemode, string startdate, string enddate,
			string categories, string race,
			string ne_lat, string ne_lng,
			string sw_lat, string sw_lng)
		{
			IEnumerable<int?> cats = categories.Trim('-').
				Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).
				Select(_ => (int?)int.Parse(_));

			float nelat = float.Parse(ne_lat, CultureInfo.GetCultureInfo("en-US"));
			float nelng = float.Parse(ne_lng, CultureInfo.GetCultureInfo("en-US"));
			float swlat = float.Parse(sw_lat, CultureInfo.GetCultureInfo("en-US"));
			float swlng = float.Parse(sw_lng, CultureInfo.GetCultureInfo("en-US"));

			var enddt = DateTime.MaxValue;
			var startdt = DateTime.MinValue;

			var db = new DatabaseDataContext();

			if (datemode == "day")
			{
				enddt = DateTime.Now.AddDays(1);
				startdt = DateTime.Now.AddDays(-1);
			}
			else if (datemode == "week")
			{
				enddt = DateTime.Now.AddDays(1);
				startdt = DateTime.Now.AddDays(-7);
			}
			else if (datemode == "month")
			{
				startdt =
					DateTime.ParseExact(startdate, "MMM-yyyy", CultureInfo.GetCultureInfo("da-DK"));
				enddt =
					DateTime.ParseExact(enddate, "MMM-yyyy", CultureInfo.GetCultureInfo("da-DK"));
			}

			var bar = GetEvents(cats, nelat, nelng, swlat, swlng, enddt, startdt, 2000, db)
				.ToArray();
			return this.Json(
				new
				{
					count = bar.Length,
					events = bar,
				});
		}

		public static IEnumerable<object[]> GetEvents(
			IEnumerable<int?> cats, 
			float nelat, float nelng, float swlat, float swlng, 
			DateTime enddt, DateTime startdt, int take, DatabaseDataContext db)
		{
			var erep = new EventRepository();

			var foo =
				from e in erep.GetGeocodedEvents(startdt, enddt)
				where
						e.Lat > swlat &&
						e.Lat < nelat &&
						e.Lng > swlng &&
						e.Lng < nelng &&
						((e.CategoryId.HasValue && cats.Contains(e.CategoryId.Value)) ||
						(!e.CategoryId.HasValue && cats.Contains(42)))
				orderby e.Lat, e.Lng, e.IncidentTime descending
				select new { Lat = e.Lat.Value, Lng = e.Lng.Value, e.EventId, e.CategoryId };

			var current = new { foo.First().Lat, foo.First().Lng };
			var akkumulator = new List<object>();

			var counter = 0;

			foreach (var item in foo)
			{
				if (counter > take)
				{
					yield break;
				}

				if (current.Lat == item.Lat && current.Lng == item.Lng)
				{
					akkumulator.Add(new object[] { item.EventId, item.CategoryId });
				}
				else
				{
					yield return new object[] { current.Lat, current.Lng, akkumulator.ToArray() };
					current = new { item.Lat, item.Lng };
					akkumulator = new List<object>(new object[] { item.EventId, item.CategoryId });
				}
				counter++;
			}
			
			yield return new object[] { current.Lat, current.Lng, akkumulator.ToArray() };
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[OutputCache(Duration = 600, VaryByParam = "*")]
		public JsonResult EventData(int eventid)
		{
			var db = new DatabaseDataContext();
			var e = db.Events.Single(_ => _.EventId == eventid);
			return this.Json(new
			{
				text = e.BodyText,
				city = e.PlaceString,
				date = e.IncidentTime.Value.ToString("d. MMMM kl. HH:mm"),
				street = e.Street ?? "",
				title = e.Title,
				displayid = ViewUtil.ToBase26(e.EventId),
				url = string.Format("/haendelse/{0}/{1}", e.Title.ToUrlFriendly(), e.EventId)
			});
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[OutputCache(Duration = 600, VaryByParam = "*")]
		public JsonResult EventDataMulti(string eventidstring)
		{
			IEnumerable<int> eids = eventidstring.Trim('-').
				Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).
				Select(_ => int.Parse(_));

			var db = new DatabaseDataContext();
			var evs = db.Events.Where(e => eids.Contains(e.EventId));
			return this.Json(evs.ToList().Select(e => new
			{
				text = e.BodyText,
				city = e.PlaceString,
				date = e.IncidentTime.Value.ToString("d. MMMM kl. HH:mm"),
				street = e.Street ?? "",
				title = e.Title,
				catid = e.CategoryId ?? 42,
				displayid = ViewUtil.ToBase26(e.EventId),
				url = string.Format("/haendelse/{0}/{1}", e.Title.ToUrlFriendly(), e.EventId)
			}));
		}
	}
}
