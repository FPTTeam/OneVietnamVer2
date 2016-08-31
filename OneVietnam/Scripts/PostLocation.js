function getAddressFromSearchBox() {
    // Create the search box and link it to the UI element.
    var input1 = document.getElementById("search-location");
    if(input1 !=null ){
        var searchBox1 = new google.maps.places.SearchBox(input1);

        searchBox1.addListener('places_changed', function () {
            var places = searchBox1.getPlaces();
            if (places.length == 0) {
                return;
            }

            // For each place, get the icon, name and location.
            places.forEach(function (place) {
                var address = '';
                if (place.address_components) {
                    address = [
                      (place.address_components[0] && place.address_components[0].short_name || ''),
                      (place.address_components[1] && place.address_components[1].short_name || ''),
                      (place.address_components[2] && place.address_components[2].short_name || ''),
                      (place.address_components[3] && place.address_components[3].short_name || ''),
                      (place.address_components[4] && place.address_components[4].short_name || ''),
                      (place.address_components[5] && place.address_components[5].short_name || ''),
                      (place.address_components[6] && place.address_components[6].short_name || '')
                    ].join(' ');
                }
               
                $(".forLocation").text(address);
                $("#PostLocation_Address").val(address);
                document.getElementById("PostLocation_XCoordinate").value = place.geometry.location.lat();
                document.getElementById("PostLocation_YCoordinate").value = place.geometry.location.lng();
                input1.value = "";
            });
        });
    }
}

function getCurrentLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var pos = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            };

            var geocoder = new window.google.maps.Geocoder();             // create a geocoder object
            var location = new window.google.maps.LatLng(pos.lat, pos.lng);    // turn coordinates into an object          
            geocoder.geocode({ 'latLng': location }, function (results, status) {
                if (status === window.google.maps.GeocoderStatus.OK) {           // if geocode success
                    var detailedLocation = results[0].formatted_address;         // if address found, pass to processing function
                    
                    var address_Post = document.getElementById("PostLocation_Address");
                    var NoEnglishLocation;
                    if (detailedLocation.indexOf("Unnamed Road,") != -1) {
                         NoEnglishLocation = detailedLocation.replace("Unnamed Road,", "");
                    } else {
                        NoEnglishLocation = detailedLocation;
                    }
                    if(address_Post != null){
                        $(".forLocation").text(NoEnglishLocation);
                        $("#PostLocation_Address").val(NoEnglishLocation);
                        document.getElementById("PostLocation_XCoordinate").value = pos.lat;
                        document.getElementById("PostLocation_YCoordinate").value = pos.lng;
                    }
                 
                } else {
                }
            })
        }, function () {
            // handleLocationError(true, myLocationMarker, map.getCenter());
            alert("Không thể định vị được vị trí của bạn. Bạn cần cho phép trình duyệt sử dụng định vị GPS.");
        });
    } else {
        // Browser doesn't support Geolocation
        //  handleLocationError(false, myLocationMarker, map.getCenter());
        alert("Trình duyệt của bạn không hỗ trợ định vị GPS. Vui lòng nâng cấp phiên bản mới nhất của trình duyệt và thử lại sau.");
    }
}

function getRegisteredLocation(id) {
    var controller = "/Map/GetUserLocation";
    var data = {
        userId : id
    }
    $.ajax({
        url: controller,
        type: 'GET',
        data:data,
        contentType: 'application/json;',
        dataType: 'json',
        success: function (location) {
            if (location == null || location == '') return;
            $(".forLocation").text(location["Address"]);
            $("#PostLocation_Address").val(location["Address"]);
            document.getElementById("PostLocation_XCoordinate").value = location["XCoordinate"];
            document.getElementById("PostLocation_YCoordinate").value = location["YCoordinate"];
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("Can not get user's location");
        }
    });        
}

function setRegisteredLocation(x, y, address) {
   // document.getElementById("Address_Edit").value = address;
   
    $(".forLocation").text(address);
    $("#PostLocation_Address").val(address);
    document.getElementById("PostLocation_XCoordinate").value = x;
    document.getElementById("PostLocation_YCoordinate").value = y;
}

$("#getloc").click();

google.maps.event.addDomListener(window, 'load', getAddressFromSearchBox);


