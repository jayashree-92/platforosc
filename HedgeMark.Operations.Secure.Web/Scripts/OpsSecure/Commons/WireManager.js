
HmOpsApp.controller("wireInitiationCtrl", function ($scope, $http, $timeout, $q, $opsSharedScopes, $interval, $filter, $sce) {
    $opsSharedScopes.store("wireInitiationCtrl", $scope);

    $scope.fnLoadWireTicketDetails = function (wireObj) {
        $scope.wireObj = angular.copy(wireObj);
        $scope.isWireLoadingInProgress = true;
        if ($scope.wireObj.WireId > 0)
            $scope.loadWireRelatedData();
        else
            $scope.loadAdhocWireRelatedData();
    }

    $scope.promise = null;
    $scope.WireTicketStatus = {};
    $scope.WireSourceModuleDetails = {};

    $scope.loadWireRelatedData = function () {

        $q.all([$scope.getWireDetails($scope.wireObj.WireId), $scope.getWireMessageTypes($scope.wireObj.Report)])
            .then(function () {
                if ($scope.dzRawFileUploads != undefined)
                    $scope.dzRawFileUploads.removeAllFiles(true);
                $scope.isAccountCollapsed = true;
                $scope.isCashBalancesCollapsed = true;
                $scope.isReceivingAccountCollapsed = true;
                $scope.isSSITemplateCollapsed = true;
                $scope.isAttachmentsCollapsed = true;
                $scope.isWorkflowLogsCollapsed = true;
                $scope.isSwiftMessagesCollapsed = true;
                $scope.canSave = false;
                $scope.initializeControls();

                $scope.fnResetDeadlineTimer();

            });
    }

    $scope.loadAdhocWireRelatedData = function () {

        $q.all([
            $scope.getAdhocWireDetails(), $scope.getWireMessageTypes($scope.wireObj.Report),
            $scope.getAdhocWireAssociations()
        ])
            .then(function () {
                if ($scope.dzRawFileUploads != undefined)
                    $scope.dzRawFileUploads.removeAllFiles(true);
                $scope.isAccountCollapsed = true;
                $scope.isCashBalancesCollapsed = true;
                $scope.isReceivingAccountCollapsed = true;
                $scope.isSSITemplateCollapsed = true;
                $scope.isAttachmentsCollapsed = true;
                $scope.isWorkflowLogsCollapsed = true;
                $scope.isSwiftMessagesCollapsed = true;
                $scope.canSave = false;
                $scope.initializeControls();
                $scope.fnResetDeadlineTimer();
            });
    }

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
        }).on("changeDate",
            function (ev) {
                angular.element("#wireValueDate").addClass("editable-unsaved");
                angular.element("#wireValueDate")
                    .html(getFormattedUIDate(ev.date == undefined ? moment()._d : ev.date));
                if (!$scope.isWireLoadingInProgress) {
                    $scope.WireTicket.ValueDate = angular.element("#wireValueDate").text();
                    $scope.checkForCreatedWires();
                    $scope.getApprovalTime($scope.accountDetail);
                    $scope.fnGetCashBalances($("#wireValueDate").text());
                }
                angular.element(".datepicker").hide();
            });
    }

    $("#wireValueDate").click(function () {
        var popup = $(this).offset();
        var popupTop = popup.top + 20;
        $timeout(function () {
            $(".datepicker").css({
                'top': popupTop
            });
        },
            50);
    });

    $scope.getWireMessageTypes = function (module) {
        return $http.get("/Home/GetWireMessageTypeDetails?module=" + module).then(function (response) {
            if (module == "Adhoc Report" || module == "Invoices")
                $scope.MessageTypes = response.data.wireMessages;
            else {
                if ($scope.wireObj.Purpose == "Respond to Broker Call")
                    $scope.MessageTypes = $filter("filter")(response.data.wireMessages,
                        function (type) { return type.text != "MT210" },
                        true);
                else
                    $scope.MessageTypes = $filter("filter")(response.data.wireMessages, { text: "MT210" }, true);
            }
            $scope.TransferTypes = response.data.wireTransferTypes;
            $scope.SenderInformation = response.data.wireSenderInformation;
            $scope.CollateralPurpose = response.data.wireCollateralCashPurpose;
        });
    }

    $scope.getAdhocWireDetails = function () {
        return $http.get("/Home/GetNewWireDetails").then(function (response) {
            $scope.wireTicketObj = response.data.wireTicket;
            $scope.WireTicket = $scope.wireTicketObj.HMWire;
            $scope.castToDate($scope.wireTicketObj.SendingAccount);
            $scope.accountDetail = angular.copy($scope.wireTicketObj.SendingAccount);
            $scope.ssiTemplate = angular.copy($scope.wireTicketObj.SSITemplate);
            $scope.wireTicketObj.IsFundTransfer = $scope.wireObj.IsFundTransfer;
            $scope.WireTicket.CreatedAt = moment($scope.WireTicket.CreatedAt).format("YYYY-MM-DD HH:mm:ss");

            $scope.WireTicketStatus = response.data.wireTicketStatus;
            $scope.WireTicketStatus.IsEditEnabled = true;
            $scope.WireTicketStatus.IsWirePurposeAdhoc = true;

            $scope.IsDuplicateWireCreated = response.data.isWireCreated;
            $scope.IsSwiftMessagesPresent = false;

            angular.forEach($scope.WireTicket.hmsWireDocuments, $scope.fnSetFormatedCreatedDt);
            angular.forEach($scope.WireTicket.hmsWireWorkflowLogs, $scope.fnSetFormatedCreatedDt);

            $timeout(function () {
                $interval.cancel($scope.promise);
                $scope.timeToApprove = "00 : 00 : 00";
            }, 100);

            $scope.viewAttachmentTable($scope.WireTicket.hmsWireDocuments);
        });
    }

    $scope.fnSetCurrentlyViewedInfo = function (allusers) {
        if (allusers.length > 0) {
            if (allusers.length == 1)
                $scope.currentlyViewedBy = allusers[0];
            else if (allusers.length == 2)
                $scope.currentlyViewedBy = allusers[0] + ", " + allusers[1];
            else if (allusers.length > 2)
                $scope.currentlyViewedBy = allusers[0] + ", " + allusers[1] + " and " + (allusers.length - 2) + " other(s).";

            $("#wireCurrentlyViewedBy").collapse("show");
        } else
            $("#wireCurrentlyViewedBy").collapse("hide");
    }

    $scope.fnSetFormatedCreatedDt = function (val, ind) {
        val.CreatedAt = moment(val.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
        val.hmsWire = null;
    };

    $scope.getWireDetails = function (wireId) {
        return $http.get("/Home/GetWireDetails?wireId=" + wireId).then(function (response) {
            $scope.wireTicketObj = response.data.wireTicket;
            $scope.WireTicketStatus = response.data.wireTicketStatus;
            $scope.wireTicketObj.HMWire.CreatedAt = moment($scope.WireTicket.CreatedAt).format("YYYY-MM-DD HH:mm:ss");

            $scope.sendingAccountsList = response.data.sendingAccountsList;
            $scope.receivingAccountsListOfFund = response.data.receivingAccountsList;

            $scope.WireTicket = $scope.wireTicketObj.HMWire;
            $scope.castToDate($scope.wireTicketObj.SendingAccount);
            $scope.accountDetail = angular.copy($scope.wireTicketObj.SendingAccount);
            if ($scope.wireTicketObj.IsFundTransfer) {
                $scope.castToDate($scope.wireTicketObj.ReceivingAccount);
                $scope.receivingAccountDetail = angular.copy($scope.wireTicketObj.ReceivingAccount);
            }
            $scope.ssiTemplate = angular.copy($scope.wireTicketObj.SSITemplate);
            $scope.workflowUsers = $scope.wireTicketObj.WorkflowUsers;
            $scope.attachmentUsers = $scope.wireTicketObj.AttachmentUsers;
            $scope.WireTicketStatus.IsWirePurposeAdhoc = response.data.isWirePurposeAdhoc;

            $scope.WireSourceModuleDetails = response.data.wireSourceModule;
            $scope.fnSetCurrentlyViewedInfo(response.data.currentlyViewedBy);

            var keyValuePair = $scope.wireTicketObj.SwiftMessages;

            $scope.wireTicketObj.SwiftMessages = {};
            $(keyValuePair).each(function (i, v) {
                $scope.wireTicketObj.SwiftMessages[v.Key] = v;
            });

            $scope.IsSwiftMessagesPresent = $scope.wireTicketObj.SwiftMessages != null && Object.keys($scope.wireTicketObj.SwiftMessages).length > 0;

            angular.forEach($scope.WireTicket.hmsWireDocuments, $scope.fnSetFormatedCreatedDt);
            angular.forEach($scope.WireTicket.hmsWireWorkflowLogs, $scope.fnSetFormatedCreatedDt);

            $scope.fnSetWireCutOffDetails(response.data.deadlineToApprove, response.data.IsWireCutOffApproved);

            //$timeout(function () {
            //    if (!$scope.WireTicketStatus.IsApprovedOrFailed) {
            //        $scope.fnValidateBasicWireDetails();
            //    }
            //}, 100);

            $scope.viewAttachmentTable($scope.WireTicket.hmsWireDocuments);
            $scope.isValidWireInitiation = $scope.validateWireInitiationofBIC();
            $scope.fnGetCashBalances(moment($scope.WireTicket.ValueDate).format("YYYY-MM-DD"));
        });
    }

    $scope.fnDownloadWireSourceFile = function (sourceModuleName, sourceModuleId) {
        if (sourceModuleName == "Invoices")
            window.location.href = "/Home/DownloadInvoiceWireFile?sourceModuleId=" + sourceModuleId;
    }

    $scope.SwiftFormatMessageActiveTag = "";

    $scope.fnToggleCollapeSwiftMessagePanel = function () {
        if ($scope.SwiftFormatMessageActiveTag == "") {
            $("#wireSwiftMessagesDiv").collapse("show");

            $timeout(function () {
                $scope.fnShowFormattedSwiftMsg(null,
                    "Outbound",
                    $scope.wireTicketObj.SwiftMessages["Outbound"]);
            },
                10);
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
        $scope.TrustedSwiftFINMessage = $sce.trustAsHtml(value.OriginalFinMsg);
        $scope.TrustedSwiftMessage = $sce.trustAsHtml(value.FormatedMsg);
    }

    $scope.fnPrintSwiftMessage = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();
        PrintSwiftMessage($(".swiftMessgeBlock").text());
    }

    $scope.fnCopySwiftMessageToClipboard = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();

        CopyToClipBoardByClassName("originalFINMessage");
        notifySuccess("FIN message copied to Clipboard");

    }

    //$("#wireSwiftMessagesDiv").on("shown.bs.collapse", function () {
    //    $scope.fnShowFormattedSwiftMsg("Outbound", $scope.wireTicketObj.SwiftMessages["Outbound"]);
    //});

    $scope.getWireLogText = function (wireLog) {
        if (wireLog == null)
            return "";

        return (wireLog.WireStatusId == 3 || wireLog.WireStatusId == 4 ? "" : " the") +
            " wire at " +
            $.getPrettyDate(wireLog.CreatedAt);
    }


    $scope.getWireLogStatusIcon = function (wireLog, index) {
        if (wireLog == null)
            return "";

        switch (wireLog.WireStatusId) {
            case 1:
                return "glyphicon-pencil";
            case 2:
                return "glyphicon-list-alt";
            case 3:
                return "glyphicon-ok";
            case 4:

                if ($scope.WireTicket.SwiftStatusId == 1)
                    return "glyphicon-ban-circle";
                else
                    return "glyphicon-trash" + $scope.getSwiftStatusString(wireLog.SwiftStatusId);
            case 5:
                return "glyphicon-remove";
            case 6:
                return "glyphicon-exclamation-sign";

        }
        return "";
    }

    $scope.getWireLogStatus = function (wireLog, index) {

        angular.element("#workflowStatus_" + index)
            .removeClass("text-info text-warning text-success text-blocked text-danger");

        if (wireLog == null)
            return "";

        switch (wireLog.WireStatusId) {
            case 1:
                angular.element("#workflowStatus_" + index).addClass("text-success");
                return "Drafted";
            case 2:
                angular.element("#workflowStatus_" + index).addClass("text-warning");
                return "Initiated";
            case 3:
                angular.element("#workflowStatus_" + index).addClass("text-info");
                return $scope.getSwiftStatusString(wireLog.SwiftStatusId) + " Approved";
            case 4:
                angular.element("#workflowStatus_" + index).addClass("text-blocked");
                if ($scope.WireTicket.SwiftStatusId == 1)
                    return "Rejected";
                else
                    return $scope.getSwiftStatusString(wireLog.SwiftStatusId) + " Cancelled";
            case 5:
                angular.element("#workflowStatus_" + index).addClass("text-danger");
                return "Failed";
            case 6:
                angular.element("#workflowStatus_" + index).addClass("text-info");
                return "On Hold";
        }
        return "";
    }

    $scope.getSwiftStatusString = function (swiftStatusId, $container) {
        switch (swiftStatusId) {
            case 1:
                return "";
            case 2:
                if ($container != null) $container.addClass("text-warning");
                return " started Processing the";
            case 3:
                if ($container != null) $container.addClass("text-success");
                return " Acknowledged the";
            case 4:
                if ($container != null) $container.addClass("text-dander");
                return " N-Acknowledged the ";
            case 5:
                if ($container != null) $container.addClass("text-info");
                return " Completed the";
            case 6:
                if ($container != null) $container.addClass("text-dander");
                return " Failed the";
        }
        return "";
    }


    $scope.getWireStatus = function () {

        angular.element("#spnwireStatus")
            .removeClass("label-info label-warning label-success label-default label-danger");

        switch ($scope.WireTicket.WireStatusId) {
            case 1:
                angular.element("#spnwireStatus").addClass("label-success");
                return $scope.WireTicketStatus.IsLastModifiedUser ? "Modified" : "Drafted";
            case 2:
                angular.element("#spnwireStatus").addClass("label-warning");
                return "Initiated";
            case 3:
                angular.element("#spnwireStatus").addClass("label-info");
                return "Approved";
            case 4:
                angular.element("#spnwireStatus").addClass("label-default");
                return $scope.WireTicket.SwiftStatusId == 1 ? "Rejected" : "Cancelled";
            case 5:
                angular.element("#spnwireStatus").addClass("label-danger");
                return "Failed";
            case 6:
                angular.element("#spnwireStatus").addClass("label-info");
                return "On Hold";
            default:
                return "Draft";
        }

    }

    $scope.getSwiftStatus = function () {

        angular.element("#spnswiftStatus")
            .removeClass("label-info label-warning label-success label-default label-danger");

        switch ($scope.WireTicket.SwiftStatusId) {
            case 2:
                angular.element("#spnswiftStatus").addClass("label-warning");
                return "Pending Acknowledgement";
            case 3:
                angular.element("#spnswiftStatus").addClass("label-success");
                return "Pending Confirmation";
            case 4:
                angular.element("#spnswiftStatus").addClass("label-dander");
                return "N-Acknowledged";
            case 5:
                angular.element("#spnswiftStatus").addClass("label-info");
                return "Completed";
            case 6:
                angular.element("#spnswiftStatus").addClass("label-dander");
                return "Failed";
        }
    }

    angular.element(document).on("change", "#liMessageType", function () {
        $timeout(function () {
            $scope.WireTicket.WireMessageTypeId = $("#liMessageType").select2("val");
            if ($scope.WireTicket.WireMessageTypeId != "") {
                if ($scope.wireTicketObj.IsFundTransfer) {
                    if ($scope.accountDetail.onBoardingAccountId != 0 && $scope.receivingAccountDetail.onBoardingAccountId != 0) {
                        $scope.validateAccountsForMessageType();
                        angular.element("#liDeliveryCharges").select2("val", ($scope.WireTicket.WireMessageTypeId != "1" ? null : "OUR")).trigger("change");
                    }
                } else if ($scope.accountDetail.onBoardingAccountId != 0 &&
                    $scope.ssiTemplate.onBoardingSSITemplateId != 0) {
                    $scope.validateAccountsForMessageType();
                }
                var messageType = $("#liMessageType").select2("data").text;
                if (messageType.indexOf("MT103") > -1 || messageType.indexOf("MT202") > -1) {
                    $scope.wireTicketObj.IsSenderInformationRequired = true;
                    angular.element("#liSenderInformation").select2("val", $scope.WireTicket.SenderInformationId).trigger("change");
                } else {
                    $scope.wireTicketObj.IsSenderInformationRequired = false;
                    angular.element("#liSenderInformation").select2("val", null).trigger("change");
                    $scope.WireTicket.SenderDescription = "";
                }

                if ($scope.WireTicket.hmsWireFieldId != null && $scope.WireTicket.hmsWireFieldId > 0) {
                    angular.element("#liCollateralPurpose").select2("val", $scope.WireTicket.hmsWireField.hmsCollateralCashPurposeLkupId).trigger("change");
                } else {
                    angular.element("#liCollateralPurpose").select2("val", "").trigger("change");
                }
            }
            $scope.isWireRequirementsFilled = !$scope.isWireRequirementsFilled;
        }, 50);
    });

    angular.element(document).on("change", "#liSenderInformation",
        function () {
            $timeout(function () {
                $scope.WireTicket.SenderInformationId = $("#liSenderInformation").select2("val");
                $scope.isWireRequirementsFilled = !$scope.isWireRequirementsFilled;
            }, 50);
        });

    angular.element(document).on("change", "#liCollateralPurpose",
        function () {
            $timeout(function () {
                if ($scope.WireTicket.hmsWireField != undefined)
                    $scope.WireTicket.hmsWireField.hmsCollateralCashPurposeLkupId = $("#liCollateralPurpose").select2("val");
                $scope.isWireRequirementsFilled = !$scope.isWireRequirementsFilled;
            }, 50);
        });

    $scope.validateAccountsForMessageType = function () {
        $http.post("/Home/ValidateAccountDetails",
            JSON.stringify({
                wireMessageType: $("#liMessageType").select2("data").text,
                account: $scope.accountDetail,
                receivingAccount: $scope.receivingAccountDetail,
                ssiTemplate: $scope.ssiTemplate,
                isFundTransfer: $scope.wireTicketObj.IsFundTransfer
            }),
            { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(function (response) {
                //$scope.isMandatoryFieldsMissing = response.data.isMandatoryFieldsMissing;
                $scope.isMandatoryFieldsMissing = false;
                if ($scope.isValidWireInitiation) {
                    $scope.fnValidateBasicWireDetails();
                }
            });
    }

    $("#wireAmount").numericEditor({
        bAllowNegative: false,
        fnFocusInCallback: function () {
            if ($(this).text() == "0")
                $(this).html("");
        },
        fnFocusOutCallback: function () {
            $scope.WireTicket.Amount = Math.abs($.convertToNumber($(this).text(), true));
            $(this).html($.convertToCurrency($scope.WireTicket.Amount, 2));
            $scope.isValidWireInitiation = $scope.validateWireInitiationofBIC();
            angular.element("#liMessageType").trigger("change");
        }
    });

    $scope.validateWireInitiationofBIC = function () {
        var isValid = false;
        switch ($scope.accountDetail.SwiftGroup.SwiftGroupStatusId) {
            case 1: isValid = false;
                break;
            case 2: isValid = $scope.WireTicket.Amount <= 10;
                break;
            case 3: isValid = true;
                break;
        }
        if (!isValid) {
            $("#wireErrorStatus").collapse("show").pulse({ times: 3 });
            if ($scope.accountDetail.SwiftGroup.SwiftGroupStatusId == 2)
                $scope.validationMsg = "Amount cannot exceed 10 for Sender's BIC of status Testing";
            else
                $scope.validationMsg = "Wire initiation is not allowed for Sender's BIC of status Requested";
        }
        return isValid;
    }

    $scope.checkForCreatedWires = function () {
        if ($scope.accountDetail.onBoardingAccountId == 0 || ($scope.WireTicket.OnBoardSSITemplateId == 0 && $scope.WireTicket.receivingAccountId == 0))
            return;

        $http.post("/Home/IsWireCreated", JSON.stringify({
            valueDate: $("#wireValueDate").text(),
            purpose: $scope.wireObj.Purpose, sendingAccountId: $scope.accountDetail.onBoardingAccountId,
            receivingAccountId: $scope.receivingAccountDetail != undefined ? angular.copy($scope.receivingAccountDetail.onBoardingAccountId) : "",
            receivingSSITemplateId: angular.copy($scope.ssiTemplate.onBoardingSSITemplateId),
            wireId: $scope.WireTicket.hmsWireId
        }), { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(function (response) {
            $scope.IsDuplicateWireCreated = $scope.WireTicket.hmsWireId == 0 && response.data;
            $scope.fnValidateBasicWireDetails();
        });
    }

    $(document).on("click", "#wireCreationDiv", function (event) {
        $timeout(function () {
            $scope.isUserActionDone = !$("#chkDuplicateWireCreation").prop("checked");
        }, 50);
    });

    $scope.bindValues = function () {
        var account = null, receivingAccount = null;
        if ($scope.WireTicket.hmsWireId == 0) {
            $scope.WireTicket.ContextDate = angular.copy($scope.wireObj.ContextDate);
            if ($scope.wireObj.Amount == "")
                $scope.wireObj.Amount = 0;
            angular.element("#wireEntryDate").html(getFormattedUIDate(moment(getDateForDisplay($scope.wireObj.ContextDate))._d));
            $scope.initializeDatePicker();
            angular.element("#wireValueDate").datepicker("setDate", null);
            if (!$scope.wireObj.IsAdhocWire) {
                var transferType = $scope.wireObj.Purpose == "Send Call" ? "Notice" : "3rd Party Transfer";
                $scope.wireTicketObj.IsNotice = transferType == "Notice";
                var wireTransferType = $filter("filter")($scope.TransferTypes, function (message) { return message.text == transferType }, true)[0];
                $scope.WireTicket.WireTransferTypeId = wireTransferType.id;
                var wireMessageType = $filter("filter")($scope.MessageTypes, function (message) { return message.text == ($scope.wireTicketObj.IsNotice ? "MT210" : $scope.ssiTemplate.MessageType) }, true)[0];
                angular.element("#liMessageType").select2("val", wireMessageType.id).trigger("change");
                if (!$scope.wireTicketObj.IsNotice) {
                    angular.element("#liDeliveryCharges").select2("val", ($scope.ssiTemplate.MessageType != "MT103" ? null : "OUR")).trigger("change");
                }
                else
                    $scope.wireTicketObj.ReceivingAccountCurrency = $scope.accountDetail.Currency;
                angular.element("#wireAmount").text($.convertToCurrency($scope.wireObj.Amount, 2)).attr("contenteditable", false);
                $scope.WireTicket.Amount = angular.copy($scope.wireObj.Amount);
            }
            if ($scope.wireObj.IsAdhocWire) {
                angular.element("#wireAmount").text("").attr("contenteditable", true);
                $scope.isWireRequirementsFilled = false;
            }
        }
        else {
            angular.element("#wireEntryDate").html(getFormattedUIDate(moment(getDateForDisplay($scope.WireTicket.ContextDate))._d));
            if ($scope.WireTicketStatus.IsEditEnabled && $scope.wireObj.Report == "Adhoc Report") {
                $scope.initializeDatePicker();
                angular.element("#wireValueDate").datepicker("setDate", moment(getDateForDisplay($scope.WireTicket.ValueDate))._d);
            }
            else
                angular.element("#wireValueDate").datepicker("remove").html(getFormattedUIDate(moment(getDateForDisplay($scope.WireTicket.ValueDate))._d))
            angular.element("#liCurrency").select2("val", $scope.accountDetail.Currency).trigger("change");
            $scope.WireTicket.Amount = $scope.WireTicket.WireStatusId == 1 ? angular.copy($scope.wireObj.Amount) : $scope.WireTicket.Amount;
            angular.element("#wireAmount").text($.convertToCurrency(($scope.WireTicket.Amount), 2)).attr("contenteditable", false);
            angular.element("#liMessageType").select2("val", $scope.WireTicket.WireMessageTypeId).trigger("change");
            angular.element("#liDeliveryCharges").select2("val", $scope.WireTicket.DeliveryCharges).trigger("change");
        }

        $scope.WireTicket.CreatedAt = moment($scope.WireTicket.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
        $scope.dummyWire = angular.copy($scope.wireTicketObj);
        $scope.dummyWire.HMWire.ContextDate = $scope.WireTicket.ContextDate = moment($scope.WireTicket.ContextDate).format("YYYY-MM-DD");
        $scope.dummyWire.HMWire.ValueDate = angular.element("#wireValueDate").text();
        $scope.wireComments = "";
    }

    $scope.bindWireValues = function () {
        angular.element("#wireEntryDate").html(getFormattedUIDate(moment(getDateForDisplay($scope.WireTicket.ContextDate))._d));
        angular.element("#liCurrency").select2("val", $scope.accountDetail.Currency).trigger("change");
        if ($scope.WireTicketStatus.IsEditEnabled && $scope.wireObj.Report == "Adhoc Report") {
            $scope.initializeDatePicker();
            angular.element("#wireAmount").text($.convertToCurrency($scope.WireTicket.Amount, 2)).attr("contenteditable", true);
            angular.element("#wireValueDate").datepicker("setDate", moment(getDateForDisplay($scope.WireTicket.ValueDate))._d);
        }
        else {
            angular.element("#wireAmount").text($.convertToCurrency($scope.WireTicket.Amount, 2)).attr("contenteditable", false);
            angular.element("#wireValueDate").datepicker("remove").html(getFormattedUIDate(moment(getDateForDisplay($scope.WireTicket.ValueDate))._d))
        }
        angular.element("#liMessageType").select2("val", $scope.WireTicket.WireMessageTypeId).trigger("change");
        angular.element("#liDeliveryCharges").select2("val", $scope.WireTicket.DeliveryCharges).trigger("change");
        angular.element("#liSenderInformation").select2("val", $scope.WireTicket.SenderInformationId).trigger("change");
        if ($scope.WireTicket != null && $scope.WireTicket.hmsWireField != null)
            angular.element("#liCollateralPurpose").select2("val", $scope.WireTicket.hmsWireField.hmsCollateralCashPurposeLkupId).trigger("change");
        $scope.WireTicket.CreatedAt = moment($scope.WireTicket.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
        $scope.dummyWire = angular.copy($scope.wireTicketObj);
        $scope.dummyWire.HMWire.ContextDate = $scope.WireTicket.ContextDate = moment($scope.WireTicket.ContextDate).format("YYYY-MM-DD");
        $scope.dummyWire.HMWire.ValueDate = angular.element("#wireValueDate").text();
        $scope.wireComments = "";
    }

    angular.element("#modalToRetrieveWires").on("hidden.bs.modal", function () {
        angular.element("#accountDetailDiv,#receivingAccountDetailDiv,#ssiTemplateDetailDiv,#attachmentsDiv,#wireWorkflowLogsDiv,#wireSwiftMessagesDiv").collapse("hide");
        if (($scope.wireObj.Purpose == "Respond to Broker Call" || $scope.wireObj.Purpose == "Send Call") && !$scope.wireObj.IsAdhocPage) {
            $opsSharedScopes.get("CollateralTableCtrl").wireObj.Amount = angular.copy($scope.WireTicket.Amount);
            $opsSharedScopes.get("CollateralTableCtrl").canSaveAmount = angular.copy($scope.canSave);
        }
        angular.element("button").button("reset");
        angular.element("#cancelWire").popover("hide");
        $("#wireErrorStatus").collapse("hide");
        angular.element("#liWireTransferType").select2("val", 1).trigger("change");
        $scope.WireTicket = {};
    });

    $scope.fnUpdateWireWithStatus = function (statusId) {
        if ($scope.bindAndValidateWireData(statusId)) {
            $scope.isUserActionDone = true;
            $scope.changeButtonStatus(statusId);
            var wireData = statusId == 4 ? $scope.dummyWire : angular.copy($scope.wireTicketObj);
            $http.post("/Home/SaveWire", JSON.stringify({ wireTicket: wireData, reportMapId: $scope.wireObj.ReportMapId, purpose: $scope.wireObj.Purpose, statusId: statusId, comment: $scope.wireComments }), { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(function (response) {
                $scope.canSave = true;
                angular.element("#modalToRetrieveWires").modal("hide");
                if ($scope.WireTicket.WireStatusId == 0)
                    notifySuccess("Wire initiated successfully");
                else if (statusId == 3)
                    notifySuccess("Wire approved successfully");
                else if (statusId == 4)
                    notifySuccess("Wire cancelled successfully");
                else if (statusId == 6)
                    notifySuccess("Wire changed to On-Hold");
                else
                    notifySuccess("Wire modified successfully");
                $scope.auditWireLogs(statusId);
            }, function (error) {
                angular.element("#modalToRetrieveWires").modal("hide");
                notifyError("Wire failed due to Internal server error");
            });
        }
    }

    $scope.changeButtonStatus = function (statusId) {
        switch (statusId) {
            case 1: $("#draftWire").button("loading");
                break;
            case 2:
                $("#initiateWire").button("loading");
                $("#unHoldWire").button("loading");
                break;
            case 3: $("#approveWire").button("loading");
                break;
            case 4: $("#cancelWire").button("loading");
                break;
            case 6: $("#holdWire").button("loading");
                break;
        }
    }

    $scope.getInitiateButtonText = function () {
        if (!$scope.WireTicketStatus.IsWirePurposeAdhoc && $scope.wireObj.Purpose == "Send Call")
            return "Pre Advise";
        if ($scope.wireTicketObj.IsNotice)
            return "Initiate & Approve";
        if ($scope.WireTicket.WireStatusId == 1)
            return "Re-Initiate";
        else
            return "Initiate";
    }

    $scope.getCancelButtonText = function (isLoading) {
        if (isLoading)
            return $scope.WireTicket.SwiftStatusId == 1 ? "Rejecting" : "Cancelling";
        else
            return $scope.WireTicket.SwiftStatusId == 1 ? "Reject" : "Cancel";

    }

    $scope.confirmCancellation = function () {
        angular.element("#cancelWire").popover("destroy").popover({
            trigger: "click",
            title: "Are you sure to " + ($scope.WireTicket.SwiftStatusId == 1 ? "reject" : "cancel") + " this wire?",
            placement: "top",
            container: "body",
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

    $(document).on("click", ".confirmCancellation", function () {
        angular.element("#cancelWire").popover("hide");
        $scope.fnUpdateWireWithStatus(4);

    });
    $(document).on("click", ".dismissCancellation", function () {
        angular.element("#cancelWire").popover("hide");
    });

    $scope.bindAndValidateWireData = function (statusId) {
        $scope.WireTicket.ValueDate = angular.element("#wireValueDate").text();
        $scope.WireTicket.PaymentOrReceipt = "Payment";
        $scope.WireTicket.SendingAccountNumber = angular.copy($scope.accountDetail.UltimateBeneficiaryAccountNumber);
        $scope.WireTicket.OnBoardAccountId = angular.copy($scope.accountDetail.onBoardingAccountId);
        $scope.WireTicket.SendingPlatform = "SWIFT";
        if ($scope.wireTicketObj.IsNotice) {
            $scope.WireTicket.ReceivingAccountNumber = " ";
            $scope.WireTicket.Currency = angular.copy($scope.accountDetail.Currency);
            $scope.WireTicket.OnBoardSSITemplateId = $scope.wireObj.IsAdhocWire ? 0 : angular.copy($scope.ssiTemplate.onBoardingSSITemplateId);
        }
        else {
            $scope.WireTicket.ReceivingAccountNumber = $scope.wireTicketObj.IsFundTransfer ? angular.copy($scope.receivingAccountDetail.UltimateBeneficiaryAccountNumber) : angular.copy($scope.ssiTemplate.UltimateBeneficiaryAccountNumber);
            $scope.WireTicket.Currency = $scope.wireTicketObj.IsFundTransfer ? angular.copy($scope.receivingAccountDetail.Currency) : angular.copy($scope.ssiTemplate.Currency);
            $scope.WireTicket.OnBoardSSITemplateId = angular.copy($scope.ssiTemplate.onBoardingSSITemplateId);
            $scope.WireTicket.ReceivingOnBoardAccountId = $scope.wireTicketObj.IsFundTransfer ? angular.copy($scope.receivingAccountDetail.onBoardingAccountId) : 0;
        }
        //$scope.WireTicket.OnBoardAgreementId = !$scope.wireTicketObj.IsFundTransfer && $scope.wireObj.IsAdhocWire ? $("#liAgreement").select2('val') : angular.copy($scope.wireObj.AgreementId);
        $scope.WireTicket.WireMessageTypeId = angular.element("#liMessageType").select2("val");
        $scope.WireTicket.DeliveryCharges = $scope.WireTicket.WireMessageTypeId == "1" ? angular.element("#liDeliveryCharges").select2("val") : null;
        if ($scope.wireTicketObj.IsSenderInformationRequired) {
            $scope.WireTicket.SenderInformationId = angular.element("#liSenderInformation").select2("val");
            $scope.WireTicket.SenderDescription = angular.element("#wireSenderDescription").val();
        }

        if ($scope.WireTicket.hmsWireField == null)
            $scope.WireTicket.hmsWireField = {};
        $scope.WireTicket.hmsWireField.hmsCollateralCashPurposeLkupId = angular.element("#liCollateralPurpose").select2("val");

        if ($scope.WireTicket.WireStatusId == 0)
            $scope.WireTicket.hmFundId = $scope.wireObj.IsAdhocWire ? $("#liFund").select2("val") : 0;

        //Validation not required for Hold Status
        if (statusId == 6)
            return true;

        if ($scope.wireTicketObj.IsSenderInformationRequired && !$scope.validateSenderDescription())
            return false;

        if ($scope.WireTicket.Amount == 0) {
            $("#wireErrorStatus").collapse("show").pulse({ times: 3 });
            $scope.fnShowErrorMessage("Please enter a non-zero amount to initiate the wire");
            return false;
        }

        if ($scope.WireTicketStatus.ShouldEnableCollateralPurpose && $("#liCollateralPurpose").val() == "") {
            $scope.fnShowErrorMessage("Please select a valid collateral purpose to initiate the wire.");
            return false;
        }

        if (!$scope.IsWireCutOffApproved && $scope.wireComments.trim() == "") {
            $scope.fnShowErrorMessage("Please enter the comments to initiate the wire without Cut-off determination.");
            return false;
        }

        if ($scope.isDeadlineCrossed && $scope.wireComments.trim() == "") {
            $scope.fnShowErrorMessage("Please enter the comments to initiate the wire as deadline is crossed.");
            return false;
        }

        if ($("#liWireTransferType").val() != undefined && $("#liWireTransferType").val() === "4") {
            $("#wireErrorStatus").collapse("hide");
            $scope.validationMsg = "";
            return true;
        }

        //validate against the available cash balances
        if ($scope.CashBalance.IsNewBalanceOffLimit && $("#chkCashBalOff").prop("checked") === false) {
            $scope.fnShowErrorMessage("Please note that the Amount exceeded the available Cash Balances.");
            return false;
        }


        $("#wireErrorStatus").collapse("hide");
        $scope.validationMsg = "";
        return true;
    }

    $scope.validateSenderDescription = function () {
        var splittedInfo = $scope.WireTicket.SenderDescription.split("\n");
        if (splittedInfo.length > 6) {
            $scope.fnShowErrorMessage("Sender Description cannot exceed 6 lines");
            return false;
        }
        var isInvalid = $filter("filter")(splittedInfo, function (split) { return split != undefined && split.length > 33 }, true)[0] != undefined;
        if (isInvalid) {
            $scope.fnShowErrorMessage("Sender Description character length cannot exceed 33 in each line");
        }
        return !isInvalid;
    }

    $scope.fnShowErrorMessage = function (message) {
        $scope.validationMsg = message;
        $("#wireErrorStatus").collapse("show").pulse({ times: 3 });
    }

    $scope.castToDate = function (account) {
        var cashSweepTime = angular.copy(account.CashSweepTime);
        var cutOffTime = null;
        if (account.WirePortalCutoff != null)
            cutOffTime = angular.copy(account.WirePortalCutoff.CutoffTime);
        var date = new Date();
        if (cashSweepTime != null && cashSweepTime != "" && cashSweepTime != undefined) {
            account.CashSweepTime = new Date(date.getYear(), date.getMonth(), date.getDate(), cashSweepTime.Hours, cashSweepTime.Minutes, cashSweepTime.Seconds);
        }
        if (account.WirePortalCutoff != null && cutOffTime != null && cutOffTime != "" && cutOffTime != undefined) {
            account.WirePortalCutoff.CutoffTime = new Date(date.getYear(), date.getMonth(), date.getDate(), cutOffTime.Hours, cutOffTime.Minutes, cutOffTime.Seconds);
        }
    }


    $scope.getApprovalTime = function (account) {

        if (account.onBoardingAccountId == undefined || account.onBoardingAccountId == 0)
            return;

        $http.post("/Home/GetTimeToApproveTheWire", ({ onBoardingAccountId: account.onBoardingAccountId, valueDate: $("#wireValueDate").text() }), { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(function (response) {
            $scope.fnSetWireCutOffDetails(response.data.timeToApprove, response.data.IsWireCutOffApproved);
        });
    }

    $scope.fnSetWireCutOffDetails = function (timeToApprove, isApproved) {
        $scope.timeToApprove = timeToApprove;
        $scope.isDeadlineCrossed = !($scope.timeToApprove.Hours > 0 || ($scope.timeToApprove.Hours == 0 && $scope.timeToApprove.Minutes >= 0 && $scope.timeToApprove.Seconds > 0));
        $scope.IsWireCutOffApproved = isApproved;

        $scope.fnValidateBasicWireDetails();
        $scope.fnResetDeadlineTimer();
    }

    $scope.fnHideErrorMessage = function () {
        $scope.isUserActionDone = false;
        $("#wireErrorStatus").collapse("hide");
        $scope.validationMsg = "";
    }

    $scope.fnValidateBasicWireDetails = function () {

        if ($scope.WireTicket.OnBoardAccountId == 0) {
            $scope.fnHideErrorMessage();
        }

        if (!$scope.IsWireCutOffApproved && !$scope.wireTicketObj.IsNotice) {
            $scope.fnShowErrorMessage("Note: Unable to determine Wire Cut-off date as it is not Approved.");
        } else if ($scope.isDeadlineCrossed && !$scope.wireTicketObj.IsNotice) {
            $scope.fnShowErrorMessage("Note: Deadline crossed. Please select a future date for settlement.");
        } else if ($scope.isMandatoryFieldsMissing) {
            $scope.fnShowErrorMessage(angular.copy($scope.mandatoryFieldMsg));
        } else if ($scope.IsDuplicateWireCreated) {
            $("#chkDuplicateWireCreation").prop("checked", false).trigger("change").trigger("change");
            $scope.isUserActionDone = true;
            $scope.fnShowErrorMessage("An Initiated wire exists for the same value date, purpose, sending and receiving account.");
        } else {
            $scope.fnHideErrorMessage();
        }
    }


    $scope.timeToShow = "00 : 00 : 00";
    $scope.fnResetDeadlineTimer = function () {
        if ($scope.promise != null)
            $interval.cancel($scope.promise);
        $scope.promise = $interval(function () {
            $scope.timeToShow = fnGetWireDeadlineCounter($scope.timeToApprove);
            $scope.isDeadlineCrossed = $scope.timeToApprove.Days <= 0 && $scope.timeToApprove.Hours < 0;
        }, 1000);
    }

    $scope.deliveryCharges = [{ id: "BEN", text: "Beneficiary" }, { id: "OUR", text: "Our customer charged" }, { id: "SHA", text: " Shared charges" }];

    function formatSelect(selectData) {
        return selectData.text.indexOf("-") != -1 ? selectData.text.split("-")[0] : selectData.text;
    }

    function formatResult(selectData) {
        return selectData.text;
    }

    $scope.initializeControls = function () {

        angular.element("#liDeliveryCharges").select2({
            placeholder: "Select Delivery Charges",
            data: $scope.deliveryCharges,
            allowClear: true,
            closeOnSelect: false
        });

        angular.element("#liMessageType").select2({
            placeholder: "Select Message Type",
            data: $scope.MessageTypes,
            allowClear: true,
            closeOnSelect: false
        });

        angular.element("#liSenderInformation").select2("destroy").val("");
        angular.element("#liSenderInformation").select2({
            placeholder: "Select Sender Information",
            data: $scope.SenderInformation,
            allowClear: true,
            formatSelection: formatSelect,
            formatResult: formatResult,
            closeOnSelect: false
        });

        angular.element("#liCollateralPurpose").select2("destroy").val("");
        angular.element("#liCollateralPurpose").select2({
            placeholder: "Select Collateral Purpose",
            data: $scope.CollateralPurpose,
            allowClear: true,
            formatSelection: formatSelect,
            formatResult: formatResult,
            closeOnSelect: false
        });

        if ($scope.wireObj.IsAdhocWire) {
            $scope.getHFunds();
            $scope.purposes = $filter("orderBy")($scope.purposes, "text");
            angular.element("#liWiresPurpose").select2("destroy").val("");
            angular.element("#liWiresPurpose").select2({
                placeholder: "Select Purpose",
                data: $scope.purposes,
                allowClear: true,
                closeOnSelect: false,
                val: ""
            });
            $scope.currencies = $filter("orderBy")($scope.currencies, "text");
            angular.element("#liCurrency").select2("destroy").val("");
            angular.element("#liCurrency").select2({
                placeholder: "Select Currency",
                data: [],
                allowClear: true,
                closeOnSelect: false,
                val: ""
            });
            angular.element("#liSendingAccount").select2("destroy").val("");
            angular.element("#liSendingAccount").select2({
                placeholder: "Select " + ($scope.wireTicketObj.IsNotice ? "Receiving" : "Sending") + " Account",
                data: [],
                allowClear: true,
                closeOnSelect: false
            });

            angular.element("#liReceivingBookAccount").select2("destroy").val("");
            angular.element("#liReceivingBookAccount").select2({
                placeholder: "Select Receiving Account",
                data: [],
                allowClear: true,
                closeOnSelect: false
            });

            angular.element("#liReceivingAccount").select2("destroy").val("");
            angular.element("#liReceivingAccount").select2({
                placeholder: "Select Receiving Account",
                data: [],
                allowClear: true,
                closeOnSelect: false
            });

            angular.element("#liWireTransferType").select2("destroy").val("");
            angular.element("#liWireTransferType").select2({
                placeholder: "Select Transfer Type",
                data: $scope.TransferTypes,
                closeOnSelect: false
            });

            angular.element("#liWiresPurpose").select2("val", "").trigger("change");
            angular.element("#liWireTransferType").select2("val", 1).trigger("change");
            $scope.isPurposeEnabled = false;
            $scope.isFundsChanged = false;
            $scope.isSendingAccountEnabled = false;
            $scope.isReceivingAccountEnabled = false;
            $scope.WireTicket.OnBoardAccountId = "";
            $scope.WireTicket.OnBoardSSITemplateId = "";
            $scope.WireTicket.ReceivingOnBoardAccountId = "";
        }
        else if ($scope.WireTicket.hmsWireId == 0) {
            angular.element("#liSendingAccount").select2("destroy").val("");
            angular.element("#liSendingAccount").select2({
                placeholder: "Select " + ($scope.wireTicketObj.IsNotice ? "Receiving" : "Sending") + " Account",
                data: $scope.sendingAccountsList,
                closeOnSelect: false
            });
            angular.element("#liSendingAccount").select2("val", $scope.sendingAccountsList[0].id).trigger("change");
            angular.element("#liSenderInformation").select2("enable");
            angular.element("#liCollateralPurpose").select2("enable");
            angular.element("#wireSenderDescription").removeAttr("disabled");
        }
        else if ($scope.WireTicketStatus.IsEditEnabled) {
            $scope.IsNormalTransfer = $scope.WireTicket.WireTransferTypeId == 1;
            $timeout(function () {
                angular.element("#liWireTransferType").select2("val", $scope.WireTicket.WireTransferTypeId).trigger("change");
                angular.element("#liSendingAccount").select2("destroy").val("");
                angular.element("#liSendingAccount").select2({
                    placeholder: "Select " + ($scope.wireTicketObj.IsNotice ? "Receiving" : "Sending") + " Account",
                    data: $scope.sendingAccountsList,
                    allowClear: true,
                    closeOnSelect: false
                });
                $scope.isSendingAccountEnabled = true;
                angular.element("#liSendingAccount").select2("val", $scope.WireTicket.OnBoardAccountId).trigger("change");
                angular.element("#liSenderInformation").select2("enable");
                angular.element("#liCollateralPurpose").select2("enable");
                angular.element("#wireSenderDescription").removeAttr("disabled");
            }, 50);
            //$timeout(function () {
            //    if ($scope.wireTicketObj.IsFundTransfer)
            //        angular.element("#liReceivingBookAccount").select2('val', $scope.WireTicket.ReceivingOnBoardAccountId).trigger('change');
            //    else
            //        angular.element("#liReceivingAccount").select2('val', $scope.WireTicket.OnBoardSSITemplateId).trigger('change');
            //}, 50);
        }
        else {
            angular.element("#liMessageType").select2("disable");
            angular.element("#liSenderInformation").select2("disable");
            angular.element("#liCollateralPurpose").select2("disable");
            angular.element("#wireSenderDescription").attr("disabled", "disabled");
        }
        if ($scope.wireObj.WireId == undefined)
            $scope.bindValues();
        else
            $scope.bindWireValues();

        $scope.isWireLoadingInProgress = false;
        $scope.isUserActionDone = false;
        //$scope.fnGetCashBalances();
    }

    $scope.WireTicket = {
        hmsWireId: 0
    }

    angular.element("#uploadWireFiles").dropzone({
        url: "/Home/UploadWireFiles?wireId=" + $scope.WireTicket.hmsWireId,
        dictDefaultMessage: "<span style='font-size:20px;font-weight:normal;font-style:italic'>Drag/Drop wire documents here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".msg,.csv,.txt,.pdf,.xls,.xlsx,.zip,.rar", accept: validateDoubleExtensionInDZ,
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
                this.removeAllFiles();
                notifySuccess("Files Uploaded successfully");
            }
        }
    });

    $scope.viewAttachmentTable = function (data) {

        if ($("#documentTable").hasClass("initialized")) {
            fnDestroyDataTable("#documentTable");
        }
        var isAttachmentsEnabled = ($scope.WireTicketStatus.IsEditEnabled || $scope.WireTicket.WireStatusId == 0);
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
                        var user = $scope.WireTicket.WireStatusId == 0 ? humanizeEmail($("#userName").val()) : $scope.attachmentUsers[rowObj.row];
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
                    "visible": isAttachmentsEnabled,
                    "mRender": function () {
                        return "<button class='btn btn-danger btn-xs' title='Remove Document'><i class='glyphicon glyphicon-remove'></i>&nbsp;Remove</button>";
                    }
                }
            ],
            "deferRender": false,
            "bScrollCollapse": true,
            //scroller: true,
            //sortable: false,
            "searching": false,
            "bInfo": false,
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
                "sEmptyTable": "No files are available for the Wires",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Files"
            }
        });

        $timeout(function () {
            $scope.wireDocumentTable.columns.adjust().draw(true);
        }, 1000);

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
            case "CashBalances": $scope.isCashBalancesCollapsed = !$scope.isCashBalancesCollapsed;
                break;
            case "ReceivingAccount": $scope.isReceivingAccountCollapsed = !$scope.isReceivingAccountCollapsed;
                break;
            case "SSITemplate": $scope.isSSITemplateCollapsed = !$scope.isSSITemplateCollapsed;
                break;
            case "Attachment": $scope.isAttachmentsCollapsed = !$scope.isAttachmentsCollapsed;
                $timeout(function () {
                    if ($scope.wireDocumentTable != undefined)
                        $scope.wireDocumentTable.columns.adjust();
                }, 100);
                break;
            case "Workflow": $scope.isWorkflowLogsCollapsed = !$scope.isWorkflowLogsCollapsed;
                break;
            case "SourceModule": $scope.isWireSourceCollapsed = !$scope.isWireSourceCollapsed;
                break;
            case "SwiftMessage": $scope.isSwiftMessagesCollapsed = !$scope.isSwiftMessagesCollapsed;
                break;
        }
    }

    //$($("#modalToRetrieveWires [data-toggle='collapse']").next()).on("show.bs.collapse", function () {
    //    var target = $(this).prev();
    //    $('#modalToRetrieveWires').animate({ scrollTop: target.offset().top - 50 }, 1000);
    //});

    // Adhoc 
    $scope.getAdhocWireAssociations = function () {
        return $http.get("/Home/GetAdhocWireAssociations").then(function (response) {
            $scope.purposes = response.data.wirePurposes;
            $scope.currencies = response.data.currencies;
        });
    };

    $scope.getHFunds = function () {

        $http.get("/Home/GetAuthorizedFunds").then(function (response) {
            var allDmaFunds = response.data;
            $("#liFund").select2("destroy");
            $("#liFund").select2({
                data: { results: allDmaFunds },
                placeholder: "Select Fund/Group", allowClear: false
            });
        });
    }

    $scope.$watch("isWireRequirementsFilled", function (newValue, oldValue) {
        if ($scope.WireTicket.WireStatusId < 1) {
            if ($scope.wireTicketObj.IsNotice)
                $scope.isWireRequirementsFilled = $("#liFund").select2("val") != "" && $("#liWiresPurpose").select2("val") != "" && $("#liSendingAccount").select2("val") != "";
            else if ($scope.wireTicketObj.IsFundTransfer)
                $scope.isWireRequirementsFilled = $("#liFund").select2("val") != "" && $("#liWiresPurpose").select2("val") != "" && $("#liSendingAccount").select2("val") != "" && $("#liReceivingBookAccount").select2("val") != "" && $("#liMessageType").select2("val") != "";
            else
                $scope.isWireRequirementsFilled = $("#liFund").select2("val") != "" && $("#liWiresPurpose").select2("val") != "" && $("#liSendingAccount").select2("val") != "" && $("#liReceivingAccount").select2("val") != "";
        }
        else if ($scope.WireTicketStatus.IsEditEnabled) {
            if ($scope.wireTicketObj.IsNotice)
                $scope.isWireRequirementsFilled = $("#liSendingAccount").select2("val") != "";
            else if ($scope.wireTicketObj.IsFundTransfer)
                $scope.isWireRequirementsFilled = $("#liSendingAccount").select2("val") != "" && $("#liReceivingBookAccount").select2("val") != "" && $("#liMessageType").select2("val") != "";
            else
                $scope.isWireRequirementsFilled = $("#liSendingAccount").select2("val") != "" && $("#liReceivingAccount").select2("val") != "";
        }
        else
            $scope.isWireRequirementsFilled = true;

    });

    angular.element(document).on("change", "#liWiresPurpose", function () {
        $timeout(function () {
            if ($("#liWiresPurpose").select2("val") != "")
                $scope.wireObj.Purpose = angular.copy($("#liWiresPurpose").select2("data").text);
            $scope.WireTicket.WirePurposeId = angular.copy($("#liWiresPurpose").select2("val"));
            $scope.isWireRequirementsFilled = !$scope.isWireRequirementsFilled;
            if (!$scope.isWireLoadingInProgress)
                $scope.checkForCreatedWires();

        }, 50);
    });
    angular.element(document).on("change", "#liFund", function () {
        $timeout(function () {
            if ($("#liFund").select2("val") != "") {
                $scope.isFundsChanged = true;
                $http.get("/Home/GetApprovedAccountsForFund?fundId=" + $("#liFund").select2("val") + "&wireTransferType=" + $("#liWireTransferType").val()).then(function (response) {
                    $scope.sendingAccountsListOfFund = response.data.sendingAccountsList;
                    $scope.receivingAccountsListOfFund = response.data.receivingAccountsList;
                    $scope.currencies = response.data.currencies;

                    angular.element("#liCurrency").select2({
                        placeholder: "Select Currency",
                        data: $scope.currencies,
                        allowClear: true,
                        closeOnSelect: false
                    });

                    //var currency = $scope.currencies.length == 0 ? null : $scope.currencies[0].id;

                    //var isUsdAvailable = false;
                    //$.each($scope.currencies,
                    //    function (i, v) {
                    //        if (v.id == "USD")
                    //            isUsdAvailable = true;
                    //    });

                    // $("#liCurrency").select2('val', isUsdAvailable ? "USD" : currency).trigger('change');
                    $scope.isSendingAccountEnabled = true;
                });
            }
            else {
                $scope.isFundsChanged = false;
                $("#liAgreement").select2("val", "").trigger("change");
                $("#liSendingAccount").select2("val", "").trigger("change");
                $("#liReceivingBookAccount").select2("val", "").trigger("change");
                $("#liReceivingAccount").select2("val", "").trigger("change");
            }
        }, 50);
    });

    angular.element(document).on("change", "#liCurrency", function () {
        $timeout(function () {
            if ($("#liCurrency").select2("val") != "" && $scope.WireTicket.hmsWireId == 0) {
                $scope.sendingAccountsList = $filter("filter")(angular.copy($scope.sendingAccountsListOfFund), { 'Currency': $("#liCurrency").select2("val") }, true);
                angular.element("#liSendingAccount").select2({
                    placeholder: "Select " + ($scope.wireTicketObj.IsNotice ? "Receiving" : "Sending") + " Account",
                    data: $scope.sendingAccountsList,
                    allowClear: true,
                    closeOnSelect: false
                });
                $scope.isSendingAccountEnabled = true;
            }
            else if ($scope.WireTicket.hmsWireId == 0) {
                $scope.isFundsChanged = false;
                $("#liAgreement").select2("val", "").trigger("change");
                $("#liSendingAccount").select2("val", "").trigger("change");
                $("#liReceivingBookAccount").select2("val", "").trigger("change");
                $("#liReceivingAccount").select2("val", "").trigger("change");
            }
        }, 50);
    });

    angular.element(document).on("change", "#liSendingAccount", function () {
        $timeout(function () {
            $scope.WireTicket.OnBoardAccountId = angular.copy($("#liSendingAccount").select2("val"));
            if ($("#liSendingAccount").select2("val") != "") {
                if ($scope.WireTicketStatus.IsWirePurposeAdhoc && !$scope.wireTicketObj.IsNotice) {
                    if (!$scope.wireTicketObj.IsFundTransfer) {
                        $http.get("/Home/GetApprovedSSITemplatesForAccount?accountId=" + $scope.WireTicket.OnBoardAccountId + "&isNormalTransfer=" + $scope.IsNormalTransfer).then(function (response) {
                            $scope.receivingAccounts = response.data.receivingAccounts;
                            $scope.receivingAccountList = response.data.receivingAccountList;
                            $scope.counterParties = response.data.counterparties;
                            angular.element("#liReceivingAccount").select2("destroy");
                            angular.element("#liReceivingAccount").select2({
                                placeholder: "Select Receiving Account",
                                data: $scope.receivingAccountList,
                                allowClear: true,
                                closeOnSelect: false
                            });

                            $scope.isReceivingAccountEnabled = true;
                            $scope.WireTicketStatus.ShouldEnableCollateralPurpose = response.data.shouldEnableCollateralPurpose;

                            if ($scope.WireTicket.hmsWireId > 0)
                                angular.element("#liReceivingAccount").select2("val", $scope.WireTicket.OnBoardSSITemplateId).trigger("change");
                        });
                    }
                    else {
                        $scope.receivingBookAccountList = $filter("filter")(angular.copy($scope.receivingAccountsListOfFund), function (acc) {
                            return acc.id != $scope.WireTicket.OnBoardAccountId && acc.Currency == $("#liCurrency").select2("val");
                        }, true);

                        angular.element("#liReceivingBookAccount").select2("destroy");
                        angular.element("#liReceivingBookAccount").select2({
                            placeholder: "Select Receiving Account",
                            data: $scope.receivingBookAccountList,
                            allowClear: true,
                            closeOnSelect: false
                        });
                        $scope.isReceivingAccountEnabled = true;
                        if ($scope.WireTicket.hmsWireId > 0)
                            angular.element("#liReceivingBookAccount").select2("val", $scope.WireTicket.ReceivingOnBoardAccountId).trigger("change");
                    }
                }
                //var account = $filter('filter')(angular.copy($scope.sendingAccounts), function (account) {
                //    return account.onBoardingAccountId == $scope.WireTicket.OnBoardAccountId;
                //}, true)[0];

                $http.get("/Home/GetFundAccount?onBoardingAccountId=" + $scope.WireTicket.OnBoardAccountId + "&valueDate=" + $("#wireValueDate").text()).then(function (response) {

                    var account = response.data.onboardAccount;
                    $scope.fnSetWireCutOffDetails(response.data.deadlineToApprove, response.data.IsWireCutOffApproved);

                    $scope.castToDate(account);
                    //$scope.getApprovalTime(account);
                    $scope.accountDetail = account;
                    if ($scope.wireTicketObj.IsNotice) {
                        $scope.wireTicketObj.ReceivingAccountCurrency = $scope.accountDetail.Currency;
                        var wireMessageType = $filter("filter")($scope.MessageTypes, function (message) { return message.text == "MT210" }, true)[0];
                        angular.element("#liMessageType").select2("val", wireMessageType.id).trigger("change");
                    }
                    $scope.isValidWireInitiation = $scope.validateWireInitiationofBIC();
                });
            }
            else {
                $scope.isReceivingAccountEnabled = false;
                $scope.accountDetail = angular.copy($scope.wireTicketObj.SendingAccount);
                $interval.cancel($scope.promise);
                $scope.timeToShow = "00 : 00 : 00";
                angular.element("#liReceivingBookAccount").select2("val", "").trigger("change");
                angular.element("#liReceivingAccount").select2("val", "").trigger("change");
            }

            $scope.isWireRequirementsFilled = !$scope.isWireRequirementsFilled;
            $scope.fnGetCashBalances($("#wireValueDate").text());
        }, 50);
    });

    $scope.CashBalance = { IsCashBalanceAvailable: false };
    $scope.fnGetCashBalances = function (valueDate) {

        $scope.CashBalance = {};
        $scope.CashBalance.IsCashBalanceAvailable = false;
        $scope.CashBalance.CalculatedBalance = "N.A";
        $scope.CashBalance.IsNewBalanceOffLimit = false;

        if ($scope.WireTicket.OnBoardAccountId == undefined || $scope.WireTicket.OnBoardAccountId == 0)
            return;

        $http.get("/Home/GetCashBalances?sendingAccountId=" + $scope.WireTicket.OnBoardAccountId + "&valueDate=" + valueDate)
            .then(function (response) {
                $scope.CashBalance = response.data;
                $scope.CashBalance.TreasuryBalance = $.convertToCurrency($scope.CashBalance.TreasuryBalance, 2);
                $scope.CashBalance.TotalWireEntered = $.convertToCurrency($scope.CashBalance.TotalWireEntered, 2);
                $scope.CashBalance.AvailableBalance = !$scope.CashBalance.IsCashBalanceAvailable ? "N.A" : $.convertToCurrency($scope.CashBalance.AvailableBalance, 2);

                $($scope.CashBalance.WireDetails).each(function (i, v) {
                    v.ValueDate = moment(v.ValueDate).format("YYYY-MM-DD");
                    v.ApprovedWireAmount = $.convertToCurrency(v.ApprovedWireAmount, 2);
                    v.PendingWireAmount = $.convertToCurrency(v.PendingWireAmount, 2);
                    v.TotalWireEntered = $.convertToCurrency(v.TotalWireEntered, 2);
                });

                $timeout($scope.fnCalculateCashBalance(), 300);
            });
    }

    $scope.fnCalculateCashBalance = function (isRetry) {

        $scope.CashBalance.IsNewBalanceOffLimit = false;
        $("#chkCashBalOff").prop("checked", false).trigger("change");
        if (!$scope.CashBalance.IsCashBalanceAvailable)
            $scope.CashBalance.CalculatedBalance = "N.A";
        else {
            var newBalance = $.convertToNumber($scope.CashBalance.AvailableBalance) - $.convertToNumber($("#wireAmount").text());
            $scope.CashBalance.CalculatedBalance = $.convertToCurrency(newBalance, 2);
            $scope.CashBalance.IsNewBalanceOffLimit = newBalance < 0;

            if (($scope.CashBalance.CalculatedBalance == "NaN.00" || $scope.CashBalance.CalculatedBalance == "N.A") && !isRetry)
                $timeout($scope.fnCalculateCashBalance(true), 300);
        }
    };


    angular.element(document).on("change", "#liReceivingBookAccount", function () {
        $timeout(function () {
            $scope.WireTicket.ReceivingOnBoardAccountId = angular.copy($("#liReceivingBookAccount").select2("val"));
            if ($scope.WireTicket.ReceivingOnBoardAccountId != "") {
                if ($scope.wireTicketObj.IsFundTransfer) {

                    $http.get("/Home/GetFundAccount?onBoardingAccountId=" + $scope.WireTicket.ReceivingOnBoardAccountId + "&valueDate=" + $("#wireValueDate").text()).then(function (response) {

                        var receivingAccount = response.data.onboardAccount;

                        //var receivingAccount = $filter('filter')(angular.copy($scope.sendingAccounts), function (template) {
                        //    return template.onBoardingAccountId == $scope.WireTicket.ReceivingOnBoardAccountId;
                        //}, true)[0];


                        $scope.castToDate(receivingAccount);
                        $scope.receivingAccountDetail = receivingAccount;
                        $scope.wireTicketObj.ReceivingAccountCurrency = angular.copy($scope.receivingAccountDetail.Currency);
                        if (!$scope.isWireLoadingInProgress && $scope.WireTicket.hmsWireId == 0)
                            $scope.checkForCreatedWires();
                        if (!$scope.wireTicketObj.IsFundTransfer) {
                            //angular.element("#liDeliveryCharges").select2("disable");
                            angular.element("#liMessageType").select2("disable");
                        }
                        else {
                            //angular.element("#liDeliveryCharges").select2("enable");
                            angular.element("#liMessageType").select2("enable");
                        }
                        if ($scope.WireTicket.hmsWireId == 0 || $scope.WireTicketStatus.IsEditEnabled) {
                            angular.element("#liSenderInformation").select2("enable");
                            angular.element("#liCollateralPurpose").select2("enable");
                            angular.element("#wireSenderDescription").removeAttr("disabled");
                        }
                        else {
                            angular.element("#liSenderInformation").select2("disable");
                            angular.element("#liCollateralPurpose").select2("disable");
                            angular.element("#wireSenderDescription").attr("disabled", "disabled");
                        }
                    });
                }
            }
            else {
                $scope.receivingAccountDetail = angular.copy($scope.wireTicketObj.ReceivingAccount);
            }



            $scope.isWireRequirementsFilled = !$scope.isWireRequirementsFilled;
        }, 50);
    });

    angular.element(document).on("change", "#liReceivingAccount", function () {
        $timeout(function () {
            $scope.WireTicket.OnBoardSSITemplateId = angular.copy($("#liReceivingAccount").select2("val"));
            if ($scope.WireTicket.OnBoardSSITemplateId != "") {
                if (!$scope.wireTicketObj.IsFundTransfer) {
                    $scope.ssiTemplate = $filter("filter")($scope.receivingAccounts, function (template) {
                        return template.onBoardingSSITemplateId == $scope.WireTicket.OnBoardSSITemplateId;
                    }, true)[0];
                    $scope.wireTicketObj.ReceivingAccountCurrency = angular.copy($scope.ssiTemplate.Currency);
                    $scope.wireTicketObj.Counterparty = $scope.counterParties[$scope.ssiTemplate.TemplateEntityId];
                    var wireMessageType = $filter("filter")($scope.MessageTypes, function (message) { return message.text == $scope.ssiTemplate.MessageType }, true)[0];
                    angular.element("#liMessageType").select2("val", wireMessageType.id).trigger("change");
                    angular.element("#liDeliveryCharges").select2("val", ($scope.ssiTemplate.MessageType != "MT103" ? null : "OUR")).trigger("change");
                    if (!$scope.isWireLoadingInProgress && $scope.WireTicket.hmsWireId == 0)
                        $scope.checkForCreatedWires();
                    if (!$scope.wireTicketObj.IsFundTransfer) {
                        //angular.element("#liDeliveryCharges").select2("disable");
                        angular.element("#liMessageType").select2("disable");
                    }
                    else {
                        // angular.element("#liDeliveryCharges").select2("enable");
                        angular.element("#liMessageType").select2("enable");
                    }
                    if ($scope.WireTicket.hmsWireId == 0 || $scope.WireTicketStatus.IsEditEnabled) {
                        angular.element("#liSenderInformation").select2("enable");
                        angular.element("#liCollateralPurpose").select2("enable");
                        angular.element("#wireSenderDescription").removeAttr("disabled");
                    }
                    else {
                        angular.element("#liSenderInformation").select2("disable");
                        angular.element("#liCollateralPurpose").select2("disable");
                        angular.element("#wireSenderDescription").attr("disabled", "disabled");
                    }
                }
            }
            else {
                angular.element("#liMessageType").select2("val", "").trigger("change");
                $scope.ssiTemplate = angular.copy($scope.wireTicketObj.SSITemplate);
            }

            $scope.isWireRequirementsFilled = !$scope.isWireRequirementsFilled;
        }, 50);
    });

    $(document).on("change", "#liWireTransferType", function (event) {
        $timeout(function () {
            $scope.WireTicket.WireTransferTypeId = angular.copy($("#liWireTransferType").select2("val"));
            $scope.wireTicketObj.IsFundTransfer = $scope.WireTicket.WireTransferTypeId == 2;
            $scope.IsNormalTransfer = $scope.WireTicket.WireTransferTypeId == 1;
            $scope.wireTicketObj.IsNotice = $scope.WireTicket.WireTransferTypeId == 4;

            if ($scope.wireTicketObj.IsNotice)
                $("#wireValueDate").datepicker("setStartDate", "-3d");
            else
                $("#wireValueDate").datepicker("setStartDate", "+0d");

            $("#liWiresPurpose").select2("val", "").trigger("change");
            $("#liFund").select2("val", "").trigger("change");
            $("#liCurrency").select2("val", "").trigger("change");
            $("#liAgreement").select2("val", "").trigger("change");
            $("#liSendingAccount").select2("val", "").trigger("change");

            $timeout(function () {
                if ($scope.wireTicketObj.IsNotice)
                    $("#s2id_liSendingAccount .select2-chosen").html("Select Receiving Account");
                else
                    $("#s2id_liSendingAccount .select2-chosen").html("Select Sending Account");
            }, 501);



            $("#liReceivingBookAccount").select2("val", "").trigger("change");
            $("#liReceivingAccount").select2("val", "").trigger("change");
            if ($scope.wireTicketObj.IsFundTransfer)
                $scope.filteredMessageTypes = $filter("filter")(angular.copy($scope.MessageTypes), function (message) { return (message.text == "MT103" || message.text == "MT202") }, true);
            else
                $scope.filteredMessageTypes = angular.copy(angular.copy($scope.MessageTypes));
            angular.element("#liMessageType").select2({
                placeholder: "Select Message Type",
                data: $scope.filteredMessageTypes,
                allowClear: true,
                closeOnSelect: false
            });
            $("#liMessageType").select2("val", "").trigger("change");
            $("#liSenderInformation").select2("val", "").trigger("change");
            $("#liCollateralPurpose").select2("val", "").trigger("change");
            $scope.wireTicketObj.IsSenderInformationRequired = false;
        }, 500);


    });

    $scope.auditWireLogs = function (statusId) {
        var auditLogData = createWireAuditData(statusId);
        $http.post("/Audit/AuditWireLogs", JSON.stringify({ auditLogData: auditLogData }), { headers: { 'Content-Type': "application/json; charset=utf-8;" } }).then(function (response) {
        });
    }

    $scope.getTransferType = function () {
        if ($scope.WireTicket.hmsWireId > 0) {
            return $scope.wireTicketObj.TransferType;
        }
        else if ($scope.WireTicketStatus.IsWirePurposeAdhoc) {
            return $("#liWireTransferType").select2("data").text;
        }
        else {
            return $scope.wireObj.Purpose == "Send Call" ? "Notice" : "3rd Party Transfer";
        }
    }

    function createWireAuditData(statusId) {
        var changes = new Array();
        var index = 0;
        var action = $scope.WireTicket.hmsWireId == 0 ? "Added" : "Edited";
        if ($scope.WireTicket.WireStatusId == 1) {
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
        auditdata.ModuleName = $scope.wireObj.Report;
        auditdata.Action = action;
        auditdata.Changes = changes;
        auditdata.AgreementName = "";
        auditdata.SendingAccount = $scope.accountDetail.AccountName;
        auditdata.IsNormalTransfer = $scope.IsNormalTransfer;
        auditdata.IsFundTransfer = $scope.WireTicket.IsFundTransfer;
        auditdata.TransferType = $scope.getTransferType();
        auditdata.ReceivingAccount = $scope.wireTicketObj.IsNotice ? "" : auditdata.IsFundTransfer ? $scope.receivingAccountDetail.AccountName : $scope.ssiTemplate.TemplateName;
        auditdata.Purpose = $scope.wireObj.Purpose;
        auditdata.AssociationId = $scope.WireTicket.hmsWireId;
        return auditdata;
    }

});
