using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using Xunit;

namespace EB.Crime.Downloader.Tests
{
	public class CopenhagenTests
	{
		private DatabaseDataContext _db;
		private Precinct _precinct;
		private ReportManager _reportManager;

		public CopenhagenTests()
		{
			_db = new DatabaseDataContext();
			_precinct = _db.Precincts.Single(x => x.UrlName == "Koebenhavn");
			_reportManager = new ReportManager(_precinct);
		}

		//[Fact]
		//public void Test()
		//{
		//    var report = _reportManager.GetReport("/Koebenhavn/da/lokalnyt/Doegnrapporter/Doegnrapport_150312.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
		//}

		//[Fact]
		//public void Test2()
		//{
		//    var report = _reportManager.GetReport("/Koebenhavn/da/lokalnyt/Doegnrapporter/doegnrapport_020212.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
		//}
	}
}
