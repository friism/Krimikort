using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using Xunit;

namespace EB.Crime.Downloader.Tests
{
	public class MidSealandParserTests
	{
		private DatabaseDataContext _db;
		private Precinct _precinct;
		private ReportManager _reportManager;

		public MidSealandParserTests()
		{
			_db = new DatabaseDataContext();
			_precinct = _db.Precincts.Single(x => x.UrlName == "Midtvestsjaelland");
			_reportManager = new ReportManager(_precinct);
		}

		[Fact]
		public void Test()
		{
			var report = _reportManager.GetReport("/Midtvestsjaelland/da/lokalnyt/Doegnrapporter/K08_dognrapport_160312.htm", new DateTime(2012, 3, 16), "Døgnrapport 150312 - 160312");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test2()
		{
			var report = _reportManager.GetReport("/Midtvestsjaelland/da/lokalnyt/Doegnrapporter/K08_dognrapport_150312.htm", new DateTime(2012, 3, 15), "Døgnrapport 140312 - 150312");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test3()
		{
			var report = _reportManager.GetReport("/Midtvestsjaelland/da/lokalnyt/Doegnrapporter/K08_dognrapport_100312.htm", new DateTime(2012, 3, 10), "Døgnrapport 090312 - 100312");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test4()
		{
			var report = _reportManager.GetReport("/Midtvestsjaelland/da/lokalnyt/Doegnrapporter/K08_dognrapport_031111.htm", new DateTime(2011, 3, 11), "Døgnrapport 021111 - 031111");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		
	}
}
