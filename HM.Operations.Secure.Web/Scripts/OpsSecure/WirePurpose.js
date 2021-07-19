$("#liWirePurpose").addClass("active");
var tblWirePurpose;

HmOpsApp.controller("WirePurposeMgmtCtrl", function ($scope, $http, $timeout) {

    $scope.IsAuthorizedToApprove = false;
    $scope.NewPurpose = "";
    $scope.AllPurposes = [];

    $("#selReportName").select2({ width: "70px" });

    $scope.fnGetWirePurposes = function () {

        $http.get("/Home/GetWirePurposes").then(function (response) {

            $scope.AllPurposes = response.data;

            tblWirePurpose = $("#tblWirePurpose").DataTable({
                aaData: response.data,
                pageResize: true,
                "bDestroy": true,
                "dom": "<'pull-right'f>itI",
                rowId: "hmsWirePurposeId",
                "columns": [
                    { "mData": "hmsWirePurposeId", "sTitle": "hmsWiresId", visible: false },
                    { "mData": "ReportName", "sTitle": "Report Name" },
                    { "mData": "Purpose", "sTitle": "Purpose" },
                    {
                        "mData": "CreatedBy", "sTitle": "CreatedBy"
                    },
                    {
                        "mData": "CreatedAt", "sTitle": "CreatedAt",
                        "mRender": function (tdata) {
                            return "<div  title='" + $.getPrettyDate(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                    { "mData": "ModifiedBy", "sTitle": "ModifiedBy" },
                    {
                        "mData": "ModifiedAt", "sTitle": "ModifiedAt",
                        "mRender": function (tdata) {

                            if (tdata == undefined || tdata == "")
                                return "-";

                            return "<div  title='" + $.getPrettyDate(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                    {
                        "mData": "IsApproved", "sTitle": "IsApproved", "mRender": function (tdata, type, row) {

                            if (row.ModifiedBy == "-")
                                return "-";
                            return tdata ? "<label class='label label-success'>Approved</label>" : "<label class='label label-danger'>Rejected</label>";
                        }
                    },

                ], "createdRow": function (row, data) {

                    if (data.ReportName != "Adhoc Report")
                        $(row).addClass("blocked");

                    else if (data.IsApproved)
                        $(row).addClass("success");

                    else if (data.ModifiedBy != "-")
                        $(row).addClass("danger");


                },
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ wire tickets",
                    "sInfoFiltered": " - filtering from _MAX_ tickets"
                },
                //"scrollX": false,
                //"sScrollX": "100%",
                //"sScrollXInner": "100%",
                //"sScrollY": false,
                
                "deferRender": true,
                "scroller": true,
                "orderClasses": false,
                "sScrollX": "100%",
                //sDom: "ift",
                "scrollY": window.innerHeight - 350,

                order: [1, 'asc'],
                //"bPaginate": false,
                iDisplayLength: -1
            });
        });

        $("#tblWirePurpose").off("click", "tr").on("click", "tr", function () {

            tblWirePurpose.rows(".info").nodes().to$().removeClass("info");
            $(this).toggleClass("info");

            var wirePurpose = tblWirePurpose.row(".info").data();

            if (wirePurpose == undefined)
                return;

            $scope.IsAuthorizedToApprove = wirePurpose.ReportName == "Adhoc Report" && wirePurpose.IsAuthorizedToApprove && !wirePurpose.IsRejected && !wirePurpose.IsApproved;
            $scope.$apply();

        });
    };

    $scope.fnGetWirePurposes();

    $scope.fnModifyWirePurpose = function (isApproved) {

        var wirePurpose = tblWirePurpose.row(".info").data();

        if (wirePurpose == undefined) {
            notifyWarning("Please select an item and try again");
            return;
        }


        bootbox.confirm("Are you sure you want to " + (isApproved ? "approve" : "reject") + " the selected purpose ?", function (result) {
            if (!result) {
                return;
            } else {
                $http.post("/Home/ApproveOrRejectWirePurpose?wirePurposeId=" + wirePurpose.hmsWirePurposeId + "&isApproved=" + isApproved).then(function (response) {
                    $scope.fnGetWirePurposes();
                }, function () {
                    notifyError("failed to perform the changes");
                });
            }
        });

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

        $http.post("/Home/AddWirePurpose?reportName=" + $("#selReportName").val() + "&purpose=" + $scope.NewPurpose).then(function (response) {
            $scope.fnGetWirePurposes();
        }, function () {
            notifyError("unable to create new wire purpose");
        });
    }

});