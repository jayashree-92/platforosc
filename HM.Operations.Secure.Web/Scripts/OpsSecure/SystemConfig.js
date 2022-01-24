/// <reference path="../data.js" />

$("#liSystemConfig").addClass("active");

HmOpsApp.controller("systemConfiguationCtrl", function ($scope, $http, $interval, $timeout, $q) {
    $http.get("/Configuration/GetSwitchModules").then(function (response) {
        $scope.switchModules = response.data;
    });

    $http.get("/Configuration/GetSwitchList").then(function (response) {
        $scope.switchList = response.data;
    });

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


            $("input[id^='chkSysSwitch-'],input[id^='txtSwitch-'],input[id^='numSwitch-'],input[id^='timeSwitch-'],textarea[id^='tareSwitch-']")
                .off("change").on("change",
                    function () {

                        var value = $(this).attr("type") == "checkbox"
                            ? $(this).prop("checked")
                            : encodeURIComponent($(this).val());

                        $http.post("/Configuration/SetSwitchValue", { key: $(this).attr("data-key"), value: value })
                            .then(
                                function (response) {
                                    notifySuccess("Changes saved successfully");
                                },
                                function (response) {
                                    notifyError("Saving changes failed, please try again");
                                });
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

                            $http.post("/Configuration/SetSwitchValue?key=" +
                                $(this).attr("data-key") +
                                "&value=" +
                                $(this).val()).then(
                                    function (response) {
                                        notifySuccess("Changes saved successfully");
                                    },
                                    function (response) {
                                        notifyError("Saving changes failed, please try again");
                                    });
                        });
                });
            }, 500);
        });
});