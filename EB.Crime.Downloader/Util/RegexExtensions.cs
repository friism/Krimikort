using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EB.Crime.Downloader.Util
{
	public static class RegexExtensions
	{
		public static IEnumerable<string> GetMatches(this Regex regex, string s)
		{
			return regex.Matches(s)
				.OfType<Match>()
				.Select(_ => _.Value.Trim())
				.Where(_ => _ != "politigården")
				.Distinct();
		}
	}
}
