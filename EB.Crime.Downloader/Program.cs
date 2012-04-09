using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using EB.Crime.DB;
using EB.Crime.Downloader.GeoCoding;
using EB.Crime.Downloader.Util;
using NLog;

namespace EB.Crime.Downloader
{
	public class CrimeDownloader
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger();
		private static string[] currentPrecincts = new[] {
				"Koebenhavn", "Oestjylland", "Fyn", "Sydsjaelland", "Midtvestsjaelland", "Nordsjaelland", "Vestegnen" };

		static void Main(string[] args)
		{
			while (true)
			{
				try
				{
					var db = new DatabaseDataContext();
					if(db.Events.Any(x => x.CreatedAt > DateTime.Now.AddDays(-1)))
					{
						Thread.Sleep(1000 * 60 * 60 * 24);
						continue;
					}

					DoArchiveHtml();

					var categorizer = new Categorizer();
					categorizer.Categorize();

					var geocoder = new EventGeocoder();
					try
					{
						geocoder.GeoCodeEvents();
					}
					catch (GoogleIsSulkingException)
					{
					}
				}
				catch (Exception exception)
				{
					using (var smtpClient = new SmtpClient())
					{
						var emailAddress = ConfigurationManager.AppSettings.Get("logemailaddress");
						smtpClient.Send(new MailMessage(
							emailAddress, emailAddress, "Problem scraping", exception.ToString()));
					}
				}
			}
		}

		static void PrintEvents(IEnumerable<Event> events)
		{
			foreach (var e in events)
			{
				Console.WriteLine(string.Format("Title: {0}, Place: {1}, Time: {2}, Street: {3}",
					e.Title, e.PlaceString, e.IncidentTime, e.Street));
				Console.WriteLine(e.BodyText);

				Console.WriteLine("--------------");
			}
		}

		static void DoArchive()
		{
			//(new CopenhagenParser()).GetArchiveFromDBReports();
			//(new WestendParser()).GetArchiveFromDBReports();
			//(new MidSealandParser()).GetArchiveFromDBReports();
			//(new NorthJutlandParser()).GetArchiveFromDBReports();
			//(new NorthSealandParser()).GetArchiveFromDBReports();
			//(new SouthSealandParser()).GetArchiveFromDBReports();
			//(new FunenParser()).GetArchiveFromDBReports();
			//(new EastJutlandParser()).GetArchiveFromDBReports();
			//(new MidWestJutlandParser()).GetArchiveFromDBReports();
			//Parser.Categorize();
			//Parser.GeoCodeEvents();
		}

		static void DoArchiveHtml()
		{
			var db = new DatabaseDataContext();
			var precincts = db.Precincts.ToList().Where(x => currentPrecincts.Contains(x.UrlName));

			foreach (var precinct in precincts)
			{
				precinct.GetArchiveFromArchiveHtml();
			}
		}

		static void DoRecentEmptyReports()
		{
			var db = new DatabaseDataContext();
			var precincts = db.Precincts.ToList().Where(x => currentPrecincts.Contains(x.UrlName));

			foreach (var precinct in precincts)
			{
				precinct.GetArchiveFromRecentDBReports();
			}
		}

		//public static void DoScrape()
		//{
		//IEnumerable<Report> reports = (new WestendParser()).GetIncremental();
		//reports = reports.Concat((new CopenhagenParser()).GetIncremental());
		//reports = reports.Concat((new MidSealandParser()).GetIncremental());
		//reports = reports.Concat((new NorthJutlandParser()).GetIncremental());
		//reports = reports.Concat((new NorthSealandParser()).GetIncremental());
		//reports = reports.Concat((new SouthSealandParser()).GetIncremental());
		//reports = reports.Concat((new FunenParser()).GetIncremental());
		//reports = reports.Concat((new EastJutlandParser()).GetIncremental());
		//reports = reports.Concat((new MidWestJutlandParser()).GetIncremental());
		//Parser.Categorize();
		//Parser.GeoCodeEvents();
		//}

		//static void EmptyReports()
		//{
		//(new WestendParser()).ExamineEmptyReports();
		//(new CopenhagenParser()).ExamineEmptyReports();
		//(new MidSealandParser()).ExamineEmptyReports();
		//(new NorthJutlandParser()).ExamineEmptyReports();
		//(new NorthSealandParser()).ExamineEmptyReports();
		//(new SouthSealandParser()).ExamineEmptyReports();
		//(new FunenParser()).ExamineEmptyReports();
		//(new EastJutlandParser()).ExamineEmptyReports();
		//(new MidWestJutlandParser()).ExamineEmptyReports();
		//Parser.Categorize();
		//Parser.GeoCodeEvents();
		//}

		static void ResolveAddresses()
		{
			var db = new DatabaseDataContext();

			foreach (var e in db.Events.Where(_ => _.Street == null || _.Street == ""))
			{
				var streets = AddressExtracter.ExtractAddress(e.BodyText, AddressExtracter.Mode.Classic);
				e.Street = streets.ElementAtOrDefault(0);
				e.StreetSecondary = streets.ElementAtOrDefault(1);
			}
			db.SubmitChanges();
		}

		static void TestTopics()
		{
			var db = new DatabaseDataContext();
			var topics = db.Events.ToArray().Select(_ => _.Title).Distinct();
			Console.WriteLine(string.Format("{0} different topics", topics.Count()));
			foreach (var e in topics)
			{
				Console.WriteLine(e);
			}
			Console.WriteLine("press the any key...");
			Console.ReadKey();
		}

		static void TestAddresses()
		{
			var db = new DatabaseDataContext();

			var addressevents =
				db.Events.Select(_ =>
					new { e = _, streets = AddressExtracter.ExtractAddress(_.BodyText, AddressExtracter.Mode.Classic) }).
					ToList().
					Select(_ => new { e = _.e, streets = _.streets.Distinct() });

			Console.WriteLine(string.Format("{0} with address",
				addressevents.Count(_ => _.streets.Count() > 0)));

			Console.WriteLine(
				string.Format("{0} with more than one address",
					addressevents.Count(_ => _.streets.Count() > 1)));

			foreach (var e in addressevents.Where(_ => _.streets.Count() > 0))
			{
				Console.WriteLine(e.streets.Aggregate((a, b) => a + ", " + b));
			}
			Console.WriteLine("press the any key...");
			Console.ReadKey();
		}
	}
}
