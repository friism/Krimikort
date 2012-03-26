using System;
using System.Collections.Generic;
using EB.Crime.Downloader.Util;
using HtmlAgilityPack;

namespace EB.Crime.Downloader.Parsers
{
	public class OverviewParser
	{
		public static IEnumerable<Tuple<string, DateTime, string>> GetLinksByUrl(string overviewUrl)
		{
			var html = Doc.GetHtml(overviewUrl);
			return GetlinksByHtml(html);
		}

		public static IEnumerable<Tuple<string, DateTime, string>> GetlinksByHtml(string html)
		{
			var document = Doc.LoadDoc(html);
			var tables = document.DocumentNode.SelectNodes("//table[@width='100%']/tr/td/table[@width='100%']");
			if (tables != null)
			{
				Dictionary<DateTime, List<string>> res = new Dictionary<DateTime, List<string>>();
				foreach (HtmlNode t in tables)
				{
					var datenode = t.SelectSingleNode("tr/td/div[@class='txtLarge']");
					if (datenode == null)
						continue;

					var datestring = datenode.InnerText;
					DateTime publishdate = DateTime.ParseExact(datestring, "dd-MM-yyyy", null);
					var url =
						t.SelectSingleNode("tr/td/span/h2/a").Attributes["href"].Value;

					var titlestring = t.SelectSingleNode("tr/td/span/h2/a").InnerText.Trim();
					//if (titlestring == "Døgnrapport")
					//{
					//    // all is lost, abort
					//    continue;
					//}

					DateTime reportdate;
					try
					{
						reportdate = publishdate; // GetReportDate(titlestring, publishdate);
					}
					catch (Exception)
					{
						//logger.Info("no proper date in " + titlestring);
						continue;
					}

					yield return new Tuple<string, DateTime, string>(url, reportdate, titlestring);
				}
			}
		}
	}
}
