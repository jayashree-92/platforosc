
$("#liMQLogs").addClass("active");
var tblAuditLogsDetails;

HmOpsApp.controller("MQLogsCtrl", function ($scope, $http, $timeout, $filter) {

    $scope.RangeStartDate = moment().subtract(1, "days");
    $scope.RangeEndDate = moment();

    function dateRangeOnChangeCallback(start, end) {
        $("#inboundLogDateRange span").html(start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY"));
        $scope.RangeStartDate = start.toDate();
        $scope.RangeEndDate = end.toDate();
    }

    dateRangeOnChangeCallback(moment().subtract(1, "days"), moment());

    $("#inboundLogDateRange").daterangepicker({
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
            'This Year': [moment().startOf("year"), moment()],
            'Last Year': [moment().subtract(1, "year").startOf("year"), moment().subtract(1, "year").endOf("year")]
        }
    }, dateRangeOnChangeCallback);


    $scope.fnGetInboundLogs = function () {
        createAuditLogsTable($scope.RangeStartDate, $scope.RangeEndDate);
    }

    function createAuditLogsTable(auditStartDate, auditEndDate) {
        $("#btnGetInboundLogs").button("loading");
        fnDestroyDataTable("#tblAuditLogsDetails");
        $http.get("/Audit/GetMQLogs?startDate=" + moment(auditStartDate).format("YYYY-MM-DD") + "&endDate=" + moment(auditEndDate).format("YYYY-MM-DD")).then(function (response) {
            var auditLogTable = $("#tblInboundLogsDetails").DataTable({
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
                "order": [[4, "desc"]],
                "scrollX": true,
                "scrollY": window.innerHeight - 350,
                "aoColumns": [
                    {
                        "mData": "IsOutBound", "sTitle": "Inbound/Outbound",
                        "mRender": function (tdata) {
                            return tdata ? "<label class='label label-primary'>Outbound&nbsp;&nbsp;<i class='glyphicon glyphicon-log-out'></i></label>" : "<label class='label label-info'><i class='glyphicon glyphicon-log-in'></i>&nbsp;&nbsp;Inbound</label>";
                        }

                    },
                    {
                        "mData": "QueueManager", "sTitle": "Queue Manager"
                    },
                    {
                        "mData": "QueueName", "sTitle": "Queue Name"
                    },
                    {
                        "sTitle": "Messages",
                        "mData": "Message",
                        "mRender": function (tdata) {
                            return "<p class=\"swiftMessgeBlock\">" + tdata + "</p>";
                        }
                    },
                    {
                        "mData": "CreatedAt", "sTitle": "Received At",
                        "mRender": renderDotNetDateAndTime
                    }, { "mData": "OpsSecureHandlerMessage", "sTitle": "Ops Secure Handler Message" }],
                "oLanguage": {
                    "sSearch": "",
                    "sEmptyTable": "No Inbound logs available for the selected context date.",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ user actions"
                }
            });
            $("html, body").animate({ scrollTop: $("#tblInboundLogsDetails").offset().top }, "slow");
            $("#btnGetInboundLogs").button("reset");
        }, function (error) {
            $("#btnGetInboundLogs").button("reset");
            notifyError(error.Message);
        });
    }
    $scope.fnGetInboundLogs();
});

$("#liLogs").addClass("active");