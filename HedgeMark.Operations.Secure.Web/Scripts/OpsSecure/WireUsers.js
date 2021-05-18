/// <reference path="../data.js" />

var tblUserDetails
HmOpsApp.controller("wireUsersCtrl", function ($scope, $http, $opsSharedScopes, $q, $filter, $timeout) {


    $scope.fnExportReport = function (groupOption) {
        window.location.href = "/User/ExportReport?groupOption=" + groupOption;
    }

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
                { "sTitle": "Role", "mData": "User.LdapRole", "mRender": function (tdata, td, row, rowObj) { return tdata == "hm-wire-approver" ? "<label class='label label-primary'>hm-wire-approver</label>" : "<label class='label label-info'>hm-wire-initiator</label>" } },
                { "sTitle": "Group", "mData": "UserGroup", "visible": false },
                {
                    "sTitle": "Added By", "mData": "CreatedBy", "mRender": function (tdata, td, row, rowObj) { return tdata == null ? "-" : tdata; }
                },
                {
                    "sTitle": "Added At",
                    "mData": "User.CreatedAt",
                    "type": "dotnet-date",
                    "mRender": function (tdata) {
                        return "<div title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + (moment(tdata).fromNow()) + "</div>";
                    }
                },
                {
                    "sTitle": "Approved By", "mData": "ApprovedBy", "mRender": function (tdata, td, row, rowObj) { return tdata == null ? "-" : tdata; }
                },
                {
                    "sTitle": "Approved At",
                    "mData": "User.ApprovedAt",
                    "type": "dotnet-date",
                    "mRender": function (tdata) {
                        return "<div title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + (moment(tdata).fromNow()) + "</div>";
                    }
                },
                { "sTitle": "Wires Initiated", "mData": "TotalWiresInitiated" },
                { "sTitle": "Wires Approved", "mData": "TotalWiresApproved" },
                { "sTitle": "Account Status", "mData": "User.AccountStatus", "mRender": function (tdata, td, row, rowObj) { return tdata.toLowerCase() == "active" ? "<label class='label label-success'>Active</label>" : "<label class='label label-danger'>" + tdata + "</label>"; } },
                {
                    "sTitle": "Last Accessed On", "mData": "LastAccessedOn", "type": "dotnet-date",
                    "mRender": function (tdata) {
                        if (moment(tdata).format("YYYY") == "0001")
                            return "-never-";

                        return "<div title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + (moment(tdata).fromNow()) + "</div>";
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

                    return group + ' (' + rows.count() + ' users)        ' + message;
                },
                dataSrc: ["UserGroup"]
            },

            "deferRender": false,

            scroller: true,
            "sScrollX": "100%",
            "scrollY": window.innerHeight - 300,
            "sScrollXInner": "100%",
            //"sScrollYInner": "60%",
            "bScrollCollapse": true,
            order: [[3, "asc"]],
            "columnDefs": [{ "targets": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11], "orderable": false }],
            iDisplayLength: -1,
            //"bScrollCollapse": true,
            ////scroller: true,
            ////sortable: false,
            //"searching": false,
            //"bInfo": false,
            //"sDom": "ift",
            ////pagination: true,
            //iDisplayLength: -1,
            //"sScrollX": "100%",
            //"sScrollY": "100%",
            //"sScrollXInner": "100%",
            //"scrollY": Window.innerHeight - 350,
            //"order": [[2, "desc"]],

            "oLanguage": {
                "sSearch": "",
                "sEmptyTable": "No User details available",
                "sInfo": "Showing _START_ to _END_ of _TOTAL_ Users"
            }
        });
    });
});