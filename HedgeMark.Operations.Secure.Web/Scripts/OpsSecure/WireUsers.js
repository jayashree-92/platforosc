/// <reference path="../data.js" />

var tblUserDetails
HmOpsApp.controller("wireUsersCtrl", function ($scope, $http, $opsSharedScopes, $q, $filter, $timeout) {

    $scope.fnRefreshUserList = function () {
        $("#btnRefreshUserList").button("loading");
        $http.get("/User/RefreshUserList").then(function (response) {
            $scope.fnLoadUserGrid();
            $("#btnRefreshUserList").button("reset");
            notifySuccess("Users list refreshed successfully.");
        }, function (error) {
            console.log(error);
            $("#btnRefreshUserList").button("reset");
            notifyError("Users list refresh un-successful. Please try again");
        });
    }

    $scope.fnExportReport = function (groupOption) {
        window.location.href = "/User/ExportReport?groupOption=" + groupOption;
    }

    $scope.IsGroupedByRole = false;
    $scope.fnLoadUserGrid = function () {
        $http.get("/User/GetWireUsers").then(function (response) {
            if ($("#tblUserDetails").hasClass("initialized")) {
                fnDestroyDataTable("#tblUserDetails");
            }

            $scope.tblUserDetails = $("#tblUserDetails").not(".initialized").addClass("initialized").DataTable({
                "bDestroy": true,
                // responsive: true,
                aaData: response.data,
                "aoColumns": [
                    { "sTitle": "User Id", "mData": "User.hmsWireUserId", "visible": false },
                    {
                        "sTitle": "Name", "mData": "UserName", "mRender": function (tdata, td, row, rowObj) {
                            if (row.AuthorizationCode == 2)
                                return tdata + "<span class='required'></span>";
                            return tdata;
                        }
                    },
                    {
                        "sTitle": "Role", "mData": "User.LdapRole", "mRender": function (tdata, td, row, rowObj) {
                            return tdata == "hm-wire-approver"
                                ? "<label class='label label-primary'>hm-wire-approver</label>" : tdata == "hm-wire-initiator" ? "<label class='label label-info'>hm-wire-initiator</label>"
                                    : tdata == "hm-wire-admin" ? "<label class='label label-danger'>hm-wire-admin</label>" : "<label class='label label-warning'>" + tdata + "</label>";
                        }
                    },
                    { "sTitle": "Group", "mData": "UserGroup", "visible": false },

                    //{ "sTitle": "Wires Initiated", "mData": "TotalWiresInitiated" },
                    //{ "sTitle": "Wires Approved", "mData": "TotalWiresApproved" },
                    { "sTitle": "Account Status", "mData": "User.AccountStatus", "mRender": function (tdata, td, row, rowObj) { return tdata.toLowerCase() == "active" ? "<label class='label label-success'>Active</label>" : "<label class='label label-danger'>" + tdata + "</label>"; } },
                    {
                        "sTitle": "Last Accessed On", "mData": "LastAccessedOn", "type": "dotnet-date",
                        "mRender": function (tdata) {
                            if (moment(tdata).format("YYYY") == "0001")
                                return "-never-";

                            return "<div title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateAndTimeForDisplay(tdata) + "</div>";
                        }
                    }],
                rowGroup: {

                    startRender: function (rows, group) {

                        var allUsers = rows.data();

                        var authorizedToHandleSystemWiresOnly = true;
                        for (var j = 0; j < allUsers.length; j++) {
                            if (allUsers[j].AuthorizationCode != 2)
                                authorizedToHandleSystemWiresOnly = false;
                        }

                        var message = "";
                        if (authorizedToHandleSystemWiresOnly)
                            message = "<i><span class='required'></span>The members of this group have wire approval authorization limited to system generated wires and those have not been subsequently edited</i>";

                        var groupRoleInfo = "";
                        if ($scope.IsGroupedByRole) {
                            groupRoleInfo = group == "hm-wire-approver"
                                ? "Group A : Authorized for Callbacks and Wire Approvals : "
                                : group == "hm-wire-initiator"
                                    ? "Group B - Electronic Wire Entry Only, Not Authorized for Callbacks or Wire Approvals :"
                                    : group == "hm-wire-admin"
                                        ? "Group C - Administrators and Not authorized to perform Wire initiation or approvals :" : ""
                                ;
                        }

                        return groupRoleInfo + "" + group + ' (' + rows.count() + ' users)        ' + message;
                    },
                    dataSrc: ["UserGroup"]
                },

                "deferRender": false,

                scroller: true,
                "sScrollX": "100%",
                "scrollY": window.innerHeight - 300,
                "sScrollXInner": "100%",
                "bScrollCollapse": true,
                //order: [[3, "asc"]],
                orderFixed: [[3, 'asc']],
                //"columnDefs": [{ "targets": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11], "orderable": false }],
                iDisplayLength: -1,
                "oLanguage": {
                    "sSearch": "",
                    "sEmptyTable": "No User details available",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ Users"
                }
            });


            $scope.tblUserDetails.on('rowgroup-datasrc', function (e, dt, val) {

                var orderIndex = 3;
                $scope.IsGroupedByRole = false;
                if (val == "Role") {
                    orderIndex = 2;
                    $scope.IsGroupedByRole = true;
                }

                $scope.tblUserDetails.order.fixed({ pre: [[orderIndex, 'asc']] }).draw();
            });

        });
    }

    $scope.fnLoadUserGrid();

    $scope.fnGroupResultsByColumn = function (val) {
        $scope.GroupByOption = val;
        $scope.tblUserDetails.rowGroup().dataSrc(val);
    }

});