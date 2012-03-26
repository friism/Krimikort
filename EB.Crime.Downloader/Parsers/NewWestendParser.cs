using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EB.Crime.DB;
using EB.Crime.Downloader.Util;
using HtmlAgilityPack;

namespace EB.Crime.Downloader.Parsers
{
	public class NewWestendParser : Parser
	{
		private static Func<HtmlNode, HtmlNode> getTitleNode =
			x => x.SelectSingleNode("span/b") ?? x.SelectSingleNode("strong");

		private static Func<HtmlNode, bool> isTitleNode = x => getTitleNode(x) != null;

		public NewWestendParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Vestegnen")
			{
				return false;
			}
			return _document.Value.DocumentNode.SelectNodes("//h3") == null;
		}

		public override IEnumerable<Event> GetEvents()
		{
			Console.WriteLine("Extracting events from {0}", _report.Uri);

			var div = _document.Value.DocumentNode
				.SelectSingleNode("//div[@id='mytable']/p[@id='mytable']/span");
			var nodes = div.ChildNodes.OfType<HtmlNode>().
				Where(x => x.Name == "p" &&
					!x.InnerText.Contains("Københavns Vestegns Politi modtager gerne oplysninger") &&
					!string.IsNullOrEmpty(x.InnerText.Trim()));

			nodes = AdvanceToNextEvent(nodes);

			while (nodes.Any())
			{
				var titleNode = getTitleNode(nodes.First());
				var fullTitle = titleNode.InnerText.Trim();

				var @event = new Event { ReportId = _report.ReportId };

				try
				{
					var parsedTitle = ParseTitle(fullTitle);
					@event.Title = parsedTitle.Item1;
					@event.PlaceString = parsedTitle.Item2;
				}
				catch(ArgumentException)
				{
					_logger.Info(string.Format("couldn't work with {0}", fullTitle));
					nodes = nodes.Skip(1);
					nodes = AdvanceToNextEvent(nodes);
					continue;
				}

				HtmlNode firstBodyParagraphNode;
				if (nodes.First().ChildNodes.Count() == 1)
				{
					firstBodyParagraphNode = nodes.First().ChildNodes.ElementAt(0).ChildNodes.ElementAt(1);
				}
				else
				{
					firstBodyParagraphNode = nodes.First().ChildNodes.ElementAt(1);
				}

				nodes = nodes.Skip(1);
				var eventBodyParagraphs = new [] { firstBodyParagraphNode }.Concat(
					nodes.TakeWhile(x => !isTitleNode(x))
				);

				try
				{
					@event.IncidentTime = GetIncidentTime(firstBodyParagraphNode.InnerText);
				}
				catch(ArgumentException)
				{
					nodes = AdvanceToNextEvent(nodes);
					continue;
				}
				
				@event.BodyText = string.Join("\n", eventBodyParagraphs.Select(x => x.InnerText)).Trim();

				var streets = AddressExtracter.ExtractAddress(@event.BodyText, AddressExtracter.Mode.Classic);
				@event.Street = streets.ElementAtOrDefault(0);
				@event.StreetSecondary = streets.ElementAtOrDefault(1);

				nodes = AdvanceToNextEvent(nodes);
				yield return @event;
			}
		}

		private IEnumerable<HtmlNode> AdvanceToNextEvent(IEnumerable<HtmlNode> nodes)
		{
			return nodes.SkipWhile(x => !isTitleNode(x));
		}

		private DateTime? GetIncidentTime(string paragraphText)
		{
			var firsttry = new Regex(@"\A\w+\ den\ \d{1,2}\.\ \w+\ kl\.\ \d\d\.?\d\d\b");
			var secondtry = new Regex(@"\A\D+\d{1,2}\.\ (\w)+\ ");

			var match = firsttry.Match(paragraphText);
			string datestring = null;
			if (match.Success)
			{
				datestring = match.Value;
			}
			else
			{
				match = secondtry.Match(paragraphText);
				if (match.Success)
				{
					datestring = match.Value;
				}
				else
				{
					throw new ArgumentException();
				}
			}
			var datetime = DateParser.ParseDate(datestring, _report.ReportDate.Value);
			return datetime ?? _report.ReportDate;
		}

		private Tuple<string, string> ParseTitle(string fullTitle)
		{
			var titleReg = new Regex(@"(?'tit'.*)\ (i|på|ved|I|–|-)\ (?'pla'([A-Z]|Ø|Æ|Å)\w+(\ ([A-Z]|Ø|Æ|Å)\w+)?)(?'pos'\ (anholdt|efterlyses))?.*\Z");

			var matches = titleReg.Matches(fullTitle);

			if (matches.Count < 1)
			{
				throw new ArgumentException();
			}

			var title = matches[0].Groups["tit"].Value;
			var place = matches[0].Groups["pla"].Value;

			if (matches[0].Groups["pos"] != null && matches[0].Groups["pos"].Value != "")
			{
				title += matches[0].Groups["pos"].Value;
			}

			return new Tuple<string, string>(title, place);
		}

		public override DateTime GetReportDate(string reportTitle, DateTime publishedDate)
		{
			// were screwed for this one and have to trust incidenttime
			return publishedDate;
		}
	}
}
