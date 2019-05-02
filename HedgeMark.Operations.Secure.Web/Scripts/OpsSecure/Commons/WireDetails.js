var tblWireStatusDetails;

HmOpsApp.controller("wireDetailsCtrl", function ($scope, $http, $timeout, $opsSharedScopes) {

    $scope.IsWireTicketModelOpen = false;

    $scope.$on("loadWireDetailsGrid", function (event, statusId, startContextDate, endContextDate) {
        $scope.fnLoadWireDetailsGrid(statusId, startContextDate, endContextDate);
    });

    $("#modalToRetrieveWires").on("show.bs.modal", function () {
    }).on("shown.bs.modal", function () {
        $scope.IsWireTicketModelOpen = true;
        $opsSharedScopes.get("wireInitiationCtrl").fnLoadWireTicketDetails($scope.wireObj.WireId, $scope.wireObj.HMWire.hmsWirePurposeLkup.ReportName, $scope.wireObj.HMWire.hmsWirePurposeLkup.Purpose, $scope.wireObj.Agreement.AgreementShortName);
        $scope.$emit("wireTicketModelOpen");
    }).on("hidden.bs.modal", function () {
        $scope.IsWireTicketModelOpen = false;
        $scope.$emit("wireTicketModelClosed", { statusId: $scope.SelectedStatusId });
    });

    $scope.fnLoadWireDetailsGrid = function (statusId, startContextDate, endContextDate) {

        if ($scope.IsWireTicketModelOpen)
            return;

        $scope.SelectedStatusId = statusId;

        $http.get("/Home/GetWireStatusDetails?startContextDate=" + startContextDate + "&endContextDate=" + endContextDate + "&statusId=" + statusId).then(function (response) {

            //$scope.userDetails = response.data.userData;
            var wireDetails = response.data;

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
                    { "mData": "WireId", "sTitle": "Id"},
                    { "mData": "FundName", "sTitle": "Fund" },
                    {
                        "mData": "", "sTitle": "CounterParty",
                        "mRender": function (tdata, type, row) {
                            if (row.IsBookTransfer)
                                return "BNY";
                            else
                                return row.Agreement.dmaCounterPartyOnBoarding.CounterpartyName;
                        }

                    },
                    {
                        "mData": "TransferType", "sTitle": "Transfer Type"
                    },
                    {
                        "mData": "Agreement.AgreementShortName", "sTitle": "Agreement",
                        "mRender": function (tdata) {
                            return "<div>" + (tdata != null ? tdata : "N/A") + "</div>";
                        }
                    },
                    {
                        "mData": "HMWire.ValueDate", "sTitle": "Value Date",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForDisplay(tdata) + "' date='" + tdata + "'>" + getDateForDisplay(tdata) + "</div>";
                        }
                    },
                    { "mData": "HMWire.hmsWirePurposeLkup.Purpose", "sTitle": "Wire Purpose" },
                    { "mData": "SendingAccount.AccountName", "sTitle": "Sending Account" },
                    { "mData": "ReceivingAccountName", "sTitle": "Receiving Account" },
                    {
                        "mData": "HMWire.Amount", "sTitle": "Amount",
                        "mRender": function (tdata, type, row) {
                            return $.convertToCurrency(tdata.toString(), 2);
                        }

                    },
                    { "mData": "HMWire.hmsWireMessageType.MessageType", "sTitle": "Wire Message Type" },
                    { "mData": "HMWire.Currency", "sTitle": "Currency" },
                    {
                        "mData": "HMWire.WireStatusId", "sTitle": "Wire Status",
                        "mRender": function (tdata, type, row) {
                            switch (tdata) {
                                case 1: return "<label class='label label-default'>Drafted</label>";
                                case 2: return "<label class='label label-warning'>Pending</label>";
                                case 3: return "<label class='label label-success'>Approved</label>";
                                case 4: return "<label class='label label-default'>Cancelled</label>";
                                case 5: return "<label class='label label-danger'>Failed</label>";
                            }
                        }
                    }, {
                        "mData": "HMWire.SwiftStatusId", "sTitle": "Swift Status",
                        "mRender": function (tdata, type, row) {
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
                        "mData": "HMWire.CreatedAt",
                        "sTitle": "Initiated At",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + $.getPrettyDate(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                     {
                         "mData": "WireLastUpdatedBy", "sTitle": "Last Updated By"
                     },
                    {
                        "mData": "HMWire.LastModifiedAt",
                        "sTitle": "Last Updated At",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + $.getPrettyDate(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
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
                "order": [[17, "desc"]],
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
                "mark": { "exclude": [".ignoreMark"] },
                "columnDefs": [
                    //{ "targets": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], "orderable": false },
                    //{ "targets": [9], className: "ignoreMark", "orderable": false }
                ],

            });


            $("#tblWireStatusDetails").off("dblclick", "tbody tr").on("dblclick", "tbody tr", function (event) {
                $scope.wireObj = tblWireStatusDetails.row($(this)).data();

                if ($scope.wireObj == undefined)
                    return;

                angular.element("#modalToRetrieveWires").modal({ backdrop: 'static', keyboard: true, show: true });
            });
        });
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