/// <reference path="../data.js" />

HmOpsApp.controller("wiresDashboardCtrl", function ($scope, $http, $opsSharedScopes, $q, $filter, $timeout) {
    $opsSharedScopes.store("wiresDashboardCtrl", $scope);

    $scope.WireStatusIndex = -1;
    $scope.WireSourceReportIndex = -1;
    $scope.WirePurposeIndex = -1;
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
                                    }
                                    if (header == "Source Report") {
                                        $scope.WireSourceReportIndex = j;
                                    }
                                    if (header == "Wire Purpose") {
                                        $scope.WirePurposeIndex = j;
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


    $("#tblWireLogReport").off("dblclick", "tbody tr").on("dblclick", "tbody tr", function (event) {
        var wireData = $scope.tblWireLogReport.row($(this)).data();
        if (wireData == undefined)
            return;

        $scope.wireObj = {
            WireId: wireData[0],
            //AgreementId: 0,
            //AgreementName: "",
            //FundId: wireData.HMWire.hmFundId,
            //ContextDate: $scope.contextDate,
            Purpose: wireData[$scope.WirePurposeIndex],
            Report: wireData[$scope.WireSourceReportIndex],
            //IsAdhocWire: false,
            //ReportMapId: 0,
            //IsAdhocPage: true
        };

        $("#modalToRetrieveWires").modal({ backdrop: 'static', keyboard: true, show: true });
    });

    $("#modalToRetrieveWires").on("show.bs.modal", function () {
    }).on("shown.bs.modal", function () {
        $scope.IsWireTicketModelOpen = true;
        $opsSharedScopes.get("wireInitiationCtrl").fnLoadWireTicketDetails($scope.wireObj);
        $scope.$emit("wireTicketModelOpen");
    }).on("hide.bs.modal", function () {
        $scope.IsWireTicketModelOpen = false;
        $("#wireCurrentlyViewedBy").collapse("hide");
        if ($scope.wireObj.WireId > 0)
            $scope.fnRemoveActionInProgres($scope.wireObj.WireId);
        $scope.$emit("wireTicketModelClosed", { statusId: $scope.SelectedStatusId });
    });

    $scope.fnRemoveActionInProgres = function (wireId) {
        $http.post("/Home/RemoveActionInProgress?wireId=" + wireId);
    }

    $scope.fnExportDashboardReport = function (startDate, endDate, format, templateName) {
        window.location.href = "/WiresDashboard/ExportReport?startDate=" + startDate + "&endDate=" + endDate + "&format=" + format + "&templateName=" + templateName;
    }
});

$(document).ready(function () {
    $("#liWiresDashboard").addClass("active");
});