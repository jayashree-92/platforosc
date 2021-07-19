/*
* File:        AutoCompletEmail.js
* Version:     1.0.0
* Description: Provides the autocomplete of email alias based on input
* Author:      Mohan Srikanth Rameshkumar
* Language:    Javascript
* License:	   MIT Public License - v 1.0
* 
* ©2020 HedgeMark International, LLC. All rights reserved.
*
*/

(function ($) {

    var aDefaultDomains = ["bnymellon.com"];

    var spanClassName = "spn_temp_span_auto_sugst_email";
    var shiftKeyDown = false;
    var shiftKey = 16;
    var atKey = 50;
    var enterKey = 13;
    var commaKey = 188;
    var semicolonKey = 186;
    var tabKey = 9;
    var rightArrowKey = 39;
    var endKey = 39;
    var isSuggessionInProgress = false;
    var bestMatchSuggesion = "";
    var suggessionInDisplay = "";
    var emailAddressRegEx = new RegExp(/^\b[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b$/i);

    $.fn.AutoCompleteEmail = function (options, params) {

        var oElement = $(this);

        var functionCall = (typeof options === "string") ? options : undefined;

        if (functionCall) {
            if (functionCall == "getEmails")
                return getEmails();
            if (functionCall == "formatEmails")
                return formatEmails(params[0], params.length > 1 ? params[1] : "");

            return true;
        }

        var oSettings = $.extend({
            sClassOnChange: "spn-auto-email-warning",
            sClassOnError: "spn-auto-email-error",
            domains: aDefaultDomains,
            bAllowListedDomainsOnly: false,
            sHighlightList: "",
            asHighlightList: [],
            onFocusOutCallback: function () { },
            onFocusInCallback: function () { }
        }, options);

        var originalVal = "";
        bestMatchSuggesion = oSettings.domains[0];

        if (oSettings.asHighlightList == null || oSettings.asHighlightList.length === 0)
            oSettings.asHighlightList = getAllEmailSplits(oSettings.sHighlightList);
        //Init
        if ($("#dvInptScheduleInternalTo").attr("disabled") !== "disabled")
            oElement.attr("contenteditable", true);

        setFinalizedUiContent();
        originalVal = oElement.text().trim();

        oElement.removeClass(oSettings.sClassOnChange);
        //oElement.removeClass(oSettings.sClassOnError);

        oElement.off("keyup").on("keyup", function (event) {
            if (event.keyCode == shiftKey) {
                shiftKeyDown = false;
            }

            //if key is @ -> append a span with one best match
            if (isSuggessionInProgress || (shiftKeyDown && event.keyCode === atKey)) {
                isSuggessionInProgress = true;

                var contentWithoutSuggesstion = $("<span>" + oElement.html().replace(suggessionInDisplay, "") + "</span>").text();
                bestMatchSuggesion = getBestMatch(contentWithoutSuggesstion);
                suggessionInDisplay = "<span class=\"" + spanClassName + "\" style=\"opacity:0.7\">" + bestMatchSuggesion + "</span>";

                if ($("span." + spanClassName).length == 0)
                    oElement.append(suggessionInDisplay);
                else
                    $("span." + spanClassName).replaceWith(suggessionInDisplay);
            }
        });

        oElement.off("keydown").on("keydown", function (event) {

            if (event.keyCode === shiftKey)
                shiftKeyDown = true;

            if (shiftKeyDown && event.keyCode === atKey)
                isSuggessionInProgress = true;

            // prevent the default behaviour of enter key pressed - to prevent DIV creation when enter key is pressed
            if (!isSuggessionInProgress && event.keyCode == enterKey) {
                document.execCommand("insertHTML", false, "<br><br>");
                event.preventDefault();
            }

            if (!isSuggessionInProgress)
                return;

            //if key is @ -> append a span with one best match
            if (event.keyCode === commaKey || event.keyCode === semicolonKey)
                isSuggessionInProgress = false;

            //if key is TAB or key is RIGHT ARROW -> append suggested text to the input
            if (event.keyCode == tabKey || event.keyCode == rightArrowKey || event.keyCode === endKey || event.keyCode === enterKey) {
                event.preventDefault();
                finishSuggessionIfAny();
            }

            //if key is UP/DOWN ARROW -> toggle next best match - reserved for the next version

        });

        oElement.off("mousedown").on("mousedown", function (event) {
            finishSuggessionIfAny();
        });

        oElement.off("focus").on("focus", function (event) {
            setCursorPositionAtLast();
            oSettings.onFocusInCallback.call(oElement);
        });

        oElement.off("dblclick").on("dblclick", function (event) {
            setReadyToEditContent();
        });

        oElement.off("focusout").on("focusout", function (event) {
            ////Perform Email Validation when not suggesting
            //if (isSuggessionInProgress)
            //    return;

            setFinalizedUiContent();
            oSettings.onFocusOutCallback.call(oElement);
        });

        oElement.unbind("paste").bind("paste", function (event) {
            var pastedData = event.originalEvent.clipboardData.getData("text");
            handleCopyPasteEvent(event, pastedData);
        });


        oElement.unbind("drop").bind("drop", function (event) {
            var pastedData = event.originalEvent.dataTransfer.getData("text");
            handleCopyPasteEvent(event, pastedData);
        });

        oElement.on("click", ".spn-auto-email-choice-close", function (event) {
            oElement.html(oElement.html().replace($(this).parent().html(), ""));
            setFinalizedUiContent();
        });

        function getEmails() {
            var tempAttachElement = "<span class=\"spn-auto-email-chioce-temp\">" + oElement.html().replaceAll("<span class=\"spn-auto-email-choice-close\"></span>", "; ") + "<span>";
            var emails = $(tempAttachElement).text();
            return emails;
        }

        //This is a simplified version of setFinalizedUiContent()
        function formatEmails(emailStr, emailHighlightStr) {
            var allValues = getAllEmailSplits(emailStr);
            var allHighlights = getAllEmailSplits(emailHighlightStr);

            var finalizedValues = "";

            //Prevent duplicate
            var validDistinctEmails = [];
            $(allValues).each(function (i, v) {
                v = v.trim();
                if (v === "")
                    return;

                if (!emailAddressRegEx.test(v)) {
                    v = "<span style=\"color:red\">" + v + ";</span>";
                }
                else {
                    if (validDistinctEmails.indexOf(v) >= 0)
                        return;

                    validDistinctEmails.push(v);
                    v = allHighlights.indexOf(v) >= 0
                        ? "<span class=\"spn-auto-email-choice-highlight\">" + v + "</span>"
                        : "<span class=\"spn-auto-email-choice\">" + v + "</span>";
                }
                finalizedValues += v;
            });

            return finalizedValues;
        }


        function setFinalizedUiContent() {
            isSuggessionInProgress = false;
            oElement.html(oElement.html().replaceAll("<span class=\"spn-auto-email-choice-close\"></span>", "; "));
            var allValues = getAllEmailSplits(oElement.text());
            var finalizedValues = "";

            //Prevent duplicate
            var validDistinctEmails = [];
            var isInValidEmailsInPlace = false;

            $(allValues).each(function (i, v) {
                v = v.trim();
                if (v === "")
                    return;

                if (!emailAddressRegEx.test(v)) {
                    v = "<span style=\"color:red\">" + v + ";</span>";
                    isInValidEmailsInPlace = true;
                }
                else {

                    var isAlreadyAvailable = false;

                    $(validDistinctEmails).each(function (j, v1) {
                        if (v1.toLowerCase() === v.toLowerCase())
                            isAlreadyAvailable = true;
                    });

                    if (isAlreadyAvailable)
                        return;

                    validDistinctEmails.push(v);

                    var emailDomain = v.split("@")[1].trim().toLowerCase();
                    if (oSettings.bAllowListedDomainsOnly && oSettings.domains.indexOf(emailDomain) < 0) {
                        v = "<span style=\"color:red\">" + v + ";</span>";
                        isInValidEmailsInPlace = true;
                    } else {
                        v = oSettings.asHighlightList.indexOf(v) >= 0
                            ? "<span class=\"spn-auto-email-choice-highlight\">" + v + "<span class=\"spn-auto-email-choice-close\"></span></span>"
                            : "<span class=\"spn-auto-email-choice\">" + v + "<span class=\"spn-auto-email-choice-close\"></span></span>";
                    }
                }

                finalizedValues += v;
            });

            oElement.html(finalizedValues + "&nbsp;");

            oElement.removeClass(oSettings.sClassOnError);
            oElement.removeClass(oSettings.sClassOnChange);

            if (isInValidEmailsInPlace)
                oElement.addClass(oSettings.sClassOnError);
            else if (originalVal.trim() !== oElement.text().trim())
                oElement.addClass(oSettings.sClassOnChange);
        }

        function setReadyToEditContent() {
            oElement.html(oElement.html().replaceAll("<span class=\"spn-auto-email-choice-close\"></span>", "; "));
            oElement.html(oElement.text());
            setCursorPositionAtLast();
        }

        function handleCopyPasteEvent(event, pastedData) {
            var isPastedFromOutlook = pastedData.indexOf("<") >= 0 && pastedData.indexOf(">") >= 0;
            var potentialEmails = getAllEmailSplits(pastedData);

            var validIdentifies = "";
            $(potentialEmails).each(function (i, str) {
                if (isPastedFromOutlook && str.indexOf("@") >= 0) {
                    validIdentifies += str.substring(str.lastIndexOf("<") + 1, str.lastIndexOf(">")) + "; ";
                }
            });

            if (isPastedFromOutlook)
                event.preventDefault();

            oElement.html(oElement.html() + validIdentifies);
            setCursorPositionAtLast();
        }

        function finishSuggessionIfAny() {
            if (!isSuggessionInProgress)
                return;

            isSuggessionInProgress = false;

            bestMatchSuggesion += "; ";

            if ($("span." + spanClassName).length === 0)
                oElement.append(bestMatchSuggesion);
            else
                $("span." + spanClassName).replaceWith(bestMatchSuggesion);

            setCursorPositionAtLast();
        }

        function getAllEmailSplits(existingVal) {
            if (existingVal == null)
                return [];

            return existingVal.split(/[,;\n\t\r ]+/);
        }

        function getBestMatch(existingVal) {
            //there can be many values
            var allValues = getAllEmailSplits(existingVal);
            var valInArgument = allValues[allValues.length - 1];
            var emailSplits = valInArgument.split("@");
            var userGivenInput = emailSplits[emailSplits.length - 1].toLowerCase();

            if (userGivenInput == "")
                return oSettings.domains[0];

            var match = oSettings.domains.filter(function (domain) {
                return domain.indexOf(userGivenInput) === 0;
            }).shift() || "";

            return match.replace(userGivenInput, "");
        }


        function setCursorPositionAtLast() {

            window.setTimeout(function () {
                var elment = oElement.get(0);
                elment.innerHTML = oElement.html();
                var range = document.createRange();
                range.setStart(elment.childNodes[elment.childNodes.length - 1], elment.childNodes[elment.childNodes.length - 1].length);
                range.collapse(true);
                var windowSelection = window.getSelection();
                windowSelection.removeAllRanges();
                windowSelection.addRange(range);
                elment.focus();
            }, 50);

        }
    };


})(jQuery);;

