$(document).ready(function () {

});

HmOpsApp.controller("WiresHomeCtrl", function ($scope, $http, $timeout, $q) {
    $(".dashBoardTile").on("mouseover", function () {
        $(this).css("cursor", "pointer");
        $(this).addClass("shadowBoxSelect");
    });
    $(".dashBoardTile").on("mouseout", function () {
        if (!$(this).hasClass("tileSelected"))
            $(this).removeClass("shadowBoxSelect");
    });

    $(".dashBoardTile").on("click", function () {

        //var isAlreaadySelected = $(this).hasClass("shadowBoxSelect");

        $(".dashBoardTile").removeClass("tileSelected").removeClass("shadowBoxSelect");
        $(this).toggleClass("tileSelected").toggleClass("shadowBoxSelect");
    });

    $("#contextDate").datepicker({
        keyboardNavigation: true,
        format: "MM/dd/yyyy",
        daysOfWeekDisabled: [6, 0],
        autoclose: false,
        endDate: "+0d",
        //minViewMode: "days",
        //daysOfWeekHighlighted: "0,6",
        //datesDisabled: JSON.parse($("#holidayDateList").val()),
        weekStart: 1
    }).on("changeDate", function (ev) {
        //$("#contextDate").addClass("editable-unsaved");
        //$("#contextDate").html(moment(ev.date).format("MMM Do"));
        $scope.fnSetContextDate(moment(ev.date));
        $(".datepicker").hide();
        $scope.fnRetriveAllWireTickets($scope.SelectedStatusId);

    });

    $scope.$on("wireClosed", function () {
        $scope.fnRetriveAllWireTickets($scope.SelectedStatusId);
    });

    $scope.SelectedStatusId = 0;
    $scope.ShouldApplyDatepickerScope = false;
    $scope.ContextDate = {};
    $scope.WireStatusCounts = {};
    $scope.WireStatusCounts.Pending = 0;
    $scope.WireStatusCounts.Approved = 0;
    $scope.WireStatusCounts.Completed = 0;
    $scope.WireStatusCounts.Cancelled = 0;
    $scope.WireStatusCounts.CancelledAndProcessing = 0;
    $scope.WireStatusCounts.Failed = 0;
    $scope.WireStatusCounts.Total = 0;


    $scope.fnSetContextDate = function (dateTime) {

        $scope.ContextDate.Date = moment(dateTime).format("YYYY-MM-DD");
        $scope.ContextDate.Day = moment(dateTime).format("MMM Do");
        $scope.ContextDate.DayOfWeek = moment(dateTime).format('dddd');
        $scope.ContextDate.Year = moment(dateTime).format('YYYY');

        if ($scope.ShouldApplyDatepickerScope)
            $scope.$apply();

        $scope.ShouldApplyDatepickerScope = true;
        $timeout(function () {
            $(".dashDateNext").css("padding", "0px " + ($("#contextDate").width() - 5) + "px");
        }, 20);
    }

    $scope.fnSetContextDate(moment($("#hdnDefaultContextDate").val()));


    $scope.fnSetNextContextDate = function (addDays) {

        var currentContextDate = moment($scope.ContextDate.Date);
        var nextContextDate = currentContextDate.add('days', addDays);

        //Previous Day
        if (addDays < 0) {
            if (nextContextDate.day() === 0)
                nextContextDate = moment(nextContextDate.add("days", -2));
            if (nextContextDate.day() === 6)
                nextContextDate = moment(nextContextDate.add("days", -1));

        } else {
            if (nextContextDate.day() === 6)
                nextContextDate = moment(nextContextDate.add("days", 2));
            if (nextContextDate.day() === 0)
                nextContextDate = moment(nextContextDate.add("days", 1));
        }

        $scope.IsNextDateNotAvailable = false;
        if (nextContextDate.diff(moment(), 'days') >= 0) {
            $scope.IsNextDateNotAvailable = true;
            return;
        }

        $scope.ShouldApplyDatepickerScope = false;
        $("#contextDate").datepicker("setDate", moment(nextContextDate).format("L"));
        $(".dashDateNext").css("padding", "0px " + ($("#contextDate").width() - 5) + "px");
    }

    $scope.fnRetriveWireCounts = function () {

        $http.get("/Home/GetWireStatusCount?contextDate=" + $scope.ContextDate.Date).then(function (response) {
            var wireCounts = response.data;
            $scope.WireStatusCounts = {};
            $scope.WireStatusCounts.Pending = wireCounts.TotalPending;
            $scope.WireStatusCounts.Approved = wireCounts.TotalApproved;
            $scope.WireStatusCounts.Cancelled = wireCounts.TotalCancelled;
            $scope.WireStatusCounts.CancelledAndProcessing = wireCounts.TotalCancelledAndProcessing;
            $scope.WireStatusCounts.Completed = wireCounts.TotalCompleted;
            $scope.WireStatusCounts.Failed = wireCounts.TotalFailed;
            $scope.WireStatusCounts.Total = wireCounts.TotalPending + wireCounts.TotalApproved + wireCounts.TotalCancelled + wireCounts.TotalCompleted + wireCounts.TotalFailed + wireCounts.TotalCancelledAndProcessing;

            $timeout(function () { initiateNumberCounter(); $scope.$apply(); }, 20);

        });
    }

    $scope.fnRetriveAllWireTickets = function (statusId) {
        $scope.fnRetriveWireCounts(),
        $timeout(function () { $scope.$broadcast("loadWireDetailsGrid", statusId, $scope.ContextDate.Date, $scope.ContextDate.Date) });
    }

    $scope.fnRetriveAllWireTickets(0);

});

