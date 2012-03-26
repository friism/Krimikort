using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using EB.Crime.Downloader.Util;

namespace EB.Crime.Downloader
{
	public class ReportManager
	{
		private Precinct _precinct;

		public ReportManager(Precinct precinct)
		{
			_precinct = precinct;
		}

		public Report GetReport(string url, DateTime reportDate, string titlestring)
		{
			var db = new DatabaseDataContext();
			var report = db.Reports.SingleOrDefault(_ =>
				_.Uri == url);

			if (report == null)
			{
				report = new Report();
				report.CreatedAt = DateTime.Now;
				report.Uri = url;

				report.PrecinctId = _precinct.PrecinctId;
				// evil
				report.Html = "";

				db.Reports.InsertOnSubmit(report);
				try
				{
					db.SubmitChanges();
				}
				catch (Exception e)
				{
					Console.WriteLine("Something wrong {0}", report.Uri);
					return null;
				}
			}

			if (string.IsNullOrEmpty(report.Html))
			{
				Console.WriteLine("retrieving {0}", url);
				report.Html = Doc.GetHtml("http://politi.dk" + url);
			}

			if (report.ReportDate == null)
			{
				var parser = ParserFactory.GetParser(report);
				report.ReportDate = parser.GetReportDate(titlestring, reportDate);
			}

			try
			{
				db.SubmitChanges();
			}
			catch (Exception e)
			{
				return report; // due to retarded date
			}

			return report;
		}

		public void ExamineEmptyReports()
		{
			var db = new DatabaseDataContext();
			var reports = (from r in db.Reports
						   where
							 r.PrecinctId == _precinct.PrecinctId &&
							 !r.Events.Any() &&
							 r.ReportDate.Value.Date > _precinct.Cutoff.Value.Date &&
							 r.ReportDate.Value.Date <= DateTime.Now.Date
						   select r).ToList();
			var events = reports.SelectMany(_ => ParserFactory.GetParser(_).GetEvents());
			db.Events.InsertAllOnSubmit(events);
			db.SubmitChanges();
		}
	}
}
