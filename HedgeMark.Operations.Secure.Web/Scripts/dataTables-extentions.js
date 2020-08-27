
var splitColumnOrderString = function (data) {
    if (data != "undefined" && data != null && data != undefined) {
        var colOrderString = data;
        return colOrderString.split(",");
    }
    return {};
};

//var makeDataTable = function (tableId, options) {

//    if (!options)
//        options = $.extend({}, datatable_defaults(tableId), {});


//    var table = $('#' + tableId).dataTable(options);

//    postInitDataTables();

//    addColumnSetChanger(tableId);

//    return table;

//};

var getFormattedHeader = function (header) {
    if (header.indexOf("<br") < 0) {
        var position = header.indexOf("(");
        return [header.slice(0, position), "<br/>", header.slice(position)].join("");
    }
    return header;
}

var getHiddenCols = function (tableId) {

    var hiddenCols = new Array();
    var oTable = $("#" + tableId).dataTable();
    $(oTable.fnSettings().aoColumns).each(function () {
        if (!this.bVisible)
            hiddenCols.push($(this.nTh).prop("id"));
    });
    return hiddenCols.join();

};
var getHiddenColsArray = function (tableId) {

    var hiddenCols = new Array();
    var oTable = $("#" + tableId).dataTable();
    $(oTable.fnSettings().aoColumns).each(function () {
        if (!this.bVisible)
            hiddenCols.push($(this.nTh).prop("id"));
    });
    return hiddenCols;

};

var getColOrder = function (tableId) {
    var tdOrder = new Array();
    $("#" + tableId + " tr th").each(function () {
        tdOrder.push($(this).prop("id"));
    });

    var hiddenColsArraySorted = getHiddenColsArray(tableId).sort(sortColumnsAlphabetically);
    tdOrder.splice(1, 0, hiddenColsArraySorted);
    return tdOrder.join();
};
var sortColumnsAlphabetically = function (a, b) {
    var asTitle = a.toLowerCase();
    var bsTitle = b.toLowerCase();
    return ((asTitle < bsTitle) ? -1 : ((asTitle > bsTitle) ? 1 : 0));
};

var getVisibleColumns = function (tableId) {
    var tdOrder = new Array();
    $("#" + tableId + " tr th").each(function () {
        tdOrder.push($(this).prop("id"));
    });
    return tdOrder.join();
};

$.extend($.fn.dataTable.defaults, {
    //"searching": false,
    //"ordering": false,
    "dom": "<'row'<'col-md-6'i><'col-md-6 pull-right'f>>trI",
    "pagination": false,
    "processing": false,
    "sScrollX": false,
    "sScrollY": $(document).height() - 150,
    "responsive": true,
    "bSortClasses": false,
    "mark": { "ignorePunctuation": [","] },
    "autoWidth": false,
    'language': {
        //search: '<div class="input-group"><span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>',
        search: "",
        searchPlaceholder: "Search",
        searchPanes: {
            title: {
                _: 'Filters Selected - %d',
                0: 'No Filters Selected',
                //1: 'One Filter Selected',
            },
            //count: '{total} items',
            countFiltered: '{shown} / {total}',
            clearMessage: 'Clear Filters',
            //collapse: 'Filter',
        },
    }
});


$.fn.dataTableExt.oApi.fnPagingInfo = function (oSettings) {
    return {
        "iStart": oSettings._iDisplayStart,
        "iEnd": oSettings.fnDisplayEnd(),
        "iLength": oSettings._iDisplayLength,
        "iTotal": oSettings.fnRecordsTotal(),
        "iFilteredTotal": oSettings.fnRecordsDisplay(),
        "iPage": Math.ceil(oSettings._iDisplayStart / oSettings._iDisplayLength),
        "iTotalPages": Math.ceil(oSettings.fnRecordsDisplay() / oSettings._iDisplayLength)
    };
};

$.fn.dataTableExt.oApi.fnFilterAll = function (oSettings, sInput, iColumn, bRegex, bSmart) {
    var settings = $.fn.dataTableSettings;

    for (var i = 0; i < settings.length; i++) {
        settings[i].oInstance.fnFilter(sInput, iColumn, bRegex, bSmart);
    }
};


$.fn.dataTable.Api.register('column().title()', function () {
    var colheader = this.header();
    return $(colheader).text().trim();
});


$.fn.dataTable.Api.register('columns().startsWith()', function (startsWith) {

    var colIndexes = [];
    var thisObj = this;
    $.each(this.header(), function (i, v) {

        if (colIndexes.indexOf(i) !== -1)
            return;

        if ($(v).html().startsWith(startsWith))
            colIndexes.push(i);
    });

    return this.columns(colIndexes);
});

$.fn.dataTable.Api.register('columns().contains()', function (inputStr) {

    var colIndexes = [];
    $.each(this.header(), function (i, v) {

        if (colIndexes.indexOf(i) !== -1)
            return;

        if ($(v).text().indexOf(inputStr) > -1)
            colIndexes.push(i);
    });

    return this.columns(colIndexes);
});



$.fn.dataTable.Api.register('columns().showWhenHeader()', function (inputStr) {

    var allColumns = this;
    $.each(this[0], function (i, v) {
        var thisColumn = allColumns.column(v);
        if ($(thisColumn.header()).text().indexOf(inputStr) > -1) {
            thisColumn.visible(true);
        }
    });
});

$.fn.dataTable.Api.register('columns().showWhenHeaderWithPulse()', function (inputStr) {

    var allColumns = this;
    $.each(this[0], function (i, v) {
        var thisColumn = allColumns.column(v);
        if ($(thisColumn.header()).text().indexOf(inputStr) > -1) {
            thisColumn.visible(true);
            $(thisColumn.header()).pulse();
        }
    });
});


$.fn.dataTable.Api.register('columns().hideWhenHeader()', function (inputStr) {

    var allColumns = this;
    $.each(this[0], function (i, v) {
        var thisColumn = allColumns.column(v);
        if ($(thisColumn.header()).text().indexOf(inputStr) > -1) {
            thisColumn.visible(false);
        }
    });
});

//USAGE processedTable.columns().index("(LOCAL IA)")
$.fn.dataTable.Api.register('columns().indexes()', function (inputStr) {

    var colIndex = [];
    $.each(this.header(), function (i, v) {

        if ($(v).text().trim().indexOf(inputStr) > -1) {
            colIndex.push(i);
        }
    });

    return colIndex;
});

//USAGE processedTable.columns().index("(LOCAL IA)")
$.fn.dataTable.Api.register('columns().index()', function (inputStr) {

    var colIndex = -1;
    $.each(this.header(), function (i, v) {

        if ($(v).text().trim().indexOf(inputStr) > -1) {
            colIndex = $(this).index() + 1;
            return;
        }
    });

    return colIndex;
});

//USAGE processedTable.columns().containsIndex("(LOCAL IA)")
$.fn.dataTable.Api.register('columns().containsIndex()', function (inputStr) {

    var colIndex = -1;

    $.each(this.header(), function (i, v) {

        if ($(v).text().indexOf(inputStr) > -1)
            colIndex = $(this).index() + 1;
    });

    return colIndex;
});



$.fn.dataTableExt.oApi.fnReloadAjax = function (oSettings, sNewSource, fnCallback, bStandingRedraw) {

    if (typeof sNewSource != "undefined" && sNewSource != null) {
        oSettings.sAjaxSource = sNewSource;
    }

    // Server-side processing should just call fnDraw
    if (oSettings.oFeatures.bServerSide) {
        this.fnDraw();
        return;
    }

    this.oApi._fnProcessingDisplay(oSettings, true);
    var that = this;
    var iStart = oSettings._iDisplayStart;
    var aData = [];

    this.oApi._fnServerParams(oSettings, aData);

    oSettings.fnServerData.call(oSettings.oInstance, oSettings.sAjaxSource, aData, function (json) {
        /* Clear the old information from the table */
        that.oApi._fnClearTable(oSettings);

        /* Got the data - add it to the table */
        var aData = (oSettings.sAjaxDataProp !== "") ?
            that.oApi._fnGetObjectDataFn(oSettings.sAjaxDataProp)(json) : json;

        for (var i = 0; i < aData.length; i++) {
            that.oApi._fnAddData(oSettings, aData[i]);
        }

        oSettings.aiDisplay = oSettings.aiDisplayMaster.slice();

        if (typeof bStandingRedraw != "undefined" && bStandingRedraw === true) {
            oSettings._iDisplayStart = iStart;
            that.fnDraw(false);
        }
        else {
            that.fnDraw();
        }

        that.oApi._fnProcessingDisplay(oSettings, false);

        /* Callback user function - for event handlers etc */
        if (typeof fnCallback == "function" && fnCallback != null) {
            fnCallback(oSettings);
        }
    }, oSettings);
};

jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "currency-pre": function (a) {
        if (a.indexOf("(") >= 0)
            a = a.replace("(", "-");

        if (a == "" || a == null || a == undefined)
            a = "0";

        if (a == "True")
            a = "2";

        if (a == "False")
            a = "1";

        a = (a === "-") ? 0 : a.replace(/[^\d\-\.]/g, "");
        return parseFloat(a);
    },

    "currency-asc": function (a, b) {
        return a - b;
    },

    "currency-desc": function (a, b) {
        return b - a;
    }
});

jQuery.fn.dataTable.ext.type.search.currency = function (data) {
    return !data ? "" : typeof data === "string" ? data + data.replace(/[^\d\-\.()]/g, "") : data;
};


jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "timespan-pre": function (a) {
        return a.Ticks;
    },

    "timespan-asc": function (a, b) {
        return a - b;
    },

    "timespan-desc": function (a, b) {
        return b - a;
    }
});


jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "dotnet-date-pre": function (a) {
        a = $(a).attr("date");

        if (a === "" || a == null)
            a = "0";

        a = (a === "-") ? 0 : a.replace(/[^\d\-\.]/g, "");
        return parseFloat(a);
    },

    "dotnet-date-asc": function (a, b) {
        return a - b;
    },

    "dotnet-date-desc": function (a, b) {
        return b - a;
    }
});


jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "task-log-pre": function (a) {
        if ($(a).hasClass("label-success"))
            return 2;
        if ($(a).hasClass("label-warning"))
            return 1;
        //if (a.hasClass("label-danger"))
        return 0;
    },

    "task-log-asc": function (a, b) {
        return a - b;
    },

    "task-log-desc": function (a, b) {
        return b - a;
    }
});


jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "file-status-pre": function (a) {
        if ($(a).hasClass("label-danger")) { return 4; }

        if (a.trim() == "Not Received") {
            return 1;
        }

        if (a.trim() == "Received") {
            return 3;
        }

        return 2;
    },

    "file-status-asc": function (a, b) {
        return a - b;
    },

    "file-status-desc": function (a, b) {
        return b - a;
    }
});


jQuery.extend(jQuery.fn.dataTableExt.oSort, {
    "file-size-pre": function (a) {
        var x = a.substring(0, a.length - 2);

        var xUnit = (a.substring(a.length - 2, a.length) === "MB" ?
            1000 : (a.substring(a.length - 2, a.length) === "GB" ? 1000000 : 1));

        return parseInt(x * xUnit, 10);
    },

    "file-size-asc": function (a, b) {
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },

    "file-size-desc": function (a, b) {
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    }
});


/* Bootstrap style pagination control */
$.extend($.fn.dataTableExt.oPagination, {
    "bootstrap": {
        "fnInit": function (oSettings, nPaging, fnDraw) {
            var oLang = oSettings.oLanguage.oPaginate;
            var fnClickHandler = function (e) {
                e.preventDefault();
                if (oSettings.oApi._fnPageChange(oSettings, e.data.action)) {
                    fnDraw(oSettings);
                }
            };

            $(nPaging).addClass("pagination").append(
                "<ul>" +
                "<li class=\"prev disabled\"><a href=\"#\">&larr; " + oLang.sPrevious + "</a></li>" +
                "<li class=\"next disabled\"><a href=\"#\">" + oLang.sNext + " &rarr; </a></li>" +
                "</ul>"
            );
            var els = $("a", nPaging);
            $(els[0]).bind("click.DT", { action: "previous" }, fnClickHandler);
            $(els[1]).bind("click.DT", { action: "next" }, fnClickHandler);
        },

        "fnUpdate": function (oSettings, fnDraw) {
            var iListLength = 5;
            var oPaging = oSettings.oInstance.fnPagingInfo();
            var an = oSettings.aanFeatures.p;
            var i, j, sClass, iStart, iEnd, iHalf = Math.floor(iListLength / 2);

            if (oPaging.iTotalPages < iListLength) {
                iStart = 1;
                iEnd = oPaging.iTotalPages;
            }
            else if (oPaging.iPage <= iHalf) {
                iStart = 1;
                iEnd = iListLength;
            } else if (oPaging.iPage >= (oPaging.iTotalPages - iHalf)) {
                iStart = oPaging.iTotalPages - iListLength + 1;
                iEnd = oPaging.iTotalPages;
            } else {
                iStart = oPaging.iPage - iHalf + 1;
                iEnd = iStart + iListLength - 1;
            }

            for (i = 0, iLen = an.length; i < iLen; i++) {
                // Remove the middle elements
                $("li:gt(0)", an[i]).filter(":not(:last)").remove();

                // Add the new list items and their event handlers
                for (j = iStart; j <= iEnd; j++) {
                    sClass = (j == oPaging.iPage + 1) ? "class=\"active\"" : "";
                    $("<li " + sClass + "><a href=\"#\">" + j + "</a></li>")
                        .insertBefore($("li:last", an[i])[0])
                        .bind("click", function (e) {
                            e.preventDefault();
                            oSettings._iDisplayStart = (parseInt($("a", this).text(), 10) - 1) * oPaging.iLength;
                            fnDraw(oSettings);
                        });
                }

                // Add / remove disabled classes from the static elements
                if (oPaging.iPage === 0) {
                    $("li:first", an[i]).addClass("disabled");
                } else {
                    $("li:first", an[i]).removeClass("disabled");
                }

                if (oPaging.iPage === oPaging.iiTotalPages - 1 || oPaging.iTotalPages === 0) {
                    $("li:last", an[i]).addClass("disabled");
                } else {
                    $("li:last", an[i]).removeClass("disabled");
                }
            }
        }
    }
});


$.fn.dataTableExt.oApi.fnSelectAllRows = function (oSettings, selectedClass) {

    if (!selectedClass)
        selectedClass = "info";

    this.$("tr", { "filter": "applied" }).addClass(selectedClass);
    // window.getSelection().removeAllRanges();
};

$.fn.dataTableExt.oApi.fnDeselectAllRows = function (oSettings, selectedClass) {

    if (!selectedClass)
        selectedClass = "info";

    this.$("tr." + selectedClass, { "filter": "applied" }).removeClass(selectedClass);
    // window.getSelection().removeAllRanges();
};

$.fn.dataTableExt.oApi.fnResetAllFilters = function (oSettings, bDraw/*default true*/) {
    for (var iCol = 0; iCol < oSettings.aoPreSearchCols.length; iCol++) {
        oSettings.aoPreSearchCols[iCol].sSearch = "";
    }
    oSettings.oPreviousSearch.sSearch = "";

    if (typeof bDraw === "undefined") bDraw = true;
    if (bDraw) this.fnDraw();
};

$.fn.dataTableExt.oApi.fnGetSelectedRows = function (oSettings, selectedClass) {

    if (!selectedClass)
        selectedClass = "info";

    return this.$("tr." + selectedClass, { "filter": "applied" });
};

$.fn.dataTableExt.oApi.fnGetSelectedRowIds = function (oSettings, selectedClass) {

    if (!selectedClass)
        selectedClass = "info";

    return $.map(this.$("tr." + selectedClass, { "filter": "applied" }).children("td:nth-child(1)"), function (element) {
        return $(element).text();
    });

};

$.fn.dataTableExt.oApi.fnGetSelectedRowDataByIndex = function (oSettings, selectedClass, index) {
    if (!selectedClass)
        selectedClass = "info";

    return $.map(this.$("tr." + selectedClass, { "filter": "applied" }).children("td:nth-child(" + index + ")"), function (element) {
        return $(element).text();
    });

};


$.fn.dataTableExt.oApi.fnGetColumnIndexByTitle = function (oSettings, columnId) {
    var cols = oSettings.aoColumns;
    for (var x = 0, xLen = cols.length; x < xLen; x++) {
        if (cols[x].sTitle == columnId) {
            return x;
        };
    }
    return -1;
};


$.fn.dataTableExt.oApi.fnGetColumnIndexByTitleIfContains = function (oSettings, columnId) {
    var cols = oSettings.aoColumns;
    for (var x = 0, xLen = cols.length; x < xLen; x++) {
        if (cols[x].sTitle.indexOf(columnId) > -1) {
            return x;
        };
    }
    return -1;
};


$.fn.dataTableExt.oApi.fnGetColumnIndex = function (oSettings, columnId) {
    var cols = oSettings.aoColumns;
    for (var x = 0, xLen = cols.length; x < xLen; x++) {
        console.log(cols[x].nTh.getAttribute("id"));
        if (cols[x].nTh.getAttribute("id") == columnId) {
            return x;
        };
    }
    return -1;
};



$.fn.dataTableExt.oApi.fnHasUnsavedEdits = function (oSettings, unsavedClass) {

    if (!unsavedClass)
        unsavedClass = "editable-unsaved";

    var result = false;
    this.$("tr").children("td").each(function (i, aTd) {
        if ($(aTd).hasClass(unsavedClass)) {
            result = true;
            return false;
        }
    });
    return result;
};


$.fn.formatNumbersUI = function (element, value) {
    var validN1 = parseFloat($.convertToNumber(value, true));
    if (validN1 < 0)
        $(element).css("color", "red");
    else
        $(element).css("color", "");
}

$.fn.dataTableExt.aoFeatures.push({
    "fnInit": function (settings) {
        settings.aoRowCallback.push({
            "fn": function (row) {
                $(row).find("td").each(function () {
                    $(this).attr("title", $(this).text());

                    $.fn.formatNumbersUI($(this), $(this).text());

                    //var validN1 = parseFloat($.convertToNumber($(this).text(), true));
                    //if (validN1 < 0)
                    //    $(this).css("color", "red");
                    //else
                    //    $(this).css("color", "");
                });
            }
        });
    },
    "cFeature": "I",
    "sFeature": "AddToolTipToAllCells"
});

$.fn.dataTableExt.aoFeatures.push({
    "fnInit": function (settings) {
        settings.aoRowCallback.push({
            "fn": function (row) {
                $(row).find("td").each(function () {
                    $(this).attr("class", $(this).find("span").attr('class'));
                });
            }
        });
    },
    "cFeature": "H",
    "sFeature": "AddCellHighlightBasedOnInnerSpan"
});

$.fn.dataTableExt.oApi.fnSelectMultipleRows = function (oSettings, sSelectRowClass) {

}


$.fn.dataTableExt.oApi.fnSelectMatchingLabel = function (oSettings, sSelectRowClass) {
    console.log(oSettings);
    $(this).find("tbody td").off("dblclick").on("dblclick", function (event) {

        //var val = $(this).fnGetData();

        var sColumnValue = $(this).html();
        var iColumn = parseInt($(this).index());

        //oSettings.aoData[$(this).index()]._anHidden.every(function (x) {
        //    if (x != null)
        //        iColumn = iColumn + 1;
        //});


        //$.each(oSettings.aoData[$(this).index()]._aFilterData, function (x) {
        //    if (x != null)
        //        iColumn = iColumn + 1;
        //});

        //aiDisplay
        var aiRows = oSettings.aiDisplayMaster;
        var isSelected = $(this).parent().hasClass(sSelectRowClass);

        //if (iColumn > iSelectColumnCount - 1)
        //    return;

        for (var i = 0, c = aiRows.length; i < c; i++) {
            var iRow = aiRows[i];
            var aoDataRow = oSettings.aoData[iRow];
            var sValue = aoDataRow._aFilterData[iColumn];

            if (sValue != sColumnValue || sValue == "") continue;

            if (isSelected)
                $(aoDataRow.nTr).removeClass(sSelectRowClass);
            else
                $(aoDataRow.nTr).addClass(sSelectRowClass);
        }
    });
};

$.fn.dataTableExt.oApi.fnGetFirstMatchingRow = function (oSettings, sSearch, iColumn) {
    var i, iLen, j, jLen, aData = [];

    for (i = 0, iLen = oSettings.aoData.length; i < iLen; i++) {
        aData = oSettings.aoData[i]._aData;

        if (typeof iColumn == "undefined") {
            for (j = 0, jLen = aData.length; j < jLen; j++) {
                if (aData[j] == sSearch) {
                    return aData;
                }
            }
        }
        else if (aData[iColumn] == sSearch) {
            return aData;
        }
    }
    return aData;
};

$.fn.pulse = function (options) {

    var pulseOptions = $.extend({
        times: 2,
        duration: 150,
        minOpacity: 0
    }, options);

    var period = function (callback) {
        $(this).animate({ opacity: pulseOptions.minOpacity }, pulseOptions.duration, function () {
            $(this).animate({ opacity: 1 }, pulseOptions.duration, callback);
        });
    };
    return this.each(function () {
        var i = +pulseOptions.times,
            self = this,
            repeat = function () { --i && period.call(self, repeat); };
        period.call(this, repeat);
    });
};


function renderDotNetDateOnly(tdata, type, row, meta) {

    // If display or filter data is requested, format the date
    if (type === 'filter') {
        return getDateForDisplay(tdata);
    }

    if (type === 'sort') {
        return tdata;
    }

    return "<div>" + getDateForDisplay(tdata) + "</div>";
}


function renderDotNetDateAndTime(tdata, type, row, meta) {

    // If display or filter data is requested, format the date
    if (type === 'filter') {
        return getDateAndTimeForDisplay(tdata);
    }

    if (type === 'sort') {
        return tdata;
    }

    return "<div>" + getDateAndTimeForDisplay(tdata) + "</div>";
}

function renderDataAsCurrency(tdata, type, row) {

    if (type == "sort")
        return tdata;

    if (type == "filter") {
        return tdata + " " + $.convertToCurrency(tdata.toString(), 2);
    }

    return $.convertToCurrency(tdata.toString(), 2);
}