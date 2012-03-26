using System;
using System.Collections.Generic;
using EB.Crime.DB;
using EB.Crime.Downloader.Util;
using HtmlAgilityPack;
using NLog;

namespace EB.Crime.Downloader.Parsers
{
	public abstract class Parser
	{
		protected Report _report;
		protected Lazy<HtmlDocument> _document;
		protected Logger _logger;

		public Parser(Report report)
		{
			_report = report;
			_document = new Lazy<HtmlDocument>(() => Doc.LoadDoc(_report.Html));
			_logger = LogManager.GetCurrentClassLogger(); ;
		}

		public abstract IEnumerable<Event> GetEvents();
		public abstract bool IsAppropriateFor();
		public abstract DateTime GetReportDate(string reportTitle, DateTime publishedDate);
	}
}
