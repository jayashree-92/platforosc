

var tblAuditLogsDetails;

HmOpsApp.controller("UserAuditsLogsCtrl", function ($scope, $http, $timeout) {

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

    };

});

