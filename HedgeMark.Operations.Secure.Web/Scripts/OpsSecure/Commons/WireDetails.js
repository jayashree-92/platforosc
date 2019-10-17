var tblWireStatusDetails;

HmOpsApp.controller("wireDetailsCtrl", function ($scope, $http, $timeout, $opsSharedScopes) {

    $scope.IsWireTicketModelOpen = false;

    $scope.$on("loadWireDetailsGrid", function (event, statusId, startContextDate, endContextDate) {
        $scope.fnLoadWireDetailsGrid(statusId, startContextDate, endContextDate);
    });

    $("#modalToRetrieveWires").on("show.bs.modal", function () {
    }).on("shown.bs.modal", function () {
        $scope.IsWireTicketModelOpen = true;
        $opsSharedScopes.get("wireInitiationCtrl").fnLoadWireTicketDetails($scope.wireObj);
        $scope.$emit("wireTicketModelOpen");
    }).on("hidden.bs.modal", function () {
        $scope.IsWireTicketModelOpen = false;
        $scope.$emit("wireTicketModelClosed", { statusId: $scope.SelectedStatusId });
    });

    $scope.fnLoadWireDetailsGrid = function (statusId, startContextDate, endContextDate) {

        if ($scope.IsWireTicketModelOpen)
            return;

        $scope.SelectedStatusId = statusId;

        $("#btnGetWireLogs").button("loading");

        $http.get("/Home/GetWireStatusDetails?startContextDate=" + startContextDate + "&endContextDate=" + endContextDate + "&statusIds=" + statusId).then(function (response) {

            //$scope.userDetails = response.data.userData;
            var wireDetails = response.data.wireData;

            tblWireStatusDetails = $("#tblWireStatusDetails").DataTable({
                aaData: wireDetails,
                pageResize: true,
                // dom: 'Bfrtip',
                fixedHeader: {
                    headerOffset: 40
                },
                //"dom": "<'row'<'col-md-6'i><'#toolbar_tasklog'><'col-md-6 pull-right'f>>trI",
                "dom": "<'pull-right'f>itI",
                rowId: "hmsWireId",

                rowCallback: function (row, data, index) {

                },
                //"dom": "trI",
                "bDestroy": true,
                "columns": [
                    { "data": "WireId", "sTitle": "Id" },
                    { "data": "PreferredFundName", "sTitle": "Fund" },
                    {
                        "data": "Counterparty", "sTitle": "Counterparty/Service Provider",
                        "render": function (tdata) {
                            return (tdata == null || tdata.trim() == "") ? "N/A" : tdata;
                        }
                    },
                    {
                        "data": "TransferType", "sTitle": "Transfer Type"
                    },
                    //{
                    //    "mData": "Agreement.AgreementShortName", "sTitle": "Agreement",
                    //    "mRender": function (tdata) {
                    //        return "<div>" + (tdata != null ? tdata : "N/A") + "</div>";
                    //    }
                    //},
                    {
                        "data": "HMWire.ValueDate", "sTitle": "Value Date",
                        "render": renderDotNetDateOnly
                    },
                    { "data": "HMWire.hmsWirePurposeLkup.Purpose", "sTitle": "Wire Purpose" },
                    { "data": "SendingAccount.AccountName", "sTitle": "Sending Account" },
                    {
                        "data": "ReceivingAccountName", "sTitle": "Receiving Account",
                        "render": function (tdata) {
                            return (tdata == null || tdata.trim() == "") ? "N/A" : tdata;
                        }
                    },
                    {
                        "data": "HMWire.Amount", "sTitle": "Amount",
                        "render": renderDataAsCurrency,
                    },
                    { "data": "HMWire.hmsWireMessageType.MessageType", "sTitle": "Wire Message Type" },
                    { "data": "HMWire.Currency", "sTitle": "Currency" },
                    {
                        "mData": "HMWire.WireStatusId", "sTitle": "Wire Status",
                        "render": function (tdata, type, row) {
                            switch (tdata) {
                                case 1: return "<label class='label label-default'>Drafted</label>";
                                case 2: return "<label class='label label-warning'>Pending</label>";
                                case 3: return "<label class='label label-success'>Approved</label>";
                                case 4: return row.HMWire.SwiftStatusId == 1 ? "<label class='label label-danger'>Rejected</label>" : "<label class='label label-default'>Cancelled</label>";
                                case 5: return "<label class='label label-danger'>Failed</label>";
                            }
                        }
                    }, {
                        "data": "HMWire.SwiftStatusId", "sTitle": "Swift Status",
                        "render": function (tdata, type, row) {
                            switch (tdata) {
                                case 1: return "<label class='label label-default'>Not Started</label>";
                                case 2: return "<label class='label label-warning'>Pending Ack</label>";
                                case 3: return "<label class='label label-info'>Acknowledged</label>";
                                case 4: return "<label class='label label-danger'>N-Acknowledged</label>";
                                case 5: return "<label class='label label-success'>Completed</label>";
                                case 6: return "<label class='label label-danger'>Failed</label>";
                            }
                        }
                    },
                     {
                         "mData": "WireCreatedBy", "sTitle": "Initiated By"
                     },
                    {
                        "data": "HMWire.CreatedAt",
                        "sTitle": "Initiated At",
                        "render": renderDotNetDateAndTime
                    },
                     {
                         "data": "WireLastUpdatedBy", "sTitle": "Last Updated By"
                     },
                    {
                        "data": "HMWire.LastModifiedAt",
                        "sTitle": "Last Updated At",
                        "render": renderDotNetDateAndTime
                    }
                ],
                "createdRow": function (row, data) {

                    switch (data.HMWire.hmsWireStatusLkup.Status) {
                        case "Drafted":
                            // $(row).addClass("info");
                            break;
                        case "Initiated":
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
                            $(row).addClass("success");
                            break;
                        case "Failed":
                            $(row).addClass("danger");
                            break;
                    }

                },
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "Showing _START_ to _END_ of _TOTAL_ wire tickets",
                    "sInfoFiltered": " - filtering from _MAX_ tickets"
                },
                "scrollX": false,
                "sScrollX": "100%",
                "sScrollXInner": "100%",
                "sScrollY": false,
                //"scrollY": $("#pageMainContent").offset().top + 600,
                //stateSave: true,
                //"scrollX": true,
                "order": [[16, "desc"]],
                //"bSort": false,
                "bPaginate": false,
                iDisplayLength: -1,
                sRowSelect: false,
                "preDrawCallback": function (settings) {
                    //    $scope.taskLogTblPageScrollPos = $(this).closest("div.dataTables_scrollBody").scrollTop();
                },
                "drawCallback": function (settings) {
                    $("[id^='tblWireStatusDetails'] tbody td").animate({ "opacity": "1", "padding-top": "8px " }, 500);
                },
                "columnDefs": [
                    //{ "targets": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], "orderable": false },
                    //{ "targets": [9], className: "ignoreMark", "orderable": false }
                ],

            });


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
            IsBookTransfer: false,
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