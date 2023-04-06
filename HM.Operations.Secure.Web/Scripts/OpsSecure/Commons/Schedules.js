/// <reference path="../../data.js" />

var tblReportSchedules, tblScheduleLogs;
HmOpsApp.controller("ReportScheduleCtrl", function ($scope, $http, $timeout, $q, $interval, $opsSharedScopes, $filter) {
    $opsSharedScopes.store("ReportScheduleCtrl", $scope);

    $scope.TimeZones = ["EST", "IST", "CET", "PST"];

    $scope.fnGetDefaultTimeZone = function () {

        var region = getTimeZoneAbbr();
        if ($scope.TimeZones.indexOf(region) >= 0)
            return region;

        return $scope.TimeZones[0];
    }

    $scope.JobDefaults = {
        Schedule: {
            Frequency: "Daily",
            TimeZone: $scope.fnGetDefaultTimeZone(),
            FileFormat: ".xlsx",
            To: "",
            CC: "",
            ExternalTo: "",
            ReportFileName: "",
            SFTPFolder: "",
            InternalFolder: "",
            ExternalToWorkflowCode: 0,
            SFTPFolderWorkflowCode: 0,
            IsActive: true,
            PreferredFundNameCode: 0
        },
        Due: {
            DueTime: "08:00",
            DueTimeStamp: "08:00",
            DueDaysOfWeek: [1],
            DueDayOfMonth: 1,
            MonthlyNthDay: "first",
            MonthlyNthDayOfWeek: "weekday"
        },
        ScheduleRangeLkupId: 1,
        ScheduleContextRunkupId: 1,
        IsExternalToRequestCreatedBySameUser: true,
        IsSFTPFolderRequestCreatedBySameUser: true,
        DeadlineTimeString: "00:00:01"
    };

    $scope.ScheduleDefaults = {};
    $timeout(function () {
        $http.get("/Schedules/GetScheduleDefaults").then(function (response) {
            $scope.ScheduleDefaults = response.data;
        });
    }, 100);

    $scope.fnInitializeJQueryEvents = function () {

        //$scope.TimeZones = $scope.ScheduleDefaults.timeZones;

        $("#inptJobScheduleAt").clockpicker({
            donetext: "Ok", placement: "right", align: "right", default: "now",//twelvehour: true,
            autoclose: true
        });

        //$('#inptJobDeadlineAt').timepicker();

        //$("#inptJobDeadlineAt").clockpicker({
        //    donetext: "Ok", placement: "right", align: "right", default: "now",//twelvehour: true,
        //    autoclose: true
        ////});
        //$("#inptJobDeadlineAt").clockpicker({
        //    donetext: "Ok",
        //    placement: "top",
        //    align: "top",
        //    container: 'body',
        //    twelvehour: true,
        //    afterDone: function() {
        //        $("#inptJobDeadlineAt").val( $("#inptJobDeadlineAt").val().slice(0, -2) + ' ' +  $("#inptJobDeadlineAt").val().slice(-2));
        //    },
        //    init: function() { 
        //        console.log("colorpicker initiated");
        //    },
        //    beforeShow: function() {
        //        console.log("before show");
        //    },
        //    afterShow: function() {
        //        console.log("after show");
        //    },
        //    beforeHide: function() {
        //        console.log("before hide");
        //    },
        //    afterHide: function() {
        //        console.log("after hide");
        //    },
        //    beforeHourSelect: function() {
        //        console.log("before hour selected");
        //    },
        //    afterHourSelect: function() {
        //        console.log("after hour selected");
        //    },
        //    beforeDone: function() {
        //        console.log("before done");
        //    },
        //    afterDone: function() {
        //        console.log("after done");
        //    }
        //});

        //$("#inptJobDeadlineAt").on("focus", function () {
        //    $("#inptJobDeadlineAt").clockpicker("show");
        //});

        //$("#inptJobDeadlineAt").on("focusout", function () {
        //    $("#inptJobDeadlineAt").clockpicker("hide");
        //});


        $("#inptJobDeadlineAt").select2({ width: "250px" });
        $("#inptJobDeadlineAt").off("change").on("change", function () {

            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.Job.DeadlineTimeString = $("#inptJobDeadlineAt").val();
        });

        $("#liScheduleTimeZone").off("change").on("change", function () {
            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.Job.Schedule.TimeZone = $("#liScheduleTimeZone").val();
        });

        $("#liScheduleFileFormat").select2({ width: "250px" });
        $("#liScheduleFileFormat").off("change").on("change", function () {
            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.Job.Schedule.FileFormat = $("#liScheduleFileFormat").val();
        });

        $("#liScheduleRange").select2({ data: $scope.ScheduleDefaults.dashboardRange, width: "250px" });
        $("#liScheduleRange").off("change").on("change", function () {
            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.Job.ScheduleRangeLkupId = $("#liScheduleRange").val();
        });

        $("#liFundPreferences").select2({ data: $scope.ScheduleDefaults.fundPrefernces, width: "250px" });
        $("#liFundPreferences").off("change").on("change", function () {
            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.Job.Schedule.PreferredFundNameCode = $("#liFundPreferences").val();
        });

        $("#liReportContextRun").select2({ data: $scope.ScheduleDefaults.reportContextRun, width: "250px" });
        $("#liReportContextRun").off("change").on("change", function () {
            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.Job.ScheduleContextRunkupId = $("#liReportContextRun").val();
        });

        $("#liScheduleSFTPFolders").select2({ data: $scope.ScheduleDefaults.sftpFolders, allowClear: true, placeholder: "Select SFTP folder", width: "250px" });
        $("#liScheduleSFTPFolders").off("change").on("change", function (e) {
            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.Job.Schedule.SFTPFolder = $("#liScheduleSFTPFolders").val();
            $scope.IsSFTPFolderIsNotBlank = $("#liScheduleSFTPFolders").val() !== "";
            $scope.Job.IsSFTPFolderRequestCreatedBySameUser = true;
            $scope.IsSFTPFolderHasChanged = false;

            if ($("#liScheduleSFTPFolders").val() !== "" && $scope.Job.Schedule.SFTPFolder === $scope.OriginalSFTFolderValue) {
                $scope.Job.Schedule.SFTPFolderWorkflowCode = $scope.OriginalSFTFolderWorkflowCode;
                $scope.Job.IsSFTPFolderRequestCreatedBySameUser = $scope.ScheduleDefaults.UserId === $scope.Job.Schedule.SFTPFolderModifiedBy;
                return;
            }

            if ($scope.Job.Schedule.SFTPFolder !== $scope.OriginalSFTFolderValue) {
                $scope.IsSFTPFolderHasChanged = true;
                $scope.Job.Schedule.SFTPFolderWorkflowCode = 0;
            }
        });

        $("#liScheduleInternalFolders").select2({ data: $scope.ScheduleDefaults.internalFolders, allowClear: true, placeholder: "Select Internal folder", width: "250px" });
        $("#liScheduleInternalFolders").off("change").on("change", function (e) {
            if ($scope.IsScheduleLoadingInProgress)
                return;

            $scope.IsInternalFolderIsNotBlank = $("#liScheduleInternalFolders").val() !== "";
            $scope.Job.Schedule.InternalFolder = $("#liScheduleInternalFolders").val();
        });

        $("#spnScheduleAdvanceSetting").on("click", function () {
            if ($("#pnlSchedulAdvanceSettings").hasClass("collapse"))
                $("#pnlSchedulAdvanceSettings").collapse("toggle");
        });

    }

    $scope.fnResetWeeklyDaysOfUi = function () {
        var days = $scope.Job.Due.DueDaysOfWeek == null ? "" : $scope.Job.Due.DueDaysOfWeek.toString().split(",");
        $("#chkWeeklyRepeatDays .btn").removeClass("active");
        $("#chkWeeklyRepeatDays input").prop("checked", false);
        $.each(days, function (i, v) {
            $("#chkWeeklyRepeatDays label:eq(" + (v - 1) + ")").addClass("active");
            $("#chkWeeklyRepeatDays input:eq(" + (v - 1) + ")").prop("checked", true);
        });
    }

    $scope.fnSetJobParameters = function () {

        $scope.TemplateName = $scope.IsDashboardSchedule ? $("#liDashboardTemplates").select2("data").text : $("#fundOrGroup").select2("data").text;;
        $scope.ReportName = $("#hdnReportName").val();

        if ($scope.Job.Schedule.To == null)
            $scope.Job.Schedule.To = "";
        if ($scope.Job.Schedule.CC == null)
            $scope.Job.Schedule.CC = "";
        if ($scope.Job.Schedule.ExternalTo == null)
            $scope.Job.Schedule.ExternalTo = "";

        $("#dvInptScheduleInternalTo").html($scope.Job.Schedule.To.trim());
        $("#dvInptScheduleInternalCC").html($scope.Job.Schedule.CC.trim());
        $("#dvInptScheduleExternalTo").html($scope.Job.Schedule.ExternalTo.trim());

        $scope.fnToggleRecurringEventFrequency($scope.Job.Schedule.Frequency);
        $scope.Job.Due.DueTime = $scope.Job.Due.DueTimeStamp;
        //$scope.Job.DeadlineTimeString = moment($scope.Job.Schedule.WaitPeriodForFiles).format("hh:mm");

        //Set Job Frequeny toggle
        $("#rdJobFrequency .btn").removeClass("active");
        $("#rdJobFrequency .btn:eq(" + ($scope.Job.Schedule.Frequency === "Daily" ? 0 : $scope.Job.Schedule.Frequency === "Weekly" ? 1 : 2) + ")").addClass("active");

        if ($scope.Job.Due.DueDaysOfWeek == null)
            $scope.Job.Due.DueDaysOfWeek = [1];

        $scope.fnResetWeeklyDaysOfUi();

        $("#liScheduleTimeZone").val($scope.Job.Schedule.TimeZone).trigger("change");
        $("#liScheduleFileFormat").select2("val", $scope.Job.Schedule.FileFormat).trigger("change");
        $("#inptJobDeadlineAt").select2("val", $scope.Job.DeadlineTimeString).trigger("change");
        $("#liScheduleRange").select2("val", $scope.Job.ScheduleRangeLkupId).trigger("change");
        $("#liReportContextRun").select2("val", $scope.Job.ScheduleContextRunkupId).trigger("change");


        if (($scope.Job.Schedule.SFTPFolder != null && $scope.Job.Schedule.SFTPFolder !== "") ||
            ($scope.Job.Schedule.InternalFolder !== null && $scope.Job.Schedule.InternalFolder !== "") ||
            $scope.Job.Schedule.ReportFileName !== null && $scope.Job.Schedule.ReportFileName !== "" ||
            $scope.Job.Schedule.PreferredFundNameCode !== 0 ||
            !$scope.IsDashboardSchedule && $scope.Job.ScheduleContextRunkupId !== "1") {

            $("#pnlSchedulAdvanceSettings").removeClass("collapse");
            $("#spnScheduleAdvanceSetting").removeClass("hmo-AdvanceSettings-plus").addClass("hmo-AdvanceSettings-minus");
            $("#liScheduleSFTPFolders").select2("val", $scope.Job.Schedule.SFTPFolder).trigger("change");
            $("#liScheduleInternalFolders").select2("val", $scope.Job.Schedule.InternalFolder).trigger("change");
            $("#liFundPreferences").select2("val", $scope.Job.Schedule.PreferredFundNameCode).trigger("change");

        } else {
            $("#pnlSchedulAdvanceSettings").addClass("collapse");
            $("#pnlSchedulAdvanceSettings").collapse("hide");
        }

    }

    $scope.IsAddNewJobClicked = true;
    $("#mdlToShowSchedulesConfig").on("shown.bs.modal", function () {

        if ($scope.IsAddNewJobClicked) {
            $scope.Job = angular.copy($scope.JobDefaults);
        }

        if ($scope.IsScheduleLoadingInProgress) {
        }
       
        $timeout(function () {
            $scope.IsAllClientAllAdminSelected = $scope.IsAllorMultipleClientSelected;//$scope.IsAllClientSelected && $scope.IsAllAdminSelected;
            if ($scope.IsAllClientAllAdminSelected) {
                $("#dvInptScheduleExternalTo").removeClass("schedule-email-box").addClass("schedule-email-box-disabled");
                $("#dvInptScheduleExternalTo").attr("contenteditable", false);
            }
            else
                $("#dvInptScheduleExternalTo").addClass("schedule-email-box").removeClass("schedule-email-box-disabled");
        }, 100);
        
       
        $scope.OriginalExternalToValue = $scope.Job.Schedule.ExternalTo;
        $scope.OriginalSFTFolderValue = $scope.Job.Schedule.SFTPFolder;

        $scope.OriginalExternalToWorkflowCode = $scope.Job.Schedule.ExternalToWorkflowCode;
        $scope.OriginalSFTFolderWorkflowCode = $scope.Job.Schedule.SFTPFolderWorkflowCode;

        $scope.Job.IsExternalToRequestCreatedBySameUser = $scope.ScheduleDefaults.UserId === $scope.Job.Schedule.ExternalToModifiedBy;
        $scope.Job.IsSFTPFolderRequestCreatedBySameUser = $scope.ScheduleDefaults.UserId === $scope.Job.Schedule.SFTPFolderModifiedBy;

        $scope.fnInitializeJQueryEvents();
        $scope.fnSetJobParameters();

        $scope.IsInternalToNotBlank = $("#dvInptScheduleInternalTo").text().trim().length > 0;
        $scope.IsExternalToNotBlank = $("#dvInptScheduleExternalTo").text().trim().length > 0;
        
        $scope.IsInternalFolderIsNotBlank = $scope.Job.Schedule.InternalFolder != null;
        $scope.IsSFTPFolderIsNotBlank = $scope.Job.Schedule.SFTPFolder != null;

        //Check if there is a delta difference  
        $scope.fnIsNewExternalToApprovalsAvailable();

        $("#dvInptScheduleInternalTo").AutoCompleteEmail({
            domains: ["innocap.com"],
            excludedEmails: ["reconfiles"],
            bAllowListedDomainsOnly: true,
            onFocusOutCallback: function () {
                $scope.IsInternalToHasValidIds = !$("#dvInptScheduleInternalTo").hasClass("spn-auto-email-error");
                $scope.IsInternalToNotBlank = $("#dvInptScheduleInternalTo").text().trim().length > 0;
                if ($scope.IsInternalToHasValidIds)
                    $scope.Job.Schedule.To = $("#dvInptScheduleInternalTo").AutoCompleteEmail("getEmails");
                $scope.$apply();
            }
        });

        $("#dvInptScheduleInternalCC").AutoCompleteEmail({
            domains: ["innocap.com"],
            excludedEmails: ["reconfiles"],
            bAllowListedDomainsOnly: true,
            onFocusOutCallback: function () {
                $scope.IsInternalCCHasValidIds = $("#dvInptScheduleInternalCC").text().trim() === "" || !$("#dvInptScheduleInternalCC").hasClass("spn-auto-email-error");
                if ($scope.IsInternalCCHasValidIds)
                    $scope.Job.Schedule.CC = $("#dvInptScheduleInternalCC").AutoCompleteEmail("getEmails");
                $scope.$apply();
            }
        });


        $("#dvInptScheduleExternalTo").AutoCompleteEmail({
            domains: $scope.ScheduleDefaults.externalDomains,
            sHighlightList: $scope.Job.Schedule.ExternalToApproved,
            bAllowListedDomainsOnly: true,
            onFocusInCallback: function () {
                $scope.IsExternalToEditingInProgress = true;
            },
            onFocusOutCallback: function () {
                $scope.IsExternalToEditingInProgress = false;
                $scope.IsExternalToHasValidIds = $("#dvInptScheduleExternalTo").text().trim() === "" || !$("#dvInptScheduleExternalTo").hasClass("spn-auto-email-error");
                $scope.IsExternalToHasChanged = $("#dvInptScheduleExternalTo").hasClass("spn-auto-email-warning");
                $scope.Job.IsExternalToRequestCreatedBySameUser = $scope.ScheduleDefaults.UserId === $scope.Job.Schedule.ExternalToModifiedBy;

                if (!$scope.IsExternalToHasChanged && $scope.OriginalExternalToValue == $("#dvInptScheduleExternalTo").text() && $("#dvInptScheduleExternalTo").text() != "") {
                    $scope.Job.Schedule.ExternalToWorkflowCode = $scope.OriginalExternalToWorkflowCode;
                    return;
                }

                $scope.IsExternalToNotBlank = $("#dvInptScheduleExternalTo").text().trim().length > 0;

                if (!$scope.IsExternalToHasChanged && $scope.OriginalExternalToValue == $("#dvInptScheduleExternalTo").text() && $("#dvInptScheduleExternalTo").text() == "") {
                    $scope.Job.IsExternalToRequestCreatedBySameUser = true;
                    $scope.IsExternalToHasValidIds = false;
                }

                if ($scope.IsExternalToHasChanged && $scope.IsExternalToHasValidIds) {
                    $scope.Job.Schedule.ExternalTo = $("#dvInptScheduleExternalTo").AutoCompleteEmail("getEmails");
                    $scope.Job.Schedule.ExternalToWorkflowCode = 0;
                    $scope.Job.IsExternalToRequestCreatedBySameUser = true;
                }

                //Check if there is a delta difference  
                $scope.fnIsNewExternalToApprovalsAvailable();

                $scope.$apply();
            }
        });

        $("#dvInptScheduleExternalToApproved").html($(this).AutoCompleteEmail("formatEmails", [$scope.Job.Schedule.ExternalToApproved, $scope.Job.Schedule.ExternalToApproved]));


        //Reset parameter

        $timeout(function () {
            $scope.IsScheduleLoadingInProgress = false;
        }, 200);


    }).on("hidden.bs.modal", function () {
        $("#pnlSchedulAdvanceSettings").collapse("hide");
        $scope.IsScheduleLoadingInProgress = false;
        $scope.IsAddNewJobClicked = true;
    });


    $scope.fnIsNewExternalToApprovalsAvailable = function () {
        $scope.NewChangesToExternalToExisits = false;
        if ($scope.Job.Schedule.ExternalToApproved == null || $scope.Job.Schedule.ExternalToApproved.length <= 0)
            return;

        var newExternalTo = $scope.Job.Schedule.ExternalTo == null ? [] : $scope.Job.Schedule.ExternalTo.split(/[,;\n\t\r ]+/);
        var externalToApproved = $scope.Job.Schedule.ExternalToApproved == null ? [] : $scope.Job.Schedule.ExternalToApproved.split(/[,;\n\t\r ]+/);
        $(externalToApproved).each(function (i, v) {
            if (newExternalTo.indexOf(v) == -1) {
                $scope.NewChangesToExternalToExisits = true;
            }
        });
    }


    $scope.fnToggleRecurringEventFrequency = function (frequency) {
        $scope.Job.Schedule.Frequency = frequency;
        if (frequency == "Monthly") {
            $("#txtRecurringEventMonthlyDayCount").val(1);
        }
    }

    $scope.fnToggleMonthlyNthDayOption = function (isNthDaySelected) {
        $scope.Job.Due.IsMonthlyNthDaySelected = isNthDaySelected;

        if (isNthDaySelected && $scope.Job.MonthlyNthDay == "" || $scope.Job.MonthlyNthDayOfWeek == null) {
            $scope.Job.Due.MonthlyNthDay = "first";
            $scope.Job.Due.MonthlyNthDayOfWeek = "weekday";
        }

        if (!isNthDaySelected)
            $scope.Job.Due.DueDayOfMonth = 1;
    }

    //$scope.fnToggleIsOnlyOnWeekDays = function () {
    //    var $isOnlyOnWeekDay = $("#btnIsOnlyOnWeekDays");
    //    $isOnlyOnWeekDay.toggleClass("on btn-info").toggleClass("btn-default");
    //    $isOnlyOnWeekDay.find("i").toggleClass("glyphicon-check").toggleClass("glyphicon-unchecked");
    //    $scope.Job.IsOnlyOnWeekDay = $isOnlyOnWeekDay.hasClass("on");
    //}


    $scope.fnToggleWeeklyRepeatDays = function (index, $event) {

        if ($scope.Job.Due.DueDaysOfWeek == null)
            $scope.Job.Due.DueDaysOfWeek = [1];

        if (!$($event.target).hasClass("active"))
            $scope.Job.Due.DueDaysOfWeek.push(index);
        else
            $scope.Job.Due.DueDaysOfWeek = $scope.Job.Due.DueDaysOfWeek.filter(function (ele) { return ele !== index; });

        if ($scope.Job.Due.DueDaysOfWeek.length > 1)
            $scope.Job.Due.DueDaysOfWeek = $scope.Job.Due.DueDaysOfWeek.sort();
    }


    $scope.fnSetScheduleStatus = function (jobId, scheduleId, isActive) {
        $http.get("/Schedules/SetScheduleStatus?jobId=" + jobId + "&scheduleId=" + scheduleId + "&isActive=" + isActive + "&isDashboard=" + $scope.IsDashboardSchedule).then(function (response) {
            $scope.fnGetSchedules($scope.SelectedPrimaryId, $scope.IsDashboardSchedule);
        });
    }

    $scope.fnDeleteSchedule = function (jobId) {
        $http.get("/Schedules/DeleteSchedule?jobId=" + jobId + "&isDashboard=" + $scope.IsDashboardSchedule).then(function (response) {
            notifySuccess("Schedule deleted successfully");
            $scope.fnGetSchedules($scope.SelectedPrimaryId, $scope.IsDashboardSchedule);
        });
    }

    $scope.fnTriggerSchedule = function (jobId, $btnTrigger) {
        $http.get("/Schedules/TriggerNow?jobId=" + jobId + "&contextDate=" + $("#contextDate").text() + "&isDashboard=" + $scope.IsDashboardSchedule).then(function (response) {
            notifySuccess("Schedule trigged successfully");
            $timeout(function () { $btnTrigger.button("reset"); }, 1000);
            $scope.fnGetSchedules($scope.SelectedPrimaryId, $scope.IsDashboardSchedule);
        });
    }

    $scope.fnApproveOrRejectExternalTo = function (code) {
        $http.get("/Schedules/SetWorkflowCodeForExternalTo?scheduleId=" + $scope.Job.Schedule.hmsScheduleId + "&workflowCode=" + code + "&isDashboard=" + $scope.IsDashboardSchedule).then(function (response) {
            $scope.Job.Schedule.ExternalToWorkflowCode = response.data.Schedule.ExternalToWorkflowCode;
            $scope.Job.Schedule.ExternalToModifiedBy = response.data.Schedule.ExternalToModifiedBy;
            $scope.Job.Schedule.ExternalToModifiedAt = moment(response.data.Schedule.ExternalToModifiedAt).format("lll");
            $scope.Job.ExternalToModifiedBy = response.data.ExternalToModifiedBy;

            if (code === 1) {
                $scope.Job.Schedule.ExternalToApproved = $scope.Job.Schedule.ExternalTo;

                //Check if there is a delta difference  
                $scope.fnIsNewExternalToApprovalsAvailable();
            }

            $scope.Job.ExternalToModifiedBy = response.data.ExternalToModifiedBy;
            $scope.fnGetSchedules($scope.SelectedPrimaryId, $scope.IsDashboardSchedule);
        });
    }

    $scope.fnApproveOrRejectSFTPFolder = function (code) {
        $http.get("/Schedules/SetWorkflowCodeForSFTPFolder?scheduleId=" + $scope.Job.Schedule.hmsScheduleId + "&workflowCode=" + code + "&isDashboard=" + $scope.IsDashboardSchedule).then(function (response) {
            $scope.Job.Schedule.SFTPFolderWorkflowCode = response.data.Schedule.SFTPFolderWorkflowCode;
            $scope.Job.Schedule.SFTPFolderModifiedBy = response.data.Schedule.SFTPFolderModifiedBy;
            $scope.Job.Schedule.SFTPFolderModifiedAt = moment(response.data.Schedule.SFTPFolderModifiedAt).format("lll");
            $scope.Job.SFTPFolderModifiedBy = response.data.SFTPFolderModifiedBy;
            $scope.fnGetSchedules($scope.SelectedPrimaryId, $scope.IsDashboardSchedule);
        });
    }

    $scope.fnSaveSchedule = function () {
        $http.post("/Schedules/SaveSchedule", { job: $scope.Job, primaryId: $scope.SelectedPrimaryId, isDashboard: $scope.IsDashboardSchedule }).then(function () {
            notifySuccess("Changes saved successfully");
            $("#mdlToShowSchedulesConfig").modal("hide");
            $scope.fnGetSchedules($scope.SelectedPrimaryId, $scope.IsDashboardSchedule);
        });
    }


    $scope.IsScheduleLoadingInProgress = false;

    $scope.IsInternalToHasValidIds = true;
    $scope.IsInternalToNotBlank = false;
    $scope.IsExternalToNotBlank = false;
    $scope.IsInternalCCHasValidIds = true;
    $scope.IsExternalToHasValidIds = true;
    $scope.IsExternalToHasChanged = false;
    $scope.IsSFTPFolderHasChanged = false;
    $scope.IsSFTPFolderIsNotBlank = false;
    $scope.IsInternalFolderIsNotBlank = false;

    $scope.SelectedPrimaryId = 0;
    $scope.IsDashboardSchedule = false;

    $scope.fnGetSchedules = function (primaryId, isDashboard) {
        $scope.SelectedPrimaryId = primaryId;
        $scope.IsDashboardSchedule = isDashboard;

        $http.get("/Schedules/GetSchedules?primaryId=" + primaryId + "&isDashboard=" + isDashboard).then(function (response) {

            //Get all Schedules associated to selected template
            if (isDashboard)
                $opsSharedScopes.get("dashboardReportCtrl").TotalSchedules = response.data.length;
            else
                $opsSharedScopes.get("getDetailsCtrl").TotalSchedules = response.data.length;

            if (response.data.length === 0) {
                $("#panelReportSchedules").collapse("hide");
            }

            if ($("#tblReportSchedules").hasClass("initialized")) {
                tblReportSchedules.clear();
                tblReportSchedules.rows.add(response.data);
                tblReportSchedules.draw();
            } else {
                tblReportSchedules = $("#tblReportSchedules").not(".initialized").addClass("initialized").DataTable(
                    {
                        aaData: response.data,
                        rowId: "Id",
                        "dom": "trI",
                        "bDestroy": true,
                        "createdRow": function (row, data, index) {
                            $(row).css("cursor", "pointer");
                        },
                        "columns": [
                            {
                                "mData": "Due.CronExpression", "sTitle": "Schedule",
                                "mRender": function (tdata, type, row) {
                                    return "<span data-cron='" + row.Due.CronExpression + "' style='cursor:pointer;' title='" + row.Due.CronDescription + "'>" + GetReadableCron(tdata, row.Schedule.Frequency, row.Due.CronDescription, 0) + "</span>";
                                }
                            },
                            { "mData": "Schedule.Frequency", "sTitle": "Frequency" },
                            { "mData": "Schedule.TimeZone", "sTitle": "Time Zone" },
                            {
                                "mData": "Schedule.To", class: "seeFullTextWithNoBreak", "sTitle": "Emails Addressed", "mRender": function (tdata, type, row) {

                                    var allInternalList = ((row.Schedule.To == null ? "" : row.Schedule.To) + ";" + (row.Schedule.CC == null ? "" : row.Schedule.CC) + ";").split(/[,;\n\t\r]+/);
                                    var externalEmailList = (row.Schedule.ExternalTo == null ? "" : row.Schedule.ExternalTo).split(/[,;\n\t\r]+/);

                                    var filteredInternal = allInternalList.filter(function (el) { return el != null && el.trim() !== ""; });
                                    var filteredExternals = externalEmailList.filter(function (el) { return el != null && el.trim() !== ""; });

                                    var totalEmails = filteredInternal.length + filteredExternals.length;

                                    var customString = "";
                                    if (filteredInternal.length > 1) {
                                        customString = filteredInternal[0] + ";" + filteredInternal[1] + ";";
                                    } else {
                                        if (filteredInternal.length > 0)
                                            customString += filteredInternal[0] + ";";
                                        if (filteredExternals.length > 0)
                                            customString += filteredExternals[0] + ";";
                                        if (filteredInternal.length == 0 && filteredExternals.length > 1)
                                            customString += filteredExternals[1] + ";";
                                    }

                                    var filteredExternalStr = "<b><i>" + filteredExternals.join("</i></b>,<br/><b><i>") + "</i></b>";

                                    var moreStr = (totalEmails > 2 ? " and <span class=\"spnScheculeEmailPopOver\"  style=\"text-decoration:underline;\" data-container=\"body\" data-toggle=\"popover\" data-html=\"true\" data-trigger=\"hover\" data-placement=\"left\" data-content=\"" + filteredInternal.join(",<br/>") + ",<br/>" + filteredExternalStr + "\"><b>" + (totalEmails - 2) + " more</b></span>" : "");

                                    var finalString = $(this).AutoCompleteEmail("formatEmails", [customString, row.Schedule.ExternalToApproved]) + moreStr;

                                    if (finalString.trim() == "")
                                        return "-";

                                    return finalString;

                                }
                            },
                            {
                                "mData": "Schedule.ExternalTo", "sTitle": "Approval Status", "mRender": function (tdata, type, row) {
                                    if (row.Schedule.ExternalTo == null || row.Schedule.ExternalTo == "" || row.Schedule.SFTPFolder == null || row.Schedule.SFTPFolder == "")
                                        return "N.A.";

                                    if (row.Schedule.ExternalToWorkflowCode == 0 || row.Schedule.SFTPFolderWorkflowCode == 0) {
                                        return "<label class=\"label label-warning\">Pending Approval</label>";
                                    }
                                    if (row.Schedule.ExternalToWorkflowCode == 1 || row.Schedule.SFTPFolderWorkflowCode == 1) {
                                        return "<label class=\"label label-success\">Approved</label> by <b>" + row.ExternalToModifiedBy + "</b> " + moment(row.Schedule.ExternalToModifiedAt).fromNow();
                                    }
                                    if (row.Schedule.ExternalToWorkflowCode == 2 || row.Schedule.SFTPFolderWorkflowCode == 2) {
                                        return "<label class=\"label label-danger\">Rejected</label> by <b>" + row.ExternalToModifiedBy + "</b> " + moment(row.Schedule.ExternalToModifiedAt).fromNow();
                                    }
                                }
                            },
                            { "mData": "LastModifiedBy", "sTitle": "Last Modified By" },
                            {
                                "mData": "Schedule.LastUpdatedAt",
                                "sTitle": "Last Modified At",
                                "mRender": function (tdata, type, row) {
                                    return "<div  title='" + getDateForToolTip(tdata) + "' date ='" + tdata + "'><i class='glyphicon glyphicon-time'></i>&nbsp;" + moment(tdata).fromNow() + "</div>";
                                },
                                "type": "dotnet-date"
                            },
                            {
                                "mData": "NextRunAt", "sTitle": "Next Scheduled Run At (" + getTimeZoneAbbr() + ")", "mRender": function (tdata, type, row) {

                                    if (!row.Schedule.IsActive) return "-";

                                    //return "<div  title='" + getDateForToolTip(tdata) + "' date ='" + tdata + "'><i class='glyphicon glyphicon-flash'></i>&nbsp;" + moment(tdata).fromNow() + "</div>";
                                    return "<i class='glyphicon glyphicon-flash'></i>&nbsp;" + moment(tdata).format("lll");
                                }
                            },
                            {
                                "mData": "Schedule.IsActive", "sTitle": "Is Active", "mRender": function (tdata, type, row) {
                                    return "<span class=\"activeToggleWrapper\">" +
                                        "<input " + (row.Schedule.IsActive ? "checked" : "") + "  type=\"checkbox\" data-size=\"mini\" data-offstyle=\"activeCheck btn btn-warning btn-xs\" data-width=\"95\" data-height=\"25\" data-onstyle=\"activeCheck btn btn-success btn-xs\" data-toggle=\"toggle\" data-on=\"Active\" data-off=\"Inactive\" />" +
                                        "</span>";
                                }
                            }, {
                                "mData": "Schedule.IsActive", "sTitle": "Action", "mRender": function (tdata, type, row) {
                                    return "<button data-loading-text=\"<i class='glyphicon glyphicon-refresh glyphicon icon-rotate'></i>&nbsp; Triggering..\" class=\"btnTriggerScheduleNow " + (row.Schedule.IsActive ? "" : " disabled ") + "btn btn-default btn-xs\"><i class='glyphicon glyphicon-flash'></i>&nbsp; Trigger now</button>";
                                }
                            },
                            {
                                "mData": "Id", "sTitle": "", "mRender": function (tdata, type, row) {
                                    return "<span class='icon-lg'>" +
                                        "<i class='glyphicon glyphicon-pencil' title='Edit Task'></i>&nbsp;&nbsp;" +
                                        "<i class='glyphicon glyphicon-trash' title='Delete Task'></i>&nbsp;&nbsp;" +
                                        "<i class='glyphicon glyphicon-list-alt' title='Show Logs'></i>&nbsp;&nbsp;" +
                                        "</span>";
                                }
                            }
                        ],
                        "oLanguage": {
                            "sSearch": "",
                            "sInfo": "Showing _START_ to _END_ of _TOTAL_ schedules",
                            "sInfoFiltered": " - filtering from _MAX_ schedules",
                            "sEmptyTable": "No schedules available"
                        },
                        "scrollX": false,
                        "scrollXInner": "100%",
                        "responsive": true,
                        "scrollY": "200px",
                        scrollCollapse: true,
                        scroller: response.data.length > 3,

                        // "sDom": "<'row header'<'col-md-4 header-left'i><'col-md-5 header-center'<'#toolbar_Notification'>><'col-md-3 header-right'f>>t",
                        "preDrawCallback": function (settings) {
                            $scope.tblReportSchedulesPageScrollPos = $(this).closest("div.dataTables_scrollBody").scrollTop();
                        },
                        "drawCallback": function (settings) {
                            $(this).closest("div.dataTables_scrollBody").scrollTop($scope.tblReportSchedulesPageScrollPos);
                            $(".activeToggleWrapper > input").bootstrapToggle();
                            $(".spnScheculeEmailPopOver").popover();
                        },
                        "order": [[6, "desc"]],
                        "columnDefs": [{ "width": "28%", "targets": [3] }],
                        "bPaginate": true,
                        iDisplayLength: -1,
                        sRowSelect: false
                    });
            }
        });
    }

    $scope.OriginalExternalToValue = "";
    $scope.OriginalSFTFolderValue = "";
    $scope.OriginalExternalToWorkflowCode = 0;
    $scope.OriginalSFTFolderWorkflowCode = 0;


    $scope.fnLoadJobFromScheduleGrid = function () {
        $scope.IsScheduleLoadingInProgress = true;
        $scope.IsAddNewJobClicked = false;
        $scope.Job.Schedule.CreatedAt = moment($scope.Job.Schedule.CreatedAt);
        $scope.Job.Schedule.LastModifiedAt = moment($scope.Job.Schedule.LastModifiedAt);
        if ($scope.Job.Schedule.ExternalToWorkflowCode)
            $scope.Job.Schedule.ExternalToModifiedAt = moment($scope.Job.Schedule.ExternalToModifiedAt).format("lll");
        if ($scope.Job.Schedule.SFTPFolderWorkflowCode)
            $scope.Job.Schedule.SFTPFolderModifiedAt = moment($scope.Job.Schedule.SFTPFolderModifiedAt).format("lll");
        if ($scope.IsDashboardSchedule) {
            if ($scope.fnValidatePreferences())
                $("#mdlToShowSchedulesConfig").modal("show");
        }
        else
            $("#mdlToShowSchedulesConfig").modal("show");
    }


    $("body").on("dblclick", "#tblReportSchedules tbody tr", function (event) {
        $scope.Job = tblReportSchedules.row(this).data();
        $scope.fnLoadJobFromScheduleGrid();
    });

    $("body").on("click", "#tblReportSchedules tbody td:last-child i.glyphicon-pencil", function (event) {
        event.preventDefault();
        $scope.Job = tblReportSchedules.row($(this).parentsUntil("tr")).data();
        $scope.fnLoadJobFromScheduleGrid();
    });

    $("body").on("click", "#tblReportSchedules tbody td:last-child i.glyphicon-trash", function (event) {
        event.preventDefault();
        var job = tblReportSchedules.row($(this).parentsUntil("tr")).data();

        bootbox.confirm("Are you sure you want to delete this job ?", function (result) {
            if (!result) {
                return;
            } else {
                notifySuccess("Job deleted successfully");
                $scope.fnDeleteSchedule(job.Id);
            }
        });
    });

    $scope.SelectedScheduleId = 0;
    $("body").on("click", "#tblReportSchedules tbody td:last-child i.glyphicon-list-alt", function (event) {
        event.preventDefault();

        $scope.TemplateName = $scope.IsDashboardSchedule ? $("#liDashboardTemplates").select2("data").text : $("#fundOrGroup").select2("data").text;;
        $scope.ReportName = $("#hdnReportName").val();

        var job = tblReportSchedules.row($(this).parentsUntil("tr")).data();
        $scope.SelectedScheduleId = job.Schedule.hmsScheduleId;

        $("#mdlToShowSchedulesLogs").modal("show");

    });


    $("#tblReportSchedules").on("click", ".activeToggleWrapper", function (event) {

        event.stopPropagation();
        event.stopImmediatePropagation();
        event.preventDefault();

        var $toggleBtn = $(this).find("input");
        var job = tblReportSchedules.row($(this).parentsUntil("tr")).data();
        var isActive = $(this).children().hasClass("btn-success") || $(this).children().hasClass("btn-primary");

        notifySuccess("changes saved successfully");
        $toggleBtn.bootstrapToggle("toggle");
        $scope.fnSetScheduleStatus(job.Id, job.Schedule.hmsScheduleId, !isActive);

    });


    $("#mdlToShowSchedulesLogs").on("shown.bs.modal", function () {

        $http.get("/Schedules/GetScheduleLogs?scheduleId=" + $scope.SelectedScheduleId + "&timeZone=" + getTimeZoneAbbr() + "&contextDate=" + $("#contextDate").text() + "&totalItems=20").then(function (response) {

            if ($("#tblScheduleLogs").hasClass("initialized")) {
                tblScheduleLogs.clear();
                tblScheduleLogs.rows.add(response.data);
                tblScheduleLogs.draw();
            } else {
                tblScheduleLogs = $("#tblScheduleLogs").not(".initialized").addClass("initialized").DataTable(
                    {
                        aaData: response.data,
                        rowId: "hmsScheduleId",
                        //"dom": "iftrI",
                        "bDestroy": true,
                        "columns": [
                            {
                                "mData": "ScheduleEndTime", "sTitle": "Status", "mRender": function (tdata, type, row) {
                                    return tdata == null ? "<label class='label label-warning'>In progress</label>" : "<label class='label label-success'>Completed</label>";
                                }
                            },
                            //{
                            //    "mData": "ContextDate", "sTitle": "Context Date", "mRender": function (tdata, type, row) {
                            //        return moment(tdata).format("YYYY-MM-DD");
                            //    }
                            //},
                            {
                                "mData": "ExpectedScheduleStartAt", "sTitle": "Expected Schedule Start Time (" + getTimeZoneAbbr() + ")", "mRender": function (tdata, type, row) {
                                    if (moment(tdata).format("YYYY") === "2020")
                                        return "N/A";

                                    return "<i class='glyphicon glyphicon-time'></i>&nbsp;</i>" + moment(tdata).format("lll") + "</i>";
                                }
                            },
                            {
                                "mData": "ScheduleStartTime", "sTitle": "Actual Schedule Start Time (" + getTimeZoneAbbr() + ")", "mRender": function (tdata, type, row) {
                                    return "<i class='glyphicon glyphicon-flash'></i>&nbsp;" + moment(tdata).format("lll");
                                }
                            },
                            {
                                "mData": "ExpectedScheduleEndAt", "sTitle": "Expected Schedule End Time (" + getTimeZoneAbbr() + ")", "mRender": function (tdata, type, row) {
                                    if (moment(tdata).format("YYYY") === "2020")
                                        return "N/A";

                                    return "<i class='glyphicon glyphicon-time'></i>&nbsp;</i>" + moment(tdata).format("lll") + "</i>";
                                }
                            },
                            {
                                "mData": "ScheduleEndTime", "sTitle": "Actual Schedule End Time (" + getTimeZoneAbbr() + ")", "mRender": function (tdata, type, row) {
                                    return tdata == null ? "-" : "<i class='glyphicon glyphicon-flash'></i>&nbsp;" + moment(tdata).format("lll");
                                }
                            },
                            {
                                "mData": "IsManualTrigger", "sTitle": "Is Manually Triggered", "mRender": function (tdata, type, row) {
                                    return tdata ? "Yes" : "No";
                                }
                            },
                        ],
                        "oLanguage": {
                            "sSearch": "",
                            "sInfo": "Showing _START_ to _END_ of _TOTAL_ schedule logs",
                            "sInfoFiltered": " - filtering from _MAX_ schedule logs",
                            "sEmptyTable": "No logs available"
                        },
                        "scrollX": true,
                        "scrollXInner": "100%",
                        "responsive": true,
                        "scrollY": "200px",
                        "order": [[1, "desc"]],
                        scrollCollapse: true,
                        scroller: response.data.length > 3,

                        // "sDom": "<'row header'<'col-md-4 header-left'i><'col-md-5 header-center'<'#toolbar_Notification'>><'col-md-3 header-right'f>>t",
                        "preDrawCallback": function (settings) {
                            $scope.tblReportSchedulesPageScrollPos = $(this).closest("div.dataTables_scrollBody").scrollTop();
                        },
                        "drawCallback": function (settings) {
                            $(this).closest("div.dataTables_scrollBody").scrollTop($scope.tblReportSchedulesPageScrollPos);
                            $(".activeToggleWrapper > input").bootstrapToggle();
                            $(".spnScheculeEmailPopOver").popover();
                        },
                        "bPaginate": true,
                        iDisplayLength: -1,
                        sRowSelect: false
                    });
            }

        });

    });



    $("#tblReportSchedules").on("click", ".btnTriggerScheduleNow", function (event) {

        var $btnTrigger = $(this).closest("button");
        $timeout(function () { $btnTrigger.button("loading"); }, 10);
        event.stopPropagation();
        event.stopImmediatePropagation();
        event.preventDefault();

        var job = tblReportSchedules.row($(this).parentsUntil("tr")).data();

        $scope.fnTriggerSchedule(job.Id, $btnTrigger);
        $(this).closest("button").button("reset");
    });


    $("#panelReportSchedules").on("shown.bs.collapse", function () {
        if (tblReportSchedules != undefined) {
            tblReportSchedules.columns.adjust();
            $(".activeToggleWrapper > input").bootstrapToggle();
        }
    });


    $("#pnlSchedulAdvanceSettings").on("show.bs.collapse",
        function () {
            $("#spnScheduleAdvanceSetting").removeClass("hmo-AdvanceSettings-plus").addClass("hmo-AdvanceSettings-minus");
        }).on("hide.bs.collapse", function () {
            $("#spnScheduleAdvanceSetting").addClass("hmo-AdvanceSettings-plus").removeClass("hmo-AdvanceSettings-minus");
        }).on("shown.bs.collapse", function () {
            $scope.IsScheduleLoadingInProgress = true;
            $("#liScheduleSFTPFolders").select2("val", $scope.Job.Schedule.SFTPFolder).trigger("change");
            $("#liScheduleInternalFolders").select2("val", $scope.Job.Schedule.InternalFolder).trigger("change");
            $("#liFundPreferences").select2("val", $scope.Job.Schedule.PreferredFundNameCode).trigger("change");

            $timeout(function () {
                $scope.IsScheduleLoadingInProgress = false;
            }, 200);
        });
});


$(document).ready(function () {
    //$(".clockpicker").clockpicker();
});