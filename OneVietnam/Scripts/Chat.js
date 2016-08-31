//this function can remove a array element.
Array.remove = function (array, from, to) {
    var rest = array.slice((to || from) + 1 || array.length);
    array.length = from < 0 ? array.length + from : from;
    return array.push.apply(array, rest);
};

//this variable represents the total number of popups can be displayed according to the viewport width
var total_popups = 0;

//arrays of popups ids
var popups = [];
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}
//this is used to close a popup
function close_popup(id) {
    for (var iii = 0; iii < popups.length; iii++) {
        if (id == popups[iii]) {
            Array.remove(popups, iii);

            document.getElementById(id).style.display = "none";

            calculate_popups();

            return;
        }
    }
}

//displays the popups. Displays based on the maximum number of popups that can be displayed on the current viewport width
function display_popups() {
    var right = 0;

    var iii = 0;
    for (iii; iii < total_popups; iii++) {
        if (popups[iii] != undefined) {
            var element = document.getElementById(popups[iii]);
            element.style.right = right + "px";
            right = right + 320;
            element.style.display = "block";
        }
    }

    for (var jjj = iii; jjj < popups.length; jjj++) {
        var element = document.getElementById(popups[jjj]);
        element.style.display = "none";
    }
}

//creates markup for a new popup. Adds the id to popups array.
function register_popup(id, name, avatarSrc) {
    for (var iii = 0; iii < popups.length; iii++) {
        //already registered. Bring it to front.
        if (id == popups[iii]) {
            Array.remove(popups, iii);
            popups.unshift(id);
            $("#" + id + "Conversation").scrollTop(document.getElementById(id + "Conversation").scrollHeight);
            calculate_popups();
            return;
        }
    }

    var textInput = '<div class="ui fluid input"><textarea onkeypress="if (event.keyCode === 13) { sendMessage(\'' + id + '\'); return false;}" rows="2" style="max-height : 45px;width:100%;border-left-style: none;border-right-style:none" id="' + id + 'Input"></textarea></div>';
    var conversation = '<div style="height:238px; margin-bottom:10px!important ;overflow-y:scroll " id="' + id + 'Conversation" class="ui items"></div>';
    var element = '<div class="popup-box chat-popup" id="' + id + '" style="z-index=1000!important">';
    element = element + '<div class="popup-head">';
    element = element + '<div class="popup-head-left"> <a href="/Timeline/Index/'+id+'" style="color:white"><img class="ui avatar image" src="' + avatarSrc + '"/><span>' + name + '</span></a></div>';
    element = element + '<div class="popup-head-right"><a href="javascript:close_popup(\'' + id + '\');">&#10005;</a></div>';
    element = element + '<div style="clear: both"></div></div><div class="popup-messages">' + conversation + textInput + '</div></div>';

    document.getElementById("ChatZone").innerHTML = document.getElementById("ChatZone").innerHTML+element;

    popups.unshift(id);

    calculate_popups();
    seenConversation(id);
    $("#" + id + "Conversation").text("");
    getConversationById(id);
}
function seenConversation(id) {
    if (document.getElementById(id + "Conversations") == null) return;
    if (document.getElementById(id + "Conversations").style.background != "") {        
        if (parseInt($('#MessageNotification').text()) == 1) {
            $('#MessageNotification').hide();
        } else {
            $('#MessageNotification').text(parseInt($('#MessageNotification').text()) - 1);            
        }
        document.getElementById(id + "Conversations").style.background = "";
    }
}
// send message to hub server
function sendMessage(friendId) {
    var message = document.getElementById(friendId + "Input").value.trim();
    if (message === null || message === "") return;
    $.connection.hub.start().done(function () {
        // Call the Send method on the hub.                    
        window.myHub.server.sendChatMessage(friendId, message);
        // Clear text box and reset focus for next comment.
        $("#" + friendId + "Conversation").append('<div class="item" style="text-align: right"><div class="content" style="margin-right:0.5em"><p class="sendMessage">' + htmlEncode(message) + '</p></div></div>');
        $("#" + friendId + "Conversation").scrollTop(document.getElementById(friendId + "Conversation").scrollHeight);
        $("#" + friendId + "Input").val('').focus();
    });
}
function getConversationById(friendId) {
    var controller = "/Home/GetConversationById";
    var data = {
        friendId: friendId
    }
    $.ajax({
        type: 'POST',
        url: controller,
        data: data,
        success: function (data, textstatus) {
            if (data == '') return;
            $("#" + friendId + "Conversation").val('');
            for (var i = 0; i < data.length; i++) {
                if (data[i]["Type"] == '0') {
                    $("#" + friendId + "Conversation").append('<div class="item" style="text-align: right"><div class="content" style="margin-right:0.5em"><p class="sendMessage">' + htmlEncode(data[i]["Content"]) + '</p></div></div>');
                } else {
                    $("#" + friendId + "Conversation").append('<div class="item"><div class="content" style="margin-left:0.5em"><p class="receiveMessage">' + htmlEncode(data[i]["Content"]) + '</p></div></div>');
                }
            }
            $("#" + friendId + "Conversation").scrollTop(document.getElementById(friendId + "Conversation").scrollHeight);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
        }
    });
}
//Load Conversation to navbar
$('#myConversations').click(function () {
    getConversations();
});
var noConversation = "Bạn không có tin nhắn nào";
function getConversations() {
    var controller = "/Home/GetConversations";
    $.ajax({
        type: 'POST',
        url: controller,
        success: function (conversations, textstatus) {
            if (conversations.length == 0) {
                var not;
                not = '<div class="item conversations" style="padding:0em!important">';
                not = not + '<div style="width: 300px; height: 30px; margin-top: 10px;text-align: center">' + noConversation + '</div>';
                document.getElementById('Conversations').innerHTML = "";
                $("#Conversations").append(not);                
                return;
            };            
            document.getElementById('Conversations').innerHTML = "";
            for (var i = 0; i < conversations.length; i++) {
                var con;
                if (conversations[i]["Seen"] == false) {
                    con = '<div class="item conversations" id="'+conversations[i]["Id"] + 'Conversations"  style="padding:0em!important;background: #E0F2F1 !important;">';
                } else {
                    con = '<div class="item conversations" id="' + conversations[i]["Id"] + 'Conversations" style="padding:0em!important">';
                }
                con = con + '<a class="item" href="javascript:register_popup(\'' + htmlEncode(conversations[i]["Id"]) + '\',\'' + htmlEncode(conversations[i]["FriendName"]) + '\',\'' + htmlEncode(conversations[i]["Avatar"]) + '\');" style="padding: 0 !important">';
                con = con + '<div class="item">';
                con = con + '<div class="ui mini image" style="margin-right: 1em !important">';
                con = con + '<img class="ui mini image" src="' + htmlEncode(conversations[i]["Avatar"]) + '"/>';
                con = con + '</div>';
                con = con + '<div class="content">';
                con = con + '<div class="header">' + htmlEncode(conversations[i]["FriendName"]) + '</div>';
                con = con + '<div class="description messagePreview">';                
                if (conversations[i]["LastestType"] == '0') {
                    con = con + '<i class="mini reply icon"></i>';
                }
                con = con + htmlEncode(conversations[i]["LastestMessage"]);                
                con = con + '</div>';
                con = con + '<div class="extra">';
                con = con + '<i>';
                con = con + conversations[i]["UpdatedDate"].toString();
                con = con + '</i>';
                con= con + '</div>'
                con = con + '</div>';
                con = con + '</div>';
                con = con + '</a>';
                con = con + '<a href="javascript:remove_conversation(\'' + conversations[i]["Id"] + '\');">';
                con = con + '<i class="minus circle red icon"></i>';
                con = con + '</a>';
                con = con + '</div>';
                $("#Conversations").append(con);
            }            
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("Can not load conversations");
        }
    });
}
function remove_conversation(id) {
    var controller = "/Home/RemoveConversationById";
    var data = {
        id: id
    }
    $.ajax({
        type: 'POST',
        url: controller,
        data:data,
        success: function (data) {
            if (data == 'True') {
                seenConversation(id);
                $("#" + id + "Conversations").remove();
                close_popup(id);
            }
        }
    });
}
//calculate the total number of popups suitable and then populate the toatal_popups variable.
function calculate_popups() {
    var width = window.innerWidth;
    if (width < 540) {
        total_popups = 0;
    }
    else {
        //width = width - 200;
        //320 is width of a single popup box
        total_popups = parseInt(width / 320);
    }

    display_popups();

}

//recalculate when window is loaded and also when window is resized.
window.addEventListener("resize", calculate_popups);
window.addEventListener("load", calculate_popups);