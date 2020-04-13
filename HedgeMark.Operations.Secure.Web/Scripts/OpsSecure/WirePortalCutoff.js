
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
                        "mData": "CashInstruction"
                    },
                    {
                        "sTitle": "Currency",
                        "mData": "Currency",
                    },
                    {
                        "sTitle": "Time Zone",
                        "mData": "CutOffTimeZone",
                    },
                    {
                        "sTitle": "Cutoff Time",
                        "mData": "CutoffTime",
                        "mRender": function (tData, type, row) {
                            return moment(tData).format("hh:mm A");
                        }
                    },
                    {
                        "sTitle": "Days to wire",
                        "mData": "DaystoWire",
                    },
                    {
                        "sTitle": "Created At",
                        "mData": "RecCreatedAt",
                        "mRender": renderDotNetDateAndTime
                    }
                ],
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

    $scope.fnGetWirePortalCutoffs();
    $scope.enableWireActions = false;

    $scope.fnAddOrUpdateWirePortalCutoff = function (isAdd) {
        $scope.isAdd = isAdd;
        if (isAdd)
            $scope.wirePortalCutoff = angular.copy($scope.dummyCutoff);
        else {
            var date = new Date();
            $scope.selectedRowData.CutoffTime = new Date(date.getYear(), date.getMonth(), date.getDate(), $scope.selectedRowData.CutoffTime.Hours, $scope.selectedRowData.CutoffTime.Minutes, $scope.selectedRowData.CutoffTime.Seconds);
            $scope.wirePortalCutoff = angular.copy($scope.selectedRowData);
        }
        angular.element("#wirePortalCutoffModal").modal({ backdrop: 'static', keyboard: true }).on("shown.bs.modal", function () {
            $("#liCashInstruction").select2("val", $scope.wirePortalCutoff.CashInstruction);
            $("#liCurrency").select2("val", $scope.wirePortalCutoff.Currency);
            $("#liTimeZone").select2("val", $scope.wirePortalCutoff.CutOffTimeZone);
        });
        $timeout(function () {
            $scope.isWireCutoffRequirementsFilled = !$scope.isWireCutoffRequirementsFilled;
        }, 50);
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
        onBoardingWirePortalCutoffId: 0,
        CashInstruction: null,
        Currency: null,
        Country: null,
        CutOffTimeZone: null,
        CutoffTime: new Date(1, 1, 2020, 0, 0, 0),
        DaystoWire: 0,
    }

    $scope.$watch("isWireCutoffRequirementsFilled", function (newValue, oldValue) {
        $scope.isWireCutoffRequirementsFilled = $("#liCashInstruction").select2('val') != "" && $("#liCurrency").select2('val') != "" && $("#liTimeZone").select2('val') != "";
    });

    $scope.fnSaveWirePortalCutoff = function () {
        var existingCutOff = $filter('filter')($scope.wireportalCutOffData, function (cutOff) {
            return cutOff.CashInstruction == $scope.wirePortalCutoff.CashInstruction && cutOff.Currency == $scope.wirePortalCutoff.Currency;
        }, true)[0];
        if (existingCutOff != undefined && $scope.isAdd) {
            notifyError("Cutoff data exists for selected Cash Instruction and Currency. Please select a new combination.")
            return;
        }
        var wirePortalCutoff = angular.copy($scope.wirePortalCutoff);
        wirePortalCutoff.CutoffTime = $("#cutoffTime").val();
        $http({
            method: "POST",
            url: "/WirePortalCutoff/SaveWirePortalCutoff",
            type: "json",
            data: JSON.stringify({
                wirePortalCutoff: wirePortalCutoff
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
                    $http.post("/WirePortalCutoff/DeleteWirePortalCutoff", { wireCutoffId: $scope.selectedRowData.onBoardingWirePortalCutoffId }).then(function () {
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

});

