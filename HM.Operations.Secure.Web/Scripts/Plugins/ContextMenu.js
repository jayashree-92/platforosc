/*
* File:        ContextMenu.js
* Version:     1.0.0
* Description: Provides the wrapper for context menu over any elements in Bootstrap UI
* Author:      Mohan Srikanth R. 
* Language:    Javascript
* License:	   Eclipse Public License - v 1.0
* 
* ©2016 HedgeMark International, LLC. All rights reserved.
*
*/

(function ($) {

    function uniqueIdGenerator() { };
    uniqueIdGenerator.prototype.rand = Math.floor(Math.random() * 26) + Date.now();
    uniqueIdGenerator.prototype.getId = function () {
        return this.rand++;
    };
    var idGen = new uniqueIdGenerator();

    $.fn.contextMenuUI = function (options) {
        var oSettings = $.extend({
            //sClass: "default",
            aoMenu: [{ value: "Copy", href: "#", callback: function () { }, sClass: "", id: "" }]
        }, options);

        var oElement = $(this);
        var elementUId = idGen.getId();
        var heightOfContextMenu = 6 + oSettings.aoMenu.length * 22;
        //Validation of initialization
        $.each(oSettings.aoMenu, function (i, menu) {
            var uId = idGen.getId();
            if (menu.value == undefined)
                menu.value = "Menu item " + (i + 1);
            if (menu.id == undefined)
                menu.id = "ctxMenu_li_" + uId;
            if (menu.sClass == undefined)
                menu.sClass = "";
            if (menu.href == undefined)
                menu.href = "#";
            if (menu.callback == undefined)
                menu.callback = function () { };
            else {
                menu.href = "#";
            }
        });

        var contextMenuHtml = "<ul id=\"ctxMenuUI_" + elementUId + "\" class=\"dropdown-menu\">";

        $.each(oSettings.aoMenu, function (index, menu) {

            if (menu.sClass === "divider") {
                contextMenuHtml += "<li role=\"separator\" class=\"divider\"></li>";
                return;
            }

            contextMenuHtml += "<li id='" + menu.id + "' class='" + menu.sClass + "'>" +
                "<a href='" + menu.href + "' target='" + (menu.href !== "#" ? "_blank" : "") + "'>" + menu.value + "</a>" +
                "</li>";
        });

        $("body").append(contextMenuHtml);

        $("body").on("click", function () {
            $("#ctxMenuUI_" + elementUId).hide(200);
        });

        $("body").on("contextmenu", oElement.selector, function (event) {
            event.preventDefault();
            var topPosition = (event.pageY - 3);
            var leftPosition = (event.pageX);
            if (document.body.clientHeight - (event.screenY - 70) < heightOfContextMenu) {
                topPosition = (event.pageY - heightOfContextMenu);
            }

            $("#ctxMenuUI_" + elementUId).css({
                "position": "absolute",
                "left": leftPosition + "px",
                "top": topPosition + "px",
                "opacity": 0.95
            });

            $("#ctxMenuUI_" + elementUId).css("display", "none").show(200);

            var thisElement = $(this);
            $.each(oSettings.aoMenu, function (index, menu) {

                if (menu.href !== "#")
                    return;

                if ($("#" + menu.id).hasClass("disabled"))
                    return;

                $("#" + menu.id).off("click").on("click", function (event) {
                    $("#ctxMenuUI_" + elementUId).hide(200);
                    event.preventDefault();
                    event.stopImmediatePropagation();
                    menu.callback.call(thisElement);
                });
            });
        });
    };
})(jQuery);

