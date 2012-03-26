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
	public class MidSealandParser : Parser
	{
		public MidSealandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			return false;
		}

		public override IEnumerable<Event> GetEvents()
		{
			var ps = 
				_document.Value.DocumentNode.SelectNodes(
					"//span[@id='GenericContentPageControl1_ContentPlaceholder']/p");

			if (ps == null)
			{
				yield break;
			}

			var lastdatetime = (DateTime?)null;
			var currentdate = _report.ReportDate;

			foreach (var p in ps)
			{
				if (string.IsNullOrEmpty(p.InnerText) || p.InnerText == "&nbsp;")
					continue;

				if (p.ChildNodes.Count < 3)
				{
					// everythings fucked
					continue;
				}

				IEnumerable<HtmlNode> nodes = null;
				if (p.ChildNodes.First().Name.ToLower() == "br")
					nodes = p.ChildNodes.Skip(1);
				else
					nodes = p.ChildNodes;

				var titlenode = nodes.ElementAt(0).InnerText;

				// first two nodes are title and a br
				var bodynodes = nodes.Skip(2).Where(_ => _.Name.ToLower() != "BR");

				char? splitchar = null;
				if (titlenode.Contains("-"))
				{
					splitchar = '-';
				}
				else if (titlenode.Contains("–"))
				{
					splitchar = '–';
				}
				else
				{
					//throw new ArgumentException();
					_logger.Info(string.Format("Problem, no seperator in '{0}'", titlenode));
					continue;
				}

				string title = titlenode.Split(splitchar.Value)[0].Trim();
				string place = titlenode.Split(splitchar.Value)[1].Trim().TrimEnd('.');

				string timestring = bodynodes.First().InnerText.TrimStart('K', 'l', '.', ' ').Substring(0, 4);

				DateTime? dt = null;
				try
				{
					dt = DateTime.ParseExact(timestring, "HHmm", null);

					if (dt < lastdatetime)
					{
						// looks like we passed midnight, increment day
						currentdate = currentdate.Value.AddDays(1);
					}
					lastdatetime = dt;
				}
				catch (Exception e)
				{
					 //suck it
				}
				
				// the datetime only gets hour and min stuff from data, 
				// rest should be from report data (which is accurate for mid sealand)
				var thedate = new DateTime(currentdate.Value.Year, currentdate.Value.Month,
					currentdate.Value.Day);
				if (dt.HasValue)
				{
					thedate = new DateTime(thedate.Year, thedate.Month, thedate.Day,
						dt.Value.Hour, dt.Value.Minute, 0);
				}

				string bodytext = bodynodes.Select(_ => _.InnerText.Replace("&nbsp;", " ")).Aggregate((a, b) => a + "\n" + b);

				var streets = AddressExtracter.ExtractAddress(bodytext, AddressExtracter.Mode.Classic);

				yield return new Event
				{
					BodyText = bodytext,
					PlaceString = place,
					Title = title,
					IncidentTime = thedate,
					Street = streets.ElementAtOrDefault(0),
					StreetSecondary = streets.ElementAtOrDefault(1),
					ReportId = _report.ReportId
				};
			}
		}

		public override DateTime GetReportDate(string reportTitle, DateTime publishedDate)
		{
			Regex midwestre = new Regex(@"\ADøgnrapport\ (?'d'\d{1,2}).\ (?'m'\w+) (?'y'\d{4})\Z");

			var match = midwestre.Match(reportTitle);
			if (match.Success)
			{
				var month = match.Groups["m"].Value;
				month = month.Replace("decmber", "december");

				return DateTime.ParseExact(
					match.Groups["y"].Value +
					match.Groups["d"].Value +
					month, "yyyydMMMM",
					CultureInfo.GetCultureInfo("da-DK"));
			}
			else
			{
				throw new ArgumentException("Could not parse date from" + reportTitle);
			}
		}
	}
}
