var listMarkers = [];
var map;
var bounds;
var isFirstTime = true;
var myCurrentLocationMarker, myHomeMarker;
var markerCluster;
var isClickOnSpiderfier = true;
var currentFilter = -1
var isPostFilter = false;
var list = [];
var Type3Icon, Type4Icon, Type5Icon, Type6Icon, Type7Icon, Type8Icon, Type9Icon;
var UsersIcon, FemaleIcon, MaleIcon, LGBTIcon;
var currentMarkerClusterer;
var isAutoCompleteBox = false;
var selectedPostMarker;

var listUserMarkers = [], listMaleMarkers = [], listFemaleMarkers = [], listLGBTMarkers = [];

var listType3Markers = [], listType4Markers = [], listType5Markers = [], listType6Markers = [], listType7Markers = [], listType8Markers = [], listType9Markers = [];

//var userMarkerCluster = [], maleMarkerCluster = [], femaleMarkerCluster = [], LGBTMarkerCluster = [];

//var type3MarkerCluster = [], type4MarkerCluster = [], type5MarkerCluster = [], type6MarkerCluster = [], type7MarkerCluster = [], type8MarkerCluster = [], type9MarkerCluster = [];

var overlappingType3 = [], overlappingType4 = [], overlappingType5 = [], overlappingType6 = [], overlappingType7 = [], overlappingType8 = [], overlappingType9 = [];

var overlappingMale = [], overlappingFemale = [], overlappingLGBT = [], overlappingUsers = [];

function checkAuthenticated() {
    var icon = {
        url: "/Content/Icon/location.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    var myhomeicon = {
        url: "/Content/Icon/myhome.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };



    // Declare a myLocation marker using icon declared above, and bind it to the map
    myCurrentLocationMarker = new google.maps.Marker({
        title: "Vị trí hiện tại của tôi",
        optimized: false,
        icon: icon
    });

    myHomeMarker = new google.maps.Marker({
        title: "Địa chỉ của tôi",
        optimized: false,
        icon: myhomeicon
    });

    if (isAuthenticated) {
        //Declare a new map
        map = new google.maps.Map(document.getElementById('map_canvas'), {
            center: { lat: authenticatedUser.x, lng: authenticatedUser.y },
            zoom: 12,
            minZoom: 2,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });
        //myCurrentLocationMarker.setMap(map);
        //myHomeMarker.setPosition({ lat: authenticatedUser.x, lng: authenticatedUser.y });
        //myHomeMarker.setMap(map);

    }
    else {
        //Declare a new map
        map = new google.maps.Map(document.getElementById('map_canvas'), {
            center: { lat: 21.0277644, lng: 105.83415979999995 },
            zoom: 12,
            minZoom: 2,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });

        //myCurrentLocationMarker.setMap(map);
        // showCurrentLocation();
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function (position) {
                var pos = {
                    lat: position.coords.latitude,
                    lng: position.coords.longitude
                };
                var pos2 = new google.maps.LatLng(position.coords.latitude, position.coords.longitude)

                myCurrentLocationMarker.setMap(map);
                myCurrentLocationMarker.setPosition(pos);
                map.setCenter(pos2);

                google.maps.event.addListener(myCurrentLocationMarker, 'click', (function () {
                    return function () {
                        map.setCenter(pos2);
                        map.setZoom(14);
                    }
                })());


            }, function () {
                handleLocationError(true, "Không thể định vị được vị trí của bạn. Bạn cần cho phép trình duyệt sử dụng định vị GPS.", map.getCenter());
            });
        } else {
            // Browser doesn't support Geolocation
            handleLocationError(false, "Trình duyệt của bạn không hỗ trợ định vị GPS. Vui lòng nâng cấp phiên bản mới nhất của trình duyệt và thử lại sau.", map.getCenter());
        }

    }
}

function initialize() {
    checkAuthenticated();

    //Declare a bound
    bounds = new google.maps.LatLngBounds();

    //google.maps.event.addListener(map, 'bounds_changed', function () {
    //    bounds = map.getBounds();
    //});

    google.maps.event.addListener(map, 'idle', function () {
        bounds = map.getBounds();
        if (map.getZoom() >= 5 && isPostFilter == false) {

            switch (currentFilter) {
                case -4: showFemales(); break;
                case -3: showMales(); break;
                case -2: showLGBT(); break;
                case -1: showUsers(); break;
                case 0: showFemales(); break;
                case 1: showMales(); break;
                case 2: showLGBT(); break;
                case 3: showAccommodation(); break;
                case 4: showJobOffer(); break;
                case 5: showFurnitureOffer(); break;
                case 6: showHandGoodsOffer(); break;
                case 7: showTradeOffer(); break;
                case 8: showSOS(); break;
                case 9: showWarning(); break;
            }
        }
        //setTimeout(function () {
        //    var cnt = map.getCenter();
        //    cnt.e += 0.000001;
        //    map.panTo(cnt);
        //    cnt.e -= 0.000001;
        //    map.panTo(cnt);
        //}, 400);
    });

    //Delcare overlapping, markerClusterer
    overlappingUsers = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingMale = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingLGBT = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingFemale = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingType3 = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingType4 = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingType5 = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingType6 = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingType7 = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingType8 = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
    overlappingType9 = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });

    overlappingType9.
    Type3Icon = {
        url: "/Content/Icon/home.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    Type4Icon = {
        url: "/Content/Icon/job.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    Type5Icon = {
        url: "/Content/Icon/free.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    Type6Icon = {
        url: "/Content/Icon/ship.jpg",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    Type7Icon = {
        url: "/Content/Icon/sale.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    Type8Icon = {
        url: "/Content/Icon/help.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    Type9Icon = {
        url: "/Content/Icon/Warning.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    UsersIcon = {
        url: "/Content/Icon/users.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    MaleIcon = {
        url: "/Content/Icon/male.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    FemaleIcon = {
        url: "/Content/Icon/female.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };

    LGBTIcon = {
        url: "/Content/Icon/LGBT.png",
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(50, 50)
    };


    createListUserMarkers();
    createListMaleMarkers();
    createListFemaleMarkers();
    createListLGBTMarkers();

    document.getElementById("filterUsers").style.background = "url(/Content/Icon/users2.png)";
    document.getElementById("filterUsers").style.backgroundSize = "100%";

    currentMarkerClusterer = new MarkerClusterer(map, list);
    //Init user makers first load
    //userMarkerCluster.setMaxZoom(9);
    //showUsers();

    // Create the search box and link it to the UI element.
    var input = document.getElementById("pac-input2");
    var searchBox = new google.maps.places.SearchBox(input);

    // Listen for the event fired when the user selects a prediction and retrieve
    // more details for that place.
    searchBox.addListener('places_changed', function () {
        var places = searchBox.getPlaces();
        if (places.length == 0) {
            return;
        }
        // For each place, get the icon, name and location.
        var bounds = new google.maps.LatLngBounds();
        places.forEach(function (place) {

            if (place.geometry.viewport) {
                // Only geocodes have viewport.
                bounds.union(place.geometry.viewport);
            } else {
                bounds.extend(place.geometry.location);
            }
        });
        //map.setZoom(12);
        isAutoCompleteBox = true;
        map.fitBounds(bounds);
    });

    $('#pac-input2').keypress(function (e) {
        if (e.which == 13) {
            google.maps.event.trigger(searchBox, 'place_changed');
            return false;
        }
    });

}

function loadScript() {
    var script = document.createElement("script");
    script.src = "http://maps.googleapis.com/maps/api/js?key=AIzaSyBiPDMBCKXsusl5-BgCw1nIyHwu5u3j8xw&libraries=places,geometry&callback=initialize";
    document.body.appendChild(script);

}

function showCurrentLocation() {
    isPostFilter = true;
    currentMarkerClusterer.setMap(null);
    // setMapToAMarkerCluster(null);
    myHomeMarker.setMap(null);
    if (isAuthenticated == true) {
        document.getElementById("myLocation").style.background = "url(/Content/Icon/myhome.png)";
        document.getElementById("myLocation").style.backgroundSize = "100%";
    }

    //Identify current user's location and bind it to the map
    //Using HTML5 geolocation.
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var pos = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            };
            var pos2 = new google.maps.LatLng(position.coords.latitude, position.coords.longitude)

            myCurrentLocationMarker.setMap(map);
            myCurrentLocationMarker.setPosition(pos);
            google.maps.event.addListener(myCurrentLocationMarker, 'click', (function () {
                return function () {
                    map.setCenter(pos2);
                    map.setZoom(14);
                }
            })());

            //map.setZoom(12);
            checkIfBoundContainPosition(pos2);
            //bounds.extend(pos);
            //map.fitBounds(bounds);
            //map.setCenter(pos);
            // map.setCenter(pos);

        }, function () {
            handleLocationError(true, "Không thể định vị được vị trí của bạn. Bạn cần cho phép trình duyệt sử dụng định vị GPS.", map.getCenter());
        });
    } else {
        // Browser doesn't support Geolocation
        handleLocationError(false, "Trình duyệt của bạn không hỗ trợ định vị GPS. Vui lòng nâng cấp phiên bản mới nhất của trình duyệt và thử lại sau.", map.getCenter());
    }

}

function showMyLocation() {
    isPostFilter = true;
    currentMarkerClusterer.setMap(null);
    //setMapToAMarkerCluster(null);
    myCurrentLocationMarker.setMap(null);

    document.getElementById("location").style.background = "url(/Content/Icon/location.png)";
    document.getElementById("location").style.backgroundSize = "100%";

    myHomeMarker.setMap(map);
    myHomeMarker.setPosition({ lat: authenticatedUser.x, lng: authenticatedUser.y });

    var pos2 = new google.maps.LatLng(authenticatedUser.x, authenticatedUser.y)
    google.maps.event.addListener(myHomeMarker, 'click', (function () {
        return function () {
            map.setCenter(pos2);
            map.setZoom(14);
        }
    })());
    checkIfBoundContainPosition(pos2);
    // map.setCenter({ lat: authenticatedUser.x, lng: authenticatedUser.y });
}

function showUsers() {
    closeAllSpiderfier();
    showMarkersOnMap(allUsers, -1, listUserMarkers);
}

function showLGBT() {

    closeAllSpiderfier();
    showMarkersOnMap(LGBT, -2, listLGBTMarkers);
}

function showMales() {
    closeAllSpiderfier();
    showMarkersOnMap(males, -3, listMaleMarkers);
}

function showFemales() {
    closeAllSpiderfier();
    showMarkersOnMap(females, -4, listFemaleMarkers);
}

function showAccommodation() {
    closeAllSpiderfier();
    loadByAjax(postType3, 3);

}


function showJobOffer() {
    closeAllSpiderfier();
    loadByAjax(postType4, 4);
}

function showFurnitureOffer() {
    closeAllSpiderfier();
    loadByAjax(postType5, 5);
}

function showHandGoodsOffer() {
    closeAllSpiderfier();
    loadByAjax(postType6, 6);
}

function showTradeOffer() {
    closeAllSpiderfier();
    loadByAjax(postType7, 7);
}

function showSOS() {
    closeAllSpiderfier();
    loadByAjax(postType8, 8);
}

function showWarning() {
    closeAllSpiderfier();
    loadByAjax(postType9, 9);
}

function createListUserMarkers() {
    var length = allUsers.length;

    for (var i = 0; i < length; i++) {
        var position = new google.maps.LatLng(allUsers[i].x, allUsers[i].y);
        bounds.extend(position);
        marker = new google.maps.Marker({
            position: position,
            map: null,
            optimized: false,
            title: "Nhấp để xem chi tiết",
            icon: UsersIcon
        });
        listUserMarkers.push(marker);
        // Allow each marker to have an info window
        google.maps.event.addListener(marker, 'click', (function (marker, i) {
            return function () {
                setTimeout(function () {
                    getUserInfo(allUsers[i].userID);
                }, 100);
            }
        })(marker, i));

        overlappingUsers.addMarker(marker);
    }

    overlappingUsers.addListener('click', function (marker) {
        isClickOnSpiderfier = false;
       // overlappingUsers = new OverlappingMarkerSpiderfier(map, { circleFootSeparation: 60 });
     
    });
    overlappingUsers.addListener('spiderfy', function (markers) {
        isClickOnSpiderfier = true;
    });

}

function createListMaleMarkers() {

    var length = males.length;
    for (var i = 0; i < length; i++) {
        var position = new google.maps.LatLng(males[i].x, males[i].y);
        bounds.extend(position);
        marker = new google.maps.Marker({
            position: position,
            map: null,
            optimized: false,
            title: "Nhấp để xem chi tiết",
            icon: MaleIcon
        });
        listMaleMarkers.push(marker);

        // Allow each marker to have an info window
        google.maps.event.addListener(marker, 'click', (function (marker, i) {
            return function () {
                setTimeout(function () {
                    getUserInfo(males[i].userID);
                }, 100);
            }
        })(marker, i));

        overlappingMale.addMarker(marker);
    }

    overlappingMale.addListener('click', function (marker) {
        isClickOnSpiderfier = false;
    });
    overlappingMale.addListener('spiderfy', function (markers) {
        isClickOnSpiderfier = true;
    });
}

function createListFemaleMarkers() {

    var length = females.length;
    for (var i = 0; i < length; i++) {
        var position = new google.maps.LatLng(females[i].x, females[i].y);
        bounds.extend(position);
        marker = new google.maps.Marker({
            position: position,
            optimized: false,
            map: null,
            title: "Nhấp để xem chi tiết",
            icon: FemaleIcon
        });
        listFemaleMarkers.push(marker);

        // Allow each marker to have an info window
        google.maps.event.addListener(marker, 'click', (function (marker, i) {
            return function () {
                setTimeout(function () {
                    getUserInfo(females[i].userID);
                }, 100);
            }
        })(marker, i));
        overlappingFemale.addMarker(marker);
    }

    overlappingFemale.addListener('click', function (marker) {
        isClickOnSpiderfier = false;
    });
    overlappingFemale.addListener('spiderfy', function (markers) {
        isClickOnSpiderfier = true;
    });
}

function createListLGBTMarkers() {

    var length = LGBT.length;
    for (var i = 0; i < length; i++) {
        var position = new google.maps.LatLng(LGBT[i].x, LGBT[i].y);
        bounds.extend(position);
        marker = new google.maps.Marker({
            position: position,
            map: null,
            optimized: false,
            title: "Nhấp để xem chi tiết",
            icon: LGBTIcon
        });
        listLGBTMarkers.push(marker);

        // Allow each marker to have an info window
        google.maps.event.addListener(marker, 'click', (function (marker, i) {
            return function () {
                setTimeout(function () {
                    getUserInfo(LGBT[i].userID);
                }, 100);
            }
        })(marker, i));

        overlappingLGBT.addMarker(marker);
    }

    overlappingLGBT.addListener('click', function (marker) {
        isClickOnSpiderfier = false;
    });
    overlappingLGBT.addListener('spiderfy', function (markers) {
        isClickOnSpiderfier = true;
    });
}

function createListPostMarker(positionList, listTypeMarker, overlappingType, TypeIcon) {
    var length = positionList.length;

    for (var i = 0; i < length; i++) {
        var position = new google.maps.LatLng(positionList[i].x, positionList[i].y);
        //bounds.extend(position);
        marker = new google.maps.Marker({
            position: position,
            map: null,
            optimized: false,
            title: "Nhấp để xem chi tiết",
            icon: TypeIcon
        });
        listTypeMarker.push(marker);

        // Allow each marker to have an info window
        google.maps.event.addListener(marker, 'click', (function (marker, i) {
            return function () {
                setTimeout(function () {
                    getPostInfo(positionList[i].postID);
                }, 100);
            }
        })(marker, i));

        overlappingType.addMarker(marker);
    }

    overlappingType.addListener('click', function (marker) {
        isClickOnSpiderfier = false;
    });
    overlappingType.addListener('spiderfy', function (markers) {
        isClickOnSpiderfier = true;
    });

}

function showMarkersOnMap(postTypeNumber, currentFilterNumber, listTypeMarkersNumber) {

    currentFilter = currentFilterNumber;

    isPostFilter = true;
    if (checkIfCurrentBoundContainMarker(listTypeMarkersNumber, currentFilter) == false) {

        if (isAutoCompleteBox == true) {
            if (currentFilter != -5) {
                if ((currentFilterNumber == -1) || (currentFilterNumber == -2) || (currentFilterNumber == -3) || (currentFilterNumber == -4)) {
                    $("#nearestUserAlertModal").modal('show');
                } else {
                    $("#nearestPostAlertModal").modal('show');
                }

                isAutoCompleteBox = false;
                isPostFilter = false;
                return;
            }
        }

        var pos = calculateNearestMarker(postTypeNumber);
        if (pos) {
            // smoothlyCenterPosition(pos);
            checkIfBoundContainPosition(pos);
            //map.setCenter(pos);
            //map.setCenter(13);
            setTimeout(function () {
                checkIfCurrentBoundContainMarker(listTypeMarkersNumber, currentFilter);

            }, 300);
        }
    }

    setTimeout(function () {
        isPostFilter = false;
    }, 700);

}

function loadByAjax(postTypeList, postTypeNumber) {

    if(selectedPostMarker){
        selectedPostMarker.setMap(null);

    }

    if (postTypeList.length == 0) {

        //$(document).ajaxStart(function () {

        //});

        $("#loading").modal({ closable: false }).modal('show');
        //$(document).ajaxStop(function () {


        //});

        //$(".abc").append('<img src="/Content/Icon/loading_spinner.gif" />');
        $.ajax({
            url: '/Map/GetListOfAPostType?PostType=' + postTypeNumber,
            type: 'GET',
            async: true,
            dataType: 'json',
            success: function (result) {

                for (var i = 0; i < result.length; i++) {
                    postTypeList.push({ postID: result[i].PostId, x: result[i].X, y: result[i].Y });
                }

                setTimeout(function () {
                    $("#loading").modal('hide');
                }, 700);

                switch (postTypeNumber) {
                    case 3: createListPostMarker(postTypeList, listType3Markers, overlappingType3, Type3Icon); showMarkersOnMap(postType3, 3, listType3Markers); break;
                    case 4: createListPostMarker(postTypeList, listType4Markers, overlappingType4, Type4Icon); showMarkersOnMap(postType4, 4, listType4Markers); break;
                    case 5: createListPostMarker(postTypeList, listType5Markers, overlappingType5, Type5Icon); showMarkersOnMap(postType5, 5, listType5Markers); break;
                    case 6: createListPostMarker(postTypeList, listType6Markers, overlappingType6, Type6Icon); showMarkersOnMap(postType6, 6, listType6Markers); break;
                    case 7: createListPostMarker(postTypeList, listType7Markers, overlappingType7, Type7Icon); showMarkersOnMap(postType7, 7, listType7Markers); break;
                    case 8: createListPostMarker(postTypeList, listType8Markers, overlappingType8, Type8Icon); showMarkersOnMap(postType8, 8, listType8Markers); break;
                    case 9: createListPostMarker(postTypeList, listType9Markers, overlappingType9, Type9Icon); showMarkersOnMap(postType9, 9, listType9Markers); break;
                }

            },
            error: function (xhr, status, error) {
                alert(xhr.responseText);
            }
        });
    } else {
        switch (postTypeNumber) {
            case 3: showMarkersOnMap(postType3, 3, listType3Markers); break;
            case 4: showMarkersOnMap(postType4, 4, listType4Markers); break;
            case 5: showMarkersOnMap(postType5, 5, listType5Markers); break;
            case 6: showMarkersOnMap(postType6, 6, listType6Markers); break;
            case 7: showMarkersOnMap(postType7, 7, listType7Markers); break;
            case 8: showMarkersOnMap(postType8, 8, listType8Markers); break;
            case 9: showMarkersOnMap(postType9, 9, listType9Markers); break;
        }
    }
}
function handleLocationError(browserHasGeolocation, message, pos) {
    alert(message);
}

var rad = function (x) {
    return x * Math.PI / 180;
};

var getDistance = function (p1, p2) {
    var R = 6378137; // Earth’s mean radius in meter
    var dLat = rad(p2.lat() - p1.lat());
    var dLong = rad(p2.lng() - p1.lng());
    var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(rad(p1.lat())) * Math.cos(rad(p2.lat())) *
      Math.sin(dLong / 2) * Math.sin(dLong / 2);
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    var d = R * c;
    return d; // returns the distance in meter
};

// Adds a marker to the map and push to the array.
function addMarker(location) {
    var marker = new google.maps.Marker({
        position: location,
        map: map
    });
    listMarkers.push(marker);
}

// Sets the map on all markers in the array.
function setMapOnAll(map, listMarkers) {
    for (var i = 0; i < listMarkers.length; i++) {
        listMarkers[i].setMap(map);
    }
}

// Removes the markers from the map, but keeps them in the array.
function clearMarkers(listMarkers) {
    setMapOnAll(null, listMarkers);
}

// Shows any markers currently in the array.
function showMarkers() {
    setMapOnAll(map);
}

// Deletes all markers in the array by removing references to them.
function deleteMarkers(listMarkers) {
    clearMarkers(listMarkersTest);
    //listMarkers = [];
    listMarkers = [];
}

function getUserInfo(userId) {
    if (isClickOnSpiderfier == false) {
        $.ajax({
            url: 'GetUserPartialView?userId=' + userId,
            type: 'GET',
            success: function (result) {
                if (result != '') {
                    $("#userModal").empty();
                    $("#userModal").html(result);
                    $("#userModal").modal('show');
                }
            },
            error: function (xhr, status, error) {
                alert(xhr.responseText);
            }
        });
    }
}

function getPostInfo(postID) {
    if (isClickOnSpiderfier == false) {
        $.ajax({
            url: '/Map/GetPostPartialView?postId=' + postID,
            type: 'GET',
            dataType: 'text',
            success: function (result) {
                //   createUserInfoWindowContent(json.UserName, 23, json.Gender, json.Location.Address);
                if (result != '') {

                    $("#postModal").empty();

                    $("#forModal").html(result);

                    $('#forModal').modal({
                        inverted: true
                    }).modal({
                        duration: 400,
                        onShow: function () {
                            //    history.pushState(null, null, "/Newsfeed/ShowPost/" + postID);
                        }
                    }).modal('show')
                    ;

                    $(".forHide")
                      .on('click',
                          function () {
                              $('#forModal').modal('hide');

                          });
                    var $carousel = $('.carousel').flickity({
                        imagesLoaded: true,
                        percentPosition: false
                    });

                    // get transform property
                    var docStyle = document.documentElement.style;
                    var transformProp = typeof docStyle.transform == 'string' ?
                      'transform' : 'WebkitTransform';

                    var flkty = $carousel.data('flickity');
                    var $imgs = $('.carousel-cell img');
                    $carousel.on('scroll.flickity', function () {
                        flkty.slides.forEach(function (slide, i) {
                            var img = $imgs[i];
                            var x = (slide.target + flkty.x) * -1 / 3;
                            img.style[transformProp] = 'translateX(' + x + 'px)';
                        });
                    });
                }
                window.addEventListener('popstate', function (e) {
                });
            },
            error: function (xhr, status, error) {
                alert(xhr.responseText);
            }
        });
    }
}

function showSelectedPostOnMap(Lat, Lng, PostType, PostId, isCallFromPostDetail) {
    isClickOnSpiderfier = false;
    isPostFilter = false;
    //currentFilter = PostType;
    currentFilter = -5;
    if(selectedPostMarker){
    selectedPostMarker.setMap(null);
    }
    var TypeIcon;
    returnToNormalState();

    currentMarkerClusterer.setMap(null);
    var pos = {
        lat: Lat,
        lng: Lng
    };

    switch (PostType) {
        case 3: TypeIcon = Type3Icon; break;
        case 4: TypeIcon = Type4Icon; break;
        case 5: TypeIcon = Type5Icon; break;
        case 6: TypeIcon = Type6Icon; break;
        case 7: TypeIcon = Type7Icon; break;
        case 8: TypeIcon = Type8Icon; break;
        case 9: TypeIcon = Type9Icon; break;
    }

    selectedPostMarker = new google.maps.Marker({
        position: pos,
        map: null,
        optimized: false,
        title: "Nhấp để xem chi tiết",
        icon: TypeIcon
    });

    selectedPostMarker.setMap(map);

    // Allow each marker to have an info window
    google.maps.event.addListener(selectedPostMarker, 'click', (function (selectedPostMarker) {
        return function () {
            setTimeout(function () {
                getPostInfo(PostId);
            }, 100);
        }
    })(selectedPostMarker));

    // map.setCenter({Lat,Lng});


    if (isCallFromPostDetail != 1) {
        var position = new google.maps.LatLng(Lat, Lng);
        map.setZoom(14);
        map.setCenter(position);
        //checkIfBoundContainPosition(position);
        setTimeout(function () {

            getPostInfo(PostId);

        }, 500);
        //getPostInfo(PostId);
    } else {
        map.setCenter({ lat: Lat, lng: Lng });
        map.setZoom(9);

    }
}

function smoothlyCenterPosition(pos) {
    if (bounds) {
        var sw = bounds.getSouthWest();
        var ne = bounds.getNorthEast();

        var lat1 = sw.lat();
        var lng1 = sw.lng();
        var lat2 = ne.lat();
        var lng2 = ne.lng();

        var dx = (lng1 - lng2) / 2.;
        var dy = (lat1 - lat2) / 2.;
        var cx = (lng1 + lng2) / 2.;
        var cy = (lat1 + lat2) / 2.;

        // work around a bug in google maps.///
        lng1 = cx + dx / 1.5;
        lng2 = cx - dx / 1.5;
        lat1 = cy + dy / 1.5;
        lat2 = cy - dy / 1.5;
        /////////////////////////////////////////

        sw = new google.maps.LatLng(lat1, lng1);
        ne = new google.maps.LatLng(lat2, lng2);
        bounds = new google.maps.LatLngBounds(sw, ne);
        map.panTo(pos);
        map.fitBounds(bounds);
        map.setCenter(pos);
        setTimeout(function () {
            smoothZoom(this.map, 13, map.getZoom());

        }, 200);
    }
}

function checkIfBoundContainPosition(pos) {
    if (map.getBounds().contains(pos) == false) {
        bounds.extend(pos);
        map.fitBounds(bounds);
        map.setCenter(pos);

        setTimeout(function () {
            // smoothZoom(this.map, 13, map.getZoom());
            smoothlyCenterPosition(pos);
        }, 1000);
    }
    else {
        //   map.fitBounds(map.getBounds());
        smoothlyCenterPosition(pos);
    }
}

function calculateNearestMarker(listLocation) {

    //bounds = map.getBounds();
    var centerOfCurrentBound = bounds.getCenter();
    if (listLocation.length > 0) {
        var position = new google.maps.LatLng(listLocation[0].x, listLocation[0].y);
        var min = getDistance(centerOfCurrentBound, position);
        var length = listLocation.length;
        for (var i = 1; i < length; i++) {
            var position2 = new google.maps.LatLng(listLocation[i].x, listLocation[i].y);
            var distance2 = getDistance(centerOfCurrentBound, position2)
            if (min > distance2) {
                min = distance2;
                position = position2;
            }
        }
        return position;
    }
}

function checkIfCurrentBoundContainMarker(listMarker, currentFilterNumber) {

    currentMarkerClusterer.removeMarkers(list);
    var currentListLength = list.length;

    for (var i = 0; i < currentListLength; i++) {
        list[i].setMap(null);
    }

    while (list.length != 0) {
        list.pop();

    }
    var length = listMarker.length;

    for (var i = 0; i < length; i++) {
        if (map.getBounds().contains(listMarker[i].position) == true) {
            //  listMarker[i].setMap(map);
            list.push(listMarker[i]);

        }
    }
    if (list.length == 0) {
        if ((currentFilterNumber == -1) || (currentFilterNumber == -2) || (currentFilterNumber == -3) || (currentFilterNumber == -4)) {
            if (listMarker.length == 0) {
                $("#userEmptyAlertModal").modal('show');
                returnToNormalState();
                currentFilter = -5;
            }
            else {
                //$("#nearestUserAlertModal").modal('show');
            }
        } else {
            if (listMarker.length == 0) {
                $("#postEmptyAlertModal").modal('show');
                returnToNormalState();
                currentFilter = -5;
            } else {
                //$("#nearestPostAlertModal").modal('show');

            }
        }
        return false;

    }
    for (var i = 0; i < list.length; i++) {
        list[i].setMap(map);
    }

    if (list.length == 1) {
        //smoothlyCenterPosition(list[0].position);
        //map.setCenter(list[0].position);
        //bounds.extend(list[0].position);
        //map.fitBounds(bounds);
        map.setCenter(list[0].position);

    }
    currentMarkerClusterer.addMarkers(list);
    currentMarkerClusterer.setMap(map);
    currentMarkerClusterer.setMaxZoom(10);

    return true;
}

function showAlertNoPost(postTypeArray) {
    if (postTypeArray.length == 0) {
        $("#postEmptyAlertModal").modal('show');
    }
}

function showAlertNoUser(postTypeArray) {
    if (postTypeArray.length == 0) {
        $("#userEmptyAlertModal").modal('show');
    }
}

function hideModel() {
    $("#userModal").modal('hide');
}

function closeAllSpiderfier() {
    overlappingUsers.unspiderfy();
    overlappingMale.unspiderfy();
    overlappingLGBT.unspiderfy();
    overlappingFemale.unspiderfy();
    overlappingType3.unspiderfy();
    overlappingType4.unspiderfy();
    overlappingType5.unspiderfy();
    overlappingType6.unspiderfy();
    overlappingType7.unspiderfy();
    overlappingType8.unspiderfy();
    overlappingType9.unspiderfy();
}
//window.onload = initialize;
google.maps.event.addDomListener(window, 'load', initialize);
//$(document).ready(initialize);