$(document)
    .ready(function () {
      
        
//        $('.ui.sticky')
//  .sticky({
//      context: '#context'
//  })
//        ;
       
        $("#getloc").click();
        $(".filter-post").dropdown({
            allowCategorySelection: true
        })
        ;
        $("#locationDr").dropdown({
            allowCategorySelection: true
        })
        ;


        $(".ui.toggle.button")
            .state({
                text: {
                    inactive: 'Bật',
                    active: 'Tắt'
                }
            })
        ;
          $('.icon')
        .popup()
          ;

        $("#messageIcon").click(function () {
            //var div = document.getElementById("messagechat");
            //div.innerHTML = div.innerHTML + 'Hello World';
            $("#messages").slideToggle();

        });
        //$('.icon').popup();

        if ($(".searchType").val() === "SearchPosts") {
            $(".ui.user").css("display", "none");
            $(".ui.post").css("display", "inline-flex");
        } else {
            $(".ui.user").css("display", "inline-flex");
            $(".ui.post").css("display", "none");
        }
        $(".searchType")
            .dropdown({

                onChange: function (value, text, $selectedItem) {
                    $("#currentSearch").val(value);
                    if (value === "SearchPosts") {
                        $(".ui.user").css("display", "none");
                        $(".ui.post").css("display", "inline-flex");

                    } else {
                        $(".ui.user").css("display", "inline-flex");
                        $(".ui.post").css("display", "none");
                    }
                }
            });
        $(".right.menu.open")
            .on("click",
                function (e) {
                    e.preventDefault();
                    $(".ui.vertical.menu.open").toggle();
                });


        //SearchBox
        $('.ui.search.post')
            .search({
                apiSettings: {
                    url: '/Search/search?query={query}'
                },
                fields: {
                    results: 'Result',
                    title: 'Title',
                    description: 'Description',
                    url: 'Url'
                },
                minCharacters: 3
            });
        $('.ui.search.user')
            .search({
                apiSettings: {
                    url: '/Search/UsersSearch?query={query}'
                },
                fields: {
                    results: 'Result',
                    title: 'Title',
                    description: 'Description',
                    url: 'Url',
                    image: '/Content/Images/Avatar_Default.jpg'
                },
                minCharacters: 2
            });

        //ThamDTH
        $('.clearing.star.rating').rating('setting', 'clearable', true);
        $('.ui.multiple.dropdown')
          .dropdown({
              allowAdditions: true,
              minCharacters: 2
          });
        $("#CreatePostForm").find('input:text').on('keypress', function () {
            if (event.keyCode === 13) {
                event.preventDefault();
            }
        });

        $('#drPostType')
          .dropdown({});

        $("#drPostTypeEditPost").dropdown();

        $("#drdGender").dropdown({});

        $('.ui.edit.button').click(function () {
            $('.ui.fullscreen.modal').modal('show');
        });

        $('.icon.link').popup({});

        $('.tabular.menu .item').tab({});

        $('#AdministrationsMenu .menu .item').tab({ context: $('#AdministrationsMenu') });

        //Message for Setting tab in Timeline page
        $('.message .close').on('click', function () {
      $(this).closest('.message')
            .transition('fade');});

        //Admin panel search user Begin

        $("#dvSearchUsers")
    .search({
        apiSettings: {
            url: '/Search/UsersSearch?query={query}'
        },
        fields: {
            results: 'Result',
            title: 'Title'
        },
        minCharacters: 2
    });

        $("#drSearchUserRole").dropdown({});

        $("#txtSearchUserName").on('keypress', function (event) {
            if (event.keyCode === 13) {
                $("#UserSearchAdminPanel").submit();
                event.preventDefault();
            }
        });

        $("#chkIsOnline").on('change', function () {
            $("#UserSearchAdminPanel").submit();
        });

        $("#txtSearchUserRole").on('change', function () {
            $("#UserSearchAdminPanel").submit();
        });

        $("#dtCreatedDateFrom").change(function () {
            $("#UserSearchAdminPanel").submit();
        });

        $("#dtCreatedDateTo").change(function () {
            $("#UserSearchAdminPanel").submit();
        });
        //Admin panel search user end

        //Search admin post begin
        $("#dvSearchPostTitle")
            .search({
                apiSettings: {
                    url: '/Search/AdminSearch?query={query}'
                },
                fields: {
                    results: 'Result',
                    title: 'Title'
                },
                minCharacters: 2
            });

        $("#txtSearchPostTitle").on('keypress', function (event) {
            if (event.keyCode === 13) {
                $("#PostSearchAdminPanel").submit();
                $("#dvSearchPostTitle").find('.force100').removeClass('visible').addClass('hidden').removeAttr('style');
            }
        });

        $('[name="rdStatus"]').on('click', function () {
            $("#PostSearchAdminPanel").submit();
        });

        $("#dtPostCreatedDateFrom").change(function () {
            $("#PostSearchAdminPanel").submit();
        });

        $("#dtPostCreatedDateTo").change(function () {
            $("#PostSearchAdminPanel").submit();
        });

        //Search admin post end

        //Search report begin

        $("#dtReportCreatedDateFrom").change(function () {
            $("#ReportSearchAdminPanel").submit();
        });
        $("#dtReportCreatedDateTo").change(function () {
            $("#ReportSearchAdminPanel").submit();
        });

        $('[name="rdReportStatus"]').on('click', function () {
            $("#ReportSearchAdminPanel").submit();
        });

        //Search report end


        //ToanLM


        var $grid = $('.grids').isotope({
            itemSelector: '.grid-item',
            percentPosition: true,
            masonry: {
                columnWidth: '.grid-sizer',
                gutter: '.gutter-sizer'
            }
        });
        
        // change size of item by toggling gigante class
        $grid.on('click', '.clickable', function (e) {
           
            var id = $(this).parent().find('#postId').val();

            $.ajax({
                type: 'GET',
                data: { "postId": id },
                url: '/Newsfeed/_ShowPostDetailModal',
                success: function (partialResult) {
                    $("#forModal").empty();
                    $("#forModal").html(partialResult);
                    $(".modal")
                            .modal({
                                allowMultiple: true
                            });
                    $('#forModal').modal({
                        duration: 300,
                        onHide: function () {
                            history.back();
                            $('#forSuggest').empty();
                        }, onShow: function () {
                            history.pushState(null, null, "/Newsfeed/ShowPostDetailPage/" + id);
                        }
                    }).modal('show')
                    ;
                    $(".forHide")
                        .on('click',
                            function() {
                                $('#forModal').modal('hide');

                            });
                    var $carousel = $('.carousel').flickity({
                        imagesLoaded: true,
                        percentPosition: false
                    });
                   
                   

                 
                    var docStyle = document.documentElement.style;
                    var transformProp = typeof docStyle.transform == 'string' ?
                      'transform' : 'WebkitTransform';
         
                   
                    var flkty = $carousel.data('flickity');
                    var $imgs = $('.carousel-cell img');
                    $carousel.on( 'scroll.flickity', function() {
                        flkty.slides.forEach( function( slide, i ) {
                            var img = $imgs[i];
                            var x = ( slide.target + flkty.x ) * -1/6;
                            img.style[ transformProp ] = 'translateX(' + x  + 'px)';
                        });
                        });

                }
            });

            window.addEventListener('popstate', function (e) {
                //Todo:back button
            }); 
            $grid.isotope('layout');
        });
        var isStamped = false;
        $('#stamp-button').on('click', function () {
            history.pushState(null,null,"/Newsfeed");
            $('body,html').animate({
                scrollTop: 0                       // Scroll to top of body
            }, 500);
            $grid.isotope('layout');
            $("#CreatePostForm").data('validator').resetForm();
            $("#CreatePostForm").find("#result").html("");
            $("#CreatePostForm").find('#Title, #Description, #TagsInput').val('');
            getCurrentLocation();
            
            ImgList.splice(0, ImgList.length);
            $(".validation-summary-errors ul li").remove();
            $(".validation-summary-errors").addClass('validation-summary-valid').removeClass('validation-summary-errors');
            $(".stamp").toggleClass("hides");
            $(".edits").toggleClass("edits-cl");
            $(".plus").toggleClass("plus-cl");
            // stamp or unstamp element

            // trigger layout
        
            // set flag
            isStamped = !isStamped;
        });

        $('#btnCancelCreatePost')
            .on('click',
                function() {
                    history.pushState(null, null, "/Newsfeed");
                    $('body,html')
                        .animate({
                                scrollTop: 0 
                            },
                            500);
                    $grid.isotope('layout');
                    $("#CreatePostForm").data('validator').resetForm();
                    $("#CreatePostForm").find("#result").html("");
                    $("#CreatePostForm").find('#Title, #Description, #TagsInput').val('');
                    getCurrentLocation();

                    ImgList.splice(0, ImgList.length);
                    $(".validation-summary-errors ul li").remove();
                    $(".validation-summary-errors")
                        .addClass('validation-summary-valid')
                        .removeClass('validation-summary-errors');
                    $("#CreatePostForm")[0].reset();

                    $(".stamp").toggleClass("hides");
                    $(".edits").toggleClass("edits-cl");
                    $(".plus").toggleClass("plus-cl");
                    isStamped = !isStamped;
                });

        $('#stamp-button2')
            .on('click',
                function () {
                    //document.location = "/Newsfeed?ShowPostDetailPage";
                    $(".stamp").toggleClass("hides");
                    $(".edits").toggleClass("edits-cl");
                    $(".plus").toggleClass("plus-cl");

                    
                    $("#CreatePostForm").data('validator').resetForm();
                    $("#CreatePostForm").find("#result").html("");
                    $("#CreatePostForm").find('#Title, #Description, #TagsInput').val('');
                    getCurrentLocation();

                    ImgList.splice(0, ImgList.length);
                    $(".validation-summary-errors ul li").remove();
                    $(".validation-summary-errors")
                        .addClass('validation-summary-valid')
                        .removeClass('validation-summary-errors');
                    $("#CreatePostForm")[0].reset();
                });

        // ===== Scroll to Top ==== 
        $(window).scroll(function () {
            if ($(this).scrollTop() >= 50) {        // If page is scrolled more than 50px
                $('#return-to-top').fadeIn(200);    // Fade in the arrow
            } else {
                $('#return-to-top').fadeOut(200);   // Else fade out the arrow
            }
        });
        $('#return-to-top').click(function () {      // When arrow is clicked
            $('body,html').animate({
                scrollTop: 0                       // Scroll to top of body
            }, 300);
        });
       

    });

$('#MessageButton').dropdown({
});
$('.ui.pointing.dropdown').dropdown({
});
