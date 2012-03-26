using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EB.Crime.Downloader.Util
{
	public class AddressExtracter
	{
		private static string baseAddressRegString =
			
				// these are endings we are sure are placenames (even if they start with lower)
				@"((\w+(gade|vej|parken|vænget|vejen|vang|gaden|toften|porten|" + 
					@"holmen|marken|boulevarden|hegnet|gården|haven|stræde|bakken|" +
					@"torvet|lund|vænge))" +
				
				// these are slightly more dubious and must start with Cap
				@"|(\ ([A-Z]|Ø|Æ|Å)\w*(ager|stien|længen|svinget|højen|bjerg|buen|dalen|" +
					@"dybet|sletten|draget|holm|høj|dammen|alle|kær|løbet|skoven|stykket|" +
					@"engen|diget|kæret|bugten|leddet|mosen|krattet|centret|dyssen|dal|" +
					@"strøget|lodden|bro|hegn|passagen|gangen|skellet|ly|mark|splanaden|by|" +
					@"center))" +

				// these signify multiword places
				@"|((\ ([A-Z]|Ø|Æ|Å)(\w|-|')*){1,2}\ (Allé|Boulevard|Torv|Centret|Plads|Havn|Bypark|" +
					@"Storcenter|Bycenter|Centrum|Alle|Parkallé|Stationstorv|Station|Vænge|" +
					@"Søpark|Butikscenter|Skolen|Vej|alle|Have|Hospital|S-togsstation|Stadion|" +
					@"Strand|Færgehavn|station|Center|Gade|Kirke|havn)))" +

				// custom ones
				@"|(Christania|lufthavnen|hovedbanegården|City2|City\ 2|Christiania|Lygten)";
				
		private static Regex classicAddressReg = new Regex(
			baseAddressRegString + "@\b");

		private static Regex inAddressReg = new Regex(
			"(" + baseAddressRegString + ")" +
				@" i ([A-Z]|Ø|Æ|Å)\w*\b"); // Unfortunately doesn't handle multi-word places

		public static IEnumerable<string> ExtractAddress(string s, Mode mode)
		{
			switch (mode)
			{
				case Mode.Classic: return classicAddressReg.GetMatches(s);
				case Mode.WithIn: return inAddressReg.GetMatches(s);
				default: throw new ArgumentException(mode.ToString());
			}
		}


		public enum Mode { Classic, WithIn };
	}
}
