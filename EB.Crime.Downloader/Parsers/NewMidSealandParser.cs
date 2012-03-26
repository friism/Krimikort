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
	public class NewMidSealandParser : Parser
	{
		private static Func<HtmlNode, bool> isTitleNode = x => GetTitleNode(x) != null;

		public NewMidSealandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Midtvestsjaelland")
			{
				return false;
			}
			return true;
		}

		public override IEnumerable<Event> GetEvents()
		{
			var nodesraw =
				_document.Value.DocumentNode.SelectNodes(
					"//span[@id='GenericContentPageControl1_ContentPlaceholder']/p");
			if (nodesraw == null)
			{
				yield break;
			}
			
			var nodes = nodesraw.OfType<HtmlNode>()
					.Where(p => !string.IsNullOrEmpty(p.InnerText) && p.InnerText != "&nbsp;");

			if (nodes == null)
			{
				yield break;
			}

			nodes = AdvanceToNextEvent(nodes);

			var lastdatetime = (DateTime?)null;
			var currentdate = _report.ReportDate;

			while(nodes.Any())
			{

				var titlenode = GetTitleNode(nodes.First()).InnerText;

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
					nodes = nodes.Skip(1);
					nodes = AdvanceToNextEvent(nodes);
					continue;
				}

				string title = titlenode.Split(splitchar.Value)[0].Trim();
				string place = titlenode.Split(splitchar.Value)[1].Trim().TrimEnd('.');

				// done with titlenode
				nodes = nodes.Skip(1);

				var bodynodes = nodes.TakeWhile(x => !isTitleNode(x));

				if (!nodes.Any() || !bodynodes.Any())
				{
					yield break;
				}

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
				catch (Exception)
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

				if (!streets.Any() && place.Contains(", "))
				{
					var components = place.Split(',')
						.Select(x => x.Trim());
					if (components.Count() > 1)
					{
						place = components.ElementAt(1);
						streets = AddressExtracter.ExtractAddress(components.ElementAt(0), AddressExtracter.Mode.Classic);
					}
				}

				nodes = AdvanceToNextEvent(nodes);

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

		private IEnumerable<HtmlNode> AdvanceToNextEvent(IEnumerable<HtmlNode> nodes)
		{
			return nodes.SkipWhile(x => !isTitleNode(x));
		}

		public override DateTime GetReportDate(string reportTitle, DateTime publishedDate)
		{
			// they seem to be published with great regularity
			return publishedDate.AddDays(-1);
			
		}
		
		private static HtmlNode GetTitleNode(HtmlNode node)
		{
			var nodes = node.SelectNodes(".//strong");
			if (nodes != null)
			{
				return nodes.FirstOrDefault(y => !string.IsNullOrEmpty(y.InnerText.Trim()));
			}
			return null;
		}
	}
}
