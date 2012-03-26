using System.Linq;
using System.Web.Mvc;
using EB.Crime.DB.Rep;
using EB.Crime.Map.Models;
using EB.Crime.Map.Views;
using Google.KML;

namespace EB.Crime.Map.Controllers
{
	public class KMLController : Controller
	{
		[OutputCache(Duration=12*3600, VaryByParam="*")]
		public FileResult KML()
		{
			geKML kml = GetKML();
			return new FileContentResult(kml.ToKML(), "application/vnd.google-earth.kml+xml");
		}

		[OutputCache(Duration = 12 * 3600, VaryByParam = "*")]
		public FileResult KMZ()
		{
			geKML kml = GetKML();
			return new FileContentResult(kml.ToKMZ(), "application/vnd.google-earth.kmz");
		}

		[OutputCache(Duration = 12 * 3600, VaryByParam = "*")]
		public FileResult GE()
		{
			var doc = new geDocument();
			doc.Visibility = true;
			doc.Features.Add(
				new geNetworkLink(
					new geLink("http://krimikort.ekstrabladet.dk/kmz")
					)
				);

			var kml = new geKML(doc);
			return new FileContentResult(kml.ToKMZ(), "application/vnd.google-earth.kmz");
		}

		private static geKML GetKML()
		{
			geDocument doc = new geDocument();

			doc.Name = "Ekstra Bladets Krimikort";
			doc.AuthorName = "Ekstra Bladet";
			doc.Link = "http://krimikort.ekstrabladet.dk/";

			var categories = (new CategoryRepository()).GetCategories();
			doc.StyleSelectors.AddRange(
				categories.Select(c =>
					new geStyle(c.DisplayName)
					{
						IconStyle = new geIconStyle
						{
							Icon = new geIcon(
								string.Format("http://krimikort.ekstrabladet.dk/Content/icons/{0}.png",
									ViewUtil.GetIconName(c.CategoryId))),
						},
					}
				));

			var erep = new EventRepository();
			var events = (from e in erep.GetGeocodedEvents()
				select new { e.Category, e.Title, e.BodyText, e.EventId, e.Lng, e.Lat }).
				ToList();

			doc.Features.AddRange(
				events.Select(e => new gePlacemark()
				{
					Name = e.Title,
					ID = e.EventId.ToString(),
					Description = e.BodyText ?? "",
					StyleUrl = "#" + (e.Category == null ? "Andet" : e.Category.DisplayName),
					Link = EventExtensions.AbsUrl(e.Title, e.EventId),
					Geometry = new gePoint(
						new geCoordinates(
							new geAngle90(e.Lat.Value), new geAngle180(e.Lng.Value)
							)
						)
				})
				);

			return new geKML(doc);
		}
	}

	//[XmlRoot(Namespace = "http://www.opengis.net/kml/2.2", ElementName="kml")]
	//public class KML
	//{
	//    public class CDocument
	//    {
	//        public class CPlacemark
	//        {
	//            public class CPoint
	//            {
	//                public string Coordinates { get; set; }
	//            }
	//            [XmlElement(ElementName="name")]
	//            public string Name { get; set; }
				
	//            [XmlElement(ElementName = "description")]
	//            public string Description { get; set; }
				
	//            public CPoint Point { get; set; }
	//        }
	//        public CPlacemark[] placemarks { get; set; }
	//    }
	//    [XmlElement(ElementName = "Document")]
	//    public CDocument Document { get; set; }
	//}
	//    return new XmlResult(
	//        new KML
	//        {
	//            Document = new KML.CDocument
	//            {
	//                placemarks =
	//                    db.Events.Where(e => e.Lat != null).
	//                    Take(20).
	//                    Select(e =>
	//                        new KML.CDocument.CPlacemark
	//                        {
	//                            Description = e.BodyText,
	//                            Name = e.Title,
	//                            Point =
	//                                new KML.CDocument.CPlacemark.CPoint
	//                                {
	//                                    Coordinates = string.Format(
	//                                        "{0},{1}",
	//                                        e.Lat.Value.ToString(CultureInfo.InvariantCulture),
	//                                        e.Lng.Value.ToString(CultureInfo.InvariantCulture)),
	//                                }
	//                        }).ToArray(),
	//            }
	//        }
	//        );
	//
}
