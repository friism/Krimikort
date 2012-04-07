using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EB.Crime.DB;
using HtmlAgilityPack;

namespace EB.Crime.Downloader.Parsers
{
	public class NewFunenParser : NewSouthSealandParser
	{
		public NewFunenParser(Report report)
			: base(report)
		{
		}

		public override bool IsAppropriateFor()
		{
			if (_report.Precinct.UrlName != "Fyn")
			{
				return false;
			}
			return true;
		}

		public override IEnumerable<Event> GetEvents()
		{
			return GetEvents(new FunenAddressExtractor());
		}

		protected IEnumerable<Event> GetEvents(FunenAddressExtractor extractor)
		{
			if (_report.Uri == "/Fyn/da/lokalnyt/Doegnrapporter/Uddrag+af+døgnrapporten+for+tirsdag+den+11.+maj+2010.htm")
			{
				// this is a duplicate
				yield break;
			}

			var span = _document.Value.DocumentNode.SelectSingleNode(
				"//span[@id='Articlewithindexpagecontrol_XMLliste1']/span");

			var ns = (span.SelectNodes("p[@class='MsoNormal']") ?? Enumerable.Empty<HtmlNode>()).
				Concat((span.SelectNodes("span") ?? Enumerable.Empty<HtmlNode>()));

			if (ns == null || ns.Count() == 0)
			{
				// this is for East Jutland, which uses more or less the same code
				ns = span.SelectNodes("p");
			}

			if (ns == null || ns.Count() == 0)
			{
				yield break;
			}

			ns = ns.Where(n => n.InnerText.Contains("Sket: "));

			foreach (var n in ns)
			{
				var texters = new LinkedList<HtmlNode>(
					n.DescendantNodes().Where(_ => _.Name.ToLower() == "#text" && 
						!string.IsNullOrEmpty(_.InnerText.Trim())));

				IEnumerable<HtmlNode> addressnodes = null;
				try
				{
					addressnodes = extractor.GetAddressNodes(texters);

					if (addressnodes == null || addressnodes.Count() != 1)
						continue;
				}
				catch (Exception e)
				{
					continue;
				}

				var addressnodell = texters.Find(addressnodes.Single());
				if (addressnodell.Previous == null)
				{
					// no title
					continue;
				}
				var titlenode = texters.Find(addressnodes.Single()).Previous.Value.InnerText;

				var timenodes = texters.Where(_ => _.InnerText.Trim().StartsWith("Sket: "));

				if (timenodes.Count() > 1)
				{
					continue;
				}

				var timenode = timenodes.Single();
				string body = null;
				if (texters.Find(timenode).Next != null)
				{
					body = texters.Find(timenode).Next.Value.InnerText;
				}
				else
				{
					// no body, too bad
				}
				var addressminuspostcode = extractor.GetAddressNodesMinusPostcode(addressnodes);

				var splits = extractor.SplitAddress(addressminuspostcode);
				var placestring = splits.First();

				string street = null;
				if (splits.Count() > 1)
				{
					street = splits.Skip(1).Aggregate((a, b) => a + " - " + b);
				}

				var timestring = timenode.InnerText.Replace("Sket: ", "").
					Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries).
					First().Trim()
					.ToLower()
					.Split(new [] { "til" }, StringSplitOptions.RemoveEmptyEntries)
					.First().Trim();

				DateTime? time = null;
				try
				{
					time = DateTime.ParseExact(timestring, "dd/MM/yyyy HH:mm",
						CultureInfo.InvariantCulture);
					if (time.Value.Year < 1991)
					{
						continue;
					}
				}
				catch(Exception e)
				{
					_logger.Info("Couldn't parse " + timestring);
					continue;
				}
				if (!string.IsNullOrEmpty(body))
				{
					yield return new Event
					{
						BodyText = body.Replace("&nbsp;", ""),
						IncidentTime = time,
						PlaceString = placestring,
						Street = street,
						Title = titlenode,
						ReportId = _report.ReportId,
					};
				}
			}
		}
	}

	public class FunenAddressExtractor
	{
		public virtual IEnumerable<HtmlNode> GetAddressNodes(LinkedList<HtmlNode> nodes)
		{
			int postcode = 0;
			return nodes.Where(_ => int.TryParse(_.InnerText.Split(' ').First(), out postcode) &&
					postcode > 999);
		}

		public virtual string GetAddressNodesMinusPostcode(IEnumerable<HtmlNode> addressnodes)
		{
			return addressnodes.Single().InnerText.Split(' ').Skip(1).
					Aggregate((a, b) => a + " " + b).Replace("&nbsp;", " ");
		}

		public virtual IEnumerable<string> SplitAddress(string address)
		{
			return address.Split(new string[] { " - ", " – " },
						StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
