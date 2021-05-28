$("#liSSITemplates").addClass("active");
HmOpsApp.controller("SSITemplateCtrl", function ($scope, $http, $timeout, $filter, $q) {
    $("#onboardingMenu").addClass("active");
    $scope.ssiTemplate = {};
    $scope.ssiTemplates = [];
    $scope.broker = "";
    $scope.accountType = "";
    $scope.currency = "";
    $scope.serviceProvider = "";
    $scope.reasonDetail = "";
    $scope.BrokerTemplateTypeId = 2;
    $scope.IsBNYMBroker = false;
    $scope.IsPendingApproval = false;
    $scope.SSITemplateTypeData = [{ id: "Broker", text: "Broker" }, { id: "Fee/Expense Payment", text: "Fee/Expense Payment" }];
    $scope.messageTypes = [{ id: "MT103", text: "MT103" }, { id: "MT202", text: "MT202" }, { id: "MT202 COV", text: "MT202 COV" }];
    // { id: "MT210", text: "MT210" }, { id: "MT540", text: "MT540" },{ id: "MT542", text: "MT542" }
    $scope.ssiTemplate.TemplateName = "";
    $scope.ssiTemplateDocuments = [];
    var tblDocuments, tblAssociatedAccounts;
    var documentData = "\"FileName\": \"\",\"RecCreatedBy\": \"\",\"RecCreatedAt\": \"\"";
    $scope.beneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }];
    $scope.ultimateBeneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }, { id: "Account Name", text: "Account Name" }];


    $scope.fnConstructAssociatedAccountsTable = function (data) {
        if ($("#tblAssociatedAccounts").hasClass("initialized")) {
            fnDestroyDataTable("#tblAssociatedAccounts");
        }
        tblAssociatedAccounts = $("#tblAssociatedAccounts").DataTable({
            aaData: data,
            "bDestroy": true,
            "columns": [
                { "mData": "onBoardingAccountId", "sTitle": "onBoardingAccountId", visible: false },
                { "mData": "dmaAgreementOnBoardingId", "sTitle": "dmaAgreementOnBoardingId", visible: false },
                { "mData": "AgreementTypeId", "sTitle": "AgreementTypeId", visible: false },
                { "mData": "BrokerId", "sTitle": "BrokerId", visible: false },
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
                { "mData": "UltimateBeneficiaryAccountNumber", "sTitle": "Account Number" },
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
            tblAssociatedAccounts.columns.adjust().draw(true);
        }, 50);

    }


    $scope.fnConstructDocumentTable = function (data) {

        if ($("#tblDocuments").hasClass("initialized")) {
            fnDestroyDataTable("#tblDocuments");
        }
        tblDocuments = $("#tblDocuments").not(".initialized").addClass("initialized").DataTable({
            "bDestroy": true,
            responsive: true,
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
                        return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + (moment(tdata).fromNow()) + "</div>";
                    }
                },
                {
                    "mData": "onBoardingSSITemplateDocumentId",
                    "sTitle": "Remove Document", "className": "dt-center",
                    "mRender": function () {
                        return "<button class='btn btn-danger btn-xs' title='Remove Document'><i class='glyphicon glyphicon-trash'></i></button>";
                    }
                }
            ],
            "deferRender": false,
            "bScrollCollapse": true,
            scroller: true,
            //"sDom": 'iftrI',
            //"scrollX": true,
            pagination: true,
            "scrollX": false,
            "order": [[2, "desc"]],
            "fnRowCallback": function (nRow, aData) {
                if (aData.FileName != "") {
                    $("td:eq(0)", nRow).html("<a title ='click to download the file' href='/SSITemplate/DownloadSsiTemplateFile?fileName=" + getFormattedFileName(aData.FileName) + "&ssiTemplateId=" + ssiTemplateId + "'>" + aData.FileName + "</a>");
                }
            },
            "scrollY": $("#panelAttachment").offset().top + 300,
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No files are available for the ssi templates",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Files"
            }
        });
        $timeout(function () {
            $("#tblDocuments").dataTable().fnAdjustColumnSizing();
            $scope.ssiTemplate.onBoardingSSITemplateDocuments = angular.copy(data);
        }, 100);
        $("#tblDocuments tbody td:last-child button").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowElement = tblDocuments.row(selectedRow).data();
            $timeout(function () {
                bootbox.confirm("Are you sure you want to remove this document from ssi template?", function (result) {
                    if (!result) {
                        return;
                    } else {
                        $http.post("/SSITemplate/RemoveSsiTemplateDocument", { documentId: rowElement.onBoardingSSITemplateDocumentId }).then(function () {
                            tblDocuments.row(selectedRow).remove().draw();
                            $scope.ssiTemplateDocuments.pop(rowElement);
                            $scope.ssiTemplate.onBoardingSSITemplateDocuments.pop(rowElement);
                            notifySuccess("Document removed succesfully");
                        });
                    }
                });
            }, 50);

        });
    }

    $scope.fnGetBankDetails = function (biCorAbaValue, id) {
        $timeout(function () {
            var accountBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.BICorABA == biCorAbaValue; })[0];

            switch (id) {
                case "Beneficiary":
                    $scope.ssiTemplate.Beneficiary = {};
                    $scope.ssiTemplate.BeneficiaryBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.ssiTemplate.Beneficiary.onBoardingAccountBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.ssiTemplate.Beneficiary.BICorABA = accountBicorAba == undefined ? "" : accountBicorAba.BICorABA;
                    $scope.ssiTemplate.Beneficiary.BankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.ssiTemplate.Beneficiary.BankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
                case "Intermediary":
                    $scope.ssiTemplate.Intermediary = {};
                    $scope.ssiTemplate.IntermediaryBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.ssiTemplate.Intermediary.onBoardingAccountBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.ssiTemplate.Intermediary.BICorABA = accountBicorAba == undefined ? "" : accountBicorAba.BICorABA;
                    $scope.ssiTemplate.Intermediary.BankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.ssiTemplate.Intermediary.BankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
                case "UltimateBeneficiary":
                    $scope.ssiTemplate.UltimateBeneficiary = {};
                    $scope.ssiTemplate.UltimateBeneficiaryBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.ssiTemplate.UltimateBeneficiary.onBoardingAccountBICorABAId = accountBicorAba == undefined ? "" : accountBicorAba.onBoardingAccountBICorABAId;
                    $scope.ssiTemplate.UltimateBeneficiary.BICorABA = accountBicorAba == undefined ? "" : accountBicorAba.BICorABA;
                    $scope.ssiTemplate.UltimateBeneficiary.BankName = accountBicorAba == undefined ? "" : accountBicorAba.BankName;
                    $scope.ssiTemplate.UltimateBeneficiary.BankAddress = accountBicorAba == undefined ? "" : accountBicorAba.BankAddress;
                    break;
            }

        }, 100);

    }

    $scope.fnToggleBeneficiaryBICorABA = function (item, id) {
        //var $toggleBtn = $("#" + id + index);

        var isAba = (item == "ABA");

        switch (id) {
            case "Beneficiary":
                //$scope.ssiTemplate.IsBeneficiaryABA = $("#btnBeneficiaryBICorABA" + index).prop("checked");

                var accountBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                var accountBicorAbaData = [];
                $.each(accountBicorAba, function (key, value) {
                    accountBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                });

                accountBicorAbaData = $filter("orderBy")(accountBicorAbaData, "text");

                if ($("#liBeneficiaryBICorABA").data("select2")) {
                    $("#liBeneficiaryBICorABA").select2("destroy");
                }
                $("#liBeneficiaryBICorABA").select2({
                    placeholder: "Select a beneficiary BIC or ABA",
                    allowClear: true,
                    data: accountBicorAbaData
                });
                break;
            case "Intermediary":
                //$scope.ssiTemplate.IsIntermediaryABA = $("#btnIntermediaryBICorABA" + index).prop("checked");
                var intermediaryBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                var intermediaryBicorAbaData = [];
                $.each(intermediaryBicorAba, function (key, value) {
                    intermediaryBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                });

                intermediaryBicorAbaData = $filter("orderBy")(intermediaryBicorAbaData, "text");

                if ($("#liIntermediaryBICorABA").data("select2")) {
                    $("#liIntermediaryBICorABA").select2("destroy");
                }
                $("#liIntermediaryBICorABA").select2({
                    placeholder: "Select a intermediary BIC or ABA",
                    allowClear: true,
                    data: intermediaryBicorAbaData
                });
                break;
            case "UltimateBeneficiary":
                //$scope.ssiTemplate.IsUltimateBeneficiaryABA = $("#btnUltimateBICorABA" + index).prop("checked");
                $scope.ssiTemplate.UltimateBeneficiaryType = item;
                if (item == "Account Name") {
                    $("#divUltimateBeneficiaryBICorABA").hide();
                    $("#ultimateBankName").hide();
                    $("#ultimateBankAddress").hide();
                    $("#accountName").show();
                    $scope.ssiTemplate.UltimateBeneficiary = {};
                    //$scope.ssiTemplate.UltimateBeneficiaryBICorABA = null;
                    //$scope.ssiTemplate.UltimateBeneficiaryBankName = null;
                    //$scope.ssiTemplate.UltimateBeneficiaryBankAddress = null;
                    return;
                } else {
                    $("#divUltimateBeneficiaryBICorABA").show();
                    $("#ultimateBankName").show();
                    $("#ultimateBankAddress").show();
                    $("#accountName").hide();
                    $scope.ssiTemplate.UltimateBeneficiaryAccountName = null;
                }
                var ultimateBicorAba = $.grep($scope.accountBicorAba, function (v) { return v.IsABA == isAba; });
                var ultimateBicorAbaData = [];
                $.each(ultimateBicorAba, function (key, value) {
                    ultimateBicorAbaData.push({ "id": value.BICorABA, "text": value.BICorABA });
                });

                ultimateBicorAbaData = $filter("orderBy")(ultimateBicorAbaData, "text");

                if ($("#liUltimateBeneficiaryBICorABA").data("select2")) {
                    $("#liUltimateBeneficiaryBICorABA").select2("destroy");
                }
                $("#liUltimateBeneficiaryBICorABA").select2({
                    placeholder: "Select a ultimate beneficiary BIC or ABA",
                    allowClear: true,
                    data: ultimateBicorAbaData
                });

                break;
        }
    }
    $scope.fnGetCurrency = function () {
        $http.get("/FundAccounts/GetAllCurrencies").then(function (response) {
            $scope.currencies = response.data.currencies;

            $("#liCurrency").select2({
                placeholder: "Select a Currency",
                allowClear: true,
                data: response.data.currencies
            });

            if ($scope.ssiTemplate.Currency != null && $scope.ssiTemplate.Currency != "undefined")
                $("#liCurrency").select2("val", $scope.ssiTemplate.Currency);
        });
    }

    $scope.fnGetBicorAba = function (isNew) {
        $http.get("/FundAccounts/GetAllAccountBicorAba").then(function (response) {
            $scope.accountBicorAba = response.data.accountBicorAba;


            if (isNew) {
                var isAba = $scope.isBicorAba == true ? "ABA" : "BIC";
                $scope.fnToggleBeneficiaryBICorABA(isAba, "Beneficiary");
                $scope.ssiTemplate.BeneficiaryBICorABA = isAba;
                $("#liBeneficiaryBICorABA").select2("val", isAba);
            } else {
                if (ssiTemplateId !== 0 && ssiTemplateId !== "0") {
                    $scope.fnToggleBeneficiaryBICorABA($scope.ssiTemplate.BeneficiaryType, "Beneficiary");
                    $scope.fnToggleBeneficiaryBICorABA($scope.ssiTemplate.IntermediaryType, "Intermediary");
                    $scope.fnToggleBeneficiaryBICorABA($scope.ssiTemplate.UltimateBeneficiaryType, "UltimateBeneficiary");
                }
            }
        });
    }


    $("#liUltimateBeneficiaryType").on("change", function (e) {
        $("#liUltimateBeneficiaryBICorABA").select2("val", '').trigger("change");
        $("#ultimateBankName").val("");
        $("#ultimateBankAddress").val("");
    });

    $("#liBeneficiaryType").on("change", function (e) {
        $("#liBeneficiaryBICorABA").select2("val", '').trigger("change");
        $("#beneficiaryBankName").val("");
        $("#beneficiaryBankAddress").val("");
    });

    $("#liIntermediaryType").on("change", function (e) {
        $("#liIntermediaryBICorABA").select2("val", '').trigger("change");
        $("#intermediaryBankName").val("");
        $("#intermediaryBankAddress").val("");
    });

    $scope.fnBrokerList = function () {
        $http.get("/SSITemplate/GetSsiTemplatePreloadData").then(function (response) {
            $scope.counterparties = response.data.counterParties;
            $scope.ssiTemplates = response.data.templates;
            $scope.AccountTypes = response.data.accountTypes;
            $scope.currencies = response.data.currencies;
            //$scope.serviceProviders = response.data.serviceProviders;            

            $("#liSSITemplateType").select2({
                placeholder: "Select Template Type",
                allowClear: true,
                data: $scope.SSITemplateTypeData
            });

            $("#liBroker").select2({
                placeholder: "Select a broker",
                allowClear: true,
                data: $scope.counterparties
            });

            $("#liAccountType").select2({
                placeholder: "Select an account type",
                allowClear: true,
                data: $scope.AccountTypes
            });
            //if ($scope.ssiTemplate.dmaAgreementTypeId != "") $("#liAccountType").val($scope.ssiTemplate.dmaAgreementTypeId);

            $("#liCurrency").select2({
                placeholder: "Select a currency",
                allowClear: true,
                data: $scope.currencies
            });

            $("#liBeneficiaryType").select2({
                placeholder: "Select a BIC or ABA",
                allowClear: true,
                data: $scope.beneficiaryType
            });

            $("#liIntermediaryType").select2({
                placeholder: "Select a BIC or ABA",
                allowClear: true,
                data: $scope.beneficiaryType
            });

            $("#liUltimateBeneficiaryType").select2({
                placeholder: "Select a BIC or ABA",
                allowClear: true,
                data: $scope.ultimateBeneficiaryType
            });
            $("#liMessageType").select2({
                placeholder: "Select an message type",
                allowClear: true,
                data: $scope.messageTypes
            });
            $scope.fnGetBicorAba(false);

            // if ($scope.ssiTemplate.Currency != "") $("#liCurrency").val($scope.ssiTemplate.Currency);

            //$scope.ssiTemplate.SSITemplateType != "" && $scope.ssiTemplate.SSITemplateType != "undefined" && 
            if (ssiTemplateId == 0) {
                $scope.ssiTemplate.SSITemplateType = "Broker";
                $scope.SSITemplateType = $scope.ssiTemplate.SSITemplateType;
            } else {
                //$("#liBroker").val($scope.ssiTemplate.TemplateEntityId);
                $scope.fnSSITemplateType($scope.ssiTemplate.SSITemplateType);
                $scope.ReasonDetail = $scope.ssiTemplate.ReasonDetail;
            }
            $scope.ssiTemplate.TemplateName = $scope.ssiTemplate.TemplateName.replace("undefined", "");
        });

    }

    $("#liBroker").change(function () {
        if ($scope.SSITemplateType == "Broker") {
            $scope.broker = ($(this).val() > 0) ? $("#liBroker").select2("data").text : "";
            $scope.IsBNYMBroker = $scope.broker == "The Bank of New York Mellon";
            $scope.ssiTemplate.TemplateName = $scope.broker + " - " + $scope.accountType + " - " + $scope.currency + " - " + $scope.reasonDetail;
        }
    });
    $("#liAccountType").change(function () {
        if ($scope.SSITemplateType == "Broker") {
            $scope.accountType = ($(this).val() > 0) ? $("#liAccountType").select2("data").text : "";
            $scope.ssiTemplate.TemplateName = $scope.broker + " - " + $scope.accountType + " - " + $scope.currency + " - " + $scope.reasonDetail;
            $scope.fnPaymentOrReceiptReason();
        }
    });
    $("#liCurrency").change(function () {
        $scope.currency = $(this).val();
        if ($scope.SSITemplateType == "Broker")
            $scope.ssiTemplate.TemplateName = $scope.broker + " - " + $scope.accountType + " - " + $scope.currency + " - " + $scope.reasonDetail;
        else
            $scope.ssiTemplate.TemplateName = $scope.serviceProvider + " - " + $scope.currency + " - " + $scope.reasonDetail;
    });

    $("#liServiceProvider").change(function () {
        $scope.serviceProvider = $(this).val();
        $scope.ssiTemplate.TemplateName = $scope.serviceProvider + " - " + $scope.currency + " - " + $scope.reasonDetail;
        $scope.fnPaymentOrReceiptReason();
    });

    $("#liReasonDetail").change(function () {
        $scope.reasonDetail = $(this).val();
        if ($scope.SSITemplateType == "Broker")
            $scope.ssiTemplate.TemplateName = $scope.broker + " - " + $scope.accountType + " - " + $scope.currency + " - " + $scope.reasonDetail;
        else
            $scope.ssiTemplate.TemplateName = $scope.serviceProvider + " - " + $scope.currency + " - " + $scope.reasonDetail;
        if ($scope.reasonDetail == "Other") {
            $("#otherReason").show();
        } else
            $("#otherReason").hide();

    });
    $scope.isLoad = false;
    if (ssiTemplateId !== 0 && ssiTemplateId !== "0") {
        $http.get("/SSITemplate/GetSsiTemplate?templateId=" + ssiTemplateId).then(function (response) {
            $scope.ssiTemplateId = ssiTemplateId;
            $scope.fnBrokerList();
            $scope.isLoad = true;
            $scope.isSSITemplateChanged = false;
            $scope.isAuthorizedUserToApprove = response.data.isAuthorizedUserToApprove;
            // $scope.fnPaymentOrReceiptReason();
            $scope.ssiTemplate = response.data.OnBoardingSsiTemplate;
            $scope.serviceProvider = $scope.ssiTemplate.ServiceProvider;
            $scope.reasonDetail = $scope.ssiTemplate.ReasonDetail;
            // $scope.SSITemplateType = $scope.ssiTemplate.SSITemplateType;
            $scope.ssiTemplate.CreatedAt = moment($scope.ssiTemplate.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
            $scope.IsKeyFieldsChanged = $scope.ssiTemplate.IsKeyFieldsChanged;
            if ($scope.ssiTemplate.SSITemplateType == "Broker") {
                var templateList = $scope.ssiTemplate.TemplateName.split("-");
                $scope.broker = templateList[0].trim();
                $scope.accountType = templateList[1].trim();
                $scope.currency = templateList[2].trim();
                $scope.reasonDetail = templateList[3].trim();

            }
            $scope.IsBNYMBroker = $scope.broker == "The Bank of New York Mellon";

            if ($scope.reasonDetail == "Other") {
                $("#otherReason").show();
            } else
                $("#otherReason").hide();

            $scope.ssiTemplateDocuments = response.data.document;
            if ($scope.ssiTemplateDocuments == null && $scope.ssiTemplateDocuments.length <= 0) {
                $scope.ssiTemplateDocuments = JSON.parse("{" + documentData + "}");
            }

            $scope.fnConstructDocumentTable($scope.ssiTemplateDocuments);
            $scope.viewCallbackTable($scope.ssiTemplate.hmsSSICallbacks);
            $scope.fnGetAssociatedAccounts();

            //$scope.fnConstructAssociatedAccountsTable($scope.associatedAccounts);

            if ($scope.ssiTemplateDocuments.length > 0 && $scope.ssiTemplate.SSITemplateStatus == "Approved") {
                $(".dz-hidden-input").prop("disabled", true);
            } else {
                $(".dz-hidden-input").prop("disabled", false);
            }

            $timeout(function () {
                $scope.watchSSITemplate = $scope.ssiTemplate;
                $timeout(function () {
                    $scope.isLoad = false;
                }, 1500);
            }, 1000);
        });

    } else {
        $scope.ssiTemplateId = ssiTemplateId;
        $scope.fnBrokerList();
        $scope.fnConstructDocumentTable(JSON.parse("{" + documentData + "}"));
    }

    $scope.CheckSSIFieldsChanges = function (val, oldVal) {
        if (val.Description != oldVal.Description || val.Currency != oldVal.currency || val.FFCName != oldVal.FFCName || val.FFCNumber != oldVal.FFCNumber || val.Reference != oldVal.Reference || val.IntermediaryAccountNumber != oldVal.IntermediaryAccountNumber || val.BeneficiaryAccountNumber != oldVal.BeneficiaryAccountNumber || val.UltimateBeneficiaryAccountNumber != oldVal.UltimateBeneficiaryAccountNumber || val.IntermediaryType != oldVal.IntermediaryType || val.BeneficiaryType != oldVal.BeneficiaryType || val.UltimateBeneficiaryType != oldVal.UltimateBeneficiaryType || val.Intermediary.BICorABA != oldVal.Intermediary.BICorABA || val.Beneficiary.BICorABA != oldVal.Beneficiary.BICorABA || val.UltimateBeneficiary.BICorABA != oldVal.UltimateBeneficiary.BICorABA || val.Intermediary.BankName != oldVal.Intermediary.BankName || val.Beneficiary.BankName != oldVal.Beneficiary.BankName || val.UltimateBeneficiary.BankName != oldVal.UltimateBeneficiary.BankName || val.UltimateBeneficiaryAccountName != oldVal.UltimateBeneficiaryAccountName || val.Intermediary.BankAddress != oldVal.Intermediary.BankAddress
            || val.Beneficiary.BankAddress != oldVal.Beneficiary.BankAddress || val.UltimateBeneficiary.BankAddress != oldVal.UltimateBeneficiary.BankAddress) {

            $scope.IsKeyFieldsChanged = true;
        }
    }

    $scope.$watch("watchSSITemplate", function (val, oldVal) {

        if (val == undefined || oldVal == undefined || $scope.isLoad) {
            $scope.isSSITemplateChanged = false;
            return;
        }

        if ($scope.IsCallBackChanged) {
            return;
        }

        $scope.CheckSSIFieldsChanges(val, oldVal);

        $scope.isSSITemplateChanged = val != oldVal;

    }, true);

    $scope.fnSSITemplateType = function (templateType) {
        $scope.SSITemplateType = templateType;
        if (templateType != "Broker")
            $scope.fnLoadServiceProvider();
        $scope.fnPaymentOrReceiptReason();
    }

    $scope.fnLoadServiceProvider = function () {
        return $http.get("/SSITemplate/GetAllServiceProviderList").then(function (response) {
            $scope.serviceProviders = response.data;
            $("#liServiceProvider").select2({
                placeholder: "Select Service Provider",
                allowClear: true,
                data: response.data
            });
        });
        //if ($scope.ssiTemplate.ServiceProvider != "")
        //    $("#liServiceProvider").val($scope.ssiTemplate.ServiceProvider);           
    }

    $scope.fnPaymentOrReceiptReason = function () {
        if ($("#liAccountType").val() > 0 || $("#liServiceProvider").val() != null) {
            $http.get("/SSITemplate/PaymentOrReceiptReasonDetails?templateType=" + $("#liSSITemplateType").val() + "&agreementTypeId=" + $("#liAccountType").val() + "&serviceProviderName=" + encodeURIComponent($("#liServiceProvider").val())).then(function (response) {
                $scope.SSIPaymentReasons = response.data;
                $scope.SSIPaymentReasons.push({ id: "Other", text: "Other" });
                $("#liReasonDetail").select2({
                    placeholder: "Select Reason",
                    allowClear: true,
                    data: $scope.SSIPaymentReasons
                });
            });
        }
        if ($scope.ssiTemplate.ReasonDetail != "") $scope.ReasonDetail = $scope.ssiTemplate.ReasonDetail;
    }

    $scope.fnGetAssociatedAccounts = function () {
        var brokerId = $scope.ssiTemplate.SSITemplateType == "Broker" ? $scope.ssiTemplate.TemplateEntityId : 0;
        var isServiceType = $scope.ssiTemplate.SSITemplateType != "Broker";
        $http.get("/FundAccounts/GetSsiTemplateAccountMap?ssiTemplateId=" + $scope.ssiTemplateId + "&brokerId=" + brokerId + "&currency=" + $scope.ssiTemplate.Currency + "&message=" + $scope.ssiTemplate.MessageType + "&isServiceType=" + isServiceType).then(function (response) {
            $scope.fundAccounts = response.data.fundAccounts;
            $scope.ssiTemplateMaps = response.data.ssiTemplateMaps;
            if ($scope.ssiTemplateMaps != null && $scope.ssiTemplateMaps != undefined && $scope.ssiTemplateMaps.length > 0) {
                //$scope.onBoardingAccountDetails[index].onBoardingAccountSSITemplateMaps = $scope.ssiTemplateMaps;
                viewSsiTemplateTable($scope.ssiTemplateMaps);
            }
        });
    }

    $scope.showAssociations = function () {
        $timeout(function () {
            $scope.ssiMapTable.columns.adjust().draw(true);
        }, 100);
    }

    function viewSsiTemplateTable(data) {

        if ($("#tblAssociatedAccounts").hasClass("initialized")) {
            fnDestroyDataTable("#tblAssociatedAccounts");
        }

        if (data.length > 0)
            $("#btnAccountMapStatusButtons").show();
        else
            $("#btnAccountMapStatusButtons").hide();

        $scope.ssiMapTable = $("#tblAssociatedAccounts").not(".initialized").addClass("initialized").DataTable({
            "bDestroy": true,
            //responsive: true,
            aaData: data,
            "aoColumns": [
                {
                    "sTitle": "Account Name",
                    "mData": "AccountName"
                },
                {
                    "mData": "AccountType",
                    "sTitle": "Account Type",
                    "mRender": function (tdata) {
                        if (tdata === "Agreement")
                            return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Agreement</label>";
                        if (tdata === "DDA")
                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">DDA</label>";
                        if (tdata == "Custody")
                            return "<label class=\"label ng-show-only label-primary\" style=\"font-size: 12px;\">Custody</label>";
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
                //{
                //    "mData": "onBoardingSSITemplateId", "sTitle": "Go to SSI Template", "className": "dt-center",
                //    "mRender": function (data, type, row) {
                //        // return " <label class=\"label ng-show-only  label-success\" style=\"font-size: 12px;\">" + row.CompletedCount + "</label> <label class=\"label ng-show-only  label-warning\"  style=\"font-size: 12px;\">" + row.InProcessCount + "</label> <label class=\"label ng-show-only  label-info\" style=\"font-size: 12px;\">" + row.TbdCount + "</label>";
                //        return "<a class=\"btn btn-primary btn-xs\" id=\"" + data + "\" ><i class=\"glyphicon glyphicon-share-alt\"></i></a>";
                //    }

                //},
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
                "sEmptyTable": "No fund accounts are available for the SSI Template",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ SSI Templates"
            }
        });

        $timeout(function () {
            $scope.ssiMapTable.columns.adjust().draw(true);
        }, 100);


        $("#tblAssociatedAccounts tbody tr td:last-child a").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowElement = $scope.ssiMapTable.row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this account from ssi template?", function (result) {
                if (!result) {
                    return;
                } else {
                    if (rowElement.onBoardingAccountSSITemplateMapId > 0) {
                        $http.post("/SSITemplate/RemoveSsiTemplateMap", { ssiTemplateMapId: rowElement.onBoardingAccountSSITemplateMapId }).then(function () {
                            $scope.ssiMapTable.row(selectedRow).remove().draw();
                            //$scope.ssiTemplateDocuments.pop(rowElement);             
                            notifySuccess("Fund account removed succesfully");
                            $scope.fnGetAssociatedAccounts();
                        });
                    } else {
                        $scope.ssiMapTable.row(selectedRow).remove().draw();
                        //$scope.onBoardingAccountDetails[rowIndex].onBoardingAccountSSITemplateMaps.pop(rowElement);
                        notifySuccess("Fund account removed succesfully");
                    }
                }
            });

        });
        //$("#accountDetailCP tbody tr td a.btn-primary").on("click", function (event) {
        //    event.preventDefault();
        //    var ssitemplateId = $(this).attr("id");
        //    window.open("/FundAccounts/SSITemplate?ssiTemplateId=" + ssitemplateId, "_blank");
        //});

        $("#tblAssociatedAccounts tbody tr").on("click", function (event) {
            event.preventDefault();

            $("#tblAssociatedAccounts tbody tr").removeClass("info");
            if (!$(this).hasClass("info")) {
                $(this).addClass("info");
            }

            var selectedRow = $(this);
            var rowElement = $scope.ssiMapTable.row(selectedRow).data();
            $scope.onBoardingAccountSSITemplateMapId = rowElement.onBoardingAccountSSITemplateMapId;

            if (rowElement.Status == "Pending Approval" && rowElement.onBoardingAccountSSITemplateMapId > 0 && rowElement.UpdatedBy != $("#userName").val()) {
                $("#btnAccountMapStatusButtons a[title='Approve']").removeClass("disabled");
                $scope.IsPendingApproval = true;
            }

        });
    }

    $scope.fnAddAccountSSITemplateMap = function () {

        $scope.onBoardingAccountSSITemplateMap = [];
        angular.forEach($("#ssiTemplateTableMap tr.info"), function (val, i) {
            var data = $scope.ssiTemplateTableMap.row($(val)).data();
            if (data != undefined) {
                $scope.onBoardingAccountSSITemplateMap.push({
                    onBoardingAccountSSITemplateMapId: 0,
                    onBoardingSSITemplateId: parseInt($scope.ssiTemplateId),
                    onBoardingAccountId: parseInt(data.onBoardingAccountId),
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
            notifySuccess("Accounts mapped to SSI Template successfully");
            $scope.fnGetAssociatedAccounts();
        });

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
        $http.post("/FundAccounts/UpdateAccountMapStatus", { status: $scope.AccountMapStatus, accountMapId: $scope.onBoardingAccountSSITemplateMapId, comments: $("#statusMapComments").val().trim() }).then(function () {
            notifySuccess("Ssi template account map  " + $scope.AccountMapStatus.toLowerCase() + " successfully");

            $("#btnAccountMapStatusButtons a[title='Approve']").addClass("disabled");

            var rowElement = $scope.ssiMapTable.row(".info").data();
            rowElement.Status = $scope.AccountMapStatus;
            rowElement.UpdatedBy = $("#userName").val();
            rowElement.Comments = $("#statusMapComments").val().trim();
            rowElement.UpdatedAt = moment();
            var selectedRowNode = $scope.ssiMapTable.row(".info").data(rowElement).draw().node();

            $(selectedRowNode).addClass("success").removeClass("warning");

        });
        $("#UpdateAccountMapStatusModal").modal("hide");

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
            aaData: $scope.fundAccounts,
            "aoColumns": [

                {
                    "sTitle": "Account Name",
                    "mData": "AccountName"
                },
                {
                    "mData": "AccountType",
                    "sTitle": "Account Type",
                    "mRender": function (tdata) {
                        if (tdata === "Agreement")
                            return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Agreement</label>";
                        if (tdata === "DDA")
                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">DDA</label>";
                        if (tdata == "Custody")
                            return "<label class=\"label ng-show-only label-primary\" style=\"font-size: 12px;\">Custody</label>";
                        return "";
                    }
                },
                {
                    "sTitle": "Account Number",
                    "mData": "UltimateBeneficiaryAccountNumber",
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
            "scrollX": $scope.fundAccounts.length > 0,
            "scrollY": "350px",
            //sortable: false,
            //"sDom": "ift",
            //pagination: true,
            "sScrollX": "100%",
            "sScrollXInner": "100%",
            "order": [[0, "asc"]],
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No fund accounts are available for the SSI Template",
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
            $("#liFundAccount").select2("val", []);
            $("#spnSsi").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }

    $(document).on("click", "#ssiTemplateTableMap tbody tr", function () {
        if ($(this).hasClass('info'))
            $(this).removeClass('info');
        else
            $(this).addClass('info');
        $timeout(function () {
            $scope.isAssociationVisible = $("#ssiTemplateTableMap tr.info").length > 0;
        }, 50);
    });

    $scope.fnAddSSITemplateDetail = function (panelIndex) {

        $scope.isAuthorizedUserToApprove = false;
        if ($("#txtDescription").val() == undefined || $("#txtDescription").val() == "") {
            //pop-up    
            $("#txtDescription").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: $scope.detail + " cannot be empty. Please add a valid " + $scope.detail,
                html: true,
                width: "250px"
            });

            $("#txtDescription").popover("show");
            return;
        }

        $("#txtDescription").popover("hide");
        var isExists = false;
        if ($scope.detail == "Reason") {
            isExists = $filter("filter")($scope.SSIPaymentReasons, { 'text': $("#txtDescription").val().trim() }, true).length > 0;
        }
        else {
            isExists = $filter("filter")($scope.serviceProviders, { 'text': $("#txtDescription").val().trim() }, true).length > 0;
        }

        if (isExists) {
            $("#txtDescription").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: $scope.detail + " already exists. Please enter a valid " + $scope.detail,
                html: true,
                width: "250px"
            });
            //notifyWarning("Governing Law is already exists. Please enter a valid governing law");
            $("#txtDescription").popover("show");
            return;
        }
        //PaymentOrReceiptReasonDetails?templateType=" + $("#liSSITemplateType").val() + "&agreementTypeId=" + $("#liAccountType").val() + "&serviceProviderName=" + $("#liServiceProvider").val()
        if ($scope.detail == "Reason") {
            $http.post("/SSITemplate/AddPaymentOrReceiptReasonDetails", { reason: $("#txtDescription").val(), templateType: $("#liSSITemplateType").val(), agreementTypeId: $("#liAccountType").val(), serviceProviderName: $("#liServiceProvider").val() }).then(function (response) {
                notifySuccess("Reason added successfully");
                $scope.fnPaymentOrReceiptReason();
                $("#liReasonDetail").val($("#txtDescription").val());
                //$scope.onBoardingAccountDetails[panelIndex].Description = $('#txtDescription').val();  
            });
        }
        else {
            $http.post("/SSITemplate/AddServiceProvider", { serviceProviderName: $("#txtDescription").val() }).then(function (response) {
                notifySuccess("Service Provider added successfully");
                $scope.fnLoadServiceProvider().then(function () {
                    var provider = $filter("filter")($scope.serviceProviders, { 'text': $("#txtDescription").val() }, true)[0];
                    $("#liServiceProvider").select2("val", provider.id).trigger("change");
                });
                //$scope.onBoardingAccountDetails[panelIndex].Description = $('#txtDescription').val();  
            });
        }

        //$("#txtDescription").val("");
        $("#ssiTemplateDetailModal").modal("hide");
    }

    $scope.fnAddSSITemplateDetailModal = function (detail) {
        //$scope.scrollPosition = $(window).scrollTop();
        //$("#txtGoverningLaw").prop("placeholder", "Enter a governing law");
        $scope.detail = detail;
        $("#ssiTemplateDetailModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtDescription").popover("hide").val("");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
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
            $scope.ssiTemplate.Currency = $("#txtCurrency").val();
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
            $scope.fnGetBicorAba(true);
            $("#txtBICorABA").val("");
            $("#txtBankName").val("");
            $("#txtBankAddress").val("");
            $("#btnBICorABA").prop("checked", false).change();
        });

        $("#beneficiaryABABICModal").modal("hide");
    }

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

    $scope.fnUpdateSSITemplate = function (isNewTemplate) {

        $scope.ssiTemplate.TemplateTypeId = $scope.BrokerTemplateTypeId;
        $scope.ssiTemplate.TemplateEntityId = $("#liBroker").val();
        $scope.ssiTemplate.dmaAgreementTypeId = $("#liSSITemplateType").val() == "Broker" ? $("#liAccountType").val() : 0;
        $scope.ssiTemplate.ServiceProvider = $("#liSSITemplateType").val() != "Broker" ? $("#liServiceProvider").val() : "";
        $scope.ssiTemplate.onBoardingSSITemplateDocuments = $scope.ssiTemplateDocuments;

        $scope.ssiTemplate.BeneficiaryBankName = $("#beneficiaryBankName").val();
        $scope.ssiTemplate.BeneficiaryBankAddress = $("#beneficiaryBankAddress").val();

        $scope.ssiTemplate.IntermediaryBankName = $("#intermediaryBankName").val();
        $scope.ssiTemplate.IntermediaryBankAddress = $("#intermediaryBankAddress").val();

        $scope.ssiTemplate.UltimateBeneficiaryBankName = $("#ultimateBankName").val();
        $scope.ssiTemplate.UltimateBeneficiaryBankAddress = $("#ultimateBankAddress").val();

        return $http.post("/SSITemplate/AddSsiTemplate", { ssiTemplate: $scope.ssiTemplate, accountType: $scope.accountType, broker: $scope.broker }).then(function (response) {

            ssiTemplateId = response.data;

            if (isNewTemplate) {
                notifySuccess("SSI template saved successfully");
                $timeout(function () {
                    window.location.href = "/SSITemplate/SSITemplate?ssiTemplateId=" + ssiTemplateId;
                }, 500);

            }
            $(".glyphicon-refresh").removeClass("icon-rotate");
        });
    }
    $scope.SaveSSITemplate = function (isValid) {

        if (!isValid) {
            if ($scope.ssiTemplateForm.$error.required == undefined)
                notifyError("FFC Name, FFC Number, Reference, Bank Name, Bank Address & Account Names can only contain ?:().,'+- characters");
            else {
                var message = "";
                angular.forEach($scope.ssiTemplateForm.$error.required, function (ele, ind) {
                    message += ele.$name + ", ";
                });
                notifyError("Please fill in the required fields " + message.substring(0, message.length - 2));
            }
            return;
        }

        var isSsiExits = false;
        var ssiName = $scope.ssiTemplate.TemplateName;
        $($scope.ssiTemplates).each(function (i, v) {
            if (ssiName == v.text && ssiTemplateId == 0) {
                isSsiExits = true;
            }
        });

        if (isSsiExits) {
            notifyWarning("SSI Template already exists. you are not able to add this ssi template");
            return;
        }
        if ($scope.ssiTemplate.UltimateBeneficiaryType == "Account Name" &&
            ($scope.ssiTemplate.UltimateBeneficiaryAccountName == null || $scope.ssiTemplate.UltimateBeneficiaryAccountName == "")) {
            notifyWarning("Account name field is required");
            return;
        }

        $(".glyphicon-refresh").addClass("icon-rotate");
        $scope.fnUpdateSSITemplate(true);
    }


    $scope.fnBack = function () {
        var searchText = getUrlParameter("searchText");
        searchText = (searchText == undefined || searchText == "undefined") ? "" : searchText;
        window.location.href = "/SSITemplate/Index?searchText=" + searchText;
    }

    //Update SSI Template Status
    $scope.fnUpdateSSITemplateStatus = function (ssiStatus, statusAction) {

        if (!$scope.ssiTemplateForm.$valid) {
            if ($scope.ssiTemplateForm.$error.required == undefined)
                notifyError("FFC Name, FFC Number, Reference, Bank Name, Bank Address & Account Names can only contain ?:().,'+- characters");
            else {
                var message = "";
                angular.forEach($scope.ssiTemplateForm.$error.required, function (ele, ind) {
                    message += ele.$name + ", ";
                });
                notifyError("Please fill in the required fields " + message.substring(0, message.length - 2));
            }
            return;
        }

        if ((statusAction == "Request for Approval" || statusAction == "Approve") && $scope.ssiTemplateDocuments.length == 0) {
            notifyWarning("Please upload document to approve/request to approve SSI template");
            return;
        }

        if (statusAction == "Approve" && ($scope.CallBackChecks == undefined || $scope.CallBackChecks.length == 0) && !$scope.IsBNYMBroker) {
            notifyWarning("Please add atleast one Callback check to approve SSI template");
            return;
        }

        $scope.SSITemplateStatus = angular.copy(ssiStatus);

        var confirmationMsg = "Are you sure you want to " + ((statusAction === "Request for Approval") ? "<b>request</b> for approval of" : "<b>" + (statusAction == "Revert" ? "save changes or sending approval for" : statusAction) + "</b>") + " the selected SSI Template?";
        if (statusAction == "Request for Approval") {
            //  $("#btnSaveCommentAgreements").addClass("btn-warning").removeClass("btn-success").removeClass("btn-info");
            $("#btnSaveCommentAgreements").html('<i class="glyphicon glyphicon-share-alt"></i>&nbsp;Request for approval');
        } else if (statusAction == "Approve") {
            if ($scope.IsKeyFieldsChanged && !$scope.IsBNYMBroker) {
                notifyWarning("Please add one Callback check to approve account");
                return;
            }
            $("#btnSaveCommentAgreements").removeClass("btn-warning").addClass("btn-success").removeClass("btn-primary");
            $("#btnSaveCommentAgreements").html('<i class="glyphicon glyphicon-ok"></i>&nbsp;Approve');
        } else if (statusAction == "Revert") {
            $("#btnSaveCommentAgreements").removeClass("btn-warning").removeClass("btn-success").addClass("btn-primary");
            $("#btnSaveCommentAgreements").html("<i class=\"glyphicon glyphicon-floppy-save\"></i>&nbsp;Save Changes");
            $("#btnSendApproval").show();
        }

        $("#pMsg").html(confirmationMsg);
        $("#updateSSITemplateModal").modal("show");
    }

    $scope.fnSaveSSITemplateStatus = function () {

        $q.all([$scope.fnUpdateSSITemplate(false)]).then(function () {
            $http.post("/SSITemplate/UpdateSsiTemplateStatus", { ssiTemplateStatus: $scope.SSITemplateStatus, ssiTemplateId: $scope.ssiTemplateId, comments: $("#ssiTemplateCommentTextArea").val().trim() }).then(function () {
                notifySuccess("SSI template " + $scope.SSITemplateStatus.toLowerCase() + " successfully");
                //notifySuccess("SSI template saved successfully");
                if ($scope.SSITemplateStatus == "Saved As Draft") {
                    $scope.ssiTemplate.SSITemplateStatus = angular.copy($scope.SSITemplateStatus);
                } else {
                    var searchText = getUrlParameter("searchText");
                    searchText = (searchText == undefined || searchText == "undefined") ? "" : searchText;
                    $timeout(function () {
                        window.location.href = "/SSITemplate/Index?searchText=" + searchText;
                    }, 500);

                }
            })
        });

        $("#updateSSITemplateModal").modal("hide");

    }

    $scope.ssiCallbackTable = null;

    $scope.viewCallbackTable = function (data) {

        if ($("#ssiCallbackTbl").hasClass("initialized")) {
            fnDestroyDataTable("#ssiCallbackTbl");
        }
        $scope.ssiCallbackTable = $("#ssiCallbackTbl").DataTable({
            aaData: data,
            "bDestroy": true,
            "columns": [
                { "mData": "onBoardingSSITemplateId", "sTitle": "onBoardingSSITemplateId", visible: false },
                { "mData": "hmsSSICallbackId", "sTitle": "hmsSSICallbackId", visible: false },
                {
                    "mData": "ContactName", "sTitle": "Contact Name"
                },
                {
                    "mData": "ContactNumber", "sTitle": "Contact Number"
                },
                {
                    "mData": "Title", "sTitle": "Title"
                },
                /*
                {
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

        $(document).on("click", ".btnCallbackConfirm", function (event) {
            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();
            var selectedRow = $(this).parents("tr");
            $scope.rowElement = $scope.ssiCallbackTable.row(selectedRow).data();
            $scope.tdEle = $(this).closest('td');
            $scope.tdEle.popover('destroy');
            $timeout(function () {
                angular.element($scope.tdEle).attr('title', 'Are you sure to confirm the call back?').popover('destroy').popover({
                    trigger: 'click',
                    title: "Are you sure to confirm the call back?",
                    placement: 'top',
                    container: 'body',
                    content: function () {
                        return "<div class=\"btn-group pull-right\" style='margin-bottom:7px;'>"
                            + "<button class=\"btn btn-sm btn-success confirmCallback\"><i class=\"glyphicon glyphicon-ok\"></i>&nbsp;Yes</button>"
                            + "&nbsp;&nbsp;<button class=\"btn btn-sm btn-default dismissCallback\"><i class=\"glyphicon glyphicon-remove\"></i>&nbsp;No</button>"
                            + "</div>";
                    },
                    html: true
                }).popover('show');
                $(".popover-content").html("<div class=\"btn-group pull-right\" style='margin-bottom:7px;'>"
                    + "<button class=\"btn btn-sm btn-success confirmCallback\"><i class=\"glyphicon glyphicon-ok\"></i></button>"
                    + "<button class=\"btn btn-sm btn-default dismissCallback\"><i class=\"glyphicon glyphicon-remove\"></i></button>"
                    + "</div>");

            }, 50);
        });

        $timeout(function () {
            $scope.ssiCallbackTable.columns.adjust().draw(true);
        }, 1000);

    }

    $scope.fnAddCallbackModal = function () {
        $scope.callback = { onBoardingSSITemplateId: angular.copy($scope.ssiTemplate.onBoardingSSITemplateId) };
        $("#callbackModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtContactName").popover("hide");
            $("#txtContactNumber").popover("hide");
        });
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
            url: "/SSITemplate/AddOrUpdateCallback",
            type: "json",
            data: JSON.stringify({
                callback: $scope.callback
            })
        }).then(function (response) {
            notifySuccess("SSI Call back added successfully");
            if ($scope.callback.hmsSSICallbackId == undefined)
                $scope.IsKeyFieldsChanged = false;
            $scope.fnGetSSICallbackData($scope.ssiTemplate.onBoardingSSITemplateId);
        });

        $("#callbackModal").modal("hide");
    }

    $scope.adjustContainer = function (isCallback) {
        $timeout(function () {
            if (isCallback) {
                if ($scope.ssiCallbackTable != undefined)
                    $scope.ssiCallbackTable.columns.adjust().draw(true);
            }
            //else {
            //    if ($scope.contactTable[index] != undefined)
            //        $scope.contactTable[index].columns.adjust().draw(true);
            //}
        }, 100);
    }

    $(document).on('click', ".confirmCallback", function () {
        angular.element($scope.tdEle).popover("destroy");
        $scope.IsCallBackChanged = true;
        $scope.rowElement.IsCallbackConfirmed = true;
        $http({
            method: "POST",
            url: "/SSITemplate/AddOrUpdateCallback",
            type: "json",
            data: JSON.stringify({
                callback: $scope.rowElement
            })
        }).then(function (response) {
            $scope.IsCallBackChanged = true;
            $scope.fnGetSSICallbackData($scope.ssiTemplate.onBoardingSSITemplateId);
            notifySuccess("SSI callback confirmed successfully");
        });
    });

    $(document).on('click', ".dismissCallback", function () {
        angular.element($scope.tdEle).popover("destroy");
    });

    $scope.fnGetSSICallbackData = function (ssiTemplateId) {
        $scope.IsCallBackChanged = true;
        $http.get("/SSITemplate/GetSSICallbackData?ssiTemplateId=" + ssiTemplateId).then(function (response) {
            $scope.ssiTemplate.hmsSSICallbacks = response.data;
            $scope.CallBackChecks = response.data;
            $scope.viewCallbackTable($scope.CallBackChecks, 0);
            $timeout(function () { $scope.IsCallBackChanged = false; }, 1000);
        });
    }

    $scope.fnSendApprovalSSITemplateStatus = function () {
        $scope.SSITemplateStatus = "Pending Approval";
        $scope.fnSaveSSITemplateStatus();
        $("#btnSendApproval").hide();
    }
    /* SSI Template Attachemnt */

    function toggleChevron(e) {
        $(e.target)
            .find("i.indicator")
            .toggleClass("glyphicon-chevron-down glyphicon-chevron-up");
        $("html, body").animate({ scrollTop: $(e.target).offset().top - 5 }, "slow");
    }
    $("#panelAttachment .panel-heading").on("click", function (e) {
        $(this).parent().find("div.collapse").collapse("toggle");
        toggleChevron(e);
    });

    $("#plnSsiTemplate .panel-heading").on("click", function (e) {
        $(this).parent().find("div.collapse").collapse("toggle");
        toggleChevron(e);
    });

    $("#panelAttachment .panel").css({
        "padding-top": "20px;"
    });

    $("#panelAttachment .panel-heading").css({
        "cursor": "pointer"
    });

    $("#uploadSSIFiles").dropzone({
        url: "/SSITemplate/UploadSsiTemplateFiles?ssiTemplateId=" + ssiTemplateId,
        dictDefaultMessage: "<span style='font-size:20px;font-weight:normal;font-style:italic'>Drag/Drop SSI documents here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".msg,.csv,.txt,.pdf,.xls,.xlsx,.zip,.rar", accept: validateDoubleExtensionInDZ,
        maxFiles: 6,
        previewTemplate: "<div class='row col-sm-2'><div class='panel panel-success panel-sm'> <div class='panel-heading'> <h3 class='panel-title' style='text-overflow: ellipsis;white-space: nowrap;overflow: hidden;'><span data-dz-name></span> - (<span data-dz-size></span>)</h3> " +
            "</div> <div class='panel-body'> <span class='dz-upload' data-dz-uploadprogress></span>" +
            "<div class='progress'><div data-dz-uploadprogress class='progress-bar progress-bar-warning progress-bar-striped active dzFileProgress' style='width: 0%'></div>" +
            "</div></div></div></div>",

        maxfilesexceeded: function (file) {
            this.removeAllFiles();
            this.addFile(file);
        },
        init: function () {
            var myDropZone = this;
            this.on("processing", function (file) {
                this.options.url = "/SSITemplate/UploadSsiTemplateFiles?ssiTemplateId=" + ssiTemplateId;
            });
        },
        processing: function (file, result) {
            $("#uploadFiles").animate({ "min-height": "140px" });
        },
        success: function (file, result) {
            if (ssiTemplateId == 0) {
                notifyWarning("Please save ssi template to upload documents");
                return;
            } else {
                $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
                $(".dzFileProgress").html("Upload Successful");
                $("#uploadFiles").animate({ "min-height": "80px" });
                var aDocument = result;
                $.each(aDocument.Documents, function (index, value) {
                    $scope.ssiTemplateDocuments.push(value);
                });

                $scope.fnConstructDocumentTable($scope.ssiTemplateDocuments);
            }
        },
        queuecomplete: function () {
        },
        complete: function (file, result) {
            if (ssiTemplateId != 0) {
                $("#uploadFiles").removeClass("dz-drag-hover");

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



    //Export a SSI Template
    //$scope.ExportSSITemplate = function () {
    //    window.location.href = "/OnBoarding/ExportDetailsFromClient?clientId=" + $scope.clientId;

    //}
    //$(".form-control").change(function () {
    //    if ($scope.ssiTemplate.SSITemplateStatus == "Pending Approval")
    //        $("#approve").hide();
    //});

});
