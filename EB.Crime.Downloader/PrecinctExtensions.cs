using System;
using System.Collections.Generic;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using EB.Crime.Downloader.Util;

namespace EB.Crime.Downloader
{
	public static class PrecinctExtensions
	{
		public static string LandingUrl(this Precinct precinct)
		{
			return string.Format("http://www.politi.dk/{0}/da/lokalnyt/Doegnrapporter/",
				precinct.UrlName);
		}

		public static IEnumerable<Report> GetIncremental(this Precinct precinct)
		{
			var links = OverviewParser.GetLinksByUrl(precinct.LandingUrl());

			var db = new DatabaseDataContext();

			var reportManager = new ReportManager(precinct);

			var reports = links.
				// filter out ones that are in db already
				Where(l =>
					// reports not in db
					!db.Reports.Any(r => r.Uri == l.Item1 &&
						precinct.PrecinctId == r.PrecinctId && !r.Events.Any())
					)
					.Select(l => reportManager.GetReport(l.Item1, l.Item2, l.Item3))
					.Where(x => x != null);

			ExtractAndInsert(reports, db);

			db.SubmitChanges();

			return reports;
		}

		public static void GetArchiveFromArchiveHtml(this Precinct precinct)
		{
			var db = new DatabaseDataContext();
			// get links from archivehtml

			var links = OverviewParser.GetlinksByHtml(Doc.GetArchiveHtmlByPost(precinct.LandingUrl()));
			var reportManager = new ReportManager(precinct);
			// create reports and download reporthtml in parallel
			var reports = links
				.AsParallel()
				//SelectMany(_ => _.Value.Select(v => GetReport(v, _.Key)));
				.Select(pr => reportManager.GetReport(pr.Item1, pr.Item2, pr.Item3))
				.Where(x => x != null);

			// parse events from reports in parallel
			var relevantReports = reports.
				Where(_ => _.ReportDate.HasValue && _.ReportDate.Value.Date > precinct.Cutoff.Value.Date &&
					_.ReportDate.Value.Date <= DateTime.Now.Date && !_.Events.Any());

			ExtractAndInsert(relevantReports, db);

			db.SubmitChanges();
		}

		private static void ExtractAndInsert(IEnumerable<Report> reports, DatabaseDataContext db)
		{
			var eventsBatches = from r in reports
								let parser = ParserFactory.GetParser(r)
								where parser != null
								select parser.GetEvents();

			foreach (var batch in eventsBatches)
			{
				foreach (var @event in batch)
				{
					@event.CreatedAt = DateTime.Now;
				}

				db.Events.InsertAllOnSubmit(batch);
				db.SubmitChanges();
			}
		}

		//public static void GetArchiveFromDBReports(this Precinct precinct)
		//{
		//    var db = new DatabaseDataContext();
		//    var reports = db.Reports.Where(r => r.PrecinctId == precinct.PrecinctId);

		//    // parse events from reports in parallel
		//    var events = reports.
		//        AsParallel().
		//        Where(_ => _.ReportDate.Date > precinct.Cutoff.Value.Date &&
		//            _.ReportDate.Date <= DateTime.Now.Date).
		//        SelectMany(_ => ParserFactory.GetParser(_).GetEvents());
		//    db.Events.InsertAllOnSubmit(events);
		//    db.SubmitChanges();

		//    // make sure new ones get permid
		//    foreach (var e in db.Events.Where(_ => _.PermId == null))
		//    {
		//        e.PermId = e.EventId;
		//    }

		//    db.SubmitChanges();
		//}

	}
}
