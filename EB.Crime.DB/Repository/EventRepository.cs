using System;
using System.Linq;

namespace EB.Crime.DB.Rep
{
	public class EventRepository : Repository
	{
		public IQueryable<Event> GetGeocodedEvents()
		{
			return GetGeocodedEvents(new DateTime(2008, 1, 1), DateTime.Now);
		}

		public IQueryable<Event> GetGeocodedEvents(DateTime startdate, DateTime enddate)
		{
			return from e in DB.Events
					where e.Lng.HasValue && e.Lat.HasValue &&
						e.IncidentTime > startdate && e.IncidentTime < enddate
					select e;
				
		}
	}
}
