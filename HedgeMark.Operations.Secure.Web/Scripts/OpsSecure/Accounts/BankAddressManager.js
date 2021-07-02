HmOpsApp.controller("BankAddressController", function ($scope, $http, $timeout, $filter, $q) {
    var accountTable;

    $scope.ValidateAccountBICorABA = function () {

        if ($("#txtBICorABA").val() == undefined || $("#txtBICorABA").val() == "") {
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

    }

    $(document).on("click", "#accountTable tbody tr ", function () {
        //$scope.AddorEditText = "Edit";
        var rowElement = accountTable.row(this).data();
        $scope.onBoardingAccountBICorABAId = rowElement.onBoardingAccountBICorABAId;

        $("#accountTable tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $("#btnEdit").prop("disabled", false);
        $("#btnDel").prop("disabled", false);
    });

    $(document).on("dblclick", "#accountTable tbody tr", function () {
        //$scope.AddorEditText = "Edit";
        var rowElement = accountTable.row(this).data();
        $scope.fnEditAccountDetails(rowElement);
    });

    $scope.fnAddorEditBICorABA = function () {
        //$scope.ValidateAccountBICorABA();       
        $scope.SaveAccountBiCorAba();
    }

    $scope.fnAddAccountDetails = function (rowElement) {
        $scope.AddorEditText = "Add";
        $("#txtBICorABA").val("");
        $("#txtBankName").val("");
        $("#txtBankAddress").val("");
        $("#btnBICorABA").prop("checked", false).change();
        $("#beneficiaryABABICModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtBICorABA").popover("hide");
            $("#txtBankAddress").popover("hide");
            $("#txtBankName").popover("hide");
            });
        $scope.onBoardingAccountBICorABAId = 0;
    }
    $scope.fnEditAccount = function () {
        var rowElement = accountTable.row(".info").data();
        $scope.accountDetail = rowElement;
        $scope.fnEditAccountDetails();
    }

    $scope.fnEditAccountDetails = function () {
        $scope.AddorEditText = "Edit";
        $("#txtBICorABA").val($scope.accountDetail.BICorABA);
        $("#txtBankName").val($scope.accountDetail.BankName);
        $("#txtBankAddress").val($scope.accountDetail.BankAddress);
        $("#btnBICorABA").prop("checked", $scope.accountDetail.IsABA).change();
        $("#beneficiaryABABICModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtBICorABA").popover("hide");
            $("#txtBankAddress").popover("hide");
            $("#txtBankName").popover("hide");
            });
    }

    $scope.fnDeleteAccount = function () {
        showMessage("Are you sure do you want to delete account? ", "Delete Account", [
            {
                label: "Delete",
                className: "btn btn-sm btn-danger",
                callback: function () {
                    $http.post("/FundAccounts/DeleteAccountBiCorAba", { onBoardingAccountBICorABAId: $scope.onBoardingAccountBICorABAId }).then(function (response) {
                        if (response.data.isDeleted)
                            notifySuccess("BIC/ABA account deleted successfully");
                        else
                            notifyInfo("BIC/ABA associated to Live account, Can't be deleted");

                        accountTable.row(".info").remove().draw();
                        $scope.onBoardingAccountBICorABAId = 0;
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

    $scope.SaveAccountBiCorAba = function () {
        if ($("#txtBICorABA").val() == undefined || $("#txtBICorABA").val() == "") {
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
            onBoardingAccountBICorABAId: $scope.onBoardingAccountBICorABAId,
            BICorABA: $("#txtBICorABA").val().toUpperCase(),
            BankName: $("#txtBankName").val(),
            BankAddress: $("#txtBankAddress").val(),
            IsABA: $("#btnBICorABA").prop("checked")
        }
        $http.post("/FundAccounts/AddorEditAccountBiCorAba", { accountBiCorAba: $scope.accountBeneficiary }).then(function (response) {
            notifySuccess("Beneficiary BIC or ABA " + $scope.AddorEditText + "ed successfully");
            $scope.onBoardingAccountBICorABAId = 0;
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
        });
    }   


    $scope.fnGetBicorAba = function () {
        return $http.get("/FundAccounts/GetAllAccountBicorAba").then(function (response) {
            $scope.accountBicorAba = response.data.accountBicorAba;
            if ($("#accountTable").hasClass("initialized")) {
                accountTable.clear();
                accountTable.rows.add($scope.accountBicorAba);
                accountTable.draw();

            } else {
                accountTable = $("#accountTable").DataTable({
                    aaData: $scope.accountBicorAba,
                    pageResize: true,
                    rowId: "onBoardingAccountBICorABAId",
                    "bDestroy": true,
                    iDisplayLength: -1,
                    //fixedColumns: {
                    //    leftColumns: 4
                    //},
                    "autoWidth":true,
                    "order": [[1, "asc"]],
                    sScrollY: $scope.accountBicorAba.length > 10 ? (window.innerHeight - 300) : false,
                    "scrollX": true,
                    "columns": [
                        { "mData": "onBoardingAccountBICorABAId", visible: false },
                        {
                            "mData": "BICorABA",
                            "sTitle": "BICorABA"
                        },
                        {
                            "mData": "BankName",
                            "sTitle": "Bank Name"
                        },
                        {
                            "mData": "IsABA",
                            "sTitle": "Type",
                            "mRender": function (tdata, type, row) {
                                return tdata ? "ABA" : "BIC";
                            }
                        }, {
                            "mData": "CreatedBy",
                            "sTitle": "CreatedBy"
                        },
                        {
                            "mData": "CreatedAt",
                            "sTitle": "CreatedAt",
                            "mRender": renderDotNetDateAndTime
                        },
                        {
                            "mData": "UpdatedBy",
                            "sTitle": "UpdatedBy"
                        },
                        {
                            "mData": "UpdatedAt",
                            "sTitle": "UpdatedAt",
                            "mRender": renderDotNetDateAndTime
                        }


                    ],
                    "drawCallback": function (settings) {

                    },
                    "rowCallback": function (row, data, index) {

                    }

                });
            }
            });
    
    }
    $scope.fnExportAllAccountBICorABAlist = function () {
        window.location.assign("/FundAccounts/ExportAccountBICorABAlist");
    }

    $scope.fnGetBicorAba();
});