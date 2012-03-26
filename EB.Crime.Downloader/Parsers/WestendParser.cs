using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EB.Crime.DB;
using EB.Crime.Downloader.Util;

namespace EB.Crime.Downloader.Parsers
{
	public class WestendParser : Parser
	{
		private static Regex titleReg = new Regex(@"(?'tit'.*)\ (i|på|ved|I|–|-)\ (?'pla'([A-Z]|Ø|Æ|Å)\w+(\ ([A-Z]|Ø|Æ|Å)\w+)?)(?'pos'\ (anholdt|efterlyses))?.*\Z");
		private static Regex firsttry = new Regex(@"\A\w+\ den\ \d{1,2}\.\ \w+\ kl\.\ \d\d\d\d\b");
		private static Regex secondtry = new Regex(@"\A\D+\d{1,2}\.\ (\w)+\ ");

		public WestendParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Vestegnen")
			{
				return false;
			}
			return false;
			//return _document.Value.DocumentNode.SelectSingleNode("//div[@id='mytable']") != null;
		}

		public override IEnumerable<Event> GetEvents()
		{
			if (_document.Value.DocumentNode.InnerText.Contains("Døgnrapporten udkommer ikke den") ||
				_document.Value.DocumentNode.InnerText.Contains("Døgnrapporten udkommer desværre ikke idag"))
			{
				// see here:
				// http://politi.dk/Vestegnen/da/lokalnyt/Doegnrapporter/K10_doegnrapport_270709.htm
				yield break;
			}

			var span = _document.Value.DocumentNode.SelectSingleNode("//div[@id='mytable']");

			Event e = null;
			string rawhtml = null;
			string bodytext = null;
			bool? isfirst = null;
			bool dontrecord = false;
			if (span == null)
			{

				_logger.Info("problem with report mounth {0} day {1} year {2}:",
					_report.ReportDate.Value.Month, _report.ReportDate.Value.Day, _report.ReportDate.Value.Year);
				_logger.Info("http://politi.dk" + _report.Uri);
			}
			foreach (var n in span.ChildNodes)
			{
				if (n.Name == "h3")
				{
					e = new Event();
					e.ReportId = _report.ReportId;
					rawhtml = n.OuterHtml;
					bodytext = "";
					// followed by date, should not go in bodytext
					isfirst = true;
					dontrecord = false;

					var headstring = n.InnerText.Trim();

					//bug fixes
					headstring =
						headstring.Replace("Tricktyveri", "Tricktyveri i").
						Replace("  ", " ");

					var matches = titleReg.Matches(headstring);

					if (matches.Count < 1)
					{
						_logger.Info(string.Format("couldn't work with {0}", headstring));
						dontrecord = true;
						continue;
					}

					var title = matches[0].Groups["tit"].Value;
					var place = matches[0].Groups["pla"].Value;

					if (matches[0].Groups["pos"] != null && matches[0].Groups["pos"].Value != "")
					{
						title += matches[0].Groups["pos"].Value;
					}

					e.Title = title;
					e.PlaceString = place;

				}
				else if (n.Name == "p" || n.Name == "#text" || n.Name == "span")
				{
					if (isfirst.Value)
					{
						isfirst = false;
						// try to extract date
						bodytext += n.InnerText.Trim() + "\n";

						var match = firsttry.Match(n.InnerText);
						string datestring = null;
						if (match.Success)
						{
							datestring = match.Value;
						}
						else
						{
							match = secondtry.Match(n.InnerText);
							if (match.Success)
							{
								datestring = match.Value;
							}
							else
							{
								dontrecord = true;
								//throw new Exception(string.Format("could not find date in {o}", n.InnerText));
							}
						}
						var dt = DateParser.ParseDate(datestring, _report.ReportDate.Value);
						e.IncidentTime = dt;
					}
					else
					{
						bodytext += n.InnerText + "\n";
					}
					rawhtml += n.OuterHtml;
				}
				else if (n.Name == "a" &&
					n.Attributes["class"] != null &&
					n.Attributes["class"].Value == "linkup" &&
					n.Attributes["href"] != null &&
					n.Attributes["href"].Value == "#top")
				{
					// at end of event
					e.BodyText = bodytext.Trim();
					var streets = AddressExtracter.ExtractAddress(e.BodyText, AddressExtracter.Mode.Classic);
					e.Street = streets.ElementAtOrDefault(0);
					e.StreetSecondary = streets.ElementAtOrDefault(1);
					if (!dontrecord)
					{
						yield return e;
					}

				}
				else
				{
					rawhtml += n.OuterHtml;
				}
			}
		}

		public override DateTime GetReportDate(string reportTitle, DateTime publishedDate)
		{
			// were screwed for this one and have to trust incidenttime
			return publishedDate;
		}
	}
}
