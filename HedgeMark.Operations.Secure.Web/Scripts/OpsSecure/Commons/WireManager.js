/// <reference path="../../angular.js" />
/// <reference path="../../moment.js" />

HmOpsApp.controller("wireInitiationCtrl", function ($scope, $http, $timeout, $q, $opsSharedScopes, $interval, $filter, $sce) {
    $opsSharedScopes.store("wireInitiationCtrl", $scope);

    $scope.$on("loadWireTicketDetails", function (event, wireTicketId, module, purpose, agreementName) {
        $scope.fnLoadWireTicketDetails(wireTicketId);
    });


    $scope.fnLoadWireTicketDetails = function (wireTicketId, module, purpose, agreementName) {
        $scope.isWireLoadingInProgress = true;
        $scope.isAccountCollapsed = true;
        $scope.isReceivingAccountCollapsed = true;
        $scope.isSSITemplateCollapsed = true;
        $scope.isAttachmentsCollapsed = true;
        $scope.isWorkflowLogsCollapsed = true;
        $scope.isSwiftMessagesCollapsed = true;
        $scope.Purpose = purpose;
        $scope.Module = module;
        $scope.AgreementName = agreementName;
        $q.all([$scope.getWireDetails(wireTicketId), $scope.getWireMessageTypes(module)])
            .then(function () {
                if ($scope.dzRawFileUploads != undefined)
                    $scope.dzRawFileUploads.removeAllFiles(true);
                $scope.initializeControls();
            });
    }


    $scope.isAccountCollapsed = true;
    $scope.isSSITemplateCollapsed = true;

    $scope.initializeDatePicker = function () {
        angular.element("#wireValueDate").datepicker({
            keyboardNavigation: true,
            format: "MM/dd/yyyy",
            daysOfWeekDisabled: [6, 0],
            autoclose: false,
            startDate: "+0d",
            endDate: "+7d",
            minViewMode: "days",
            //datesDisabled: JSON.parse(angular.element("#holidayDateList").val()),
            weekStart: 1
        }).on("changeDate", function (ev) {
            angular.element("#wireValueDate").addClass("editable-unsaved");
            angular.element("#wireValueDate").html(getFormattedUIDate(ev.date == undefined ? moment()._d : ev.date));
            if (!$scope.isWireLoadingInProgress) {
                $scope.WireTicket.ValueDate = angular.element("#wireValueDate").text();
                $scope.checkForCreatedWires();
                $scope.getApprovalTime($scope.accountDetail);
            }
            angular.element(".datepicker").hide();
        });
    }

    $scope.getWireMessageTypes = function (module) {
        return $http.get("/Home/GetWireMessageTypeDetails?module=" + module).then(function (response) {
            if (module == "Adhoc Report")
                $scope.MessageTypes = response.data;
            else {
                if ($scope.Purpose == "Respond to Broker Call")
                    $scope.MessageTypes = $filter('filter')(response.data, function (type) { return type.text != "MT210" }, true);
                else
                    $scope.MessageTypes = $filter('filter')(response.data, { text: 'MT210' }, true);
            }
        });
    }


    $scope.SwiftFormatMessageActiveTag = "";

    $scope.fnToggleCollapeSwiftMessagePanel = function () {
        if ($scope.SwiftFormatMessageActiveTag == "") {
            $("#wireSwiftMessagesDiv").collapse("show");

            $timeout(function () { $scope.fnShowFormattedSwiftMsg(null, "Outbound", $scope.wireTicketObj.SwiftMessages["Outbound"]); }, 10);
            $scope.isSwiftMessagesCollapsed = false;
            return;
        }
        if ($scope.SwiftFormatMessageActiveTag != "") {
            $("#wireSwiftMessagesDiv").collapse("hide");
            $scope.SwiftFormatMessageActiveTag = "";
            $scope.isSwiftMessagesCollapsed = true;
            return;
        }
    }


    $scope.fnShowFormattedSwiftMsg = function ($event, key, value) {

        if (value == "")
            return;

        if ($event != null) {
            $event.preventDefault();
            $event.stopPropagation();
        }

        if ($scope.SwiftFormatMessageActiveTag == key) {
            return;

            //$("#wireSwiftMessagesDiv").collapse("hide");
            //$scope.SwiftFormatMessageActiveTag = "";
            //$scope.isSwiftMessagesCollapsed = true;
        }

        $("#wireSwiftMessagesDiv").collapse("show");
        $scope.SwiftFormatMessageActiveTag = key;
        $scope.isSwiftMessagesCollapsed = false;

        //$scope.SwiftFormatMessageActiveTag = key;
        $scope.TrustedSwiftMessage = $sce.trustAsHtml(value);
    }

    $scope.fnPrintSwiftMessage = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        PrintSwiftMessage($(".swiftMessgeBlock").html());
    }

    //$("#wireSwiftMessagesDiv").on("shown.bs.collapse", function () {
    //    if ($scope.SwiftFormatMessageActiveTag == "")
    //        $scope.fnShowFormattedSwiftMsg(null, "Outbound", $scope.wireTicketObj.SwiftMessages["Outbound"]);
    //});

    $scope.getWireDetails = function (wireId) {
        return $http.get("/Home/GetWireDetails?wireId=" + wireId).then(function (response) {
            $scope.wireTicketObj = response.data.wireTicket;
            $scope.isEditEnabled = response.data.isEditEnabled;
            $scope.isAuthorizedUserToApprove = response.data.isAuthorizedUserToApprove;
            $scope.isCancelEnabled = response.data.isCancelEnabled;
            $scope.isApprovedOrFailed = response.data.isApprovedOrFailed;
            $scope.isInitiationEnabled = response.data.isInitiationEnabled;
            $scope.isLastModifiedUser = response.data.isLastModifiedUser;
            $scope.isDraftEnabled = response.data.isDraftEnabled;
            $scope.wireTicketObj.HMWire.CreatedAt = moment($scope.WireTicket.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
            $scope.WireTicket = $scope.wireTicketObj.HMWire;
            $scope.castToDate($scope.wireTicketObj.SendingAccount);
            $scope.accountDetail = angular.copy($scope.wireTicketObj.SendingAccount);
            if ($scope.WireTicket.IsBookTransfer) {
                $scope.castToDate($scope.wireTicketObj.ReceivingAccount);
                $scope.receivingAccountDetail = angular.copy($scope.wireTicketObj.ReceivingAccount);
            }
            $scope.ssiTemplate = angular.copy($scope.wireTicketObj.SSITemplate);
            $scope.workflowUsers = $scope.wireTicketObj.WorkflowUsers;
            $scope.attachmentUsers = $scope.wireTicketObj.AttachmentUsers;
            $scope.isWirePurposeAdhoc = response.data.isWirePurposeAdhoc;

            var keyValuePair = $scope.wireTicketObj.SwiftMessages;

            $scope.wireTicketObj.SwiftMessages = {};
            $(keyValuePair).each(function (i, v) {
                $scope.wireTicketObj.SwiftMessages[v.Key] = v.Value;
            });

            $scope.IsSwiftMessagesPresent = $scope.wireTicketObj.SwiftMessages != null && Object.keys($scope.wireTicketObj.SwiftMessages).length > 0;

            angular.forEach($scope.WireTicket.hmsWireDocuments, function (val, ind) {
                val.CreatedAt = moment(val.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
                val.hmsWire = null;
            });
            angular.forEach($scope.WireTicket.hmsWireWorkflowLogs, function (val, ind) {
                val.CreatedAt = moment(val.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
                val.hmsWire = null;
            });
            $timeout(function () {
                $scope.timeToApprove = angular.copy(response.data.deadlineToApprove);
                $scope.timeToApprove.Hours = $scope.timeToApprove.Hours + ($scope.timeToApprove.Days * 24);
                if (!$scope.isApprovedOrFailed) {
                    if ($scope.timeToApprove.Hours > 0) {
                        $scope.isDeadlineCrossed = false;
                        $("#wireErrorStatus").collapse("hide");
                        $scope.validationMsg = response.data.validationMsg;
                    }
                    else {
                        $("#wireErrorStatus").collapse("show");
                        $scope.isDeadlineCrossed = true;
                        $scope.validationMsg = "Note:Deadline crossed. Please select a future date for settlement.";
                    }
                }
                $interval.cancel($scope.promise);
                $scope.promise = $interval(timer, 1000);
            }, 50);

            $scope.viewAttachmentTable($scope.WireTicket.hmsWireDocuments);
        });
    }

    angular.element(document).on("change", "#liMessageType", function () {
        $timeout(function () {
            $scope.WireTicket.WireMessageTypeId = $("#liMessageType").select2("val");
        }, 50);
    });

    $("#wireAmount").numericEditor({
        bAllowNegative: false,
        fnFocusInCallback: function () {
            if ($(this).text() == "0")
                $(this).html('');
        },
        fnFocusOutCallback: function () {
            $scope.WireTicket.Amount = Math.abs($.convertToNumber($(this).text(), true));
            $(this).html($.convertToCurrency($scope.WireTicket.Amount, 2));
        }
    });

    $scope.checkForCreatedWires = function () {
        var receivingAccountId = $scope.WireTicket.IsBookTransfer ? angular.copy($scope.receivingAccountDetail.onBoardingAccountId) : angular.copy($scope.ssiTemplate.onBoardingSSITemplateId);
        if ($scope.accountDetail.onBoardingAccountId != 0 && receivingAccountId != 0) {
            $http.post("/Home/IsWireCreated", JSON.stringify({ valueDate: $("#wireValueDate").text(), purpose: $scope.Purpose, sendingAccountId: $scope.accountDetail.onBoardingAccountId, receivingAccountId: receivingAccountId }), { headers: { 'Content-Type': 'application/json; charset=utf-8;' } }).then(function (response) {
                $scope.isWireCreated = response.data;
                if (!$scope.isWireCreated)
                    $("#wireErrorStatus").collapse("hide");
                else {
                    $("#wireErrorStatus").collapse("show");
                    $scope.validationMsg = "Note:An Initiated wire exists for the same value date, purpose, sending and receiving account.";
                }
            });
        }
    }

    $scope.getWireLogText = function (wireLog) {
        if (wireLog == null)
            return "";
        return " the wire at " + $.getPrettyDate(wireLog.CreatedAt);
    }

    $scope.getWireLogStatus = function (wireLog, index) {

        angular.element("#workflowStatus_" + index).removeClass("text-info text-warning text-success text-blocked text-danger");

        if (wireLog == null)
            return "";

        switch (wireLog.WireStatusId) {
            case 1: angular.element("#workflowStatus_" + index).addClass("text-info");
                return "Drafted";
            case 2: angular.element("#workflowStatus_" + index).addClass("text-warning");
                return "Initiated";
            case 3: angular.element("#workflowStatus_" + index).addClass("text-success");
                return "Approved" + $scope.getSwiftStatusString(wireLog.SwiftStatusId);
            case 4: angular.element("#workflowStatus_" + index).addClass("text-blocked");
                angular.element("#workflowStatus_" + index).addClass("text-blocked");
                if($scope.WireTicket.SwiftStatusId == 1)
                    return "Rejected";
                else 
                    return  "Cancelled" + $scope.getSwiftStatusString(wireLog.SwiftStatusId);
            case 5: angular.element("#workflowStatus_" + index).addClass("text-danger");
                return "Failed";// + $scope.getSwiftStatusString(wireLog.SwiftStatusId);

        }
        return "";
    }

    $scope.getSwiftStatusString = function (swiftStatusId, $container) {
        switch (swiftStatusId) {

            case 1: return "";
            case 2: if ($container != null) $container.addClass("text-warning");
                return " & is Processing";
            case 3: if ($container != null) $container.addClass("text-info");
                return " & Acknowledged";
            case 4: if ($container != null) $container.addClass("text-dander");
                return " & N-Acknowledged";
            case 5: if ($container != null) $container.addClass("text-success");
                return " & Completed";
            case 6: if ($container != null) $container.addClass("text-dander");
                return " & Failed";
        }
        return "";
    }

    $scope.getWireStatus = function () {

        angular.element("#spnwireStatus").removeClass("text-info text-warning text-success text-blocked text-danger");

        switch ($scope.WireTicket.WireStatusId) {
            case 1: angular.element("#spnwireStatus").addClass("text-info");
                return $scope.isLastModifiedUser ? "Modified" : "Drafted";
            case 2: angular.element("#spnwireStatus").addClass("text-warning");
                return "Initiated";
            case 3: angular.element("#spnwireStatus").addClass("text-success");
                return "Approved";
            case 4: angular.element("#spnwireStatus").addClass("text-blocked");
                return $scope.WireTicket.SwiftStatusId == 1 ? "Rejected" : "Cancelled";
            case 5: angular.element("#spnwireStatus").addClass("text-danger");
                return "Failed";
        }
    }

    $scope.getSwiftStatus = function () {

        angular.element("#spnswiftStatus").removeClass("text-info text-warning text-success text-blocked text-danger");

        switch ($scope.WireTicket.SwiftStatusId) {
            case 2: angular.element("#spnswiftStatus").addClass("text-warning");
                return "Pending Acknowledgement";
            case 3: angular.element("#spnswiftStatus").addClass("text-info");
                return "Pending Confirmation";
            case 4: angular.element("#spnswiftStatus").addClass("text-danger");
                return "N-Acknowledged";
            case 5: angular.element("#spnswiftStatus").addClass("text-success");
                return "Completed";
            case 6: angular.element("#spnswiftStatus").addClass("text-danger");
                return "Failed";
        }
    }

    $scope.bindValues = function () {
        angular.element("#wireEntryDate").html(getFormattedUIDate(moment($scope.WireTicket.ContextDate)._d));
        if ($scope.isEditEnabled) {
            $scope.initializeDatePicker();
            angular.element("#wireAmount").text($.convertToCurrency($scope.WireTicket.Amount, 2)).attr('contenteditable', true);
            angular.element("#wireValueDate").datepicker("setDate", moment(getDateForDisplay($scope.WireTicket.ValueDate))._d);
        }
        else {
            angular.element("#wireAmount").text($.convertToCurrency($scope.WireTicket.Amount, 2)).attr('contenteditable', false);
            angular.element("#wireValueDate").datepicker('remove').html(getFormattedUIDate(moment(getDateForDisplay($scope.WireTicket.ValueDate))._d))
        }
        angular.element("#liMessageType").select2("val", $scope.WireTicket.WireMessageTypeId).trigger("change");
        angular.element("#liDeliveryCharges").select2("val", $scope.WireTicket.DeliveryCharges).trigger("change");
        $scope.WireTicket.CreatedAt = moment($scope.WireTicket.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
        $scope.dummyWire = angular.copy($scope.wireTicketObj);
        $scope.dummyWire.HMWire.ContextDate = $scope.WireTicket.ContextDate = moment($scope.WireTicket.ContextDate).format("YYYY-MM-DD");
        $scope.dummyWire.HMWire.ValueDate = angular.element("#wireValueDate").text();
        $scope.wireComments = "";
    }

    $scope.fnUpdateWireWithStatus = function (statusId) {
        if ($scope.bindAndValidateWireData()) {
            $scope.isUserActionDone = true;
            $scope.changeButtonStatus(statusId);
            var wireData = statusId == 4 ? $scope.dummyWire : $scope.WireTicket;
            $http.post("/Home/SaveWire", JSON.stringify({ wireTicket: $scope.wireTicketObj, statusId: statusId, comment: $scope.wireComments }), { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(function (response) {
                angular.element("#modalToRetrieveWires").modal("hide");
                if (statusId == 3)
                    notifySuccess("Wire approved successfully");
                else if (statusId == 4)
                    notifySuccess("Wire cancelled successfully");
                else
                    notifySuccess("Wire modified successfully");
                $scope.auditWireLogs(statusId);
            }, function (response, status, headers) {
                console.log(response, status, headers);
                angular.element("#modalToRetrieveWires").modal("hide");
                notifyWarning(response.statusText);
            });
        }
    }

    $scope.changeButtonStatus = function (statusId) {
        switch (statusId) {
            case 1: $("#draftWire").button("loading");
                break;
            case 2: $("#initiateWire").button("loading");
                break;
            case 3: $("#approveWire").button("loading");
                break;
            case 4: $("#cancelWire").button("loading");
                break;
        }
    }

    $scope.getInitiateButtonText = function () {
        if (!$scope.isWirePurposeAdhoc && $scope.wireObj.Purpose == "Send Call")
            return "Pre Advise";
        if ($scope.WireTicket.WireStatusId == 1)
            return "Re-Initiate";
        else
            return "Initiate";
    }

    $scope.confirmCancellation = function () {
        angular.element('#cancelWire').popover('destroy').popover({
            trigger: 'click',
            title: "Are you sure to " + ($scope.WireTicket.SwiftStatusId == 1 ? "reject" : "cancel") + " this wire?",
            placement: 'top',
            container: 'body',
            content: function () {
                return "<div class=\"btn-group pull-right\" style='margin-bottom:7px;'>"
                    + "<button class=\"btn btn-sm btn-success confirmCancellation\"><i class=\"glyphicon glyphicon-ok\"></i></button>"
                    + "<button class=\"btn btn-sm btn-default dismissCancellation\"><i class=\"glyphicon glyphicon-remove\"></i></button>"
                    + "</div>";
            },
            html: true
        }).popover("show");
        $(".popover-content").html("<div class=\"btn-group pull-right\" style='margin-bottom:7px;'>"
                    + "<button class=\"btn btn-sm btn-success confirmCancellation\"><i class=\"glyphicon glyphicon-ok\"></i></button>"
                    + "<button class=\"btn btn-sm btn-default dismissCancellation\"><i class=\"glyphicon glyphicon-remove\"></i></button>"
                    + "</div>");
    }

    $(document).on('click', ".confirmCancellation", function () {
        angular.element('#cancelWire').popover("hide");
        $scope.fnUpdateWireWithStatus(4);

    });
    $(document).on('click', ".dismissCancellation", function () {
        angular.element('#cancelWire').popover("hide");
    });

    angular.element("#modalToRetrieveWires").on("hidden.bs.modal", function () {
        $("#accountDetailDiv,#receivingAccountDetailDiv,#ssiTemplateDetailDiv,#attachmentsDiv,#wireWorkflowLogsDiv,#wireSwiftMessagesDiv").collapse("hide");
        $("button").button("reset");
        angular.element("#cancelWire").popover("hide");
        $scope.TrustedSwiftMessage = "";
        $scope.SwiftFormatMessageActiveTag = "";
        $scope.wireTicketObj.SwiftMessages = [];
        $("#wireErrorStatus").collapse("hide");
    });

    $scope.bindAndValidateWireData = function () {
        $scope.WireTicket.ValueDate = angular.element("#wireValueDate").text();
        $scope.WireTicket.PaymentOrReceipt = "Payment";
        $scope.WireTicket.SendingAccountNumber = angular.copy($scope.accountDetail.AccountNumber);
        $scope.WireTicket.OnBoardAccountId = angular.copy($scope.accountDetail.onBoardingAccountId);
        $scope.WireTicket.SendingPlatform = "SWIFT";
        if ($scope.wireTicketObj.IsNotice) {
            $scope.WireTicket.ReceivingAccountNumber = " ";
            $scope.WireTicket.Currency = angular.copy($scope.accountDetail.Currency);
            $scope.WireTicket.OnBoardSSITemplateId = 0;
        }
        else {
            $scope.WireTicket.ReceivingAccountNumber = $scope.wireTicketObj.IsBookTransfer ? angular.copy($scope.receivingAccountDetail.AccountNumber) : angular.copy($scope.ssiTemplate.AccountNumber);
            $scope.WireTicket.Currency = $scope.wireTicketObj.IsBookTransfer ? angular.copy($scope.receivingAccountDetail.Currency) : angular.copy($scope.ssiTemplate.Currency);
            $scope.WireTicket.OnBoardSSITemplateId = $scope.wireTicketObj.IsBookTransfer ? angular.copy($scope.receivingAccountDetail.onBoardingAccountId) : angular.copy($scope.ssiTemplate.onBoardingSSITemplateId);
        }
        //$scope.WireTicket.OnBoardAgreementId = !$scope.wireTicketObj.IsBookTransfer && $scope.wireObj.IsAdhocWire ? $("#liAgreement").select2('val') : angular.copy($scope.wireObj.AgreementId);
        $scope.WireTicket.WireMessageTypeId = angular.element("#liMessageType").select2('val');
        $scope.WireTicket.DeliveryCharges = $scope.WireTicket.WireMessageTypeId == "1" ? angular.element("#liDeliveryCharges").select2('val') : null;

        if ($scope.WireTicket.WireStatusId == 0)
            $scope.WireTicket.hmFundId = $scope.wireObj.IsAdhocWire ? $("#liFund").select2('val') : 0;

            if ($scope.WireTicket.Amount != 0) {
                if (!$scope.isDeadlineCrossed || $scope.wireComments.trim() != "") {
                    $("#wireErrorStatus").collapse("hide");
                    $scope.validationMsg = "";
                    return true;
                }
                else {
                    $("#wireErrorStatus").collapse("show");
                    $scope.validationMsg = "Please enter the comments to initiate the wire as deadline is crossed.";
                    return false;
                }
            }
            else {
                $("#wireErrorStatus").collapse("show");
                $scope.validationMsg = "Please enter a non-zero amount to initiate the wire";
                return false;
            }
    }

    $scope.castToDate = function (account) {
        var cashSweepTime = angular.copy(account.CashSweepTime);
        var cutOffTime = angular.copy(account.CutoffTime);
        var date = new Date();
        if (cashSweepTime != null && cashSweepTime != "" && cashSweepTime != undefined) {
            account.CashSweepTime = new Date(date.getYear(), date.getMonth(), date.getDay(), cashSweepTime.Hours, cashSweepTime.Minutes, cashSweepTime.Seconds);
        }
        if (cutOffTime != null && cutOffTime != "" && cutOffTime != undefined) {
            account.CutoffTime = new Date(date.getYear(), date.getMonth(), date.getDay(), cutOffTime.Hours, cutOffTime.Minutes, cutOffTime.Seconds);
        }
    }

    $scope.getApprovalTime = function (account) {
        $http.post("/Home/GetTimeToApproveTheWire", JSON.stringify({ cashSweepOfAccount: account.CashSweepTime, cutOffTimeOfAccount: account.CutoffTime, valueDate: $("#wireValueDate").text(), cashSweepTimeZone: account.CashSweepTimeZone }), { headers: { 'Content-Type': 'application/json; charset=utf-8;' } }).then(function (response) {

            $scope.timeToApprove = response.data;
            $scope.timeToApprove.Hours = $scope.timeToApprove.Hours + ($scope.timeToApprove.Days * 24);
            if ($scope.timeToApprove.Hours > 0) {
                $("#wireErrorStatus").collapse("hide");
                $scope.isDeadlineCrossed = false;
                $scope.validationMsg = "";
            }
            else {
                $("#wireErrorStatus").collapse("show");
                $scope.isDeadlineCrossed = true;
                $scope.validationMsg = "Deadline crossed. Please select a future date for settlement.";
            }
            $interval.cancel($scope.promise);
            $scope.promise = $interval(timer, 1000);
        });
    }

    $scope.timeToShow = "00 : 00 : 00";
    var timer = function () {
        if ($scope.timeToApprove.Seconds > 0) {
            $scope.timeToApprove.Seconds--;
            if ($scope.timeToApprove.Seconds == -1) {
                $scope.timeToApprove.Minutes--;
                if ($scope.timeToApprove.Minutes == -1) {
                    $scope.timeToApprove.Hours--;
                    $scope.timeToApprove.Minutes = 59;
                }
                $scope.timeToApprove.Seconds = 59;
            }
            $scope.timeToShow = (($scope.timeToApprove.Hours >= 10 ? $scope.timeToApprove.Hours : $scope.timeToApprove.Hours > 0 ? ("0" + $scope.timeToApprove.Hours) : $scope.timeToApprove.Hours)) + " : " + ($scope.timeToApprove.Minutes >= 10 ? $scope.timeToApprove.Minutes : ("0" + $scope.timeToApprove.Minutes)) + " : " + ($scope.timeToApprove.Seconds >= 10 ? $scope.timeToApprove.Seconds : ("0" + $scope.timeToApprove.Seconds));
        }
        else
            $scope.timeToShow = "00 : 00 : 00";
    }

    $scope.deliveryCharges = [{ id: "BEN", text: "Beneficiary" }, { id: "OUR", text: "Our customer charged" }, { id: "SHA", text: " Shared charges" }];

    $scope.initializeControls = function () {

        angular.element("#liDeliveryCharges").select2({
            placeholder: "Select Delivery Charges",
            data: $scope.deliveryCharges,
            allowClear: true,
            closeOnSelect: false,
            height: "30px"
        });

        angular.element("#liMessageType").select2({
            placeholder: "Select Message Type",
            data: $scope.MessageTypes,
            allowClear: true,
            closeOnSelect: false,
            height: "30px"
        });
        $scope.bindValues();
        $scope.isWireLoadingInProgress = false;
        $scope.isUserActionDone = false;
    }

    $scope.WireTicket = {
        hmsWireId: 0
    }

    angular.element("#uploadWireFiles").dropzone({
        url: "/Home/UploadWireFiles?wireId=" + $scope.WireTicket.hmsWireId,
        dictDefaultMessage: "<span style='font-size:15px;font-weight:normal;font-style:italic'>Drag/Drop wire documents here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".csv,.txt,.pdf,.xls,.xlsx,.zip,.rar",
        maxFiles: 3,
        previewTemplate: "<div class='row col-sm-2'><div class='panel panel-success panel-sm'> <div class='panel-heading'> <h3 class='panel-title' style='text-overflow: ellipsis;white-space: nowrap;overflow: hidden;'><span data-dz-name></span> - (<span data-dz-size></span>)</h3> " +
            "</div> <div class='panel-body'> <span class='dz-upload' data-dz-uploadprogress></span>" +
            "<div class='progress'><div data-dz-uploadprogress class='progress-bar progress-bar-warning progress-bar-striped active dzFileProgress' style='width: 0%'></div>" +
            "</div></div></div></div>",

        maxfilesexceeded: function (file) {
            this.removeAllFiles();
            this.addFile(file);
        },
        init: function () {
            this.on("processing", function (file) {
                this.options.url = "/Home/UploadWireFiles?wireId=" + $scope.WireTicket.hmsWireId
            });
        },
        processing: function (file, result) {
            angular.element("#uploadWireFiles").animate({ "min-height": "140px" });
        },
        success: function (file, result) {
            angular.element(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
            angular.element(".dzFileProgress").html("Upload Successful");

            angular.forEach(result.Documents, function (value, index) {
                value.CreatedAt = getFormattedUIDate(moment(value.CreatedAt)._d);
                $scope.WireTicket.hmsWireDocuments.push(value);
            });

            $scope.viewAttachmentTable($scope.WireTicket.hmsWireDocuments);
        },
        queuecomplete: function () {
        },
        complete: function (file, result) {
            $scope.dzRawFileUploads = this;
            angular.element("#uploadWireFiles").removeClass("dz-drag-hover");

            if (this.getRejectedFiles().length > 0 && this.getAcceptedFiles().length === 0 && this.getQueuedFiles().length === 0) {
                showMessage("File format is not supported to upload.", "Status");
                return;
            }

            if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
                notifySuccess("Files Uploaded successfully");
            }
        }
    });

    $scope.viewAttachmentTable = function (data) {

        if ($("#documentTable").hasClass("initialized")) {
            fnDestroyDataTable("#documentTable");
        }
        $scope.wireDocumentTable = $("#documentTable").not(".initialized").addClass("initialized").DataTable({
            "bDestroy": true,
            // responsive: true,
            aaData: data,
            "aoColumns": [
                {
                    "sTitle": "File Name",
                    "mData": "FileName"
                },
                {
                    "sTitle": "Uploaded By",
                    "mData": "CreatedBy",
                    "mRender": function (tdata, td, row, rowObj) {
                        var user = $scope.attachmentUsers[rowObj.row];
                        return "<div title='" + user + "' date='" + user + "'>" + user + "</div>";
                    }
                },
                {
                    "sTitle": "Uploded At",
                    "mData": "CreatedAt",
                    "type": "dotnet-date",
                    "mRender": function (tdata) {
                        return "<div title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + (moment(tdata).fromNow()) + "</div>";
                    }
                },
                {
                    "mData": "hmsWireDocumentId",
                    "sTitle": "Remove Document",
                    "visible": $scope.isEditEnabled,
                    "mRender": function () {
                        return "<button class='btn btn-danger btn-xs' title='Remove Document'><i class='glyphicon glyphicon-remove'></i>&nbsp;Remove</button>";
                    }
                }
            ],
            "deferRender": false,
            "bScrollCollapse": true,
            "searching": false,
            "bInfo": false,
            //scroller: true,
            //sortable: false,
            "sDom": "ift",
            //pagination: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "scrollY": 350,
            "order": [[2, "desc"]],

            "fnRowCallback": function (nRow, aData) {
                if (aData.FileName != "") {
                    $("td:eq(0)", nRow).html("<a title ='click to download the file' href='/Home/DownloadWireFile?fileName=" + aData.FileName + "&wireId=" + $scope.WireTicket.hmsWireId + "'>" + aData.FileName + "</a>");
                }
            },
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No files are available for the ssi templates",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Files"
            }
        });

        $timeout(function () {
            $scope.wireDocumentTable.columns.adjust().draw(true);
        }, 500);

        $("#documentTable tbody tr td:last-child button").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowElement = $scope.wireDocumentTable.row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this document from wire?", function (result) {
                if (!result) {
                    return;
                } else {
                    if (rowElement.hmsWireDcoumentId > 0) {
                        $http.post("/Home/RemoveWireDocument", { documentId: rowElement.hmsWireDcoumentId }).then(function () {
                            $scope.wireDocumentTable.row(selectedRow).remove().draw();
                            $scope.WireTicket.hmsWireDocuments.pop(rowElement);
                            notifySuccess("Account document has removed succesfully");
                        });
                    } else {
                        $scope.wireDocumentTable.row(selectedRow).remove().draw();
                        $scope.WireTicket.hmsWireDocuments.pop(rowElement);
                        notifySuccess("Account document has removed succesfully");
                    }
                }
            });
        });
    }

    $scope.collapseContainer = function (container) {
        switch (container) {
            case "Account": $scope.isAccountCollapsed = !$scope.isAccountCollapsed;
                break;
            case "ReceivingAccount": $scope.isReceivingAccountCollapsed = !$scope.isReceivingAccountCollapsed;
                break;
            case "SSITemplate": $scope.isSSITemplateCollapsed = !$scope.isSSITemplateCollapsed;
                break;
            case "Attachment": $scope.isAttachmentsCollapsed = !$scope.isAttachmentsCollapsed;
                $timeout(function () { $scope.wireDocumentTable.columns.adjust(); }, 100);
                break;
            case "Workflow": $scope.isWorkflowLogsCollapsed = !$scope.isWorkflowLogsCollapsed;
                break;
                //case "SwiftMessage": $scope.isSwiftMessagesCollapsed = !$scope.isSwiftMessagesCollapsed;
                //    break;
        }
    }

    $($("#modalToRetrieveWires [data-toggle='collapse']").next()).on("show.bs.collapse", function () {
        var target = $(this).prev();
        $("#modalToRetrieveWires").animate({ scrollTop: $("#modalToRetrieveWires").scrollTop() + target.offset().top - 50 }, 1000);
    });

    $scope.auditWireLogs = function (statusId) {
        var auditLogData = createWireAuditData(statusId);
        $http.post("/Audit/AuditWireLogs", JSON.stringify({ auditLogData: auditLogData }), { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(function (response) {
        });
    }

    function createWireAuditData(statusId) {
        var changes = new Array();
        var index = 0;
        var action = $scope.WireTicket.hmsWireId == 0 ? "Added" : "Edited";
        if ($scope.WireTicket.WireStatusId < 2) {
            changes[index] = new Array();
            changes[index][1] = "Value Date";
            changes[index][2] = action == "Added" ? "" : $scope.dummyWire.HMWire.ValueDate;
            changes[index][3] = $scope.WireTicket.ValueDate;
            index++;
            changes[index] = new Array();
            changes[index][1] = "Amount";
            changes[index][2] = action == "Added" ? "" : $scope.dummyWire.HMWire.Amount;
            changes[index][3] = $scope.WireTicket.Amount;
            index++;
            changes[index] = new Array();
            changes[index][1] = "Wire Message Type";
            changes[index][2] = action == "Added" ? "" : $scope.dummyWire.HMWire.WireMessageTypeId;
            changes[index][3] = $scope.WireTicket.WireMessageTypeId;
            index++;
            if ($scope.WireTicket.WireMessageTypeId == 1) {
                changes[index] = new Array();
                changes[index][1] = "Delivery Charges";
                changes[index][2] = action == "Added" ? "" : $scope.dummyWire.HMWire.DeliveryCharges;
                changes[index][3] = $scope.WireTicket.DeliveryCharges;
                index++;
            }
        }
        changes[index] = new Array();
        changes[index][1] = "Wire Status";
        changes[index][2] = action == "Added" ? "" : $scope.dummyWire.HMWire.WireStatusId;
        changes[index][3] = statusId;
        var auditdata = {};
        auditdata.ModuleName = $scope.Module;
        auditdata.Action = action;
        auditdata.Changes = changes;
        auditdata.AgreementName = "";
        auditdata.SendingAccount = $scope.accountDetail.AccountName;
        auditdata.IsBookTransfer = $scope.wireTicketObj.IsBookTransfer;
        auditdata.TransferType = $scope.wireTicketObj.TransferType;
        auditdata.ReceivingAccount = $scope.wireTicketObj.IsNotice ? "" : auditdata.IsBookTransfer ? $scope.receivingAccountDetail.AccountName : $scope.ssiTemplate.TemplateName;
        auditdata.Purpose = $scope.Purpose;
        auditdata.AssociationId = $scope.WireTicket.hmsWireId;
        return auditdata;
    }

});


