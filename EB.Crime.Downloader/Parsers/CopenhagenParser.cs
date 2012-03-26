using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Util;

namespace EB.Crime.Downloader.Parsers
{
	public class CopenhagenParser : Parser
	{
		public CopenhagenParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Koebenhavn")
			{
				return false;
			}
			return true;
		}

		public override IEnumerable<Event> GetEvents()
		{
			Console.WriteLine("Extracting events from {0}", _report.Uri);

			var span = _document.Value.DocumentNode.SelectSingleNode("//span[@id='Articlewithindexpagecontrol_XMLliste1']/span");

			Event e = null;
			string rawhtml = null;
			string bodytext = null;
			bool? ignorenext = null;
			bool dontrecord = false;
			foreach (var n in span.ChildNodes)
			{
				if (n.Name == "h3")
				{
					e = new Event();
					e.ReportId = _report.ReportId;
					dontrecord = false;
					rawhtml = n.OuterHtml;
					bodytext = "";
					// followed by date, should not go in bodytext
					ignorenext = true;

					var headstring = n.InnerText;
					var title = headstring.Split(',')[0].Trim();
					if (headstring.Contains(','))
					{
						var street = headstring.Split(',')[1].Trim();
						string street2 = null;
						if (street.Contains('/'))
						{
							street2 = street.Split('/')[1].Trim();
							street = street.Split('/')[0].Trim();
						}
						e.Street = street;
						e.StreetSecondary = street2;
					}

					e.Title = title;
					e.PlaceString = "København"; // as good as it gets

				}
				else if (n.Name == "#text")
				{
					if (!ignorenext.Value)
					{
						bodytext += n.InnerText + "\n";
					}
					else
					{
						// we're in the datetime thingy

						e.IncidentTime = DateParser.ParseDate(n.InnerText, _report.ReportDate.Value);
						if (e.IncidentTime == null)
						{
							if (n.InnerText.Length < 100 && n.InnerText.Length > 2)
							{
								_logger.Info(string.Format("Could not parse {0}", n.InnerText));
							}
							dontrecord = true;
						}
						ignorenext = false;
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
					e.BodyText = bodytext.Replace("&nbsp;", " ").Trim();

					if (string.IsNullOrEmpty(e.Street) || string.IsNullOrEmpty(e.StreetSecondary))
					{
						var streets = AddressExtracter.ExtractAddress(e.BodyText, AddressExtracter.Mode.Classic);
						if (string.IsNullOrEmpty(e.Street))
						{
							e.Street = streets.ElementAtOrDefault(0);
							e.StreetSecondary = streets.ElementAtOrDefault(1);
						} else if (string.IsNullOrEmpty(e.StreetSecondary))
						{
							e.StreetSecondary = streets.ElementAtOrDefault(0);
						}
					}

					if ((!dontrecord || !string.IsNullOrEmpty(e.BodyText.Trim())) && e.IncidentTime.HasValue)
					{
						yield return e;
					}
					else
					{
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
			try
			{
				string reportdatestring = s.Split(' ').Last();
				if (reportdatestring.Contains('/'))
				{
					reportdatestring = reportdatestring.Split('/').Last();
				}
				try
				{
					return DateTime.ParseExact(reportdatestring, "dd-MM-yyyy", null);
				}
				catch (Exception e7)
				{
					try
					{
						return DateTime.ParseExact(reportdatestring, "dd-MM-yy", null);
					}
					catch (Exception e8)
					{
						return DateTime.ParseExact(reportdatestring, "d-MM-yyyy", null);
					}
				}
			}
			catch (Exception e)
			{
				try
				{
					string reportdatestring =
						s.Split(new string[] { " den " },
							StringSplitOptions.RemoveEmptyEntries).Last().Trim('.');
					if (reportdatestring.Contains(" og "))
					{
						reportdatestring =
							reportdatestring.Split(new string[] { " og " },
							StringSplitOptions.RemoveEmptyEntries).Last().Trim();
					}
					return DateTime.ParseExact(reportdatestring,
						"d. MMMM yyyy", CultureInfo.GetCultureInfo("da-DK"));
				}
				catch (Exception ee)
				{
					try
					{
						string reportdatestring =
							s.Split(new string[] { " den " },
								StringSplitOptions.RemoveEmptyEntries).Last().Trim('.');
						return DateTime.ParseExact(reportdatestring,
							"d-MM yyyy", CultureInfo.GetCultureInfo("da-DK"));
					}
					catch (Exception eee)
					{
						try
						{
							string reportdatestring =
								s.Split('-').Last().Trim();
							return DateTime.ParseExact(reportdatestring,
								"d. MMMM yyyy", CultureInfo.GetCultureInfo("da-DK"));
						}
						catch (Exception eeee)
						{
							try
							{
								return DateTime.ParseExact(s, "dd-MM-yy", null);
							}
							catch (Exception eeeee)
							{
								try
								{
									string reportdatestring =
										s.Split(new string[] { " den " },
										StringSplitOptions.RemoveEmptyEntries).Last().Trim();
									return DateTime.ParseExact(reportdatestring,
										"dd-MM-yy", CultureInfo.GetCultureInfo("da-DK"));
								}
								catch (Exception e6)
								{
									try
									{
										string reportdatestring =
											s.Split(new string[] { " den " },
											StringSplitOptions.RemoveEmptyEntries).Last().Trim();
										return DateTime.ParseExact(reportdatestring,
											"dd-MM-yyyy", CultureInfo.GetCultureInfo("da-DK"));

									}
									catch (Exception e7)
									{
										throw;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
