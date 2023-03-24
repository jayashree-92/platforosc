var loadingDiv = $("#loading");

function extractor(query) {
    var result = /([^/[.=]+)$/.exec(query);
    if (result && result[1])
        return result[1].trim();
    return "";
}
var defaultTimeZone = "EST";
var abbrs = {
    EST: 'Eastern Standard Time',
    EDT: 'Eastern Daylight Time',
    CST: 'Central Standard Time',
    CDT: 'Central Daylight Time',
    MST: 'Mountain Standard Time',
    MDT: 'Mountain Daylight Time',
    PST: 'Pacific Standard Time',
    PDT: 'Pacific Daylight Time',
    GMT: 'Greenwich Mean Time'
};

moment.fn.getZoneName = function () {
    return abbrs[defaultTimeZone] || defaultTimeZone;
};
moment.fn.getZoneAbbr = function () {
    return defaultTimeZone;
};

function UniqueIdGenerator() { };
UniqueIdGenerator.prototype.rand = Math.floor(Math.random() * 26) + Date.now();
UniqueIdGenerator.prototype.getId = function () {
    return this.rand++;
};

function getUrlParameter(sParam) {
    var sPageUrl = window.location.search.substring(1);
    var sUrlVariables = sPageUrl.split("&");
    for (var i = 0; i < sUrlVariables.length; i++) {
        var sParameterName = sUrlVariables[i].split("=");
        if (sParameterName[0] == sParam) {
            return sParameterName[1];
        }
    }
}

function formatComments(comment, container) {
    if (comment == undefined)
        comment = "";
    //"#*()?-@_$%+;"
    var regExForSpecialCharecters = /[`~!^&|\=:'"<>\{\}\[\]\\\/]/gi;
    var regExForConstant = /^[a-zA-Z0-9\s]+$/;
    if (regExForSpecialCharecters.test(comment) && !regExForConstant.test(comment)) {

        if (container != undefined) {
            container.popover({
                placement: "top",
                trigger: "manual",
                container: "body",
                content: 'The special characters backslash(\\),forwardslash(//),",{},[],\' will be removed from comments',
                html: true,
                width: "250px"
            }).popover("show");

            setTimeout(function () { container.popover('hide'); }, 4000);
        }
        else {
            notifyInfo('The special characters, backslash(\\),forwardslash(//),",{},\',\- will be removed from comments');
        }
        comment = comment.replace(regExForSpecialCharecters, '');
        var lineBreak = /\r|\n/.exec(comment);
        if (lineBreak != null) {
            var lineBreakArray = lineBreak.input.split(lineBreak[0]);
            var formattedComment = '';
            $.each(lineBreakArray, function (i, val) {
                formattedComment += val + "\n";
            });
            comment = formattedComment.substring(0, formattedComment.length - 1);
        }
    }
    else if (comment.indexOf("\\") >= 0) {
        notifyInfo("The character backslash(\\) will be removed from comments");
        comment = comment.replace(/\\/g, '');
    }
    return comment.trim();
}


function isNullOrWhiteSpace(value) {
    if (value == null) return true;
    return value.replace(/\s/g, "").length === 0;
}

function isClassifedEntity(entities, entityKey) {
    var isValid = false;
    $.each(entities, function (i, v) {
        if (entityKey.indexOf(v) > -1) {
            isValid = true;
        }
    });
    return isValid;
}


function getFormattedFileSize(bytes, si) {
    var thresh = si ? 1000 : 1024;
    if (Math.abs(bytes) < thresh) {
        return bytes + " B";
    }
    var units = si
        ? ["kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"]
        : ["KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB"];
    var u = -1;
    do {
        bytes /= thresh;
        ++u;
    } while (Math.abs(bytes) >= thresh && u < units.length - 1);
    return bytes.toFixed(1) + " " + units[u];
}

$(document).ready(function () {
    makeCenter();

    if ($(".center").length > 0) {
        $(window).resize(function () {
            makeCenter();
        });
    }

    initialzeGlobalAjaxStartEvents();
    loadingDiv.hide(100);
    //$.fn.select2.defaults.set("theme", "bootstrap");

    $("#spanReportAccessTags").tooltip();
    var block = false;
    $(".animateIcon").on("mouseenter", function () {

        if (!block) {
            block = true;
            var thisfontSize = $(this).css("font-size");
            $(this).animate({
                "font-size": "16px"
            }, 50, function () { block = false; });
        }
    }).on("mouseleave", function () {
        if (!block) {
            block = true;
            var thisfontSize = $(this).css("font-size");
            $(this).animate({
                "font-size": "15px"
            }, 50, function () { block = false; });
        }
    });

});

$footer = $('.sticky-footer');
$header = $('.sticky-header');
$tableHeader = $('.sticky-table-header');


//var tableOffset = $(".sticky-table-header").closest("table").offset().top;
if ($tableHeader.length > 0) {

    var $tableHeaderCloned = $('.sticky-table-header').clone();
    var $virtualFixedHeaderTable = $('.sticky-table-header').closest("table").append("<table class='fixed_header_table'></table>");
    $(".fixed_header_table").attr("class", $('.sticky-table-header').closest("table").attr("class") + " fixed_header_table");
    var $fixedHeader = $(".fixed_header_table").append($tableHeaderCloned);
}

$(window).scroll(function () {
    if ($(this).scrollTop() > 200) {
        $footer.addClass('fixed_footer');
        $header.addClass('fixed_header');
    } else if ($(this).scrollTop() < 200) {
        $footer.removeClass('fixed_footer');
        $header.removeClass('fixed_header');
    }
    if ($tableHeader.length > 0) {
        var tableScrollTop = $(".sticky-table-header:eq(0)").closest("table").offset().top;

        if ($(this).scrollTop() > tableScrollTop) {
            $fixedHeader.show(10, function () {

                $(".sticky-table-header:eq(0) th").each(function (i, v) {
                    $(".fixed_header_table th:eq(" + i + ")").css("width", $(v).width());
                });
            });
            $fixedHeader.width($('.sticky-table-header:eq(0)').width());
        } else if ($(this).scrollTop() < tableScrollTop) {
            $fixedHeader.hide();
        }

        $fixedHeader.css("left", (-$(this).scrollLeft()) + $(".sticky-table-header:eq(0)").closest("table").offset().left);
    }
});

var pendingResponse = 0;
function initialzeGlobalAjaxStartEvents() {
    $(document).ajaxStart(function () {
        loadingDiv.show(100);
        if (pendingResponse == 0) {
            pendingResponse++;
        }
    }).ajaxStop(function () {
        loadingDiv.hide(100);
    });
}

var getDateIfEmpty = function (date) {
    if (date == null || date == '' || date == undefined)
        return new Date();
    else
        return date;
}

function fnDestroyDataTable(container) {
    if ($.fn.DataTable.fnIsDataTable(container)) {
        $(container).removeClass("initialized");
        $(container).html("");
        $(container).DataTable().clear();
        $(container).DataTable().destroy();
        $(container).empty();
    }
}
function isEmptyOrSpaces(str) {
    return str === null || str.match(/^ *$/) !== null;
}

function isNumber(stringVal) {
    return !/[^0-9-.,()$]/.test(stringVal);
}

function classStartsWith(element, str) {
    return $(element).map(function (i, e) {
        var classes = e.className.split(' ');
        for (var i = 0, j = classes.length; i < j; i++) {
            if (classes[i].substr(0, str.length) == str) return e;
        }
    }).get();
}

Array.prototype.insert = function (index, item) {
    this.splice(index, 0, item);
};

Array.prototype.move = function (from, to) {
    this.splice(to, 0, this.splice(from, 1)[0]);
};

Array.prototype.toArray = function () {
    return this;
};

Array.prototype.containsIndexOf = function (inputStr) {
    var colIndex = -1;

    $.each($(this), function (i, v) {

        if (v.indexOf(inputStr) > -1)
            colIndex = i;
    });

    return colIndex;
};


Array.prototype.areAllValuesSame = function () {

    for (var i = 1; i < this.length; i++) {
        if (this[i] !== this[0])
            return false;
    }

    return true;
}

String.prototype.endsWith = function (suffix) {
    return this.indexOf(suffix, this.length - suffix.length) !== -1;
};

String.prototype.lastButOneEndsWith = function (suffix) {
    return this.indexOf(suffix, (this.length - 1 - suffix.length)) !== -1;
};

String.prototype.countOf = function (string) {
    var substrings = this.split(string);
    return substrings.length - 1;
};

String.prototype.startsWith = function (string) {
    return this.indexOf(string) === 0;
};


String.prototype.containsAny = function (substrings) {
    for (var i = 0; i !== substrings.length; i++) {
        var substring = substrings[i];
        if (this.indexOf(substring) !== -1) {
            return substring;
        }
    }
    return null;
}

String.prototype.contains = function (string) {
    return this.indexOf(string) > -1;
}

String.prototype.removeLastCharacter = function (string) {

    var pos = this.lastIndexOf(string);
    return this.substring(0, pos) + this.substring(pos + 1);
};


String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};

Array.prototype.move = function (from, to) {
    this.splice(to, 0, this.splice(from, 1)[0]);
};

function select2Data() {
    var id;
    var test;
}

function GetSelect2DataFromArray(array, isAllDefault) {
    var select2List = [];

    if (isAllDefault)
        array.insert(0, "All");

    $(array).each(function (i, v) {
        var thisData = new select2Data();
        thisData.id = i;
        thisData.text = v;

        select2List.push(thisData);
    });

    return select2List;
}


var showAlert = function (message) {
    bootbox.alert({
        message: message
    });
}

var showMessage = function (message, header, buttons) {
    if (!buttons)
        buttons = {
            main: {
                label: "OK",
                callback: function () {
                }
            }
        }

    //bootbox.alert(message, function () {
    //    //Example.show("Hello world callback");
    //});

    bootbox.dialog({
        message: message,
        title: !header ? "Alert" : header,
        buttons: buttons
    });
};


var makeCenter = function () {
    var centered = $(".center");
    centered.css({
        left: ($(window).width() - centered.outerWidth()) / 2,
        top: ($(window).height() - centered.outerHeight()) / 2
    });
};

var notifySuccess = function (message) {

    $(".top-right").notify({
        message: { text: message },
        type: "success",
        fadeOut: { enabled: true, delay: 1000 }
    }).show();

};

var notifyInputError = function (message) {

    $(".top-right").notify({
        message: { text: message },
        type: "danger",
        fadeOut: { enabled: true, delay: 3000 }
    }).show();

};

var notifyError = function (message) {

    $(".top-right").notify({
        message: { text: message },
        type: "danger",
        fadeOut: { enabled: false }
    }).show();

};

var notifyWarning = function (message) {

    $(".top-right").notify({
        message: { text: message },
        type: "warning",
        fadeOut: { enabled: true, delay: 2000 }
    }).show();
};

var notifyInfo = function (message) {
    $(".top-right").notify({
        message: { text: message },
        type: "info",
        fadeOut: { enabled: true, delay: 2000 }
    }).show();
}

jQuery.browser = {};
(function () {
    jQuery.browser.msie = false;
    jQuery.browser.version = 0;
    if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
        jQuery.browser.msie = true;
        jQuery.browser.version = RegExp.$1;
    }
})();



var AuditData = function () {
    this.moduleName = "";
    this.action = "";
    this.reportName = "";
    this.fundName = "";
    this.groupName = "";
    this.brokerName = "";
    this.fundOrBrokerName = "";
    this.section = "";
    this.agreementName = "";
    this.agreementType = "";
    this.contextDate = "";
    this.description = "";
    this.shouldApplyChangeToAllFuture = "";
    this.changes = new Array();
    this.AdditionalSectionDetails = new Array();
};

function log(auditData) {
    $.ajax({
        type: "Post",
        dataType: "json",
        url: "/Audit/LogAudit",
        data: JSON.stringify({ auditData: auditData }),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            //console.log(data);
        },
        error: function (data) {
            //console.log(data);
        }
    });
};


(function ($) {

    $.getPrettyDate = function (dateTime) {
        if (dateTime == null)
            return '';
        return moment(dateTime).format('lll');
    };

    $.fn.popOverInfo = function (options) {

        var oSettings = $.extend({
            message: "invalid value",
            placement: "top"
        }, options);

        var oElement = $(this);
        //pop-up    
        oElement.popover({
            placement: oSettings.placement,
            trigger: "manual",
            container: "body",
            content: oSettings.message,
            html: true,
            width: "250px"
        });

        oElement.popover("show");
    }

    $.fn.equalizeHeights = function () {
        return this.height(Math.max.apply(this, $(this).map(function (i, e) { return $(e).height() }).get()));
    }

})(jQuery);

function getChecklistDate(dateTime) {
    if (dateTime == null)
        return '';

    return "<span style='white-space:nowrap;'>" + moment(dateTime).format("ll") + "</span><br/>" + moment(dateTime).format("hh:mm A");
}

function getDateForToolTip(dateTime) {
    if (dateTime == null)
        return '';
    return moment(dateTime).format("lll");
}

function getFormattedFileName(fileName) {
    return escape(fileName).split('+').join('%2B');
};


function getDateForDisplay(dateTime) {
    if (dateTime == null)
        return '';
    return moment(dateTime).format("YYYY-MM-DD");
}


function getDateAndTimeForDisplay(dateTime) {
    if (dateTime == null)
        return '-';

    var momentDate = moment(dateTime);

    if (momentDate.year() <= 1)
        return '-';

    return momentDate.format("lll");
}

function placeCaretAtEnd(el) {
    el = el.get(0);
    el.focus();
    if (typeof window.getSelection != "undefined"
        && typeof document.createRange != "undefined") {
        var range = document.createRange();
        range.selectNodeContents(el);
        range.collapse(false);
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
    } else if (typeof document.body.createTextRange != "undefined") {
        var textRange = document.body.createTextRange();
        textRange.moveToElementText(el);
        textRange.collapse(false);
        textRange.select();
    }
}
var TODAY = moment().startOf('day');

function getClonedObject(source) {
    if (Object.prototype.toString.call(source) === '[object Array]') {
        var cloneArray = [];
        for (var i = 0; i < source.length; i++) {
            cloneArray[i] = getClonedObject(source[i]);
        }
        return cloneArray;
    }
    else if (typeof (source) == "object") {
        var cloneObject = {};
        for (var prop in source) {
            if (source.hasOwnProperty(prop)) {
                cloneObject[prop] = getClonedObject(source[prop]);
            }
        }
        return cloneObject;
    } else {
        return source;
    }
}


function LastDayOfMonth(year, month) {
    return (new Date((new Date(year, month + 1, 1)) - 1)).getDate();
}

function GetLastDateOfMonth(year, month) {
    return (new Date((new Date(year, month + 1, 1)) - 1));
}

function GetFirstDateOfMonth(year, month) {
    return (new Date(year, month, 1));
}


function toggleChevron(e) {
    $(e.target)
        .find("i.indicator")
        .toggleClass("glyphicon-chevron-down").toggleClass("glyphicon-chevron-up");
    $("html, body").animate({ scrollTop: $(e.target).offset().top - 10 }, "slow");
}

function humanizeNumber(number) {

    if (number == "1")
        return "1st";
    if (number == "2")
        return "2nd";
    if (number == "3")
        return "3rd";
    if (number == "L")
        return "last";
    return number + "th";
}

function dayOfWeek(number) {

    if (number == "0")
        return "Sunday";
    if (number == "1")
        return "Monday";
    if (number == "2")
        return "Tuesday";
    if (number == "3")
        return "Wednesday";
    if (number == "4")
        return "Thursday";
    if (number == "5")
        return "Friday";
    if (number == "6")
        return "Saturday";
    if (number == "W")
        return "business day";
    return number + "th day";
}

function GetReadableCron(cronExpression, frequency, cronDescripion, followingPeriod) {


    var followText = "";

    if (followingPeriod != undefined && followingPeriod > 0)
        followText = ",<br\> on the following " + humanizeNumber(followingPeriod) + " " + (frequency.toLowerCase() == "monthly" ? "month" : "week");

    var timeStamp = cronDescripion.split(",")[0];

    if (frequency.toLowerCase() == "daily") {
        return timeStamp;
    }

    if (frequency.toLowerCase() == "weekly") {
        return cronDescripion.replace("only", "") + followText;
    }

    if (frequency.toLowerCase() == "monthly") {
        var cronSplit = cronExpression.split(" ");
        var dayOfMonth = cronSplit[2];
        var rx = new RegExp(/^[0-9]*$/);

        //if contains only day number
        if (rx.test(dayOfMonth)) {
            return timeStamp + ", on " + humanizeNumber(dayOfMonth) + " " + (cronSplit[4] == "1-5" ? "business day" : "calendar day") + followText;
        }

        //if contains * on the dayOfMonth
        if (dayOfMonth == "*" && cronSplit[4].indexOf("#") >= 0) {
            var daySplit = cronSplit[4].split("#");
            return timeStamp + ", on " + humanizeNumber(daySplit[1]) + " " + dayOfWeek(daySplit[0]) + followText;
        }

        //scenario: 0 0 10W 0 0
        if (dayOfMonth.length === 3 && dayOfMonth.indexOf("L") < 0) {
            return timeStamp + ", on " + humanizeNumber(dayOfMonth[0] + dayOfMonth[1]) + " " + (cronSplit[4] == "1-5" ? "business day" : "calendar day") + followText;
        }

        //scenario: 0 0 LW 0 0
        if (dayOfMonth.length == 2 && dayOfMonth.indexOf("L") >= 0 && dayOfMonth.indexOf("W") >= 0) {
            return timeStamp + ", on " + humanizeNumber(dayOfMonth[0]) + " " + dayOfWeek(dayOfMonth[1]) + followText;
        }

        //scenario: 0 0 L-3W 0 0
        if (dayOfMonth.length > 3 && dayOfMonth.indexOf("L") >= 0 && dayOfMonth.indexOf("W") >= 0) {
            var dayOfMonthSplit = dayOfMonth.split("-");

            if (dayOfMonthSplit.length > 0) {
                return timeStamp + ", on " + humanizeNumber(dayOfMonthSplit[1].replace("W", "")) + " " + (cronSplit[4] == "1-5" ? "business day" : "calendar day") + " from the last" + followText;
            }
        }

        if (dayOfMonth.indexOf("W") >= 0) {
            return timeStamp + ", on " + humanizeNumber(dayOfMonth[0]) + " " + (cronSplit[4] == "1-5" ? "business day" : "calendar day") + followText;
        }

        return cronDescripion;
    }

    return cronExpression;

}

function prettify(str) {
    return str.split('.').map(function capitalize(part) {
        return part.charAt(0).toUpperCase() + part.slice(1);
    }).join(' ');
}

function humanizeEmail(emailString) {
    if (emailString == undefined || emailString == "")
        return emailString;

    if (emailString.indexOf("@") < 0)
        return emailString;

    emailString = emailString.substring(0, emailString.lastIndexOf("@"));
    emailString = emailString.charAt(0).toUpperCase() + emailString.slice(1);

    return prettify(emailString);
}

function delay(callback, ms) {
    var timer = 0;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        }, ms || 0);
    };
}

function initiateNumberCounter() {

    $('.number-counter').each(function () {
        var $this = $(this),
            countTo = $this.attr('data-count');

        $({ countNum: $this.text() }).animate({
            countNum: countTo == "" ? 0 : countTo
        }, {
            duration: 300,
            easing: 'linear',
            step: function () {
                if (Math.floor(this.countNum) === NaN)
                    this.countNum = 0;

                $this.text(Math.floor(this.countNum));
            },
            complete: function () {
                $this.text(this.countNum);
            }
        });
    });
}

var getFormattedUIDate = function (date) {
    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    var d = date.getDate();
    return "" + y + "-" + (m <= 9 ? "0" + m : m) + "-" + (d <= 9 ? "0" + d : d);
};


/*************************************/
Dropzone.options.myAwesomeDropzone = false;
Dropzone.autoDiscover = false;

var validateUploadFile = function (file, done) {
    if (file.name.length > 100) {
        done("Warning:" + "File name '" + file.name + " cannot exceed 100 chars. Please shorten the file name and try again.  ");
        $(".callout").pulse();
    }
    else if (file.name.toLowerCase().indexOf(".exe") >= 0) {
        notifyError("Warning: filename appeares to be .exe and cannot be uploaded to the system");
        done("Warning: filename appeares to be .exe and cannot be uploaded to the system");
        this.removeFile(file);
    }
    else
        done();

};
var validateDoubleExtensionInDZ = function (file, done) {
    if (file.name.toLowerCase().indexOf(".exe") >= 0) {
        notifyError("Warning: filename appeares to be .exe and cannot be uploaded to the system");
        done("Warning: filename appeares to be .exe and cannot be uploaded to the system");
        this.removeFile(file);
    } else
        done();
};
/*************************************/


//initiateNumberCounter();

function PrintSwiftMessage(htmlContent) {
    var mywindow = window.open('', 'PRINT');

    mywindow.document.write('<html><head>');
    mywindow.document.write('</head><body >');
    mywindow.document.write('<span style="font-size:14px;">HedgeMark Operations Secure - Swift Message</span>');
    mywindow.document.write('<hr/><br/>');
    mywindow.document.write('<span style="white-space: pre;font-size:12px">');
    mywindow.document.write(htmlContent);
    mywindow.document.write('</span>');
    mywindow.document.write('</body></html>');

    mywindow.document.close();
    mywindow.focus();

    mywindow.print();
    mywindow.close();

    return true;
}


function deSelectAllText() {
    var selection = window.getSelection();
    selection.removeAllRanges();
}

function selectText(className) {
    var node = document.getElementsByClassName(className)[0];
    var selection = window.getSelection();
    var range = document.createRange();
    range.selectNodeContents(node);
    selection.removeAllRanges();
    selection.addRange(range);
}

function CopyToClipBoardByClassName(className) {
    selectText(className);
    window.document.execCommand("copy");
    deSelectAllText();
}


function isEquivalent(a, b) {

    var aProps = Object.getOwnPropertyNames(a);
    var bProps = Object.getOwnPropertyNames(b);

    if (aProps.length !== bProps.length) {
        return false;
    }

    for (var i = 0; i < aProps.length; i++) {
        var propName = aProps[i];

        if (a[propName] !== b[propName]) {
            return false;
        }
    }

    return true;
}


var fnGetWireDeadlineCounter = function (timeToApprove) {
    if (timeToApprove == undefined)
        timeToApprove = {};

    var isPast = moment().add(timeToApprove).isBefore(moment());

    if (isPast)
        return moment().add(timeToApprove).fromNow();

    timeToApprove.Seconds--;
    if (timeToApprove.Seconds == -1) {
        timeToApprove.Minutes--;
        if (timeToApprove.Minutes == -1) {
            timeToApprove.Hours--;
            timeToApprove.Minutes = 59;
        }
        timeToApprove.Seconds = 59;
    }

    if (timeToApprove.Days > 0)
        return moment(timeToApprove).format("D") + "d + " + moment(timeToApprove).format("HH:mm:ss");

    return moment(timeToApprove).format("HH:mm:ss");
}

var ContextDatesOfTodayAndTomorrow;

function GetContextDatesOfTodayAndYesterday() {
    $.ajax({
        "url": "/WiresDashboard/GetContextDatesOfTodayAndYesterday",
        "async": false,
        "dataType": "json",
        "success": function (json) {
            ContextDatesOfTodayAndTomorrow = [moment(json.thisContextDate), moment(new Date())];
        }
    });
}

GetContextDatesOfTodayAndYesterday();

function getTimeZoneAbbr() {
    return moment.tz.zone(moment.tz.guess()).abbr("");
}