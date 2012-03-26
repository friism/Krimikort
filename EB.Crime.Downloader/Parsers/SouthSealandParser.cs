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
	public class SouthSealandParser : Parser
	{
		public SouthSealandParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Sydsjaelland")
			{
				return false;
			}
			return false;
		}

		public override IEnumerable<Event> GetEvents()
		{
			IEnumerable<Event> res = GetMainEvents().ToList();

			var sideboxes = _document.Value.DocumentNode.SelectNodes("//span[starts-with(@id, 'Genericrightmenu1_RightMenuPlaceholderBox')]");
			if (sideboxes != null)
			{
				foreach (var sb in sideboxes)
				{
					IEnumerable<Event> sbevents = DoSideBox(sb).ToList();
					res = res.Concat(sbevents);
				}
			}

			return res;
		}

		private IEnumerable<Event> DoSideBox(HtmlNode sb)
		{
			var foo = sb.SelectNodes("p");

			IEnumerable<HtmlNode> ps = null;
			if(foo == null)
			{
				yield break;
			}
			else{    
			ps = foo.Where(_ => 
				!string.IsNullOrEmpty(_.InnerText.Replace("&nbsp;","").Trim()));
			}
			// the title of what's going on is in the first p
			string title = UppercaseFirst(ps.First().InnerText.ToLower());

			Event lastevent = null;

			// the juicy ones are after the first two
			foreach (var ep in ps.Skip(1))
			{
				// check to see whether this is underheading
				var italics = ep.SelectNodes("i");
				if (italics != null)
				{
					continue;
				}
				string body = ep.InnerText;
				var placep = ep.SelectSingleNode("b");
				string place = null;
				string street = null;
				string placestring = null;
				if (placep != null)
				{
					placestring = placep.InnerText;
				}
				else
				{
					placestring = AddressExtracter.ExtractAddress(body, AddressExtracter.Mode.WithIn).ElementAtOrDefault(0);
				}
				if (!string.IsNullOrEmpty(placestring))
				{
					var splits = placestring.Split(new string[] { " i " }, StringSplitOptions.RemoveEmptyEntries);
					place = splits.Last().Trim();

					if (splits.Length > 1)
					{
						street = splits.First().Trim();
					}
				}
				else
				{
					continue;
				}

				string dayofweek = GetDayOfWeek(body.ToLower());
				if (dayofweek == null)
				{
					if (lastevent != null)
					{
						dayofweek = lastevent.IncidentTime.Value.DayOfWeek.ToString().ToLower();
					}
					else
					{
						// TODO, this is dangerous, assume reportdate
						dayofweek = _report.ReportDate.Value.DayOfWeek.ToString().ToLower();
					}
				}
				DateTime date = GetNextOfDayOfWeek(_report.ReportDate.Value, dayofweek);

				var findtime = new Regex(@"lokken\ \d{1,2}\.\d{2}");
				DateTime? incidentdate = null;
				var match = findtime.Match(body);
				if (match.Success)
				{
					string timestring = match.Value;
					timestring = timestring.Replace("lokken", "").Trim();
					DateTime timedate = DateTime.ParseExact(timestring, "H.mm", null);
					incidentdate =  new DateTime(date.Year, date.Month, date.Day, timedate.Hour, timedate.Minute, 0);
				}
				else
				{
					// try with just hour
					var hourtime = new Regex(@"lokken \d{1,2}");
					var hourmatch = hourtime.Match(body);
					if (hourmatch.Success)
					{
						string timestring = hourmatch.Value;
						timestring = timestring.Replace("lokken", "").Trim();
						int hour = int.Parse(timestring);
						incidentdate = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
					}

					if (incidentdate == null)
					{
						incidentdate = date;
						_logger.Info("No time in:");
						_logger.Info(body);
					}
				}

				if (incidentdate != null)
				{
					lastevent = new Event
					{
						Title = title,
						Street = street,
						ReportId = _report.ReportId,
						PlaceString = place,
						IncidentTime = incidentdate,
						BodyText = body
					};
					yield return lastevent;
				}
			}
		}

		private IEnumerable<Event> GetMainEvents()
		{
			var span = _document.Value.DocumentNode.SelectSingleNode("//span[@id='Articlewithindexpagecontrol_XMLliste1']/span");

			Func<HtmlNode, HtmlNode> getbodyfromtitle = null;

			// [@face='Verdana, serif' or @face='Verdana, sans-serif' or @face='Calibri, serif']

			// fish out the known headlines 
			if (span == null)
			{
				yield break;
			}
			var titlebs = span.SelectNodes(
				"p[@style='MARGIN-BOTTOM: 0cm']/font/font[@size='2' or @size='3']/b");
			getbodyfromtitle = b => b.ParentNode.ParentNode.ParentNode.NextSibling.NextSibling.NextSibling;
			string placeselector = "font/font/b";
			if (titlebs == null)
			{
				// [@face='Verdana, serif']
				titlebs = span.SelectNodes(
					"p[@style='MARGIN-BOTTOM: 0cm']/font/b");
				getbodyfromtitle = b => b.ParentNode.ParentNode.NextSibling.NextSibling.NextSibling;
				placeselector = "font/b";

				if (titlebs == null)
				{
					titlebs = span.SelectNodes(
						"p[@class='MsoNormal']/b");
					getbodyfromtitle = b => b.ParentNode.NextSibling.NextSibling.NextSibling;
					placeselector = "b";

					if (titlebs == null)
					{
						// [@face='Verdana']
						titlebs = span.SelectNodes(
							"p[@dir='ltr']/span[@lang='da']/b/font");
						getbodyfromtitle = b => b.ParentNode.ParentNode.ParentNode.NextSibling.NextSibling.NextSibling;
						// [@face='Verdana']
						placeselector = "span[@lang='da']/n/font";
					}
				}
			}
			Event lastevent = null;

			if (titlebs == null)
			{
				yield break;
			}

			foreach (var b in titlebs)
			{

				if (b.PreviousSibling != null || b.NextSibling != null)
				{
					continue;
				}

				string title = b.InnerText;
				
				
				if (title.Contains("Ringstedvej"))
				{
					continue;
				}
				var p = getbodyfromtitle(b);
				string body = p.InnerText;

				string placestring = null;
				
				if (placeselector == null)
				{
					placestring = AddressExtracter.ExtractAddress(body, AddressExtracter.Mode.WithIn).
						ElementAtOrDefault(0);
				}
				else
				{
					var placeelem = p.SelectSingleNode(placeselector);
					if (placeelem == null)
					{
						// we get too many with xpath, ignore
						continue;
					}

					placestring = placeelem.InnerText;
				}

				if (placestring == null)
				{
					continue;
				}

				var splits = placestring.Split(new string[]{" i "}, StringSplitOptions.RemoveEmptyEntries);
				string place = splits.Last().Trim();

				string street = null;
				if (splits.Length > 1)
				{
					street = splits.First().Trim();
				}

				string dayofweek = GetDayOfWeek(body.ToLower());
				if(dayofweek == null)
				{
					if(lastevent != null)
					{
						dayofweek = lastevent.IncidentTime.Value.DayOfWeek.ToString().ToLower();
					}
					else
					{
						// TODO, this is dangerous, assume reportdate
						dayofweek = _report.ReportDate.Value.DayOfWeek.ToString().ToLower();
					}
				}
				DateTime date = GetNextOfDayOfWeek(_report.ReportDate.Value, dayofweek);

				var findtime = new Regex(@"lokken\ \d{1,2}\.\d{2}");
				DateTime? incidentdate = null;
				var match = findtime.Match(body);
				if (match.Success)
				{
					string timestring = match.Value;
					timestring = timestring.Replace("lokken", "").Trim();
					DateTime timedate = DateTime.ParseExact(timestring, "H.mm", null);
					incidentdate =  new DateTime(date.Year, date.Month, date.Day, timedate.Hour, timedate.Minute, 0);
				}
				else
				{
					// try with just hour
					var hourtime = new Regex(@"lokken \d{1,2}");
					var hourmatch = hourtime.Match(body);
					if (hourmatch.Success)
					{
						string timestring = hourmatch.Value;
						timestring = timestring.Replace("lokken", "").Trim();
						int hour = int.Parse(timestring);
						incidentdate = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
					}

					if (incidentdate == null)
					{
						incidentdate = date;
						_logger.Info("No time in:");
						_logger.Info(body);
					}
				}

				if(incidentdate != null)
				{
					lastevent = new Event
					{
						BodyText = body,
						Title = title,
						Street = street,
						ReportId = _report.ReportId,
						PlaceString = place,
						IncidentTime = incidentdate,
					};
					yield return lastevent;
				}

			}
		}

		private string GetDayOfWeek(string s)
		{
			if (s.Contains("mandag"))
			{
				return "monday";
			}
			else if (s.Contains("tirsdag"))
			{
				return "tuesday";
			}
			else if (s.Contains("onsdag"))
			{
				return "wednesday";
			}
			else if (s.Contains("torsdag"))
			{
				return "thursday";
			}
			else if (s.Contains("fredag"))
			{
				return "friday";
			}
			else if (s.Contains("lørdag"))
			{
				return "saturday";
			}
			else if (s.Contains("søndag"))
			{
				return "sunday";
			}
			else
			{
				return null;
				//throw new ArgumentException(s);
			}
		}

		private DateTime GetNextOfDayOfWeek(DateTime start, string dayofweek)
		{
			for (int i = 0; i < 10; i++)
			{
				DateTime trydate = start.AddDays(i);
				if (trydate.DayOfWeek.ToString(CultureInfo.GetCultureInfo("da-DK")).ToLower() == dayofweek)
				{
					return trydate;
				}

			}
			throw new ArgumentException(dayofweek);
		}

		public override DateTime GetReportDate(string s, DateTime publishedDate)
		{
			try
			{
				s = s.Replace("2010", "").Replace("2009", "").
					Replace("Uddrag af døgnrapporten fra", "").
					Replace("Uddrag af døgnrapporten for", "").
					Replace("Døgnrapport fra", "").
					Replace("Døgnrapporten for", "").
					Replace("Døgnrapport for", "").Replace("Døgnrappport for", "").
					Replace("Døgnrapport", "").Replace("Døgrapport for", "").
					Trim();
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

				string datestring = null;

				if (splitter != null)
				{
					var split = s.Split(
						new string[] { splitter },
						StringSplitOptions.RemoveEmptyEntries).
						Select(_ => _.Trim());
					if (split.First().EndsWith("."))
					{
						// they are on same month
						string month = s.Split(' ').Last();
						datestring = split.First().Replace(".", "") + month;
					}
					else
					{
						// diff months, get first month
						string thedate = split.First().Split('.').First().Trim();
						string month = split.First().Split('.').Last().Trim();
						datestring = thedate + month;
					}
				}
				else
				{
					string thedate = s.Split('.').First().Split(' ').Last().Trim();
					string month = s.Split(' ').Last();
					datestring = thedate + month;
				}

				if (datestring.Contains(" den "))
				{
					datestring = datestring.Split(new string[] {" den "}, 
						StringSplitOptions.RemoveEmptyEntries).Last().Trim();
				}

				var date = DateTime.ParseExact(datestring,
					"dMMMM",
					CultureInfo.GetCultureInfo("da-DK"));

				return new DateTime(publishedDate.Year, date.Month, date.Day);
			}
			catch (Exception e)
			{
				//logger.Info(s);
				throw;
			}
		}

		static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] a = s.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
	}
}

//string firstdate =
//    s.Split(new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries).
//    First().Trim().Replace("Døgnrapport for", "").
//        Replace(".", "").Replace(" ", "");

