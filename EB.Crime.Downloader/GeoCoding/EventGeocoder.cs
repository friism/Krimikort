using System;
using System.Linq;
using EB.Crime.DB;

namespace EB.Crime.Downloader.GeoCoding
{
	public class EventGeocoder
	{
		public void GeoCodeEvents()
		{
			var db = new DatabaseDataContext();
			foreach (var e in db.Events.Where(_ => _.Street != null &&
				_.PlaceString != null && _.Lat == null).OrderByDescending(_ => _.Report.ReportDate))
			{
				try
				{
					var pos = GeoCodeEvent(e);
					if (pos != null)
					{
						e.Lat = pos.Lat;
						e.Lng = pos.Lng;
						db.SubmitChanges();
					}
				}
				catch (Exception exception)
				{
					// give up
					return;
				}
			}
		}

		public Position GeoCodeEvent(Event e)
		{
			var pos = Geocoder.GeoCode(e.Street + " " + e.PlaceString);
			if (pos == null)
			{
				pos = Geocoder.GeoCode(
					e.Street.Split(
					new string[] { "ved ", " v.", " overfor ", 
								" over for ", " ud for ", " i ", " – ", "bag " },
					StringSplitOptions.RemoveEmptyEntries).First().Trim()
						+ " " + e.PlaceString);

				if (e.StreetSecondary != null)
				{
					pos = Geocoder.GeoCode(e.StreetSecondary + " " + e.PlaceString);
				}

				if (pos == null)
				{
					pos = Geocoder.GeoCode(e.PlaceString);
				}
			}
			return pos;
		}
	}
}
