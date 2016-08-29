function addPhone() {
    //    var phoneTextval = $("#PhoneNumber").val();
    //    if (phoneTextval === "") $("#title").text("Thêm số điện thoại");
    //    else {
    //        $("#phonetext").val(phoneTextval);
    //        $("#title").text("Cập nhật số điện thoại");
    //        $("#add").text("Sửa");
    //    }

    $('#addPhoneModal').modal('show');
    $("#countryDr").dropdown({
        onChange: function (value, text, $selectedItem) {
            $("#phonetext").val("+" + $selectedItem.attr("data-value"));
        }
    });
}
function editableForm() {
    $('.tog').toggleClass('disabled');
    $('#btnSaveEditProfile').toggleClass('hides');
    $('#btnCancelEditProfile').toggleClass('hides');
    $("#btnEditProfile").hide();
    $("#lblGender").hide();
    $("#drdGender").show();
    $("#btnUpdateLocation").toggleClass('hides');
    $("#drdGender").dropdown({});
    $("#DateOfBirth").attr('type', 'date');
    $("#btnConfirmTelephone").toggleClass('hides');
    $("#drdGenderIcon").show({});
}

function submitEditProfile() {
    var oldName = document.getElementById("lblTimeLineUserName");
    var oldHeaderName = document.getElementById("txtHeaderUserName");
    var currentName = document.getElementById("UserName");

    oldName.innerText = currentName.value;
    oldHeaderName.innerText = currentName.value;
    showUserMarkerOnMap(x, y, address);
}

function cancelEditProfile(pUrl) {
    $.ajax({
        type: 'GET',
        url: pUrl,
        success: function (partialResult) {
            $("#EditProfileForm").html("");
            $("#EditProfileForm").html(partialResult);
            showUserMarkerOnMap(x, y, address);
        }
    });
}

function changeTwoFactorAuthentication(pUrl) {
    var param = $(".ui.toggle.button").text();
    //TaiLM : Users can enable two-factor authentication by email even there are no phonenumber
    //if (param === "Bật" && $("#txtMobilePhone").val() === "") {
    //    $('.message')[0].className = "ui negative message";
    //    $(".ui.toggle.button")[0].click();
    //    return;
    //}               
    $.ajax({
        type: 'POST',
        url: pUrl,
        data: { 'value': param },
        success: function () {
            $('.message')[0].className = "ui negative message transition hidden";
        }
    });
}

function showChangePasswordForm(pUrl) {
    $.ajax({
        type: 'GET',
        url: pUrl,
        success: function (partialResult) {
            $("#ChangePasswordForm").html(partialResult);
            $("#ShowPassword").html("");
            $("#btnChangePass").hide();
        }
    });
}

function cancelChangePassword() {
    $("#ChangePasswordForm").html("");
    $("#ShowPassword").html("Thay đổi mật khẩu thường xuyên để nâng cao bảo mật hơn");
    $("#btnChangePass").show();
}


function closeChangePasswordForm() {
    var errorMsg = document.querySelector(".validation-summary-errors");
    if (errorMsg === null) {
        $("#ChangePasswordForm").html("");
        $("#ShowPassword").html("Thay đổi mật khẩu thường xuyên để nâng cao bảo mật hơn");
        $("#btnChangePass").show();
    }

}

function showSetPasswordForm(pUrl) {
    $.ajax({
        type: 'GET',
        url: pUrl,
        success: function (partialResult) {
            $("#ChangePasswordForm").html(partialResult);
            $("#ShowPassword").html("");
            $("#btnChangePass").hide();
        }
    });
}

function cancelSetPassword() {
    $("#ChangePasswordForm").html("");
    $("#ShowPassword").html("Tạo mật khẩu mới đê đăng nhập bằng email");
    $("#btnChangePass").show();
}

function closeSetPasswordForm(url) {
    var errorMsg = document.querySelector(".validation-summary-errors");
    if (errorMsg === null) {
        $("#ChangePasswordForm").html("");
        $("#ShowPassword").html("Thay đổi mật khẩu thường xuyên để nâng cao bảo mật hơn");
        $("#btnChangePass").show();
        $("#btnChangePass").val('Đổi');
        $("#btnChangePass")
            .click(function () {
                showChangePasswordForm(url);
            });
    }
}

function showUserMarkerOnMap(x, y, address) {
    //Declare icon for userLocationmarker
    icon = {
        url: "/Content/Icon/location.png",
        size: new google.maps.Size(71, 71),
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(17, 34),
        scaledSize: new google.maps.Size(25, 25)
    };

    // Declare a myLocation marker using icon declared above, and bind it to the map
    userLocationMarker = new google.maps.Marker({
        title: address,
        icon: icon
    });
    map2 = new google.maps.Map(document.getElementById('divShowMap'), {
        center: { lat: x, lng: y },
        zoom: 10,
        minZoom: 4,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });

    userLocationMarker.setPosition({ lat: x, lng: y });
    userLocationMarker.setMap(map2);

    google.maps.event.addListenerOnce(map2, 'idle', function () {
        google.maps.event.trigger(map2, 'resize');
        map2.setCenter({ lat: x, lng: y });
        map2.setZoom(10);
    });
}

function updateCurrentLocation() {
    var addr = document.getElementById("Location_Address");
    var xcoordinate = document.getElementById("Location_XCoordinate");
    var ycoordinate = document.getElementById("Location_YCoordinate");

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var pos = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            };

            var geocoder = new window.google.maps.Geocoder();             // create a geocoder object
            var location = new window.google.maps.LatLng(pos.lat, pos.lng);    // turn coordinates into an object          
            geocoder.geocode({ 'latLng': location },
                function (results, status) {
                    if (status === window.google.maps.GeocoderStatus.OK) { // if geocode success
                        var detailedLocation = results[0]
                            .formatted_address; // if address found, pass to processing function
                        if (detailedLocation.indexOf("Unnamed Road,") != -1) {
                            var NoEnglishLocation = detailedLocation.replace("Unnamed Road,", "");
                        }
                        addr.value = NoEnglishLocation;
                        xcoordinate.value = pos.lat;
                        ycoordinate.value = pos.lng;
                        userLocationMarker.setTitle(addr);
                        userLocationMarker.setPosition({ lat: pos.lat, lng: pos.lng });
                        userLocationMarker.setMap(map);
                        map.setCenter({ lat: pos.lat, lng: pos.lng });
                    } else {
                    }
                });
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


function ShowThankModal(userId) {
    if ($("#modalReport_" + userId).find('.validation-summary-errors').length > 0) {
        $("#modalReport_" + userId).modal('show');
    } else {
        $("#reportedModal_" + userId).modal('show');
        cancelReport(userId);
    }
}
function cancelReport() {
    $(".validation-summary-errors ul li").remove();
    $(".input-validation-error").removeClass('input-validation-error');
    $(".validation-summary-errors").addClass('validation-summary-valid').removeClass('validation-summary-errors');
    $("#ReportDescription").val('');
}