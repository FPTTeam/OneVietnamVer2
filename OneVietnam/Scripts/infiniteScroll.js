var page = 1,
    inCallback = false,
    hasReachedEndOfInfiniteScroll = false;

var scrollHandler = function () {
    if (hasReachedEndOfInfiniteScroll === false &&
            ($(window).scrollTop() >= ($(document).height() - $(window).height() - 100))) {        
        loadMoreToInfiniteScrollTable(moreRowsUrl);
    }
}
$('.filter-items').on('click', '.item', function () {
    page = 0;    
    $(forLoad).empty();
     filterValue = $(this).attr('data-filter');
    loadMoreToInfiniteScrollTable(moreRowsUrl);


});
var ulScrollHandler = function () {
    if (hasReachedEndOfInfiniteScroll === false &&
            ($(window).scrollTop() === $(document).height() - $(window).height()-100)) {
       
        loadMoreToInfiniteScrollUl(moreRowsUrl);
    }
}

function loadMoreToInfiniteScrollUl(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        
        $("div#loading").show();
        $.ajax({
            type: 'GET',
            url: loadMoreRowsUrl,
            data: "pageNum=" + page,
            success: function (data, textstatus) {
                if (data !=="") {
                    $("ul.infinite-scroll").append(data);
                }
                else {
                    page = -1;
                }

                inCallback = false;

                $("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function loadMoreToInfiniteScrollTable(loadMoreRowsUrl) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        if (typeof filterValue === 'undefined') filterValue = -1;
        $.ajax({
            type: 'GET',
            url: loadMoreRowsUrl,
            data: { pageNum: page, filterVal: filterValue },
            success: function (data, textstatus) {
                if (data !=="") {
                    var $items = $(data);
                    $(forLoad).append($items);
                    grid.isotope('appended', $items);   
                    grid.isotope('layout');
                    grid.imagesLoaded().progress(function () {
                        grid.isotope('layout');
                    });
                   
                }
                else {
                    page = -1;
                }
                inCallback = false;
            },
            async: false,
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Can not load more data infiniteScroll");
            }
        });
    }
}
function smoothZoom(map, max, cnt) {
    if (cnt >= max) {
        return;
    }
    else {
        z = google.maps.event.addListener(this.map, 'zoom_changed', function (event) {
            google.maps.event.removeListener(z);
            smoothZoom(this.map, max, cnt + 1);
        });
        setTimeout(function () { this.map.setZoom(cnt) }, 100); // 80ms is what I found to work well on my system -- it might not work well on all systems
    }
}
function showNoMoreRecords() {
    hasReachedEndOfInfiniteScroll = true;
}