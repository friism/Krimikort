using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using Xunit;

namespace EB.Crime.Downloader.Tests
{
	public class SouthSealandParserTests
	{
		private DatabaseDataContext _db;
		private Precinct _precinct;
		private ReportManager _reportManager;

		public SouthSealandParserTests()
		{
			_db = new DatabaseDataContext();
			_precinct = _db.Precincts.Single(x => x.UrlName == "Sydsjaelland");
			_reportManager = new ReportManager(_precinct);
		}

		[Fact]
		public void Test()
		{
			var report = _reportManager.GetReport("/Sydsjaelland/da/lokalnyt/Doegnrapporter/Døgnrapport_09-110312.htm", new DateTime(2012, 3, 12), "Døgnrapport for 9.-11. marts");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test2()
		{
			var report = _reportManager.GetReport("/Sydsjaelland/da/lokalnyt/Doegnrapporter/Døgnrapport_220312.htm", new DateTime(2012, 3, 23), "Døgnrapport for 22. marts");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test3()
		{
			var report = _reportManager.GetReport("/Sydsjaelland/da/lokalnyt/Doegnrapporter/Døgnrapport_210312.htm", new DateTime(2012, 3, 22), "Døgnrapporten for 21. marts");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test4()
		{
			var report = _reportManager.GetReport("/Sydsjaelland/da/lokalnyt/Doegnrapporter/Døgnrapport_010312.htm", new DateTime(2012, 3, 2), "Døgnrapport for 1. marts 2012 ");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test5()
		{
			var report = _reportManager.GetReport("/Sydsjaelland/da/lokalnyt/Doegnrapporter/Døgnrapport_130409.htm", new DateTime(2009, 4, 14), "Døgnrapport for 13. april");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test6()
		{
			var report = _reportManager.GetReport("/Sydsjaelland/da/lokalnyt/Doegnrapporter/Døgnrapport_300909.htm", new DateTime(2009, 10, 1), "Døgnrapport for 30. september");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}
	}
}
