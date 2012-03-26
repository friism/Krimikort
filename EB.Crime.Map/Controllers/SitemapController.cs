using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using EB.Crime.DB.Rep;
using EB.Crime.Map.Models;

namespace EB.Crime.Map.Controllers
{
	public class SitemapController : Controller
	{
		[OutputCache(Duration = 12 * 3600, VaryByParam = "*")]
		public ContentResult Sitemap()
		{
			string smdatetimeformat = "yyyy-MM-dd";

			var erep = new EventRepository();
			var events = (from e in erep.GetGeocodedEvents()
							where e.IncidentTime.HasValue
						select new {e.Title, e.EventId, e.IncidentTime}).ToList();

			XNamespace sm = "http://www.sitemaps.org/schemas/sitemap/0.9";
			XNamespace geo = "http://www.google.com/geo/schemas/sitemap/1.0";
			
			XDocument doc = new XDocument(
				new XElement(sm + "urlset",
					new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
					new XAttribute(XNamespace.Xmlns + "geo", 
						"http://www.google.com/geo/schemas/sitemap/1.0"),
					new XElement(sm + "url",
						new XElement(sm + "loc", "http://krimikort.ekstrabladet.dk/gearth.kmz"),
						new XElement(sm + "lastmod", DateTime.Now.ToString(smdatetimeformat)),
						new XElement(sm + "changefreq", "daily"),
						new XElement(sm + "priority", "1.0"),
						new XElement(geo + "geo",
							new XElement(geo + "format", "kmz")
						)
					)
					,
					events.Select(e => 
						new XElement(sm + "url",
							new XElement(sm + "loc", EventExtensions.AbsUrl(e.Title, e.EventId)),
							new XElement(sm + "lastmod", e.IncidentTime.Value.ToString(smdatetimeformat)),
							new XElement(sm + "changefreq", "monthly"),
							new XElement(sm + "priority", "0.5")
						)
					)
				)
			);

			return Content(doc.ToString(), "text/xml");
		}
	}
}
