$("#liAccounts").addClass("active");
HmOpsApp.controller("AccountListController", function ($scope, $http, $timeout, $filter) {
    $("#onboardingMenu").addClass("active");
    $("#loading").show();
    var accountTable, accountSsiTemplateTable, tblSsiTemplateRow;
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
    $scope.SwiftGroups = [];
    $scope.SwiftGroupData = [];
    var accountDocumentTable = [];
    var ssiMapTable = [];
    $scope.onBoardingAccountSSITemplateMap = {};
    $scope.ssiTemplates = [];
    $scope.accountDocuments = [];
    var contactTable = [];
    var approvedStatus = "Approved";
    //var rejectedStatus = "Rejected";
    var pendingStatus = "Pending Approval";
    var createdStatus = "Created";
    $scope.isExistsDocument = "False";

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
                    "mData": "RecCreatedBy", "mRender":function(data) {
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
                    $("td:eq(0)", nRow).html("<a title ='click to download the file' href='/Accounts/DownloadAccountFile?fileName=" + getFormattedFileName(aData.FileName) + "&accountId=" + $scope.onBoardingAccountId + "'>" + aData.FileName + "</a>");
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
                            notifySuccess("Account document has removed successfully");
                        });
                    } else {
                        accountDocumentTable[rowIndex].row(selectedRow).remove().draw();
                        $scope.onBoardingAccountDetails[rowIndex].onBoardingAccountDocuments.pop(rowElement);
                        notifySuccess("Account document has removed successfully");
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

            if (panelIndex != undefined && panelIndex != null) {
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
            if (panelIndex != undefined && panelIndex != null) {
                $("#liCashInstruction" + panelIndex).select2({
                    placeholder: "Select a Cash Instruction",
                    allowClear: true,
                    data: response.data.cashInstructions
                });

                if ($scope.onBoardingAccountDetails[panelIndex].CashInstruction != null && $scope.onBoardingAccountDetails[panelIndex].CashInstruction != 'undefined')
                    $("#liCashInstruction" + panelIndex).select2("val", $scope.onBoardingAccountDetails[panelIndex].CashInstruction);
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

    $scope.fnGetBicorAba();
    $scope.fnGetCurrency();
    $scope.fnGetCashInstruction();

    $scope.$on("onRepeatLast", function (scope, element, attrs) {
        $timeout(function () {
            $scope.fnIntialize();
        }, 100);
    });

    $scope.fnGetAccounts = function () {
        $http.get("/Accounts/GetAllOnBoardingAccount").then(function (response) {

            $scope.agreementTypes = response.data.accountTypes;
            if (response.data.OnBoardingAccounts.length > 0)
                $("#btnAccountStatusButtons").show();
            $scope.allAccountList = response.data.OnBoardingAccounts;
            accountTable = $("#accountTable").DataTable(
           {
               aaData: response.data.OnBoardingAccounts,
               "bDestroy": true,
               "columns": [
                  { "mData": "onBoardingAccountId", "sTitle": "onBoardingAccountId", visible: false },
                  { "mData": "dmaAgreementOnBoardingId", "sTitle": "dmaAgreementOnBoardingId", visible: false },
                  { "mData": "AgreementTypeId", "sTitle": "AgreementTypeId", visible: false },
                  { "mData": "BrokerId", "sTitle": "BrokerId", visible: false },
                  //{ "mData": "AccountType", "sTitle": "Account Type" },
                  {
                      "mData": "AccountType", "sTitle": "Entity Type",
                      "mRender": function (tdata) {
                          if (tdata != null && tdata != "undefinied") {
                              switch (tdata) {
                                  case "Agreement": return "<label class='label label-success'>" + tdata + "</label>";
                                  case "DDA": return "<label class='label label-warning'>" + tdata + "</label>";
                                  case "Custody": return "<label class='label label-info'>" + tdata + "</label>";
                              }
                              return "<label class='label label-default'>" + tdata + "</label>";
                          }
                          return "";
                      }
                  },
                  {
                      "mData": "FundName", "sTitle": "Fund Name"
                  },
                  { "mData": "AgreementName", "sTitle": "Agreement Name" },
                  { "mData": "Broker", "sTitle": "Broker" },
                  { "mData": "AccountName", "sTitle": "Account Name" },
                  { "mData": "AccountNumber", "sTitle": "Account Number" },
                  { "mData": "AccountPurpose", "sTitle": "Account Type" },
                  { "mData": "AccountStatus", "sTitle": "Account Status" },
                  { "mData": "Currency", "sTitle": "Currency" },
                  { "mData": "Description", "sTitle": "Description" },
                  { "mData": "Notes", "sTitle": "Notes" },
                  { "mData": "AuthorizedParty", "sTitle": "Authorized Party" },
                  { "mData": "CashInstruction", "sTitle": "Cash Instruction Mechanism" },
                  { "mData": "SwiftGroup", "sTitle": "Swift Group" },
                  { "mData": "SendersBIC", "sTitle": "Senders BIC" },
                  { "mData": "CashSweep", "sTitle": "Cash Sweep" },
                  {
                      "mData": "CashSweepTime", "sTitle": "Cash Sweep Time",
                      "mRender": function (tdata) {
                          if (tdata == "" || tdata == null)
                              return "";

                          return moment(tdata).format("LT");
                      }
                  },
                  { "mData": "CashSweepTimeZone", "sTitle": "Cash Sweep Time Zone" },
                  {
                      "mData": "CutoffTime", "sTitle": "Cutoff Time",
                      "mRender": function (tdata) {
                          if (tdata == "" || tdata == null)
                              return "";
                          return moment(tdata).format("LT");
                      }
                  },
                  { "mData": "DaystoWire", "sTitle": "Days to wire per V.D" },
                  { "mData": "HoldbackAmount", "sTitle": "Holdback Amount" },
                  { "mData": "SweepComments", "sTitle": "Sweep Comments" },
                  { "mData": "AssociatedCustodyAcct", "sTitle": "Associated Custody Acct" },
                  { "mData": "PortfolioSelection", "sTitle": "Portfolio Selection" },
                  { "mData": "TickerorISIN", "sTitle": "Ticker/ISIN" },
                  { "mData": "SweepCurrency", "sTitle": "Sweep Currency" },
                  //{ "mData": "ContactType", "sTitle": "Contact Type" },
                  //{ "mData": "ContactName", "sTitle": "Contact Name" },
                  //{ "mData": "ContactEmail", "sTitle": "Contact Email" },
                  //{ "mData": "ContactNumber", "sTitle": "Contact Number" },
                  { "mData": "BeneficiaryType", "sTitle": "Beneficiary Type" },
                  { "mData": "BeneficiaryBICorABA", "sTitle": "Beneficiary BIC or ABA" },
                  { "mData": "BeneficiaryBankName", "sTitle": "Beneficiary Bank/Account Name" },
                  { "mData": "BeneficiaryBankAddress", "sTitle": "Beneficiary Bank Address" },
                  { "mData": "BeneficiaryAccountNumber", "sTitle": "Beneficiary Account Number" },
                  { "mData": "IntermediaryType", "sTitle": "Intermediary Beneficiary Type" },
                  { "mData": "IntermediaryBICorABA", "sTitle": "Intermediary BIC or ABA" },
                  { "mData": "IntermediaryBankName", "sTitle": "Intermediary Bank/Account Name" },
                  { "mData": "IntermediaryBankAddress", "sTitle": "Intermediary Bank Address" },
                  { "mData": "IntermediaryAccountNumber", "sTitle": "Intermediary Account Number" },
                  { "mData": "UltimateBeneficiaryType", "sTitle": "Ultimate Beneficiary Type" },
                  { "mData": "UltimateBeneficiaryBICorABA", "sTitle": "Ultimate Beneficiary BIC or ABA" },
                  { "mData": "UltimateBeneficiaryBankName", "sTitle": "Ultimate Beneficiary Bank Name" },
                  { "mData": "UltimateBeneficiaryBankAddress", "sTitle": "Ultimate Beneficiary Bank Address" },
                  { "mData": "UltimateBeneficiaryAccountName", "sTitle": "Ultimate Beneficiary Account Name" },
                  { "mData": "FFCName", "sTitle": "FFC Name" },
                  { "mData": "FFCNumber", "sTitle": "FFC Number" },
                  { "mData": "Reference", "sTitle": "Reference" },
                  {
                      "mData": "onBoardingAccountStatus", "sTitle": "Status",
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
                  { "mData": "StatusComments", "sTitle": "Comments" },
                  { "mData": "CreatedBy", "sTitle": "Created By", "mRender":function(data) {
                      return humanizeEmail(data);
                  } },
                 {
                     "mData": "CreatedAt",
                     "sTitle": "Created Date",
                     "type": "dotnet-date",
                     "mRender": function (tdata) {
                         return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                     }
                 },
                 {
                     "mData": "UpdatedBy", "sTitle": "Last Modified By", "mRender": function (data) {
                         return humanizeEmail(data);
                     }
                 },
                 {
                     "mData": "UpdatedAt",
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
                   switch (data.onBoardingAccountStatus) {
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
               "scroller": true,
               "orderClasses": false,
               "sScrollX": "100%",
               //sDom: "ift",
               "sScrollY": $("#accountListDiv").offset().top + 350,
               "sScrollXInner": "100%",
               "bScrollCollapse": true,
               "order": [[53, "desc"]],
               //"bPaginate": false,
               iDisplayLength: -1
           });

            var searchText = decodeURI(getUrlParameter("searchText"));

            if (searchText != "" && searchText != undefined && searchText != 'undefined') {
                $timeout(function () {
                    accountTable.search(searchText).draw(false);
                }, 50);
            } else {
                window.setTimeout(function () {
                    accountTable.columns.adjust().draw(true);
                }, 100);
            }
            $("#loading").hide();
        });
    }

    $(document).on("click", "#accountTable tbody tr ", function () {
        $("#accountTable tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $("#btnAccountStatusButtons button").addClass("disabled");
        var rowElement = accountTable.row(this).data();
        $scope.onBoardingAccountId = rowElement.onBoardingAccountId;
        $scope.FundId = rowElement.dmaFundOnBoardId;
        $scope.AgreementId = rowElement.dmaAgreementOnBoardingId;
        $scope.BrokerId = rowElement.BrokerId;
        $scope.AccountType = rowElement.AccountType;

        $scope.AgreementTypeId = rowElement.AgreementTypeId;
        $scope.AccountStatus = rowElement.onBoardingAccountStatus;

        if (rowElement.onBoardingAccountStatus == pendingStatus && rowElement.UpdatedBy != $("#userName").val()) {
            $("#btnAccountStatusButtons button[id='approve']").removeClass("disabled");
        }
        if (rowElement.onBoardingAccountStatus == createdStatus) {
            $("#btnAccountStatusButtons button[id='requestForApproval']").removeClass("disabled");
        }
        if (rowElement.onBoardingAccountStatus != createdStatus) {
            $("#btnAccountStatusButtons button[id='revert']").removeClass("disabled");
        }

        $("#btnEdit").prop("disabled", false);

        $http.get("/Accounts/IsAccountDocumentExists?accountId=" + $scope.onBoardingAccountId).then(function (response) {
            $scope.isExistsDocument = response.data;
        });

        $("#btnDel").prop("disabled", false);
    });

    $(document).on("dblclick", "#accountTable tbody tr", function () {

        var rowElement = accountTable.row(this).data();
        $scope.accountDetail = rowElement;
        $scope.onBoardingAccountId = rowElement.onBoardingAccountId;
        $scope.AgreementId = rowElement.dmaAgreementOnBoardingId;
        $scope.FundId = rowElement.dmaFundOnBoardId;
        $scope.AgreementTypeId = rowElement.AgreementTypeId;
        $scope.BrokerId = rowElement.BrokerId;
        $scope.counterpartyFamilyId = rowElement.BrokerId;
        $scope.AccountType = rowElement.AccountType;
        $scope.broker = rowElement.Broker;

        $scope.fundName = rowElement.FundName;

        $http.get("/Accounts/GetOnBoardingAccount?accountId=" + $scope.onBoardingAccountId).then(function (response) {
            var account = response.data.OnBoardingAccount;
            if (account.CashSweepTime != null && account.CashSweepTime != "" && account.CashSweepTime != undefined) {
                //var times = account.CashSweepTime.split(':');
                account.CashSweepTime = new Date(2014, 0, 1, account.CashSweepTime.Hours, account.CashSweepTime.Minutes, account.CashSweepTime.Seconds);

            }
            if (account.CutoffTime != null && account.CutoffTime != "" && account.CutoffTime != undefined) {
                //var cutoffTimes = account.CutoffTime.split(':');
                account.CutoffTime = new Date(2014, 0, 1, account.CutoffTime.Hours, account.CutoffTime.Minutes, account.CutoffTime.Seconds);
            }
            account.CreatedAt = moment(account.CreatedAt).format("YYYY-MM-DD HH:mm:ss");

            var agreementType = $.grep($scope.agreementTypes, function (v) { return v.id == $scope.AgreementTypeId; })[0];

            if (agreementType.text == "PB" || agreementType.text == "FCM" || $scope.AccountType == "DDA") {
                $scope.accountPurpose = [{ id: "Cash", text: "Cash" }, { id: "Margin", text: "Margin" }];
            } else {
                $scope.accountPurpose = [{ id: "Pledge Account", text: "Pledge Account" }, { id: "Return Account", text: "Return Account" }, { id: "Both", text: "Both" }];
            }
            $scope.onBoardingAccountDetails.push(account);
            $scope.watchAccountDetails = $scope.onBoardingAccountDetails;


        });

        $("#accountModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {

            $scope.onBoardingAccountDetails = [];
            $scope.accountDetail = {};
            $scope.fnGetAccounts();
            var searchText = $('#accountListDiv input[type="search"]').val();
            window.location.href = "/Accounts/Index?searchText=" + searchText;

        }).off("shown.bs.modal").on("shown.bs.modal", function () {

        });
    });

    $scope.fnUpdateAccountStatus = function (status, statusAction) {
        $scope.AccountStatus = status;

        if ((statusAction == "Request for Approval" || statusAction == "Approve") && $scope.isExistsDocument == "False") {
            notifyWarning("Please upload document to approve account");
            return;
        }
        var confirmationMsg = "Are you sure you want to " + ((statusAction === "Request for Approval") ? "<b>request</b> for approval of" : "<b>" + (statusAction == "Revert" ? "save changes or sending approval for" : statusAction) + "</b>") + " the selected account?";
        if (statusAction == "Request for Approval") {
            $("#btnSaveComment").addClass("btn-warning").removeClass("btn-success").removeClass("btn-primary");
            $("#btnSaveComment").html('<i class="glyphicon glyphicon-share-alt"></i>&nbsp;Request for approval');
        } else if (statusAction == "Approve") {
            $("#btnSaveComment").removeClass("btn-warning").addClass("btn-success").removeClass("btn-primary");
            $("#btnSaveComment").html('<i class="glyphicon glyphicon-ok"></i>&nbsp;Approve');
        }
        else if (statusAction == "Revert") {
            $("#btnSaveComment").removeClass("btn-warning").removeClass("btn-success").addClass("btn-primary");
            $("#btnSaveComment").html('<i class="glyphicon glyphicon-floppy-save"></i>&nbsp;Save Changes');
            $("#btnSendApproval").show();
        }

        $("#pMsg").html(confirmationMsg);
        $("#UpdateAccountStatusModal").modal("show");
    }

    $scope.fnSaveAccountStatus = function () {
        $http.post("/Accounts/UpdateAccountStatus", { accountStatus: $scope.AccountStatus, accountId: $scope.onBoardingAccountId, comments: $("#statusComments").val().trim() }).then(function () {
            notifySuccess("Account  " + $scope.AccountStatus.toLowerCase() + " successfully");
            window.location.href = "/Accounts/Index";
        });
        $("#btnSendApproval").hide();
        $("#UpdateAccountStatusModal").modal("hide");
    }
    $scope.fnSendApprovalAccountStatus = function () {
        $scope.AccountStatus = pendingStatus;
        $scope.fnSaveAccountStatus();
    }
    $scope.fnEditAccount = function () {
        var searchText = $('#accountListDiv input[type="search"]').val();
        var accountListUrl = "/Accounts/Index?searchText=" + searchText;
        window.history.pushState("", "", accountListUrl);
        window.location.assign("/Accounts/Account?fundId=" + $scope.FundId + "&brokerId=" + $scope.BrokerId + "&agreementId=" + $scope.AgreementId + "&accountType=" + $scope.AccountType + "&searchText=" + searchText);
    }

    $scope.fnCreateAccount = function () {
        window.location.assign("/Accounts/Account?fundId=0&brokerId=0&agreementId=0&accountType=");
    }

    $scope.fnDeleteAccount = function () {
        showMessage("Are you sure do you want to delete account? ", "Delete Account", [
                 {
                     label: "Delete",
                     className: "btn btn-sm btn-danger",
                     callback: function () {
                         $http.post("/Accounts/DeleteAccount", { onBoardingAccountId: $scope.onBoardingAccountId }).then(function () {
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
        window.location.assign("/Accounts/ExportAllAccountlist");
    }

    //Save Account
    $scope.fnSaveAccount = function (isValid, status) {

        if (!isValid)
            return;

        var isAccountNameEmpty = false;

        $.each($scope.onBoardingAccountDetails, function (key, value) {

            var liAccountDescriptionsValue = "#liAccountDescriptions" + key;
            value.Description = $(liAccountDescriptionsValue).val();

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

            value.BeneficiaryBankName = $("#beneficiaryBankName" + key).val();
            value.BeneficiaryBankAddress = $("#beneficiaryBankAddress" + key).val();

            value.IntermediaryBankName = $("#intermediaryBankName" + key).val();
            value.IntermediaryBankAddress = $("#intermediaryBankAddress" + key).val();

            value.UltimateBeneficiaryBankName = $("#ultimateBankName" + key).val();
            value.UltimateBeneficiaryBankAddress = $("#ultimateBankAddress" + key).val();
            if (status != "" && status != undefined)
                value.onBoardingAccountStatus = status;
            if (value.UltimateBeneficiaryType == "Account Name" &&
                (value.UltimateBeneficiaryAccountName == null || value.UltimateBeneficiaryAccountName == ""))
                isAccountNameEmpty = true;
        });

        if (isAccountNameEmpty) {
            notifyWarning("Account name field is required");
            return;
        }

        $http.post("/Accounts/AddAccounts", { onBoardingAccounts: $scope.onBoardingAccountDetails }).then(function () {
            notifySuccess("Account Saved successfully");
            $("#accountModal").modal("hide");
        });
    }

    $scope.fnGetAccounts();

    $scope.fnCashSweep = function (cashSweep, index) {
        //var cashSweepTimeZone = "#cashSweepTimeZone" + index;
        if (cashSweep == "Yes") {
            $(".cashSweepTimeDiv" + index).show();
        }
        else $(".cashSweepTimeDiv" + index).hide();
    }

    $scope.fnGetAuthorizedParty = function (panelIndex) {
        $http.get("/Accounts/GetAllAuthorizedParty").then(function (response) {
            $scope.authorizedPartyData = response.data.AuthorizedParties;

            if (panelIndex != undefined) {
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

            if (panelIndex != undefined) {
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

    $scope.fnCutOffTime = function (currency, cashInstruction, index) {

        $http.get("/Accounts/GetCutoffTime?cashInstruction=" + cashInstruction + "&currency=" + currency).then(function (response) {
            var cutOff = response.data.cutOffTime;
            if (cutOff != undefined && cutOff != "") {
                //var cutoffTimes = cutOff.CutoffTime;
                $scope.onBoardingAccountDetails[index].CutoffTime = new Date(2014, 0, 1, cutOff.CutoffTime.Hours, cutOff.CutoffTime.Minutes, cutOff.CutoffTime.Seconds);
                //$("#cutOffTime" + index).val($scope.onBoardingAccountDetails[index].CutoffTime);
                $scope.onBoardingAccountDetails[index].DaystoWire = cutOff.DaystoWire;
            }
            else {
                $("#cutOffTime" + index).val("");
                $("#wireDays" + index).val("");
            }

        });
    }

    $scope.fnGetBankDetails = function (biCorAbaValue, id, index) {

        var accountBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.BICorABA == biCorAbaValue; })[0];
        switch (id) {
            case "Beneficiary":
                if (accountBicorAba != undefined) {
                    $("#beneficiaryBankName" + index).val(accountBicorAba.BankName);
                    $("#beneficiaryBankAddress" + index).val(accountBicorAba.BankAddress);

                }
                else {
                    $("#beneficiaryBankName" + index).val("");
                    $("#beneficiaryBankAddress" + index).val("");
                }
                break;
            case "Intermediary":
                if (accountBicorAba != undefined) {
                    $("#intermediaryBankName" + index).val(accountBicorAba.BankName);
                    $("#intermediaryBankAddress" + index).val(accountBicorAba.BankAddress);

                }
                else {
                    $("#intermediaryBankName" + index).val("");
                    $("#intermediaryBankAddress" + index).val("");
                }
                break;
            case "UltimateBeneficiary":
                if (accountBicorAba != undefined) {
                    $("#ultimateBankName" + index).val(accountBicorAba.BankName);
                    $("#ultimateBankAddress" + index).val(accountBicorAba.BankAddress);

                }
                else {
                    $("#ultimateBankName" + index).val("");
                    $("#ultimateBankAddress" + index).val("");
                }
                break;
        }

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
                if (item == "Account Name") {
                    $("#divUltimateBeneficiaryBICorABA" + index).hide();
                    $("#ultimateBankName" + index).hide();
                    $("#ultimateBankAddress" + index).hide();
                    $("#accountName" + index).show();
                    return;
                } else {
                    $("#divUltimateBeneficiaryBICorABA" + index).show();
                    $("#ultimateBankName" + index).show();
                    $("#ultimateBankAddress" + index).show();
                    $("#accountName" + index).hide();
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

    function toggleChevron(e) {
        $(e.target)
            .find("i.indicator")
            .toggleClass("glyphicon-chevron-down").toggleClass("glyphicon-chevron-up");
        // $("html, body").animate({ scrollTop: $(e.target).offset().top - 10 }, "slow");
    }

    $scope.fnGetAccountDescriptions = function (panelIndex) {

        $http.get("/Accounts/GetAccountDescriptionsByAgreementTypeId?agreementTypeId=" + $scope.AgreementTypeId).then(function (response) {
            $scope.AccountDescriptions = response.data.accountDescriptions;
            $("#liAccountDescriptions" + panelIndex).select2({
                placeholder: "Select Description",
                allowClear: true,
                data: response.data.accountDescriptions
            });

            if ($scope.AgreementTypeId > 0)
                $("#liAccountDescriptions" + panelIndex).val($scope.onBoardingAccountDetails[panelIndex].Description);
        });
    }

    $scope.fnLoadContactDetails = function (brokerId, contactName, index) {
        $http.get("/Accounts/GetAllOnBoardingAccountContacts?entityId=" + brokerId).then(function (response) {
            $scope.contactNames = [];
            if (response.data.OnBoardingContacts.length > 0) {
                $.each(response.data.OnBoardingContacts, function (i, v) {
                    $scope.contactNames.push({ id: v.id, text: v.name });
                    $scope.OnBoardingContactsDetails.push(v);
                });
            }

            $("#liContacts" + index).select2({
                placeholder: "Select Contacts",
                multiple: true,
                //templateResult: groupNameFormat,
                //templateSelection: groupNameFormat,
                data: $scope.contactNames
            });
            if (contactName != null && contactName != undefined && contactName != "") {
                $("#liContacts" + index).select2("val", contactName);
                var onboardingContacts = $filter('filter')(($scope.OnBoardingContactsDetails), function (c) {
                    return (contactName.indexOf(c.id) > -1);
                });
                viewContactTable(onboardingContacts, index);
            }
        });
        //}
    }

    $scope.fnLoadDefaultDropDowns = function (key) {

        $("#liBeneficiaryType" + key).select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.beneficiaryType
        });

        $("#liIntermediaryType" + key).select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.beneficiaryType
        });

        $("#liUltimateBeneficiaryType" + key).select2({
            placeholder: "Select a BIC or ABA",
            allowClear: true,
            data: $scope.ultimateBeneficiaryType
        });

        $("#AuthorizedParty" + key).select2({
            placeholder: "Select Authorized Party",
            allowClear: true,
            data: $scope.authorizedPartyData
        });
        $("#cashSweep" + key).select2({
            placeholder: "Select Cash Sweep",
            allowClear: true,
            data: $scope.cashSweepData
        });
        $("#cashSweepTimeZone" + key).select2({
            placeholder: "Zone",
            allowClear: true,
            data: $scope.cashSweepTimeZoneData
        });
        $("#contactType" + key).select2({
            placeholder: "Contact Type",
            allowClear: true,
            data: $scope.ContactType
        });
        $("#liCurrency" + key).select2({
            placeholder: "Select a Currency",
            allowClear: true,
            data: $scope.currencies
        });
        $("#liSweepCurrency" + key).select2({
            placeholder: "Select a Sweep Currency",
            allowClear: true,
            data: $scope.currencies
        });
        $("#liCashInstruction" + key).select2({
            placeholder: "select a Cash Instruction",
            allowClear: true,
            data: $scope.cashInstructions
        });

        $("#liAccountPurpose" + key).select2({
            placeholder: "Select a Account Type",
            allowClear: true,
            data: $scope.accountPurpose
        });
        $("#liAccountStatus" + key).select2({
            placeholder: "Select a Account Status",
            allowClear: true,
            data: $scope.accountStatus
        });
        $("#liCustodyAcct" + key).select2({
            placeholder: "Select a Associated Custody Account",
            allowClear: true,
            data: $scope.cusodyAccountData
        });
    }

    function viewSsiTemplateTable(data, key) {

        if ($("#ssiTemplateTable" + key).hasClass("initialized")) {
            fnDestroyDataTable("#ssiTemplateTable" + key);
        }

        if (data.length > 0)
            $("#btnAccountMapStatusButtons").show();
        else
            $("#btnAccountMapStatusButtons").hide();

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
                    "mData": "AccountNumber",
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
                        // return " <label class=\"label ng-show-only shadowbox label-success\" style=\"font-size: 12px;\">" + row.CompletedCount + "</label> <label class=\"label ng-show-only shadowbox label-warning\"  style=\"font-size: 12px;\">" + row.InProcessCount + "</label> <label class=\"label ng-show-only shadowbox label-info\" style=\"font-size: 12px;\">" + row.TbdCount + "</label>";
                        return "<a class=\"btn btn-primary btn-xs\" id=\"" + data + "\" ><i class=\"glyphicon glyphicon-share-alt\"></i></a>";
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

        window.setTimeout(function () {
            ssiMapTable[key].columns.adjust().draw(true);
        }, 10);


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

    $scope.fnSsiTemplateMap = function (accountId, fundId, index) {
        $http.get("/Accounts/GetAccountSsiTemplateMap?accountId=" + accountId + "&fundId=" + fundId).then(function (response) {
            $scope.ssiTemplates = response.data.ssiTemplates;
            $scope.ssiTemplateMaps = response.data.ssiTemplateMaps;
            if ($scope.ssiTemplateMaps != null && $scope.ssiTemplateMaps != undefined && $scope.ssiTemplateMaps.length > 0) {
                //$scope.onBoardingAccountDetails[index].onBoardingAccountSSITemplateMaps = $scope.ssiTemplateMaps;
                viewSsiTemplateTable($scope.ssiTemplateMaps, index);
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

    function attachment(key) {

        $("#uploadFiles" + key).dropzone({
            url: "/Accounts/UploadAccountFiles?accountId=" + $scope.onBoardingAccountId,
            dictDefaultMessage: "<span>Drag/Drop account documents here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
            autoDiscover: false,
            acceptedFiles: ".csv,.txt,.pdf,.xls,.xlsx,.zip,.rar",
            maxFiles: 5,
            previewTemplate: "<div class='row col-sm-2'><div class='panel panel-success panel-dz'> <div class='panel-heading'> <h3 class='panel-title' style='text-overflow: ellipsis;white-space: nowrap;overflow: hidden;'><span data-dz-name></span> - (<span data-dz-size></span>)</h3> " +
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
                //    this.options.url = "/Accounts/UploadAccountFiles";
                //});
            },
            processing: function (file, result) {
                $("#uploadFiles" + key).animate({ "min-height": "120px" });
            },
            success: function (file, result) {
                $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
                $(".dzFileProgress").html("Upload Successful");

                var aDocument = result;
                $.each(aDocument.Documents, function (index, value) {
                    // $scope.ssiTemplateDocuments.push(value);
                    $scope.accountDocuments.push(value);
                });

                viewAttachmentTable($scope.accountDocuments, key);
            },
            queuecomplete: function () {
            },
            complete: function (file, result) {
                $("#uploadFiles" + key).removeClass("dz-drag-hover");

                if (this.getRejectedFiles().length > 0 && this.getAcceptedFiles().length === 0 && this.getQueuedFiles().length === 0) {
                    showMessage("File format is not supported to upload.", "Status");
                    return;
                }

                if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
                    notifySuccess("Files Uploaded successfully");
                }
            }
        });
    }

    $scope.fnAccountDocuments = function (accountId, index) {
        $http.get("/Accounts/GetAccountDocuments?accountId=" + accountId).then(function (response) {
            $scope.accountDocuments = response.data.accountDocuments;
            if ($scope.accountDocuments != null && $scope.accountDocuments != undefined && $scope.accountDocuments.length > 0) {
                // $scope.onBoardingAccountDetails[index].onBoardingAccountSSITemplateMaps = $scope.ssiTemplateMaps;
                viewAttachmentTable($scope.accountDocuments, index);
            }

            if ($scope.accountDocuments.length > 0 && $scope.onBoardingAccountDetails[index].onBoardingAccountStatus == approvedStatus) {
                $(".dz-hidden-input").prop("disabled", true);
            } else {
                $(".dz-hidden-input").prop("disabled", false);
            }
        });
    }

    $scope.fnIntialize = function () {
        // $("#btnAccountDescription").hide();

        //Custody account
        var cusodyAccountList = $.grep($scope.allAccountList, function (v) { return v.AccountType == "Custody"; });
        $scope.cusodyAccountData = [];
        $.each(cusodyAccountList, function (key, value) {
            $scope.cusodyAccountData.push({ "id": value.AccountName, "text": value.AccountName });
        });


        $.each($scope.onBoardingAccountDetails, function (key, value) {

            $scope.fnGetAccountDescriptions(key);
            $scope.fnLoadContactDetails($scope.BrokerId, value.ContactName, key);
            $scope.fnLoadDefaultDropDowns(key);
            $scope.fnGetAuthorizedParty(key);
            $scope.fnGetSwiftGroup(key);
            var cashSweepTimeDiv = ".cashSweepTimeDiv" + key;
            var cashSweepTime = "#cashSweepTime" + key;
            if (value.CashSweep == "Yes") {
                $(cashSweepTimeDiv).show();
                //if (value.CashSweepTime != null && value.CashSweepTime != "" && value.CashSweepTime != undefined) {
                //    var times = value.CashSweepTime.split(':');
                //    value.CashSweepTime = new Date(2014, 0, 1, times[0], times[1], times[2]);
                //    $(cashSweepTime).val(value.CashSweepTime);
                //}


                $("#cashSweepTimeZone" + key).val(value.CashSweepTimeZone);
            }
            else $(cashSweepTimeDiv).hide();

            var descriptionValue = "#liAccountDescriptions" + key;
            $(descriptionValue).val(value.Description);

            //var contactNameValue = "#contactName" + key;
            //$(contactNameValue).val(value.ContactName);



            $scope.fnToggleBeneficiaryBICorABA(value.BeneficiaryType, "Beneficiary", key);
            $scope.fnToggleBeneficiaryBICorABA(value.IntermediaryType, "Intermediary", key);
            $scope.fnToggleBeneficiaryBICorABA(value.UltimateBeneficiaryType, "UltimateBeneficiary", key);

            //var contactEmailValue = "#contactEmail" + key;
            //$(contactEmailValue).val(value.ContactEmail);        
            //var contactNumberValue = "#contactNumber" + key;
            //$(contactNumberValue).val(value.ContactNumber);
            if (value.onBoardingAccountStatus == createdStatus) {
                $("#spnAgrCurrentStatus").html("Saved as Draft");
                $("#hmStatus").show();
                $("#btnPendingApproval").show();
                $("#btnApprove").hide();
                $("#btnRevert").hide();
                $("#btnSave").show();
            }
            else if (value.onBoardingAccountStatus == pendingStatus && value.UpdatedBy != $("#userName").val()) {
                $("#spnAgrCurrentStatus").html(value.onBoardingAccountStatus);
                $("#hmStatus").show();
                $("#spnAgrCurrentStatus").removeClass("text-default").removeClass("text-success").addClass("text-warning");
                $("#btnPendingApproval").hide();
                $("#btnApprove").show();
                $("#btnRevert").hide();
                $("#btnSave").hide();
            }
            else if (value.onBoardingAccountStatus == pendingStatus && value.UpdatedBy == $("#userName").val()) {
                $("#spnAgrCurrentStatus").html(value.onBoardingAccountStatus);
                $("#hmStatus").show();
                $("#spnAgrCurrentStatus").parent().removeClass("text-default").removeClass("text-success").addClass("text-warning");
                $("#btnPendingApproval").hide();
                $("#btnApprove").hide();
                $("#btnRevert").hide();
                $("#btnSave").show();
            }
            else if (value.onBoardingAccountStatus == approvedStatus) {
                $("#spnAgrCurrentStatus").html(value.onBoardingAccountStatus);
                $("#hmStatus").show();
                $("#spnAgrCurrentStatus").parent().removeClass("text-default").removeClass("text-warning").addClass("text-success");
                $("#btnPendingApproval").hide();
                $("#btnApprove").hide();
                $("#btnRevert").hide();
                $("#btnSave").hide();
            } else {
                $("#btnPendingApproval").hide();
                $("#btnApprove").hide();
                $("#btnRevert").hide();
                $("#btnSave").hide();
            }
            $scope.fnSsiTemplateMap(value.onBoardingAccountId, $scope.FundId, key);
            attachment(key);
            $scope.fnAccountDocuments(value.onBoardingAccountId, key);

        });


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
        $scope.$watch('watchAccountDetails', function (val, oldVal) {
            if (val != oldVal && $("#spnAgrCurrentStatus").html() == pendingStatus) {
                $("#btnApprove").hide();
                $("#btnSave").show();
            }
            if (val != oldVal && $("#spnAgrCurrentStatus").html() == approvedStatus) {
                $("#btnPendingApproval").show();
                $("#btnSave").show();
            }

        }, true);
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
        var ssiTemplateExists = $.grep($scope.ssiTemplateMaps, function (v) { return v.onBoardingSSITemplateId == $("#liSsiTemplate").val(); })[0];

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
            onBoardingAccountId: $scope.onBoardingAccountId,
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

        $http.post("/Accounts/AddAccountSsiTemplateMap", { accountSsiTemplateMap: $scope.onBoardingAccountSSITemplateMap }).then(function () {
            notifySuccess("Ssi template mapped to account successfully");
            $scope.ssiTemplateMaps.push($scope.onBoardingAccountSSITemplateMap);
            //$scope.onBoardingAccountDetails[$scope.PanelIndex].onBoardingAccountSSITemplateMaps.push($scope.onBoardingAccountSSITemplateMap);
            viewSsiTemplateTable($scope.ssiTemplateMaps, $scope.PanelIndex);
        });

        $("#accountSSITemplateMapModal").modal("hide");
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

    $scope.downloadAccountSample = function () {
        window.location.href = "/Accounts/ExportSampleAccountlist";
    }

    Dropzone.options.myAwesomeDropzone = false;
    Dropzone.autoDiscover = false;

    $("#uploadFiles").dropzone({
        url: "/Accounts/UploadAccount",
        dictDefaultMessage: "<span>Drag/Drop account files to add/update here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".csv,.xls,.xlsx",
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
                this.options.url = "/Accounts/UploadAccount";
            });
        },
        processing: function (file, result) {
            $("#uploadFiles").animate({ "min-height": "140px" });
        },
        success: function (file, result) {
            $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
            $(".dzFileProgress").html("Upload Successful");
            $("#loading").show();
            fnDestroyDataTable("#accountTable");
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
        window.open("/" + subDomain + "Contact/OnboardContact?onBoardingTypeId=3&entityId=" + $scope.BrokerId + "&contactId=0", "_blank");
    }

    $scope.fnAddAccountDescription = function () {
        if ($('#txtDescription').val() == undefined || $('#txtDescription').val() == "") {
            //pop-up    
            $("#txtDescription").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Description cannot be empty. Please add a valid Description",
                html: true,
                width: "250px"
            });

            $("#txtDescription").popover("show");
            return;
        }

        $("#txtDescription").popover("hide");
        var isExists = false;
        $($scope.AccountDescriptions).each(function (i, v) {
            if ($("#txtDescription").val() == v.text) {
                isExists = true;
                return false;
            }
        });
        if (isExists) {
            $("#txtDescription").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Description is already exists. Please enter a valid Description",
                html: true,
                width: "250px"
            });
            //notifyWarning("Governing Law is already exists. Please enter a valid governing law");
            $("#txtDescription").popover("show");
            return;
        }

        $http.post("/Accounts/AddAccountDescriptions", { accountDescription: $("#txtDescription").val(), agreementTypeId: $scope.AgreementTypeId }).then(function (response) {
            notifySuccess("Description added successfully");
            $scope.onBoardingAccountDetails[$scope.PanelIndex].Description = $("#txtDescription").val();
            $scope.fnGetAccountDescriptions($scope.PanelIndex);
            $("#txtDescription").val("");
        });


        $('#accountDescriptionModal').modal('hide');
    }

    $scope.fnAddAccountDescriptionModal = function (panelIndex) {
        $scope.PanelIndex = panelIndex;
        //$scope.scrollPosition = $(window).scrollTop();
        //$("#txtGoverningLaw").prop("placeholder", "Enter a governing law");
        $('#accountDescriptionModal').modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtDescription").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }
    angular.element("#txtDescription").on("focusin", function () { angular.element("#txtDescription").popover("hide"); });

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
            BICorABA: $("#txtBICorABA").val(),
            BankName: $("#txtBankName").val(),
            BankAddress: $("#txtBankAddress").val(),
            IsABA: $("#btnBICorABA").prop("checked")
        }


        $http.post("/Accounts/AddAccountBiCorAba", { accountBiCorAba: $scope.accountBeneficiary }).then(function (response) {
            notifySuccess("Beneficiary BIC or ABA added successfully");
            $scope.BicorAba = $("#txtBICorABA").val();
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

        $http.post("/Accounts/AddSwiftGroup", { swiftGroup: $("#txtSwiftGroup").val(), senderBic: $("#txtSendersBIC").val() }).then(function (response) {
            notifySuccess("Swift Group added successfully");
            $scope.onBoardingAccountDetails[$scope.PanelIndex].SwiftGroup = $("#txtSwiftGroup").val();
            $scope.onBoardingAccountDetails[$scope.PanelIndex].SendersBIC = $("#txtSendersBIC").val();
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


    //Association View

    function hierarchyFormat(aId) {
        return "<div class=\"slider center-block onboardingMapping\" style=\"margin-bottom: 10px !important;\">" +
            "<img src=\"../img/loading.gif\" alt=\"Loading...\" style='position:absolute;text-align:center;' class='onboardingLoadSpinner col-md-offset-6'/>" +
            "<table id=\"accountRowTable" + aId + "\" class=\"table table-bordered table-condensed\" cellpadding=\"5\" cellspacing=\"0\" border=\"0\" width=\"100%\"></table>" +
            "</div>";
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
                 { "mData": "onBoardingAccountId", "sTitle": "onBoardingAccountId", visible: false },
                 { "mData": "dmaAgreementOnBoardingId", "sTitle": "dmaAgreementOnBoardingId", visible: false },
                 { "mData": "AgreementTypeId", "sTitle": "AgreementTypeId", visible: false },
                 { "mData": "BrokerId", "sTitle": "BrokerId", visible: false },
                 {
                     "orderable": false,
                     "data": null,
                     "defaultContent": "<i class=\"glyphicon glyphicon-menu-right\" style=\"cursor:pointer;\"></i>"
                 },
                 {
                     "mData": "AccountType", "sTitle": "Entity Type",
                     "mRender": function (tdata) {
                         if (tdata != null && tdata != "undefinied") {
                             switch (tdata) {
                                 case "Agreement": return "<label class='label label-success'>" + tdata + "</label>";
                                 case "DDA": return "<label class='label label-warning'>" + tdata + "</label>";
                                 case "Custody": return "<label class='label label-info'>" + tdata + "</label>";
                             }
                             return "<label class='label label-default'>" + tdata + "</label>";
                         }
                         return "";
                     }
                 },
                 {
                     "mData": "FundName", "sTitle": "Fund Name"
                 },
                 { "mData": "AgreementName", "sTitle": "Agreement Name" },
                 { "mData": "Broker", "sTitle": "Broker" },
                 //{ "mData": "AccountName", "sTitle": "Account Name" },
                 { "mData": "AccountNumber", "sTitle": "Account Number" },
                 {
                     "mData": "onBoardingAccountStatus", "sTitle": "Account Status",
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
                     "mData": "CreatedBy", "sTitle": "Created By", "mRender": function (data) {
                         return humanizeEmail(data);
                     }
                 },
                 {
                     "mData": "CreatedAt",
                     "sTitle": "Created Date",
                     "type": "dotnet-date",
                     "mRender": function (tdata) {
                         return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                     }
                 },
                 {
                     "mData": "UpdatedBy", "sTitle": "Last Modified By", "mRender": function (data) {
                         return humanizeEmail(data);
                     }
                 },
                 {
                     "mData": "UpdatedAt",
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
                  switch (data.onBoardingAccountStatus) {
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

    $http.get("/Accounts/GetAccountAssociationPreloadData").then(function (response) {
        $scope.allFunds = response.data.funds;
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
            accountList = $.grep($scope.allAccountList, function (v) { return v.dmaFundOnBoardId == fundId; });
        }
        viewAssociationTable(accountList);
    });

    $(document).on("click", "#accountSSITemplateTable tbody tr td:first-child ", function () {

        var tr = $(this).parent();
        var row = accountSsiTemplateTable.row(tr);
        if (row.data() != undefined) {
            var onBoardingAccountId = row.data().onBoardingAccountId;
            var fId = row.data().dmaFundOnBoardId;

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
                $http.get("/Accounts/GetAccountSsiTemplateMap?accountId=" + onBoardingAccountId + "&fundId=" + fId).then(function (response) {

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
                                "mData": "CreatedBy", "mRender":function(data) {
                                    return humanizeEmail(data);
                                }
                            },
                            {
                                "sTitle": "Updated By",
                                "mData": "UpdatedBy", "mRender":function(data) {
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
        window.open("/Accounts/SSITemplate?ssiTemplateId=" + ssitemplateId, "_blank");
    });

});