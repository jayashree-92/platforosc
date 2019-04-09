

var tblAuditLogsDetails;

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


    $scope.fnGetUserAuditLogs = function () {
        var moduleText = "All";
        //if ($("#liModules").select2("data") != null && $("#liModules").select2("data").text != null)
        //    moduleText = $("#liModules").select2("data").text;
        createAuditLogsTable($scope.RangeStartDate, $scope.RangeEndDate, moduleText);
    }

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
                "order": [[2, "desc"]],
                "scrollX": true,
                "scrollY": $("#tblAuditLogsDetails").offset().top + 450,
                "aoColumns": [{
                    "sTitle": "Description",
                    "mData": "Log",
                    "className": "seeFullText"
                },
                {
                    "sTitle": "Action",
                    "mData": "Action",
                    "mRender": function (tdata, type, row) {
                        switch (tdata) {
                            case "Log In": return "<span class='text-info'>Log In</span>";
                            case "Log Out": return "<span class='text-success'>Log Out</span>";
                            case "Edited": return "<span class='text-warning'>Edited</span>";
                        }
                    }
                },
                {
                    "sTitle": "Field Modified",
                    "mData": "Field",
                    "mRender": function (tdata, type, row) {
                        switch (row.Action) {
                            case "Log In": 
                            case "Log Out": return "<span class='text-success'>Log Out</span>";
                            case "Edited": return "<span class='text-warning'>Edited</span>";
                        }
                    }
                },
                {
                    "sTitle": "User",
                    "mData": "ModifiedStateValue",
                    "className": "seeFullText"
                },
                {
                "sTitle": "User",
                "mData": "UserName",
                "className": "seeFullText"
                },
                {
                "sTitle": "User Activity",
                "mData": "hmsUserAuditLogId",
                "mRender": function (tdata, type, row) {
                    switch (row.Action) {
                        case "Log In": return row.UserName + " logged into the Operations Secure System.";
                        case "Log Out": return row.UserName + " logged out from the Operations Secure System.";
                        case "Added": if (row.Field == "Wire Status") {
                            return row.UserName + " added the status as " + $scope.getWireStatus(row.ModifiedStateValue) + (row.IsLogFromOps ? " in Operations." : "");
                        }
                        else {
                            return row.UserName + " added " + row.Field + " as <b>" + $scope.getFieldValue(row.Field, row.ModifiedStateValue) + "</b>" + (row.IsLogFromOps ? " in Operations." : "");
                        }
                        case "Edited": if (row.Field == "Wire Status") {
                            return row.UserName + " modified the status from " + $scope.getWireStatus(row.PreviousStateValue) + " to " + $scope.getWireStatus(row.ModifiedStateValue) + (row.IsLogFromOps ? " in Operations." : "");
                        }
                        else {
                            return row.UserName + " modified " + row.Field + " from <b>" + $scope.getFieldValue(row.Field, row.PreviousStateValue) + "</b>" + " to <b>" + $scope.getFieldValue(row.Field, row.ModifiedStateValue) + "</b>" + (row.IsLogFromOps ? " in Operations." : "");
                        }
                    }
                }
            },
            {
                "mData": "CreatedAt", "sTitle": "Updated As Of",
                "type": "dotnet-date",
                "mRender": function (tdata, type, row) {
                    if (tdata == null)
                        return "-";

                    return "<div  class='auditUpdatedAtColumn' title='" + getDateForToolTip(tdata) + "' date ='" + tdata + "'>" + $.getPrettyDate(tdata) + "</div>";
                }
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

    $scope.getWireStatus = function (wireStatusId)
    {
        switch (parseInt(wireStatusId)) {
            case 1: return "<span class='text-info'>Drafted</span>";
            case 2: return "<span class='text-warning'>Initiated</span>";
            case 3: return "<span class='text-danger'>Approved</span>";
            case 4: return "<span class='text-blocked'>Cancelled</span>";
            case 5: return "<span class='text-danger'>Failed</span>";
        }
    }

    $scope.getFieldValue = function (field, value) {
        switch (field) {
            case "Value Date": return value;
            case "Amount": return $.convertToCurrency(value, 2);
            case "Wire Message Type": return $filter('filter')($scope.MessageTypes, { id: parseInt(value) }, true)[0].text;
            case "Delivery Charges": return $filter('filter')($scope.DeliveryCharges, { id: value }, true)[0].text;
        }
    }

    $scope.DeliveryCharges = [{ id: "BEN", text: "Beneficiary" }, { id: "OUR", text: "Our customer charged" }, { id: "SHA", text: " Shared charges" }];

    $http.get("/Audit/GetMessageTypesForAudits").then(function (response) {
       $scope.MessageTypes = response.data;
    });

});

