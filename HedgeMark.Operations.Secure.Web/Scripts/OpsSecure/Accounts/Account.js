$("#liAccounts").addClass("active");
HmOpsApp.controller("AccountCtrl", function ($scope, $http, $timeout, $filter, $q) {
    $("#onboardingMenu").addClass("active");
    $scope.$on("onRepeatLast", function (scope, element, attrs) {
        $timeout(function () {
            $scope.fnIntialize();
        }, 100);
    });

    var pendingStatus = "Pending Approval";
    var ssiMapTable = [];
    var accountDocumentTable = [];
    var contactTable = [];
    $scope.accountDetail = {};
    $scope.onBoardingAccountDetails = [];
    $scope.onBoardingAccountDetail = {};
    $scope.onBoardingAccountSSITemplateMap = {};
    $scope.agreements = [];
    $scope.agreementTypes = [];
    $scope.ssiTemplates = [];
    $scope.agreementTypeId = 0;
    $scope.allAccounts = [];
    $scope.SwiftGroups = [];
    $scope.SwiftGroupData = [];

    $scope.isBicorAba = false;
    $scope.BicorAba = "";
    $scope.AccountDescriptions = [];
    $scope.OnBoardingContactsDetails = [];
    $scope.cashSweepData = [{ id: "Yes", text: "Yes" }, { id: "No", text: "No" }];
    $scope.beneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }];
    $scope.ultimateBeneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }, { id: "Account Name", text: "Account Name" }];
    $scope.ContactType = [{ id: "Cash", text: "Cash" }, { id: "Custody", text: "Custody" }, { id: "PB Client Service", text: "PB Client Service" }, { id: "Margin", text: "Margin" }];
    //$scope.authorizedPartyData = [{ id: "Hedgemark", text: "Hedgemark" }, { id: "Administrator", text: "Administrator" }, { id: "Counterparty", text: "Counterparty" }, { id: "Client", text: "Client" }, { id: "Investment Manager", text: "Investment Manager" }];
    $scope.authorizedPartyData = [];
    $scope.cashSweepTimeZoneData = [{ id: "EST", text: "EST" }, { id: "GMT", text: "GMT" }, { id: "CET", text: "CET" }];
    $scope.entityTypes = [{ id: "Agreement", text: "Agreement" }, { id: "DDA", text: "DDA" }, { id: "Custody", text: "Custody" }];
    $scope.accountPurpose = [];
    $scope.accountStatus = [{ id: "Requested", text: "Requested" }, { id: "Reserved", text: "Reserved" }, { id: "Open", text: "Open" }, { id: "Requested Closure", text: "Requested Closure" }, { id: "Closed", text: "Closed" }];
    $scope.fundName = "";
    $scope.counterpartyFamilyId = 0;
    $scope.accountBicorAba = [];
    $scope.contactNames = [];
    $scope.cusodyAccountData = [];
    $scope.broker = "";
    $scope.agreementName = "";
    // var documentData = "\"FileName\": \"\",\"RecCreatedBy\": \"\",\"RecCreatedAt\": \"\"";


    $scope.fnGetAllAccounts = function () {
        $http.get("/Accounts/GetAllAccounts").then(function (response) {
            $scope.allAccounts = response.data.accounts;
            $scope.cusodyAccountData = response.data.custodyAccounts;
        });
    }

    $scope.fnGetAllAccounts();

    function viewAttachmentTable(data, key) {

        if ($("#documentTable" + key).hasClass("initialized")) {
            fnDestroyDataTable("#documentTable" + key);
        }
        accountDocumentTable[key] = $("#documentTable" + key).not(".initialized").addClass("initialized").DataTable({
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
                    "mData": "RecCreatedBy",
                    "mRender": function (data) {
                        return humanizeEmail(data);
                    }
                },
                {
                    "sTitle": "Uploaded At",
                    "mData": "RecCreatedAt",
                    "type": "dotnet-date",
                    "mRender": function (tdata) {
                        return "<div title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + (moment(tdata).fromNow()) + "</div>";
                    }
                },
                {
                    "mData": "onBoardingAccountDocumentId",
                    "sTitle": "Remove", "className": "dt-center",
                    "mRender": function () {
                        return "<button class='btn btn-danger btn-xs' title='Remove Document' id='" + key + "'><i class='glyphicon glyphicon-trash'></i></button>";
                    }
                }
            ],
            "deferRender": false,
            "bScrollCollapse": true,
            //scroller: true,
            //sortable: false,
            //"sDom": "ift",
            //pagination: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "scrollY": 350,
            "order": [[2, "desc"]],

            "fnRowCallback": function (nRow, aData) {
                if (aData.FileName != "") {
                    $("td:eq(0)", nRow).html("<a title ='click to download the file' href='/Accounts/DownloadAccountFile?fileName=" + getFormattedFileName(aData.FileName) + "&accountId=" + $scope.onBoardingAccountDetails[key].onBoardingAccountId + "'>" + aData.FileName + "</a>");
                }
            },
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No files are available for the ssi templates",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Files"
            }
        });

        window.setTimeout(function () {
            accountDocumentTable[key].columns.adjust().draw(true);
        }, 50);

        $("#accountDetailCP tbody tr td:last-child button").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowIndex = $(this).attr("id");
            var rowElement = accountDocumentTable[rowIndex].row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this document from account?", function (result) {
                if (!result) {
                    return;
                } else {
                    if (rowElement.onBoardingAccountDocumentId > 0) {
                        $http.post("/Accounts/RemoveAccountDocument", { fileName: rowElement.FileName, documentId: rowElement.onBoardingAccountDocumentId }).then(function () {
                            accountDocumentTable[rowIndex].row(selectedRow).remove().draw();
                            $scope.onBoardingAccountDetails[rowIndex].onBoardingAccountDocuments.pop(rowElement);
                            notifySuccess("Account document has removed succesfully");
                        });
                    } else {
                        accountDocumentTable[rowIndex].row(selectedRow).remove().draw();
                        $scope.onBoardingAccountDetails[rowIndex].onBoardingAccountDocuments.pop(rowElement);
                        notifySuccess("Account document has removed succesfully");
                    }
                }
            });
        });
    }

    function viewContactTable(data, key) {

        if ($("#contactTable" + key).hasClass("initialized")) {
            fnDestroyDataTable("#contactTable" + key);
        }
        contactTable[key] = $("#contactTable" + key).not(".initialized").addClass("initialized").DataTable({
            "bDestroy": true,
            // responsive: true,
            aaData: data,
            "aoColumns": [
                { "mData": "id", "sTitle": "id", visible: false },
                {
                    "mData": "ContactType",
                    "sTitle": "Email Type",
                    "mRender": function (tdata) {
                        if (tdata === "Individual")
                            return "<label class=\"label ng-show-only shadowbox label-default\" style=\"font-size: 12px;\">Individual</label>";
                        if (tdata === "Group")
                            return "<label class=\"label ng-show-only shadowbox label-info\" style=\"font-size: 12px;\">Group</label>";
                        return "";
                    }
                },
                  { "mData": "JobTitle", "sTitle": "Group / Job Function" },
                  { "mData": "name", "sTitle": "Name" },
                  //{ "mData": "LastName", "sTitle": "LastName" },
                  { "mData": "Email", "sTitle": "Email" },
                  { "mData": "BusinessPhone", "sTitle": "Business Phone" },
                  { "mData": "Notes", "sTitle": "Notes" },
                  { "mData": "wires", "sTitle": "Wires" },
                  { "mData": "margin", "sTitle": "Margin" },
                  { "mData": "cash", "sTitle": "Cash" },
                  { "mData": "collateral", "sTitle": "Collateral" },
                  { "mData": "Interest", "sTitle": "Interest" }
            ],
            "deferRender": false,
            "bScrollCollapse": true,
            //scroller: true,
            //sortable: false,
            //"sDom": "ift",
            //pagination: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "scrollY": 350,
            "order": [[3, "desc"]],
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No contact are available for the account",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Contacts"
            }
        });

        window.setTimeout(function () {
            contactTable[key].columns.adjust().draw(true);
        }, 50);
    }

    $scope.fnGetCurrency = function (panelIndex) {
        $http.get("/Accounts/GetAllCurrencies").then(function (response) {
            $scope.currencies = response.data.currencies;

            if (agreementId > 0 && panelIndex != undefined) {
                $("#liCurrency" + panelIndex).select2({
                    placeholder: "Select a Currency",
                    allowClear: true,
                    data: response.data.currencies
                });

                if ($scope.onBoardingAccountDetails[panelIndex].Currency != null && $scope.onBoardingAccountDetails[panelIndex].Currency != 'undefined')
                    $("#liCurrency" + panelIndex).select2("val", $scope.onBoardingAccountDetails[panelIndex].Currency);
            }
        });
    }

    $scope.fnGetCashInstruction = function (panelIndex) {
        $http.get("/Accounts/GetAllCashInstruction").then(function (response) {
            $scope.cashInstructions = response.data.cashInstructions;
            $scope.timeZones = response.data.timeZones;
            if (panelIndex != undefined && panelIndex != null) {
                $("#liCashInstruction" + panelIndex).select2({
                    placeholder: "Select a Cash Instruction",
                    allowClear: true,
                    data: response.data.cashInstructions
                });

                //$("#liTimeZone" + panelIndex).select2({
                //    placeholder: "Select a TimeZone",
                //    allowClear: true,
                //    data: response.data.timeZones
                //});

                if ($scope.onBoardingAccountDetails[panelIndex].CashInstruction != null && $scope.onBoardingAccountDetails[panelIndex].CashInstruction != 'undefined')
                    $("#liCashInstruction" + panelIndex).select2("val", $scope.onBoardingAccountDetails[panelIndex].CashInstruction);

                //if ($scope.onBoardingAccountDetails[panelIndex].CutOffTimeZone != null && $scope.onBoardingAccountDetails[panelIndex].CutOffTimeZone != 'undefined')
                //    $("#liTimeZone" + panelIndex).select2("val", $scope.onBoardingAccountDetails[panelIndex].CutOffTimeZone);
            }
        });
    }

    $scope.fnGetAuthorizedParty = function (panelIndex) {
        $http.get("/Accounts/GetAllAuthorizedParty").then(function (response) {
            $scope.authorizedPartyData = response.data.AuthorizedParties;

            if (agreementId > 0 && panelIndex != undefined) {
                $("#liAuthorizedParty" + panelIndex).select2({
                    placeholder: "Select a Authorized Party",
                    allowClear: true,
                    data: response.data.AuthorizedParties
                });

                if ($scope.onBoardingAccountDetails[panelIndex].AuthorizedParty != null && $scope.onBoardingAccountDetails[panelIndex].AuthorizedParty != 'undefined')
                    $("#liAuthorizedParty" + panelIndex).select2("val", $scope.onBoardingAccountDetails[panelIndex].AuthorizedParty);
            }
        });
    }

    $scope.fnGetSwiftGroup = function (panelIndex) {
        $http.get("/Accounts/GetAllSwiftGroup").then(function (response) {
            $scope.SwiftGroups = response.data.swiftGroups;
            $scope.SwiftGroupData = response.data.SwiftGroupData;

            if (agreementId > 0 && panelIndex != undefined) {
                $("#liSwiftGroup" + panelIndex).select2({
                    placeholder: "Select a Swift Group",
                    allowClear: true,
                    data: response.data.SwiftGroupData
                });

                if ($scope.onBoardingAccountDetails[panelIndex].SwiftGroup != null && $scope.onBoardingAccountDetails[panelIndex].SwiftGroup != 'undefined')
                    $("#liSwiftGroup" + panelIndex).select2("val", $scope.onBoardingAccountDetails[panelIndex].SwiftGroup);
            }
        });
    }

    $scope.fnGetBicorAba = function (panelIndex) {
        $http.get("/Accounts/GetAllAccountBicorAba").then(function (response) {
            $scope.accountBicorAba = response.data.accountBicorAba;
            if (panelIndex != null) {
                var isAba = $scope.isBicorAba == true ? "ABA" : "BIC";
                $scope.fnToggleBeneficiaryBICorABA(isAba, 'Beneficiary', panelIndex);
                $("#liBeneficiaryBICorABA" + panelIndex).select2("val", isAba);
            }
        });
    }

    $scope.fnAddAccountDetail = function () {

        $scope.copyAccount = angular.copy($scope.onBoardingAccountDetail);
        if ($scope.onBoardingAccountDetails == undefined || $scope.onBoardingAccountDetails == null) {
            $scope.onBoardingAccountDetails = [];
        }

        $scope.copyAccount.onBoardingAccountId = 0;
        $scope.copyAccount.AccountType = accountType;
        $scope.copyAccount.AccountName = $scope.fundName;

        if (accountType == "Agreement") {
            $scope.copyAccount.dmaAgreementOnBoardingId = agreementId;
        }

        $scope.copyAccount.hmFundId = fundId;
        $scope.copyAccount.BrokerId = brokerId;
        $scope.copyAccount.onBoardingAccountSSITemplateMaps = [];
        $scope.copyAccount.onBoardingAccountDocuments = [];
        $scope.copyAccount.IsReceivingAccountType = accountType == "Agreement" && $.inArray($scope.agreementType, ["FCM", "CDA", "ISDA", "GMRA", "MRA", "MSFTA", "FXPB"]) > -1;
        if (account.IsReceivingAccountType || account.AuthorizedParty != "Hedgemark")
            account.IsReceivingAccount = true;
        else
            account.IsReceivingAccount = false;
        $scope.onBoardingAccountDetails.push($scope.copyAccount);
    }

    $scope.fnAssignAccountDetails = function () {

        $http.get("/Accounts/GetAllOnBoardingAccounts?accountType=" + accountType + "&agreementId=" + agreementId + "&fundId=" + fundId + "&brokerId=" + brokerId).then(function (response) {
            $scope.onBoardingAccountDetails = response.data.OnBoardingAccounts;
            $scope.fundName = response.data.legalFundName;
            $scope.counterpartyFamilyId = response.data.counterpartyFamilyId;
            $scope.accountDescriptions = response.data.accountDescriptions;
            $scope.accountModules = response.data.accountModules;
            $scope.accountReports = response.data.accountReports;
            $scope.ssiTemplates = response.data.ssiTemplates;
            $scope.agreementTypeId = response.data.agreementTypeId;
            $scope.agreementType = response.data.agreementType;
            $scope.broker = response.data.broker;
            $scope.authorizedPartyData = response.data.authorizedParties;
            $scope.SwiftGroups = response.data.swiftGroups;
            $scope.SwiftGroupData = response.data.SwiftGroupData;
            $scope.contactNames = [];
            $scope.OnBoardingContactsDetails = [];
            brokerId = response.data.counterpartyFamilyId;
            fundId = response.data.fundId;


            if (response.data.OnBoardingContacts.length > 0) {
                $.each(response.data.OnBoardingContacts, function (i, v) {
                    $scope.contactNames.push({ id: v.id, text: v.name });
                    $scope.OnBoardingContactsDetails.push(v);
                });
            }

            angular.forEach($scope.onBoardingAccountDetails, function (val, ind) {
                if (val.CashSweepTime != null && val.CashSweepTime != "" && val.CashSweepTime != undefined) {
                    var times = val.CashSweepTime.split(':');
                    val.CashSweepTime = new Date(2014, 0, 1, times[0], times[1], times[2]);
                }
                if (val.CutoffTime != null && val.CutoffTime != "" && val.CutoffTime != undefined) {
                    var cutoffTimes = val.CutoffTime.split(':');
                    val.CutoffTime = new Date(2014, 0, 1, cutoffTimes[0], cutoffTimes[1], cutoffTimes[2]);
                }
                //if (val.CashSweepTime != null && val.CashSweepTime != "" && val.CashSweepTime != undefined) {
                //    //var times = account.CashSweepTime.split(':');
                //    val.CashSweepTime = new Date(2014, 0, 1, val.CashSweepTime.Hours, val.CashSweepTime.Minutes, val.CashSweepTime.Seconds);

                //}
                //if (val.CutoffTime != null && val.CutoffTime != "" && val.CutoffTime != undefined) {
                //    //var cutoffTimes = account.CutoffTime.split(':');
                //    val.CutoffTime = new Date(2014, 0, 1, val.CutoffTime.Hours, val.CutoffTime.Minutes, val.CutoffTime.Seconds);
                //}
                val.BrokerId = $scope.counterpartyFamilyId;
            });

            if ($scope.onBoardingAccountDetails.length === 0) $scope.fnAddAccountDetail();
            if (accountType == "Agreement") {
                $("#agrName").show();
                $scope.agreementName = response.data.agreementName;
                $("#agrName").html(response.data.agreementName);
            }
            if (response.data.agreementType == "PB" || response.data.agreementType == "FCM" || accountType == "DDA") {
                $scope.accountPurpose = [{ id: "Cash", text: "Cash" }, { id: "Margin", text: "Margin" }];
            } else {
                $scope.accountPurpose = [{ id: "Pledge Account", text: "Pledge Account" }, { id: "Return Account", text: "Return Account" }, { id: "Both", text: "Both" }];
            }
            //if (accountType == "DDA")
            //    $scope.accountPurpose = [{ id: "Cash", text: "Cash" }, { id: "Margin", text: "Margin" }];

            $("#liAccountReport").select2({
                placeholder: "Select Modules",
                data: $scope.accountReports
            });
            $("#liAccountReport").select2('val', $scope.accountReports[0].id);

            $scope.fnIntialize();
        });
    }

    $scope.fnGetAccounts = function () {

        if (agreementId !== "0" && agreementId !== 0) {
            $("#basicAccountDetail").hide();
            $("#plnAddAccount").show();
            $scope.fnAssignAccountDetails();
        }
        else if (accountType != "" && accountType != "Agreement" && fundId !== "0" && fundId !== 0 && brokerId !== "0" && brokerId !== 0) {
            $("#basicAccountDetail").hide();
            $("#plnAddAccount").show();
            $scope.fnAssignAccountDetails();
        }
        else {
            $("#plnAddAccount").hide();
            $("#basicAccountDetail").show();
            $("#agrName").hide();
            $http.get("/Accounts/GetAccountPreloadData").then(function (response) {
                $scope.funds = response.data.funds;
                $scope.agreements = response.data.agreements;
                $scope.counterpartyFamilies = response.data.counterpartyFamilies;

                $("#liAccountType").select2({
                    placeholder: "Select a entity type",
                    allowClear: true,
                    data: $scope.entityTypes
                });

                $("#liFund").select2({
                    placeholder: "Select a fund",
                    allowClear: true,
                    data: $scope.funds
                });

                $("#liBroker").select2({
                    placeholder: "Select a broker",
                    allowClear: true,
                    data: $scope.counterpartyFamilies
                });

                $("#liAgreement").select2({
                    placeholder: "Select an agreement",
                    allowClear: true,
                    data: []
                });
            });
        }
    }

    var initAccount = function () {
        $q.all([$scope.fnGetBicorAba(null), $scope.fnGetCurrency(), $scope.fnGetCashInstruction()]).then($scope.fnGetAccounts);
    }

    initAccount();

    $scope.fnBack = function () {
        var searchText = getUrlParameter("searchText");
        searchText = (searchText == undefined || searchText == 'undefined') ? "" : searchText;
        window.location.href = "/Accounts/Index?searchText=" + searchText;
    }

    $("#liAccountType").change(function () {
        accountType = $(this).val();
        if ($(this).val() != "" && $(this).val() != undefined) {
            if ($(this).val() == "Agreement") {
                $("#spnBroker").hide();
                $("#spnAgreement").show();
            } else {
                $("#spnBroker").show();
                $("#spnAgreement").hide();
            }
        } else {
            $("#spnBroker").hide();
            $("#spnAgreement").hide();
        }
    });

    $("#liFund").change(function () {

        fundId = $(this).val();
        if (fundId > 0) {

            var agreements = $.grep($scope.agreements, function (v) { return v.hmFundId == fundId; });
            var agreementData = [];
            $.each(agreements, function (key, value) {
                agreementData.push({ "id": value.AgreementOnboardingId, "text": value.AgreementShortName });
            });

            agreementData = $filter('orderBy')(agreementData, 'text');

            if ($("#liAgreement").data("select2")) {
                $("#liAgreement").select2("destroy");
            }

            $("#liAgreement").select2({
                placeholder: "Select the agreements",
                allowClear: true,
                data: agreementData
            });
        }
        else {

            if ($("#liAgreement").data("select2")) {
                $("#liAgreement").select2("destroy");
            }
            $("#liAgreement").select2({
                placeholder: "Select the agreements",
                allowClear: true,
                data: []
            });
        }
    });

    $("#liAgreement").change(function () {
        agreementId = $(this).val();
        if ($(this).val() > 0) {
            $("#plnAddAccount").show();
            $scope.fnAssignAccountDetails();
        }
        else $("#plnAddAccount").hide();
    });

    $("#liBroker").change(function () {

        brokerId = $(this).val();
        $scope.counterpartyFamilyId = $(this).val();

        if ($(this).val() > 0) {
            $("#plnAddAccount").show();
            $scope.fnAssignAccountDetails();
        }
        else $("#plnAddAccount").hide();
    });


    $scope.fnLoadDefaultDropDowns = function (key) {

        $("#liBeneficiaryType" + key).select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.beneficiaryType == undefined ? [] : $scope.beneficiaryType
        });

        $("#liIntermediaryType" + key).select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.beneficiaryType == undefined ? [] : $scope.beneficiaryType
        });

        $("#liUltimateBeneficiaryType" + key).select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.ultimateBeneficiaryType == undefined ? [] : $scope.ultimateBeneficiaryType
        });

        $("#liAuthorizedParty" + key).select2({
            placeholder: "Select Authorized Party",
            allowClear: true,
            data: $scope.authorizedPartyData == undefined ? [] : $scope.authorizedPartyData
        });
        $("#cashSweep" + key).select2({
            placeholder: "Select Cash Sweep",
            allowClear: true,
            data: $scope.cashSweepData == undefined ? [] : $scope.cashSweepData
        });
        $("#cashSweepTimeZone" + key).select2({
            placeholder: "Zone",
            allowClear: true,
            data: $scope.cashSweepTimeZoneData == undefined ? [] : $scope.cashSweepTimeZoneData
        });
        $("#contactType" + key).select2({
            placeholder: "Contact Type",
            allowClear: true,
            data: $scope.ContactType == undefined ? [] : $scope.ContactType
        });
        $("#liCurrency" + key).select2({
            placeholder: "Select a Currency",
            allowClear: true,
            data: $scope.currencies == undefined ? [] : $scope.currencies
        });
        $("#liSweepCurrency" + key).select2({
            placeholder: "Select a Sweep Currency",
            allowClear: true,
            data: $scope.currencies == undefined ? [] : $scope.currencies
        });
        $("#liCashInstruction" + key).select2({
            placeholder: "select a Cash Instruction",
            allowClear: true,
            data: $scope.cashInstructions == undefined ? [] : $scope.cashInstructions
        });
        

        $("#liAccountDescriptions" + key).select2({
            placeholder: "Select Description",
            allowClear: true,
            data: $scope.accountDescriptions == undefined ? [] : $scope.accountDescriptions
        });
        $("#liAccountModule_" + key).select2({
            placeholder: "Select Module",
            multiple: true,
            allowClear: true,
            data: $scope.accountModules == undefined ? [] : $scope.accountModules,
            formatResult: formatResult,
            formatSelection: formatResult
        });


        $("#liContacts" + key).select2({
            placeholder: "Select Contacts",
            multiple: true,
            //templateResult: groupNameFormat,
            //templateSelection: groupNameFormat,
            data: $scope.contactNames == undefined ? [] : $scope.contactNames
        });

        $("#liAccountPurpose" + key).select2({
            placeholder: "Select Account Type",
            allowClear: true,
            data: $scope.accountPurpose == undefined ? [] : $scope.accountPurpose
        });
        $("#liAccountStatus" + key).select2({
            placeholder: "Select Account Status",
            allowClear: true,
            data: $scope.accountStatus == undefined ? [] : $scope.accountStatus
        });
        $("#liSwiftGroup" + key).select2({
            placeholder: "Select Swift Group",
            allowClear: true,
            data: $scope.SwiftGroupData == undefined ? [] : $scope.SwiftGroupData
        });
        $("#liCustodyAcct" + key).select2({
            placeholder: "Select a Associated Custody Account",
            allowClear: true,
            data: $scope.cusodyAccountData == undefined ? [] : $scope.cusodyAccountData
        });
    }

    $scope.fnRemoveContactDetail = function (accountDetail) {
        var index = $scope.onBoardingAccountDetails.indexOf(accountDetail);
        $scope.onBoardingAccountDetails.splice(index, 1);
        notifySuccess("Account has been deleted");
    }

    $scope.submitAccount = function () {

        var isAccountNameEmpty = false;

        $.each($scope.onBoardingAccountDetails, function (key, value) {

            var liAccountDescriptionsValue = "#liAccountDescriptions" + key;
            value.Description = $(liAccountDescriptionsValue).val();

            var liModuleValue = "#liAccountModule_" + key;
            value.AccountModule = $(liModuleValue).val();

            var liAccountPurposeValue = "#liAccountPurpose" + key;
            value.AccountPurpose = $(liAccountPurposeValue).val();

            var liAccountStatusValue = "#liAccountStatus" + key;
            value.AccountStatus = $(liAccountStatusValue).val();

            if ($("#cashSweep" + key).val() == "Yes") {
                var cashSweepTimeValue = "#cashSweepTime" + key;
                value.CashSweepTime = $(cashSweepTimeValue).val();
                var cashSweepTimeZoneValue = "#cashSweepTimeZone" + key;
                value.CashSweepTimeZone = $(cashSweepTimeZoneValue).val();
            }
            else {
                value.CashSweepTime = "";
                value.CashSweepTimeZone = "";
            }

            value.CutoffTime = $("#cutOffTime" + key).val();
            value.SendersBIC = $("#txtSender" + key).val();

            //var contactNameValue = "#contactName" + key;
            //value.ContactName = $(contactNameValue).val();

            //var contactEmailValue = "#contactEmail" + key;
            //value.ContactEmail = $(contactEmailValue).val();

            //var contactNumberValue = "#contactNumber" + key;
            //value.ContactNumber = $(contactNumberValue).val();

            value.BeneficiaryBankName = $("#beneficiaryBankName" + key).val();
            value.BeneficiaryBankAddress = $("#beneficiaryBankAddress" + key).val();

            value.IntermediaryBankName = $("#intermediaryBankName" + key).val();
            value.IntermediaryBankAddress = $("#intermediaryBankAddress" + key).val();

            value.UltimateBeneficiaryBankName = $("#ultimateBankName" + key).val();
            value.UltimateBeneficiaryBankAddress = $("#ultimateBankAddress" + key).val();

            if (value.UltimateBeneficiaryType == "Account Name" &&
                (value.UltimateBeneficiaryAccountName == null || value.UltimateBeneficiaryAccountName == ""))
                isAccountNameEmpty = true;

        });

        if (isAccountNameEmpty) {
            notifyWarning("Account name field is required");
            return;
        }


        $http.post("/Accounts/AddAccounts", { onBoardingAccounts: $scope.onBoardingAccountDetails, fundName: $scope.fundName, agreement: $scope.agreementName, broker: $scope.broker }).then(function () {
            notifySuccess("accounts Saved successfully");
            var searchText = getUrlParameter("searchText");
            searchText = (searchText == undefined || searchText == 'undefined') ? "" : searchText;
            window.location.href = "/Accounts/Index?searchText=" + searchText;
            $(".glyphicon-refresh").removeClass("icon-rotate");
        });

    }

    $scope.fnCreateContact = function () {
        window.open("/" + subDomain + "Contact/OnboardContact?onBoardingTypeId=3&entityId=" + brokerId + "&contactId=0", "_blank");
    }

    $scope.SaveAccount = function (form, isValid) {

        if (!isValid) {
            if ($scope.accountForm.$error.required == undefined)
            notifyError("FFC Name, FFC Number, Reference, Bank Name, Bank Address & Account Names can only contain ?:().,'+- characters");
            else {
                var message = "";
                angular.forEach($scope.accountForm.$error.required, function (ele, ind) {
                    message += ele.$name + ", ";
                });
                notifyError("Please fill in the required fields " + message.substring(0, message.length - 2));
            }
            return;
        }
            
        var isAccountExits = false;
        var existAccount = "";
        var accountList = [];
        $($scope.onBoardingAccountDetails).each(function (i, v) {
            if (v.onBoardingAccountId == 0) {
                accountList.push(v.AccountNumber + "|" + (v.FFCNumber == null ? "" : v.FFCNumber));
            }
        });
        $($scope.allAccounts).each(function (i, v) {
            if (accountList.indexOf(v.text) > -1) {
                isAccountExits = true;
                existAccount = v.text.split("|");
                return false;
            }
        });
        $(".glyphicon-refresh").addClass("icon-rotate");
        if (isAccountExits) {
            bootbox.confirm(
                {
                    "message": "Account Number: " + existAccount[0]  + " and FFC Number: " + existAccount[1] + " combination already exists. Do you wish to add the account?",
                    buttons: {
                        cancel: {
                            label: "No"
                        },
                        confirm: {
                            label: "Yes"
                        }
                    },
                    callback: function (result) {
                        if (!result) return;
                        else
                            $scope.submitAccount();
                    }
                });
        }
        else
            $scope.submitAccount();
    }

    $scope.fnCashSweep = function (cashSweep, index) {
        var cashSweepTimeZone = "#cashSweepTimeZone" + index;
        if (cashSweep == "Yes") {
            $(".cashSweepTimeDiv" + index).show();
        }
        else $(".cashSweepTimeDiv" + index).hide();
    }

    $scope.fnOnContactNameChange = function (contacts, index) {

        if (contacts != "" && contacts != 'undefined') {

            var onboardingContacts = $filter('filter')(($scope.OnBoardingContactsDetails), function (c) {
                return (contacts.indexOf(c.id) > -1);
            });
            viewContactTable(onboardingContacts, index);
        }
    }
    $scope.fnOnSwiftGroupChange = function (swiftGroup, index) {

        var swData = $.grep($scope.SwiftGroups, function (v) { return v.SwiftGroup == swiftGroup; })[0];
        if (swData != undefined) {
            $("#txtSender" + index).val(swData.SendersBIC);
        }
        else {
            $("#txtSender" + index).val("");
        }
    }

    $scope.fnAuthorizedPartyChange = function (ev, index) {');
        if ($scope.onBoardingAccountDetails[index].AuthorizedParty != "Hedgemark") {
            $scope.onBoardingAccountDetails[index].IsReceivingAccount = true;
            $scope.onBoardingAccountDetails[index].AccountModule = null;
            $scope.onBoardingAccountDetails[index].SwiftGroup = null;
            $scope.onBoardingAccountDetails[index].SendersBIC = null;
            $scope.onBoardingAccountDetails[index].CashSweepTime = null;
            $scope.onBoardingAccountDetails[index].CashSweepTimeZone = null;
            $scope.onBoardingAccountDetails[index].CashSweep = 'No';
            $("#liAccountModule_" + index).select2("val", null);
            $("#liSwiftGroup" + index).select2("val", null);
            $("#cashSweep" + index).select2("val", "No");
        }
        else
            $scope.onBoardingAccountDetails[index].IsReceivingAccount = angular.copy($scope.onBoardingAccountDetails[index].IsReceivingAccountType);
    }

    $scope.fnCutOffTime = function (currency, cashInstruction, index) {

        $http.get("/Accounts/GetCutoffTime?cashInstruction=" + cashInstruction + "&currency=" + currency).then(function (response) {
            var cutOff = response.data.cutOffTime;
            if (cutOff != undefined && cutOff != "") {
                //var cutoffTimes = cutOff.CutoffTime;
                $scope.onBoardingAccountDetails[index].CutoffTime = new Date(2014, 0, 1, cutOff.CutoffTime.Hours, cutOff.CutoffTime.Minutes, cutOff.CutoffTime.Seconds);
                //$("#cutOffTime" + index).val($scope.onBoardingAccountDetails[index].CutoffTime);
            }
            else {
                $("#cutOffTime" + index).val("");
                $("#wireDays" + index).val("");
            }
            $scope.onBoardingAccountDetails[index].CutOffTimeZone = cutOff != undefined && cutOff.CutOffTimeZone != null ? cutoff.CutOffTimeZone : "EST";
            $scope.onBoardingAccountDetails[index].Currency = currency;
            $scope.onBoardingAccountDetails[index].CashInstruction = cashInstruction;

        });
    }

    $scope.fnGetBankDetails = function (biCorAbaValue, id, index) {
        $timeout(function () {
            var accountBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.BICorABA == biCorAbaValue; })[0];
            switch (id) {
                case "Beneficiary":
                    $scope.onBoardingAccountDetails[index].BeneficiaryBankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.onBoardingAccountDetails[index].BeneficiaryBankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
                case "Intermediary":
                    $scope.onBoardingAccountDetails[index].IntermediaryBankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.onBoardingAccountDetails[index].IntermediaryBankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
                case "UltimateBeneficiary":
                    $scope.onBoardingAccountDetails[index].UltimateBeneficiaryBankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.onBoardingAccountDetails[index].UltimateBeneficiaryBankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
            }
        }, 100);

    }

    $scope.fnToggleBeneficiaryBICorABA = function (item, id, index) {
        //var $toggleBtn = $("#" + id + index);

        var isAba = (item == "ABA");

        switch (id) {
            case "Beneficiary":
                //$scope.onBoardingAccountDetails[index].IsBeneficiaryABA = $("#btnBeneficiaryBICorABA" + index).prop("checked");

                var accountBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                var accountBicorAbaData = [];
                $.each(accountBicorAba, function (key, value) {
                    accountBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                });

                accountBicorAbaData = $filter('orderBy')(accountBicorAbaData, 'text');

                if ($("#liBeneficiaryBICorABA" + index).data("select2")) {
                    $("#liBeneficiaryBICorABA" + index).select2("destroy");
                }
                $("#liBeneficiaryBICorABA" + index).select2({
                    placeholder: "Select a beneficiary BIC or ABA",
                    allowClear: true,
                    data: accountBicorAbaData
                });
                break;
            case "Intermediary":
                //$scope.onBoardingAccountDetails[index].IsIntermediaryABA = $("#btnIntermediaryBICorABA" + index).prop("checked");
                var intermediaryBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                var intermediaryBicorAbaData = [];
                $.each(intermediaryBicorAba, function (key, value) {
                    intermediaryBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                });

                intermediaryBicorAbaData = $filter('orderBy')(intermediaryBicorAbaData, 'text');

                if ($("#liIntermediaryBICorABA" + index).data("select2")) {
                    $("#liIntermediaryBICorABA" + index).select2("destroy");
                }
                $("#liIntermediaryBICorABA" + index).select2({
                    placeholder: "Select a intermediary BIC or ABA",
                    allowClear: true,
                    data: intermediaryBicorAbaData
                });
                break;
            case "UltimateBeneficiary":
                //$scope.onBoardingAccountDetails[index].IsUltimateBeneficiaryABA = $("#btnUltimateBICorABA" + index).prop("checked");
                $scope.onBoardingAccountDetails[index].UltimateBeneficiaryType = item;
                
                if (item == "Account Name") {
                    $("#divUltimateBeneficiaryBICorABA" + index).hide();
                    $("#ultimateBankName" + index).hide();
                    $("#ultimateBankAddress" + index).hide();
                    $("#accountName" + index).show();
                    $scope.onBoardingAccountDetails[index].UltimateBeneficiaryBICorABA = null;
                    $scope.onBoardingAccountDetails[index].UltimateBeneficiaryBankName = null;
                    $scope.onBoardingAccountDetails[index].UltimateBeneficiaryBankAddress = null;
                    return;
                } else {
                    $("#divUltimateBeneficiaryBICorABA" + index).show();
                    $("#ultimateBankName" + index).show();
                    $("#ultimateBankAddress" + index).show();
                    $("#accountName" + index).hide();
                    $scope.onBoardingAccountDetails[index].UltimateBeneficiaryAccountName = null;
                }
                var ultimateBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                var ultimateBicorAbaData = [];
                $.each(ultimateBicorAba, function (key, value) {
                    ultimateBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                });

                ultimateBicorAbaData = $filter('orderBy')(ultimateBicorAbaData, 'text');

                if ($("#liUltimateBeneficiaryBICorABA" + index).data("select2")) {
                    $("#liUltimateBeneficiaryBICorABA" + index).select2("destroy");
                }
                $("#liUltimateBeneficiaryBICorABA" + index).select2({
                    placeholder: "Select a ultimate beneficiary BIC or ABA",
                    allowClear: true,
                    data: ultimateBicorAbaData
                });

                break;
        }
    }

    function viewSsiTemplateTable(data, key) {

        if ($("#ssiTemplateTable" + key).hasClass("initialized")) {
            fnDestroyDataTable("#ssiTemplateTable" + key);
        }

        if (data.length > 0)
            $("#btnAccountMapStatusButtons" + key).show();
        else
            $("#btnAccountMapStatusButtons" + key).hide();

        ssiMapTable[key] = $("#ssiTemplateTable" + key).not(".initialized").addClass("initialized").DataTable({
            "bDestroy": true,
            //responsive: true,
            aaData: data,
            "aoColumns": [
                {
                    "sTitle": "Template Name",
                    "mData": "TemplateName"
                },
                {
                    "mData": "SSITemplateType",
                    "sTitle": "SSI Template Type",
                    "mRender": function (tdata) {
                        if (tdata === "Broker")
                            return "<label class=\"label ng-show-only shadowBox label-info\" style=\"font-size: 12px;\">Broker</label>";
                        if (tdata === "Fee/Expense Payment")
                            return "<label class=\"label ng-show-only shadowBox label-default\" style=\"font-size: 12px;\">Fee/Expense Payment</label>";
                        return "";
                    }
                },
                {
                    "sTitle": "Account Number",
                    "mData": "AccountNumber"
                },
                {
                    "sTitle": "Created By",
                    "mData": "CreatedBy"
                },
                {
                    "sTitle": "Updated By",
                    "mData": "UpdatedBy"
                },
                {
                    "mData": "Status",
                    "sTitle": "Status",
                    "mRender": function (tdata) {
                        if (tdata === "Pending Approval")
                            return "<label class=\"label ng-show-only label-warning\" style=\"font-size: 12px;\">Pending Approval</label>";
                        if (tdata === "Approved")
                            return "<label class=\"label ng-show-only label-success\" style=\"font-size: 12px;\">Approved</label>";
                        return "";
                    }
                },
                {
                    "mData": "onBoardingSSITemplateId", "sTitle": "Go to SSI Template", "className": "dt-center",
                    "mRender": function (tdata, type, row) {
                        // return " <label class=\"label ng-show-only shadowbox label-success\" style=\"font-size: 12px;\">" + row.CompletedCount + "</label> <label class=\"label ng-show-only shadowbox label-warning\"  style=\"font-size: 12px;\">" + row.InProcessCount + "</label> <label class=\"label ng-show-only shadowbox label-info\" style=\"font-size: 12px;\">" + row.TbdCount + "</label>";
                        return "<a class=\"btn btn-primary btn-xs\" id=\"" + tdata + "\" ><i class=\"glyphicon glyphicon-share-alt\"></i></a>";
                    }

                },
                {
                    "mData": "onBoardingAccountSSITemplateMapId", "className": "dt-center",
                    "sTitle": "Remove", "mRender": function (tdata) {
                        return "<a class='btn btn-danger btn-xs' title='Remove SSI' id='" + key + "'><i class='glyphicon glyphicon-trash'></i></a>";
                    }
                }
            ],
            "createdRow": function (row, rowData) {
                switch (rowData.Status) {
                    case "Approved":
                        $(row).addClass("success");
                        break;
                    case "Pending Approval":
                        $(row).addClass("warning");
                        break;
                }

            },
            "deferRender": false,
            "bScrollCollapse": true,
            //scroller: true,
            //sortable: false,
            //"sDom": "ift",
            //pagination: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "scrollY": 350,
            "order": [[0, "desc"]],
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No ssi templates are available for the account",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ SSI Templates"
            }
        });

        window.setTimeout(function () {
            ssiMapTable[key].columns.adjust().draw(true);
        }, 50);


        $("#accountDetailCP tbody tr td:last-child a").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowIndex = $(this).attr("id");
            var rowElement = ssiMapTable[rowIndex].row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this ssi template from account?", function (result) {
                if (!result) {
                    return;
                } else {
                    if (rowElement.onBoardingAccountSSITemplateMapId > 0) {
                        $http.post("/Accounts/RemoveSsiTemplateMap", { ssiTemplateMapId: rowElement.onBoardingAccountSSITemplateMapId }).then(function () {
                            ssiMapTable[rowIndex].row(selectedRow).remove().draw();
                            //$scope.ssiTemplateDocuments.pop(rowElement);
                            $scope.onBoardingAccountDetails[rowIndex].onBoardingAccountSSITemplateMaps.pop(rowElement);
                            notifySuccess("ssi template has removed succesfully");
                        });
                    } else {
                        ssiMapTable[rowIndex].row(selectedRow).remove().draw();
                        $scope.onBoardingAccountDetails[rowIndex].onBoardingAccountSSITemplateMaps.pop(rowElement);
                        notifySuccess("ssi template has removed succesfully");
                    }
                }
            });

        });

        $("#accountDetailCP tbody tr td a.btn-primary").on("click", function (event) {
            event.preventDefault();
            var ssitemplateId = $(this).attr("id");
            window.open("/Accounts/SSITemplate?ssiTemplateId=" + ssitemplateId, "_blank");
        });

        $("#accountDetailCP table.ssiTemplate tbody tr").on("click", function (event) {
            event.preventDefault();

            $("#accountDetailCP table.ssiTemplate tbody tr").removeClass("info");
            if (!$(this).hasClass("info")) {
                $(this).addClass("info");
            }

            var selectedRow = $(this);
            var rowIndex = $(this).parents("table").attr("tIndex");
            var rowElement = ssiMapTable[rowIndex].row(selectedRow).data();

            $scope.onBoardingAccountSSITemplateMapId = rowElement.onBoardingAccountSSITemplateMapId;
            $scope.plIndex = rowIndex;

            if (rowElement.Status == pendingStatus && rowElement.onBoardingAccountSSITemplateMapId > 0 && rowElement.UpdatedBy != $("#userName").val()) {
                $("#btnAccountMapStatusButtons" + rowIndex + " a[title='Approve']").removeClass("disabled");
            }
            //if (rowElement.onBoardingAccountStatus == createdStatus) {
            //    $("#btnAccountStatusButtons button[id='requestForApproval']").removeClass("disabled");
            //}
            //if (rowElement.onBoardingAccountStatus != createdStatus) {
            //    $("#btnAccountStatusButtons button[id='revert']").removeClass("disabled");
            //}

        });
    }

    function toggleChevron(e) {
        $(e.target)
            .find("i.indicator")
            .toggleClass("glyphicon-chevron-down").toggleClass("glyphicon-chevron-up");
        $("html, body").animate({ scrollTop: $(e.target).offset().top - 10 }, "slow");
    }

    Dropzone.options.myAwesomeDropzone = false;
    Dropzone.autoDiscover = false;

    function attachment(key) {

        if (!($("#uploadFiles" + key).hasClass("dz-clickable"))) {
            $("#uploadFiles" + key).dropzone({
                url: "/Accounts/UploadAccountFiles?accountId=" + $scope.onBoardingAccountDetails[key].onBoardingAccountId,
                dictDefaultMessage: "<span><span style=\"color: red\"> * </span>Drag/Drop account documents here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
                autoDiscover: false,
                acceptedFiles: ".msg,.csv,.txt,.pdf,.xls,.xlsx,.zip,.rar",
                maxFiles: 5,
                previewTemplate: "<div class='row col-sm-2' style='padding: 15px;'><div class='panel panel-success panel-dz'> <div class='panel-heading'> <h3 class='panel-title' style='text-overflow: ellipsis;white-space: nowrap;overflow: hidden;'><span data-dz-name></span> - (<span data-dz-size></span>)</h3> " +
                    "</div> <div class='panel-body'> <span class='dz-upload' data-dz-uploadprogress></span>" +
                    "<div class='progress'><div data-dz-uploadprogress class='progress-bar progress-bar-warning progress-bar-striped active dzFileProgress' style='width: 0%'></div>" +
                    "</div></div></div></div>",

                maxfilesexceeded: function (file) {
                    this.removeAllFiles();
                    this.addFile(file);
                },
                init: function () {
                    //var myDropZone = this;
                    this.on("processing", function (file) {
                        this.options.url = "/Accounts/UploadAccountFiles?accountId=" + $scope.onBoardingAccountDetails[key].onBoardingAccountId;
                    });
                },
                processing: function (file, result) {
                    $("#uploadFiles" + key).animate({ "min-height": "140px" });
                },
                success: function (file, result) {
                    if ($scope.onBoardingAccountDetails[key].onBoardingAccountId == 0)
                        notifyWarning("Please save account to upload documents");
                    else {
                        $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
                        $(".dzFileProgress").html("Upload Successful");
                        $("#uploadFiles" + key).animate({ "min-height": "80px" });

                        var aDocument = result;
                        $.each(aDocument.Documents, function (index, value) {
                            // $scope.ssiTemplateDocuments.push(value);
                            $scope.onBoardingAccountDetails[key].onBoardingAccountDocuments.push(value);
                        });

                        viewAttachmentTable($scope.onBoardingAccountDetails[key].onBoardingAccountDocuments, key);
                    }
                },
                queuecomplete: function () {
                },
                complete: function (file, result) {
                    if ($scope.onBoardingAccountDetails[key].onBoardingAccountId != 0) {
                        $("#uploadFiles" + key).removeClass("dz-drag-hover");

                        if (this.getRejectedFiles().length > 0 && this.getAcceptedFiles().length === 0 && this.getQueuedFiles().length === 0) {
                            showMessage("File format is not supported to upload.", "Status");
                            return;
                        }

                        if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
                            this.removeAllFiles();
                            notifySuccess("Files Uploaded successfully");
                        }
                    }
                }
            });
        }
    }

    $scope.fnIntialize = function () {

        $.each($scope.onBoardingAccountDetails, function (key, value) {

            //$scope.fnGetAccountDescriptions(key);
            //$scope.fnLoadContactDetails(agreementId, key);
            $scope.fnLoadDefaultDropDowns(key);
            // var cashSweepTime = "#cashSweepTime" + key;            
            if (value.CashSweep == "Yes") {
                $(".cashSweepTimeDiv" + key).show();
                // $(cashSweepTime).val(value.CashSweepTime);
                $("#cashSweepTimeZone" + key).val(value.CashSweepTimeZone);
            } else $(".cashSweepTimeDiv" + key).hide();

            var descriptionValue = "#liAccountDescriptions" + key;
            $(descriptionValue).val(value.Description);

            var moduleValue = "#liAccountModule_" + key;
            $(moduleValue).val(value.AccountModule);

            //var contactNameValue = "#contactName" + key;
            //$(contactNameValue).val(value.ContactName);

            var accountPurposeValue = "#liAccountPurpose" + key;
            $(accountPurposeValue).val(value.AccountPurpose);

            var accountStatusValue = "#liAccountStatus" + key;
            $(accountStatusValue).val(value.AccountStatus);

            $scope.fnToggleBeneficiaryBICorABA(value.BeneficiaryType, "Beneficiary", key);
            $scope.fnToggleBeneficiaryBICorABA(value.IntermediaryType, "Intermediary", key);
            $scope.fnToggleBeneficiaryBICorABA(value.UltimateBeneficiaryType, "UltimateBeneficiary", key);

            //if (value.BeneficiaryBICorABA != null && value.BeneficiaryBICorABA != "")
            //    $("#liBeneficiaryBICorABA" + key).select2("val", value.BeneficiaryBICorABA);

            //if (value.IntermediaryBICorABA != null && value.IntermediaryBICorABA != "")
            //    $("#liIntermediaryBICorABA" + key).select2("val", value.IntermediaryBICorABA);

            //if (value.UltimateBeneficiaryBICorABA != null && value.UltimateBeneficiaryBICorABA != "")
            //    $("#liUltimateBeneficiaryBICorABA" + key).select2("val", value.UltimateBeneficiaryBICorABA);


            //$("#btnBeneficiaryBICorABA" + key).bootstrapToggle('off');
            //$("#btnIntermediaryBICorABA" + key).bootstrapToggle('off');
            //$("#btnUltimateBICorABA" + key).bootstrapToggle('off');


            //var contactEmailValue = "#contactEmail" + key;
            //$(contactEmailValue).val(value.ContactEmail);         
            //var contactNumberValue = "#contactNumber" + key;
            //$(contactNumberValue).val(value.ContactNumber);

            //var ssiTemplateMapData = (value.onBoardingAccountSSITemplateMaps != null && value.onBoardingAccountSSITemplateMaps != undefined) ?
            //    value.onBoardingAccountSSITemplateMaps : JSON.parse("{" + ssiTemplateData + "}");

            attachment(key);

            if (value.onBoardingAccountDocuments != null && value.onBoardingAccountDocuments != undefined && value.onBoardingAccountDocuments.length > 0) {
                viewAttachmentTable(value.onBoardingAccountDocuments, key);
            }
            if (value.ContactName != null && value.ContactName != undefined && value.ContactName != "") {


                var onboardingContacts = $filter('filter')(($scope.OnBoardingContactsDetails), function (c) {
                    return (value.ContactName.indexOf(c.id) > -1);
                });
                viewContactTable(onboardingContacts, key);
                //$("#liContacts" + key).select2("val", value.ContactName.split(',').map(Number));
            }
            //else {
            //    var documents = JSON.parse("{" + documentData + "}");
            //    viewAttachmentTable(documents, key);
            //}

            if (value.onBoardingAccountSSITemplateMaps != null && value.onBoardingAccountSSITemplateMaps != undefined && value.onBoardingAccountSSITemplateMaps.length > 0) {
                viewSsiTemplateTable(value.onBoardingAccountSSITemplateMaps, key);
            }

            if (value.onBoardingAccountDocuments != null && value.onBoardingAccountDocuments != undefined && value.onBoardingAccountDocuments.length > 0 && value.onBoardingAccountStatus == "Approved") {
                $(".dz-hidden-input").prop("disabled", true);
            } else {
                $(".dz-hidden-input").prop("disabled", false);
            }


        });
        $(".txtHoldbackAmount").numericEditor({ iDecimalPlaces: 0 });
        $("#accountDetailCP .panel-default .panel-heading").on("click", function (e) {
            $(this).parent().find("div.collapse").collapse("toggle");
            toggleChevron(e);
            var tableIndex = parseInt($(this).parent().attr("plindex"));
            if (ssiMapTable != null && ssiMapTable.length > 0 && tableIndex < ssiMapTable.length) {
                window.setTimeout(function () {
                    ssiMapTable[tableIndex].columns.adjust().draw(true);
                }, 10);
            }
            if (accountDocumentTable != null && accountDocumentTable.length > 0 && tableIndex < accountDocumentTable.length) {
                window.setTimeout(function () {
                    accountDocumentTable[tableIndex].columns.adjust().draw(true);
                }, 10);
            }
            if (contactTable != null && contactTable.length > 0 && tableIndex < contactTable.length) {
                window.setTimeout(function () {
                    contactTable[tableIndex].columns.adjust().draw(true);
                }, 10);
            }
        });
        $("#btnAgrExpandAllPanel").click(function () {
            angular.element("#accountDetailCP div.collapse").collapse("show");
            angular.element("#accountDetailCP .panel-heading i.glyphicon-chevron-down").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
        });

        $("#btnAgrCollapseAllPanel").click(function () {
            angular.element("#accountDetailCP div.collapse").collapse("hide");
            angular.element("#accountDetailCP .panel-heading i.glyphicon-chevron-up").addClass("glyphicon-chevron-down").removeClass("glyphicon-chevron-up");
        });

        var inde = $scope.onBoardingAccountDetails.length - 1;
        if (inde == 0) {
            //angular.element("#Panel" + inde + "").css("margin-bottom", "80px");
            angular.element("#accountDetailCP .panel:first-child").find(".collapse").addClass("in");
            angular.element("#accountDetailCP .panel:first-child .panel-heading i.glyphicon-chevron-down").addClass("glyphicon-chevron-up").removeClass("glyphicon-chevron-down");
        }
        else {
            //angular.element("#Panel0").css("margin-bottom", "18px");
            $("#btnAgrCollapseAllPanel").trigger("click");
            if ($scope.copyAccount != undefined) {
                angular.element("#accountDetailCP #Panel" + inde + "").find(".collapse").addClass("in");
                angular.element("#accountDetailCP #Panel" + inde + " .panel-heading i.glyphicon-chevron-down").addClass("glyphicon-chevron-up").removeClass("glyphicon-chevron-down");
            }
        }
        var target = angular.element("#Panel" + inde + "");
        if (target.length) {

            $("html,body").animate({
                scrollTop: target.offset().top - 50
            }, 100, function () { });

        }

        //$("#accountDetailCP .panel").each(function (i, ele) {

        //    var searchLabel = "";
        //    $.each($(ele).find(".panel-title").find(".control-label"), function (j, v) {
        //        searchLabel += " " + $(v).text().toLowerCase();
        //    });
        //    $.each($(ele).find(".panel-body").find(".control-label"), function (j, v) {
        //        searchLabel += " " + $(v).text().toLowerCase();
        //    });

        //    $(ele).attr("data-search-term", searchLabel);
        //});

        //$(".live-search-box").on("keyup", function (event) {
        //    var searchTerm = $(this).val().toLowerCase();
        //    if (searchTerm == "" && event.keyCode == 8) {

        //        if (searchTerm == "") {
        //            $(".live-search-highlight").removeClass("active");
        //        }
        //        $("#contactDetailCP .panel").show(200);
        //        return;
        //    }

        //    var regexOfRuleHeaders = RegExp("\(" + searchTerm.replace(/[^a-zA-Z0-9()]/g, "\\$&") + "\)", "ig");

        //    $("#accountDetailCP .control-label").each(function (i, v) {
        //        $(v).html($(v).text().replace(regexOfRuleHeaders, "<font class='live-search-highlight active'>$&</font>"));
        //        //$(v).text($(v).text().replace(regexOfRuleHeaders, "<font class='live-search-highlight active'>$&</font>"));
        //    });


        //    $("#accountDetailCP .panel").each(function (index, li) {
        //        if ($(li).filter('[data-search-term *= ' + searchTerm + ']').length > 0 || searchTerm.length < 1) {
        //            $(li).show(200);
        //            $(li).find("div.collapse").collapse("show");
        //        } else {
        //            $(li).hide(200);
        //        }
        //    });
        //});
    }

    $scope.fnGetAccountDescriptions = function (panelIndex) {
        $http.get("/Accounts/GetAccountDescriptionsByAgreementTypeId?agreementTypeId=" + $scope.agreementTypeId).then(function (response) {
            $scope.AccountDescriptions = response.data.accountDescriptions;
            $("#liAccountDescriptions" + panelIndex).select2({
                placeholder: "Select Description",
                allowClear: true,
                multiple: true,
                data: response.data.accountDescriptions
            });
            if ($scope.agreementTypeId > 0)
                $("#liAccountDescriptions" + panelIndex).val($scope.onBoardingAccountDetails[panelIndex].Description);    
        });
    }

    $scope.fnGetAccountModules = function (panelIndex) {
        $http.get("/Accounts/GetAccountModules").then(function (response) {
            $scope.accountModules = response.data.accountModules;
            $("#liAccountModule_" + panelIndex).select2({
                placeholder: "Select Modules",
                multiple: true,
                allowClear: true,
                data: response.data.accountModules,
                formatResult: formatResult,
                formatSelection: formatResult
            });
            $("#liAccountModule_" + panelIndex).val($scope.onBoardingAccountDetails[panelIndex].AccountModule);
        });
    }

    function formatResult(selectData) {
        var stat = $filter('filter')($scope.accountModules, { 'id': selectData.id }, true)[0];
        return selectData.text + "&nbsp;&nbsp;<label class='label " + (selectData.report == "Collateral" ? " label-info" : "label-default") + " shadowBox'>" + selectData.report + "</label>";
        //return selectData.text;
    }

    $scope.fnGetAccountReports = function (panelIndex) {
        $http.get("/Accounts/GetAccountReports").then(function (response) {
            $scope.accountReports = response.data.accountReports;
            $("#liAccountReport").select2({
                placeholder: "Select Modules",
                data: response.data.accountReports
            });
            $("#liAccountReport").select2('val', $scope.accountReports[0].id);
        });
    }

    $scope.addAccountDetail = function () {
        if ($('#txtDetail').val() == undefined || $('#txtDetail').val() == "") {
            //pop-up    
            $("#txtDetail").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: $scope.detail + " cannot be empty. Please add a valid " + $scope.detail,
                html: true,
                width: "250px"
            });

            $("#txtDetail").popover("show");
            return;
        }

        $("#txtDetail").popover("hide");
        var isExists = false;
        if ($scope.detail == "Description") {
            $($scope.AccountDescriptions).each(function (i, v) {
                if ($("#txtDetail").val().trim() == v.text) {
                    isExists = true;
                    return false;
                }
            });
        }
        else {
            $($scope.accountModules).each(function (i, v) {
                if ($("#txtDetail").val().trim() == v.text) {
                    isExists = true;
                    return false;
                }
            });
        }
        if (isExists) {
            $("#txtDetail").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: $scope.detail + " already exists. Please enter a valid " + $scope.detail,
                html: true,
                width: "250px"
            });
            //notifyWarning("Governing Law already exists. Please enter a valid governing law");
            $("#txtDetail").popover("show");
            return;
        }
        if ($scope.detail == "Description") {
            $http.post("/Accounts/AddAccountDescriptions", { accountDescription: $("#txtDetail").val(), agreementTypeId: $scope.agreementTypeId }).then(function (response) {
                notifySuccess("Description added successfully");
                $scope.onBoardingAccountDetails[$scope.PanelIndex].Description = $("#txtDetail").val();
                $scope.fnGetAccountDescriptions($scope.PanelIndex);
            });
        }
        else {
            $http.post("/Accounts/AddAccountModule", { reportId: $("#liAccountReport").select2('val'), accountModule: $("#txtDetail").val() }).then(function (response) {
                notifySuccess("Module added successfully");
                $scope.fnGetAccountModules($scope.PanelIndex);
            });
        }

        $("#accountDetailModal").modal("hide");
    }

    $scope.fnAddAccountDetailModal = function (panelIndex, detail) {
        $scope.PanelIndex = panelIndex;
        $scope.detail = detail;
        //$scope.scrollPosition = $(window).scrollTop();
        //$("#txtGoverningLaw").prop("placeholder", "Enter a governing law");
        $('#accountDetailModal').modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtDetail").popover("hide").val("");
            $("#liAccountReport").select2('val', $scope.accountReports[0].id);
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }
    angular.element("#txtDetail").on("focusin", function () { angular.element("#txtDetail").popover("hide"); });

    $scope.fnAddCurrency = function () {
        if ($("#txtCurrency").val() == undefined || $("#txtCurrency").val() == "") {
            //pop-up    
            $("#txtCurrency").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Currency cannot be empty. Please add a valid Currency",
                html: true,
                width: "250px"
            });

            $("#txtCurrency").popover("show");
            return;
        }

        $("#txtCurrency").popover("hide");
        var isExists = false;
        $($scope.currencies).each(function (i, v) {
            if ($("#txtCurrency").val() == v.text) {
                isExists = true;
                return false;
            }
        });
        if (isExists) {
            $("#txtCurrency").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Currency is already exists. Please enter a valid Currency",
                html: true,
                width: "250px"
            });
            $("#txtCurrency").popover("show");
            return;
        }

        $http.post("/Accounts/AddCurrency", { currency: $("#txtCurrency").val() }).then(function (response) {
            notifySuccess("Currency added successfully");
            $scope.onBoardingAccountDetails[$scope.PanelIndex].Currency = $("#txtCurrency").val();
            $scope.fnGetCurrency($scope.PanelIndex);
            $("#txtCurrency").val("");
        });

        $("#currencyModal").modal("hide");
    }

    $scope.fnAddCurrencyModal = function (panelIndex) {
        $scope.PanelIndex = panelIndex;
        $("#currencyModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtCurrency").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }
    angular.element("#txtCurrency").on("focusin", function () { angular.element("#txtCurrency").popover("hide"); });

    $scope.fnAddCashInstruction = function () {
        if ($("#txtCashInstruction").val() == undefined || $("#txtCashInstruction").val() == "") {
            //pop-up    
            $("#txtCashInstruction").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Cash Instruction cannot be empty. Please add a valid Cash Instruction",
                html: true,
                width: "250px"
            });

            $("#txtCashInstruction").popover("show");
            return;
        }

        $("#txtCashInstruction").popover("hide");
        var isExists = false;
        $($scope.cashInstructions).each(function (i, v) {
            if ($("#txtCashInstruction").val() == v.text) {
                isExists = true;
                return false;
            }
        });
        if (isExists) {
            $("#txtCashInstruction").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Cash Instruction is already exists. Please enter a valid Cash Instruction",
                html: true,
                width: "250px"
            });
            $("#txtCashInstruction").popover("show");
            return;
        }

        $http.post("/Accounts/AddCashInstruction", { cashInstruction: $("#txtCashInstruction").val() }).then(function (response) {
            notifySuccess("Cash instruction mechanism added successfully");
            $scope.onBoardingAccountDetails[$scope.PanelIndex].CashInstruction = $("#txtCashInstruction").val();
            $scope.fnGetCashInstruction($scope.PanelIndex);
            $("#txtCashInstruction").val("");
        });

        $("#cashInstructionModal").modal("hide");
    }

    angular.element("#txtCashInstruction").on("focusin", function () { angular.element("#txtCashInstruction").popover("hide"); });

    $scope.fnAddCashInstructionModal = function (panelIndex) {
        $scope.PanelIndex = panelIndex;
        $("#cashInstructionModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtCashInstruction").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }

    $scope.fnAddBICorABA = function () {
        if ($("#txtBICorABA").val() == undefined || $("#txtBICorABA").val() == "") {
            //pop-up    
            $("#txtBICorABA").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "BIC or ABA cannot be empty. Please add a valid BIC or ABA",
                html: true,
                width: "250px"
            });

            $("#txtBICorABA").popover("show");
            return;
        }

        var isExists = false;
        $($scope.accountBicorAba).each(function (i, v) {
            if ($("#txtBICorABA").val() == v.BICorABA) {
                isExists = true;
                return false;
            }
        });

        if (isExists) {
            $("#txtBICorABA").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "BIC or ABA is already exists. Please enter a valid BIC or ABA",
                html: true,
                width: "250px"
            });
            $("#txtBICorABA").popover("show");
            $(".popover-content").html("BIC or ABA is already exists. Please enter a valid BIC or ABA");
            return;
        }
        if ($("#btnBICorABA").prop("checked") && ($("#txtBICorABA").val().length != 9 || !$.isNumeric($("#txtBICorABA").val()))) {
            $("#txtBICorABA").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "ABA is 9 digit numeric value. Please enter a valid ABA",
                html: true,
                width: "250px"
            });
            $("#txtBICorABA").popover("show");
            $(".popover-content").html("ABA is 9 digit numeric value. Please enter a valid ABA");
            return;
        }

        if (!$("#btnBICorABA").prop("checked") && (!($("#txtBICorABA").val().length > 7 && $("#txtBICorABA").val().length < 12) || $.isNumeric($("#txtBICorABA").val()))) {

            $("#txtBICorABA").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "BIC is 8 or 11 digit alpha numeric value. Please enter a valid BIC",
                html: true,
                width: "250px"
            });
            $("#txtBICorABA").popover("show");
            $(".popover-content").html("BIC is 8 or 11 digit alpha numeric value. Please enter a valid BIC");
            return;
        }

        $("#txtBICorABA").popover("hide");

        if ($("#txtBankName").val() == undefined || $("#txtBankName").val() == "") {
            //pop-up    
            $("#txtBankName").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Bank name cannot be empty. Please add a valid bank name",
                html: true,
                width: "250px"
            });

            $("#txtBankName").popover("show");
            return;
        }

        $("#txtBankName").popover("hide");

        if ($("#txtBankAddress").val() == undefined || $("#txtBankAddress").val() == "") {
            //pop-up    
            $("#txtBankAddress").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Bank Address cannot be empty. Please add a valid Bank Address",
                html: true,
                width: "250px"
            });

            $("#txtBankAddress").popover("show");
            return;
        }

        $("#txtBankAddress").popover("hide");

        $scope.accountBeneficiary = {
            onBoardingAccountBICorABAId: 0,
            BICorABA: $("#txtBICorABA").val().toUpperCase(),
            BankName: $("#txtBankName").val(),
            BankAddress: $("#txtBankAddress").val(),
            IsABA: $("#btnBICorABA").prop("checked")
        }


        $http.post("/Accounts/AddAccountBiCorAba", { accountBiCorAba: $scope.accountBeneficiary }).then(function (response) {
            notifySuccess("Beneficiary BIC or ABA added successfully");
            $scope.BicorAba = $("#txtBICorABA").val().toUpperCase();
            $scope.isBicorAba = $("#btnBICorABA").prop("checked");
            $scope.fnGetBicorAba($scope.PanelIndex);
            $("#txtBICorABA").val("");
            $("#txtBankName").val("");
            $("#txtBankAddress").val("");
            $("#btnBICorABA").prop("checked", false).change();
        });

        $("#beneficiaryABABICModal").modal("hide");
    }

    angular.element("#txtBICorABA").on("focusin", function () { angular.element("#txtBICorABA").popover("hide"); });
    angular.element("#txtBankName").on("focusin", function () { angular.element("#txtBankName").popover("hide"); });
    angular.element("#txtBankAddress").on("focusin", function () { angular.element("#txtBankAddress").popover("hide"); });

    $scope.fnAddBeneficiaryModal = function (panelIndex) {
        $scope.PanelIndex = panelIndex;
        $("#beneficiaryABABICModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtBICorABA").popover("hide");
            $("#txtBankAddress").popover("hide");
            $("#txtBankName").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }

    $("#liSsiTemplate").change(function () {

        if ($(this).val() > 0) {

            var ssiTemplateId = $(this).val();
            var ssiTemplate = $.grep($scope.ssiTemplates, function (v) { return v.onBoardingSSITemplateId == ssiTemplateId; })[0];
            $("#FFCName").val(ssiTemplate.FFCName);
            $("#FFCNumber").val(ssiTemplate.FFCNumber);
            $("#Reference").val(ssiTemplate.Reference);
            $("#accountNumber").val(ssiTemplate.AccountNumber);
            $("#templateType").val(ssiTemplate.SSITemplateType);

        } else {

            $("#FFCName").val("");
            $("#FFCNumber").val("");
            $("#Reference").val("");
            $("#accountNumber").val("");
            $("#templateType").val("");
        }
    });

    $scope.fnAddAccountSSITemplateMap = function () {

        if ($("#liSsiTemplate").val() == undefined || $("#liSsiTemplate").val() == "") {
            //pop-up    
            $("#spnSsi").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Ssi template cannot be empty. Please select a valid ssi template",
                html: true,
                width: "250px"
            });

            $("#spnSsi").popover("show");
            return;
        }
        var ssiTemplateExists = $.grep($scope.onBoardingAccountDetails[$scope.PanelIndex].onBoardingAccountSSITemplateMaps, function (v) { return v.onBoardingSSITemplateId == $("#liSsiTemplate").val(); })[0];

        if (ssiTemplateExists != null) {
            $("#spnSsi").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Ssi template is already exists. Please select a valid ssi template",
                html: true,
                width: "250px"
            });

            $("#spnSsi").popover("show");
            return;
        }

        $("#spnSsi").popover("hide");

        $scope.onBoardingAccountSSITemplateMap = {
            onBoardingAccountSSITemplateMapId: 0,
            onBoardingSSITemplateId: $("#liSsiTemplate").val(),
            FFCName: $("#FFCName").val(),
            FFCNumber: $("#FFCNumber").val(),
            Reference: $("#Reference").val(),
            TemplateName: $("#liSsiTemplate").select2("data").text,
            AccountNumber: $("#accountNumber").val(),
            SSITemplateType: $("#templateType").val(),
            CreatedBy: $("#userName").val(),
            UpdatedBy: $("#userName").val(),
            Status: pendingStatus
        };

        $scope.onBoardingAccountDetails[$scope.PanelIndex].onBoardingAccountSSITemplateMaps.push($scope.onBoardingAccountSSITemplateMap);

        viewSsiTemplateTable($scope.onBoardingAccountDetails[$scope.PanelIndex].onBoardingAccountSSITemplateMaps, $scope.PanelIndex);

        $("#accountSSITemplateMapModal").modal("hide");
    }

    $scope.fnUpdateAccountMapStatus = function (status, statusAction) {
        $scope.AccountMapStatus = status;

        var confirmationMsg = "Are you sure you want to " + ((statusAction === "Request for Approval") ? "<b>request</b> for approval of" : "<b>" + statusAction + "</b>") + " the selected account ssi template map?";
        if (statusAction == "Request for Approval") {
            $("#btnMapSaveComment").addClass("btn-warning").removeClass("btn-success").removeClass("btn-info");
            $("#btnMapSaveComment").html('<i class="glyphicon glyphicon-share-alt"></i>&nbsp;Request for approval');
        } else if (statusAction == "Approve") {
            $("#btnMapSaveComment").removeClass("btn-warning").addClass("btn-success").removeClass("btn-info");
            $("#btnMapSaveComment").html('<i class="glyphicon glyphicon-ok"></i>&nbsp;Approve');
        }
        else if (statusAction == "Revert") {
            $("#btnMapSaveComment").removeClass("btn-warning").removeClass("btn-success").addClass("btn-info");
            $("#btnMapSaveComment").html('<i class="glyphicon glyphicon-repeat"></i>&nbsp;Revert');
        }

        $("#pAccountMapMsg").html(confirmationMsg);
        $("#UpdateAccountMapStatusModal").modal("show");
    }

    $scope.fnSaveAccountMapStatus = function () {
        $http.post("/Accounts/UpdateAccountMapStatus", { status: $scope.AccountMapStatus, accountMapId: $scope.onBoardingAccountSSITemplateMapId, comments: $("#statusMapComments").val().trim() }).then(function () {
            notifySuccess("Account ssi template map  " + $scope.AccountMapStatus.toLowerCase() + " successfully");

            $("#btnAccountMapStatusButtons a[title='Approve']").addClass("disabled");

            var rowElement = ssiMapTable[$scope.plIndex].row(".info").data();
            rowElement.Status = $scope.AccountMapStatus;
            rowElement.UpdatedBy = $("#userName").val();
            rowElement.Comments = $("#statusMapComments").val().trim();
            rowElement.UpdatedAt = moment();
            var selectedRowNode = ssiMapTable[$scope.plIndex].row(".info").data(rowElement).draw().node();

            $(selectedRowNode).addClass("success").removeClass("warning");

        });
        $("#UpdateAccountMapStatusModal").modal("hide");
    }

    $scope.fnAssociationSSI = function (panelIndex) {
        $scope.PanelIndex = panelIndex;

        var ssiData = [];
        $.each($scope.ssiTemplates, function (key, value) {
            ssiData.push({ "id": value.onBoardingSSITemplateId, "text": value.TemplateName });
        });

        ssiData = $filter('orderBy')(ssiData, 'text');

        if ($("#liSsiTemplate").data("select2")) {
            $("#liSsiTemplate").select2("destroy");
        }

        $("#liSsiTemplate").select2({
            placeholder: "Select the Ssi Template",
            allowClear: true,
            data: ssiData
        });

        $("#accountSSITemplateMapModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#FFCName").val("");
            $("#FFCNumber").val("");
            $("#Reference").val("");
            $("#liSsiTemplate").select2("val", []);
            $("#spnSsi").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }

    $scope.fnAddAuthorizedParty = function () {
        if ($("#txtAuthorizedParty").val() == undefined || $("#txtAuthorizedParty").val() == "") {
            //pop-up    
            $("#txtAuthorizedParty").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Authorized Party cannot be empty. Please add a valid authorized party",
                html: true,
                width: "250px"
            });

            $("#txtAuthorizedParty").popover("show");
            return;
        }

        $("#txtAuthorizedParty").popover("hide");
        var isExists = false;
        $($scope.authorizedPartyData).each(function (i, v) {
            if ($("#txtAuthorizedParty").val() == v.text) {
                isExists = true;
                return false;
            }
        });
        if (isExists) {
            $("#txtAuthorizedParty").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Authorized Party is already exists. Please enter a valid authorized party",
                html: true,
                width: "250px"
            });
            $("#txtAuthorizedParty").popover("show");
            return;
        }

        $http.post("/Accounts/AddAuthorizedParty", { authorizedParty: $("#txtAuthorizedParty").val() }).then(function (response) {
            notifySuccess("Authorized Party added successfully");
            $scope.onBoardingAccountDetails[$scope.PanelIndex].AuthorizedParty = $("#txtAuthorizedParty").val();
            $scope.fnGetAuthorizedParty($scope.PanelIndex);
            $("#txtAuthorizedParty").val("");
        });

        $("#authorizedPartyModal").modal("hide");
    }

    $scope.fnAddAuthorizedPartyModal = function (panelIndex) {
        $scope.PanelIndex = panelIndex;
        $("#authorizedPartyModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtAuthorizedParty").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }
    angular.element("#txtAuthorizedParty").on("focusin", function () { angular.element("#txtAuthorizedParty").popover("hide"); });

    $scope.fnAddSwiftGroup = function () {
        if ($("#txtSwiftGroup").val() == undefined || $("#txtSwiftGroup").val() == "") {
            //pop-up    
            $("#txtSwiftGroup").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Swift Group cannot be empty. Please add a valid swift group",
                html: true,
                width: "250px"
            });

            $("#txtSwiftGroup").popover("show");
            return;
        }

        $("#txtSwiftGroup").popover("hide");
        var isExists = false;
        $($scope.SwiftGroupData).each(function (i, v) {
            if ($("#txtSwiftGroup").val() == v.text) {
                isExists = true;
                return false;
            }
        });
        if (isExists) {
            $("#txtSwiftGroup").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Swift Group is already exists. Please enter a valid swift group",
                html: true,
                width: "250px"
            });
            $("#txtSwiftGroup").popover("show");
            return;
        }

        if ($("#txtSendersBIC").val() == undefined || $("#txtSendersBIC").val() == "") {
            //pop-up    
            $("#txtSendersBIC").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Senders BIC cannot be empty. Please add a valid senders BIC",
                html: true,
                width: "250px"
            });

            $("#txtSendersBIC").popover("show");
            return;
        }

        $http.post("/Accounts/AddSwiftGroup", { swiftGroup: $("#txtSwiftGroup").val(), senderBic: $("#txtSendersBIC").val().toUpperCase() }).then(function (response) {
            notifySuccess("Swift Group added successfully");
            $scope.onBoardingAccountDetails[$scope.PanelIndex].SwiftGroup = $("#txtSwiftGroup").val();
            $scope.onBoardingAccountDetails[$scope.PanelIndex].SendersBIC = $("#txtSendersBIC").val().toUpperCase();
            $scope.fnGetSwiftGroup($scope.PanelIndex);
            $("#txtSwiftGroup").val("");
            $("#txtSendersBIC").val("");
        });

        $("#swiftGroupModal").modal("hide");
    }

    $scope.fnAddSwiftGroupModal = function (panelIndex) {
        $scope.PanelIndex = panelIndex;
        $("#swiftGroupModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtSwiftGroup").popover("hide");
            $("#txtSendersBIC").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }

    angular.element("#txtSwiftGroup").on("focusin", function () { angular.element("#txtSwiftGroup").popover("hide"); });
    angular.element("#txtSendersBIC").on("focusin", function () { angular.element("#txtSendersBIC").popover("hide"); });
    angular.element("table").off("click", "td a.btn-primary").on("click", "td a.btn-primary", function () {

        var ssitemplateId = $(this).attr("id");
        window.open("/Accounts/SSITemplate?ssiTemplateId=" + ssitemplateId, "_blank");

    });
});