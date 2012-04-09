<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="EB.Crime.DB" %>
<%@ Import Namespace="EB.Crime.Map.Views" %>
<%@ Import Namespace="SquishIt.Framework.JavaScript.Minifiers" %>
<%@ Import Namespace="SquishIt.Framework" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
	<head>
		<!-- All hail Peter Brodersen of findvej.dk (although Michael Friis added support for gmaps v3) -->
		<meta name="viewport" content="initial-scale=1.0, user-scalable=no" />
		<meta http-equiv="content-type" content="text/html; charset=UTF-8" />
		<% if (ViewData["CurrentEvent"] == null)
		{%>
			<meta property="og:title" content="Krimikort" />
			<meta property="og:type" content="website" />
			<meta property="og:url" content="http://krimikort.ekstrabladet.dk/" />
		<%
		}
		else
		{
			Event theevent = (Event)ViewData["CurrentEvent"];
		%>
			<meta property="og:title" content="<%= theevent.Title %>" />
			<meta property="og:type" content="article" />
			<meta property="og:url" content="<%= Request.Url %>" />
			<meta property="og:description" content="<%= theevent.BodyText.Truncate(200, true) %>" />
		<%
		}
		%>
		<meta property="og:image" content="<%= Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/Content/krimikort.png") %>" />
		<meta property="fb:app_id" content="120327367997883" />
		<meta property="og:site_name" content="Ekstra Bladet Krimikort"/>
		
		<title>Ekstra Bladet Krimikort</title>
		<meta name="google-site-verification" content="xozWETOjx8izjji2ptxskl9EVyHfJG-wwVvxOAtBrIs" />
		<link href="<%= ResolveUrl("~/Content/Site.css")%>" rel="stylesheet" type="text/css" />
		<link href="<%= ResolveUrl("~/Content/ui.slider.extras.css")%>" rel="stylesheet"
			type="text/css" />
		<link href="<%= ResolveUrl("~/Content/jquery-ui.css.css")%>" rel="stylesheet" type="text/css" />
		<link rel="icon" href="<%= ResolveUrl("~/Content/ebfav.png") %>" />
		<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js"></script>
		<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.1/jquery-ui.min.js"></script>
		<script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=true"></script>
		
		<%--<script src="http://connect.facebook.net/en_US/all.js"></script>--%>
		<script type="text/javascript">
			// we have to this here to get script tags
			var baseurl =
				'<%= Request.Url.GetLeftPart(UriPartial.Authority) %>';
				<% if(ViewData["CurrentEvent"] != null)
				{
					Event currentevent = (Event) ViewData["CurrentEvent"];
				%>
				var currentevent = <%= 
					(new JavaScriptSerializer()).
						Serialize(
							new {
									lat = currentevent.Lat,
									lng = currentevent.Lng,
									categoryid = currentevent.CategoryId,
									eventid = currentevent.EventId,
									year = currentevent.IncidentTime.Value.Year,
									month = currentevent.IncidentTime.Value.ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("da-DK")),
									nextmonthyear = currentevent.IncidentTime.Value.AddMonths(1).Year,
									nextmonth = currentevent.IncidentTime.Value.AddMonths(1).ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("da-DK")),
								}
						) 
				%>;
				<%
				}
				else{ %>
					var currentevent = null;
				<% }%>
		</script>
		<%= Bundle.JavaScript()
				.Add("~/Scripts/oms.min.js")
				.Add("~/Scripts/underscore-min.js")
				.Add("~/Scripts/selectToUISlider.jQuery.js")
				.Add("~/Scripts/templater.js")
				.Add("~/Scripts/mainmap.js")
				.WithMinifier(JavaScriptMinifiers.Ms)
				.RenderOnlyIfOutputFileMissing()
				.Render("~/Scripts/combined_#.js") %>

		<script id="infoWindowContentTemplate" type="text/html">
			<div>
				<p>
					<strong><#= title #></strong>, <#= street #> - <#= city #> d. <#= date #>
				</p>
				<p>
					<#= text #>
				</p>
				<p id="<#= displayid #>">
					<a href="#" class="tip">Har du billede, tip eller kommentar?</a> 
					| <a href="<#= url #>">Link til hændelse</a> <br />
					<iframe 
						src="http://www.facebook.com/plugins/like.php?href=<#= escapedUrl #>%2F&amp;layout=standard&amp;show_faces=false&amp;width=450&amp;action=recommend&amp;colorscheme=light&amp;height=35" 
						scrolling="no" frameborder="0" 
						style="border:none; overflow:hidden; width:450px; height:35px;" 
						allowTransparency="true"></iframe>
				</p>
			</div>
		</script>
	</head>
	<body>
		<img alt="fbpic" style="display: none" src="<%= ResolveUrl("~/Content/krimikort.png") %>" />
		<div id="map_canvas">
		</div>
		<div id="menu">
			<a href="javascript:switchBlock('menucontent',0);">
				<img width="10" height="10" title="Minimér" alt="Minimér" class="topicon" src="/Content/icon_min.gif" />
			</a>
			<h1>
				<img alt="" src="<%= ResolveUrl("~/Content/ebfav.png") %>" style="vertical-align: middle;" />Ekstra
				Bladets Krimikort</h1>
			<div id="menucontent">
				<p style="margin-top: 2px">
					...et <a href="http://ekstrabladet.dk/112/">Ekstra Bladet 112</a> produkt.</p>
				<form id="findevents" action="">
				<table>
					<tbody>
						<tr>
							<td colspan="2">
								<strong>Datointerval</strong>
							</td>
						</tr>
						<tr>
							<td>
								<input type="radio" name="datemode" value="day" />
							</td>
							<td>
								Igår
							</td>
						</tr>
						<tr>
							<td>
								<input type="radio" name="datemode" value="week" checked="checked" />
							</td>
							<td>
								Sidste uge
							</td>
						</tr>
						<tr>
							<td>
								<input type="radio" name="datemode" value="month" />
							</td>
							<td>
								Vælg måneder
							</td>
						</tr>
						<tr>
							<td colspan="2">
								<strong>Vælg Kategogier</strong>
							</td>
						</tr>
						<% foreach (var c in (IEnumerable<Category>)ViewData["Categories"])
						{
						%>
						<tr>
							<td>
								<input id="<%= c.CategoryId %>" type="checkbox" checked="checked" class="cat" />
							</td>
							<td>
								<span>
									<%= c.DisplayName %></span><img style="vertical-align: middle; height: 20px" alt="icon"
										src="<%= ResolveUrl("~/Content/icons") + "/" + ViewUtil.GetIconName(c.CategoryId) %>.png" />
							</td>
						</tr>
						<%
							} %>
						<tr>
							<td colspan="2">
								Vælg <a id="selectall" href="#">alle</a>/<a id="deselectall" href="#">ingen</a>
							</td>
						</tr>
					</tbody>
				</table>
				</form>
				<p style="margin-top: 2px">
					<a href="javascript:switchBlock('helpinfo',1)">[Læs om krimikortet]</a>
				</p>
			</div>
		</div>
		<div id="helpinfo">
			<a href="javascript:switchBlock('helpinfo',0);">
				<img width="10" height="10" title="Close" alt="Close" class="topicon" src="/Content/icon_close.gif" />
			</a>
			<h1>
				Om Krimikortet</h1>
			<p>
				Krimikortet giver dig dagligt opdateret information om Politiets arbejde. Kortet
				er baseret på information fra Politiets døgnrapporter. Lige nu dækker vi Københavns,
				Vestegnens, Midt- og Vestsjællands, Fyns og Nordjyllands politikredse. De øvrige
				politikredse udgiver ikke døgnrapporter på en organiseret facon. Brok dig gerne
				til din lokale politimester hvis du vil have din by dækket.
			</p>
			<p>
				Du kan læse mere på udviklingsafdelingens blog, <a href="http://bits.ekstrabladet.dk/post/558455227/ekstra-bladet-krimikort-lanceret">
					bits.ekstrabladet.dk</a>.
			</p>
			<p>
				<strong>Credits</strong>
				<br />
				Ikoner er fra <a href="http://code.google.com/p/google-maps-icons/">Maps icons collection</a>.
				<br />
				Kortinterfacet er baseret på Peter Brodersens <a href="http://findvej.dk/">findvej.dk</a>.
				<br />
				Ide delvis baseret på <a href="http://www.dognrapporten.dk/">dognrapporten.dk</a>
			</p>
		</div>
		<div id="dateslider" style="display: none">
			<form action="#">
			<%
				var earliestevent = (DateTime)ViewData["Earliest"];
				var mostrecentevent = (DateTime)ViewData["MostRecent"];

				var alldates = ViewUtil.GetDateRange(earliestevent, mostrecentevent.AddMonths(1));

				var yearmonts =
					from d in alldates.Select(
							d => new { year = d.Year, month = d.ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("da-DK")) })
						.Distinct()
					group d by d.year into g
					select new { year = g.Key, months = g };
			%>
			<select id="start" name="start">
				<% foreach (var y in yearmonts)
				   {
				%>
				<optgroup label="<%= y.year %>">
					<% foreach (var m in y.months)
					   { %>
					<option value="<%= m.month %>">
						<%= m.month%></option>
					<% } %>
				</optgroup>
				<%
					} %>
			</select>
			<select id="end" name="end">
				<% foreach (var y in yearmonts)
				   {
				%>
				<optgroup label="<%= y.year %>">
					<% foreach (var m in y.months)
					   { %>
					<option value="<%= m.month %>">
						<%= m.month%></option>
					<% } %>
				</optgroup>
				<%
					} %>
			</select>
			</form>
		</div>
		<div id="tipdialog" style="display: none;">
			<p>
				Hvis du har tip, billede eller kommentar til hændelsen så har vi måske 1.000 kr.
				til dig!</p>
			<p>
				Send en SMS eller MMS til <span class="red">1224</span> (alm. takst). Du kan også
				sende en e-mail til <a class="red" href="mailto:1224@eb.dk?subject=Tip til krimikort">
					1224@eb.dk</a> eller ringe til <span class="red">3347 1224</span>.
			</p>
			<p>
				Inkluder koden <span id="eventcode" class="red"></span>&nbsp;i din henvendelse så
				vi ved hvilken hændelse der er tale om.
			</p>
			<p>
				<a class="red" href="http://ekstrabladet.dk/om_ekstra_bladet/1224/" target="_blank">
					Læs mere om vilkår</a>
			</p>
			<p>
				<a id="closedialog" href="#">Luk</a>
			</p>
		</div>
		<script type="text/javascript">
			var gaJsHost = (("https:" == document.location.protocol) ? "https://ssl." : "http://www.");
			document.write(unescape("%3Cscript src='" + gaJsHost + "google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E"));
		</script>
		<script type="text/javascript">
			try {
				var pageTracker = _gat._getTracker("UA-2135460-20");
				pageTracker._trackPageview();
			} catch (err) { }</script>
		<script type="text/javascript">
			var pp_gemius_identifier = new String('Aprgi2_2I0Hse9G.TdT0UHXorjvZH2ejHQ8SR4yNfnb.Q7');
		</script>
		<script type="text/javascript" charset="utf-8" src="http://ekstrabladet.dk/grafik/ver3/hydrogen/js/xgemius.js"></script>
		<div id="fb-root">
		</div>
	</body>
</asp:Content>
