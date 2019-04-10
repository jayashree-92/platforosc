

var tblAuditLogsDetails;

HmOpsApp.controller("InboundLogsCtrl", function ($scope, $http, $timeout, $filter) {

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
            'This Year': [moment().startOf("year"), moment()]
        }
    }, dateRangeOnChangeCallback);


    $scope.fnGetInboundLogs = function () {
        createAuditLogsTable($scope.RangeStartDate, $scope.RangeEndDate);
    }

    function createAuditLogsTable(auditStartDate, auditEndDate) {
        $("#btnGetInboundLogs").button("loading");
        fnDestroyDataTable("#tblAuditLogsDetails");
        $http.get("/Audit/GetInboundMQLogs?startDate=" + moment(auditStartDate).format("YYYY-MM-DD") + "&endDate=" + moment().format("YYYY-MM-DD")).then(function (response) {
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
                "order": [[1, "desc"]],
                "scrollX": true,
                "scrollY": $("#tblInboundLogsDetails").offset().top + 900,
                "aoColumns": [{
                    "sTitle": "Inbound Message",
                    "mData": "InBoundMessage",
                    "mRender": function (tdata) {
                        return "<p class=\"swiftMessgeBlock\">" + tdata + "</p>";
                    }
                },
                {
                    "mData": "CreatedAt", "sTitle": "Received At",
                    "type": "dotnet-date",
                    "mRender": function (tdata, type, row) {
                        if (tdata == null)
                            return "-";

                        return "<div  class='auditUpdatedAtColumn' title='" + getDateForToolTip(tdata) + "' date ='" + tdata + "'>" + $.getPrettyDate(tdata) + "</div>";
                    }
                }],
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