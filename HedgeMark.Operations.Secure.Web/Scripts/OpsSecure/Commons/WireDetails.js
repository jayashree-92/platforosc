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
                            //var allColumns = tblWireStatusDetails.columns().data().toArray();
                            var allColumns = tblWireStatusDetails.columns().nodes().to$().toArray();
                            var totalColumns = allColumns.length;

                            var rows = [];
                            var headerRow = "";
                            for (var i = 0; i < totalColumns; i++) {
                                headerRow += "\"" + tblWireStatusDetails.column(i).header().textContent + "\",";
                            }
                            rows.push(headerRow);
                            for (var i = 0; i < totalColumns; i++) {

                                for (var j = 0; j < allColumns[i].length; j++) {
                                    var row = rows[j + 1];
                                    if (row == undefined)
                                        row = "";

                                    row += "\"" + $(allColumns[i][j]).text() + "\",";
                                    rows[j + 1] = row;
                                }

                            }
                            var fullCsv = "";
                            for (var k = 0; k < rows.length; k++) {
                                fullCsv += rows[k] + "\n";
                            }

                            return fullCsv;
                        }
                    }
                ],
                rowCallback: function (row, data, index) {

                },
                "bDestroy": true,
                "columns": [
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
                    { "data": "SendingAccount.AccountNumber", "sTitle": "Sending Account Number" },
                    {
                        "data": "TransferType", "sTitle": "Transfer Type"
                    },

                    { "data": "HMWire.hmsWirePurposeLkup.Purpose", "sTitle": "Wire Purpose" },
                    {
                        "data": "HMWire.ValueDate", "sTitle": "Value Date",
                        "render": renderDotNetDateOnly
                    },

                    { "data": "HMWire.Currency", "sTitle": "Currency" },
                    {
                        "data": "HMWire.Amount", "sTitle": "Amount",
                        "render": renderDataAsCurrency,
                    },
                    {
                        "data": "ReceivingAccountName", "sTitle": "Template Name",
                        "render": function (tdata) {
                            return (tdata == null || tdata.trim() == "") ? "N/A" : tdata;
                        }
                    },

                    {
                        "data": "ReceivingAccount.AccountNumber", "sTitle": "Beneficiary Bank",
                        "render": function (tdata, type, row) {
                            if (row.IsFundTransfer) {
                                if (row.ReceivingAccount.UltimateBeneficiaryType == "Account Name")
                                    return row.ReceivingAccount.Beneficiary.BankName;
                                else
                                    return row.ReceivingAccount.UltimateBeneficiary.BankName;
                            }
                            else if (!row.IsNotice) {
                                if (row.SSITemplate.UltimateBeneficiaryType == "Account Name")
                                    return row.SSITemplate.Beneficiary.BankName;
                                else
                                    return row.SSITemplate.UltimateBeneficiary.BankName;
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": "ReceivingAccount.AccountNumber", "sTitle": "Beneficiary",
                        "render": function (tdata, type, row) {
                            if (row.IsFundTransfer) {
                                if (row.ReceivingAccount.UltimateBeneficiaryType == "Account Name")
                                    return row.ReceivingAccount.UltimateBeneficiaryAccountName;
                                else
                                    return row.ReceivingAccount.UltimateBeneficiary.BICorABA;
                            }
                            else if (!row.IsNotice) {
                                if (row.SSITemplate.UltimateBeneficiaryType == "Account Name")
                                    return row.SSITemplate.UltimateBeneficiaryAccountName;
                                else
                                    return row.SSITemplate.UltimateBeneficiary.BICorABA;
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": "ReceivingAccount.AccountNumber", "sTitle": "Beneficiary A/C Number",
                        "render": function (tdata, type, row) {
                            if (row.IsFundTransfer) {
                                return row.ReceivingAccount.AccountNumber;
                            }
                            else if (!row.IsNotice) {
                                return row.SSITemplate.AccountNumber;
                            }
                            return "N/A";
                        }

                    },

                    { "data": "HMWire.hmsWireMessageType.MessageType", "sTitle": "Wire Message Type" },
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
                    },
                    {
                        "data": "WireApprovedBy", "sTitle": "Approved By"
                    },
                    {
                        "data": "HMWire.ApprovedAt",
                        "sTitle": "Approved At",
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
                },
                "columnDefs": [
                    //{ "targets": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15], "orderable": false },
                    //{ "targets": [9], className: "ignoreMark", "orderable": false }
                ],

            });

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