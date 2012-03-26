using System;
using System.Linq;
using EB.Crime.DB;
using EB.Crime.Downloader.Parsers;
using Xunit;

namespace EB.Crime.Downloader.Tests
{
	public class NorthSealandTests
	{
		private DatabaseDataContext _db;
		private Precinct _precinct;
		private ReportManager _reportManager;

		public NorthSealandTests()
		{
			_db = new DatabaseDataContext();
			_precinct = _db.Precincts.Single(x => x.UrlName == "Nordsjaelland");
			_reportManager = new ReportManager(_precinct);
		}

		//[Fact]
		//public void Test()
		//{
		//    var report = _reportManager.GetReport("/Nordsjaelland/da/lokalnyt/Doegnrapporter/doegnrapport_160312.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
		//}

		//[Fact]
		//public void Test2()
		//{
		//    // TODO: Get street out of these
		//    var report = _reportManager.GetReport("/NR/exeres/425B0418-67BF-42F6-86C3-565879D46474.htm", DateTime.Now);
		//    var parser = ParserFactory.GetParser(report);
		//    var foo = parser.GetEvents().ToList();
			
		//}
	}
}
