using System;
using System.Linq;
using EB.Crime.DB;

namespace EB.Crime.Downloader.Util
{
	static class CustomCategorizer
	{
		public static Event CustomCategorize(Event e)
		{
			string b = e.BodyText.ToLower();
			if (b.Contains("arabisk") ||
				b.Contains("anden etnisk") ||
				b.Contains("mellemøst") ||
				b.Contains(" mørk i huden"))
			{
				e.Race = 0;
			} else if(b.Contains("svensk ")) {
				e.Race = 1;
			}
			else if (b.Contains("østeuro") ||
			  b.Contains("rumæn") ||
			  b.Contains("polak") ||
			  b.Contains("polsk"))
			{
				e.Race = 2;
			}
			else if (b.Contains("afrikan"))
			{
				e.Race = 3;
			}
			else if (b.Contains("sigøjn") ||
				b.Contains(" roma"))
			{
				e.Race = 4;
			}

			return e;
		}

		private static void PrintWithHighlight(string highlight, string s)
		{
			var nonhighlights = s.Split(new string[] { highlight }, StringSplitOptions.None);

			// print initial component
			Console.Write(nonhighlights.First());
			foreach (var c in nonhighlights.Skip(1).Take(nonhighlights.Count() - 1))
			{
				// print the stuff supposed to be higlighted
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.Write(highlight);
				Console.ResetColor();
				Console.Write(c);
			}

			//Console.Write(nonhighlights.Last());
			Console.WriteLine();
		}
	}
}
