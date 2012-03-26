using System;
using System.Globalization;
using System.Linq;
using EB.Crime.DB;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace EB.Crime.Downloader.Parsers
{
	public class NewEastJutlandParser : NewFunenParser
	{
		public NewEastJutlandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Oestjylland")
			{
				return false;
			}
			return true;
		}

		public override IEnumerable<Event> GetEvents()
		{
			return base.GetEvents(new EastJutlandAddressExtractor());
		}

		public override DateTime GetReportDate(string reportTitle, DateTime publishedDate)
		{
			string datestring = reportTitle.Split(new string[] { " den " }, 
				StringSplitOptions.RemoveEmptyEntries).Last();
			try
			{
				DateTime date = DateTime.ParseExact(datestring, "d. MMMM yyyy",
					CultureInfo.GetCultureInfo("da-DK"));
				return date;
			}
			catch (Exception e)
			{

			}
			return publishedDate;
		}
	}

	public class EastJutlandAddressExtractor : FunenAddressExtractor
	{
		public override IEnumerable<HtmlNode> GetAddressNodes(LinkedList<HtmlNode> nodes)
		{
			int postcode = 0;
			return nodes.Where(_ => int.TryParse(_.InnerText.Split(' ').ElementAt(1), out postcode) &&
					postcode > 999);
		}

		public override string GetAddressNodesMinusPostcode(IEnumerable<HtmlNode> addressnodes)
		{
			return addressnodes.Single().InnerText.Split(' ').Where(x => !IsInt(x)).
					Aggregate((a, b) => a + " " + b).Replace("&nbsp;", " ");
		}

		public override IEnumerable<string> SplitAddress(string address)
		{
			var components = address.Split(new string[] { " " },
						StringSplitOptions.RemoveEmptyEntries);
			yield return components.Skip(1).Aggregate((a, b) => a + " " + b);
			yield return components.First();
		}

		private bool IsInt(string s)
		{
			int val;
			return int.TryParse(s, out val);
		}
	}
}
