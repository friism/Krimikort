var map = null;
var markers = [];
var iconurlstart = '/Content/icons';
			
$(function () {
	var sealandlatlng = new google.maps.LatLng(55.62944317510878, 12.178001403808594);
	var myOptions = {
		zoom: 10,
		center: sealandlatlng,
		mapTypeId: google.maps.MapTypeId.ROADMAP
	};

	var ne = new google.maps.LatLng(57.8355660225202, 15.325698028124979);
	var sw = new google.maps.LatLng(54.54931541565687, 7.876967559374979);

	var dkbounds = new google.maps.LatLngBounds(sw, ne);

	map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);
	currentinfowindow = null;

	var bounds = new google.maps.LatLngBounds();

	_.each(events, function(e) {
		var latlng = new google.maps.LatLng(e.lat, e.lng);
					
		if(dkbounds.contains(latlng)){
			bounds.extend(latlng);
		}
		var marker = new google.maps.Marker({
			map: map,
			position: latlng,
			icon: baseurl + iconurlstart + "/" + e.iconname + ".png"
		});

		e.text = $('#event-' + e.eventid + ' > .eventbody').html();
		e.url = baseurl + e.url;

		var infoWindow = new google.maps.InfoWindow({
			content: parseTemplate(
				'<div><p><strong><#= title #></strong>, <#= street #> - <#= city #> d. <#= date #></p><p><#= text #></p><p id="<#= displayid #>"><a target="_blank" href="<#= url #>">Link til hændelse</a>(stort kort)</p></div>', e) // parseTemplate($("#infoWindowContentTemplate").html(), e)
		});

		marker.infoWindow = infoWindow;
		marker.event = e;

		markers.push(marker);

		google.maps.event.addListener(marker, 'click', function () {
			if(currentinfowindow){
				currentinfowindow.close();
			}
			infoWindow.open(map, marker);
			currentinfowindow = marker.infoWindow;
		});

		$('.mapanchor').click(function() {
			var id = parseInt($(this).parents('.event').attr('id').split("-")[1]);

			var marker = _(markers).chain().
				select(function(m) {
					return m.event.eventid == id;
				}).
				first().value();
						
			if(currentinfowindow){
				currentinfowindow.close();
			}
			marker.infoWindow.open(map, marker);
			currentinfowindow = marker.infoWindow;
		});
	});
	map.fitBounds(bounds);
});