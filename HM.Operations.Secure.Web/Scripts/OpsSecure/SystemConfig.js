/// <reference path="../data.js" />

$("#liSystemConfig").addClass("active");

HmOpsApp.controller("systemConfiguationCtrl", function ($scope, $http, $interval, $timeout, $q) {
    $http.get("/Configuration/GetSwitchModules").then(function (response) {
        $scope.switchModules = response.data;
    });

    $http.get("/Configuration/GetSwitchList").then(function (response) {
        $scope.switchList = response.data;
    });

    $scope.fnRefreshClaims = function () {

        $("#btnRefreshClaims").button("loading");

        $http.post("/Configuration/RefreshClaims").then(
            function (response) {
                notifySuccess("Claims refreshed successfully");
                $("#btnRefreshClaims").button("reset");
            }, function (response) {
                notifyError("re-initialization failed");
                $("#btnRefreshClaims").button("reset");
            });
    }
    $scope.reports = [];
    //$http.get("/Files/GetReportsList").then(function(response) {
    //    $scope.reports = response.data;
    //});
    $scope.ItemsInitialized = [];
    $scope.$on("onRepeatLast",
        function (scope, element, attrs) {

            $(".clockpicker").clockpicker();

            $($scope.switchList).each(function (i, v) {
                $(v.switches).each(function (j, w) {
                    if ($("#chkSysSwitch-" + w.key).length > 0)
                        $("#chkSysSwitch-" + w.key).bootstrapToggle();
                });
            });


            $scope.SaveSwitchValue = function (key, value) {
                $http.post("/Configuration/SetSwitchValue", { key: key, value: value })
                    .then(
                        function (response) {
                            notifySuccess("Changes saved successfully");
                        },
                        function (response) {
                            notifyError("Saving changes failed, please try again");
                        });
            }

            $("input[id^='chkSysSwitch-'],input[id^='txtSwitch-'],input[id^='numSwitch-'],input[id^='timeSwitch-'],textarea[id^='tareSwitch-']")
                .off("change").on("change",
                    function () {

                        var value = $(this).attr("type") == "checkbox"
                            ? $(this).prop("checked")
                            : encodeURIComponent($(this).val());
                        $scope.SaveSwitchValue($(this).attr("data-key"), value);
                        
                    });

            $timeout(function () {
                $("input[id^='selSysSwitch-']").each(function (i, v) {

                    if ($(v).length == 0 || $(this).attr("type") != "hidden")
                        return;

                    if ($scope.ItemsInitialized.indexOf($(v).attr("id")) >= 0)
                        return;

                    $(v).off("change");
                    $(v).select2({
                        data: $scope.reports,
                        multiple: true,
                        width: "500px",
                        sortSelection: data => data.sort((a, b) => a.text.localeCompare(b.text)),
                    });

                    $(v).select2("val", [$(v).attr("data-val")]).trigger("change");
                });
            }, 500);


            $timeout(function () {
                $("input[id^='selSysSwitch-']").each(function (i, v) {

                    $(v).off("change").on("change",
                        function (e) {
                            $scope.SaveSwitchValue($(this).attr("data-key"), $(this).val());                            
                        });
                });
            }, 500);
        });
});