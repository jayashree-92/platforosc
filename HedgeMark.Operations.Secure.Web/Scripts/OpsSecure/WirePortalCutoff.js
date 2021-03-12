
$("#liWireCutOffs").addClass("active");

HmOpsApp.controller("WirePortalCutoffCtrl", function ($scope, $http, $timeout, $filter) {

    $scope.fnGetWirePortalCutoffs = function () {
        $http.get("/WirePortalCutoff/GetWirePortalCutOffData").then(function (response) {
            fnDestroyDataTable("#tblWirePortalCutoffData");
            $scope.wireportalCutOffData = response.data;
            $scope.cutOffTable = $("#tblWirePortalCutoffData").DataTable({
                "bDestroy": true,
                // responsive: true,
                aaData: response.data,
                "aoColumns": [
                    {
                        "sTitle": "Cash Instruction",
                        "mData": "WirePortalCutoff.CashInstruction"
                    },
                    {
                        "sTitle": "Currency",
                        "mData": "WirePortalCutoff.Currency",
                    },
                    {
                        "sTitle": "Time Zone",
                        "mData": "WirePortalCutoff.CutOffTimeZone",
                    },
                    {
                        "sTitle": "Cutoff Time",
                        "mData": "WirePortalCutoff.CutoffTime",
                        "mRender": function (tData, type, row) {
                            return moment(tData).format("hh:mm A");
                        }
                    },
                    {
                        "sTitle": "Days to wire",
                        "mData": "WirePortalCutoff.DaystoWire",
                    },
                    {
                        "sTitle": "Created By",
                        "mData": "RequestedBy",
                        "mRender": humanizeEmail
                    },
                    {
                        "sTitle": "Created At",
                        "mData": "WirePortalCutoff.RecCreatedAt",
                        "mRender": renderDotNetDateAndTime
                    }, {
                        "sTitle": "Is Approved",
                        "mData": "WirePortalCutoff.IsApproved",
                        "mRender": function (tdata, type, row) {

                            //if (row.ModifiedBy == "-")
                            //    return "-";
                            return tdata ? "<label class='label label-success'>Approved</label>" : "<label class='label label-warning'>Pending Approval</label>";
                        }
                    },
                    {
                        "sTitle": "Approved By",
                        "mData": "ApprovedBy",
                        "mRender": humanizeEmail
                    },
                    {
                        "sTitle": "Approved At",
                        "mData": "WirePortalCutoff.ApprovedAt",
                        "mRender": renderDotNetDateAndTime
                    }
                ], "createdRow": function (row, data) {
                    if (data.WirePortalCutoff.IsApproved)
                        $(row).addClass("success");
                    else
                        $(row).addClass("warning");
                },
                "deferRender": false,
                "bScrollCollapse": true,
                scroller: true,
                "sScrollX": "100%",
                "sScrollXInner": "100%",
                "scrollY": window.innerHeight - 400,
                "order": [[0, "asc"], [1, "asc"]],
                "oLanguage": {
                    "sSearch": "",
                    "sEmptyTable": "No Wire Portal Cut Offs available.",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ Wire Portal Cut Offs"
                }
            });
            $("html, body").animate({ scrollTop: $("#tblWirePortalCutoffData").offset().top }, "slow");

            $timeout(function () {
                $scope.cutOffTable.columns.adjust().draw();
                $scope.enableWireActions = false;
            }, 200);
        }, function (error) {
            notifyError(error.Message);
            $scope.enableWireActions = false;
        });
    }

    $scope.fnLoadWireCutOffRelatedDate = function () {
        
        $http.get("/WirePortalCutoff/GetCutoffRelatedData").then(function (response) {
            $scope.timeZones = response.data.timeZones;
            $scope.currencies = response.data.currencies;
            $scope.cashInstructions = response.data.cashInstructions;

            angular.element("#liCashInstruction").select2({
                placeholder: "Select a Cash Instruction",
                data: $scope.cashInstructions,
                closeOnSelect: false
            });

            angular.element("#liCurrency").select2({
                placeholder: "Select a Currency",
                data: $scope.currencies,
                closeOnSelect: false
            });

            angular.element("#liTimeZone").select2({
                placeholder: "Select a Time Zone",
                data: $scope.timeZones,
                closeOnSelect: false
            });

        });

    }

    $scope.fnLoadWireCutOffRelatedDate();
    $scope.fnGetWirePortalCutoffs();
    $scope.enableWireActions = false;
    $scope.IsWireCutOffLoading = false;
    $scope.ExistingWireCutOff = {};

    $scope.fnAddOrUpdateWirePortalCutoff = function (isAdd) {
        $scope.isAdd = isAdd;
        if (isAdd)
            $scope.wirePortalCutoff = angular.copy($scope.dummyCutoff);
        else {
            var date = new Date();
            $scope.selectedRowData.WirePortalCutoff.CutoffTime = new Date(date.getYear(), date.getMonth(), date.getDate(), $scope.selectedRowData.WirePortalCutoff.CutoffTime.Hours, $scope.selectedRowData.WirePortalCutoff.CutoffTime.Minutes, $scope.selectedRowData.WirePortalCutoff.CutoffTime.Seconds);
            $scope.wirePortalCutoff = angular.copy($scope.selectedRowData);
        }

        angular.element("#wirePortalCutoffModal").modal({ backdrop: 'static', keyboard: true }).on("shown.bs.modal", function () {
            $scope.IsWireCutOffLoading = true;
            $("#liCashInstruction").select2("val", $scope.wirePortalCutoff.WirePortalCutoff.CashInstruction);
            $("#liCurrency").select2("val", $scope.wirePortalCutoff.WirePortalCutoff.Currency);
            $("#liTimeZone").select2("val", $scope.wirePortalCutoff.WirePortalCutoff.CutOffTimeZone);
            $scope.ExistingWireCutOff = angular.copy($scope.wirePortalCutoff);

            $timeout(function () {
                $scope.IsWireCutOffLoading = false;
                $scope.fnChangeWireCutOffStatus();
            }, 50);
        });
        $timeout(function () {
            $scope.isWireCutoffRequirementsFilled = !$scope.isWireCutoffRequirementsFilled;
        }, 50);
    }

    $("#liTimeZone").on("change", function () { $scope.fnChangeWireCutOffStatus(); });

    $scope.IsChangeMade = false;
    $scope.IsSameUserRequested = false;
    $scope.IsWireApprover = false;
    $scope.fnChangeWireCutOffStatus = function () {
        $scope.IsChangeMade = false;
        if ($scope.IsWireCutOffLoading)
            return;

        if (moment($scope.ExistingWireCutOff.WirePortalCutoff.CutoffTime).format("hh:mm") == moment($scope.wirePortalCutoff.WirePortalCutoff.CutoffTime).format("hh:mm")
            && $scope.ExistingWireCutOff.WirePortalCutoff.DaystoWire == $("#daysToWire").val()
            && $scope.ExistingWireCutOff.WirePortalCutoff.CutOffTimeZone == $("#liTimeZone").select2("val")) {
            $scope.IsChangeMade = false;
        } else {
            $scope.IsChangeMade = true;
        }

        $scope.IsSameUserRequested = $("#userName").val() == $scope.ExistingWireCutOff.RequestedBy;
        $scope.IsWireApprover = $("#IsWireApprover").val() === "true";
    }

    $(document).on("click", "#tblWirePortalCutoffData tbody tr ", function () {
        $("#tblWirePortalCutoffData tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $scope.selectedRowData = $scope.cutOffTable.row(this).data();
        $timeout(function () {
            $scope.enableWireActions = true;
        }, 50);

    });

    $(document).on("dblclick", "#tblWirePortalCutoffData tbody tr", function () {
        $scope.selectedRowData = $scope.cutOffTable.row(this).data();
        $scope.fnAddOrUpdateWirePortalCutoff(false);
    });

    $(document).on("change", ".dropDown", function () {
        $timeout(function () {
            $scope.isWireCutoffRequirementsFilled = !$scope.isWireCutoffRequirementsFilled;
        }, 50);
    });

    $scope.dummyCutoff = {
        WirePortalCutoff: {
            hmsWirePortalCutoffId: 0,
            CashInstruction: null,
            Currency: null,
            Country: null,
            CutOffTimeZone: null,
            CutoffTime: new Date(1, 1, 2020, 0, 0, 0),
            DaystoWire: 0
        }
    }

    $scope.$watch("isWireCutoffRequirementsFilled", function (newValue, oldValue) {
        $scope.isWireCutoffRequirementsFilled = $("#liCashInstruction").select2('val') != "" && $("#liCurrency").select2('val') != "" && $("#liTimeZone").select2('val') != "";
    });

    $scope.fnSaveWirePortalCutoff = function () {
        var existingCutOff = $filter('filter')($scope.wireportalCutOffData, function (cutOff) {
            return cutOff.CashInstruction == $scope.wirePortalCutoff.WirePortalCutoff.CashInstruction && cutOff.Currency == $scope.wirePortalCutoff.WirePortalCutoff.Currency;
        }, true)[0];
        if (existingCutOff != undefined && $scope.isAdd) {
            notifyError("Cutoff data exists for selected Cash Instruction and Currency. Please select a new combination.")
            return;
        }
        var wirePortalCutoff = angular.copy($scope.wirePortalCutoff.WirePortalCutoff);
        wirePortalCutoff.CutoffTime = $("#cutoffTime").val();
        $http({
            method: "POST",
            url: "/WirePortalCutoff/SaveWirePortalCutoff",
            type: "json",
            data: JSON.stringify({
                wirePortalCutoff: wirePortalCutoff,
                shouldApprove: !$scope.IsChangeMade && !$scope.IsSameUserRequested
            })
        }).then(function (response) {
            notifySuccess("Wire Portal Cutoff " + ($scope.isAdd ? "added" : "updated") + " successfully");
            $scope.fnGetWirePortalCutoffs();
            angular.element("#wirePortalCutoffModal").modal('hide');
        },
            function (error) {
                notifyError("Changes failed to save with error :" + error.data);
            });
    }

    $scope.fnDeleteWireCutoff = function () {
        showMessage("Are you sure do you want to delete this cutoff? ", "Delete Wire Cutoff", [
            {
                label: "Delete",
                className: "btn btn-sm btn-danger",
                callback: function () {
                    $http.post("/WirePortalCutoff/DeleteWirePortalCutoff", { wireCutoffId: $scope.selectedRowData.WirePortalCutoff.hmsWirePortalCutoffId }).then(function () {
                        notifySuccess("Wire cutoff deleted successfully");
                        $scope.fnGetWirePortalCutoffs();
                    });
                }
            },
            {
                label: "Cancel",
                className: "btn btn-sm btn-default"
            }
        ]);
    }

    $scope.fnExportData = function () {
        window.location.assign("/WirePortalCutoff/ExportData");
    }

    
    $scope.fnAddCashInstructionModal = function () {
        $("#cashInstructionModal").modal({
            show: true,
            keyboard: true
        }).on("hidden.bs.modal", function () {
            $("#txtCashInstruction").popover("hide");
            // $("html, body").animate({ scrollTop: $scope.scrollPosition }, "fast");
        });
    }

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

        $http.post("/FundAccounts/AddCashInstruction", { cashInstruction: $("#txtCashInstruction").val() }).then(function (response) {
            notifySuccess("Cash instruction mechanism added successfully");
            $scope.fnLoadWireCutOffRelatedDate();
        });

        $("#cashInstructionModal").modal("hide");
    }

});

