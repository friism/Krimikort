using System.Net;
using System.Text;
using HtmlAgilityPack;
using SimpleBrowser;

namespace EB.Crime.Downloader.Util
{
	public static class Doc
	{
		public static string GetHtml(string url)
		{
			WebClient wc = new WebClient();
			return ExtractString(wc.DownloadData(url));
		}

		public static HtmlDocument LoadDoc(string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			return doc;
		}

		static string ExtractString(byte[] webResult)
		{
			string s = Encoding.UTF8.GetString(webResult);
			return s;
		}

		public static string GetArchiveHtmlByPost(string url)
		{
			var browser = new Browser();
			browser.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";
			browser.Navigate(url);
			browser.ExtraFormValues.Add("__EVENTTARGET", "Archivepagecontrol2:ShowAll");
			var form = browser.Find("Form1");
			var newAction = "/CMS.Politi.Web/Templates/Content/" + form.XElement.GetAttribute("action");
			form.XElement.SetAttributeValue("action", newAction);
			form.SubmitForm();
			return browser.CurrentHtml;
		}
	}
}
