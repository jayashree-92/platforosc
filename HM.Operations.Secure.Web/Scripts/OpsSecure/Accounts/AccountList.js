$("#liAccounts").addClass("active");
HmOpsApp.controller("AccountListController", function ($scope, $http, $timeout, $filter, $q) {
    $("#onboardingMenu").addClass("active");
    var accountTable, accountSsiTemplateTable,accountClearingBrokerTable, tblSsiTemplateRow;
    var myDropZone;

    $scope.onBoardingAccountDetails = [];
    $scope.allAccountList = [];
    $scope.accountDetail = {};
    $scope.OnBoardingContactsDetails = [];
    $scope.beneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }];
    $scope.ultimateBeneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }, { id: "Account Name", text: "Account Name" }];
    $scope.cashSweepData = [{ id: "Yes", text: "Yes" }, { id: "No", text: "No" }];
    $scope.authorizedPartyData = [];// [{ id: "Hedgemark", text: "Hedgemark" }, { id: "Administrator", text: "Administrator" }, { id: "Counterparty", text: "Counterparty" }, { id: "Client", text: "Client" }, { id: "Investment Manager", text: "Investment Manager" }];
    $scope.cashSweepTimeZoneData = [{ id: "EST", text: "EST" }, { id: "GMT", text: "GMT" }, { id: "CET", text: "CET" }];
    $scope.ContactType = [{ id: "Cash", text: "Cash" }, { id: "Custody", text: "Custody" }, { id: "PB Client Service", text: "PB Client Service" }, { id: "Margin", text: "Margin" }];
    $scope.accountPurpose = [];
    $scope.accountStatus = [{ id: "Requested", text: "Requested" }, { id: "Reserved", text: "Reserved" }, { id: "Open", text: "Open" }, { id: "Requested Closure", text: "Requested Closure" }, { id: "Closed", text: "Closed" }];
    $scope.entityTypes = [{ id: "Agreement", text: "Agreement" }, { id: "Agreement (Reporting Only)", text: "Agreement (Reporting Only)" }, { id: "DDA", text: "DDA" }, { id: "Custody", text: "Custody" }];
    $scope.SwiftGroups = [];
    $scope.SwiftGroupData = [];
    var tblAccountDocuments;
    var tblSsiTemplateAssociations;
    var tblAccountCallBacks;
    var tblAccClearingBrokers;
    $scope.onBoardingAccountSSITemplateMap = {};
    $scope.ssiTemplates = [];
    $scope.accountDocuments = [];
    var tblContacts = [];
    var approvedStatus = "Approved";
    //var rejectedStatus = "Rejected";
    var pendingStatus = "Pending Approval";
    var createdStatus = "Created";
    $scope.IsBNYMBroker = false;
    $scope.IsPendingApproval = false;
    $scope.IsApproved = false;
    $scope.IsWireReadOnly = $("#IsWireReadOnly").val() === "true";
    $scope.DisabledAgreementForCashInstructions = ["CDA", "ISDA", "GMRA", "MRA", "MSFTA"];

    function viewAttachmentTable(data) {

        if ($("#documentTable0").hasClass("initialized")) {
            fnDestroyDataTable("#documentTable0");
        }
        tblAccountDocuments = $("#documentTable0").not(".initialized").addClass("initialized").DataTable({
            "bDestroy": true,
            aaData: data,
            "aoColumns": [
                {
                    "sTitle": "File Name",
                    "mData": "FileName",
                    "mRender": function (data) {
                        var href = "/FundAccounts/DownloadAccountFile?accountId=" + $scope.onBoardingAccountId + "&fileName=" + escape(data);
                        return "<a target='_blank' title='click to download this file' href='" + href + "'><i class='glyphicon glyphicon-file' ></i>&nbsp;" + data + "</a>";
                    }
                },
                {
                    "sTitle": "Uploaded By",
                    "mData": "RecCreatedBy", "mRender": function (data) {
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
                        return "<button class='btn btn-danger btn-xs' " + ($scope.IsWireReadOnly ? "disabled='disabled'" : "") + " title='Remove Document'><i class='glyphicon glyphicon-trash'></i></button>";
                    }
                }
            ],
            "deferRender": false,
            "bScrollCollapse": true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "scrollY": 350,
            "order": [[2, "desc"]],
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No files are available for the ssi templates",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Files"
            }
        });

        $timeout(function () {
            tblAccountDocuments.columns.adjust().draw(true);
            $scope.onBoardingAccountDetails[0].onBoardingAccountDocuments = angular.copy(data);
        }, 50);


        $("#documentTable0 tbody a").on("click",
            function (event) {
                event.preventDefault();
                window.location = $(this).attr("href");
            });

        $("#accountDetailCP tbody tr td:last-child button").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowElement = tblAccountDocuments.row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this document from account?", function (result) {
                if (!result) {
                    return;
                } else {
                    $timeout(function () {
                        if (rowElement.onBoardingAccountDocumentId > 0) {
                            $http.post("/FundAccounts/RemoveAccountDocument", { fileName: rowElement.FileName, documentId: rowElement.onBoardingAccountDocumentId }).then(function () {
                                tblAccountDocuments.row(selectedRow).remove().draw();
                                $("#spnAgrCurrentStatus").html("Saved as Draft");
                                $("#hmStatus").show();
                                $scope.onBoardingAccountDetails[0].onBoardingAccountDocuments.pop(rowElement);
                                notifySuccess("Account document has removed successfully");

                            });
                        } else {
                            tblAccountDocuments.row(selectedRow).remove().draw();
                            $("#spnAgrCurrentStatus").html("Saved as Draft");
                            $("#hmStatus").show();
                            $scope.onBoardingAccountDetails[0].onBoardingAccountDocuments.pop(rowElement);
                            notifySuccess("Account document has removed successfully");
                        }
                        $scope.fnGetAccounts();
                    }, 100);
                }
            });
        });
    }

    function viewContactTable(data) {

        if ($("#contactTable0").hasClass("initialized")) {
            fnDestroyDataTable("#contactTable0");
        }
        tblContacts = $("#contactTable0").not(".initialized").addClass("initialized").DataTable({
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
                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">Individual</label>";
                        if (tdata === "Group")
                            return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Group</label>";
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

        $timeout(function () {
            tblContacts.columns.adjust().draw(true);
        }, 1000);
    }

    $scope.fnGetCurrency = function () {
        return $http.get("/FundAccounts/GetAllCurrencies").then(function (response) {
            $scope.currencies = response.data.currencies;

            $("#liCurrency0").select2({
                placeholder: "Select a Currency",
                allowClear: true,
                data: response.data.currencies
            });

            if ($scope.onBoardingAccountDetails[0] != undefined && $scope.onBoardingAccountDetails[0].Currency != null && $scope.onBoardingAccountDetails[0].Currency != "undefined")
                $("#liCurrency0").select2("val", $scope.onBoardingAccountDetails[0].Currency);
        });
    }

    $scope.fnGetCashInstruction = function () {
        return $http.get("/FundAccounts/GetAllCashInstruction").then(function (response) {
            $scope.cashInstructions = response.data.cashInstructions;
            $scope.timeZones = response.data.timeZones;
            $("#liCashInstruction0").select2({
                placeholder: "Select a Cash Instruction",
                allowClear: true,
                data: response.data.cashInstructions
            });

            if ($scope.onBoardingAccountDetails[0] != undefined && $scope.onBoardingAccountDetails[0].CashInstruction != null && $scope.onBoardingAccountDetails[0].CashInstruction != "undefined")
                $("#liCashInstruction0").select2("val", $scope.onBoardingAccountDetails[0].CashInstruction);
        });
    }

    $scope.fnGetBicorAba = function () {
        return $http.get("/FundAccounts/GetAllAccountBicorAba").then(function (response) {
            $scope.accountBicorAba = response.data.accountBicorAba;

            var isAba = $scope.isBicorAba == true ? "ABA" : "BIC";
            $scope.fnToggleBeneficiaryBICorABA(isAba, "Beneficiary");
            $("#liBeneficiaryBICorABA0").select2("val", isAba);

        });
    }

    $scope.fnPreloadAccountData = function () {

        return $http.get("/FundAccounts/GetAccountPreloadData").then(function (response) {
            $scope.funds = response.data.funds;
            $scope.fundsWithAgreements = response.data.fundsWithAgreements;
            $scope.agreements = response.data.agreements;
            $scope.counterpartyFamilies = response.data.counterpartyFamilies;
            $scope.ddaAgreementTypeId = response.data.ddaAgreementTypeId;
            $scope.custodyAgreementTypeId = response.data.custodyAgreementTypeId;
        });
    }



    //$scope.fnInitializeSelect2 = function() {

    //}
    $scope.fnInitPreLoadEvents = function () {


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

        $("#liExposureAgreementType").select2({
            placeholder: "Select an Exposure Type",
            allowClear: false,
            data: []
        });

        $("#liAccountType").select2("val", "");
        $("#liFund").select2("val", "");
        $("#liAgreement").select2("val", "");
        $("#liBroker").select2("val", "");
        if (!$scope.isEdit) {
            $scope.AgreementTypeId = 0;
            $scope.CounterpartyFamilyId = 0;
            $scope.CounterpartyId = 0;
            $scope.AgrementType = "";
            $scope.broker = "";
        }
        $("#spnBroker").hide();
        $("#spnBrokerFamily").hide();
        $("#spnAgreement").hide();


    }

    angular.element(document).on("change", "#liAccountType", function (event) {
        event.stopPropagation();
        $scope.accountType = $(this).val();
        $scope.IsReportingOnly = $scope.accountType == "Agreement (Reporting Only)";
        var thisFunds = [];
        $scope.AgreementTypeId = 0;
        if ($(this).val() != "" && $(this).val() != undefined) {
            if ($(this).val() == "Agreement" || $(this).val() == "Agreement (Reporting Only)") {
                $("#spnBroker").hide();
                $("#spnBrokerFamily").hide();
                $("#spnAgreement").show();
                thisFunds = angular.copy($scope.fundsWithAgreements);
            } else {
                $scope.AgreementTypeId = angular.copy($(this).val() == "DDA" ? $scope.ddaAgreementTypeId : $scope.custodyAgreementTypeId);
                $("#spnBroker").show();
                $("#spnBrokerFamily").show();
                $("#spnAgreement").hide();
                thisFunds = angular.copy($scope.funds);
            }
        } else {
            $scope.AgreementTypeId = 0;
            $("#spnBroker").hide();
            $("#spnBrokerFamily").hide();
            $("#spnAgreement").hide();
        }

        $("#liFund").select2({
            placeholder: "Select a fund",
            allowClear: true,
            data: thisFunds
        });
        $("#liFund").select2("val", "");
        $("#liAgreement").select2("val", "");
        $("#liBroker").select2("val", "");

        $scope.CounterpartyFamilyId = 0;
        $scope.AgrementType = "";
        $scope.broker = "";
    });

    angular.element(document).on("change", "#liFund", function (event) {

        fundId = $(this).val();
        event.stopPropagation();
        if (fundId > 0) {

            var agreements = $filter("filter")(angular.copy($scope.agreements), { 'hmFundId': parseInt(fundId) }, true);
            agreements = $filter("orderBy")(agreements, "text");

            if ($("#liAgreement").data("select2")) {
                $("#liAgreement").select2("destroy");
            }

            $("#liAgreement").select2({
                placeholder: "Select the agreements",
                allowClear: true,
                data: agreements
            });

            $("#liAgreement").select2("val", "");
            $scope.FundName = $(this).select2("data").LegalName;
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
            $("#liAgreement").select2("val", "");
        }
        $scope.FundId = fundId;

    });

    angular.element(document).on("change", "#liAgreement", function (event) {
        event.stopPropagation();
        $scope.AgreementId = $(this).val();
        if ($(this).val() > 0) {
            // Get row details 
            $scope.AgreementTypeId = $(this).select2("data").AgreementTypeId;
            $scope.AgreementType = $(this).select2("data").AgreementType;
            $scope.CounterpartyFamilyId = $(this).select2("data").CounterpartyFamilyId;
            $scope.CounterpartyId = $(this).select2("data").CounterpartyId;

            var broker = $filter("filter")(angular.copy($scope.counterpartyFamilies), { 'CounterpartyFamilyId': $scope.CounterpartyFamilyId }, true)[0];
            if (broker != undefined)
                $scope.broker = broker.text;
            $scope.loadAccountData();
        }
    });

    angular.element(document).on("change", "#liBroker", function (event) {
        event.stopPropagation();
        $scope.CounterpartyId = $(this).val();

        if ($(this).val() > 0) {
            $scope.CounterpartyName = $(this).select2("data").text;
            $scope.CounterpartyFamilyName = $(this).select2("data").familyText;
            $scope.CounterpartyFamilyId = $(this).select2("data").familyId;
            $scope.IsBNYMBroker = $scope.CounterpartyName == "The Bank of New York Mellon";
            $scope.$apply();
            $scope.loadAccountData();
        }
    });

    angular.element(document).on("click", "#btnAgrExpandAllPanel", function (event) {
        angular.element("#collapseContainer .panel-body").collapse("show");
        angular.element("#collapseContainer .panel-heading i.glyphicon-chevron-down").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
    });

    angular.element(document).on("click", "#btnAgrCollapseAllPanel", function (event) {
        angular.element("#collapseContainer .panel-body").collapse("hide");
        angular.element("#collapseContainer .panel-heading i.glyphicon-chevron-up").addClass("glyphicon-chevron-down").removeClass("glyphicon-chevron-up");
    });

    $scope.loadAccountData = function () {
        $scope.copyAccount = {};
        $scope.onBoardingAccountDetails = [];

        $scope.copyAccount.onBoardingAccountId = 0;
        $scope.copyAccount.AccountType = $scope.accountType;
        $scope.copyAccount.IsReportingOnly = $scope.accountType == "Agreement (Reporting Only)";
        $scope.copyAccount.AccountName = $scope.FundName;

        if ($scope.accountType == "Agreement" || $scope.accountType == "Agreement (Reporting Only)") {
            $scope.copyAccount.dmaAgreementOnBoardingId = $scope.AgreementId;
        }

        $scope.copyAccount.hmFundId = $scope.FundId;
        $scope.copyAccount.dmaCounterpartyFamilyId = $scope.CounterpartyFamilyId;
        $scope.copyAccount.dmaCounterpartyId = $scope.CounterpartyId;
        $scope.copyAccount.onBoardingAccountSSITemplateMaps = [];
        $scope.copyAccount.onBoardingAccountDocuments = [];
        $scope.copyAccount.IsReceivingAccountType = ($scope.accountType == "Agreement" || $scope.accountType == "Agreement (Reporting Only)") && $.inArray($scope.AgreementType, $scope.receivingAccountTypes) > -1;
        if ($scope.copyAccount.IsReceivingAccountType || $scope.copyAccount.AuthorizedParty != "Hedgemark")
            $scope.copyAccount.IsReceivingAccount = true;
        else
            $scope.copyAccount.IsReceivingAccount = false;
        $timeout(function () {
            $scope.onBoardingAccountDetails.push($scope.copyAccount);
        }, 50);
    }

    $scope.$on("onRepeatLast", function (scope, element, attrs) {
        $timeout(function () {
            $scope.fnIntialize();

            $timeout(function () {
                $scope.watchAccountDetails = $scope.onBoardingAccountDetails;
                $timeout(function () {
                    $scope.isLoad = false;
                }, 1500);
            }, 1000);
        }, 100);
        $scope.fnSetupSideMenu();
    });

    $scope.fnClearAdvanceSearch = function () {

        if ($("#fundAccountSearchPane").hasClass("in")) {
            $timeout(function () {
                accountTable.searchPanes.clearSelections();
            }, 500);
        }
    }

    $scope.fnGetAccounts = function () {

        $("#btnAddNewAccount").button("loading");

        $http.get("/FundAccounts/GetAllOnBoardingAccount").then(function (response) {

            $scope.agreementTypes = response.data.agreementTypes;
            $scope.receivingAccountTypes = response.data.receivingAccountTypes;
            if (response.data.OnBoardingAccounts.length > 0)
                $("#btnAccountStatusButtons").show();
            $scope.allAccountList = response.data.OnBoardingAccounts;
            $scope.accountDetail = response.data.OnBoardingAccounts[0];
            $scope.clearingBrokersList = response.data.FundAccountClearingBrokers;
            if ($("#accountTable").hasClass("initialized")) {
                accountTable.clear();
                accountTable.rows.add($scope.allAccountList);
                accountTable.draw();

            } else {
                accountTable = $("#accountTable").not(".initialized").addClass("initialized").DataTable({
                    aaData: $scope.allAccountList,
                    "dom": "<'#fundAccountSearchPane.collapse'P><'row'<'col-md-6'i><'col-md-6 pull-right'f>>trI",
                    searchPanes: {
                        cascadePanes: true,
                        viewTotal: true,
                        dataLength: false,
                        //controls: false,
                        layout: "columns-4",
                        columns: [6, 8, 13, 49],
                        orderable: false,
                        //clear: false
                    },

                    "bDestroy": true,
                    "columns": [
                        { "mData": "Account.onBoardingAccountId", "sTitle": "onBoardingAccountId", visible: false },
                        { "mData": "Account.dmaAgreementOnBoardingId", "sTitle": "dmaAgreementOnBoardingId", visible: false },
                        { "mData": "AgreementTypeId", "sTitle": "AgreementTypeId", visible: false },
                        { "mData": "Account.dmaCounterpartyFamilyId", "sTitle": "CounterpartyFamilyId", visible: false },
                        { "mData": "Account.dmaCounterpartyId", "sTitle": "CounterpartyId", visible: false },
                        //{ "mData": "AccountType", "sTitle": "Account Type" },
                        {
                            "mData": "Account.onBoardingAccountId", "sTitle": "SSI Association Status", "mRender":
                                function (tdata, type, row, meta) {
                                    var totalTemplateMaps = row.PendingApprovalMaps + row.ApprovedMaps;
                                    if (totalTemplateMaps == 0) {
                                        return "";
                                    }

                                    //return row.ApprovedMaps + "/" + totalTemplateMaps + " approved";

                                    var totalApproved = (row.ApprovedMaps / totalTemplateMaps) * 100;
                                    var totalPending = (row.PendingApprovalMaps / totalTemplateMaps) * 100;


                                    var taskProgress = "<div class=\"progress\" style=\"margin-bottom: 0px;\">"
                                        + "<div class=\"progress-bar progress-bar-success\"  aria-value=\"" + totalApproved + "\">"
                                        + "<span class=\"checklistProgressText\">" + (row.ApprovedMaps == "0" ? "" : row.ApprovedMaps) + " </span>"
                                        + "</div>"
                                        + "<div class=\"progress-bar progress-bar-warning progress-bar-striped\" aria-value=\"" + totalPending + "\">"
                                        + "<span class=\"checklistProgressText\">" + (row.PendingApprovalMaps == "0" ? "" : row.PendingApprovalMaps) + " </span>"
                                        + "</div>"
                                        + "</div>";

                                    return taskProgress;
                                }

                        },
                        {
                            "mData": "Account.AccountType", "sTitle": "Entity Type",
                            render: {
                                _: function (tData) {
                                    if (tData != null && tData != "undefined") {
                                        switch (tData) {
                                            case "Agreement": return "<label class='label label-success'>" + tData + "</label>";
                                            case "Agreement (Reporting Only)": return "<label class='label label-default'>" + tData + "</label>";
                                            case "DDA": return "<label class='label label-warning'>" + tData + "</label>";
                                            case "Custody": return "<label class='label label-info'>" + tData + "</label>";
                                        }
                                        return "<label class='label label-default'>" + tData + "</label>";
                                    }
                                    return "";
                                },
                                sp: function (tData) { return tData; }
                            },

                            searchPanes: { orthogonal: "sp" }
                        },
                        { "mData": "ClientName", "sTitle": "Client Name" },
                        { "mData": "FundName", "sTitle": "Fund Name" },
                        { "mData": "FundStatus", "sTitle": "Fund Status" },
                        { "mData": "AgreementName", "sTitle": "Agreement Name" },
                        { "mData": "CounterpartyFamilyName", "sTitle": "Counterparty Family" },
                        { "mData": "CounterpartyName", "sTitle": "Counterparty" },
                        { "mData": "Account.AccountName", "sTitle": "Account Name" },
                        { "mData": "AccountNumber", "sTitle": "Account Number" },
                        { "mData": "Account.AccountPurpose", "sTitle": "Account Type" },
                        { "mData": "Account.AccountStatus", "sTitle": "Account Status" },
                        { "mData": "Account.Currency", "sTitle": "Currency" },
                        { "mData": "Account.Description", "sTitle": "Description" },
                        { "mData": "Account.Notes", "sTitle": "Notes" },
                        { "mData": "Account.AuthorizedParty", "sTitle": "Authorized Party" },
                        { "mData": "Account.CashInstruction", "sTitle": "Cash Instruction Mechanism" },
                        { "mData": "Account.SwiftGroup", "sTitle": "Swift Group", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.SwiftGroup : ""; } },
                        { "mData": "Account.SwiftGroup", "sTitle": "Senders BIC", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.SendersBIC : ""; } },

                        { "mData": "Account.CashSweep", "sTitle": "Cash Sweep" },
                        {
                            "mData": "Account.CashSweepTime", "sTitle": "Cash Sweep Time",
                            "mRender": function (tData) {
                                if (tData == "" || tData == null)
                                    return "";

                                return moment(tData).format("LT");
                            }
                        },
                        { "mData": "Account.CashSweepTimeZone", "sTitle": "Cash Sweep Time Zone" },
                        {
                            "mData": "Account.WirePortalCutoff", "sTitle": "Cutoff Time",
                            "mRender": function (tData) {
                                if (tData == null)
                                    return "";

                                return moment(tData.CutoffTime).format("LT");
                            }
                        },
                        { "mData": "Account.HoldbackAmount", "sTitle": "Holdback Amount" },
                        { "mData": "Account.SweepComments", "sTitle": "Sweep Comments" },
                        { "mData": "Account.AssociatedCustodyAcct", "sTitle": "Associated Custody Acct" },
                        { "mData": "Account.PortfolioSelection", "sTitle": "Portfolio Selection" },
                        { "mData": "Account.TickerorISIN", "sTitle": "Ticker/ISIN" },
                        { "mData": "Account.SweepCurrency", "sTitle": "Sweep Currency" },
                        { "mData": "Account.BeneficiaryType", "sTitle": "Beneficiary Type" },
                        { "mData": "Account.Beneficiary", "sTitle": "Beneficiary BIC or ABA", "mRender": function (tData, type, row, meta) { return tData != null ? tData.BICorABA : ""; } },
                        { "mData": "Account.Beneficiary", "sTitle": "Beneficiary Bank/Account Name", "mRender": function (tData, type, row, meta) { return tData != null ? tData.BankName : ""; } },
                        { "mData": "Account.BeneficiaryAccountNumber", "sTitle": "Beneficiary Account Number" },
                        { "mData": "Account.UltimateBeneficiaryType", "sTitle": "Ultimate Beneficiary Type" },
                        { "mData": "Account.UltimateBeneficiary", "sTitle": "Ultimate Beneficiary BIC or ABA", "mRender": function (tData, type, row, meta) { return tData != null ? tData.BICorABA : ""; } },
                        { "mData": "Account.UltimateBeneficiary", "sTitle": "Ultimate Beneficiary Bank Name", "mRender": function (tData, type, row, meta) { return tData != null ? tData.BankName : ""; } },
                        { "mData": "Account.UltimateBeneficiaryAccountName", "sTitle": "Ultimate Beneficiary Account Name" },
                        { "mData": "Account.FFCName", "sTitle": "FFC Name" },
                        { "mData": "Account.FFCNumber", "sTitle": "FFC Number" },
                        { "mData": "Account.MarginAccountNumber", "sTitle": "Margin Account Number" },
                        { "mData": "Account.TopLevelManagerAccountNumber", "sTitle": "Top Level Manager Account" },
                        { "mData": "Account.Reference", "sTitle": "Reference" },
                        {
                            "mData": "Account.onBoardingAccountStatus", "sTitle": "Status",
                            "mRender": function (tData) {
                                if (tData != null && tData != "undefined") {
                                    switch (tData) {
                                        case "Approved": return "<label class='label label-success'>" + tData + "</label>";
                                        case "Pending Approval": return "<label class='label label-warning'>" + tData + "</label>";
                                        case "Created": return "<label class='label label-default'>" + "Saved As Draft" + "</label>";
                                    }
                                    return "<label class='label label-default'>" + tData + "</label>";
                                }
                                return "";
                            }
                        },
                        { "mData": "Account.StatusComments", "sTitle": "Comments" },
                        { "mData": "Account.CreatedBy", "sTitle": "Created By" },// "mRender": function (data) { return humanizeEmail(data); } },
                        { "mData": "Account.CreatedAt", "sTitle": "Created Date", "mRender": renderDotNetDateAndTime },
                        { "mData": "Account.UpdatedBy", "sTitle": "Last Modified By" },// "mRender": function (data) { return humanizeEmail(data); } },
                        { "mData": "Account.UpdatedAt", "sTitle": "Last Modified At", "mRender": renderDotNetDateAndTime },
                        { "mData": "Account.ApprovedBy", "sTitle": "Approved By" },// "mRender": function (data) { return humanizeEmail(data == null ? "" : data); } }

                    ],
                    "oLanguage": {
                        "sSearch": "",
                        "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ Accounts",
                        "sInfoFiltered": " - filtering from _MAX_ Accounts"
                    },
                    "createdRow": function (row, data) {

                        switch (data.Account.onBoardingAccountStatus) {
                            case "Approved":
                                $(row).addClass("success");
                                break;
                            case "Pending Approval":
                                $(row).addClass("warning");
                                break;
                        }

                    },
                    "deferRender": true,
                    "scroller": true,
                    "orderClasses": false,
                    "sScrollX": "100%",
                    "scrollY": window.innerHeight - 400,
                    "sScrollXInner": "100%",
                    "bScrollCollapse": true,
                    "order": [[52, "desc"]],
                    "rowCallback": function (row, data) {
                    },
                    "drawCallback": function (settings) {
                        $scope.fnLoadProgress();
                    },
                    iDisplayLength: -1
                });
            }
            var searchText = decodeURI(getUrlParameter("searchText"));

            if (searchText != "" && searchText != undefined && searchText != "undefined") {
                $timeout(function () {
                    accountTable.search(searchText).draw(false);
                }, 50);
            } else {
                window.setTimeout(function () {
                    accountTable.columns.adjust().draw(true);
                }, 100);
            }

            $("#btnAddNewAccount").button("reset");
        });
    }

    $scope.fnLoadProgress = function () {

        $timeout(function () {
            $(".progress-bar").each(function () {
                var now = $(this).attr("aria-value");
                var siz = (now) * 100 / (100);
                $(this).css("width", siz + "%");
            });
        }, 300);

    };

    $(document).on("click", "#accountTable tbody tr", function () {
        $("#accountTable tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $("#btnAccountStatusButtons button").addClass("disabled");
        var rowElement = accountTable.row(this).data();

        var account = rowElement.Account;


        $scope.onBoardingAccountId = account.onBoardingAccountId;
        $scope.FundId = account.hmFundId;
        $scope.AgreementId = account.dmaAgreementOnBoardingId;
        $scope.CounterpartyFamilyId = account.dmaCounterpartyFamilyId;
        $scope.CounterpartyId = account.dmaCounterpartyId;
        $scope.AccountType = account.AccountType;
        $scope.IsReportingOnly = $scope.AccountType == "Agreement (Reporting Only)";
        $scope.CounterpartyName = rowElement.CounterpartyName;
        $scope.AgreementTypeId = rowElement.AgreementTypeId;
        $scope.AccountStatus = account.onBoardingAccountStatus;

        if (account.onBoardingAccountStatus == pendingStatus && account.CreatedBy != $("#userName").val() && account.UpdatedBy != $("#userName").val()) {
            $("#btnAccountStatusButtons button[id='approve']").removeClass("disabled");
            $scope.IsPendingApproval = true;
        }
        if (account.onBoardingAccountStatus == approvedStatus)
            $scope.IsApproved = true;
        if (account.onBoardingAccountStatus == createdStatus) {
            $("#btnAccountStatusButtons button[id='requestForApproval']").removeClass("disabled");
        }
        if (account.onBoardingAccountStatus != createdStatus) {
            $("#btnAccountStatusButtons button[id='revert']").removeClass("disabled");
        }
        $scope.IsBNYMBroker = $scope.CounterpartyName == "The Bank of New York Mellon";

        $("#btnEdit").prop("disabled", false);

        $("#btnDel").prop("disabled", false);
    });


    $(document).on("dblclick", "#accountTable tbody tr", function () {
        var rowElement = accountTable.row(this).data();
        $scope.fnEditAccountDetails(rowElement);
    });


    var initAccount = function () {
        $q.all([$scope.fnGetBicorAba(null), $scope.fnGetCurrency(), $scope.fnGetCashInstruction()]).then($scope.fnPreloadAccountData);
    }
    $scope.fnGetAccounts();
    initAccount();


    $scope.fnAddAccountDetail = function () {
        $scope.watchAccountDetails = [];
        $scope.onBoardingAccountDetails = [];
        $scope.fnPreloadAccountData().then($scope.fnInitPreLoadEvents());
        $scope.isAuthorizedUserToApprove = false;
        $scope.FundRegistedAddress = "";
        $scope.isEdit = false;
        $scope.isStatusUpdate = false;
        $scope.CounterpartyFamilyName = "";
        $("#accountModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            if (!$scope.isStatusUpdate) {
                $scope.onBoardingAccountDetails = [];
                $scope.accountDetail = {};
            }
        }).off("shown.bs.modal").on("shown.bs.modal", function () {
            if (!$scope.isStatusUpdate)
                angular.element("#basicDetailCP").collapse("show");
            $(window).scrollTop(0);

            $scope.fnSetupSideMenu();
        });
    }


    $scope.fnEditAccountDetails = function (rowElement) {
        $scope.watchAccountDetails = [];
        $scope.onBoardingAccountDetails = [];

        var accDetails = rowElement.Account;
        $scope.accountDetail = accDetails;
        $scope.onBoardingAccountId = accDetails.onBoardingAccountId;
        $scope.AgreementId = accDetails.dmaAgreementOnBoardingId;
        $scope.FundId = accDetails.hmFundId;
        $scope.AgreementTypeId = rowElement.AgreementTypeId;
        $scope.CounterpartyFamilyId = accDetails.dmaCounterpartyFamilyId;
        $scope.CounterpartyId = accDetails.dmaCounterpartyId;
        $scope.AccountType = accDetails.AccountType;
        $scope.IsReportingOnly = $scope.AccountType == "Agreement (Reporting Only)";
        $scope.CounterpartyFamilyName = rowElement.CounterpartyFamilyName;
        $scope.CounterpartyName = rowElement.CounterpartyName;
        $scope.isEdit = true;
        $scope.fundName = accDetails.FundName;
        $scope.isLoad = true;
        $scope.IsKeyFieldsChanged = accDetails.IsKeyFieldsChanged;

        if ($scope.AgreementTypeId > 0) {
            $scope.AgreementType = $.grep($scope.agreementTypes, function (v) { return v.id == $scope.AgreementTypeId; })[0].text;
        }

        $http.get("/FundAccounts/GetOnBoardingAccount?accountId=" + $scope.onBoardingAccountId).then(function (response) {
            var account = response.data.OnBoardingAccount;

            //$(".accntActions button").hide();
            $scope.isAuthorizedUserToApprove = response.data.isAuthorizedUserToApprove;
            $scope.FundRegistedAddress = response.data.registedAddress;

            if ($("#spnAgrCurrentStatus").html() == pendingStatus && val[0].UpdatedBy != $("#userName").val())
                $("#btnApprove").show();

            if (account.CashSweepTime != null && account.CashSweepTime != "" && account.CashSweepTime != undefined) {
                account.CashSweepTime = new Date(2014, 0, 1, account.CashSweepTime.Hours, account.CashSweepTime.Minutes, account.CashSweepTime.Seconds);
            }

            if (account.WirePortalCutoff.CutoffTime != null && account.WirePortalCutoff.CutoffTime != "" && account.WirePortalCutoff.CutoffTime != undefined) {
                account.WirePortalCutoff.CutoffTime = new Date(2014, 0, 1, account.WirePortalCutoff.CutoffTime.Hours, account.WirePortalCutoff.CutoffTime.Minutes, account.WirePortalCutoff.CutoffTime.Seconds);
            }
            account.CreatedAt = moment(account.CreatedAt).format("YYYY-MM-DD HH:mm:ss");

            var agreementType = {};
            if ($scope.AgreementTypeId > 0) {
                agreementType = $.grep($scope.agreementTypes, function (v) { return v.id == $scope.AgreementTypeId; })[0];
            }

            if (agreementType != undefined && (agreementType.text == "PB" || agreementType.text == "FCM" || $scope.AccountType == "DDA")) {
                $scope.accountPurpose = [{ id: "Cash", text: "Cash" }, { id: "Margin", text: "Margin" }];
            } else {
                $scope.accountPurpose = [{ id: "Pledge Account", text: "Pledge Account" }, { id: "Return Account", text: "Return Account" }, { id: "Both", text: "Both" }];
            }
            account.IsReceivingAccountType = account.AccountType != undefined && (account.AccountType == "Agreement" || account.AccountType == "Agreement (Reporting Only)") && $.inArray(agreementType.text, $scope.receivingAccountTypes) > -1;
            if (account.IsReceivingAccountType || account.AuthorizedParty != "Hedgemark")
                account.IsReceivingAccount = true;
            else
                account.IsReceivingAccount = false;
            $scope.onBoardingAccountDetails[0] = account;
            $timeout(function () {
                $("#chkIsExcludedFromMarginCheck").bootstrapToggle();
                $("#chkIsExcludedFromMarginCheck").prop("checked", account.IsExcludedFromTreasuryMarginCheck).change();
            }, 200);
            //$scope.accountDetail = account;
        });
        $scope.fnPreloadAccountData().then($scope.fnInitPreLoadEvents());
        $scope.isStatusUpdate = false;
        $("#accountModal").modal({
            show: true,
            keyboard: true,
            backdrop: "static"
        }).on("hidden.bs.modal", function () {

            //$scope.onBoardingAccountDetails = [];
            //$scope.accountDetail = {};
            //$scope.fnGetAccounts();
            //var searchText = $('#accountListDiv input[type="search"]').val();
            //window.location.href = "/FundAccounts/Index?searchText=" + searchText;

        }).off("shown.bs.modal").on("shown.bs.modal", function () {
            if (!$scope.isStatusUpdate) {
                angular.element("#basicDetailCP").collapse("hide");
            }


            $timeout(function () {
                $(window).scrollTop(0);

                $("#txtHoldbackAmount").numericEditor({
                    bAllowNegative: false,
                    fnFocusInCallback: function () {
                        if ($(this).text() == "0")
                            $(this).html("");
                    },
                    fnFocusOutCallback: function () {
                        var number = Math.abs($.convertToNumber($(this).val(), true));
                        $scope.onBoardingAccountDetails[0].HoldbackAmount = number;
                        $scope.$apply();
                    }
                });
            }, 100);

        });
    }

    $scope.fnUpdateAccountStatus = function (status, statusAction) {

        //Status need not be updated for Reporting Only Type
        if ($scope.onBoardingAccountDetails[0].AccountType == "Agreement (Reporting Only)")
            return;

        if ((statusAction == "Request for Approval" || statusAction == "Approve") && $scope.accountDocuments.length == 0) {
            notifyWarning("Please upload document to approve/request to approve account");
            return;
        }

        if (statusAction == "Approve" && ($scope.CallBackChecks == undefined || $scope.CallBackChecks.length == 0) && !$scope.IsBNYMBroker && $scope.onBoardingAccountDetails[0].AccountStatus != "Closed") {
            notifyWarning("Please add at-least one Callback check to approve account");
            return;
        }

        $scope.AccountStatus = status;
        var confirmationMsg = "Are you sure you want to " + ((statusAction === "Request for Approval") ? "<b>request</b> for approval of" : "<b>" + (statusAction == "Revert" ? "save changes or sending approval for" : statusAction) + "</b>") + " the selected account?";
        if (statusAction == "Request for Approval") {
            $("#btnSaveComment").addClass("btn-warning").removeClass("btn-success").removeClass("btn-primary");
            $("#btnSaveComment").html('<i class="glyphicon glyphicon-share-alt"></i>&nbsp;Request for approval');
        } else if (statusAction == "Approve") {

            if ($scope.IsKeyFieldsChanged && !$scope.IsBNYMBroker && $scope.onBoardingAccountDetails[0].AccountStatus != "Closed") {
                notifyWarning("Please add one Callback check to approve account");
                return;
            }
            $("#btnSaveComment").removeClass("btn-warning").addClass("btn-success").removeClass("btn-primary");
            $("#btnSaveComment").html('<i class="glyphicon glyphicon-ok"></i>&nbsp;Approve');
        }
        else if (statusAction == "Revert") {
            $("#btnSaveComment").removeClass("btn-warning").removeClass("btn-success").addClass("btn-primary");
            $("#btnSaveComment").html('<i class="glyphicon glyphicon-floppy-save"></i>&nbsp;Save Changes');
            $("#btnSendApproval").show();
        }

        $("#pMsg").html(confirmationMsg);
        $scope.isStatusUpdate = true;
        $("#UpdateAccountStatusModal").modal("show");
        $("#accountModal").modal("hide");
    }
    $scope.isHigherStatus = false;
    $scope.fnSaveAccountStatus = function () {
        $timeout(function () {
            $scope.isHigherStatus = true;
            if ($scope.validateAccount($scope.AccountStatus))
                $q.all([$scope.fnUpdateAccount(false)]).then(function () {
                    $http.post("/FundAccounts/UpdateAccountStatus", { accountStatus: $scope.AccountStatus, accountId: $scope.onBoardingAccountId, comments: $("#statusComments").val().trim() }).then(function () {
                        notifySuccess("Account  " + $scope.AccountStatus.toLowerCase() + " successfully");
                        $scope.fnGetAccounts();

                    });
                    $("#btnSendApproval").hide();
                    $("#UpdateAccountStatusModal").modal("hide");
                });
            else {
                $("#UpdateAccountStatusModal").modal("hide");
                $("#accountModal").modal({
                    show: true,
                    keyboard: true,
                    backdrop: "static"
                });
            }
        }, 100);
    }

    $scope.fnSendApprovalAccountStatus = function () {
        $scope.AccountStatus = pendingStatus;
        $scope.fnSaveAccountStatus();
    }
    $scope.fnOpenAccountModal = function () {
        $scope.isStatusUpdate = true;
        $("#accountModal").modal({
            show: true,
            keyboard: true,
            backdrop: "static"
        });
    }
    $scope.fnEditAccount = function () {
        var rowElement = accountTable.row(".info").data();
        $scope.fnEditAccountDetails(rowElement);
    }

    $scope.fnCreateAccount = function () {
        window.location.assign("/FundAccounts/Account?fundId=0&brokerId=0&agreementId=0&accountType=");
    }

    $scope.fnDeleteAccount = function () {
        showMessage("Are you sure do you want to delete account? ", "Delete Account", [
            {
                label: "Delete",
                className: "btn btn-sm btn-danger",
                callback: function () {
                    $http.post("/FundAccounts/DeleteAccount", { onBoardingAccountId: $scope.onBoardingAccountId }).then(function () {
                        accountTable.row(".info").remove().draw();
                        notifySuccess("Delete successfull");
                        $scope.onBoardingAccountId = 0;
                        $("#btnEdit").prop("disabled", true);
                        $("#btnDel").prop("disabled", true);
                    });
                }
            },
            {
                label: "Cancel",
                className: "btn btn-sm btn-default"
            }
        ]);
    }

    //Export all Accounts
    $scope.fnExportAllAccountlist = function () {
        window.location.assign("/FundAccounts/ExportAllAccountlist");
    }

    $scope.validateAccount = function (status) {
        if (!$scope.accountForm.$valid) {
            if ($scope.accountForm.$error.required == undefined)
                notifyError("FFC Name, FFC Number, Reference, Bank Name, Bank Address & Account Names can only contain ?:().,'+- characters");
            else {
                var message = "";
                angular.forEach($scope.accountForm.$error.required, function (ele, ind) {
                    message += ele.$name + ", ";
                });
                notifyError("Please fill in the required fields " + message.substring(0, message.length - 2));
            }
            return false;
        }
        var isAccountNameEmpty = false;

        var value = $scope.onBoardingAccountDetails[0];
        value.IsKeyFieldsChanged = $scope.IsKeyFieldsChanged;
        value.Description = $("#liAccountDescriptions0").val();

        if ($("#cashSweep0").val() == "Yes") {
            var cashSweepTimeValue = "#cashSweepTime0";
            value.CashSweepTime = $(cashSweepTimeValue).val();
            var cashSweepTimeZoneValue = "#cashSweepTimeZone0";
            value.CashSweepTimeZone = $(cashSweepTimeZoneValue).val();
        }
        else {
            value.CashSweepTime = "";
            value.CashSweepTimeZone = "";
        }
        value.CutoffTime = $("#cutOffTime0").val();
        value.SendersBIC = $("#txtSender0").val();

        value.BeneficiaryBankName = $("#beneficiaryBankName0").val();
        value.BeneficiaryBankAddress = $("#beneficiaryBankAddress0").val();

        value.IntermediaryBankName = $("#intermediaryBankName0").val();
        value.IntermediaryBankAddress = $("#intermediaryBankAddress0").val();

        var data = $("#liAccountModule_0").select2("data") == undefined ? [] : $("#liAccountModule_0").select2("data");
        var allsIds = data.map(s => { return s.id });
        value.AccountModule = allsIds.length > 0 ? data.map(s => { return s.id }).join(",") : "";

        value.UltimateBeneficiaryBankName = $("#ultimateBankName0").val();
        value.UltimateBeneficiaryBankAddress = $("#ultimateBankAddress0").val();
        if (status != "" && status != undefined)
            value.onBoardingAccountStatus = status;
        if (value.UltimateBeneficiaryType == "Account Name" &&
            (value.UltimateBeneficiaryAccountName == null || value.UltimateBeneficiaryAccountName == ""))
            isAccountNameEmpty = true;
        if (value.IsReceivingAccount)
            value.onBoardingAccountSSITemplateMaps = [];

        if (isAccountNameEmpty) {
            notifyWarning("Account name field is required");
            return false;
        }
        return true;
    }

    //Save Account
    $scope.fnSaveAccount = function (status) {
        $timeout(function () {
            $scope.isHigherStatus = false;
            if ($scope.validateAccount(status))
                $scope.fnUpdateAccount();
        }, 100);
    }
    $scope.fnUpdateAccount = function () {

        return $http.post("/FundAccounts/AddAccounts", { onBoardingAccounts: $scope.onBoardingAccountDetails }).then(function () {
            if (!$scope.isHigherStatus)
                notifySuccess("Account Saved successfully");
            $scope.onBoardingAccountDetails = [];
            $scope.accountDetail = {};
            $scope.fnGetAccounts();

            $("#accountModal").modal("hide");
        });
    }

    $scope.fnCashSweep = function (cashSweep) {
        if (cashSweep == "Yes")
            $(".cashSweepTimeDiv0").show();
        else
            $(".cashSweepTimeDiv0").hide();
    }

    $scope.fnGetAuthorizedParty = function () {
        $http.get("/FundAccounts/GetAllAuthorizedParty").then(function (response) {
            $scope.authorizedPartyData = response.data.AuthorizedParties;

            $("#liAuthorizedParty0").select2({
                placeholder: "Select a Authorized Party",
                allowClear: true,
                data: response.data.AuthorizedParties
            });

            if ($scope.onBoardingAccountDetails[0].AuthorizedParty != null && $scope.onBoardingAccountDetails[0].AuthorizedParty != "undefined") {
                $("#liAuthorizedParty0").select2("val", $scope.onBoardingAccountDetails[0].AuthorizedParty);
                $scope.fnAuthorizedPartyChange();
            }

        });
    }

    $scope.fnGetSwiftGroup = function () {
        $http.get("/FundAccounts/GetAllRelatedSwiftGroup?brokerId=" + $scope.onBoardingAccountDetails[0].dmaCounterpartyFamilyId).then(function (response) {
            $scope.SwiftGroups = response.data.swiftGroups;
            $scope.SwiftGroupData = response.data.SwiftGroupData;


            $("#liSwiftGroup0").select2({
                placeholder: "Select a Swift Group",
                allowClear: true,
                data: response.data.SwiftGroupData
            });

            if ($scope.onBoardingAccountDetails[0].SwiftGroupId != null && $scope.onBoardingAccountDetails[0].SwiftGroupId != "undefined") {
                $("#liSwiftGroup0").select2("val", $scope.onBoardingAccountDetails[0].SwiftGroupId);
            }
            else if (!$scope.isEdit) {
                $("#liSwiftGroup0").select2("val", response.data.SwiftGroupData[0] != undefined ? response.data.SwiftGroupData[0].id : null);
            }

            $scope.fnOnSwiftGroupChange($("#liSwiftGroup0").select2("val"));

        });
    }

    $scope.fnOnContactNameChange = function (contacts) {

        if (contacts != "" && contacts != "undefined") {
            names = contacts.split(",");
            var onboardingContacts = $filter("filter")(($scope.OnBoardingContactsDetails), function (c) {
                return $.inArray(c.id.toString(), names) > -1;
            });
            $scope.onBoardingAccountDetails[0].ContactName = contacts;
            viewContactTable(onboardingContacts);
            $scope.fnUpdateContacts(contacts)
        }
    }
    $scope.fnOnSwiftGroupChange = function (swiftGroup) {
        $scope.onBoardingAccountDetails[0].SwiftGroupId = swiftGroup;
        var swData = $.grep($scope.SwiftGroups, function (v) { return v.hmsSwiftGroupId == swiftGroup; })[0];
        if (swData != undefined) {
            $scope.swiftGroupInfo = swData;
            $("#txtSender0").val(swData.SendersBIC);
        }
        else {
            $scope.swiftGroupInfo = undefined;
            $("#txtSender0").val("");
        }
    }

    $scope.fnAuthorizedPartyChange = function () {

        if ($scope.onBoardingAccountDetails[0].AuthorizedParty != "Hedgemark") {
            $scope.onBoardingAccountDetails[0].IsReceivingAccount = true;
            $scope.onBoardingAccountDetails[0].AccountModule = null;
            $scope.onBoardingAccountDetails[0].SwiftGroupId = null;
            $scope.onBoardingAccountDetails[0].SwiftGroup = null;
            $scope.onBoardingAccountDetails[0].CashSweepTime = null;
            $scope.onBoardingAccountDetails[0].CashSweepTimeZone = null;
            $scope.onBoardingAccountDetails[0].CashSweep = "No";
            $("#liAccountModule_0").select2("val", null);
            $("#liSwiftGroup0").select2("val", null);
            $("#cashSweep0").select2("val", "No").trigger("change");
        }
        else
            $scope.onBoardingAccountDetails[0].IsReceivingAccount = angular.copy($scope.onBoardingAccountDetails[0].IsReceivingAccountType);
    }

    $scope.fnCutOffTime = function (currency, cashInstruction) {

        $http.get("/FundAccounts/GetCutoffTime?cashInstruction=" + cashInstruction + "&currency=" + currency).then(function (response) {
            var cutOff = response.data.cutOffTime;

            $scope.onBoardingAccountDetails[0].WirePortalCutoff = {};
            $scope.onBoardingAccountDetails[0].WirePortalCutoff.CutoffTime = new Date(2014, 0, 1, 0, 0, 0);
            $scope.onBoardingAccountDetails[0].WirePortalCutoff.CutOffTimeZone = "EST";

            if (cutOff != undefined && cutOff != "") {

                $scope.onBoardingAccountDetails[0].WirePortalCutoff.CutoffTime = new Date(2014, 0, 1, cutOff.CutoffTime.Hours, cutOff.CutoffTime.Minutes, cutOff.CutoffTime.Seconds);
                $scope.onBoardingAccountDetails[0].WirePortalCutoff.DaystoWire = cutOff.DaystoWire;
                $scope.onBoardingAccountDetails[0].WirePortalCutoff.CutOffTimeZone = cutOff.CutOffTimeZone;
                $scope.onBoardingAccountDetails[0].WirePortalCutoff.hmsWirePortalCutoffId = cutOff.hmsWirePortalCutoffId;
                $scope.onBoardingAccountDetails[0].WirePortalCutoffId = cutOff.hmsWirePortalCutoffId;
            }
            else {
                $("#cutOffTime0").val("");
                $("#wireDays0").val("");
            }

            $scope.onBoardingAccountDetails[0].Currency = currency;
            $scope.onBoardingAccountDetails[0].CashInstruction = cashInstruction;
        });
    }


    $scope.fnGetBankDetails = function (biCorAbaValue, id) {
        $timeout(function () {
            var accountBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.BICorABA == biCorAbaValue; })[0];

            switch (id) {
                case "Beneficiary":
                    $scope.onBoardingAccountDetails[0].Beneficiary = {};

                    $scope.onBoardingAccountDetails[0].BeneficiaryBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.onBoardingAccountDetails[0].Beneficiary.onBoardingAccountBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.onBoardingAccountDetails[0].Beneficiary.BICorABA = accountBicorAba == undefined ? "" : accountBicorAba.BICorABA;
                    $scope.onBoardingAccountDetails[0].Beneficiary.BankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.onBoardingAccountDetails[0].Beneficiary.BankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
                case "Intermediary":
                    $scope.onBoardingAccountDetails[0].Intermediary = {};
                    $scope.onBoardingAccountDetails[0].IntermediaryBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.onBoardingAccountDetails[0].Intermediary.onBoardingAccountBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.onBoardingAccountDetails[0].Intermediary.BICorABA = accountBicorAba == undefined ? "" : accountBicorAba.BICorABA;
                    $scope.onBoardingAccountDetails[0].Intermediary.BankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.onBoardingAccountDetails[0].Intermediary.BankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
                case "UltimateBeneficiary":
                    $scope.onBoardingAccountDetails[0].UltimateBeneficiary = {};
                    $scope.onBoardingAccountDetails[0].UltimateBeneficiaryBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.onBoardingAccountDetails[0].UltimateBeneficiary.onBoardingAccountBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.onBoardingAccountDetails[0].UltimateBeneficiary.BICorABA = accountBicorAba == undefined ? "" : accountBicorAba.BICorABA;
                    $scope.onBoardingAccountDetails[0].UltimateBeneficiary.BankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.onBoardingAccountDetails[0].UltimateBeneficiary.BankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
            }
        }, 100);

    }

    $scope.fnToggleBeneficiaryBICorABA = function (item, id) {
        $timeout(function () {

            var isAba = (item == "ABA");

            switch (id) {
                case "Beneficiary":
                    //$scope.onBoardingAccountDetails[0].IsBeneficiaryABA = $("#btnBeneficiaryBICorABA" + 0).prop("checked");

                    var accountBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                    var accountBicorAbaData = [];
                    $.each(accountBicorAba, function (key, value) {
                        accountBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                    });

                    accountBicorAbaData = $filter("orderBy")(accountBicorAbaData, "text");

                    if ($("#liBeneficiaryBICorABA0").data("select2")) {
                        $("#liBeneficiaryBICorABA0").select2("destroy");
                    }
                    $("#liBeneficiaryBICorABA0").select2({
                        placeholder: "Select a beneficiary BIC or ABA",
                        allowClear: true,
                        data: accountBicorAbaData
                    });
                    break;
                case "Intermediary":
                    //$scope.onBoardingAccountDetails[0].IsIntermediaryABA = $("#btnIntermediaryBICorABA" + 0).prop("checked");
                    var intermediaryBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                    var intermediaryBicorAbaData = [];
                    $.each(intermediaryBicorAba, function (key, value) {
                        intermediaryBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                    });

                    intermediaryBicorAbaData = $filter("orderBy")(intermediaryBicorAbaData, "text");

                    if ($("#liIntermediaryBICorABA0").data("select2")) {
                        $("#liIntermediaryBICorABA0").select2("destroy");
                    }
                    $("#liIntermediaryBICorABA0").select2({
                        placeholder: "Select a intermediary BIC or ABA",
                        allowClear: true,
                        data: intermediaryBicorAbaData
                    });
                    break;
                case "UltimateBeneficiary":
                    //$scope.onBoardingAccountDetails[0].IsUltimateBeneficiaryABA = $("#btnUltimateBICorABA" + 0).prop("checked");
                    $scope.onBoardingAccountDetails[0].UltimateBeneficiaryType = item;

                    if (item == "Account Name") {
                        $("#divUltimateBeneficiaryBICorABA0").hide();
                        $("#ultimateBankName0").hide();
                        $("#ultimateBankAddress0").hide();
                        $("#accountName0").show();
                        $scope.onBoardingAccountDetails[0].UltimateBeneficiary = {};
                        //$scope.onBoardingAccountDetails[0].UltimateBeneficiaryBICorABA = null;
                        //$scope.onBoardingAccountDetails[0].UltimateBeneficiaryBankName = null;
                        //$scope.onBoardingAccountDetails[0].UltimateBeneficiaryBankAddress = null;
                        return;
                    } else {
                        $("#divUltimateBeneficiaryBICorABA0").show();
                        $("#ultimateBankName0").show();
                        $("#ultimateBankAddress0").show();
                        $("#accountName0").hide();
                        $scope.onBoardingAccountDetails[0].UltimateBeneficiaryAccountName = null;
                    }
                    var ultimateBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                    var ultimateBicorAbaData = [];
                    $.each(ultimateBicorAba, function (key, value) {
                        ultimateBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                    });

                    ultimateBicorAbaData = $filter("orderBy")(ultimateBicorAbaData, "text");

                    if ($("#liUltimateBeneficiaryBICorABA0").data("select2")) {
                        $("#liUltimateBeneficiaryBICorABA0").select2("destroy");
                    }
                    $("#liUltimateBeneficiaryBICorABA0").select2({
                        placeholder: "Select a ultimate beneficiary BIC or ABA",
                        allowClear: true,
                        data: ultimateBicorAbaData
                    });

                    break;
            }
        }, 100);
    }

    function toggleChevron(e) {
        $(e.target)
            .find("i.indicator")
            .toggleClass("glyphicon-chevron-down").toggleClass("glyphicon-chevron-up");
        // $("html, body").animate({ scrollTop: $(e.target).offset().top - 10 }, "slow");
    }

    $scope.fnGetAccountDescriptions = function () {
        var agrmTypeId = $scope.AccountType == "DDA" ? $scope.ddaAgreementTypeId : $scope.AccountType == "Custody" ? $scope.custodyAgreementTypeId : $scope.AgreementTypeId;
        $http.get("/FundAccounts/GetAccountDescriptionsByAgreementTypeId?agreementTypeId=" + agrmTypeId).then(function (response) {
            $scope.AccountDescriptions = response.data.accountDescriptions;
            $("#liAccountDescriptions0").select2({
                placeholder: "Select Description",
                allowClear: true,
                data: response.data.accountDescriptions
            });

            if ($scope.AgreementTypeId > 0)
                $("#liAccountDescriptions0").val($scope.onBoardingAccountDetails[0].Description);
        });
    }

    $scope.fnGetAccountModules = function () {
        $http.get("/FundAccounts/GetAccountModules").then(function (response) {
            $scope.accountModules = response.data.accountModules;
            $("#liAccountModule_0").select2({
                placeholder: "Select Modules",
                multiple: true,
                allowClear: true,
                data: response.data.accountModules,
                formatResult: formatResult,
                formatSelection: formatResult
            });
            $("#liAccountModule_0").val($scope.onBoardingAccountDetails[0].AccountModule);
        });
    }

    function formatResult(selectData) {
        var stat = $filter("filter")($scope.accountModules, { 'id': selectData.id }, true)[0];
        return selectData.text + "&nbsp;&nbsp;<label class='label " + (selectData.report == "Collateral" ? " label-info" : "label-default") + " shadowBox'>" + selectData.report + "</label>";
    }

    $scope.fnGetAccountReports = function () {
        $http.get("/FundAccounts/GetAccountReports").then(function (response) {
            $scope.accountReports = response.data.accountReports;
            $("#liAccountReport").select2({
                placeholder: "Select Modules",
                data: response.data.accountReports
            });
            $("#liAccountReport").select2("val", $scope.accountReports[0].id);
        });
    }

    $scope.addAccountDetail = function () {
        if ($("#txtDetail").val() == undefined || $("#txtDetail").val() == "") {
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
                if ($("#txtDetail").val() == v.text) {
                    isExists = true;
                    return false;
                }
            });
        }
        else {
            $($scope.accountModules).each(function (i, v) {
                if ($("#txtDetail").val() == v.text) {
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
            var agrmTypeId = $scope.AccountType == "DDA" ? $scope.ddaAgreementTypeId : $scope.AccountType == "Custody" ? $scope.custodyAgreementTypeId : $scope.AgreementTypeId;
            $http.post("/FundAccounts/AddAccountDescriptions", { accountDescription: $("#txtDetail").val(), agreementTypeId: agrmTypeId }).then(function (response) {
                notifySuccess("Description added successfully");
                $scope.onBoardingAccountDetails[0].Description = $("#txtDetail").val();
                $scope.fnGetAccountDescriptions();
            });
        }
        else {
            $http.post("/FundAccounts/AddAccountModule", { reportId: $("#liAccountReport").select2("val"), accountModule: $("#txtDetail").val() }).then(function (response) {
                notifySuccess("Module added successfully");
                $scope.fnGetAccountModules();
            });
        }

        $("#accountDetailModal").modal("hide");
    }

    $scope.fnAddAccountDetailModal = function (detail) {
        $scope.detail = detail;
        //$scope.scrollPosition = $(window).scrollTop();
        //$("#txtGoverningLaw").prop("placeholder", "Enter a governing law");
        $("#accountDetailModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtDetail").popover("hide").val("");
            $("#liAccountReport").select2("val", $scope.accountReports[0].id);
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }
    angular.element("#txtDetail").on("focusin", function () { angular.element("#txtDetail").popover("hide"); });

    $scope.fnLoadContactDetails = function (contactName) {
        $http.get("/FundAccounts/GetAllOnBoardingAccountContacts?hmFundId=" + $scope.FundId).then(function (response) {
            $scope.contactNames = [];
            $scope.OnBoardingContactsDetails = [];
            if (response.data.OnBoardingContacts.length > 0) {
                $.each(response.data.OnBoardingContacts, function (i, v) {
                    $scope.contactNames.push({ id: v.id, text: v.name });
                    $scope.OnBoardingContactsDetails.push(v);
                });
            }

            $("#liContacts0").select2({
                placeholder: "Select Contacts",
                multiple: true,
                //templateResult: groupNameFormat,
                //templateSelection: groupNameFormat,
                data: $scope.contactNames
            });
            if (contactName != null && contactName != undefined && contactName != "") {
                var names = contactName.split(",");
                $("#liContacts0").select2("val", names);
                $scope.onboardingContacts = $filter("filter")(($scope.OnBoardingContactsDetails), function (c) {
                    return $.inArray(c.id.toString(), names) > -1;
                });
                viewContactTable($scope.onboardingContacts);
            }
        });
        //}
    }

    $scope.fnGetAccountCallbackData = function (accountId) {
        $scope.IsCallBackChanged = true;
        $http.get("/FundAccounts/GetAccountCallbackData?accountId=" + accountId).then(function (response) {
            $scope.onBoardingAccountDetails[0].hmsAccountCallbacks = response.data;
            $scope.CallBackChecks = response.data;
            $scope.viewCallbackTable($scope.CallBackChecks);
            $timeout(function () { $scope.IsCallBackChanged = false; }, 1000);
        });
    }

    $scope.fnGetClearingBrokerData = function (accountId) {
        $http.get("/FundAccounts/GetAccountClearingBrokers?accountId=" + accountId).then(function (response) {
            $scope.ConstructClearingBrokerTable(response.data);
            $scope.TotalClearingBrokers = response.data.length;
            $("#liExposureAgreementType").select2({
                placeholder: "Select an Agreement Type",
                allowClear: true,
                data: $scope.agreementTypes
            });
        });
    }


    $scope.fnLoadDefaultDropDowns = function () {

        $("#liBeneficiaryType0").select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.beneficiaryType
        });

        $("#liIntermediaryType0").select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.beneficiaryType
        });

        $("#liUltimateBeneficiaryType0").select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.ultimateBeneficiaryType
        });

        $("#liUltimateBeneficiaryType0").on("change", function (e) {
            $("#liUltimateBeneficiaryBICorABA0").select2("val", "").trigger("change");
            $("#ultimateBankName0").val("");
            $("#ultimateBankAddress0").val("");
        });

        $("#liBeneficiaryType0").on("change", function (e) {
            $("#liBeneficiaryBICorABA0").select2("val", "").trigger("change");
            $("#beneficiaryBankName0").val("");
            $("#beneficiaryBankAddress0").val("");
        });

        $("#liIntermediaryType0").on("change", function (e) {
            $("#liIntermediaryBICorABA0").select2("val", "").trigger("change");
            $("#intermediaryBankName0").val("");
            $("#intermediaryBankAddress0").val("");
        });





        $("#AuthorizedParty0").select2({
            placeholder: "Select Authorized Party",
            allowClear: true,
            data: $scope.authorizedPartyData
        });
        $("#cashSweep0").select2({
            placeholder: "Select Cash Sweep",
            allowClear: true,
            data: $scope.cashSweepData
        });
        $("#cashSweepTimeZone0").select2({
            placeholder: "Zone",
            allowClear: true,
            data: $scope.cashSweepTimeZoneData
        });
        $("#contactType0").select2({
            placeholder: "Contact Type",
            allowClear: true,
            data: $scope.ContactType
        });
        $("#liCurrency0").select2({
            placeholder: "Select a Currency",
            allowClear: true,
            data: $scope.currencies
        });
        $("#liSweepCurrency0").select2({
            placeholder: "Select a Sweep Currency",
            allowClear: true,
            data: $scope.currencies
        });
        $("#liCashInstruction0").select2({
            placeholder: "select a Cash Instruction",
            allowClear: true,
            data: $scope.cashInstructions
        });

        $("#liAccountPurpose0").select2({
            placeholder: "Select a Account Type",
            allowClear: true,
            data: $scope.accountPurpose
        });
        $("#liAccountStatus0").select2({
            placeholder: "Select a Account Status",
            allowClear: true,
            data: $scope.accountStatus
        });
    }



    function viewSsiTemplateTable(data) {

        if ($("#ssiTemplateTable0").hasClass("initialized")) {
            fnDestroyDataTable("#ssiTemplateTable0");
        }

        if (data.length > 0)
            $("#btnAccountMapStatusButtons").show();
        else
            $("#btnAccountMapStatusButtons").hide();

        tblSsiTemplateAssociations = $("#ssiTemplateTable0").not(".initialized").addClass("initialized").DataTable({
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
                            return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Broker</label>";
                        if (tdata === "Fee/Expense Payment")
                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">Fee/Expense Payment</label>";
                        if (tdata === "Bank Loan/Private/IPO")
                            return "<label class=\"label ng-show-only label-success\" style=\"font-size: 12px;\">Bank Loan/Private/IPO</label>";
                        return "";
                    }
                },
                {
                    "sTitle": "Account Number",
                    "mData": "AccountNumber",
                },
                {
                    "sTitle": "FFC Number",
                    "mData": "FFCNumber",
                },
                {
                    "sTitle": "FFC Name",
                    "mData": "FFCName",
                },
                {
                    "sTitle": "Created By",
                    "mData": "CreatedBy",
                    "mRender": function (data) {
                        return humanizeEmail(data);
                    }
                },
                {
                    "sTitle": "Updated By",
                    "mData": "UpdatedBy",
                    "mRender": function (data) {
                        return humanizeEmail(data);
                    }
                },
                {
                    "mData": "Status",
                    "sTitle": "Status",
                    "mRender": function (data) {
                        if (data === "Pending Approval")
                            return "<label class=\"label ng-show-only label-warning\" style=\"font-size: 12px;\">Pending Approval</label>";
                        if (data === "Approved")
                            return "<label class=\"label ng-show-only label-success\" style=\"font-size: 12px;\">Approved</label>";
                        return "";
                    }
                },
                {
                    "mData": "onBoardingSSITemplateId", "sTitle": "Go to SSI Template", "className": "dt-center",
                    "mRender": function (data, type, row) {
                        return "<a class=\"btn btn-primary btn-xs\" id=\"" + data + "\" ><i class=\"glyphicon glyphicon-share-alt\"></i></a>";
                    }

                },
                {
                    "mData": "onBoardingAccountSSITemplateMapId", "className": "dt-center",
                    "sTitle": "Remove", "mRender": function (tdata) {
                        return "<a class='btn btn-danger btn-xs' title='Remove SSI'><i class='glyphicon glyphicon-trash'></i></a>";
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
            "bPaginate": false,
            //"scroller": false,
            "scrollX": data.length > 0,
            "scrollY": "350px",
            //sortable: false,
            //"sDom": "ift",
            //pagination: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "order": [[0, "desc"]],
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No ssi templates are available for the account",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ SSI Templates"
            }
        });

        $timeout(function () {
            tblSsiTemplateAssociations.columns.adjust().draw(true);
        }, 1000);


        $("#accountDetailCP tbody tr td:last-child a").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowElement = tblSsiTemplateAssociations.row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this ssi template from account?", function (result) {
                if (!result) {
                    return;
                } else {
                    if (rowElement.onBoardingAccountSSITemplateMapId > 0) {
                        $http.post("/SSITemplate/RemoveSsiTemplateMap", { ssiTemplateMapId: rowElement.onBoardingAccountSSITemplateMapId }).then(function () {
                            tblSsiTemplateAssociations.row(selectedRow).remove().draw();
                            //$scope.ssiTemplateDocuments.pop(rowElement);
                            $scope.onBoardingAccountDetails[0].onBoardingAccountSSITemplateMaps.pop(rowElement);
                            notifySuccess("ssi template has removed successfully");
                            $scope.fnSsiTemplateMap($scope.onBoardingAccountDetails[0].onBoardingAccountId, $scope.FundId, $scope.onBoardingAccountDetails[0].Currency);
                        });
                    } else {
                        tblSsiTemplateAssociations.row(selectedRow).remove().draw();
                        $scope.onBoardingAccountDetails[0].onBoardingAccountSSITemplateMaps.pop(rowElement);
                        notifySuccess("ssi template has removed successfully");
                    }
                }
            });

        });
        $("#accountDetailCP tbody tr td a.btn-primary").on("click", function (event) {
            event.preventDefault();
            var ssitemplateId = $(this).attr("id");
            window.open("/SSITemplate/SSITemplate?ssiTemplateId=" + ssitemplateId, "_blank");
        });

        $("#accountDetailCP table.ssiTemplate tbody tr").on("click", function (event) {
            event.preventDefault();

            $("#accountDetailCP table.ssiTemplate tbody tr").removeClass("info");
            if (!$(this).hasClass("info")) {
                $(this).addClass("info");
            }

            var selectedRow = $(this);
            var rowElement = tblSsiTemplateAssociations.row(selectedRow).data();

            $scope.onBoardingAccountSSITemplateMapId = rowElement.onBoardingAccountSSITemplateMapId;

            if (rowElement.Status == pendingStatus && rowElement.onBoardingAccountSSITemplateMapId > 0 && rowElement.UpdatedBy != $("#userName").val()) {
                $("#btnAccountMapStatusButtons a[title='Approve']").removeClass("disabled");
            }
            //if (rowElement.onBoardingAccountStatus == createdStatus) {
            //    $("#btnAccountStatusButtons button[id='requestForApproval']").removeClass("disabled");
            //}
            //if (rowElement.onBoardingAccountStatus != createdStatus) {
            //    $("#btnAccountStatusButtons button[id='revert']").removeClass("disabled");
            //}

        });
    }

    $scope.fnSsiTemplateMap = function (accountId, fundId, currency) {
        var messages = $scope.onBoardingAccountDetails[0].SwiftGroup != undefined ? $scope.onBoardingAccountDetails[0].SwiftGroup.AcceptedMessages : "";
        $http.get("/FundAccounts/GetAccountSsiTemplateMap?accountId=" + accountId + "&fundId=" + fundId + "&currency=" + currency + "&messages=" + messages).then(function (response) {
            $scope.ssiTemplates = response.data.ssiTemplates;
            $scope.ssiTemplateMaps = response.data.ssiTemplateMaps;
            if ($scope.ssiTemplateMaps != null && $scope.ssiTemplateMaps != undefined && $scope.ssiTemplateMaps.length > 0) {
                viewSsiTemplateTable($scope.ssiTemplateMaps);
            }
        });
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
        $http.post("/FundAccounts/UpdateAccountMapStatus", { status: $scope.AccountMapStatus, accountMapId: $scope.onBoardingAccountSSITemplateMapId, comments: $("#statusMapComments").val().trim() }).then(function () {
            notifySuccess("Account ssi template map  " + $scope.AccountMapStatus.toLowerCase() + " successfully");

            $("#btnAccountMapStatusButtons a[title='Approve']").addClass("disabled");

            var rowElement = tblSsiTemplateAssociations.row(".info").data();
            rowElement.Status = $scope.AccountMapStatus;
            rowElement.UpdatedBy = $("#userName").val();
            rowElement.Comments = $("#statusMapComments").val().trim();
            rowElement.UpdatedAt = moment();
            var selectedRowNode = tblSsiTemplateAssociations.row(".info").data(rowElement).draw().node();

            $(selectedRowNode).addClass("success").removeClass("warning");

        });
        $("#UpdateAccountMapStatusModal").modal("hide");

    }

    function attachment() {

        $("#uploadFiles0").dropzone({
            url: "/FundAccounts/UploadAccountFiles?accountId=" + $scope.onBoardingAccountId,
            dictDefaultMessage: "<span><span style=\"color: red\"> * </span>Drag/Drop account documents here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
            autoDiscover: false,
            acceptedFiles: ".msg,.csv,.txt,.pdf,.xls,.xlsx,.zip,.rar", accept: validateDoubleExtensionInDZ,
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
                //this.on("processing", function (file) {
                //    this.options.url = "/FundAccounts/UploadAccountFiles";
                //});
            },
            processing: function (file, result) {
                $("#uploadFiles0").animate({ "min-height": "140px" });
            },
            success: function (file, result) {
                $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
                $(".dzFileProgress").html("Upload Successful");
                $("#uploadFiles0").animate({ "min-height": "80px" });
                var aDocument = result;
                $.each(aDocument.Documents, function (index, value) {
                    $scope.accountDocuments.push(value);
                });
                $("#spnAgrCurrentStatus").html("Saved as Draft");
                $("#hmStatus").show();
                viewAttachmentTable($scope.accountDocuments);
                $scope.fnGetAccounts();
            },
            queuecomplete: function () {
            },
            complete: function (file, result) {
                $("#uploadFiles0").removeClass("dz-drag-hover");

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
    }

    $scope.fnAccountDocuments = function (accountId) {
        $http.get("/FundAccounts/GetAccountDocuments?accountId=" + accountId).then(function (response) {
            $scope.accountDocuments = response.data.accountDocuments;
            if ($scope.accountDocuments != null && $scope.accountDocuments != undefined && $scope.accountDocuments.length > 0) {
                viewAttachmentTable($scope.accountDocuments);
            }

            if ($scope.accountDocuments.length > 0 && $scope.onBoardingAccountDetails[0].onBoardingAccountStatus == approvedStatus) {
                $(".dz-hidden-input").prop("disabled", true);
            } else {
                $(".dz-hidden-input").prop("disabled", false);
            }
        });
    }

    $scope.fnIntialize = function () {

        $scope.fnGetAccountReports();
        var fndAccount = $scope.onBoardingAccountDetails[0];

        $scope.fnGetAccountDescriptions();
        $scope.fnGetAccountModules();
        if (fndAccount.onBoardingAccountId > 0) {
            $scope.fnLoadContactDetails(fndAccount.ContactName);
            $scope.fnGetAccountCallbackData(fndAccount.onBoardingAccountId);
            $scope.fnGetClearingBrokerData(fndAccount.onBoardingAccountId);
        }

        $scope.fnLoadDefaultDropDowns();
        $scope.fnGetAuthorizedParty();
        $scope.fnGetSwiftGroup();
        var cashSweepTimeDiv = ".cashSweepTimeDiv0";
        var cashSweepTime = "#cashSweepTime0";
        if (fndAccount.CashSweep == "Yes") {
            $(cashSweepTimeDiv).show();
            $("#cashSweepTimeZone0").val(fndAccount.CashSweepTimeZone);
        }
        else $(cashSweepTimeDiv).hide();

        $("#liAccountDescriptions0").val(fndAccount.Description);


        $scope.fnToggleBeneficiaryBICorABA(fndAccount.BeneficiaryType, "Beneficiary");
        $scope.fnToggleBeneficiaryBICorABA(fndAccount.IntermediaryType, "Intermediary");
        $scope.fnToggleBeneficiaryBICorABA(fndAccount.UltimateBeneficiaryType, "UltimateBeneficiary");

        if (fndAccount.onBoardingAccountStatus == createdStatus) {
            $("#spnAgrCurrentStatus").html("Saved as Draft");
            $("#hmStatus").show();
        }
        else if (fndAccount.onBoardingAccountStatus == pendingStatus && fndAccount.UpdatedBy != $("#userName").val()) {
            $("#spnAgrCurrentStatus").html(fndAccount.onBoardingAccountStatus);
            $("#hmStatus").show();
            $("#spnAgrCurrentStatus").removeClass("text-default").removeClass("text-success").addClass("text-warning");
        }
        else if (fndAccount.onBoardingAccountStatus == approvedStatus) {
            $("#spnAgrCurrentStatus").html(fndAccount.onBoardingAccountStatus);
            $("#hmStatus").show();
            $("#spnAgrCurrentStatus").parent().removeClass("text-default").removeClass("text-warning").addClass("text-success");
        } else {
            $("#spnAgrCurrentStatus").html(fndAccount.onBoardingAccountStatus);
        }
        $scope.fnSsiTemplateMap(fndAccount.onBoardingAccountId, $scope.FundId, fndAccount.Currency);
        attachment();
        $scope.fnAccountDocuments(fndAccount.onBoardingAccountId);
        $scope.validateAccountNumber(true);


        $("#accountDetailCP .panel-default .panel-heading").on("click", function (e) {
            $(this).parent().find("div.collapse").collapse("toggle");
            toggleChevron(e);
            $scope.fnAddjustTableColumns();
        });

        $("#navListSideBarHelp li a").on("click", function (e) {
            //$(this).parent().find("div.collapse").collapse("toggle");
            //toggleChevron(e);
            $scope.fnAddjustTableColumns();
        });

    }

    $scope.fnAddjustTableColumns = function () {
        if (tblSsiTemplateAssociations != undefined && tblSsiTemplateAssociations.columns != undefined) {
            window.setTimeout(function () {
                tblSsiTemplateAssociations.columns.adjust().draw(true);
            }, 100);
        }
        if (tblAccountDocuments != undefined && tblAccountDocuments.columns != undefined) {
            window.setTimeout(function () {
                tblAccountDocuments.columns.adjust().draw(true);
            }, 100);
        }
        if (tblContacts != undefined && tblContacts.columns != undefined) {
            $timeout(function () {
                tblContacts.columns.adjust().draw(true);
            }, 100);
        }
        if (tblAccountCallBacks != undefined && tblAccountCallBacks.columns != undefined) {
            $timeout(function () {
                tblAccountCallBacks.columns.adjust().draw(true);
            }, 100);
        }
        if (tblAccClearingBrokers != undefined && tblAccClearingBrokers.columns != undefined) {
            $timeout(function () {
                tblAccClearingBrokers.columns.adjust().draw(true);
                $("#liExposureAgreementType").select2("val", $scope.onBoardingAccountDetails[0].MarginExposureTypeID).trigger("change");
            }, 100);
        }
    }

    $scope.CheckFundAccountFieldsChanges = function (val, oldVal) {
        if (val[0].SwiftGroupId != oldVal[0].SwiftGroupId || val[0].Currency != oldVal[0].Currency || val[0].FFCName != oldVal[0].FFCName || val[0].FFCNumber != oldVal[0].FFCNumber || val[0].Reference != oldVal[0].Reference || val[0].MarginAccountNumber != oldVal[0].MarginAccountNumber || val[0].TopLevelManagerAccountNumber != oldVal[0].TopLevelManagerAccountNumber || val[0].IntermediaryAccountNumber != oldVal[0].IntermediaryAccountNumber || val[0].BeneficiaryAccountNumber != oldVal[0].BeneficiaryAccountNumber || val[0].UltimateBeneficiaryAccountNumber != oldVal[0].UltimateBeneficiaryAccountNumber || val[0].IntermediaryType != oldVal[0].IntermediaryType || val[0].BeneficiaryType != oldVal[0].BeneficiaryType || val[0].UltimateBeneficiaryType != oldVal[0].UltimateBeneficiaryType || val[0].Intermediary.BICorABA != oldVal[0].Intermediary.BICorABA || val[0].Beneficiary.BICorABA != oldVal[0].Beneficiary.BICorABA || val[0].UltimateBeneficiary.BICorABA != oldVal[0].UltimateBeneficiary.BICorABA || val[0].Intermediary.BankName != oldVal[0].Intermediary.BankName || val[0].Beneficiary.BankName != oldVal[0].Beneficiary.BankName || val[0].UltimateBeneficiary.BankName != oldVal[0].UltimateBeneficiary.BankName || val[0].UltimateBeneficiaryAccountName != oldVal[0].UltimateBeneficiaryAccountName || val[0].Intermediary.BankAddress != oldVal[0].Intermediary.BankAddress
            || val[0].Beneficiary.BankAddress != oldVal[0].Beneficiary.BankAddress || val[0].UltimateBeneficiary.BankAddress != oldVal[0].UltimateBeneficiary.BankAddress) {

            $scope.IsKeyFieldsChanged = true;
        }
    }

    $scope.$watch("watchAccountDetails", function (val, oldVal) {

        if (val == undefined || val.length == 0 || oldVal == undefined || oldVal.length == 0 || $scope.isLoad || $scope.IsTreasuryMarginCheckUpdated || $scope.IsContactsUpdated || $scope.IsContactTypeChanged) {
            $scope.isAccountChanged = false;
            $scope.isApproved = false;
            if ($("#spnAgrCurrentStatus").html() == "Saved as Draft" || !$scope.isEdit) {
                $scope.isDrafted = true;
            }
            else if (($("#spnAgrCurrentStatus").html() == approvedStatus || $("#spnAgrCurrentStatus").html() == pendingStatus) && $scope.isEdit) {
                $scope.isDrafted = false;
                $scope.isApproved = true;
            }
            return;
        }

        if ($scope.IsCallBackChanged) {
            return;
        }

        if (val[0].onBoardingAccountId != 0)
            $scope.CheckFundAccountFieldsChanges(val, oldVal);

        if (val[0].onBoardingAccountId == oldVal[0].onBoardingAccountId) {
            $scope.isApproved = false;
            $scope.isAccountChanged = true;
            if ($("#spnAgrCurrentStatus").html() == "Saved as Draft" || !$scope.isEdit) {
                $scope.isDrafted = true;
            }
            else if (($("#spnAgrCurrentStatus").html() == approvedStatus || $("#spnAgrCurrentStatus").html() == pendingStatus) && $scope.isEdit) {
                $scope.isDrafted = false;
                $scope.isApproved = true;
            }
        }

    }, true);

    $("#liSsiTemplate").change(function () {

        if ($(this).val() > 0) {

            var ssiTemplateId = $(this).val();
            var ssiTemplate = $.grep($scope.ssiTemplates, function (v) { return v.onBoardingSSITemplateId == ssiTemplateId; })[0];
            $("#FFCName").val(ssiTemplate.FFCName);
            $("#FFCNumber").val(ssiTemplate.FFCNumber);
            $("#Reference").val(ssiTemplate.Reference);
            $("#accountNumber").val(ssiTemplate.UltimateBeneficiaryAccountNumber);
            $("#templateType").val(ssiTemplate.SSITemplateType);

        } else {

            $("#FFCName").val("");
            $("#FFCNumber").val("");
            $("#Reference").val("");
            $("#accountNumber").val("");
            $("#templateType").val("");
        }
    });

    $scope.IsTreasuryMarginCheckUpdated = false;
    $scope.fnUpdateTreasuryMarginCheck = function () {
        if ($scope.isEdit) {
            $http({
                method: "POST",
                url: "/FundAccounts/UpdateTreasuryMarginCheck",
                type: "json",
                data: JSON.stringify({
                    accountId: $scope.onBoardingAccountDetails[0].onBoardingAccountId,
                    isExcludedFromTreasuryMarginCheck: $("#chkIsExcludedFromMarginCheck").prop("checked")
                })
            }).then(function () {
                $scope.IsTreasuryMarginCheckUpdated = true;
                $scope.onBoardingAccountDetails[0].IsExcludedFromTreasuryMarginCheck = $("#chkIsExcludedFromMarginCheck").prop("checked");
                notifySuccess("Treasury Margin Check updated successfully");
                $scope.fnGetAccounts();
            });
        }
    }

    $scope.IsContactTypeChanged = false;
    $scope.fnOnContactTypeChange = function (contactType) {
        if ($scope.isEdit)
            $scope.IsContactTypeChanged = true;
    }
   
    $scope.IsContactsUpdated = false;
    $scope.fnUpdateContacts = function (contacts) {
        if ($scope.isEdit) {
            $http({
                method: "POST",
                url: "/FundAccounts/UpdateContacts",
                type: "json",
                data: JSON.stringify({
                    accountId: $scope.onBoardingAccountDetails[0].onBoardingAccountId,
                    contactType: $("#contactType0").val(),
                    contactName: contacts
                })
            }).then(function () {
                $scope.IsContactsUpdated = true;
                notifySuccess("Contact details updated successfully");
                $scope.fnGetAccounts();
            });
        }
    }

    $scope.fnUpdateMarginExposureType = function () {
        $http({
            method: "POST",
            url: "/FundAccounts/AddOrUpdateMarginExposureType",
            type: "json",
            data: JSON.stringify({
                accountId: $scope.onBoardingAccountDetails[0].onBoardingAccountId,
                exposureTypeId: $("#liExposureAgreementType").val()
            })
        }).then(function () {
            $scope.onBoardingAccountDetails[0].MarginExposureTypeID = $("#liExposureAgreementType").val();
            notifySuccess("Margin Exposure Type updated successfully");
        });
    }
    $scope.txtNewClearingBroker = "";
    $scope.fnAddClearingBroker = function () {

        if ($("#txtNewClearingBroker").val() == "") {
            notifyWarning("Please add an Admin Broker and try again.");
            $("#txtNewClearingBroker").pulse();
            return;
        }

        $http({
            method: "POST",
            url: "/FundAccounts/AddClearingBrokers",
            type: "json",
            data: JSON.stringify({
                accountId: $scope.onBoardingAccountDetails[0].onBoardingAccountId,
                clearingBrokerName: $("#txtNewClearingBroker").val(),
            })
        }).then(function (response) {
            if (response.data != ''){
                notifyError("Clearing broker - '" + $("#txtNewClearingBroker").val() + "' already added in Fund Account-'" + response.data + "'");
                return;
            }
            notifySuccess("Admin Account added successfully");
            $scope.fnGetClearingBrokerData($scope.onBoardingAccountDetails[0].onBoardingAccountId);
        });
    }

    $scope.fnAddAccountSSITemplateMap = function () {

        $scope.onBoardingAccountSSITemplateMap = [];
        angular.forEach($("#ssiTemplateTableMap tr.info"), function (val, i) {
            var data = $scope.ssiTemplateTableMap.row($(val)).data();
            if (data != undefined) {
                $scope.onBoardingAccountSSITemplateMap.push({
                    onBoardingAccountSSITemplateMapId: 0,
                    onBoardingAccountId: parseInt($scope.onBoardingAccountId),
                    onBoardingSSITemplateId: parseInt(data.onBoardingSSITemplateId),
                    CreatedBy: $("#userName").val(),
                    UpdatedBy: $("#userName").val(),
                    Status: "Pending Approval"
                });
            }
        });
        $http({
            method: "POST",
            url: "/FundAccounts/AddAccountSsiTemplateMap",
            type: "json",
            data: JSON.stringify({
                accountSsiTemplateMap: $scope.onBoardingAccountSSITemplateMap
            })
        }).then(function () {
            notifySuccess("Ssi template mapped to account successfully");
            var thisAccount = $scope.onBoardingAccountDetails[0];
            if (thisAccount != undefined)
                $scope.fnSsiTemplateMap(thisAccount.onBoardingAccountId, $scope.FundId, thisAccount.Currency);
        });

        $("#accountSSITemplateMapModal").modal("hide");
    }
    $scope.isAssociationVisible = false;
    $scope.fnAssociationSSI = function () {

        if ($("#ssiTemplateTableMap").hasClass("initialized")) {
            fnDestroyDataTable("#ssiTemplateTableMap");
        }
        $scope.isAssociationVisible = false;
        $scope.ssiTemplateTableMap = $("#ssiTemplateTableMap").not(".initialized").addClass("initialized").DataTable({
            "bDestroy": true,
            //responsive: true,
            aaData: $scope.ssiTemplates,
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
                            return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Broker</label>";
                        if (tdata === "Fee/Expense Payment")
                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">Fee/Expense Payment</label>";
                        if (tdata === "Bank Loan/Private/IPO")
                            return "<label class=\"label ng-show-only label-success\" style=\"font-size: 12px;\">Bank Loan/Private/IPO</label>";
                        return "";
                    }
                },
                {
                    "sTitle": "Account Number",
                    "mData": "UltimateBeneficiaryAccountNumber",
                    //"mRender": function (tdata) {
                    //    if (tdata == null)
                    //        return "";
                    //}
                },
                {
                    "sTitle": "FFC Number",
                    "mData": "FFCNumber",
                },
                {
                    "sTitle": "FFC Name",
                    "mData": "FFCName",
                },

            ],
            //"createdRow": function (row, rowData) {
            //    switch (rowData.Status) {
            //        case "Approved":
            //            $(row).addClass("success");
            //            break;
            //        case "Pending Approval":
            //            $(row).addClass("warning");
            //            break;
            //    }

            //},
            "deferRender": false,
            "bScrollCollapse": true,
            "bPaginate": false,
            //"scroller": false,
            "scrollX": $scope.ssiTemplates.length > 0,
            "scrollY": "350px",
            //sortable: false,
            //"sDom": "ift",
            //pagination: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "order": [[0, "asc"]],
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No ssi templates are available for the account",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ SSI Templates"
            }
        });



        $timeout(function () {
            $scope.ssiTemplateTableMap.columns.adjust().draw(true);
        }, 1000);

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

    $(document).on("click", "#ssiTemplateTableMap tbody tr", function () {
        if ($(this).hasClass("info"))
            $(this).removeClass("info");
        else
            $(this).addClass("info");
        $timeout(function () {
            $scope.isAssociationVisible = $("#ssiTemplateTableMap tr.info").length > 0;
        }, 50);
    });

    $scope.downloadAccountSample = function () {
        window.location.href = "/FundAccounts/ExportSampleAccountlist";
    }

    Dropzone.options.myAwesomeDropzone = false;
    Dropzone.autoDiscover = false;

    $("#uploadFiles").dropzone({
        url: "/FundAccounts/UploadAccount",
        dictDefaultMessage: "<span><span style=\"color: red\"> * </span>Drag/Drop account files to add/update here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".csv,.xls,.xlsx", accept: validateDoubleExtensionInDZ,
        maxFiles: 6,
        previewTemplate: "<div class='row col-sm-2'><div class='panel panel-success panel-dz'> <div class='panel-heading'> <h3 class='panel-title' style='text-overflow: ellipsis;white-space: nowrap;overflow: hidden;'><span data-dz-name></span> - (<span data-dz-size></span>)</h3> " +
            "</div> <div class='panel-body'> <span class='dz-upload' data-dz-uploadprogress></span>" +
            "<div class='progress'><div data-dz-uploadprogress class='progress-bar progress-bar-warning progress-bar-striped active dzFileProgress' style='width: 0%'></div>" +
            "</div></div></div></div>",

        maxfilesexceeded: function (file) {
            this.removeAllFiles();
            this.addFile(file);
        },
        init: function () {

            this.on("processing", function (file) {
                this.options.url = "/FundAccounts/UploadAccount";
            });
        },
        processing: function (file, result) {
            $("#uploadFiles").animate({ "min-height": "140px" });
        },
        success: function (file, result) {
            $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
            $(".dzFileProgress").html("Upload Successful");
            //fnDestroyDataTable("#accountTable");
            $scope.fnGetAccounts();
        },
        queuecomplete: function () {
        },
        complete: function (file, result) {
            myDropZone = this;
            $("#uploadFiles").removeClass("dz-drag-hover");

            if (this.getRejectedFiles().length > 0 && this.getAcceptedFiles().length === 0 && this.getQueuedFiles().length === 0) {
                showMessage("File format is not supported to upload.", "Status");
                return;
            }

            if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
                notifySuccess("Files Uploaded successfully");
            }
        }
    });

    $("#btnUploadSource").click(function () {
        if (myDropZone != undefined) {
            myDropZone.removeAllFiles(true);
        }

    });

    $scope.fnCreateContact = function () {
        var subDomain = $("#subDomain").val();
        window.open(subDomain + "Contact/OnboardContact?onBoardingTypeId=3&entityId=" + $scope.CounterpartyFamilyId + "&contactId=0", "_blank");
    }

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

        $http.post("/FundAccounts/AddCurrency", { currency: $("#txtCurrency").val() }).then(function (response) {
            notifySuccess("Currency added successfully");
            $scope.onBoardingAccountDetails[0].Currency = $("#txtCurrency").val();
            $scope.fnGetCurrency();
            $("#txtCurrency").val("");
        });

        $("#currencyModal").modal("hide");
    }

    $scope.fnAddCurrencyModal = function () {
        $("#currencyModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtCurrency").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }
    angular.element("#txtCurrency").on("focusin", function () { angular.element("#txtCurrency").popover("hide"); });


    $scope.validateAccountNumber = function (isFfc) {

        var onBoardingAccount = $scope.onBoardingAccountDetails[0];

        if ((isFfc && onBoardingAccount.FFCNumber == null || onBoardingAccount.FFCNumber == "") || (onBoardingAccount.UltimateBeneficiaryAccountNumber == null || onBoardingAccount.UltimateBeneficiaryAccountNumber == "")) {
            onBoardingAccount.ContactNumber = angular.copy(onBoardingAccount.FFCNumber == undefined || onBoardingAccount.FFCNumber == "" ? onBoardingAccount.UltimateBeneficiaryAccountNumber : onBoardingAccount.FFCNumber);
            return;
        }

        var acc = $filter("filter")(angular.copy($scope.allAccountList), function (account) {
            return account.Account.onBoardingAccountId != onBoardingAccount.onBoardingAccountId &&
                account.Account.FFCNumber == onBoardingAccount.FFCNumber && account.Account.UltimateBeneficiaryAccountNumber == onBoardingAccount.UltimateBeneficiaryAccountNumber;
        }, true)[0];
        if (acc == undefined) {
            onBoardingAccount.ContactNumber = angular.copy(onBoardingAccount.FFCNumber == undefined || onBoardingAccount.FFCNumber == "" ? onBoardingAccount.UltimateBeneficiaryAccountNumber : onBoardingAccount.FFCNumber);
        }
        else {
            var accNo = angular.copy(isFfc ? onBoardingAccount.FFCNumber : onBoardingAccount.UltimateBeneficiaryAccountNumber);
            if (isFfc)
                onBoardingAccount.FFCNumber = "";
            else
                onBoardingAccount.UltimateBeneficiaryAccountNumber = "";
            notifyError("Please choose a different FFC Number or Account Number as an account exists with same information - " + accNo);
        }
    }

    angular.element("#txtCashInstruction").on("focusin", function () { angular.element("#txtCashInstruction").popover("hide"); });


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


        $http.post("/FundAccounts/AddAccountBiCorAba", { accountBiCorAba: $scope.accountBeneficiary }).then(function (response) {
            notifySuccess("Beneficiary BIC or ABA added successfully");
            $scope.BicorAba = $("#txtBICorABA").val().toUpperCase();
            $scope.isBicorAba = $("#btnBICorABA").prop("checked");
            $scope.fnGetBicorAba(0);
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

    $scope.fnAddBeneficiaryModal = function () {
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

        $http.post("/FundAccounts/AddAuthorizedParty", { authorizedParty: $("#txtAuthorizedParty").val() }).then(function (response) {
            notifySuccess("Authorized Party added successfully");
            $scope.onBoardingAccountDetails[0].AuthorizedParty = $("#txtAuthorizedParty").val();
            $scope.fnGetAuthorizedParty(0);
            $("#txtAuthorizedParty").val("");
        });

        $("#authorizedPartyModal").modal("hide");
    }

    $scope.fnAddAuthorizedPartyModal = function () {
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

        $http.post("/FundAccounts/AddSwiftGroup", { swiftGroup: $("#txtSwiftGroup").val(), senderBic: $("#txtSendersBIC").val().toUpperCase() }).then(function (response) {
            notifySuccess("Swift Group added successfully");
            $scope.onBoardingAccountDetails[0].SwiftGroup = $("#txtSwiftGroup").val();
            $scope.onBoardingAccountDetails[0].SendersBIC = $("#txtSendersBIC").val().toUpperCase();
            $scope.fnGetSwiftGroup(0);
            $("#txtSwiftGroup").val("");
            $("#txtSendersBIC").val("");
        });

        $("#swiftGroupModal").modal("hide");
    }

    $scope.fnAddSwiftGroupModal = function () {
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


    //Association View

    function hierarchyFormat(aId) {
        return "<div class=\"slider center-block onboardingMapping\" style=\"margin-bottom: 10px !important;\">" +
            "<img src=\"../Images/loading.gif\" alt=\"Loading...\" style='position:absolute;text-align:center;' class='onboardingLoadSpinner col-md-offset-6'/>" +
            "<table id=\"accountRowTable" + aId + "\" class=\"table table-bordered table-condensed\" cellpadding=\"5\" cellspacing=\"0\" border=\"0\" width=\"100%\"></table>" +
            "</div>";
    }

    $scope.IsCallBackChanged = false;
    $scope.fnSaveCallback = function () {
        if ($("#txtContactName").val() == undefined || $("#txtContactName").val() == "") {
            //pop-up    
            $("#txtContactName").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Contact Name cannot be empty. Please add a valid name",
                html: true,
                width: "250px"
            });

            $("#txtContactName").popover("show");
            return;
        }

        $("#txtContactName").popover("hide");
        if ($("#txtContactNumber").val() == undefined || $("#txtContactNumber").val() == "") {
            //pop-up    
            $("#txtContactNumber").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Contact Number cannot be empty. Please add a valid number",
                html: true,
                width: "250px"
            });

            $("#txtContactNumber").popover("show");
            return;
        }

        $("#txtContactNumber").popover("hide");
        var isExists = false;
        $($scope.callbackData).each(function (i, v) {
            if ($("#txtContactNumber").val() == v.ContactNumber && ("#txtContactName").val() == v.ContactName) {
                isExists = true;
                return false;
            }
        });
        if (isExists) {
            $("#txtContactName").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Contact Name & Contact Number already exists. Please enter a different combination.",
                html: true,
                width: "250px"
            });
            $("#txtContactName").popover("show");
            return;
        }

        $http({
            method: "POST",
            url: "/FundAccounts/AddOrUpdateCallback",
            type: "json",
            data: JSON.stringify({
                callback: $scope.callback
            })
        }).then(function (response) {
            notifySuccess("Account Call back added successfully");
            if ($scope.callback.hmsAccountCallbackId == undefined)
                $scope.IsKeyFieldsChanged = false;
            $scope.IsCallBackChanged = true;
            $scope.fnGetAccountCallbackData($scope.onBoardingAccountDetails[0].onBoardingAccountId);
        });

        $("#callbackModal").modal("hide");
    }

    $scope.fnAddCallbackModal = function () {
        $scope.callback = { onBoardingAccountId: $scope.onBoardingAccountDetails[0].onBoardingAccountId };
        $("#callbackModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtContactName").popover("hide");
            $("#txtContactNumber").popover("hide");
        });
    }

    $scope.viewCallbackTable = function (data) {

        if ($("#accountCallbackTbl_0").hasClass("initialized")) {
            fnDestroyDataTable("#accountCallbackTbl_0");
        }
        tblAccountCallBacks = $("#accountCallbackTbl_0").DataTable(
            {
                aaData: data,
                "bDestroy": true,
                "columns": [
                    { "mData": "onBoardingAccountId", "sTitle": "onBoardingAccountId", visible: false },
                    { "mData": "hmsAccountCallbackId", "sTitle": "hmsAccountCallbackId", visible: false },
                    {
                        "mData": "ContactName", "sTitle": "Contact Name"
                    },
                    {
                        "mData": "ContactNumber", "sTitle": "Contact Number"
                    },
                    {
                        "mData": "Title", "sTitle": "Title"
                    },

                    /*{
                        "mData": "IsCallbackConfirmed", "sTitle": "Callback Confirmation",
                        "mRender": function (tdata) {
                            if (tdata)
                                return "<label class='label label-success'>Confirmed</label>";

                            return "<button class='btn btn-primary btn-xs btnCallbackConfirm' title='Confirm'>Confirm</button>";
                        }
                    },*/
                    {
                        "mData": "RecCreatedBy", "sTitle": "Created By", "mRender": function (data) {
                            return humanizeEmail(data);
                        }
                    },
                    {
                        "mData": "RecCreatedDt",
                        "sTitle": "Created At",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                    /*{
                        "mData": "ConfirmedBy", "sTitle": "Confirmed By", "mRender": function (data, type, row) {
                            return row.IsCallbackConfirmed ? humanizeEmail(data) : "";
                        }
                    },
                    {
                        "mData": "ConfirmedAt",
                        "sTitle": "Confirmed At",
                        "type": "dotnet-date",
                        "mRender": function (tdata, type, row) {
                            if (!row.IsCallbackConfirmed)
                                return "";

                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },*/

                ],
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ Callbacks",
                    "sInfoFiltered": " - filtering from _MAX_ Callbacks"
                },
                "createdRow": function (row, data) {
                    $(row).addClass("succcess");
                    //switch (data.IsCallbackConfirmed) {
                    //    case true:
                    //        $(row).addClass("success");
                    //        break;
                    //    case false:
                    //        $(row).addClass("warning");
                    //        break;
                    //}

                },
                //"scrollX": false,
                "deferRender": true,
                // "scroller": true,
                "orderClasses": false,
                "sScrollX": "100%",
                //sDom: "ift",
                "sScrollY": 450,
                "sScrollXInner": "100%",
                "bScrollCollapse": true,
                "order": [],
                //"bPaginate": false,
                iDisplayLength: -1
            });

        //$(document).on("click", ".btnCallbackConfirm", function (event) {
        //    event.preventDefault();
        //    event.stopPropagation();
        //    event.stopImmediatePropagation();
        //    var selectedRow = $(this).parents("tr");
        //    $scope.rowElement = tblAccountCallBacks.row(selectedRow).data();
        //    $scope.tdEle = $(this).closest("td");
        //    $scope.tdEle.popover("destroy");
        //    $timeout(function () {
        //        angular.element($scope.tdEle).attr("title", "Are you sure to confirm the call back?").popover("destroy").popover({
        //            trigger: "click",
        //            title: "Are you sure to confirm the call back?",
        //            placement: "top",
        //            container: "body",
        //            content: function () {
        //                return "<div class=\"btn-group pull-right\" style='margin-bottom:7px;'>"
        //                    + "<button class=\"btn btn-sm btn-success confirmCallback\"><i class=\"glyphicon glyphicon-ok\"></i>&nbsp;Yes</button>"
        //                    + "&nbsp;&nbsp;<button class=\"btn btn-sm btn-default dismissCallback\"><i class=\"glyphicon glyphicon-remove\"></i>&nbsp;No</button>"
        //                    + "</div>";
        //            },
        //            html: true
        //        }).popover("show");
        //        $(".popover-content").html("<div class=\"btn-group pull-right\" style='margin-bottom:7px;'>"
        //            + "<button class=\"btn btn-sm btn-success confirmCallback\"><i class=\"glyphicon glyphicon-ok\"></i></button>"
        //            + "<button class=\"btn btn-sm btn-default dismissCallback\"><i class=\"glyphicon glyphicon-remove\"></i></button>"
        //            + "</div>");

        //    }, 50);
        //});

        $timeout(function () {
            tblAccountCallBacks.columns.adjust().draw(true);
        }, 1000);
    }

    $scope.ConstructClearingBrokerTable = function (data) {

        if ($("#tblAccClearingBrokers").hasClass("initialized")) {
            fnDestroyDataTable("#tblAccClearingBrokers");
        }
        tblAccClearingBrokers = $("#tblAccClearingBrokers").DataTable(
            {
                aaData: data,
                "bDestroy": true,
                "columns": [
                    { "mData": "hmsFundAccountClearingBrokerId", "sTitle": "hmsFundAccountClearingBrokerId", visible: false },
                    { "mData": "onBoardingAccountId", "sTitle": "onBoardingAccountId", visible: false },
                    {
                        "mData": "ClearingBrokerName", "sTitle": "Admin Account"
                    },
                    {
                        "mData": "RecCreatedAt",
                        "sTitle": "Created At",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                    {
                        "mData": "hmsFundAccountClearingBrokerId",
                        "sTitle": "Remove", "className": "dt-center",
                        "mRender": function () {
                            return "<button class='btn btn-danger btn-xs' title='Remove Admin Account'><i class='glyphicon glyphicon-trash'></i></button>";
                        }
                    }
                ],
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ Admin Accounts",
                    "sInfoFiltered": " - filtering from _MAX_ Admin Accounts"
                },

                "deferRender": true,
                // "scroller": true,
                "orderClasses": false,
                "sScrollX": "100%",
                //sDom: "ift",
                "sScrollY": 450,
                "sScrollXInner": "100%",
                "bScrollCollapse": true,
                "order": [],
                //"bPaginate": false,
                iDisplayLength: -1
            });

        $timeout(function () {
            tblAccClearingBrokers.columns.adjust().draw(true);
        }, 1000);


        $("#tblAccClearingBrokers tbody tr td:last-child button").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowElement = tblAccClearingBrokers.row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this Admin Account from Fund Account?", function (result) {
                if (!result) {
                    return;
                } else {
                    $timeout(function () {
                        $http.post("/FundAccounts/DeleteClearingBrokers", { clearingBrokerId: rowElement.hmsFundAccountClearingBrokerId }).then(function () {
                            //    tblAccClearingBrokers.row(selectedRow).remove().draw();
                            notifySuccess("Admin Account removed successfully");
                        });
                        $scope.fnGetClearingBrokerData(rowElement.onBoardingAccountId);
                    }, 100);
                }
            });
        });
    }



    $(document).on("click", ".confirmCallback", function () {
        angular.element($scope.tdEle).popover("destroy");
        $scope.IsCallBackChanged = true;
        $scope.rowElement.IsCallbackConfirmed = true;
        $http({
            method: "POST",
            url: "/FundAccounts/AddOrUpdateCallback",
            type: "json",
            data: JSON.stringify({
                callback: $scope.rowElement
            })
        }).then(function (response) {
            $scope.fnGetAccountCallbackData($scope.onBoardingAccountDetails[0].onBoardingAccountId);
            notifySuccess("Account callback confirmed successfully");

        });
    });

    $(document).on("click", ".dismissCallback", function () {
        angular.element($scope.tdEle).popover("destroy");
    });
    $("#clearingBrokersTab").on("click", function () {
        viewClearingBrokerAssociationTable($scope.clearingBrokersList);
    });
    $scope.CollapsedFundAccountGroupsInMapped = {};
    function viewClearingBrokerAssociationTable(data) {

        if ($("#accountClearingBrokerTable").hasClass("initialized")) {
            fnDestroyDataTable("#accountClearingBrokerTable");
        }

        accountClearingBrokerTable = $("#accountClearingBrokerTable").DataTable(
            {
                aaData: data,
                "bDestroy": true,
                "columns": [
                    { "mData": "ClearingBroker.hmsFundAccountClearingBrokerId", "sTitle": "hmsFundAccountClearingBrokerId", visible: false },                    
                    {
                        "mData": "AccountName", "sTitle": "Fund Account Name", visible: false
                    },
                    {
                        "mData": "FundName", "sTitle": "Fund Name", visible: false
                    },
                    {
                        "mData": "Currency", "sTitle": "Currency", visible: false
                    },
                    {
                        "mData": "ClearingBroker.ClearingBrokerName", "sTitle": "Clearing Broker Name",
                    },
                    {
                        "mData": "CounterpartyName", "sTitle": "Counterparty", visible: false
                    },
                    {
                        "mData": "AgreementName", "sTitle": "Agreement Name",
                    },
                    {
                        "mData": "AccountType", "sTitle": "Account Type",
                    },
                    {
                        "mData": "AccountNumber", "sTitle": "Account Number",
                    },
                    {
                        "mData": "ExposureTypeId", "sTitle": "Exposure Type", mRender: function (tdata) {
                           var selectedData=  $.grep($scope.agreementTypes, function (value) {
                                return (value.id == tdata)
                            });
                            return selectedData[0].text;
                        }
                    },
                    {
                        "mData": "FFCName", "sTitle": "FFC Name",
                    },
                    {
                        "mData": "FFCNumber", "sTitle": "FFC Number",
                    },
                    {
                        "mData": "RecCreatedBy", "sTitle": "Created By"
                    },
                    {
                        "mData": "ClearingBroker.RecCreatedAt",
                        "sTitle": "Created Date",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                   
                ],
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ Clearing Brokers",
                    "sInfoFiltered": " - filtering from _MAX_ Clearing Brokers"
                },
                
                //"scrollX": false,
                "deferRender": true,
                // "scroller": true,
                "orderClasses": false,
                "sScrollX": "100%",
                //sDom: "ift",
                "sScrollY": 450,
                "sScrollXInner": "100%",
                "bScrollCollapse": true,
                "order": [[3, "desc"]],
                //"bPaginate": false,
                iDisplayLength: -1,
                rowGroup: {
                    startRender: function (rows, group) {

                        var isCollapsed = !!$scope.CollapsedFundAccountGroupsInMapped[group];
                        rows.nodes().each(function (r) {
                            r.style.display = isCollapsed ? "none" : "";
                        });

                        $scope.CollapsedFundAccountGroupsInMapped[group] = isCollapsed;

                        return $("<tr style='cursor:pointer;'/>")
                            .append("<td colspan=\"10\"> " + "<i class=\"glyphicon " + (isCollapsed ? "glyphicon-chevron-right" : "glyphicon-chevron-down") + "\" ></i>&nbsp;&nbsp;" + group + " (" + rows.count() + ")</td>")
                            .attr("data-name", group)
                            .toggleClass("collapsed", isCollapsed);
                    },
                    dataSrc: function (row, sdata) {
                        return row.AccountName + " - " + row.FundName + " - " + row.CounterpartyName + " - " + row.Currency;
                    }

                },
                "drawCallback": function (settings) {
                },
            });

        $("#accountClearingBrokerTable tbody").off("click", "tr.group-start").on("click", "tr.group-start", function (event) {
            var name = $(this).data("name");
            $scope.CollapsedFundAccountGroupsInMapped[name] = !$scope.CollapsedFundAccountGroupsInMapped[name];
            accountClearingBrokerTable.draw(false);
            $timeout(function () { accountClearingBrokerTable.columns.adjust().draw(false); }, 100);
        });

        window.setTimeout(function () {
            accountClearingBrokerTable.columns.adjust().draw(true);
        }, 200);

    }
    $scope.fnExportClearingBrokerslist = function () {
        window.location.assign("/FundAccounts/ExportClearingBrokerslist");
    }
    function viewAssociationTable(data) {

        if ($("#accountSSITemplateTable").hasClass("initialized")) {
            fnDestroyDataTable("#accountSSITemplateTable");
        }

        accountSsiTemplateTable = $("#accountSSITemplateTable").DataTable(
            {
                aaData: data,
                "bDestroy": true,
                "columns": [
                    { "mData": "Account.onBoardingAccountId", "sTitle": "onBoardingAccountId", visible: false },
                    { "mData": "Account.dmaAgreementOnBoardingId", "sTitle": "dmaAgreementOnBoardingId", visible: false },
                    { "mData": "AgreementTypeId", "sTitle": "AgreementTypeId", visible: false },
                    { "mData": "Account.dmaCounterpartyFamilyId", "sTitle": "BrokerId", visible: false },
                    {
                        "orderable": false,
                        "data": null,
                        "defaultContent": "<i class=\"glyphicon glyphicon-menu-right\" style=\"cursor:pointer;\"></i>"
                    },
                    {
                        "mData": "Account.AccountType", "sTitle": "Entity Type",
                        "mRender": function (tData) {
                            if (tData != null && tData != "undefined") {
                                switch (tData) {
                                    case "Agreement": return "<label class='label label-success'>" + tData + "</label>";
                                    case "Agreement (Reporting Only)": return "<label class='label label-default'>" + tData + "</label>";
                                    case "DDA": return "<label class='label label-warning'>" + tData + "</label>";
                                    case "Custody": return "<label class='label label-info'>" + tData + "</label>";
                                }
                                return "<label class='label label-default'>" + tData + "</label>";
                            }
                            return "";
                        }
                    },
                    {
                        "mData": "FundName", "sTitle": "Fund Name"
                    },
                    { "mData": "AgreementName", "sTitle": "Agreement Name" },
                    { "mData": "CounterpartyFamilyName", "sTitle": "Counterparty Family" },
                    { "mData": "CounterpartyName", "sTitle": "Counterparty" },
                    //{ "mData": "AccountName", "sTitle": "Account Name" },
                    { "mData": "Account.UltimateBeneficiaryAccountNumber", "sTitle": "Account Number" },
                    {
                        "mData": "Account.onBoardingAccountStatus", "sTitle": "Account Status",
                        "mRender": function (tdata) {
                            if (tdata != null && tdata != "undefined") {
                                switch (tdata) {
                                    case "Approved": return "<label class='label label-success'>" + tdata + "</label>";
                                    case "Pending Approval": return "<label class='label label-warning'>" + tdata + "</label>";
                                    case "Created": return "<label class='label label-default'>" + "Saved As Draft" + "</label>";
                                }
                                return "<label class='label label-default'>" + tdata + "</label>";
                            }
                            return "";
                        }
                    },
                    {
                        "mData": "Account.CreatedBy", "sTitle": "Created By", "mRender": function (data) {
                            return humanizeEmail(data);
                        }
                    },
                    {
                        "mData": "Account.CreatedAt",
                        "sTitle": "Created Date",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                    {
                        "mData": "Account.UpdatedBy", "sTitle": "Last Modified By", "mRender": function (data) {
                            return humanizeEmail(data);
                        }
                    },
                    {
                        "mData": "Account.UpdatedAt",
                        "sTitle": "Last Modified",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    }
                ],
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ Onboarded Accounts",
                    "sInfoFiltered": " - filtering from _MAX_ Onboarded Accounts"
                },
                "createdRow": function (row, data) {
                    switch (data.Account.onBoardingAccountStatus) {
                        case "Approved":
                            $(row).addClass("success");
                            break;
                        case "Pending Approval":
                            $(row).addClass("warning");
                            break;
                    }

                },
                //"scrollX": false,
                "deferRender": true,
                // "scroller": true,
                "orderClasses": false,
                "sScrollX": "100%",
                //sDom: "ift",
                "sScrollY": 450,
                "sScrollXInner": "100%",
                "bScrollCollapse": true,
                "order": [[14, "desc"]],
                //"bPaginate": false,
                iDisplayLength: -1
            });


        window.setTimeout(function () {
            accountSsiTemplateTable.columns.adjust().draw(true);
        }, 50);

    }

    $http.get("/FundAccounts/GetAccountAssociationPreloadData").then(function (response) {
        $scope.allFunds = response.data;
        $("#liOnboardingFund").select2({
            placeholder: "select a fund",
            allowClear: true,
            data: $scope.allFunds
        });
    });

    $("#liOnboardingFund").change(function () {
        var fundId = $(this).val();
        var accountList = [];
        if (fundId > 0) {
            accountList = $.grep($scope.allAccountList, function (v) { return v.Account.hmFundId == fundId; });
        }
        viewAssociationTable(accountList);
    });

    $(document).on("click", "#accountSSITemplateTable tbody tr td:first-child ", function () {

        var tr = $(this).parent();
        var row = accountSsiTemplateTable.row(tr);

        var account = row.data().Account;

        if (account != undefined) {
            var onBoardingAccountId = account.onBoardingAccountId;
            var fId = account.hmFundId;
            var currency = account.Currency;

            var icon = $(this).find("i");
            if ($("#accountRowTable").hasClass("initialized")) {
                fnDestroyDataTable("#accountRowTable");
            }

            if (row.child.isShown()) {

                // This row is already open - close it           
                $("div.slider", row.child()).slideUp(200, function () {
                    row.child.hide();
                    tr.removeClass("shown");
                });
                icon.addClass("glyphicon-menu-right").removeClass("glyphicon-menu-down");
            } else {

                // Open this row  
                row.child(hierarchyFormat(onBoardingAccountId)).show();
                var rowTableId = "#accountRowTable" + onBoardingAccountId;
                if ($(rowTableId).hasClass("initialized")) {
                    fnDestroyDataTable(rowTableId);
                }
                $http.get("/FundAccounts/GetAccountSsiTemplateMap?accountId=" + onBoardingAccountId + "&fundId=" + fId + "&currency=" + currency).then(function (response) {

                    tblSsiTemplateRow = $(rowTableId).not(".initialized").addClass("initialized").DataTable(
                        {
                            aaData: response.data.ssiTemplateMaps,
                            rowId: "onBoardingAccountSSITemplateMapId",
                            "bDestroy": true,
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
                                            return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Broker</label>";
                                        if (tdata === "Fee/Expense Payment")
                                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">Fee/Expense Payment</label>";
                                        if (tdata === "Bank Loan/Private/IPO")
                                            return "<label class=\"label ng-show-only label-success\" style=\"font-size: 12px;\">Bank Loan/Private/IPO</label>";
                                        return "";
                                    }
                                },
                                {
                                    "sTitle": "Account Number",
                                    "mData": "UltimateBeneficiaryAccountNumber"
                                },
                                {
                                    "sTitle": "FFC Number",
                                    "mData": "FFCNumber",
                                },
                                {
                                    "sTitle": "FFC Name",
                                    "mData": "FFCName",
                                },
                                {
                                    "sTitle": "Created By",
                                    "mData": "CreatedBy", "mRender": function (data) {
                                        return humanizeEmail(data);
                                    }
                                },
                                {
                                    "sTitle": "Updated By",
                                    "mData": "UpdatedBy", "mRender": function (data) {
                                        return humanizeEmail(data);
                                    }
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
                                    "mData": "onBoardingSSITemplateId",
                                    "sTitle": "Go to SSI Template",
                                    "className": "dt-center",
                                    "mRender": function (tdata, type, row) {
                                        return "<a class=\"btn btn-primary btn-xs\" id=\"" + tdata + "\" ><i class=\"glyphicon glyphicon-share-alt\"></i></a>";
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
                            //"scroller": true,
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

                    $(".onboardingLoadSpinner").hide();
                });


                icon.addClass("glyphicon-menu-down").removeClass("glyphicon-menu-right");
                $("div.slider", row.child()).slideDown(200, function () {
                    tr.addClass("shown");
                });
            }
        }
    });
    $(document).on("click", "#accountSSITemplateTable tbody tr td a.btn-primary", function (event) {
        event.preventDefault();
        var ssitemplateId = $(this).attr("id");
        window.open("/SSITemplate/SSITemplate?ssiTemplateId=" + ssitemplateId, "_blank");
    });


    $scope.fnSetupSideMenu = function () {

        //*Starting of live filter functions*//
        $("#navListSideBarHelp li").each(function () {
            $(this).attr("data-search-term", $(this).text().toLowerCase());
        });
        $("#accountDetailCP label").each(function () {
            $(this).attr("data-search-term", $(this).text().toLowerCase());
        });


        $(".live-search-box").on("keyup", function (event) {
            var searchTerm = $(this).val().toLowerCase();
            $(".live-search-highlight-rules").removeClass("active");
            if (searchTerm == "" && event.keyCode == 8) {

                if (searchTerm == "") {
                    $(".live-search-highlight-rules").removeClass("active");
                }

                $("#navListSideBarHelp li ul").hide(200);
                $("#navListSideBarHelp li").show(200);
                return;
            }

            var regexOfSystemConfig = RegExp("\(" + searchTerm.replace(/[^a-zA-Z0-9()]/g, "\\$&") + "\)", "ig");
            $("#navListSideBarHelp li").each(function (index, li) {
                if ($(li).filter("[data-search-term *= " + searchTerm + "]").length > 0 || searchTerm.length < 1) {
                    $(li).show(200);
                    $(li).find("ul").show(200);
                    $($(li).find("a")).each(function (i, v) {
                        $(v).html($(v).text().replace(regexOfSystemConfig, "<font class='live-search-highlight-rules active'>$&</font>"));
                    });
                } else {
                    $(li).hide();
                    $(li).find("ul").hide();
                }
            });

            $("#accountDetailCP label").each(function (index, li) {
                if ($(li).filter("[data-search-term *= " + searchTerm.trim() + "]").length > 0 || searchTerm.length < 1) {
                    $(li).html($(li).text().replace(regexOfSystemConfig, "<font class='live-search-highlight-rules active'>$&</font>"));

                    var parentTarget = $(li).parentsUntil(".panel").parent().attr("id");
                    $("#navListSideBarHelp li a[href='#" + parentTarget + "']").parent().show();
                }
            });
        });
        //*Ending of live filter functions*//

        /* smooth scrolling sections */
        $("#navListSideBarHelp a:not([href='#'])").click(function (event) {

            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();

            var $dataTarget = $($($(this).attr("href") + " div").attr("data-target"));
            $dataTarget.collapse("show");

            if (location.pathname.replace(/^\//, "") != this.pathname.replace(/^\//, "") || location.hostname != this.hostname)
                return false;

            var target = $(this.hash);
            target = target.length ? target : $("[name=" + this.hash.slice(1) + "]");

            if (!target.length)
                return false;

            $("#accountModal").animate({
                scrollTop: target.position().top + 100
            });
        });
    }
});
