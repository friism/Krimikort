using System;
using System.Globalization;
using System.Linq;

namespace EB.Crime.Downloader.Util
{
	class DateParser
	{
		public static DateTime? ParseDate(string s, DateTime reportdate)
		{
			if (s == null)
				return null;

			s = s.Replace("den", "").
							Replace("d. ", "").
							Replace("&nbsp;", "").
							Replace(",", "").
							ToLower().
							Replace("kl. ", "").
							Replace("kl.", "").
							Replace(" fra ca", "").
							Replace(" ca", "").
							Replace(".", "").
							Replace(",", "").
							Replace("  ", " ").
				// some bugfixes
							Replace("febrauar", "februar").
							Replace("septemeber", "september").
							Replace("20009", "2009").
							Replace("den", "").
							Replace(" en", "").

							Trim();
			// remove day of week
			s = string.Join(" ", s.Split(' ').Skip(1));
			if (s.Contains('-'))
			{
				s = s.Substring(0, s.IndexOf('-'));
			}
			if (s.Contains('–'))
			{
				s = s.Substring(0, s.IndexOf('–'));
			}
			s = s.Trim().TrimEnd('.', 't').ToLower().Trim();

			try
			{
				DateTime dt =
					DateTime.ParseExact(s,
						"d MMMM yyyy HHmm",
						CultureInfo.GetCultureInfo("da-DK"));
				return dt;
			}
			catch (Exception)
			{
				try
				{
					DateTime dt =
						DateTime.ParseExact(s,
							"d MMMM HHmm",
							CultureInfo.GetCultureInfo("da-DK"));
					return new DateTime(reportdate.Year,
						dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
				}
				catch (Exception)
				{
					try
					{
						DateTime dt =
							DateTime.ParseExact(s,
								"d MMMM yyyy",
								CultureInfo.GetCultureInfo("da-DK"));
						return dt;
					}
					catch (Exception)
					{
						return null;
					}
				}
			}
		}
	}
}
