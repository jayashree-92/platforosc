

var tblWireLogsDetails;

HmOpsApp.controller("WireLogsCtrl", function ($scope, $http, $timeout) {

    $scope.RangeStartDate = moment().subtract(7, "days");
    $scope.RangeEndDate = moment();

    function dateRangeOnChangeCallback(start, end) {
        $("#wireLogDateRange span").html(start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY"));
        $scope.RangeStartDate = start.toDate();
        $scope.RangeEndDate = end.toDate();
    }
    dateRangeOnChangeCallback(moment().subtract(6, "days"), moment());

    $("#wireLogDateRange").daterangepicker({
        "alwaysShowCalendars": true,
        "showDropdowns": true,
        "startDate": moment().subtract(6, "days"),
        "endDate": moment(),
        "autoApply": true,
        "maxDate": moment().add(7, "days"),
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


    $scope.fnGetWireLogs = function () {
        $timeout(function () { $scope.$broadcast("loadWireDetailsGrid", 0, moment($scope.RangeStartDate).format("YYYY-MM-DD"), moment($scope.RangeEndDate).format("YYYY-MM-DD")) });
    };

});

$("#liWireLogs").addClass("active");