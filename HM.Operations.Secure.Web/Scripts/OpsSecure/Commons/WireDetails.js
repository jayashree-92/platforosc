/// <reference path="../../data.js" />
/// <reference path="../../angular.js" />
var tblWireStatusDetails;

HmOpsApp.controller("wireDetailsCtrl", function ($scope, $http, $timeout, $opsSharedScopes, $interval) {

    $scope.IsWireTicketModelOpen = false;

    $scope.$on("loadWireDetailsGrid", function (event, statusId, startContextDate, endContextDate) {
        $scope.fnLoadWireDetailsGrid(statusId, startContextDate, endContextDate);
    });

    $("#modalToRetrieveWires").on("show.bs.modal", function () {
    }).on("shown.bs.modal", function () {
        $scope.IsWireTicketModelOpen = true;
        $opsSharedScopes.get("wireInitiationCtrl").fnLoadWireTicketDetails($scope.wireObj);
        $scope.$emit("wireTicketModelOpen");
    }).on("hide.bs.modal", function () {
        $scope.IsWireTicketModelOpen = false;
        $("#wireCurrentlyViewedBy").collapse("hide");
        if ($scope.wireObj.WireId > 0)
            $scope.fnRemoveActionInProgres($scope.wireObj.WireId);
        $scope.$emit("wireTicketModelClosed", { statusId: $scope.SelectedStatusId });
    });

    $scope.fnRemoveActionInProgres = function (wireId) {
        $http.post("/Home/RemoveActionInProgress?wireId=" + wireId);
    }

    $scope.promise = null;

    $scope.fnLoadWireDetailsGrid = function (statusId, startContextDate, endContextDate) {

        if ($scope.IsWireTicketModelOpen)
            return;

        $scope.SelectedStatusId = statusId;

        $("#btnGetWireLogs").button("loading");

        $http.get("/Home/GetWireStatusDetails?startContextDate=" + startContextDate + "&endContextDate=" + endContextDate + "&statusIds=" + statusId + "&timeZone=" + getTimeZoneAbbr()).then(function (response) {

            //$scope.userDetails = response.data.userData;
            var wireDetails = response.data.wireData;

            tblWireStatusDetails = $("#tblWireStatusDetails").DataTable({
                aaData: wireDetails,
                pageResize: true,
                //fixedHeader: {
                //    //headerOffset: 10
                //},

                "dom": "<'pull-right'f>itI",
                rowId: "hmsWireId",
                buttons: [
                    {
                        extend: 'csv',
                        text: 'Export as .csv',
                        className: "btn-sm",
                        customize: function (csv) {
                            return constructCSVstring(tblWireStatusDetails);
                        }
                    }
                ],
                rowCallback: function (row, data, index) {

                },
                "bDestroy": true,
                "columns": [{
                    "data": "Deadline", "sTitle": "Deadline", "render": function (tdata, type, row) {

                        if (type == "sort")
                            return tdata;

                        if (row.HMWire.WireStatusId != 2)
                            return "n.a";

                        return fnGetWireDeadlineCounter(tdata);
                    }, "type": "timespan"
                },
                {
                    "mData": "HMWire.WireStatusId", "sTitle": "Wire Status",
                    "render": function (tdata, type, row) {
                        switch (tdata) {
                            case 1: return "<label class='label label-default'>Drafted</label>";
                            case 2: return "<label class='label label-warning'>Pending</label>";
                            case 3: return "<label class='label label-success'>Approved</label>";
                            case 4: return row.HMWire.SwiftStatusId == 1 ? "<label class='label label-danger'>Rejected</label>" : "<label class='label label-default'>Cancelled</label>";
                            case 5: return "<label class='label label-danger'>Failed</label>";
                            case 6: return "<label class='label label-info'>On-Hold</label>";
                        }
                    }
                }, {
                    "data": "HMWire.SwiftStatusId", "sTitle": "Swift Status",
                    "render": function (tdata, type, row) {
                        switch (tdata) {
                            case 1: return "<label class='label label-default'>Not Started</label>";
                            case 2: return "<label class='label label-warning'>Pending Ack</label>";
                            case 3: return "<label class='label label-success'>Acknowledged</label>";
                            case 4: return "<label class='label label-danger'>N-Acknowledged</label>";
                            case 5: return "<label class='label label-info'>Completed</label>";
                            case 6: return "<label class='label label-danger'>Failed</label>";
                        }
                    }
                },
                { "data": "WireId", "sTitle": "Id" },
                { "data": "ClientLegalName", "sTitle": "Client" },
                { "data": "PreferredFundName", "sTitle": "Fund" },
                { "data": "SendingAccount.AccountName", "sTitle": "Sending Account Name" },
                { "data": "SendingAccountNumber", "sTitle": "Sending Account Number" },
                { "data": "TransferType", "sTitle": "Transfer Type" },
                { "data": "HMWire.hmsWirePurposeLkup.ReportName", "sTitle": "Source Report" },
                { "data": "HMWire.hmsWirePurposeLkup.Purpose", "sTitle": "Wire Purpose" },
                { "data": "HMWire.ValueDate", "sTitle": "Value Date", "render": renderDotNetDateOnly },
                { "data": "HMWire.Currency", "sTitle": "Currency" },
                { "data": "HMWire.Amount", "sTitle": "Amount", "render": renderDataAsCurrency, },
                { "data": "ReceivingAccountName", "sTitle": "Template Name" },
                /*{ "data": "BeneficiaryBank", "sTitle": "Beneficiary Bank" },*/
                { "data": "UltimateBeneficiary", "sTitle": "Ultimate Beneficiary", },
                { "data": "UltimateBeneficiaryAccountNumber", "sTitle": "Ultimate Beneficiary A/C Number" },
                { "data": "HMWire.hmsWireMessageType.MessageType", "sTitle": "Wire Message Type" },
                { "data": "WireCreatedBy", "sTitle": "Initiated By" },
                { "data": "HMWire.CreatedAt", "sTitle": "Initiated At (" + getTimeZoneAbbr() + ")", "render": renderDotNetDateAndTime },
                { "data": "WireLastUpdatedBy", "sTitle": "Last Updated By" },
                { "data": "HMWire.LastModifiedAt", "sTitle": "Last Updated At (" + getTimeZoneAbbr() + ")", "render": renderDotNetDateAndTime },
                { "data": "WireApprovedBy", "sTitle": "Approved By" },
                { "data": "HMWire.ApprovedAt", "sTitle": "Approved At (" + getTimeZoneAbbr() + ")", "render": renderDotNetDateAndTime }
                ],
                "createdRow": function (row, data) {

                    switch (data.HMWire.hmsWireStatusLkup.Status) {
                        case "Drafted":
                            // $(row).addClass("info");
                            break;
                        case "Initiated":
                        case "Pending":
                            $(row).addClass("warning");
                            break;
                        case "Approved":
                        case "Processing":
                            $(row).addClass("success");
                            break;
                        case "Cancelled":
                            $(row).addClass("blocked");
                            break;
                        case "Completed":
                            $(row).addClass("info");
                            break;
                        case "Failed":
                            $(row).addClass("danger");
                            break;
                        case "On Hold":
                            $(row).addClass("blockedSection");
                            break;
                    }

                },
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ wire tickets",
                    "sInfoFiltered": " - filtering from _MAX_ tickets"
                },

                scroller: wireDetails.length > 5,
                "sScrollX": "100%",
                "scrollY": window.innerHeight - 250,
                "sScrollXInner": "76%",
                //"sScrollYInner": "60%",
                "bScrollCollapse": true,
                "order": [],
                iDisplayLength: -1,
                sRowSelect: false,
                "preDrawCallback": function (settings) {
                    //    $scope.taskLogTblPageScrollPos = $(this).closest("div.dataTables_scrollBody").scrollTop();
                },
                "drawCallback": function (settings) {
                    $("[id^='tblWireStatusDetails'] tbody td").animate({ "opacity": "1", "padding-top": "8px " }, 500);
                }
            });

            if ($scope.promise != null)
                $interval.cancel($scope.promise);
            $scope.promise = $interval(function () {
                tblWireStatusDetails.rows().every(function (rowindex) {
                    tblWireStatusDetails.cell(rowindex, 0).data(tblWireStatusDetails.row(rowindex).data().Deadline);
                });
            }, 1000);


            tblWireStatusDetails.buttons().container().appendTo('#spnCustomButtons');


            //$("#tblWireStatusDetails").off("dblclick", "tbody tr").on("dblclick", "tbody tr", function (event) {
            //    $scope.wireObj = tblWireStatusDetails.row($(this)).data();

            //    if ($scope.wireObj == undefined)
            //        return;

            //    angular.element("#modalToRetrieveWires").modal({ backdrop: 'static', keyboard: true, show: true });
            //});

            $("#tblWireStatusDetails").off("dblclick", "tbody tr").on("dblclick", "tbody tr", function (event) {
                var wireData = tblWireStatusDetails.row($(this)).data();
                if (wireData == undefined)
                    return;

                $scope.wireObj = {
                    WireId: wireData.WireId,
                    AgreementId: 0,
                    AgreementName: "",
                    FundId: wireData.HMWire.hmFundId,
                    ContextDate: $scope.contextDate,
                    Purpose: wireData.HMWire.hmsWirePurposeLkup.Purpose,
                    Report: wireData.HMWire.hmsWirePurposeLkup.ReportName,
                    IsAdhocWire: false,
                    ReportMapId: 0,
                    IsAdhocPage: true
                };
                angular.element("#modalToRetrieveWires").modal({ backdrop: 'static', keyboard: true, show: true });
            });


            $("#btnGetWireLogs").button("reset");
        });
    }

    $scope.initiateWireModal = function () {
        $scope.wireObj = {
            AgreementId: 0,
            ContextDate: $scope.contextDate,
            Purpose: "",
            Report: "Adhoc Report",
            IsAdhocWire: true,
            ReportMapId: 0,
            IsFundTransfer: false,
            AgreementName: "",
            IsAdhocPage: true
        };
        angular.element("#modalToRetrieveWires").modal({ backdrop: 'static', keyboard: true, show: true });
    }

    //$("#modalToRetrieveWires").on("scroll", function () {
    //    if ($(this).scrollTop() > 200) {
    //        $footer.addClass('fixed_footer');
    //        $header.addClass('fixed_header_table');
    //    } else if ($(this).scrollTop() < 200) {
    //        $footer.removeClass('fixed_footer');
    //        $header.removeClass('fixed_header_table');
    //    }
    //});

});