if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(function (position) {
        var pos = {
            lat: position.coords.latitude,
            lng: position.coords.longitude
        };        
        if ($("#XCoordinate").length > 0) {
            document.getElementById("XCoordinate").value = pos.lat;
            document.getElementById("YCoordinate").value = pos.lng;
            document.getElementById("XCoordinateExternal").value = pos.lat;
            document.getElementById("YCoordinateExternal").value = pos.lng;
        } else {
            document.getElementById("XCoordinateExternal").value = pos.lat;
            document.getElementById("YCoordinateExternal").value = pos.lng;
        }
        
        //  alert(pos.lat+'aa'+pos.lng);

        var geocoder = new window.google.maps.Geocoder();             // create a geocoder object
        var location = new window.google.maps.LatLng(pos.lat, pos.lng);    // turn coordinates into an object          
        geocoder.geocode({ 'latLng': location }, function (results, status) {
            if (status === window.google.maps.GeocoderStatus.OK) {           // if geocode success
                var detailedLocation = results[0].formatted_address;
                var NoEnglishLocation;
                if (detailedLocation.indexOf("Unnamed Road,") != -1) {
                     NoEnglishLocation = detailedLocation.replace("Unnamed Road,", "");
                } else {
                    NoEnglishLocation = detailedLocation;
                }
                // if address found, pass to processing function
                if ($("#XCoordinate").length > 0) {
                   
                    document.getElementById("Address").value = NoEnglishLocation;
                    document.getElementById("LocationExternal").value = NoEnglishLocation;
                } else {
                    document.getElementById("LocationExternal").value = NoEnglishLocation;
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