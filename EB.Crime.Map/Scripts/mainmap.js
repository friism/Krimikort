var map = null;
var cluster = null;
var eventids = [];
var coder = null;
var iconurlstart = '/Content/icons'; //'<%= ResolveUrl("~/Content/icons") %>';
var currenrequest = null;

google.maps.Map.prototype.currentOpenWindow = null;
google.maps.Map.prototype.cluster = null;

google.maps.Map.prototype.openInfoWindow = function (window, marker) {
    if (this.currentOpenWindow) {
        this.currentOpenWindow.close();
    }
    this.currentOpenWindow = window;
    if (this.currentOpenWindow && marker) {
        this.currentOpenWindow.open(this, marker);
    }
};

google.maps.Map.prototype.addMarker = function (pos, eid, ecat, icon, z) {
    if (_.indexOf(eventids, eid) === -1) {
        eventids.push(eid);
        var marker = new google.maps.Marker({
            position: pos,
            icon: iconurlstart + '/' + icon + ".png",
            zIndex: z
        });
        marker.event = { id: eid, catid: ecat };
        cluster.addMarker(marker);
        return marker;
    }
};

google.maps.Map.prototype.clearOutSideMarkers = function () {
    var bounds = map.getBounds();
    var removethese =
                    _.select(cluster.getMarkers(), function (m) {
                        if (bounds.contains(m.getPosition()) === false) {
                            var index = _.indexOf(eventids, m.event.id);
                            eventids.splice(index, 1);
                            return m;
                        }
                    });
    cluster.removeMarkers(removethese);
};

$.extend({
    getUrlVars: function () {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },
    getUrlVar: function (name) {
        return $.getUrlVars()[name];
    }
});

function GetIconUrl(catid) {
    return iconurlstart + '/' + GetIconName(catid) + ".png";
}

function GetIconName(catid) {
    switch (catid) {
        case 2: return 'thefth';
        case 8:
        case 9: return 'crimescene';
        case 11: return 'rape';
        case 13: return 'gun';
        case 14: return 'revolution';
        case 5: return 'fire';
        case 4: return 'icyroad';
        case 3: return 'bank';
        default: return 'accident';
    }
}

function refresh() {
    var bounds = map.getBounds();
    var NE = bounds.getNorthEast();
    var SW = bounds.getSouthWest();

    var datemode, startdate, enddate;
    switch ($('input[name=datemode]:checked').val()) {
        case 'day':
            {
                datemode = 'day';
                break;
            }
        case 'week':
            {
                datemode = 'week';
                break;
            }
        case 'month':
            {
                datemode = 'month';
                startdate = getDateFromSelect('start');
                enddate = getDateFromSelect('end');
                break;
            }
        default: return;
    }

    var race = '';
    race = $.getUrlVar('race');

    var cats = _.reduce($('.cat:checked'), '', function (memo, cb) {
        return memo + '-' + cb.id;
    });

    currenrequest = $.post('/data', //'<%= ResolveUrl("~/data")%>',
                    {
                    categories: cats,
                    ne_lat: NE.lat(),
                    ne_lng: NE.lng(),
                    sw_lat: SW.lat(),
                    sw_lng: SW.lng(),
                    datemode: datemode,
                    startdate: startdate,
                    enddate: enddate,
                    race: race
                },
                    function (result) {
                        if (result.count >= 2000) {
                            alert('For mange punkter, zoom ind eller vælg smallere dato-interval');
                            return;
                        }

                        map.clearOutSideMarkers();

                        // iterate groups
                        _.each(result.events, function (g) {
                            var position = new google.maps.LatLng(g[0], g[1]);
                            if (g[2].length > 1) {
                                //put top one on
                                var marker = map.addMarker(position, g[2][0][0], g[2][0][1], 'police2', 1);

                                if (marker) {
                                    // we need a copy, as the other one gets shifted in a bit
                                    var copy = g[2].slice();
                                    google.maps.event.addListener(marker, 'click', function () {
                                        if (marker.infoWindowCached) {
                                            map.openInfoWindow(marker.infoWindowCached, marker);
                                        }
                                        else {
                                            $.post('/eventdatamulti',
                                                {
                                                    eventidstring: _.reduce(copy, '',
                                                    function (memo, ee) {
                                                        return memo + '-' + ee[0];
                                                    })
                                                },
                                                function (result) {
                                                    var infoWindow = new google.maps.InfoWindow({
                                                        content: _.reduce(result, '', function (memo, r) {
                                                            r.url = baseurl + r.url;
                                                            r.iconUrl = GetIconUrl(r.catid); // add iconurl to object
                                                            r.escapedUrl = escape(r.url);
                                                            r.escapedTitle = escape(r.title);
                                                            return memo +
                                                                parseTemplate($("#infoWindowContentTemplateIcon").html(), r);
                                                        })
                                                    });
                                                    marker.infoWindowCached = infoWindow;
                                                    map.openInfoWindow(marker.infoWindowCached, marker);
                                                    //                                                    FB.XFBML.parse();
                                                }
                                            ); // post
                                        } //else
                                    });
                                    if (currentevent && currentevent.eventid === marker.event.id) {
                                        // trigger showing this baby
                                        google.maps.event.trigger(marker, 'click');
                                        currentevent = null;
                                    }

                                } //if

                                // zap first event
                                g[2].shift();

                                //iterate events after first one
                                _(g[2]).chain().each(function (e) {
                                    if (currentevent && currentevent.eventid === e[0]) {
                                        // trigger showing this baby
                                        google.maps.event.trigger(marker, 'click');
                                        currentevent = null;
                                    }

                                    map.addMarker(position, e[0], e[1], '1x1', -1);
                                });
                            } else {
                                // place the single event as normal
                                var marker = map.addMarker(position, g[2][0][0],
                                    g[2][0][1], GetIconName(g[2][0][1]), 1);

                                if (marker) {
                                    google.maps.event.addListener(marker, 'click', function () {
                                        if (marker.infoWindowCached) {
                                            map.openInfoWindow(marker.infoWindowCached, marker);
                                        } else {
                                            $.post('/eventdata',
                                            {
                                                eventid: marker.event.id
                                            },
                                            function (result) {
                                                result.url = baseurl + result.url; // augment url with baseurl
                                                result.escapedUrl = escape(result.url);
                                                result.escapedTitle = escape(result.title);
                                                var infoWindow = new google.maps.InfoWindow({
                                                    content: parseTemplate($("#infoWindowContentTemplate").html(), result)
                                                });
                                                map.openInfoWindow(infoWindow, marker);
                                                marker.infoWindowCached = infoWindow;
                                                //                                                FB.XFBML.parse();
                                            });
                                        }
                                    });
                                } //if
                                if (currentevent && currentevent.eventid === marker.event.id) {
                                    // trigger showing this baby
                                    google.maps.event.trigger(marker, 'click');
                                    currentevent = null;
                                }
                            } // else
                        });
                    }, 'json')
}

function getDateFromSelect(id) {
    var option = $('#' + id + ' option:selected');
    return option.attr('value') + '-' + option.parent().attr('label');
}

$(function () {


    // disable stuff that will yield too many points for IE
    if ($.browser.msie) {
        $("input[value='month']").click(function (e) {
            e.preventDefault();
            alert('Internet Explorer kan ikke vises mange punkter, skift browser.');
            $("input[value='week']").attr('checked', true);
            e.preventDefault();
            return;
        });
    }

    // goddam firefox saves checkbox state, make sure categorycheckboxes are checked
    _.each($('.cat'), function (i) {
        $(i).attr('checked', 'checked');
    });

    // select proper elements in slider-dropdowns
    if (currentevent) {
        // we are showing one particular event, set selectors to bracket date
        $("#start optgroup[label='" +
            currentevent.year +
            "'] option[value='" +
            currentevent.month + "']").
            attr('selected', 'selected').index();
        $("#end optgroup[label='" +
            currentevent.nextmonthyear +
            "'] option[value='" +
            currentevent.nextmonth + "']").
            attr('selected', 'selected').index();

        // set datemode to month
        $('input[value=month]:radio').attr('checked', 'checked');
    } else {
        // just set to most recent month
        var optioncount = $('select#start option').length;
        $('#start option').eq(optioncount - 2).attr('selected', 'selected');
        $('#end option').eq(optioncount - 1).attr('selected', 'selected');
    }

    if (currentevent) {
        var latlng = new google.maps.LatLng(currentevent.lat, currentevent.lng);
        var zoom = 15;
    } else {
        // temporarry measure until after map create
        var latlng = new google.maps.LatLng(56.08429756206141, 10.9423828125);
        var zoom = 7;
    }

    var myOptions = {
        zoom: zoom,
        center: latlng,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

//    FB.init({ appId: '120327367997883', status: true, cookie: true, xfbml: false });
    // apparently, this needs a warm-up?
//    FB.XFBML.parse();

    map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);

    if (!currentevent) {
        // ensure correct bounds and zoom
        var ne = new google.maps.LatLng(57.8355660225202, 15.325698028124979);
        var sw = new google.maps.LatLng(54.54931541565687, 7.876967559374979);

        var dkbounds = new google.maps.LatLngBounds(sw, ne);
        map.fitBounds(dkbounds);
    }

    cluster = new MarkerClusterer(map, [], { maxZoom: 14 });

    google.maps.event.addListener(map, 'idle', function () {
        refresh();
    });

    google.maps.event.addListener(map, 'bounds_changed', function () {
        if (currenrequest) {
            currenrequest.abort();
        }
    });

    $('.cat').change(function () {
        if ($(this).is(':checked')) {
            // cb was checked, get new points
            refresh();
        } else {
            // cb was unchecked, remove relevant markers
            var cbCat = parseInt($(this).attr('id'));
            var removeThese = _.select(cluster.getMarkers(), function (m) {
                if (m.event.catid === cbCat) {

                    var index = _.indexOf(eventids, m.event.id);
                    eventids.splice(index, 1);

                    return m;
                }
            });
            cluster.removeMarkers(removeThese);
        }
    });

    $('#selectall').click(function (e) {
        e.preventDefault();
        $('.cat').attr('checked', true);
        refresh();
    });

    $('#deselectall').click(function (e) {
        e.preventDefault();
        $('.cat').attr('checked', false);
        cluster.clearMarkers();
        eventids = [];
    });

    $('select').selectToUISlider({
        labels: 12,
        sliderOptions: {
            change: function (e, ui) {
                cluster.clearMarkers();
                eventids = [];
                refresh();
            }
        }
    }).hide();

    $('input[value=day]:radio').change(function () {
        if ($(this).attr('checked')) {
            $('#dateslider').hide();
            cluster.clearMarkers();
            eventids = [];
            refresh();
        }
    });

    $('input[value=week]:radio').change(function () {
        if ($(this).attr('checked')) {
            $('#dateslider').hide();
            cluster.clearMarkers();
            eventids = [];
            refresh();
        }
    });

    $('input[value=month]:radio').change(function () {
        if ($.browser.msie) {
            // IE users aren't allowed to fiddle with this
            return;
        }

        if ($(this).attr('checked')) {
            $('#dateslider').show();
            cluster.clearMarkers();
            eventids = [];
            refresh();
        }
    });

    // make sure the month-slider is shown
    if ($('input[name=datemode]:checked').val() == 'month') {
        $('#dateslider').show();
    } else {
        $('#dateslider').hide();
    }

    // get dialog going
    $('#tipdialog').dialog({ autoOpen: false, modal: true, buttons: {}, resizable: false });
    // enable close link
    $('#closedialog').click(function (e) { e.preventDefault(); $('#tipdialog').dialog('close') });

    $('a.tip').live('click', function (e) {
        e.preventDefault();
        // replace event code in dialog
        $('#eventcode').html($(this).parent().attr('id'));
        $('#tipdialog').dialog('open');
    });


});

// this function stolen from findvej.dk
function switchBlock(id, initialblock) {
    var myblock = document.getElementById(id);
    if (myblock.style.display == 'none') {
        myblock.style.display = 'block';
    } else if (myblock.style.display == 'block') {
        myblock.style.display = 'none';
    } else if (initialblock === 0) {
        myblock.style.display = 'none';
    } else {
        myblock.style.display = 'block';
    }
}
