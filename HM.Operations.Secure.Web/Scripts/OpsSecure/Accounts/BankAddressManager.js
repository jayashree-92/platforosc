$("#liABAOrBIC").addClass("active");
HmOpsApp.controller("BankAddressController", function ($scope, $http, $timeout, $filter, $q) {
    var accountTable, accountBICorABATable;
    $scope.AddorEditAccountText = "Add";
    $scope.AddorEditText = "Add";
    $(document).on("click", "#accountTable tbody tr ", function () {
        $scope.AddorEditAccountText = "Edit";
        var rowElement = accountTable.row(this).data();
        $scope.hmsBankAccountAddressId = rowElement.hmsBankAccountAddressId;

        $("#accountTable tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $("#btnEditAccount").prop("disabled", false);
        $("#btnDelAccount").prop("disabled", false);
    });

    $(document).on("dblclick", "#accountTable tbody tr", function () {
        $scope.AddorEditAccountText = "Edit";
        var rowElement = accountTable.row(this).data();
        $scope.accountDetail = rowElement;
        $scope.fnEditAccountDetails(rowElement);
    });
    $scope.fnAddAccountDetails = function (rowElement) {
        $scope.AddorEditAccountText = "Add";
        $("#txtAccountBankName").val("");
        $("#txtAccountBankAddress").val("");
        $("#beneficiaryAccountModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtAccountBankAddress").popover("hide");
            $("#txtAccountBankName").popover("hide");
        });
        $scope.hmsBankAccountAddressId = 0;
    }

    $scope.fnEditAccount = function () {
        $scope.AddorEditAccountText = "Edit";
        var rowElement = accountTable.row(".info").data();
        $scope.accountDetail = rowElement;
        $scope.fnEditAccountDetails();
    }


    $scope.fnEditAccountDetails = function () {
        $timeout(function () {
            $scope.AddorEditAccountText = "Edit";
        }, 100);
        $("#txtAccountBankName").val($scope.accountDetail.AccountName);
        $("#txtAccountBankAddress").val($scope.accountDetail.AccountAddress);
        $("#beneficiaryAccountModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtAccountBankAddress").popover("hide");
            $("#txtAccountBankName").popover("hide");
        });
    }

    $scope.fnDeleteAccount = function () {
        showMessage("Are you sure do you want to delete account? ", "Delete Account", [
            {
                label: "Delete",
                className: "btn btn-sm btn-danger",
                callback: function () {
                    $http.post("/FundAccounts/DeleteAccountAddress", { hmsBankAccountAddressId: $scope.hmsBankAccountAddressId }).then(function (response) {
                        if (response.data.isDeleted)
                            notifySuccess("Bank Address deleted successfully");
                        else
                            notifyInfo("Bank account associated to Live account, Can't be deleted");

                        accountTable.row(".info").remove().draw();
                        $scope.hmsBankAccountAddressId = 0;
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

    $scope.SaveAccount = function () {
        

        if ($("#txtAccountBankName").val() == undefined || $("#txtAccountBankName").val() == "") {
            //pop-up    
            $("#txtAccountBankName").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Bank name cannot be empty. Please add a valid bank name",
                html: true,
                width: "250px"
            });

            $("#txtAccountBankName").popover("show");
            return;
        }

        $("#txtAccountBankName").popover("hide");

        /*if ($("#txtAccountBankAddress").val() == undefined || $("#txtAccountBankAddress").val() == "") {
            //pop-up    
            $("#txtAccountBankAddress").popover({
                placement: "right",
                trigger: "manual",
                container: "body",
                content: "Bank Address cannot be empty. Please add a valid Bank Address",
                html: true,
                width: "250px"
            });

            $("#txtAccountBankAddress").popover("show");
            return;
        }*/

        $("#txtAccountBankAddress").popover("hide");
        $scope.accountAddress = {
            hmsBankAccountAddressId: $scope.hmsBankAccountAddressId,
            AccountName: $("#txtAccountBankName").val(),
            AccountAddress: $("#txtAccountBankAddress").val(),
        }
        $http.post("/FundAccounts/AddorEditBankAccountAddress", { accountAddress: $scope.accountAddress }).then(function (response) {
            notifySuccess("Bank Address " + $scope.AddorEditText + "ed successfully");
            $scope.hmsBankAccountAddressId = 0;
            $scope.fnGetAccountList();
            $("#txtAccountBankName").val("");
            $("#txtAccountBankAddress").val("");
        });

        $("#beneficiaryAccountModal").modal("hide");
    }

    $scope.fnGetAccountList = function () {
        return $http.get("/FundAccounts/GetAllBankAccountAddress").then(function (response) {
            $scope.accountList = response.data.addressList;
            if ($("#accountTable").hasClass("initialized")) {
                accountTable.clear();
                accountTable.rows.add($scope.accountList);
                accountTable.draw();

            } else {
                accountTable = $("#accountTable").DataTable({
                    aaData: $scope.accountList,
                    pageResize: true,
                    rowId: "hmsBankAccountAddressId",
                    "bDestroy": true,
                    iDisplayLength: -1,
                    //fixedColumns: {
                    //    leftColumns: 4
                    //},
                    //"autoWidth": true,
                    "order": [[1, "asc"]],
                    sScrollY: $scope.accountList.length > 10 ? (window.innerHeight - 300) : false,
                    "scrollX": true,
                    "columns": [
                        { "mData": "hmsBankAccountAddressId", visible: false },
                        
                        {
                            "mData": "AccountName",
                            "sTitle": "Account Name"
                        }, {
                            "mData": "AccountAddress",
                            "sTitle": "Account Address"
                        },
                         {
                            "mData": "CreatedBy",
                            "sTitle": "Created By"
                        },
                        {
                            "mData": "CreatedAt",
                            "sTitle": "Created At",
                            "mRender": renderDotNetDateAndTime
                        },
                        {
                            "mData": "UpdatedBy",
                            "sTitle": "Updated By"
                        },
                        {
                            "mData": "UpdatedAt",
                            "sTitle": "Updated At",
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
        $timeout(function () {
            accountTable.columns.adjust().draw(true);
        }, 50);


    }
    $scope.fnExportAllAccountlist = function () {
        window.location.assign("/FundAccounts/ExportBankAccountlist");
    }

    $scope.fnGetAccountList();

    $(document).on("click", "#accountBICorABATable tbody tr ", function () {
        $scope.AddorEditText = "Edit";
        var rowElement = accountBICorABATable.row(this).data();
        $scope.onBoardingAccountBICorABAId = rowElement.onBoardingAccountBICorABAId;

        $("#accountBICorABATable tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $("#btnEdit").prop("disabled", false);
        $("#btnDel").prop("disabled", false);
    });

    $(document).on("dblclick", "#accountBICorABATable tbody tr", function () {
        $scope.AddorEditText = "Edit";
        var rowElement = accountBICorABATable.row(this).data();
        $scope.accountDetail = rowElement;
        $scope.fnEditBICorABAAccountDetails(rowElement);
    });

   

    $scope.fnAddAccountBICorABADetails = function (rowElement) {
        $scope.AddorEditText = "Add";
        $scope.BICorABAText =  "BIC";
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
    $scope.fnEditBICorABAAccount = function () {
        var rowElement = accountBICorABATable.row(".info").data();
        $scope.accountDetail = rowElement;
        $scope.fnEditBICorABAAccountDetails();
    }

    $scope.fnEditBICorABAAccountDetails = function () {
        $scope.AddorEditText = "Edit";
        $scope.BICorABAText = $scope.accountDetail.IsABA ? "ABA" : "BIC";
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

    $scope.fnDeleteBICorABAAccount = function () {
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
        if ($scope.AddorEditText == "Add") {
            $($scope.accountBicorAba).each(function (i, v) {
                if ($("#txtBICorABA").val() == v.BICorABA) {
                    isExists = true;
                    return false;
                }
            });
        }

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
    angular.element("#txtAccountBankName").on("focusin", function () { angular.element("#txtAccountBankName").popover("hide"); });
    angular.element("#txtAccountBankAddress").on("focusin", function () { angular.element("#txtAccountBankAddress").popover("hide"); });
       

    $("#btnBICorABA").on('change', function () {
        $timeout(function () {
            $scope.BICorABAText = $("#btnBICorABA").prop("checked") ? "ABA" : "BIC";
        }, 50);
    });

    $scope.fnGetBicorAba = function () {
        return $http.get("/FundAccounts/GetAllAccountBicorAba").then(function (response) {
            $scope.accountBicorAba = response.data.accountBicorAba;
            if ($("#accountBICorABATable").hasClass("initialized")) {
                accountBICorABATable.clear();
                accountBICorABATable.rows.add($scope.accountBicorAba);
                accountBICorABATable.draw();

            } else {
                accountBICorABATable = $("#accountBICorABATable").DataTable({
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
                            "mData": "IsABA",
                            "sTitle": "Type",
                            "mRender": function (tdata, type, row) {
                                return tdata ? "ABA" : "BIC";
                            }
                        },
                        {
                            "mData": "BICorABA",
                            "sTitle": "BICorABA"
                        },
                        {
                            "mData": "BankName",
                            "sTitle": "Bank Name"
                        },                       
                        {
                            "mData": "BankAddress",
                            "sTitle": "Bank Address"
                        },{
                            "mData": "CreatedBy",
                            "sTitle": "Created By"
                        },
                        {
                            "mData": "CreatedAt",
                            "sTitle": "Created At",
                            "mRender": renderDotNetDateAndTime
                        },
                        {
                            "mData": "UpdatedBy",
                            "sTitle": "Updated By"
                        },
                        {
                            "mData": "UpdatedAt",
                            "sTitle": "Updated At",
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
        $timeout(function () {
           accountBICorABATable.columns.adjust().draw(false);
        }, 100);
    
    }
    $scope.fnExportAllAccountBICorABAlist = function () {
        window.location.assign("/FundAccounts/ExportAccountBICorABAlist");
    }

    $scope.fnGetBicorAba();


});