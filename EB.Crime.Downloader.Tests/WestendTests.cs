using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using Xunit;

namespace EB.Crime.Downloader.Tests
{
	public class WestendTests
	{
		private DatabaseDataContext _db;
		private Precinct _precinct;
		private ReportManager _reportManager;

		public WestendTests()
		{
			_db = new DatabaseDataContext();
			_precinct = _db.Precincts.Single(x => x.UrlName == "Vestegnen");
			_reportManager = new ReportManager(_precinct);
		}

		[Fact]
		public void ShouldGetLinks()
		{
			var links = OverviewParser.GetLinksByUrl("http://www.politi.dk/Vestegnen/da/lokalnyt/Doegnrapporter/");
			var firstLink = links.First();
			
			//var report = _reportManager.GetReport(firstLink);
			//var parser = ParserFactory.GetParser(report);
			//var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void ShouldParseOldReport()
		{
			//var report = _reportManager.GetReport("/Vestegnen/da/lokalnyt/Doegnrapporter/Doegnrapport_uge_10+2009.htm");
			//var parser = ParserFactory.GetParser(report);
			//var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void TestPost()
		{
			//var url = "/CMS.Politi.Web/Templates/Content/ArchivePage.aspx?NRMODE=Published&NRNODEGUID=%7bB881E735-5851-4F79-B0E0-9D1268E36088%7d&NRORIGINALURL=%2fVestegnen%2fda%2flokalnyt%2fDoegnrapporter%2f&NRCACHEHINT=NoModifyGuest";
			//var response = Doc.GetHtmlByPost(url);
		}

		[Fact]
		public void GetFromArchive()
		{
			_precinct.GetArchiveFromArchiveHtml();
		}

		//[Fact]
		//public void TestNewParser()
		//{
		//    var report = _reportManager.GetReport("/Vestegnen/da/lokalnyt/Doegnrapporter/K10_doegnrapport_uge_09_020312.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
		//}

		//[Fact]
		//public void Test()
		//{
		//    var report = _reportManager.GetReport("/Vestegnen/da/lokalnyt/Doegnrapporter/K10_doegnrapport_uge_09_280211.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
		//}

		//[Fact]
		//public void Test2()
		//{
		//    var report = _reportManager.GetReport("/Vestegnen/da/lokalnyt/Doegnrapporter/K10_doegnrapport_uge_02_090112.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
		//}

		//[Fact]
		//public void Test3()
		//{
		//    var report = _reportManager.GetReport("/Vestegnen/da/lokalnyt/Doegnrapporter/K10_doegnrapport_uge_11_140312.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
		//}
	}
}
