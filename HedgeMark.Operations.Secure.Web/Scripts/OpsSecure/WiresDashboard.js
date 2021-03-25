/// <reference path="../data.js" />

HmOpsApp.controller("wiresDashboardCtrl", function ($scope, $http, $opsSharedScopes, $q, $filter, $timeout) {
    $opsSharedScopes.store("wiresDashboardCtrl", $scope);

    $scope.WireStatusIndex = -1
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

                            $(thead).each(function (i, v) {
                                $($(v).html()).each(function (j, y) {
                                    var header = $(y).text();
                                    if (header == "Wire Status") {
                                        $scope.WireStatusIndex = j;
                                        return;
                                    }
                                });
                            });


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
                                    var outputHeader = split[0] + "&nbsp;&nbsp;<label class='label " + (reportName == "Cash" ? " label-info" : "label-default") + "'>" + reportName + "</label>";
                                    $(this).html(outputHeader);
                                }
                            });
                        },
                        "rowCallback": function (row, data) {

                            if ($scope.WireStatusIndex == -1)
                                return;

                            switch ($(data[$scope.WireStatusIndex]).text()) {
                                case "Drafted":
                                    // $(row).addClass("info");
                                    break;
                                case "Initiated":
                                case "Pending":
                                    $(row).addClass("warning");
                                    break;
                                case "Approved":
                                case "Processing":
                                    $(row).addClass("success");
                                    break;
                                case "Cancelled":
                                    $(row).addClass("blocked");
                                    break;
                                case "Completed":
                                    $(row).addClass("info");
                                    break;
                                case "Failed":
                                    $(row).addClass("danger");
                                    break;
                                case "On Hold":
                                    $(row).addClass("blockedSection");
                                    break;
                            }

                        },
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