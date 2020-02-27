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
    $scope.SSITemplateTypeData = [{ id: "Broker", text: "Broker" }, { id: "Fee/Expense Payment", text: "Fee/Expense Payment" }];
    $scope.messageTypes = [{ id: "MT103", text: "MT103" }, { id: "MT202", text: "MT202" }, { id: "MT202 COV", text: "MT202 COV" }];
    // { id: "MT210", text: "MT210" }, { id: "MT540", text: "MT540" },{ id: "MT542", text: "MT542" }
    $scope.ssiTemplate.TemplateName = "";
    $scope.ssiTemplateDocuments = [];
    var ssiDocumentTable;
    var documentData = "\"FileName\": \"\",\"RecCreatedBy\": \"\",\"RecCreatedAt\": \"\"";
    $scope.beneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }];
    $scope.ultimateBeneficiaryType = [{ id: "BIC", text: "BIC" }, { id: "ABA", text: "ABA" }, { id: "Account Name", text: "Account Name" }];


    function viewAttachmentTable(data) {

        if ($("#documentTable").hasClass("initialized")) {
            fnDestroyDataTable("#documentTable");
        }
        ssiDocumentTable = $("#documentTable").not(".initialized").addClass("initialized").DataTable({
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
                    $("td:eq(0)", nRow).html("<a title ='click to download the file' href='/Accounts/DownloadSsiTemplateFile?fileName=" + getFormattedFileName(aData.FileName) + "&ssiTemplateId=" + ssiTemplateId + "'>" + aData.FileName + "</a>");
                }
            },
            "scrollY": $("#Attachment").offset().top + 300,
            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No files are available for the ssi templates",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Files"
            }
        });
        $timeout(function () {
            $("#documentTable").dataTable().fnAdjustColumnSizing();
            $scope.ssiTemplate.onBoardingSSITemplateDocuments = angular.copy(data);
        }, 100);
        $("#documentTable tbody td:last-child button").on("click", function (event) {
            event.preventDefault();
            var selectedRow = $(this).parents("tr");
            var rowElement = ssiDocumentTable.row(selectedRow).data();
            bootbox.confirm("Are you sure you want to remove this document from ssi template?", function (result) {
                if (!result) {
                    return;
                } else {
                    $http.post("/Accounts/RemoveSsiTemplateDocument", { fileName: rowElement.FileName, documentId: rowElement.onBoardingSSITemplateDocumentId }).then(function () {
                        ssiDocumentTable.row(selectedRow).remove().draw();
                        $scope.ssiTemplateDocuments.pop(rowElement);
                        notifySuccess("Document removed succesfully");
                    });
                }
            });

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

                accountBicorAbaData = $filter('orderBy')(accountBicorAbaData, 'text');

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

                intermediaryBicorAbaData = $filter('orderBy')(intermediaryBicorAbaData, 'text');

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

                ultimateBicorAbaData = $filter('orderBy')(ultimateBicorAbaData, 'text');

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
        $http.get("/Accounts/GetAllCurrencies").then(function (response) {
            $scope.currencies = response.data.currencies;

            $("#liCurrency").select2({
                placeholder: "Select a Currency",
                allowClear: true,
                data: response.data.currencies
            });

            if ($scope.ssiTemplate.Currency != null && $scope.ssiTemplate.Currency != 'undefined')
                $("#liCurrency").select2("val", $scope.ssiTemplate.Currency);
        });
    }

    $scope.fnGetBicorAba = function (isNew) {
        $http.get("/Accounts/GetAllAccountBicorAba").then(function (response) {
            $scope.accountBicorAba = response.data.accountBicorAba;


            if (isNew) {
                var isAba = $scope.isBicorAba == true ? "ABA" : "BIC";
                $scope.fnToggleBeneficiaryBICorABA(isAba, 'Beneficiary');
                $scope.ssiTemplate.BeneficiaryBICorABA = isAba;
                $("#liBeneficiaryBICorABA").select2("val", isAba);
            } else {
                if (ssiTemplateId !== 0 && ssiTemplateId !== "0") {
                    $scope.fnToggleBeneficiaryBICorABA($scope.ssiTemplate.BeneficiaryType, 'Beneficiary');
                    $scope.fnToggleBeneficiaryBICorABA($scope.ssiTemplate.IntermediaryType, 'Intermediary');
                    $scope.fnToggleBeneficiaryBICorABA($scope.ssiTemplate.UltimateBeneficiaryType, 'UltimateBeneficiary');
                }
            }
        });
    }

    $scope.fnBrokerList = function () {
        $http.get("/Accounts/GetSsiTemplatePreloadData").then(function (response) {
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

    if (ssiTemplateId !== 0 && ssiTemplateId !== "0") {
        $http.get("/Accounts/GetSsiTemplate?templateId=" + ssiTemplateId).then(function (response) {
            $scope.ssiTemplateId = ssiTemplateId;
            $scope.fnBrokerList();
            $scope.isAuthorizedUserToApprove = response.data.isAuthorizedUserToApprove;
            // $scope.fnPaymentOrReceiptReason();
            $scope.ssiTemplate = response.data.OnBoardingSsiTemplate;
            $scope.serviceProvider = $scope.ssiTemplate.ServiceProvider;
            $scope.reasonDetail = $scope.ssiTemplate.ReasonDetail;
            // $scope.SSITemplateType = $scope.ssiTemplate.SSITemplateType;
            $scope.ssiTemplate.CreatedAt = moment($scope.ssiTemplate.CreatedAt).format("YYYY-MM-DD HH:mm:ss");
            if ($scope.ssiTemplate.SSITemplateType == "Broker") {
                var templateList = $scope.ssiTemplate.TemplateName.split('-');
                $scope.broker = templateList[0].trim();
                $scope.accountType = templateList[1].trim();
                $scope.currency = templateList[2].trim();
                $scope.reasonDetail = templateList[3].trim();

            }
            if ($scope.reasonDetail == "Other") {
                $("#otherReason").show();
            } else
                $("#otherReason").hide();

            $scope.ssiTemplateDocuments = response.data.document;
            if ($scope.ssiTemplateDocuments == null && $scope.ssiTemplateDocuments.length <= 0) {
                $scope.ssiTemplateDocuments = JSON.parse("{" + documentData + "}");
            }

            viewAttachmentTable($scope.ssiTemplateDocuments);

            if ($scope.ssiTemplateDocuments.length > 0 && $scope.ssiTemplate.SSITemplateStatus == "Approved") {
                $(".dz-hidden-input").prop("disabled", true);
            } else {
                $(".dz-hidden-input").prop("disabled", false);
            }

            $timeout(function () {
                $scope.watchSSITemplate = $scope.ssiTemplate;
            }, 3000);
        });

    } else {
        $scope.ssiTemplateId = ssiTemplateId;
        $scope.fnBrokerList();
        viewAttachmentTable(JSON.parse("{" + documentData + "}"));
    }

    $scope.$watch('watchSSITemplate', function (val, oldVal) {

        if (val == undefined && oldVal == undefined)
            $scope.isSSITemplateChanged = true;

        else if (val.SSITemplateStatus != "Approved" || (oldVal != undefined && val != oldVal)) {
            $scope.isSSITemplateChanged = true;
        }
        else {
            $scope.isSSITemplateChanged = false;
        }

    }, true);

    $scope.fnSSITemplateType = function (templateType) {
        $scope.SSITemplateType = templateType;
        if (templateType != "Broker")
            $scope.fnLoadServiceProvider();
        $scope.fnPaymentOrReceiptReason();
    }

    $scope.fnLoadServiceProvider = function () {
        return $http.get("/Accounts/GetAllServiceProviderList").then(function (response) {
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
            $http.get("/Accounts/PaymentOrReceiptReasonDetails?templateType=" + $("#liSSITemplateType").val() + "&agreementTypeId=" + $("#liAccountType").val() + "&serviceProviderName=" + encodeURIComponent($("#liServiceProvider").val())).then(function (response) {
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

    $scope.fnAddSSITemplateDetail = function (panelIndex) {
        if ($('#txtDescription').val() == undefined || $('#txtDescription').val() == "") {
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
            isExists = $filter('filter')($scope.SSIPaymentReasons, { 'text': $("#txtDescription").val().trim() }, true).length > 0;
        }
        else {
            isExists = $filter('filter')($scope.serviceProviders, { 'text': $("#txtDescription").val().trim() }, true).length > 0;
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
            $http.post("/Accounts/AddPaymentOrReceiptReasonDetails", { reason: $('#txtDescription').val(), templateType: $('#liSSITemplateType').val(), agreementTypeId: $("#liAccountType").val(), serviceProviderName: $("#liServiceProvider").val() }).then(function (response) {
                notifySuccess("Reason added successfully");
                $scope.fnPaymentOrReceiptReason();
                $("#liReasonDetail").val($("#txtDescription").val());
                //$scope.onBoardingAccountDetails[panelIndex].Description = $('#txtDescription').val();  
            });
        }
        else {
            $http.post("/Accounts/AddServiceProvider", { serviceProviderName: $('#txtDescription').val() }).then(function (response) {
                notifySuccess("Service Provider added successfully");
                $scope.fnLoadServiceProvider().then(function () {
                    var provider = $filter('filter')($scope.serviceProviders, { 'text': $('#txtDescription').val() }, true)[0];
                    $("#liServiceProvider").select2('val', provider.id).trigger('change');
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
        $('#ssiTemplateDetailModal').modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtDescription").popover("hide").val('');
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

        $http.post("/Accounts/AddCurrency", { currency: $("#txtCurrency").val() }).then(function (response) {
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


        $http.post("/Accounts/AddAccountBiCorAba", { accountBiCorAba: $scope.accountBeneficiary }).then(function (response) {
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

        return $http.post("/Accounts/AddSsiTemplate", { ssiTemplate: $scope.ssiTemplate, accountType: $scope.accountType, broker: $scope.broker }).then(function (response) {

            //window.location.href = "/OnBoarding/SSITemplateList";
            ssiTemplateId = response.data;

            if (isNewTemplate) {
                notifySuccess("SSI template saved successfully");
                window.location.href = "/Accounts/SSITemplate?ssiTemplateId=" + ssiTemplateId;
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
        searchText = (searchText == undefined || searchText == 'undefined') ? "" : searchText;
        window.location.href = "/Accounts/SSITemplateList?searchText=" + searchText;
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

        $scope.SSITemplateStatus = angular.copy(ssiStatus);

        if ((statusAction == "Request for Approval" || statusAction == "Approve") && $scope.ssiTemplateDocuments.length == 0) {
            notifyWarning("Please upload document to approve ssi template");
            return;
        }

        var confirmationMsg = "Are you sure you want to " + ((statusAction === "Request for Approval") ? "<b>request</b> for approval of" : "<b>" + (statusAction == "Revert" ? "save changes or sending approval for" : statusAction) + "</b>") + " the selected SSI Template?";
        if (statusAction == "Request for Approval") {
            //  $("#btnSaveCommentAgreements").addClass("btn-warning").removeClass("btn-success").removeClass("btn-info");
            $("#btnSaveCommentAgreements").html('<i class="glyphicon glyphicon-share-alt"></i>&nbsp;Request for approval');
        } else if (statusAction == "Approve") {
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
            $http.post("/Accounts/UpdateSsiTemplateStatus", { ssiTemplateStatus: $scope.SSITemplateStatus, ssiTemplateId: $scope.ssiTemplateId, comments: $("#ssiTemplateCommentTextArea").val().trim() }).then(function () {
                notifySuccess("SSI template " + $scope.SSITemplateStatus.toLowerCase() + " successfully");
                //notifySuccess("SSI template saved successfully");
                if ($scope.SSITemplateStatus == "Saved As Draft") {
                    $scope.ssiTemplate.SSITemplateStatus = angular.copy($scope.SSITemplateStatus);
                } else {
                    var searchText = getUrlParameter("searchText");
                    searchText = (searchText == undefined || searchText == 'undefined') ? "" : searchText;
                    window.location.href = "/Accounts/SSITemplateList?searchText=" + searchText;
                }
            })
        });

        $("#updateSSITemplateModal").modal("hide");

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
    $("#Attachment .panel-heading").on("click", function (e) {
        $(this).parent().find("div.collapse").collapse("toggle");
        toggleChevron(e);
    });

    $("#plnSsiTemplate .panel-heading").on("click", function (e) {
        $(this).parent().find("div.collapse").collapse("toggle");
        toggleChevron(e);
    });

    $("#Attachment .panel").css({
        "padding-top": "20px;"
    });

    $("#Attachment .panel-heading").css({
        "cursor": "pointer"
    });

    $("#uploadSSIFiles").dropzone({
        url: "/Accounts/UploadSsiTemplateFiles?ssiTemplateId=" + ssiTemplateId,
        dictDefaultMessage: "<span style='font-size:20px;font-weight:normal;font-style:italic'>Drag/Drop SSI documents here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".msg,.csv,.txt,.pdf,.xls,.xlsx,.zip,.rar",
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
                this.options.url = "/Accounts/UploadSsiTemplateFiles?ssiTemplateId=" + ssiTemplateId;
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

                viewAttachmentTable($scope.ssiTemplateDocuments);
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
    $(".form-control").change(function () {
        if ($scope.ssiTemplate.SSITemplateStatus == 'Pending Approval')
            $("#approve").hide();
    });

});
