$("#liWirePurpose").addClass("active");
var tblWirePurpose, tblWirePurposeForOtherReports, tblWireControls;

HmOpsApp.controller("WirePurposeMgmtCtrl", function ($scope, $http, $timeout) {

    $scope.IsWirePurposeRowSelected = false;
    $scope.NewPurpose = "";
    $scope.AllPurposes = [];

    $("#selReportName").select2({ width: "70px" });

    $scope.fnGetWirePurposesForOtherReports = function () {

        $http.get("/WirePurpose/GetWirePurposesForOtherReports").then(function (response) {
            $scope.AllPurposes = response.data;

            tblWirePurposeForOtherReports = $("#tblWirePurposeForOtherReports").DataTable({
                data: response.data,
                "bDestroy": true,
                iDisplayLength: -1,
                rowId: "hmsWirePurposeId",
                "columns": [
                    { "mData": "hmsWirePurposeId", "sTitle": "Id" },
                    { "mData": "ReportName", "sTitle": "Report Name" },
                    { "mData": "Purpose", "sTitle": "Purpose" }
                ],
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ other reports wire purpose",
                    "sInfoFiltered": " - filtering from _MAX_ wire purpose"
                },
                "deferRender": true,
                "sScrollX": "100%",
                "scrollY": window.innerHeight - 350,
                order: [1, "asc"]
            });
        });
    };


    $scope.fnGetWirePurposes = function () {

        $http.get("/WirePurpose/GetWirePurposesForAdhocReport").then(function (response) {
            var data = JSON.parse(response.data);

            tblWirePurpose = $("#tblWirePurpose").DataTable({
                "aaData": data.aaData,
                "aoColumns": data.aoColumns,
                "columnDefs": data.columnDefs,
                pageResize: true,
                "bDestroy": true,
                "createdRow": function (row, data) {
                    for (var i = 0; i < data.length - 2; i++) {
                        var controlResult = data[i];
                        switch (controlResult) {
                            case "Blocked":
                                $("td:eq(" + (i - 1) + ")", row).html("<label class='label label-default'>" + controlResult + "</label>");
                                break;
                            case "Approved":
                                $("td:eq(" + (i - 1) + ")", row).html("<label class='label label-success'>" + controlResult + "</label>");
                                break;
                            case "Requested":
                                $("td:eq(" + (i - 1) + ")", row).html("<label class='label label-warning'>" + controlResult + "</label>");
                                break;
                        }
                    }
                },
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ ad-hoc reports wire purpose",
                    "sInfoFiltered": " - filtering from _MAX_ wire purpose"
                },

                "deferRender": true,
                "scroller": true,
                "orderClasses": false,
                "sScrollX": "100%",
                "scrollY": window.innerHeight - 350,
                order: [0, "asc"],
                iDisplayLength: -1
            });
        });

        $("#tblWirePurpose").off("click", "tr").on("click", "tr", function () {

            tblWirePurpose.rows(".info").nodes().to$().removeClass("info");
            $(this).toggleClass("info");

            var wirePurpose = tblWirePurpose.row(".info").data();

            if (wirePurpose == undefined)
                return;

            $scope.IsWirePurposeRowSelected = true;
            $scope.$apply();
        });

        $("#tblWirePurpose").off("dblclick", "tr").on("dblclick", "tr", function () {
            var wirePurpose = tblWirePurpose.row(".info").data();
            $scope.fnSelectedWirePurpose = wirePurpose;
            $scope.$apply();
            $scope.fnShowWirePurposeControls(wirePurpose);
        });
    }

    $scope.fnGetWirePurposes();


    $scope.fnGetWirePurposeControls = function (wirePurpose) {

        if (wirePurpose == null)
            wirePurpose = $scope.fnSelectedWirePurpose;

        $http.get("/WirePurpose/GetWireControl?wirePurposeId=" + wirePurpose[1] + "&wirePurposeName=" + wirePurpose[2]).then(function (response) {

            tblWireControls = $("#tblWireControls").DataTable({
                data: response.data,
                "bDestroy": true,
                "columns": [
                    { "mData": "WirePurpose", "sTitle": "Wire Purpose" },
                    { "mData": "TransferType", "sTitle": "Transfer Type" },
                    {
                        "mData": "ControlStatus", "sTitle": "Current Status", "mRender": function (tData, type, row) {
                            if (tData == "Blocked")
                                return "<i class='glyphicon glyphicon-ban-circle'></i> " + tData;
                            if (tData == "Approved")
                                return "<i class='glyphicon glyphicon-ok'></i> " + tData;
                            if (row.WireControl.IsApprovalRequested) {
                                return "<i class='glyphicon glyphicon-warning-sign'></i> " + tData;
                            }
                        }
                    },
                    {
                        "mData": "ControlStatus", "sTitle": "Action", "mRender": function (tData, type, row) {

                            if (row.ControlStatus == "Approved")
                                return "<button class='btn btn-xs btn-danger btnClsBlockPurpose' data-loading-text=\"<i class='glyphicon glyphicon-refresh icon-rotate'></i> blocking...\"><i class='glyphicon glyphicon-ban-circle' title='Block'></i>&nbsp; Block Purpose</button>";

                            if (row.ControlStatus == "Blocked")
                                return "<button class='btn btn-xs btn-info btnClsRequestPurpose' data-loading-text=\"<i class='glyphicon glyphicon-refresh icon-rotate'></i> requesting...\"><i class='glyphicon glyphicon-unchecked' title='Request Approval'></i>&nbsp; Request Approval </button>";

                            if (row.WireControl.IsApprovalRequested) {
                                if (row.IsAuthorizedToApprove)
                                    return "<div class='btn-group'>" +
                                        "<button class='btn btn-xs btn-success btnClsApprovePurpose' data-loading-text=\"<i class='glyphicon glyphicon-refresh icon-rotate'></i> approving...\"><i class='glyphicon glyphicon-ok' title='Edit Task'></i>&nbsp; Approve</button>"
                                        + "<button class='btn btn-xs btn-danger btnClsBlockPurpose' data-loading-text=\"<i class='glyphicon glyphicon-refresh icon-rotate'></i> rejecting...\"><i class='glyphicon glyphicon-remove' title='Edit Task'></i>&nbsp; Block</button>"
                                        + "</div>";
                                else
                                    return "<label class='label label-warning'>Pending Approval</label>";
                            }

                            return "<label class='label label-warning'>" + tData + "</label>";
                        }
                    },
                    { "mData": "RecCreatedBy", "sTitle": "Requested By" },
                    {
                        "mData": "WireControl.RecCreatedAt", "sTitle": "Requested At", "mRender": function (tData) {
                            if (moment(tData).year() == 1)
                                return "-";
                            return "<div  title='" + $.getPrettyDate(tData) + "' date='" + tData + "'>" + getDateForToolTip(tData) + "</div>";
                        }
                    },
                    { "mData": "LastModifiedBy", "sTitle": "ApprovedBy" },
                    {
                        "mData": "WireControl.LastModifiedAt", "sTitle": "Approved At", "mRender": function (tData) {
                            if (moment(tData).year() == 1)
                                return "-";
                            return "<div  title='" + $.getPrettyDate(tData) + "' date='" + tData + "'>" + getDateForToolTip(tData) + "</div>";
                        }
                    }
                ],
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ other reports wire purpose",
                    "sInfoFiltered": " - filtering from _MAX_ wire purpose"
                },
                "deferRender": true,
                "sScrollX": "100%",
                "scrollY": "100%",
                order: [1, "asc"],
            });
        });

        $("#tblWireControls").off("click", "tr").on("click", "tr", function () {

            tblWireControls.rows(".info").nodes().to$().removeClass("info");
            $(this).toggleClass("info");

            var wireControls = tblWireControls.row(".info").data();
            $scope.SelectedWirePurposeConrol = wireControls.WireControl;
            $scope.$apply();
        });
    }

    $scope.fnSelectedWirePurpose = null;
    $scope.fnShowWirePurposeControls = function (wirePurpose) {
        $scope.fnSelectedWirePurpose = wirePurpose;
        $("#mdlManageControls").modal("show").off("shown.bs.modal").on("shown.bs.modal", function () {
            $scope.fnGetWirePurposeControls(wirePurpose);
        }).off("hidden.bs.modal").on("hidden.bs.modal", function () {
            $scope.fnGetWirePurposes();
        });
    }

    $scope.SelectedWirePurposeConrolId = 0;
    $(document).on("click", ".btnClsBlockPurpose", function () {
        $scope.SelectedButton = $(this);
        $scope.SelectedWirePurposeConrolId = tblWirePurpose.row(".info").data();
        $(this).popover("destroy").popover({
            trigger: "click",
            title: "Are you sure you want to <b>block</b> the Purpose to selected Transfer type?",
            placement: "top",
            container: "body",
            content: function () {
                return "<div class=\"btn-group pull-right\" style='margin-bottom:7px; !important'>"
                    + "<a class=\"btn btn-xs btn-danger confirmBlockPurpose\"><i class=\"glyphicon glyphicon-ban-circle\"></i>&nbsp;Block</a>"
                    + "<a class=\"btn btn-xs btn-default dismissBlockPurpose\"><i class=\"glyphicon glyphicon-remove\"></i>&nbsp;Cancel</a>"
                    + "</div>"
                    + "<br/>";
            },
            html: true
        }).popover("show");
    });



    $(document).on("click", ".btnClsApprovePurpose", function () {
        $scope.SelectedButton = $(this);
        $scope.SelectedButton.button("loading");
        $http.post("/WirePurpose/ApproveWirePurposeControl?wirePurposeId=" + $scope.SelectedWirePurposeConrol.hmsWirePurposeId + "&transferTypeId=" + $scope.SelectedWirePurposeConrol.WireTransferTypeId).then(function (response) {
            $scope.fnGetWirePurposeControls();
            $scope.SelectedButton.button("reset");
        }, function () {
            notifyError("failed to perform the changes");
            $scope.SelectedButton.button("reset");
        });
    });

    $scope.SelectedButton = null;
    $(document).on("click", ".btnClsRequestPurpose", function () {
        $scope.SelectedButton = $(this);
        $scope.SelectedButton.button("loading");
        $http.post("/WirePurpose/RequestWirePurposeControl?wirePurposeId=" + $scope.SelectedWirePurposeConrol.hmsWirePurposeId + "&transferTypeId=" + $scope.SelectedWirePurposeConrol.WireTransferTypeId).then(function (response) {
            $scope.fnGetWirePurposeControls();
            $scope.SelectedButton.button("reset");
        }, function () {
            notifyError("failed to perform the changes");
            $scope.SelectedButton.button("reset");
        });
    });


    $(document).on("click", ".confirmBlockPurpose", function () {
        angular.element(".btnClsBlockPurpose").popover("hide");

        $scope.SelectedButton.button("loading");
        $http.post("/WirePurpose/BlockWirePurposeControl?wirePurposeId=" + $scope.SelectedWirePurposeConrol.hmsWirePurposeId + "&transferTypeId=" + $scope.SelectedWirePurposeConrol.WireTransferTypeId).then(function (response) {
            $scope.fnGetWirePurposeControls();
            $scope.SelectedButton.button("reset");
        }, function () {
            notifyError("failed to perform the changes");
            $scope.SelectedButton.button("reset");
        });
    });

    $(document).on("click", ".dismissBlockPurpose", function () {
        angular.element(".btnClsBlockPurpose").popover("hide");
    });


    $scope.fnModifyWirePurpose = function (isApproved) {
        var wirePurpose = tblWirePurpose.row(".info").data();
        if (wirePurpose == undefined) {
            notifyWarning("Please select an item and try again");
            return;
        }
        $scope.fnShowWirePurposeControls(wirePurpose);
    };


    $scope.fnAddNewPurpose = function () {

        $("#mdlAddNewPurpose").modal("hide");

        if ($scope.NewPurpose.trim() == "") {
            notifyWarning("purpose cannot be blank");
            return;
        }

        var isAlreadyPresent = false;
        $($scope.AllPurposes).each(function (i, v) {
            if (v.ReportName == "Adhoc Report" && v.Purpose == $scope.NewPurpose)
                isAlreadyPresent = true;
        });

        if (isAlreadyPresent) {
            notifyWarning("purpose already defined");
            return;
        }

        $http.post("/WirePurpose/AddWirePurpose?reportName=" + $("#selReportName").val() + "&purpose=" + $scope.NewPurpose).then(function (response) {
            $scope.fnGetWirePurposes();
        }, function () {
            notifyError("unable to create new wire purpose");
        });
    }

});