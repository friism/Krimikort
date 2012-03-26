using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using Xunit;

namespace EB.Crime.Downloader.Tests
{
	public class FunenParserTestsTests
	{
		private DatabaseDataContext _db;
		private Precinct _precinct;
		private ReportManager _reportManager;

		public FunenParserTestsTests()
		{
			_db = new DatabaseDataContext();
			_precinct = _db.Precincts.Single(x => x.UrlName == "Fyn");
			_reportManager = new ReportManager(_precinct);
		}

		[Fact]
		public void Test()
		{
			var report = _reportManager.GetReport("/Fyn/da/lokalnyt/Doegnrapporter/Doegnrapport_22032012.htm", new DateTime(2012, 3, 23), "Uddrag af døgnrapporten for torsdag den 22. marts 2012");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}

		[Fact]
		public void Test2()
		{
			var report = _reportManager.GetReport("/Fyn/da/lokalnyt/Doegnrapporter/dognrapport_16022012.htm", new DateTime(2012, 2, 17), "Uddrag af døgnrapporten for torsdag den 16. februar 2012");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}
	}
}
