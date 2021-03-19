/// <reference path="../data.js" />

HmOpsApp.controller("wiresDashboardCtrl", function ($scope, $http, $opsSharedScopes, $q, $filter, $timeout) {
    $opsSharedScopes.store("wiresDashboardCtrl", $scope);


    $scope.fnLoadDashboard = function (startDate, endDate, preferences) {

        $("#btnGetDetails").button("loading");
        $http.post("/WiresDashboard/GetWireLogData", JSON.stringify({ startDate: startDate, endDate: endDate, searchPreference: preferences }), { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(
            function (response) {
                if ($("#tblWireLogReport").hasClass("initialized")) {
                    fnDestroyDataTable("#tblWireLogReport");
                }
                var wireLogDataData = JSON.parse(response.data.rows);

                $("#btnGetDetails").button("reset");

                $scope.tblWireLogReport = $("#tblWireLogReport").not(".initialized").addClass("initialized").DataTable(
                    {
                        "aaData": wireLogDataData.aaData,
                        "aoColumns": wireLogDataData.aoColumns,
                        "columnDefs": wireLogDataData.columnDefs,
                        "oLanguage": {
                            "sSearch": "",
                            "sInfo": "Showing _START_ to _END_ of _TOTAL_ Wire Data"
                        },
                        "scrollY": 500,
                        //"bScrollCollapse": true,
                        "deferRender": true,
                        "order": [[0, "asc"]],
                        "bPaginate": true,
                        "scroller": wireLogDataData.aaData.length > 20,
                        "scrollX": wireLogDataData.aaData.length > 0,
                        "sScrollXInner": "100%",
                        //"ordering": false,
                        "bStateSave": false,
                        "destroy": true,
                        "iDisplayLength": -1,
                        //"fixedColumns": {
                        //    leftColumns:5,
                        //},
                        "headerCallback": function (thead, data, start, end, display) {
                            $($(thead).find("th:contains(\"(\"),th:contains(\"-\")")).each(function (i, v) {
                                var header = $(this).html();
                                if (header.indexOf("<br") < 0) {
                                    var position = $(this).html().indexOf("(");
                                    var output = [header.slice(0, position), "<br/>", header.slice(position)].join("");
                                    $(this).html(output);
                                }
                                if (header.indexOf("(") > 0) {
                                    var split = header.split("(");
                                    var reportName = split[1].replace(")", "");
                                    var outputHeader = split[0] + "&nbsp;&nbsp;<label class='label " + (reportName == "Cash" ? " label-info" : "label-default") + " shadowBox'>" + reportName + "</label>";
                                    $(this).html(outputHeader);
                                }
                            });
                        },
                        "rowCallback": function (row, data, index) {
                            $(row).find("td:lt(6)").addClass("fixed");
                            var stringBuilder = "";
                            angular.forEach(data, function (val, ind) {
                                if (!isNaN(val) && val.indexOf("/") == -1 && val != "" && ind > 6 && ind != data.length - 3) {
                                    stringBuilder += "td:eq(" + (ind) + "),";
                                }
                            });
                            stringBuilder = stringBuilder.substring(0, stringBuilder.length - 1);

                            angular.forEach($(stringBuilder, row), function (val, i) {
                                $(val).html($.convertToCurrency($(val).html(), 2));
                                $(val).attr("title", $.convertToCurrency($(val).html(), 2));
                            });

                            //  $("td:eq(" + ($scope.statusIndex - 2) + ")", row).html(data[$scope.statusIndex] == "True" ? "Complete" : "Pending");

                        }
                    });

                $timeout(function () {
                    $scope.tblWireLogReport.columns.adjust().draw(true);
                    //if ($scope.HiddenReportId != 16) {
                    //var modelHeaderfixedColumn = new $.fn.dataTable.FixedColumns($scope.tblWireLogReport, {
                    //    leftColumns: 6
                    //});

                }, 100);
            }, function (response) {
                $("#btnGetDetails").button("reset");
                notifyError(response.data.Message);
            });
    };

    $scope.fnExportDashboardReport = function (startDate, endDate, format, templateName) {
        window.location.href = "/WiresDashboard/ExportReport?startDate=" + startDate + "&endDate=" + endDate + "&format=" + format + "&templateName=" + templateName;
    }
});

$(document).ready(function () {
    $("#liWiresDashboard").addClass("active");
});