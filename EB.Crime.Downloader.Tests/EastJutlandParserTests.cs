using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using Xunit;

namespace EB.Crime.Downloader.Tests
{
	public class EastJutlandParserTests
	{
		private DatabaseDataContext _db;
		private Precinct _precinct;
		private ReportManager _reportManager;

		public EastJutlandParserTests()
		{
			_db = new DatabaseDataContext();
			_precinct = _db.Precincts.Single(x => x.UrlName == "Oestjylland");
			_reportManager = new ReportManager(_precinct);
		}

		[Fact]
		public void Test()
		{
			var report = _reportManager.GetReport("/Oestjylland/da/lokalnyt/Doegnrapporter/doegnrapport_230312.htm", new DateTime(2012, 3, 23), "Døgnrapport fredag den 23. marts 2012");
			var parser = ParserFactory.GetParser(report);
			var foo = parser.GetEvents().ToList();
		}
	}
}
