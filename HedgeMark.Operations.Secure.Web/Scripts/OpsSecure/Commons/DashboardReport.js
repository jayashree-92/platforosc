/// <reference path="../../data.js" />

HmOpsApp.controller("dashboardReportCtrl", function ($scope, $http, $interval, $timeout, $opsSharedScopes) {
    $opsSharedScopes.store("dashboardReportCtrl", $scope);

    $scope.dateRangeOnChangeCallback = function (start, end) {
        $scope.dateRangeDisplayString = start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY");
        $scope.StartDate = start.format("YYYY-MM-DD");
        $scope.EndDate = end.format("YYYY-MM-DD");
    }

    $("#contextDateRange").daterangepicker({
        "alwaysShowCalendars": true,
        "showDropdowns": true,
        "startDate": moment().startOf("month"),
        "endDate": moment(),
        //"datesDisabled": JSON.parse($("#holidayDateList").val()),
        "autoApply": true,
        "maxDate": moment(),
        ranges: {
            'Today and Yesterday': ContextDatesOfTodayAndTomorrow,
            'Last 7 Days': [moment().subtract(6, "days"), moment()],
            'Last 30 Days': [moment().subtract(29, "days"), moment()],
            'This Month': [moment().startOf("month"), moment()],
            'Last Month': [moment().subtract(1, "month").startOf("month"), moment().subtract(1, "month").endOf("month")],
            'This Year': [moment().startOf("year"), moment()]
        }
    }, function (start, end) {
        $scope.dateRangeOnChangeCallback(start, end);
        $scope.$apply();
    });

    $scope.dateRangeOnChangeCallback(moment().startOf("month"), moment());

    function formatSelect(selectData) {

        if (selectData == null || selectData.text == null)
            return "";

        if (selectData.text.indexOf("All ") == 0) {
            if (selectData.text == "All & Everything" || $scope.PreferenceKeys.indexOf(selectData.text.replace("All ", "")) >= 0)
                return "<b>" + selectData.text + "</b>";
        }
        if (selectData.text.indexOf("(") != -1) {
            var split = selectData.text.split("(");
            var reportName = split[1].replace(")", "");
            selectData.text = split[0] + "&nbsp;&nbsp;<label class='label " + (reportName == "Cash" ? " label-info" : "label-default") + " shadowBox'>" + reportName + "</label>";
        }


        return selectData.text;
    }


    $scope.FavoriteTemplateId = 0;

    $scope.IsAllAndEverythingSelected = false;
    $scope.IsPreferencesChanged = false;
    $scope.IsPreferencesSaved = false;
    $scope.IsNoTemplateSelected = true;
    $scope.IsTemplateLoadingInProgress = false;
    $scope.IsTemplatePreferencePanelCollapsed = false;
    $scope.IsDashboardLoading = false;
    $scope.PreferenceKeys = [];

    $http.get("/DashboardReport/GetAllReportPreferences").then(
        function (response) {
            $scope.AllPreferences = response.data;
            $scope.PreferenceKeys = [];
            $timeout(function () {
                $($scope.AllPreferences).each(function (i, v) {

                    $scope.PreferenceKeys.push(v.Preference);
                    //Add All items - options
                    var allOptions = v.Options;
                    allOptions.splice(0, 0, { id: -1, text: "All " + v.Preference });

                    //initialize Select2
                    $("#li" + v.Preference).select2({
                        data: allOptions, multiple: true, width: "100%", closeOnSelect: false,
                        formatResult: formatSelect,
                        formatSelection: formatSelect
                    });

                    $("#li" + v.Preference).select2("container").find("ul.select2-choices").sortable({
                        containment: "parent",
                        start: function () { $("#li" + v.Preference).select2("onSortStart"); },
                        update: function () { $("#li" + v.Preference).select2("onSortEnd"); }
                    });

                    $("#li" + v.Preference).on("change", function () {
                        var selectedPref = $("#li" + v.Preference).select2("val");
                        v.TotalSelected = selectedPref.length == 0 || selectedPref.indexOf("-1") >= 0 ? "All" : $("#li" + v.Preference).select2("val").length;

                        $scope.IsDashboardLoading = false;
                        if (!$scope.IsTemplateLoadingInProgress && !$scope.IsNoTemplateSelected) {
                            $scope.IsPreferencesChanged = true;
                        }

                        $scope.fnUpdatePrefernceChoices(v.Preference, selectedPref);
                    });
                });

                //Load Templates
                $scope.fnGetAllTemplates();

            }, 200);
        });

    $scope.fnUpdatePrefernceChoices = function (preference, selectedPref) {

        if (selectedPref.length === 0)
            return;

        var url = "";

        if (preference === "Clients") {
            url = "/WiresDashboard/GetFundDetails?clientIds=" + selectedPref;
        } else if (preference === "Funds") {
            url = "/WiresDashboard/GetAgreementTypes?fundIds=" + selectedPref;
        }

        if (url === "")
            return;

        //Update Counterparties and agreement Types
        $http.get(url).then(function (response) {
            $(response.data).each(function (i, v) {
                $scope.PreferenceKeys.push(v.Preference);
                //Add All items - options
                var allOptions = v.Options;
                allOptions.splice(0, 0, { id: -1, text: "All " + v.Preference });

                //initialize Select2
                $("#li" + v.Preference).select2({
                    data: allOptions, multiple: true, width: "100%", closeOnSelect: false,
                    formatResult: formatSelect,
                    formatSelection: formatSelect
                });

            });
        });
    }

    $scope.fnGetAllTemplates = function (defaultTemplateId) {
        $http.get("/DashboardReport/GetAllTemplates").then(
            function (response) {
                $scope.FavoriteTemplateId = response.data.favoriteId;

                var allTemplates = response.data.templates;
                $("#liDashboardTemplates").select2({
                    placeholder: "Select Template", data: allTemplates, allowClear: true,
                    formatResult: formatSelect,
                    formatSelection: formatSelect
                });

                if (defaultTemplateId == undefined || defaultTemplateId == 0)
                    defaultTemplateId = $scope.FavoriteTemplateId;

                if (defaultTemplateId != undefined && defaultTemplateId > 0) {

                    if ($("#liDashboardTemplates").val() != defaultTemplateId)
                        $("#liDashboardTemplates").select2("val", defaultTemplateId).trigger("change");
                }
            });
    }

    $scope.fnSetFavoriteTemplate = function () {
        if ($scope.SelectedTemplateId == undefined || $scope.SelectedTemplateId == "") {
            notifyWarning("Please select a valid template to set as Favorite");
            return;
        }

        var newTemplateId = $scope.FavoriteTemplateId !== $scope.SelectedTemplateId ? $scope.SelectedTemplateId : 0;
        $http.post("/DashboardReport/SaveFavoriteTemplate?templateId=" + newTemplateId).then(function (response) {
            $scope.FavoriteTemplateId = newTemplateId;
            notifySuccess("Changes saved successfully.");
        });
    }

    $("#liDashboardTemplates").on("change", function (e) {

        $("#pnlTemplateSelection").collapse("hide");
        $("#pnlDashboardPreferences").collapse("show");
        $scope.IsPreferencesChanged = false;
        $scope.IsDashboardLoading = false;
        $scope.IsAllAndEverythingSelected = false;
        $scope.TemplateName = "";
        $scope.SelectedTemplateId = $("#liDashboardTemplates").val();

        if ($scope.SelectedTemplateId == undefined || $scope.SelectedTemplateId == "") {
            $scope.IsNoTemplateSelected = true;
            $($scope.AllPreferences).each(function (i, v) {
                $("#li" + v.Preference).select2("val", "").trigger("change");
            });
            $scope.$evalAsync();
            return;
        }

        $scope.IsTemplateLoadingInProgress = true;
        $scope.IsNoTemplateSelected = false;
        $scope.TemplateName = $("#liDashboardTemplates").select2("data").text;
        $scope.IsAllAndEverythingSelected = $scope.TemplateName === "All & Everything";

        //Get the preferences of the template
        $http.get("/DashboardReport/GetPreferences?templateId=" + $scope.SelectedTemplateId).then(
            function (response) {
                $timeout(function () {
                    $scope.IsTemplateLoadingInProgress = true;
                    $(response.data).each(function (i, v) {
                        $("#li" + v.Preference).select2("val", v.SelectedIds).trigger("change");
                    });
                    $scope.IsTemplateLoadingInProgress = false;
                }, 200);
            });

        //Get all Schedules associated to selected template
        $opsSharedScopes.get("ReportScheduleCtrl").fnGetSchedules($scope.SelectedTemplateId, true);
    });

    $("#pnlDashboardPreferences").collapse().on("hide.bs.collapse", function () {
        $scope.IsTemplatePreferencePanelCollapsed = true;
    }).on("show.bs.collapse", function () {
        $scope.IsTemplatePreferencePanelCollapsed = false;
    });

    $scope.fnGetActivePreferences = function () {
        var preferences = [];

        $($scope.AllPreferences).each(function (i, key) {
            var pref = $("#li" + key.Preference).val();
            preferences.push({ Key: key.Preference, Value: pref == "" || pref.indexOf("-1") >= 0 ? -1 : pref });
        });

        return preferences;
    }

    $scope.fnOpenSaveTemplateModal = function () {
        var isAnyPreferencesAvailable = false;
        $($scope.AllPreferences).each(function (i, key) {
            var prefStat = $("#li" + key.Preference).val();

            if (prefStat != "")
                isAnyPreferencesAvailable = true;

        });

        if (!isAnyPreferencesAvailable) {
            notifyWarning("Please select valid preferences before proceeding");
            return;
        }

        $("#mdlSaveTemplate").modal("show").on("hidden.bs.modal", function () {
            $scope.IsRenameTemplate = false;
            $scope.IsSaveAsNew = false;

        });
    }

    $scope.IsRenameTemplate = false;
    $scope.IsSaveAsNew = false;

    $scope.fnEditTemplateName = function () {
        $scope.IsRenameTemplate = true;
        $scope.IsSaveAsNew = false;
        $scope.TemplateName = $("#liDashboardTemplates").select2("data").text;
        $scope.fnOpenSaveTemplateModal();
    }

    $scope.fnSaveAsNewTemplate = function () {
        $scope.IsRenameTemplate = false;
        $scope.IsSaveAsNew = true;
        $scope.TemplateName = "";
        $scope.fnOpenSaveTemplateModal();
    }

    $scope.fnSaveTemplateAndPreferences = function (saveOriginal) {

        if ($("#liDashboardTemplates").val() == "" && saveOriginal) {
            $scope.fnOpenSaveTemplateModal();
            return;
        }

        if ($scope.TemplateName == "" || $scope.TemplateName == undefined) {
            notifyWarning("Please enter valid template name before proceeding");
            return;
        }

        var templateId = 0;

        if (saveOriginal) {
            $scope.TemplateName = $("#liDashboardTemplates").select2("data").text;
            templateId = $("#liDashboardTemplates").val();
        }

        if ($scope.IsSaveAsNew)
            templateId = 0;

        if ($scope.IsRenameTemplate)
            templateId = $("#liDashboardTemplates").val();

        if (templateId == "")
            templateId = 0;

        $http.post("/DashboardReport/SaveTemplateAndPreferences", { templateName: $scope.TemplateName, templateId: templateId, preferences: $scope.fnGetActivePreferences() }).then(function (response) {
            $("#mdlSaveTemplate").modal("hide");
            $scope.IsPreferencesChanged = false;
            $scope.IsPreferencesSaved = true;
            $scope.fnGetAllTemplates(response.data);
            $scope.IsRenameTemplate = false;
            $scope.IsSaveAsNew = false;
            notifySuccess("Template preferences saved successfully.");
            $timeout(function () { $scope.IsPreferencesSaved = false; }, 5000);
        });
    }

    $scope.fnDeleteTemplate = function () {
        bootbox.confirm({
            message: "Are you sure you want to delete template <b>'" + $("#liDashboardTemplates").select2("data").text + "'</b> ?",
            buttons: {
                confirm: {
                    label: "Delete Template",
                    className: "btn-sm btn-danger"
                },
                cancel: {
                    label: "Dismiss",
                    className: "btn-sm btn-default"
                }
            },
            callback: function (result) {
                if (result) {
                    $http.post("/DashboardReport/DeleteTemplate", { templateId: $("#liDashboardTemplates").val() }).then(function (response) {
                        notifySuccess("Template deleted successfully.");
                        $scope.fnGetAllTemplates();
                        $("#liDashboardTemplates").select2("val", "").trigger("change");

                    });
                }
            }
        });
    }

    $scope.fnSelectAll = function (preference) {
        $("#li" + preference).select2("val", -1).trigger("change");
    }
    $scope.fnDeSelectAll = function (preference) {
        $("#li" + preference).select2("val", "").trigger("change");
    }

    $scope.fnSetTotalSelected = function () {
        $($scope.AllPreferences).each(function (i, v) {
            var selectedPref = $("#li" + v.Preference).select2("val");
            v.TotalSelected = selectedPref.length == 0 || selectedPref.indexOf("-1") >= 0 ? "All" : $("#li" + v.Preference).select2("val").length;
        });
    }

    $scope.fnGetLoadParameters = function () {

        $("#pnlTemplateSelection").collapse("show");
        $("#pnlDashboardPreferences").collapse("hide");
        $("#pnlDashboardReports").collapse("show");
        $scope.fnSetTotalSelected();
        $scope.IsDashboardLoading = true;

        return $scope.fnGetActivePreferences();
    }


    $("#pnlDashboardPreferences").collapse("show");
    $("#pnlDashboardReports").collapse("hide");

    //********************************************//

    //**//User needs to Create following Functions to incorporate Compartmentalization of dashboard.

    //$scope.fnLoadDashboard = function (startDate, endDate, preferences) {}
    //$scope.fnExportDashboardReport = function (startDate, endDate, format) {}

    //********************************************//

});