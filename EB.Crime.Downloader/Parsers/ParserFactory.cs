using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EB.Crime.DB;

namespace EB.Crime.Downloader.Parsers
{
	public static class ParserFactory
	{
		private static IEnumerable<Type> _parserTypes = Assembly.GetAssembly(typeof(Parser)).GetTypes()
			.Where(type => type.IsSubclassOf(typeof(Parser)));

		public static Parser GetParser(Report report)
		{
			var parser = _parserTypes
				.Select(x => (Parser)Activator.CreateInstance(x, report))
				.SingleOrDefault(x => x.IsAppropriateFor());
			if (parser == null)
			{
				Console.WriteLine("No parser for {0}", report.Uri);
			}
			return parser;
		}
	}
}
