using System;
using System.Globalization;
using System.Linq;
using EB.Crime.DB;

namespace EB.Crime.Downloader.Parsers
{
	public class EastJutlandParser : FunenParser
	{
		public EastJutlandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Oestjylland")
			{
				return false;
			}
			return false;
		}

		public override DateTime GetReportDate(string reportTitle, DateTime publishedDate)
		{
			string datestring = reportTitle.Split(new string[] { " den " }, 
				StringSplitOptions.RemoveEmptyEntries).Last();
			DateTime date = DateTime.ParseExact(datestring, "d. MMMM yyyy", 
				CultureInfo.GetCultureInfo("da-DK"));

			return date;
		}
	}
}
