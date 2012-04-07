using EB.Crime.DB;
using EB.Crime.Map.Views;

namespace EB.Crime.Map.Models
{
	public class EventModel
	{
	}

	public static class EventExtensions
	{
		public static string AbsUrl(this Event e)
		{
			return e.AbsUrl("http://krimikort.ekstrabladet.dk");
		}

		public static string AbsUrl(this Event e, string lefturlpart)
		{
			return AbsUrl(e.Title, e.EventId);
		}

		public static string AbsUrl(string title, int eventid)
		{
			return "http://krimikort.ekstrabladet.dk" + string.Format("/haendelse/{0}/{1}",
						title.ToUrlFriendly(), eventid);
		}
	}
}
