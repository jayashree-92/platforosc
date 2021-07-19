/*
* File:        NumericCellEditor.js
* Version:     1.0.0
* Description: Provides ability to a datatable td/input text box to accept numeric inputs only with formatting
* Author:      Mohan Srikanth R. 
* Language:    Javascript
* License:	   Eclipse Public License - v 1.0
* 
* ©2013 HedgeMark International, LLC. All rights reserved.
*
*/

var ctrlDown = false;
var ctrlKey = 17;
var cmdKey = 91;
var vKey = 86;
var cKey = 67;

function endsWith(strVal, suffix) {
    if (strVal == undefined || strVal == "" || (strVal.length - suffix.length) < 0)
        return false;

    return strVal.indexOf(suffix, strVal.length - suffix.length) !== -1;
};
(function ($) {
    $.fn.numericEditor = function (options) {
        var oSettings = $.extend({
            sHeader: "Value",
            bAllowNegative: false,
            bAutoFormat: true,
            bAllowBlank: false,
            bPreventEventProbagation: true,
            bHighlightEditedRow: true,
            sClassToHighlight: "info",
            sClassEdited: "row_edited",
            iInputLimit: 20,
            iInputMaxNumber: -1,
            iDecimalPlaces: 2,
            aAllowedKeys: [189, 109, 110, 8, 37, 39, 46, 190, 188, 17],
            fnFocusOutCallback: function () { },
            fnFocusInCallback: function () { },
            fnKeyDownCallback: function () { }
        }, options);


        var oCell = $(this);

        if (oSettings.bAutoFormat) {
            oCell.each(function (i, v) {
                var stringVal;
                if (!$(v).is("td")) {
                    stringVal = $(v).val();
                    if (!oSettings.bAllowBlank || stringVal != "") {
                        $(v).val($.convertToCurrency(stringVal, oSettings.iDecimalPlaces));
                    }
                } else {
                    stringVal = $(v).text();
                    if (!oSettings.bAllowBlank || stringVal != "") {
                        $(v).text($.convertToCurrency(stringVal, oSettings.iDecimalPlaces));
                    }
                }
            });
        }

        if (oSettings.bPreventEventProbagation) {
            $(this).on("click", function (event) {
                event.stopPropagation();
                event.stopImmediatePropagation();
                event.preventDefault();
            });
        }

        $(this).on("focusin", function (event) {

            var stringVal;
            if (!oCell.is("td")) {
                stringVal = $(this).val();

                if (oSettings.bAllowBlank && stringVal === "0") {
                    oCell.addClass(oSettings.sClassEdited);
                }

                $(this).val($.convertToNumber(stringVal));
            } else {
                stringVal = $(this).text();

                if (oSettings.bAllowBlank && stringVal === "0") {
                    oCell.addClass(oSettings.sClassEdited);
                }

                $(this).text($.convertToNumber(stringVal));
            }

            oSettings.fnFocusInCallback.call(this, $(this));
        });

        oCell.on("focusout blur", function (event) {
            var number, stringVal;
            if (!oCell.is("td")) {
                number = $(this).val().toString();

                if (!oSettings.bAllowNegative) {
                    if (number.indexOf("(") >= 0 || number.indexOf("-") >= 0) {
                        $(this).val("");
                        notifyInputError(oSettings.sHeader + " can not be negative!");
                        $(this).focus();
                        return;
                    }
                }
                stringVal = $(this).val();

                if (oSettings.bAllowBlank && stringVal === "") {
                    //oCell.addClass(oSettings.sClassEdited);
                } else {
                    $(this).val($.convertToCurrency(stringVal, oSettings.iDecimalPlaces));
                }

            } else {
                number = $(this).text().toString();

                if (!oSettings.bAllowNegative) {
                    if (number.indexOf("(") >= 0 || number.indexOf("-") >= 0) {
                        $(this).text("");
                        notifyInputError(oSettings.sHeader + " can not be negative!");
                        $(this).focus();
                        return;
                    }
                }
                stringVal = $(this).text();

                if (oSettings.bAllowBlank && stringVal === "") {
                    //oCell.addClass(oSettings.sClassEdited);
                } else {
                    $(this).text($.convertToCurrency(stringVal, oSettings.iDecimalPlaces));
                }
            }

            if (stringVal.indexOf("-") == 0 || stringVal.indexOf("(") == 0)
                $(this).css("color", "red");
            else
                $(this).css("color", "");

            oSettings.fnFocusOutCallback.call(this, oCell);
        });

        //if (!oCell.is("td")) {
        //    oCell.attr("onkeypress", "return (event.charCode >= 48 && event.charCode <= 57) || event.charCode == 190");
        //    return;
        //}

        oCell.attr("contenteditable", "true");



        oCell.on("keyup", function (event) {
            if (event.keyCode == ctrlKey || event.keyCode == cmdKey) {
                ctrlDown = false;
            }

            //$(this).popover("hide");
        });

        oCell.on("keydown", function (event) {

            // $(this).popover("hide");
            if (event.keyCode == ctrlKey || event.keyCode == cmdKey) {
                ctrlDown = true;
            }

            //Definition for TAB and ARROW key navigation to/from cells
            defineNavigation($(this), event);

            //Validate input based on parameters
            if (!IsValidInput($(this), oSettings.iInputLimit, event))
                return;

            //Validate input based on parameters
            if (oSettings.iInputMaxNumber != -1) {
                if (!IsValidInputMax($(this), oSettings.iInputMaxNumber, event)) {
                    event.stopPropagation();
                    return;
                }
            }
            $(this).popover("hide");
            //Enter valid inputs to Numeric columns
            editField($(this), oSettings, event);
            oSettings.fnKeyDownCallback.call(this, $(this));

            //Ctrl+V should clear the existing items
            if (ctrlDown && (event.keyCode == vKey)) {
                $(this).text("");
            }

        });
    };


    function editField(oCellInFocus, oSettings, event) {

        oCellInFocus.addClass(oSettings.sClassEdited);

        //   Allow only numeric keys
        if ((event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 96 && event.keyCode <= 105) || event.keyCode == 8 || event.keyCode == 46 || event.keyCode == 189 || event.keyCode == 109 || event.keyCode == 190) {
            //oCellInFocus.addClass(oSettings.sClassEdited);

            if (oSettings.bHighlightEditedRow)
                oCellInFocus.addClass(oSettings.sClassToHighlight);
        } else if (isNaN(String.fromCharCode(event.which)) && (oSettings.aAllowedKeys != undefined && $.inArray(event.keyCode, oSettings.aAllowedKeys) == -1)) {
            var isShiftOrCtrl = !!window.event.shiftKey || !!window.event.ctrlKey;

            if (!isShiftOrCtrl)
                event.preventDefault();
        }
    }

    function IsValidInput(oCellInFocus, maxInputLength, event) {
        if (oCellInFocus.text().length == maxInputLength && event.keyCode != 8) {
            //notifyWarning();
            $(oCellInFocus).popOverInfo({ message: "field's input limit reached. Cannot add more numbers", placement: "right" });
            event.preventDefault();
            return false;
        }
        return true;
    }

    function IsValidInputMax(oCellInFocus, maxInputNumber, event) {
        var key = event.keyCode;
        if ($.convertToNumber(oCellInFocus.text() + String.fromCharCode((96 <= key && key <= 105) ? key - 48 : key)) > maxInputNumber && event.keyCode != 8) {

            $(oCellInFocus).popOverInfo({ message: "Please enter number between 1 to " + maxInputNumber, placement: "right" });

            //notifyWarning("Please enter number within 1 to " + maxInputNumber);
            event.preventDefault();
            return false;;
        }
        return true;
    }


    $.convertToAbsoluteNumber = function (stringVal, shouldReturnZero) {
        if (stringVal == undefined)
            stringVal = "";

        if (shouldReturnZero == undefined)
            shouldReturnZero = false;

        var number = stringVal.toString();
        if (number === "")
            return "";

        number = number.replace("$", "");

        if (number.indexOf("(") === 0 || number.indexOf("-") === 0) {
            number = number.replace("(", "").replace(")", "").replace("-", "");
        }

        number = parseFloat(number.replace(/,/g, ""));

        if (number === 0 || number === "NaN") {
            if (shouldReturnZero) {
                return 0;
            } else {
                return "";
            }
        }

        return number;
    }

    $.convertToNumber = function (stringVal, shouldReturnZero) {

        if (stringVal == undefined)
            stringVal = "";

        if (shouldReturnZero == undefined)
            shouldReturnZero = false;

        var number = stringVal.toString();
        if (number === "")
            return "";

        number = number.replace("$", "");

        var isNegative = false;
        if (number.indexOf("(") === 0 || number.indexOf("-") === 0) {
            isNegative = true;
            number = number.replace("(", "").replace(")", "").replace("-", "");
        }

        number = parseFloat(number.replace(/,/g, ""));

        if (number === 0 || number === "NaN") {
            if (shouldReturnZero) {
                return 0;
            } else {
                return "";
            }
        }

        if (isNegative)
            return "-" + number;

        return number;
    }

    //function roundUp(num, precision) {
    //    return Math.ceil(num * precision) / precision;
    //}
    //function roundUp(rnum, rlength) {
    //    var newnumber = Math.ceil(rnum * Math.pow(10, rlength)) / Math.pow(10, rlength);
    //    return newnumber;
    //}
    //function roundUp(num, precision) {
    //    var rounder = Math.pow(10, precision);
    //    return (Math.round(num * rounder) / rounder).toFixed(precision);
    //};

    function roundUp(value, decimals) {
        if (value == 0)
            return value;

        return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
    }

    $.convertToCurrency = function (stringVal, iDecimalPlaces, shouldRound) {

        if (stringVal == undefined)
            return 0;

        if (iDecimalPlaces == undefined) {
            iDecimalPlaces = stringVal.split(".")[1].length || 3;
        }

        var isPercentage = endsWith(stringVal.toString(), "%");

        stringVal = $.trim(stringVal.toString().replace("%", ""));
        var number = stringVal.toString();
        number = number.replace("$", "");

        if (shouldRound) {
            var currencyFloat = parseFloat(number);

            if (currencyFloat == "NaN" || currencyFloat == "")
                return "0";
            else
                number = roundUp(currencyFloat, iDecimalPlaces).toString();
        }

        var isNegative = false;
        if (number.indexOf("(") >= 0 || number.indexOf("-") >= 0) {
            isNegative = true;
            number = number.replace("(", "").replace(")", "").replace("-", "");
        }
        number = number.replace(/,/g, "");

        var dollars = number.split(".")[0],
        cents = (number.split(".")[1] || "0") + "000000";

        if (dollars == "")
            dollars = "0";

        dollars = dollars.split("").reverse().join("")
            .replace(/(\d{3}(?!$))/g, "$1,")
            .split("").reverse().join("");

        var currency;

        if (iDecimalPlaces != 0)
            currency = dollars + "." + cents.slice(0, iDecimalPlaces);
        else
            currency = dollars;

        if (isNegative)
            currency = "(" + currency + ")";

        if (currency == "0.00" || currency == "NaN")
            currency = "0";

        if (isPercentage)
            currency = currency + " %";

        return currency;
    }


    $.fn.textEditor = function (options) {

        var oSettings = $.extend({
            sHeader: "Value",
            bHighlightEditedRow: true,
            sClassToHighlight: "info",
            sClassEdited: "row_edited",
            bAutoComplete: false,
            aOptions: [],
            iInputLimit: 1000,
            fnFocusOutCallback: function () { }
        }, options);

        var oCell = $(this);

        oCell.on("focusout blur", function (event) {
            oSettings.fnFocusOutCallback.call(this, $(this));
        });

        oCell.attr("contenteditable", "true");

        if (oSettings.bAutoComplete) {
            oCell.attr("data-provide", "typeahead");
            oCell.typeahead({ source: oSettings.aOptions });
        }

        oCell.on("keyup", function (event) {
            if (event.keyCode == ctrlKey || event.keyCode == cmdKey) {
                ctrlDown = false;
            }

        });

        oCell.on("keydown", function (event) {

            if (event.keyCode == ctrlKey || event.keyCode == cmdKey) {
                ctrlDown = true;
            }

            //Definition for TAB and ARROW key navigation to/from cells
            defineNavigation($(this), event);

            //Validate input based on parameters
            if (!IsValidInput($(this), oSettings.iInputLimit, event))
                return;

            //Enter valid inputs to Numeric columns
            editField($(this), oSettings, event);
            //oSettings.fnKeyDownCallback.call(this, $(this));

            //Ctrl+V should clear the existing items
            if (ctrlDown && (event.keyCode == vKey)) {
                $(this).text("");
            }
        });
    }

    $.fn.currencyEditor = function (options) {

        var currencyList = [
            "ADP", "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "ATS", "AUD", "AWG", "AZN", "BAM", "BBD", "BDT", "BEF", "BGN", "BHD", "BIF", "BMD", "BND", "BOB", "BOV", "BRL", "BSD", "BTN", "BWP", "BYR", "BZD", "CAD", "CDF", "CHF", "CLF", "CLP", "CNH", "CNY", "COP", "COU", "CRC", "CSD", "CUP", "CVE", "CYP", "CZK", "DEM", "DJF", "DKK", "DOP", "DZD", "ECS", "EEK", "EGP", "ERN", "ESP", "ETB", "EUR", "FIM", "FJD", "FKP", "FRF", "GBP", "GBS", "GEL", "GGP", "GHC", "GHS", "GIP", "GMD", "GNF", "GQE", "GRD", "GTQ", "GWP", "GYD", "HKD", "HNL", "HRK", "HTG", "HUF", "IDR", "IEP", "ILS", "IMP", "INR", "IQD", "IRR", "ISK", "ITL", "JEP", "JMD", "JOD", "JPY", "KES", "KGS", "KHR", "KMF", "KPW", "KRW", "KWD", "KYD", "KZT", "LAK", "LBP", "LKR", "LRD", "LSL", "LTL", "LUF", "LVL", "LVR", "LYD", "MAD", "MDL", "MGA", "MGF", "MKD", "MLF", "MMK", "MNT", "MOP", "MRO", "MTL", "MTP", "MUR", "MVR", "MWK", "MXN", "MYR", "MZM", "MZN", "NAD", "NGN", "NIC", "NID", "NIO", "NLG", "NOK", "NPR", "NZD", "OMR", "PAB", "PEN", "PGK", "PHP", "PKR", "PLN", "PTE", "PYG", "QAR", "ROL", "RON", "RSD", "RUB", "RWF", "SAR", "SBD", "SCR", "SDD", "SDG", "SDP", "SEK", "SGD", "SHP", "SIT", "SKK", "SLL", "SOS", "SPL", "SRD", "SRG", "SSP", "STD", "SVC", "SYP", "SZL", "THB", "THO", "TJS", "TMM", "TMT", "TND", "TOP", "TPE", "TRL", "TRY", "TTD", "TVD", "TWD", "TZS", "UAH", "UDI", "UGX", "USD", "UYP", "UYU", "UZS", "VEB", "VEE", "VEF", "VND", "VUV", "WST", "XAF", "XAG", "XAU", "XCD", "XOF", "XPF", "XSU", "YER", "ZAR", "ZMK", "ZMW", "ZRN", "ZWD", "ZWL"];

        var currencyList2 = [
          "(ADP) Andorran Peseta",
          "(AED) United Arab Emirates Dirham",
          "(AFN) Afghani",
          "(ALL) Lek",
          "(AMD) Armenian Dram",
          "(ANG) Netherlands Antillian Guilder",
          "(AOA) Kwanza",
          "(ARS) Argentine Peso",
          "(ATS) Austrian Schilling",
          "(AUD) Australian Dollar",
          "(AWG) Aruban Guilder",
          "(AZN) Azerbaijanian Manat",
          "(BAM) Convertible Marks",
          "(BBD) Barbados Dollar",
          "(BDT) Bangladeshi Taka",
          "(BEF) Belgian Franc",
          "(BGN) Bulgarian Lev",
          "(BHD) Bahraini Dinar",
          "(BIF) Burundian Franc",
          "(BMD) Bermudian Dollar",
          "(BND) Brunei Dollar",
          "(BOB) Boliviano",
          "(BOV) Bolivian Mvdol",
          "(BRL) Brazilian Real",
          "(BSD) Bahamian Dollar",
          "(BTN) Ngultrum",
          "(BWP) Pula",
          "(BYR) Belarussian Ruble",
          "(BZD) Belize Dollar",
          "(CAD) Canadian Dollar",
          "(CDF) Franc Congolais",
          "(CHF) Swiss Franc",
          "(CLF) Chilean UF",
          "(CLP) Chilean Peso",
          "(CNH) Offshore RMB (Chinese Yuan)",
          "(CNY) Yuan Renminbi",
          "(COP) Colombian Peso",
          "(COU) Unidad de Valor Real",
          "(CRC) Costa Rican Colon",
          "(CSD) Serbian Dinar",
          "(CUP) Cuban Peso",
          "(CVE) Cape Verde Escudo",
          "(CYP) Cyprus Pound",
          "(CZK) Czech Koruna",
          "(DEM) German Mark",
          "(DJF) Djibouti Franc",
          "(DKK) Danish Krone",
          "(DOP) Dominican Peso",
          "(DZD) Algerian Dinar",
          "(ECS) Ecuadorean Sucre",
          "(EEK) Kroon",
          "(EGP) Egyptian Pound",
          "(ERN) Nakfa",
          "(ESP) Spanish Peseta",
          "(ETB) Ethiopian Birr",
          "(EUR) Euro",
          "(FIM) Finnish Markka",
          "(FJD) Fiji Dollar",
          "(FKP) Falkland Islands Pound",
          "(FRF) French Franc",
          "(GBP) Pound Sterling",
          "(GBS) Guinea Bissau Peso",
          "(GEL) Lari",
          "(GGP) Guernsey Pound",
          "(GHC) Ghana Cedi",
          "(GHS) Cedi",
          "(GIP) Gibraltar pound",
          "(GMD) Dalasi",
          "(GNF) Guinea Franc",
          "(GQE) Equatorial Guinea",
          "(GRD) Greek Drachma",
          "(GTQ) Quetzal",
          "(GWP) Guinea-Bissau Peso",
          "(GYD) Guyana Dollar",
          "(HKD) Hong Kong Dollar",
          "(HNL) Lempira",
          "(HRK) Croatian Kuna",
          "(HTG) Haiti Gourde",
          "(HUF) Forint",
          "(IDR) Rupiah",
          "(IEP) Irish Punt",
          "(ILS) New Israeli Shekel",
          "(IMP) Isle Of Man Pound",
          "(INR) Indian Rupee",
          "(IQD) Iraqi Dinar",
          "(IRR) Iranian Rial",
          "(ISK) Iceland Krona",
          "(ITL) Italian Lira",
          "(JEP) Jersey Pound",
          "(JMD) Jamaican Dollar",
          "(JOD) Jordanian Dinar",
          "(JPY) Japanese yen",
          "(KES) Kenyan Shilling",
          "(KGS) Som",
          "(KHR) Riel",
          "(KMF) Comoro Franc",
          "(KPW) North Korean Won",
          "(KRW) South Korean Won",
          "(KWD) Kuwaiti Dinar",
          "(KYD) Cayman Islands Dollar",
          "(KZT) Tenge",
          "(LAK) Kip",
          "(LBP) Lebanese Pound",
          "(LKR) Sri Lanka Rupee",
          "(LRD) Liberian Dollar",
          "(LSL) Loti",
          "(LTL) Lithuanian Litas",
          "(LUF) Luxembourg Franc",
          "(LVL) Latvian Lats",
          "(LVR) Latvian Lat",
          "(LYD) Libyan Dinar",
          "(MAD) Moroccan Dirham",
          "(MDL) Moldovan Leu",
          "(MGA) Malagasy Ariary",
          "(MGF) Madagascar Franc",
          "(MKD) Denar",
          "(MLF) Mali Republic Franc",
          "(MMK) Kyat",
          "(MNT) Tugrik",
          "(MOP) Pataca",
          "(MRO) Ouguiya",
          "(MTL) Maltese Lira",
          "(MTP) Maltese Lira",
          "(MUR) Mauritius Rupee",
          "(MVR) Rufiyaa",
          "(MWK) Kwacha",
          "(MXN) Mexican Peso",
          "(MYR) Malaysian Ringgit",
          "(MZM) Mozambique Metical",
          "(MZN) Metical",
          "(NAD) Namibian Dollar",
          "(NGN) Naira",
          "(NIC) Nicaragua Cordoba",
          "(NID) New Iraqi Dinar",
          "(NIO) Cordoba Oro",
          "(NLG) Dutch Guilder",
          "(NOK) Norwegian Krone",
          "(NPR) Nepalese Rupee",
          "(NZD) New Zealand Dollar",
          "(OMR) Rial Omani",
          "(PAB) Balboa",
          "(PEN) Nuevo Sol",
          "(PGK) Kina",
          "(PHP) Philippine Peso",
          "(PKR) Pakistan Rupee",
          "(PLN) Zloty",
          "(PTE) Portuguese Escudo",
          "(PYG) Guarani",
          "(QAR) Qatari Rial",
          "(ROL) Romanian Leu",
          "(RON) Romanian New Leu",
          "(RSD) Serbian Dinar",
          "(RUB) Russian Ruble",
          "(RWF) Rwanda Franc",
          "(SAR) Saudi Riyal",
          "(SBD) Solomon Islands Dollar",
          "(SCR) Seychelles Rupee",
          "(SDD) Sudanese Dinar",
          "(SDG) Sudanese Pound",
          "(SDP) Old Sudanese Pound",
          "(SEK) Swedish Krona",
          "(SGD) Singapore Dollar",
          "(SHP) Saint Helena Pound",
          "(SIT) Slovenia Tolar",
          "(SKK) Slovak Koruna",
          "(SLL) Leone",
          "(SOS) Somali Shilling",
          "(SPL) Seborga Luigini",
          "(SRD) Surinam Dollar",
          "(SRG) Surinam Guilder",
          "(SSP) South Sudanese Pound",
          "(STD) Dobra",
          "(SVC) El Salvador Colon",
          "(SYP) Syrian Pound",
          "(SZL) Lilangeni",
          "(THB) Baht",
          "(THO) Thai Baht Onshore",
          "(TJS) Somoni",
          "(TMM) Manat",
          "(TMT) New Turkmenistan Manat",
          "(TND) Tunisian Dinar",
          "(TOP) Pa'anga",
          "(TPE) East Timor Escudo",
          "(TRL) Old Turkish Lira",
          "(TRY) New Turkish Lira",
          "(TTD) Trinidad and Tobago Dollar",
          "(TVD) Tuvalu Dollar",
          "(TWD) New Taiwan Dollar",
          "(TZS) Tanzanian Shilling",
          "(UAH) Hryvnia",
          "(UDI) Mexican UDI",
          "(UGX) Uganda Shilling",
          "(USD) US Dollar",
          "(UYP) Uruguay Peso",
          "(UYU) Peso Uruguayo",
          "(UZS) Uzbekistan Som",
          "(VEB) Venezuelan bolívar",
          "(VEE) Venezuela Essential Rate",
          "(VEF) Venezuelan Bolivar",
          "(VND) Vietnamese ??ng",
          "(VUV) Vatu",
          "(WST) Samoan Tala",
          "(XAF) CFA Franc BEAC",
          "(XAG) Silver",
          "(XAU) Gold",
          "(XCD) East Caribbean Dollar",
          "(XOF) CFA Franc BCEAO",
          "(XPF) Pacific Island  Franc",
          "(XSU) Sucre",
          "(YER) Yemeni Rial",
          "(ZAR) South African Rand",
          "(ZMK) Kwacha",
          "(ZMW) Zambian Kwacha",
          "(ZRN) Zaire Zaire",
          "(ZWD) Zimbabwe Dollar",
          "(ZWL) Zimbabwe Dollar (New)"];


        var oSettings = $.extend({
            sHeader: "Value",
            bHighlightEditedRow: true,
            sClassToHighlight: "info",
            sClassEdited: "row_edited",
            iInputLimit: 3,
            bAutoComplete: true,
            aCurrencyList: currencyList,
            fnFocusOutCallback: function () { }
        }, options);

        var oCell = $(this);

        oCell.on("focusout blur", function (event) {
            var crncy;
            if (!oCell.is("td")) {
                crncy = $(this).val().toUpperCase();

                $(this).val(crncy);
                if ($.inArray(crncy, oSettings.aCurrencyList) < 0) {
                    if (crncy.length > 0) {
                        $(this).val("");
                        notifyInputError("Invalid Currency.");
                        return;
                    }
                }
            } else {
                crncy = $(this).text().toUpperCase();
                $(this).text(crncy);
                if ($.inArray(crncy, oSettings.aCurrencyList) < 0) {
                    if (crncy.length > 0) {
                        $(this).text("");
                        notifyInputError("Invalid Currency.");
                        return;
                    }
                }
            }

            oSettings.fnFocusOutCallback.call(this, $(this));
        });

        oCell.attr("contenteditable", "true");

        if (oSettings.bAutoComplete) {
            oCell.attr("data-provide", "typeahead");
            oCell.typeahead({ source: oSettings.aCurrencyList });
        }

        if (!oCell.is("td")) {
            oCell.attr("maxlength", "3");
            return;
        }
    };

    function defineNavigation(oCellInFocus, event) {

        var isShiftOrCtrl = !!window.event.shiftKey || !!window.event.ctrlKey;

        if (isShiftOrCtrl)
            return;

        var nextCellRight = oCellInFocus.next("td[contenteditable='true']");

        if (nextCellRight.length == 0) {
            nextCellRight = oCellInFocus.parent().nextUntil().children("td[contenteditable='true']:eq(0)");
        }

        if (nextCellRight.length == 0 || nextCellRight.attr("contenteditable") !== "true") {
            nextCellRight = oCellInFocus.parent().parent().find("td[contenteditable='true']:eq(0)");
        }

        var nextCellLeft = oCellInFocus.prev("td[contenteditable='true']");

        if (nextCellLeft.length == 0) {
            nextCellLeft = oCellInFocus.prevUntil().parent().children("td[contenteditable='true']:last");
        }

        if (nextCellLeft.length == 0 || nextCellLeft.attr("contenteditable") !== "true") {
            nextCellLeft = oCellInFocus.parent().parent().find("td[contenteditable='true']:last");
        }


        var nextCellBelow = oCellInFocus.parent().nextUntil().children("td:eq(" + oCellInFocus.index() + ")");

        //if (nextCellBelow.length == 0 || nextCellBelow.attr("contenteditable") !== "true") {
        //    nextCellBelow = oCellInFocus.parent().parent().find("td[contenteditable='true']:eq(" + oCellInFocus.index() + ")");
        //}

        var nextCellAbove = oCellInFocus.parent().prevUntil().children("td:eq(" + oCellInFocus.index() + ")");


        //if (nextCellAbove.length == 0 || nextCellAbove.attr("contenteditable") !== "true") {
        //    nextCellAbove = oCellInFocus.parent().parent().find("td[contenteditable='true']:eq(" + oCellInFocus.index() + ")");
        //}


        //TAB
        if (event.keyCode === 9) {
            if (nextCellRight.attr("contenteditable") === "true") {
                nextCellRight.focus();
            } else {
                oCellInFocus.closest().find("td[contenteditable='true']:eq(0)").focus();
            }
            event.preventDefault();
            return;
        }

        //ENTER
        if (event.keyCode === 13) {
            if (nextCellBelow.length !== 0 && nextCellBelow.attr("contenteditable") === "true")
                nextCellBelow.focus();
                //else if (nextCellRight.length !== 0 && nextCellRight.attr("contenteditable") === "true")
                //    nextCellRight.focus();
            else
                oCellInFocus.focusout();

            event.preventDefault();
            return;
        }

        //RIGHT ARROW
        if (event.keyCode == 39) {
            if (nextCellRight.length !== 0)
                nextCellRight.focus();

            return;
        }

        //LEFT ARROW
        if (event.keyCode == 37) {
            if (nextCellLeft.length !== 0)
                nextCellLeft.focus();

            return;
        }

        //UP ARROR
        if (event.keyCode == 38) {
            if (nextCellAbove.length !== 0)
                nextCellAbove.focus();

            event.preventDefault();
            return;
        }

        //DOWN ARROW
        if (event.keyCode == 40) {
            if (nextCellBelow.length !== 0)
                nextCellBelow.focus();

            event.preventDefault();
            return;
        }
    }


})(jQuery);
