using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EB.Crime.DB;
using EB.Crime.Downloader.Util;

namespace EB.Crime.Downloader.Parsers
{
	public class NorthSealandParser : Parser
	{
		public NorthSealandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Nordsjaelland")
			{
				return false;
			}
			return _document.Value.DocumentNode
				.SelectSingleNode("//span[@id='Articlewithindexpagecontrol_XMLliste1']/span") != null;
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
			bool? isfirst = null;

			foreach (var n in span.ChildNodes)
			{
				if (n.Name == "h3")
				{
					e = new Event();
					e.ReportId = _report.ReportId;
					rawhtml = n.OuterHtml;
					bodytext = "";

					isfirst = true;
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
						e.IncidentTime = thedate;
					}

					e.Title = headstring.Split(',').First().Trim();
					e.PlaceString = headstring.Split(',').Last().Trim().Trim('.');

				}
				else if (n.Name == "p" || n.Name == "#text" 
					|| n.Name == "span" || n.Name == "strong")
				{
					if (isfirst.Value)
					{
						isfirst = false;
						// try to extract date
						bodytext += n.InnerText.Trim() + "\n";

						var findtime = new Regex(@"\d{4}");
						string timestring = null;
						var match = findtime.Match(n.InnerText);
						if (match.Success)
						{
							timestring = match.Value;
						}
						else
						{
							dontrecord = true;
							_logger.Info("couldn't find time in " + n.InnerText);
							//throw new Exception(string.Format("could not find date in {o}", n.InnerText));
						}

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
					}
					else
					{
						bodytext += n.InnerText + "\n";
					}
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

					if (e.Street == null && e.PlaceString.Contains(" i "))
					{
						var components = e.PlaceString.Split(new[] { " i " }, StringSplitOptions.RemoveEmptyEntries);
						if (components.Length > 1)
						{
							e.PlaceString = components[1];
							e.Street = components[0];
						}
					}

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

			string lastdate = 
				s.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries).
				Last().Trim().Replace("d.","").Replace(".","").Replace(" ","");
			return DateTime.ParseExact(lastdate,
				"dMMMMyyyy", 
				CultureInfo.GetCultureInfo("da-DK")).
				AddDays(-1); // this to get the start of report
		}
	}
}
