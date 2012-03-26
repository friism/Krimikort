using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EB.Crime.DB;
using EB.Crime.Downloader.Util;

namespace EB.Crime.Downloader.Parsers
{
	public class NorthJutlandParser : Parser
	{
		private static Regex titleReg = new Regex(@"(?'tit'.*)\ (i|på|ved|I|–|-|hos|fra)\ (?'pla'([A-Z]|Ø|Æ|Å)(\w+|\.)(\ ([A-Z]|Ø|Æ|Å)\w+)?)(?'pos'\ (anholdt|efterlyses))?.*\Z");

		public NorthJutlandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Nordjylland")
			{
				return false;
			}
			return true;
		}

		public override IEnumerable<Event> GetEvents()
		{
			var span = _document.Value.DocumentNode.SelectSingleNode("//span[@id='Articlewithindexpagecontrol_XMLliste1']/span");

			var lastdatetime = (DateTime?)null;
			var currentdate = _report.ReportDate;

			Event e = null;
			string rawhtml = null;
			string bodytext = null;
			bool dontrecord = false;

			foreach (var n in span.ChildNodes)
			{
				if (n.Name == "h3")
				{
					e = new Event();
					e.ReportId = _report.ReportId;
					rawhtml = n.OuterHtml;
					bodytext = "";
					
					dontrecord = false;

					var headstring = n.InnerText.Trim();

					var timestring = headstring.Split('-', '–','/').First().Replace("kl.", "").Trim();

					DateTime? dt = null;
					try
					{
						dt = DateTime.ParseExact(timestring, "HH.mm", null);

						if (dt < lastdatetime)
						{
							// looks like we passed midnight, increment day
							currentdate = currentdate.Value.AddDays(1);
						}
						lastdatetime = dt;
					}
					catch (Exception ex)
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
						e.IncidentTime = thedate;
					}

					try
					{
						headstring =
							headstring.Split('-', '–', '/').Skip(1).Aggregate((a, b) => a + "-" + b).Trim();
					}
					catch (Exception ex)
					{
						dontrecord = true;
						continue;
					}

					var matches = titleReg.Matches(headstring);

					if (matches.Count < 1)
					{
						//Console.WriteLine(string.Format("couldn't work with {0}", headstring));
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
					bodytext += n.InnerText + "\n";
					rawhtml += n.OuterHtml;
				}
				else if (n.Name == "ul")
				{
					bodytext += n.InnerText.Replace("\r","") + "\n";
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

		public override DateTime GetReportDate(string s, DateTime publishedDate)
		{
			string splitter = null;
			if (s.Contains("-"))
			{
				splitter = "-";
			}
			else if (s.Contains("til den"))
			{
				splitter = "til den";
			}
			else if (s.Contains("til"))
			{
				splitter = "til";
			}
			else
			{
				throw new ArgumentException("no spliter found for " + s);
			}

			//bugfixes
			if (s.Contains("209"))
			{
				s = s.Replace("209", "2009");
			}

			string lastdate = 
				s.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries).
				Last().Trim().Replace(" ","").Replace(".","");
			return DateTime.ParseExact(lastdate,
				"dMMMMyyyy", 
				CultureInfo.GetCultureInfo("da-DK")).
				AddDays(-1); // this to get the start of report
		}
	}
}
