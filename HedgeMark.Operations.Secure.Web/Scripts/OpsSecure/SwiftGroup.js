
$("#liSwiftGroups").addClass("active");

HmOpsApp.controller("SwiftGroupCtrl", function ($scope, $http, $timeout, $filter) {

    $scope.fnGetSwiftGroupData = function () {
        $http.get("/SwiftGroup/GetSwiftGroupData").then(function (response) {
            fnDestroyDataTable("#tblSwiftGroupData");
            $scope.swiftGroupData = response.data.swiftGroupData;
            $scope.brokerLegalEntityData = response.data.brokerLegalEntityData;
            $scope.swiftGroupStatusData = response.data.swiftGroupStatusData;
            $scope.wireMessageTypes = response.data.wireMessageTypes;

            $scope.swiftGroupTable = $("#tblSwiftGroupData").DataTable({
                "bDestroy": true,
                // responsive: true,
                aaData: $scope.swiftGroupData,
                "aoColumns": [
                    {
                        "sTitle": "Swift Group",
                        "mData": "SwiftGroup.SwiftGroup"
                    },
                    {
                        "sTitle": "Sender's BIC",
                        "mData": "SwiftGroup.SendersBIC",
                    },
                    {
                        "sTitle": "Broker",
                        "mData": "Broker",
                    },
                    {
                        "sTitle": "Status",
                        "mData": "SwiftGroupStatus",
                    },
                    {
                        "sTitle": "Swift Messages",
                        "mData": "SwiftGroup.AcceptedMessages",
                        "render": function (tdata) {
                            return tdata != undefined ? tdata.replace(/,/g, ", ") : tdata;
                        }
                    },
                    {
                        "sTitle": "Notes",
                        "mData": "SwiftGroup.Notes",
                    },
                    {
                        "sTitle": "Created By",
                        "mData": "SwiftGroup.RecCreatedBy",
                    },
                    {
                        "sTitle": "Created At",
                        "mData": "SwiftGroup.RecCreatedAt",
                        "mRender": renderDotNetDateAndTime
                    }
                ],
                "deferRender": false,
                "bScrollCollapse": true,
                scroller: true,
                "sScrollX": "100%",
                "sScrollXInner": "100%",
                "scrollY": window.innerHeight - 400,
                "order": [[7, "desc"]],
                "oLanguage": {
                    "sSearch": "",
                    "sEmptyTable": "No Swift Group Data available.",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ Swift Groups"
                }
            });
            $("html, body").animate({ scrollTop: $("#tblSwiftGroupData").offset().top }, "slow");

            angular.element("#liSwiftGroupStatus").select2({
                placeholder: "Select a Status",
                data: $scope.swiftGroupStatusData,
                closeOnSelect: false
            });

            angular.element("#liBrokerEntity").select2({
                placeholder: "Select a Broker",
                data: $scope.brokerLegalEntityData,
                closeOnSelect: false
            });
            angular.element("#liMessageTypes").select2({
                placeholder: "Select a Broker",
                data: $scope.wireMessageTypes,
                multiple: true,
                closeOnSelect: false,
                formatResult: formatSelection,
                formatSelection: formatSelection
            });
            $scope.dummySwiftGroup = {
                SwiftGroup: {
                    hmsSwiftGroupId: 0,
                    SwiftGroup: null,
                    SendersBIC: null,
                    AcceptedMessages: [],
                    Notes: null,
                    BrokerLegalEntityId: $scope.brokerLegalEntityData[0].id,
                    SwiftGroupStatusId: $scope.swiftGroupStatusData[0].id,
                }
            }

            $timeout(function () {
                $scope.swiftGroupTable.columns.adjust().draw();
                $scope.enableSwiftGroupActions = false;
            }, 200);
        }, function (error) {
            notifyError(error.Message);
            $scope.enableSwiftGroupActions = false;
        });
    }

    $scope.fnGetSwiftGroupData();
    $scope.enableSwiftGroupActions = false;
    function formatSelection(selectData) {
        var stat = $filter('filter')(angular.copy($scope.wireMessageTypes), { 'id': selectData.id }, true)[0];
        return stat.text + "<label class='pull-right label " + (selectData.isOutBound ? "label-info" : "label-default") + " shadowBox'>" + (selectData.isOutBound ? "OutBound" : "InBound") + "</label>";
    }


    //$scope.formatSwiftGroup = function () {
    //    if ($scope.swiftGroup.SwiftGroup.SendersBIC == null || $scope.swiftGroup.SwiftGroup.SendersBIC == "") {
    //        $scope.isSwiftGroupRequirementsFilled = !$scope.isSwiftGroupRequirementsFilled;
    //        return;
    //    }
    //    $scope.swiftGroup.SwiftGroup.SendersBIC = $scope.swiftGroup.SwiftGroup.SendersBIC.trim().toUpperCase();
    //    if ($scope.swiftGroup.SwiftGroup.SendersBIC.length < 8) {
    //        $scope.swiftGroup.SwiftGroup.SendersBIC = "";
    //        notifyError("Sender's BIC should contain minimum 8 characters");
    //    }
    //    $scope.isSwiftGroupRequirementsFilled = !$scope.isSwiftGroupRequirementsFilled;
    //}

    $scope.fnAddOrUpdateSwiftGroup = function (isAdd) {
        $scope.isAdd = isAdd;
        if (isAdd)
            $scope.swiftGroup = angular.copy($scope.dummySwiftGroup);
        else {
            $scope.swiftGroup = angular.copy($scope.selectedRowData);
        }
        angular.element("#swiftGroupModal").modal({ backdrop: 'static', keyboard: true }).on("shown.bs.modal", function () {

            $("#liBrokerEntity").select2("val", $scope.swiftGroup.SwiftGroup.BrokerLegalEntityId);
            $("#liSwiftGroupStatus").select2("val", $scope.swiftGroup.SwiftGroup.SwiftGroupStatusId);
            $("#liMessageTypes").select2("val", [$scope.swiftGroup.SwiftGroup.AcceptedMessages]);
        });
        //$timeout(function () {
        //    $scope.isSwiftGroupRequirementsFilled = !$scope.isSwiftGroupRequirementsFilled;
        //}, 50);
    }

    $(document).on("click", "#tblSwiftGroupData tbody tr ", function () {
        $("#tblSwiftGroupData tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $scope.selectedRowData = $scope.swiftGroupTable.row(this).data();
        $timeout(function () {
            $scope.enableSwiftGroupActions = true;
        }, 50);

    });

    $(document).on("dblclick", "#tblSwiftGroupData tbody tr", function () {

        $scope.selectedRowData = $scope.swiftGroupTable.row(this).data();
        $scope.fnAddOrUpdateSwiftGroup(false);
    });

    //$(document).on("change", ".dropDown", function () {
    //    $timeout(function () {
    //        $scope.isSwiftGroupRequirementsFilled = !$scope.isSwiftGroupRequirementsFilled;
    //    }, 50);
    //});

    //$scope.$watch("isSwiftGroupRequirementsFilled", function (newValue, oldValue) {
    //    if (oldValue != undefined)
    //        $scope.isSwiftGroupRequirementsFilled = $("#liBrokerEntity").select2('val') != "" && $("#liSwiftGroupStatus").select2('val') != "" && $("#liMessageTypes").select2('val') != "" && $scope.swiftGroup.SwiftGroup != null && $scope.swiftGroup.SwiftGroup.SwiftGroup != "" && $scope.swiftGroup.SwiftGroup.SendersBIC != null && $scope.swiftGroup.SwiftGroup.SendersBIC != "";
    //});

    $scope.fnSaveSwiftGroup = function () {
        var existingSwiftGroup = $filter('filter')($scope.swiftGroupData, function (swift) {
            return swift.SwiftGroup.hmsSwiftGroupId != $scope.swiftGroup.SwiftGroup.hmsSwiftGroupId && (swift.SwiftGroup.SwiftGroup == $scope.swiftGroup.SwiftGroup.SwiftGroup || swift.SwiftGroup.SendersBIC == $scope.swiftGroup.SwiftGroup.SendersBIC);
        }, true)[0];
        if (existingSwiftGroup != undefined) {
            notifyError("Swift group data exists for selected Swift Group and Sender's BIC. Please select a new combination.")
            return;
        }
        //$scope.formatSwiftGroup();
        $scope.swiftGroup.SwiftGroup.SendersBIC = $scope.swiftGroup.SwiftGroup.SendersBIC.toUpperCase();
        var hmsSwiftGroup = angular.copy($scope.swiftGroup.SwiftGroup);

        $http({
            method: "POST",
            url: "/SwiftGroup/AddOrUpdateSwiftGroup",
            type: "json",
            data: JSON.stringify({
                hmsSwiftGroup: hmsSwiftGroup
            })
        }).then(function (response) {
            notifySuccess("Swift Group Details " + ($scope.isAdd ? "added" : "updated") + " successfully");
            $scope.fnGetSwiftGroupData();
            angular.element("#swiftGroupModal").modal('hide');
        },
            function (error) {
                notifyError("Changes failed to save with error :" + error.data);
            });
    }

    $scope.fnDeleteSwiftGroup = function () {
        if (!$scope.selectedRowData.IsAssociatedToAccount) {
            showMessage("Are you sure do you want to delete this Swift Group? ", "Delete Swift Group", [
                {
                    label: "Delete",
                    className: "btn btn-sm btn-danger",
                    callback: function () {
                        $http.post("/SwiftGroup/DeleteSwiftGroup", { swiftGroupId: $scope.selectedRowData.SwiftGroup.hmsSwiftGroupId }).then(function () {
                            notifySuccess("Swift group deleted successfully");
                            $scope.fnGetSwiftGroupData();
                        });
                    }
                },
                {
                    label: "Cancel",
                    className: "btn btn-sm btn-default"
                }
            ]);
        }
        else {
            notifyError("Swift group cannot be deleted as it is associated to 1 or more fund accounts.");
        }
    }

    $scope.fnExportData = function () {
        window.location.assign("/SwiftGroup/ExportData");
    }

});

