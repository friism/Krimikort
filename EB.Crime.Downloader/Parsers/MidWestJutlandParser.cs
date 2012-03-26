using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EB.Crime.DB;
using EB.Crime.Downloader.Util;
using HtmlAgilityPack;

namespace EB.Crime.Downloader.Parsers
{
	public class MidWestJutlandParser : Parser
	{
		public MidWestJutlandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Midtvestjylland")
			{
				return false;
			}
			return true;
		}

		public override IEnumerable<Event> GetEvents()
		{
			DateTime lastdt = DateTime.MinValue;
			DateTime currentdate = _report.ReportDate.Value;
			var span =
				_document.Value.DocumentNode.SelectSingleNode("//span[@id='Articlewithindexpagecontrol_XMLliste1']/span");

			if (span == null)
			{
				// defunct report: http://politi.dk/Midtvestjylland/da/lokalnyt/Doegnrapporter/03_D%C3%B8gnrapporter_041010.htm
				yield break;
			}

			var h3s = span.SelectNodes("h3");
			foreach (var h3 in h3s)
			{
				string titlestring = h3.InnerText.Trim();
				Regex titleteg =
					new Regex(@"(K|k)(l|L).(\ )?(?'h'\d{2}).?(?'m'\d{2})(-|:)?\ (?'t'.*)(\ )?(-|–)\ (?'l'.*)");

				var match = titleteg.Match(titlestring);
				if (match.Success)
				{
					DateTime incidenttime = new DateTime(
						currentdate.Year,
						currentdate.Month,
						currentdate.Day,
						int.Parse(match.Groups["h"].Value),
						int.Parse(match.Groups["m"].Value), 0);

					if (incidenttime < lastdt)
					{
						// turned over midnight
						incidenttime = incidenttime.AddDays(1);
						currentdate = currentdate.AddDays(1);
					}

					lastdt = incidenttime;
					
					//var following = h3.SelectNodes("following-sibling::*");
					//var taken = following.TakeWhile(n => n.Name.ToLower() != "h3");
					var texters = GetTextSiblings(h3);
					var texterstext = texters.Select(n => n.InnerText.Replace("&nbsp;"," "));
					string bodytext = texterstext.Aggregate((a, b) => a + "\n" + b);

					var addresses = AddressExtracter.ExtractAddress(
						bodytext, AddressExtracter.Mode.Classic);

					var e = new Event
					{
						BodyText = bodytext,
						IncidentTime = incidenttime,
						PlaceString = match.Groups["l"].Value,
						ReportId = _report.ReportId,
						Street = addresses.ElementAtOrDefault(0),
						StreetSecondary = addresses.ElementAtOrDefault(1),
						Title = match.Groups["t"].Value
					};
					yield return e;
				}
				else
				{
					//logger.Info("couldn't parse: " + titlestring);
					continue;
				}
			}
		}

		public override DateTime GetReportDate(string reportTitle, DateTime publishedDate)
		{
			if (!reportTitle.Contains("(uddrag)") && !reportTitle.Contains("(udddrag)"))
			{
				throw new ArgumentException(reportTitle);
			}

			string thedate = reportTitle.Replace("Døgnrapport", "").Replace("(uddrag)","").
				Replace("(udddrag)", "").Replace("den", "").Replace("Døgrnrapport","").
				Replace("(udrag)","").
				Trim().Split(' ').Skip(1).Aggregate((a, b) => a + b).
				Replace(".","");
			return DateTime.ParseExact(thedate, "ddMMMMyyyy", CultureInfo.GetCultureInfo("da-DK"));
		}

		private IEnumerable<HtmlNode> GetTextSiblings(HtmlNode h3)
		{
			var node = h3;
			while (node.NextSibling != null && node.NextSibling.Name.ToLower() != "h3")
			{
				node = node.NextSibling;
				if (node.Name.ToLower() == "#text" || 
					node.Name.ToLower() == "p" ||
					node.Name.ToLower() == "div" ||
					node.Name.ToLower() == "span")
				{
					yield return node;
				}
			}
		}
	}
}
