using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Web;
using EB.Crime.DB;

namespace EB.Crime.Downloader
{
	public class Geocoder
	{
		private static int sleepinterval = 200;

		public static Position GeoCode(string address)
		{
			return GeoCode(address, true);
		}

		public static Position GeoCode(string address, bool usecache)
		{
			Func<GeoCacheEntry, Position> topos = 
				c => new Position { Lat = c.Lat.Value, Lng = c.Lng.Value };

			address = address.Trim().ToLower();

			GeoCacheEntry entry = null;

			var db = new DatabaseDataContext();
			if (usecache)
			{
				entry = db.GeoCacheEntries.SingleOrDefault(_ => _.AddressText == address);
			}

			if (entry != null)
			{
				if (entry.Lng != null && entry.Lat != null)
				{
					return topos(entry);
				}
				else
				{
					return null;
				}
			}
			else
			{
				// we have to geocode the motherfucker
				var res = CallWSCount(address, 0);
				if (res.Status == "OK")
				{
					var e = new GeoCacheEntry
						{
							AddressText = address,
							Lat = res.Results.First().Geometry.Location.Lat,
							Lng = res.Results.First().Geometry.Location.Lng,
							StatusCode = res.Status
						};

					if (usecache) { 
						db.GeoCacheEntries.InsertOnSubmit(e);
						db.SubmitChanges();
					}
					return topos(e);
				}
				else if (res.Status == "ZERO_RESULTS")
				{
					var e = new GeoCacheEntry
					{
						AddressText = address,
						StatusCode = res.Status
					};
					if (usecache)
					{
						db.GeoCacheEntries.InsertOnSubmit(e);
						db.SubmitChanges();
					}
					return null;
				}
				else if (res.Status == "OVER_QUERY_LIMIT")
				{
					// TODO: shouldn't happen, gets eaten
					throw new QueryLimitException();
				}
				else
				{
					throw new ArgumentException(res.Status);
				}
			}
		}

		public static IEnumerable<Position> GeoCode(IEnumerable<string> addresses)
		{
			foreach (var a in addresses)
			{
				try
				{

				}
				catch (Exception e)
				{
				}
			}

			return addresses.Select(_ => GeoCode(_));
		}

		private static GeoResponse CallGeoWS(string address)
		{
			string url = string.Format(
				"http://maps.google.com/maps/api/geocode/json?address={0}&region=dk&sensor=false",
				HttpUtility.UrlEncode(address)
				);
			var request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GeoResponse));
			var res = (GeoResponse)serializer.ReadObject(request.GetResponse().GetResponseStream());
			return res;
		}

		private static GeoResponse CallWSCount(string address, int badtries)
		{
			Thread.Sleep(sleepinterval);
			GeoResponse res;
			try
			{
				res = CallGeoWS(address);
			}
			catch (GoogleIsSulkingException)
			{
				throw;
			}
			catch (Exception e)
			{
				Console.WriteLine("Caught exception: " + e);
				res = null;
			}
			if (res == null || res.Status == "OVER_QUERY_LIMIT")
			{
				int maxsleep = 60000;
				// we're hitting Google too fast, increase interval
				sleepinterval = Math.Min(sleepinterval + ++badtries * 1000, maxsleep);

				if (sleepinterval == maxsleep)
				{
					throw new GoogleIsSulkingException();
				}

				Console.WriteLine("Interval:" + sleepinterval + "                           \r");
				return CallWSCount(address, badtries);
			}
			else
			{
				// no throttling, go a little bit faster
				if (sleepinterval > 10000)
					sleepinterval = 200;
				else
					sleepinterval = Math.Max(sleepinterval / 2, 50);

				Console.WriteLine("Interval:" + sleepinterval);
				return res;
			}
		}
	}

	public class QueryLimitException : Exception
	{
	}

	public class GoogleIsSulkingException : Exception
	{
	}

	public class Position
	{
		public double Lat { get; set; }
		public double Lng { get; set; }
	}
}
