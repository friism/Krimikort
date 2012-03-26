using System.Linq;
using EB.Crime.DB;

namespace EB.Crime.Downloader
{
	public class Categorizer
	{
		private DatabaseDataContext db;

		public Categorizer()
		{
			db = new DatabaseDataContext();
		}

		public void Categorize()
		{
			foreach (var _ in db.Events.Where(_ => _.CategoryId == null))
			{
				FindCategory(_);
			}
			db.SubmitChanges();
		}

		public void FindCategory(Event e)
		{
			if (string.IsNullOrEmpty(e.Title))
				return;
			// try to get cat from title
			var title = e.Title.ToLower();
			e.CategoryId = GetCategory(title);
			if (e.CategoryId == null)
			{
				// try to get it from body
				e.CategoryId = GetCategory(e.BodyText.ToLower());
			}
		}

		private int? GetCategory(string title)
		{
			int? categoryid = null;
			if (false)
				;
			else if (title.Contains("drab"))
				categoryid = 9;
			else if (title.Contains("voldtægt"))
				categoryid = 11;
			else if ((
				title.Contains("vold") ||
				title.Contains("overfald") ||
				title.Contains("slagsmål")) &&
				!title.Contains("voldtægt"))
				categoryid = 8;
			//else if ()
			//    categoryid = 7;
			else if (title.Contains("kokain") ||
					title.Contains("hash") ||
					title.Contains("narko") ||
					title.Contains("stoffer") ||
					title.Contains("stofpåvirket") ||
					title.Contains("amfetamin") ||
					title.Contains("cannabis") ||
					title.Contains("doping") ||
					title.Contains("steroider") ||
					title.Contains("joint") ||
					title.Contains("skunk") ||
					title.Contains("heroin") ||
					title.Contains("euforiserende"))
				categoryid = 12;
			else if (title.Contains("skyderi") ||
					title.Contains("skud") ||
					title.Contains("kniv") ||
					title.Contains("gevær") ||
					title.Contains("våben") ||
					title.Contains("håndgranat") ||
					title.Contains("kanonslag") ||
					title.Contains("økse") ||
					title.Contains("bombe") ||
					title.Contains("pistol") ||
					title.Contains("granat") ||
					title.Contains("ammunition"))
				categoryid = 13;
			else if (title.Contains("røveri") || title.Contains("røver"))
				categoryid = 3;
			//else if (title.Contains("indbrud"))
			//    categoryid = 1;
			else if (title.Contains("tyveri") ||
					title.Contains("butikstyv") ||
					title.Contains("tyv") ||
					title.Contains("stjålet") ||
					title.Contains("stjæle")
					||
					title.Contains("indbrud") ||
					title.Contains("kup"))

				categoryid = 2;
			else if (title.Contains("færdselsuheld") ||
					title.Contains("færdselsluheld") ||
					title.Contains("påkørt") ||
					title.Contains("solouheld") ||
					title.Contains("spøgelsesbilist ")
					||
					title.Contains("spirituskørsel") ||
					title.Contains("spritbilist") ||
					title.Contains("spiritusbilist") ||
					title.Contains("spritkørsel")
					||
					title.Contains("færdsel") ||
					title.Contains("motorcyklist") ||
					title.Contains("eftersættelse"))
				categoryid = 4;
			else if (title.Contains("hærværk") ||
					title.Contains("graffiti")
					||
					title.Contains("brand") ||
					title.Contains("ild") ||
					title.Contains("bål") ||
					title.Contains("udbrændt") ||
					title.Contains("afbrændt") ||
					title.Contains("røg"))
				categoryid = 5;
			//else if (title.Contains("brand")||
			//        title.Contains("ild") ||
			//        title.Contains("bål")
			//    )
			//    categoryid = 6;
			else if (title.Contains("uro") ||
					title.Contains("uorden") ||
					title.Contains("larm") ||
					title.Contains("demonstration"))
				categoryid = 14;
			else
			{
				//Console.WriteLine("no cat for " + title);
			}
			return categoryid;
		}
	}
}
