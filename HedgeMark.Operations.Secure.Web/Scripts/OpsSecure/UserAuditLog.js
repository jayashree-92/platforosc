

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
                "scrollY": $("#tblAuditLogsDetails").offset().top + 450,
                "initComplete": function (settings, json) {
                    var actionsMessage = "";
                    if (actions.length > 1) {
                        $(actions).each(function (i, action) {
                            if (action == "Deleted") {
                                actionsMessage += "<button type=\"button\" class=\"btn btn-sm btn-danger\" onclick=\"$('#tblAuditLogsDetails').dataTable().fnFilter('" + action + "', " + actionIndex + ");\" id=" + action + ">" + action + "</button>";
                            }
                            else if (action == "Edited") {
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
                        return tdata != null ? tdata : "";
                    }
                },
                {
                    "sTitle": "Previous State Value",
                    "mData": "PreviousStateValue",
                    "mRender": function (tdata, type, row) {
                        switch (row.Action) {
<<<<<<< HEAD
                            case "Log In": 
                            case "Log Out": return "";
                            case "Edited": if (row.Field == "Wire Status") {
                                return  $scope.getWireStatus(row.PreviousStateValue);
=======
                            case "Log In":
                            case "Log Out":
                            case "Added": return "";
                            case "Edited": if (row.Field == "Wire Status") {
                                return $scope.getWireStatus(row.PreviousStateValue);
>>>>>>> 06d282f95336b758aedf29b94430fa49dcee3096
                            }
                            else {
                                return "<b>" + $scope.getFieldValue(row.Field, row.PreviousStateValue) + "</b>";
                            }
<<<<<<< HEAD
=======

>>>>>>> 06d282f95336b758aedf29b94430fa49dcee3096
                        }
                    }
                },
                {
                    "sTitle": "Modified State Value",
                    "mData": "ModifiedStateValue",
                    "mRender": function (tdata, type, row) {
                        switch (row.Action) {
<<<<<<< HEAD
                            case "Log In": 
                            case "Log Out": return "";
                            case "Edited": if (row.Field == "Wire Status") {
=======
                            case "Log In":
                            case "Log Out": return "";
                            case "Edited":
                            case "Added": if (row.Field == "Wire Status") {
>>>>>>> 06d282f95336b758aedf29b94430fa49dcee3096
                                return $scope.getWireStatus(row.ModifiedStateValue);
                            }
                            else {
                                return "<b>" + $scope.getFieldValue(row.Field, row.ModifiedStateValue) + "</b>";
                            }
                        }
                    }
<<<<<<< HEAD
                },
                {
                    "sTitle": "Origin",
                    "mData": "IsLogFromOps",
                    "mRender": function (tdata, type, row) {
                        return tdata == true || row.Action != "Edited" ? "Operations Secure" : "Operations";
                    }
=======
>>>>>>> 06d282f95336b758aedf29b94430fa49dcee3096
                },
                {
                    "sTitle": "Origin",
                    "mData": "IsLogFromOps",
                    "mRender": function (tdata, type, row) {
                        return tdata == true || row.Action != "Edited" ? "Operations Secure" : "Operations";
                    }
                },
                {
<<<<<<< HEAD
                "mData": "CreatedAt", "sTitle": "Updated As Of",
                "type": "dotnet-date",
                "mRender": function (tdata, type, row) {
                    if (tdata == null)
                        return "-";

                    return "<div  class='auditUpdatedAtColumn' title='" + getDateForToolTip(tdata) + "' date ='" + tdata + "'>" + $.getPrettyDate(tdata) + "</div>";
                }
            }],
=======
                    "sTitle": "User",
                    "mData": "UserName",
                    "className": "seeFullText"
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
>>>>>>> 06d282f95336b758aedf29b94430fa49dcee3096
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
            case "Amount": return $.convertToCurrency(value, 2);
            case "Wire Message Type": return $filter('filter')($scope.MessageTypes, { id: parseInt(value) }, true)[0].text;
            case "Delivery Charges": return $filter('filter')($scope.DeliveryCharges, { id: value }, true)[0].text;
        }
    }

    $scope.DeliveryCharges = [{ id: "BEN", text: "Beneficiary" }, { id: "OUR", text: "Our customer charged" }, { id: "SHA", text: " Shared charges" }];

    $http.get("/Audit/GetMessageTypesForAudits").then(function (response) {
        $scope.MessageTypes = response.data;
    });


    $scope.fnGetUserAuditLogs();

});

