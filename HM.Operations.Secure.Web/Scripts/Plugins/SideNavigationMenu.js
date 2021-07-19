/*
* File:        SideNavigationMenu.js
* Version:     1.0.0
* Description: Provides the wrapper for creating a sidebar menu open/close Bootstrap UI
* Author:      Mohan Srikanth R. 
* Language:    Javascript
* License:	   Eclipse Public License - v 1.0
* 
* ©2017 HedgeMark International, LLC. All rights reserved.
*
*/

(function ($) {

    $(".sidenav").prepend("<button class=\"btn sidenav-togglebtn\"></button>");
    $(".sidenav").prepend("<div class=\"btn sidenav-selectedVal\"></div>");


    function toggleSlideFrame($sidenav, $sidenavToggleBtn, $sidenavSelectedVal, $sidenavFrame) {
        if ($sidenavToggleBtn.hasClass("in")) {
            $sidenavToggleBtn.removeClass("in");

            $sidenavToggleBtn.html("<i class='glyphicon glyphicon-triangle-right'></i>");
            $sidenav.css("width", "3.5%");

            window.setTimeout(function () {
                $sidenavFrame.animate({ "width": "94%" });
                $sidenav.children(':not(.sidenav-togglebtn)').hide(100);
                $sidenavSelectedVal.show();
                $sidenavSelectedVal.html($sidenav.find("li.active").html());

                //$('.sidenav-frame table:eq(0), .sidenav').equalizeHeights();
            }, 300);


        } else {

            $sidenavToggleBtn.addClass("in");
            $sidenav.children(':not(.sidenav-togglebtn)').show(100);
            $sidenavSelectedVal.hide();
            $sidenavToggleBtn.html("<i class='glyphicon glyphicon-triangle-left'></i>");
            $sidenavFrame.animate({ "width": "84%" }, 100, function () {
                $sidenav.css("width", "16%");
                //$('.sidenav-frame table, .sidenav').equalizeHeights();
            });
        }
    }

    $(".sidenav .sidenav-togglebtn").on("click", function () {

        var $sidenav = $(this).parent();
        var $sidenavToggleBtn = $(this);
        var $sidenavSelectedVal = $sidenav.children('.sidenav-selectedVal');
        var $sidenavFrame = $sidenav.parent().find('.sidenav-frame');

        toggleSlideFrame($sidenav, $sidenavToggleBtn, $sidenavSelectedVal, $sidenavFrame);
    });

    $(".sidenav .sidenav-selectedVal").on("click", function (event) {
        event.stopImmediatePropagation();
        event.stopPropagation();
        event.preventDefault();

        var $sidenav = $(this).parent();
        var $sidenavToggleBtn = $sidenav.children(".sidenav-togglebtn");
        var $sidenavSelectedVal = $sidenav.children('.sidenav-selectedVal');
        var $sidenavFrame = $sidenav.parent().find('.sidenav-frame');

        toggleSlideFrame($sidenav, $sidenavToggleBtn, $sidenavSelectedVal, $sidenavFrame);

    });

    $(".sidenav .sidenav-togglebtn").trigger("click");

    //$('.sidenav-frame table:eq(0),.sidenav-togglebtn, .sidenav').equalizeHeights();
    //window.setTimeout(function() {
    //    $('.sidenav-frame table, .sidenav-menu').equalizeHeights();
    //}, 2000);

    //$('.sidenav-menu,.sidenav-frame ').equalizeHeights();
    //$.fn.sideNav = function (options) {
    //    var oSettings = $.extend({
    //        sClass: "default",
    //        aoMenu: [{ value: "Copy", href: "#", callback: function () { }, sClass: "", id: "" }]
    //    }, options);


    //};
})(jQuery);

