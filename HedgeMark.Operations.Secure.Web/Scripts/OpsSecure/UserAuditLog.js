﻿

var tblAuditLogsDetails;
$("#liUserLogs").addClass("active");

HmOpsApp.controller("UserAuditsLogsCtrl", function ($scope, $http, $timeout, $filter) {

    $scope.RangeStartDate = moment().subtract(1, "days");
    $scope.RangeEndDate = moment();

    function dateRangeOnChangeCallback(start, end) {
        $("#userAuditLogDateRange span").html(start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY"));
        $scope.RangeStartDate = start.toDate();
        $scope.RangeEndDate = end.toDate();
    }

    dateRangeOnChangeCallback(moment().subtract(1, "days"), moment());

    $("#userAuditLogDateRange").daterangepicker({
        "alwaysShowCalendars": true,
        "showDropdowns": true,
        "startDate": moment().subtract(1, "days"),
        "endDate": moment(),
        "autoApply": true,
        "maxDate": moment(),
        ranges: {
            'Today and Yesterday': [moment().subtract(1, "days"), moment()],
            'Last 7 Days': [moment().subtract(6, "days"), moment()],
            'Last 30 Days': [moment().subtract(29, "days"), moment()],
            'This Month': [moment().startOf("month"), moment()],
            'Last Month': [moment().subtract(1, "month").startOf("month"), moment().subtract(1, "month").endOf("month")],
            'This Year': [moment().startOf("year"), moment()]
        }
    }, dateRangeOnChangeCallback);

    $scope.isUserAuditsActive = true;
    $scope.isFundAccountLog = true;

    $scope.fnGetAuditLogs = function () {
        if ($scope.isUserAuditsActive)
            $scope.fnGetUserAuditLogs();
        else
            $scope.getBulkUploadLogs();

    }

    $scope.navigateAuditTabs = function (isUserAudit) {
        if ($scope.isUserAuditsActive != isUserAudit) {
            $scope.isUserAuditsActive = isUserAudit;
            $scope.fnGetAuditLogs();
        }
    }

    $scope.navigateUploadTabs = function (isFundAccount) {
        if ($scope.isFundAccountLog != isFundAccount) {
            $scope.isFundAccountLog = isFundAccount;
            $scope.getBulkUploadLogs();
        }
    }

    $scope.fnGetUserAuditLogs = function () {
        var moduleText = "All";
        //if ($("#liModules").select2("data") != null && $("#liModules").select2("data").text != null)
        //    moduleText = $("#liModules").select2("data").text;
        createAuditLogsTable($scope.RangeStartDate, $scope.RangeEndDate, moduleText);
    }

    $scope.getBulkUploadLogs = function () {
        $("#btnGetAuditLogs").button("loading");
        $http.get("/Audit/GetBulkUploadLogs?startDate=" + moment($scope.RangeStartDate).format("YYYY-MM-DD") + "&endDate=" + moment($scope.RangeEndDate).format("YYYY-MM-DD") + "&isFundAccountLog=" + $scope.isFundAccountLog).then(function (response) {
            var containerId = $scope.isFundAccountLog ? "#tblAccountUploadDetails" : "#tblSSITemplateUploadDetails";
            fnDestroyDataTable(containerId);
            var auditLogTable = $(containerId).DataTable({
                "bDestroy": true,
                // responsive: true,
                aaData: response.data,
                "aoColumns": [
                    {
                        "sTitle": "File Name",
                        "mData": "FileName"
                    },
                    {
                        "sTitle": "Uploaded By",
                        "mData": "UserName",
                    },
                    {
                        "sTitle": "Uploaded At",
                        "mData": "CreatedAt",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + (moment(tdata).fromNow()) + "</div>";
                        }
                    }
                ],
                "deferRender": false,
                "bScrollCollapse": true,
                //scroller: true,
                //sortable: false,
                "searching": false,
                "bInfo": false,
                "sDom": "ift",
                //pagination: true,
                "sScrollX": "100%",
                "sScrollXInner": "100%",
                "scrollY": 350,
                "order": [[2, "desc"]],

                "fnRowCallback": function (nRow, aData) {
                    if (aData.FileName != "") {
                        $("td:eq(0)", nRow).html("<a title ='click to download the file' href='/Audit/DownloadLogFile?fileName=" + aData.FileName + "&isFundAccountLog=" + aData.IsFundAccountLog  + "'>" + aData.FileName + "</a>");
                    }
                },
                "oLanguage": {
                    "sSearch": "",
                    "sEmptyTable": "No logs available.",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ Files"
                }
            });
            $("#btnGetAuditLogs").button("reset");
        }, function (error) {
            $("#btnGetAuditLogs").button("reset");
            notifyError(error.Message);
        });
        
    }


    var actions = []; actions.push("All");
    // The indices are hardcoded w.r.t table index   
    var actionIndex = 0;

    function createAuditLogsTable(auditStartDate, auditEndDate, module) {
        $("#btnGetAuditLogs").button("loading");
        fnDestroyDataTable("#tblAuditLogsDetails");
        $http.get("/Audit/GetWireAuditLogs?startDate=" + moment(auditStartDate).format("YYYY-MM-DD") + "&endDate=" + moment().format("YYYY-MM-DD") + "&module=" + module).then(function (response) {
            var auditLogTable = $("#tblAuditLogsDetails").DataTable({
                "aaData": response.data,
                "dom": "<\"toolbar\"><'row header'<'col-md-6 header-left'i><'col-md-6 header-right'f>>trI",
                "deferRender": true,
                "scroller": true,
                "iDisplayLength": -1,
                "bScrollCollapse": true,
                "autoWidth": false,
                //"sScrollX": "100%",
                //"sScrollXInner": "100%",
                "bDestroy": true,
                "order": [[7, "desc"]],
                "scrollX": true,
                "scrollY": window.innerHeight - 350,
                "initComplete": function (settings, json) {
                    var actionsMessage = "";
                    if (actions.length > 1) {
                        $(actions).each(function (i, action) {
                            if (action == "Deleted") {
                                actionsMessage += "<button type=\"button\" class=\"btn btn-sm btn-danger\" onclick=\"$('#tblAuditLogsDetails').dataTable().fnFilter('" + action + "', " + actionIndex + ");\" id=" + action + ">" + action + "</button>";
                            }
                            else if (action == "Edited" || action == "Updated") {
                                actionsMessage += "<button type=\"button\" class=\"btn btn-sm btn-warning\" onclick=\"$('#tblAuditLogsDetails').dataTable().fnFilter('" + action + "', " + actionIndex + ");\" id=" + action + ">" + action + "</button>";
                            }
                            else if (action == "Added") {
                                actionsMessage += "<button type=\"button\" class=\"btn btn-sm btn-success\" onclick=\"$('#tblAuditLogsDetails').dataTable().fnFilter('" + action + "', " + actionIndex + ");\" id=" + action + ">" + action + "</button>";
                            }
                            else if (action == "All") {
                                actionsMessage += "<button type=\"button\" class=\"btn btn-sm btn-default active\" onclick=\"$('#tblAuditLogsDetails').dataTable().fnFilter('', " + actionIndex + ");\" id=" + action + ">" + action + "</button>";
                            }
                        });
                        $("div.toolbar").append("<div class=\"btn-group btn-group-sm\" data-toggle=\"buttons-radio\" id = \"actionFilter\">" + actionsMessage + "</div>");
                    }
                },
                "mark": { "exclude": [".ignoreMark"] },
                "columnDefs": [{ className: "ignoreMark", "targets": [0] }],
                "aoColumns": [
                    {
                        "sTitle": "Action",
                        "mData": "Action",
                        "mRender": function (tdata, type, row) {


                            if ($.inArray(tdata, actions) < 0) {
                                actions.push(tdata);
                            }

                            switch (tdata) {
                                case "Logged In":
                                case "Logged Out":
                                    return "<span class=\"label label-default ignoreMark\"> " + tdata + "</span>";
                                case "Edited":
                                case "Updated":
                                    return "<span class=\"label label-warning ignoreMark\"> " + tdata + "</span>";
                                case "Added":
                                    return "<span class=\"label label-success ignoreMark\"> " + tdata + "</span>";
                            }
                        }
                    },

                    {
                        "sTitle": "Description",
                        "mData": "Log",
                        "className": "seeFullText"
                    },

                {
                    "sTitle": "Field Modified",
                    "mData": "Field",
                    "mRender": function (tdata, type, row) {
                        return tdata != null ? tdata : "";
                    }
                },
                {
                    "sTitle": "Previous State Value",
                    "mData": "PreviousStateValue",
                    "mRender": function (tdata, type, row) {
                        switch (row.Action) {
                            case "Logged In":
                            case "Logged Out":
                            case "Added": return "";
                            case "Edited":
                            case "Updated":
                                if (row.Field == "Wire Status") {
                                    return $scope.getWireStatus(row.PreviousStateValue);
                                }
                                else {
                                    return $scope.getFieldValue(row.Field, row.PreviousStateValue);
                                }
                        }
                    }
                },
                {
                    "sTitle": "Modified State Value",
                    "mData": "ModifiedStateValue",
                    "mRender": function (tdata, type, row) {
                        switch (row.Action) {
                            case "Logged In":
                            case "Logged Out":
                                return "";
                            case "Edited":
                            case "Updated":
                            case "Added":
                                if (row.Field == "Wire Status") {
                                    return $scope.getWireStatus(row.ModifiedStateValue);
                                }
                                else {
                                    return "<b>" + $scope.getFieldValue(row.Field, row.ModifiedStateValue) + "</b>";
                                }
                        }
                    }
                },
                {
                    "sTitle": "Origin",
                    "mData": "IsLogFromOps",
                    "mRender": function (tdata, type, row) {
                        return !tdata ? "Operations Secure" : "Operations";
                    }
                },
                {
                    "sTitle": "User",
                    "mData": "UserName",
                    "className": "seeFullText"
                },
                {
                    "mData": "CreatedAt", "sTitle": "Updated As Of",                    
                    "mRender": renderDotNetDateAndTime
                }],
                "oLanguage": {
                    "sSearch": "",
                    "sEmptyTable": "No audit logs available for the selected context date.",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ user actions"
                }
            });
            $("html, body").animate({ scrollTop: $("#tblAuditLogsDetails").offset().top }, "slow");
            $("#btnGetAuditLogs").button("reset");
        }, function (error) {
            $("#btnGetAuditLogs").button("reset");
            notifyError(error.Message);
        });
    }

    $scope.getWireStatus = function (wireStatusId) {
        switch (parseInt(wireStatusId)) {
            case 1: return "<span class='text-info'><b>Drafted</b></span>";
            case 2: return "<span class='text-warning'><b>Initiated</b></span>";
            case 3: return "<span class='text-success'><b>Approved</b></span>";
            case 4: return "<span class='text-blocked'><b>Cancelled</b></span>";
            case 5: return "<span class='text-danger'><b>Failed</b></span>";
        }
    }

    $scope.getFieldValue = function (field, value) {
        switch (field) {
            case "Value Date": return value;
            case "Amount": return "<b>" + $.convertToCurrency(value, 2) + "</b>";
            case "Wire Message Type": return "<b>" + $filter('filter')($scope.MessageTypes, { id: parseInt(value) }, true)[0].text + "</b>";
            case "Delivery Charges": return "<b>" + $filter('filter')($scope.DeliveryCharges, { id: value }, true)[0].text + "</b>";
        }
        return value;
    }

    $scope.DeliveryCharges = [{ id: "BEN", text: "Beneficiary" }, { id: "OUR", text: "Our customer charged" }, { id: "SHA", text: " Shared charges" }];

    $http.get("/Audit/GetMessageTypesForAudits").then(function (response) {
        $scope.MessageTypes = response.data;
    });


    $scope.fnGetUserAuditLogs();

});

