$("#liSSITemplates").addClass("active");

var ssiTemplateTable;
var tblonBoardAccountRow, accountSsiTemplateTable;
var myDropZone;


HmOpsApp.controller("SSITemplateListController", function ($scope, $http, $timeout, $filter) {
    $("#onboardingMenu").addClass("active");
    $("#loading").show();

    var pendingStatus = "Pending Approval";
    var createdStatus = "Saved As Draft";
    $scope.isExistsDocument = "False";
    $("#btnSSITemplateStatusButtons button").addClass("disabled");
    $("#btnUploadSource").removeClass("disabled");


    $scope.fnClearAdvanceSearch = function () {

        if ($("#ssiSearchPane").hasClass("in")) {
            $timeout(function () {
                ssiTemplateTable.searchPanes.clearSelections();
            }, 500);
        }
    }

    $scope.fnGetSSITemplates = function () {
        $http.get("/SSITemplate/GetAllBrokerSsiTemplates").then(function (response) {

            if (response.data.BrokerSsiTemplates.length > 0)
                $("#btnSSITemplateStatusButtons").show();
            $scope.ssiTemplateList = response.data.BrokerSsiTemplates;
            $scope.brokers = response.data.counterParties;
            $scope.serviceProviders = response.data.serviceProviders;


            ssiTemplateTable = $("#ssiTemplateTable").DataTable(
                {
                    aaData: response.data.BrokerSsiTemplates,
                    "bDestroy": true,
                    "dom": "<'#ssiSearchPane.collapse'P><'row'<'col-md-6'i><'col-md-6 pull-right'f>>trI",
                    searchPanes: {
                        cascadePanes: true,
                        viewTotal: true,
                        dataLength: false,
                        //controls: false,
                        layout: 'columns-5',
                        columns: [2, 3, 4, 5, 28],
                        orderable: false,
                        //clear: false
                    },
                    language: {
                        "sSearch": "",
                        "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ Onboarded SSI Templates",
                        "sInfoFiltered": " - filtering from _MAX_ Onboarded SSI Templates",
                    },

                    "fixedColumns": {
                        leftColumns: 2
                    },
                    "columns": [
                        { "mData": "SSITemplate.onBoardingSSITemplateId", "sTitle": "onBoardingSSITemplateId", visible: false },
                        { "mData": "SSITemplate.TemplateName", "sTitle": "Template Name" },
                        {
                            "mData": "SSITemplate.SSITemplateType",
                            "sTitle": "SSI Template Type",
                            render: {
                                _: function (tdata) {
                                    if (tdata === "Broker")
                                        return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Broker</label>";
                                    if (tdata === "Fee/Expense Payment")
                                        return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">Fee/Expense Payment</label>";
                                    return "";
                                },
                                sp: function (tdata) { return tdata; }
                            },

                            searchPanes: {
                                orthogonal: 'sp'
                            }
                        },
                        { "mData": "Broker", "sTitle": "Legal Entity" },
                        { "mData": "AgreementType", "sTitle": "Account Type" },
                        { "mData": "SSITemplate.ServiceProvider", "sTitle": "Service Provider" },
                        { "mData": "SSITemplate.Currency", "sTitle": "Currency" },
                        { "mData": "SSITemplate.ReasonDetail", "sTitle": "Payment/Receipt Reason" },
                        { "mData": "SSITemplate.MessageType", "sTitle": "Message Type" },
                        { "mData": "SSITemplate.BeneficiaryType", "sTitle": "Beneficiary Type" },
                        { "mData": "SSITemplate.Beneficiary", "sTitle": "Beneficiary BIC or ABA", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BICorABA : ""; } },
                        { "mData": "SSITemplate.Beneficiary", "sTitle": "Beneficiary Bank/Account Name", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BankName : ""; } },
                        { "mData": "SSITemplate.Beneficiary", "sTitle": "Beneficiary Bank Address", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BankAddress : ""; } },
                        { "mData": "SSITemplate.BeneficiaryAccountNumber", "sTitle": "Beneficiary Account Number" },
                        { "mData": "SSITemplate.IntermediaryType", "sTitle": "Intermediary Beneficiary Type" },
                        { "mData": "SSITemplate.Intermediary", "sTitle": "Intermediary BIC or ABA", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BICorABA : ""; } },
                        { "mData": "SSITemplate.Intermediary", "sTitle": "Intermediary Bank/Account Name", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BankName : ""; } },
                        { "mData": "SSITemplate.Intermediary", "sTitle": "Intermediary Bank Address", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BankAddress : ""; } },
                        { "mData": "SSITemplate.IntermediaryAccountNumber", "sTitle": "Intermediary Account Number" },
                        { "mData": "SSITemplate.UltimateBeneficiaryType", "sTitle": "Ultimate Beneficiary Type" },
                        { "mData": "SSITemplate.UltimateBeneficiary", "sTitle": "Ultimate Beneficiary BIC or ABA", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BICorABA : ""; } },
                        { "mData": "SSITemplate.UltimateBeneficiary", "sTitle": "Ultimate Beneficiary Bank Name", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BankName : ""; } },
                        { "mData": "SSITemplate.UltimateBeneficiary", "sTitle": "Ultimate Beneficiary Bank Address", "mRender": function (tdata, type, row, meta) { return tdata != null ? tdata.BankAddress : ""; } },
                        { "mData": "SSITemplate.UltimateBeneficiaryAccountName", "sTitle": "Ultimate Beneficiary Account Name" },
                        { "mData": "SSITemplate.AccountNumber", "sTitle": "Ultimate Beneficiary Account Number" },

                        { "mData": "SSITemplate.FFCName", "sTitle": "FFC Name" },
                        { "mData": "SSITemplate.FFCNumber", "sTitle": "FFC Number" },
                        { "mData": "SSITemplate.Reference", "sTitle": "Reference" },
                        {
                            "mData": "SSITemplate.SSITemplateStatus", "sTitle": "SSI Template Status",
                            "mRender": function (tdata) {
                                if (tdata != null && tdata != "undefinied") {
                                    switch (tdata) {
                                        case "Approved": return "<label class='label label-success'>" + tdata + "</label>";
                                        case "Pending Approval": return "<label class='label label-warning'>" + tdata + "</label>";
                                        case "Saved As Draft":
                                        case "Created":
                                            return "<label class='label label-info'>" + "Saved As Draft" + "</label>";
                                    }
                                    return "<label class='label label-default'>" + tdata + "</label>";
                                }
                                return "";
                            },

                        },
                        { "mData": "SSITemplate.StatusComments", "sTitle": "Comments" },
                        {
                            "mData": "SSITemplate.CreatedBy", "sTitle": "Created By",
                            "mRender": function (data) {
                                return humanizeEmail(data);
                            }
                        },
                        {
                            "mData": "SSITemplate.CreatedAt",
                            "sTitle": "Created Date",
                            "mRender": renderDotNetDateAndTime
                        },
                        {
                            "mData": "SSITemplate.UpdatedBy", "sTitle": "Last Modified By",
                            "mRender": function (data) {
                                return humanizeEmail(data);
                            }
                        },
                        {
                            "mData": "SSITemplate.UpdatedAt",
                            "sTitle": "Last Modified",
                            "mRender": renderDotNetDateAndTime
                        },
                        {
                            "mData": "SSITemplate.ApprovedBy", "sTitle": "Approved By", "mRender": function (data) {
                                return humanizeEmail(data == null ? "" : data);
                            }
                        }
                    ],
                    "createdRow": function (row, data) {
                        switch (data.SSITemplate.SSITemplateStatus) {
                            case "Approved":
                                $(row).addClass("success");
                                break;
                            case "Pending Approval":
                                $(row).addClass("warning");
                                break;
                            case "De-Activated":
                                $(row).addClass("blockedSection");
                                break;
                        }

                    },

                    "deferRender": true,
                    "scroller": true,
                    "orderClasses": false,
                    //"scrollX": false,
                    "sScrollX": "100%",
                    //sDom: "ift",
                    "scrollY": window.innerHeight - 350,
                    "sScrollXInner": "100%",
                    "bScrollCollapse": true,
                    "order": [[33, "desc"]],
                    //"bPaginate": false,
                    iDisplayLength: -1
                });

            var searchText = decodeURI(getUrlParameter("searchText"));

            if (searchText != "" && searchText != undefined && searchText != 'undefined') {
                $timeout(function () {
                    ssiTemplateTable.search(searchText).draw(false);
                }, 50);
            } else {
                window.setTimeout(function () {
                    ssiTemplateTable.columns.adjust().draw(true);
                    $("#liBroker").select2({
                        placeholder: "Select Broker",
                        allowClear: true,
                        data: $scope.brokers
                    });
                    $("#liServiceProvider").select2({
                        placeholder: "Select Service Provider",
                        allowClear: true,
                        data: $scope.serviceProviders
                    });
                }, 100);
            }
            $("#loading").hide();
        });
    }

    $scope.isServiceType = false;
    $scope.toggleTemplateType = function () {
        $timeout(function () {
            $scope.isServiceType = $("#chkTemplateType").prop('checked');
        }, 50);

    }
    $scope.isAssociationVisible = false;
    $(".templateType").change(function () {
        var entityId = $(this).select2('val');
        var entityText = $(this).select2('data').text;
        $timeout(function () {
            if (entityId != undefined && entityId != "") {
                var ssiTemplates = $filter('filter')(angular.copy($scope.ssiTemplateList),
                    function (ssi) {
                        return !$scope.isServiceType ? ssi.SSITemplate.TemplateEntityId == entityId : ssi.SSITemplate.ServiceProvider == entityText;
                    }, true);
                $scope.viewAssociationTable(ssiTemplates);
            }
            else
                $scope.isAssociationVisible = false;
        }, 50);
    });

    function hierarchyFormat(aId) {
        return "<div class=\"slider center-block onboardingMapping\" style=\"margin-bottom: 10px !important;\">" +
            "<table id=\"accountRowTable" + aId + "\" class=\"table table-bordered table-condensed\" cellpadding=\"5\" cellspacing=\"0\" border=\"0\" width=\"100%\"></table>" +
            "</div>";
    }

    $scope.viewAssociationTable = function (data) {

        if ($("#accountSSITemplateTable").hasClass("initialized")) {
            fnDestroyDataTable("#accountSSITemplateTable");
        }
        $scope.isAssociationVisible = true;
        accountSsiTemplateTable = $("#accountSSITemplateTable").DataTable(
            {
                aaData: data,
                "bDestroy": true,
                "columns": [
                    { "mData": "SSITemplate.onBoardingSSITemplateId", "sTitle": "onBoardingSSITemplateId", visible: false },
                    {
                        "orderable": false,
                        "data": null,
                        "defaultContent": "<i class=\"glyphicon glyphicon-menu-right\" style=\"cursor:pointer;\"></i>"
                    },
                    {
                        "mData": "Broker", "sTitle": "Broker"
                    },
                    {
                        "mData": "SSITemplate.ServiceProvider", "sTitle": "Service Provider"
                    },
                    { "mData": "SSITemplate.TemplateName", "sTitle": "Template Name" },
                    { "mData": "SSITemplate.AccountNumber", "sTitle": "Account Number" },
                    {
                        "mData": "SSITemplate.SSITemplateStatus", "sTitle": "SSI Template Status",
                        "mRender": function (tdata) {
                            if (tdata != null && tdata != "undefined") {
                                switch (tdata) {
                                    case "Approved": return "<label class='label label-success'>" + tdata + "</label>";
                                    case "Pending Approval": return "<label class='label label-warning'>" + tdata + "</label>";
                                    case "Created": return "<label class='label label-default'>" + "Saved As Draft" + "</label>";
                                }
                                return "<label class='label label-default'>" + tdata + "</label>";
                            }
                            return "";
                        }
                    },
                    {
                        "mData": "SSITemplate.CreatedBy", "sTitle": "Created By", "mRender": function (data) {
                            return humanizeEmail(data);
                        }
                    },
                    {
                        "mData": "SSITemplate.CreatedAt",
                        "sTitle": "Created Date",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    },
                    {
                        "mData": "SSITemplate.UpdatedBy", "sTitle": "Last Modified By", "mRender": function (data) {
                            return humanizeEmail(data);
                        }
                    },
                    {
                        "mData": "SSITemplate.UpdatedAt",
                        "sTitle": "Last Modified",
                        "type": "dotnet-date",
                        "mRender": function (tdata) {
                            return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                        }
                    }
                ],
                "oLanguage": {
                    "sSearch": "",
                    "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ SSI Templates",
                    "sInfoFiltered": " - filtering from _MAX_ SSI Templates"
                },
                "createdRow": function (row, data) {
                    switch (data.SSITemplateStatus) {
                        case "Approved":
                            $(row).addClass("success");
                            break;
                        case "Pending Approval":
                            $(row).addClass("warning");
                            break;
                        case "De-Activated":
                            $(row).addClass("blockedSection");
                            break;
                    }

                },
                //"scrollX": false,
                "deferRender": true,
                // "scroller": true,
                "orderClasses": false,
                "sScrollX": "100%",
                //sDom: "ift",
                "sScrollY": 450,
                "sScrollXInner": "100%",
                "bScrollCollapse": true,
                "order": [[10, "desc"]],
                //"bPaginate": false,
                iDisplayLength: -1
            });


        window.setTimeout(function () {
            accountSsiTemplateTable.columns.adjust().draw(true);
        }, 50);

    }
    $(document).on("click", "#accountSSITemplateTable tbody tr td:first-child ", function () {

        var tr = $(this).parent();
        var row = accountSsiTemplateTable.row(tr);
        if (row.data() != undefined) {

            var ssiTemplate = row.data().SSITemplate;

            var ssiTemplateId = ssiTemplate.onBoardingSSITemplateId;
            var brokerId = ssiTemplate.TemplateEntityId;
            var currency = ssiTemplate.Currency;
            var message = ssiTemplate.MessageType;
            var icon = $(this).find("i");
            if ($("#accountRowTable").hasClass("initialized")) {
                fnDestroyDataTable("#accountRowTable");
            }

            if (row.child.isShown()) {

                // This row is already open - close it           
                $("div.slider", row.child()).slideUp(200, function () {
                    row.child.hide();
                    tr.removeClass("shown");
                });
                icon.addClass("glyphicon-menu-right").removeClass("glyphicon-menu-down");
            } else {

                // Open this row  
                row.child(hierarchyFormat(ssiTemplateId)).show();
                var rowTableId = "#accountRowTable" + ssiTemplateId;
                if ($(rowTableId).hasClass("initialized")) {
                    fnDestroyDataTable(rowTableId);
                }
                $http.get("/Accounts/GetSsiTemplateAccountMap?ssiTemplateId=" + ssiTemplateId + "&brokerId=" + brokerId + "&currency=" + currency + "&message=" + message + "&isServiceType=" + $scope.isServiceType).then(function (response) {
                    tblonBoardAccountRow = $(rowTableId).not(".initialized").addClass("initialized").DataTable(
                        {
                            "bDestroy": true,
                            //responsive: true,
                            aaData: response.data.ssiTemplateMaps,
                            "aoColumns": [
                                { "sTitle": "Account Name", "mData": "AccountName" },
                                {
                                    "mData": "AccountType",
                                    "sTitle": "Account Type",
                                    "mRender": function (tdata) {
                                        if (tdata === "Agreement")
                                            return "<label class=\"label ng-show-only label-info\" style=\"font-size: 12px;\">Agreement</label>";
                                        if (tdata === "DDA")
                                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">DDA</label>";
                                        if (tdata == "Custody")
                                            return "<label class=\"label ng-show-only label-primary\" style=\"font-size: 12px;\">Custody</label>";
                                        return "";
                                    }
                                },
                                { "sTitle": "Account Number", "mData": "AccountNumber", },
                                { "sTitle": "FFC Number", "mData": "FFCNumber", },
                                { "sTitle": "FFC Name", "mData": "FFCName", },
                                {
                                    "sTitle": "Created By", "mData": "CreatedBy", "mRender": function (data) {
                                        return humanizeEmail(data);
                                    }
                                },
                                {
                                    "sTitle": "Updated By", "mData": "UpdatedBy",
                                    "mRender": function (data) {
                                        return humanizeEmail(data);
                                    }
                                },
                                {
                                    "mData": "Status", "sTitle": "Status",
                                    "mRender": function (data) {
                                        if (data === "Pending Approval")
                                            return "<label class=\"label ng-show-only label-warning\" style=\"font-size: 12px;\">Pending Approval</label>";
                                        if (data === "Approved")
                                            return "<label class=\"label ng-show-only label-success\" style=\"font-size: 12px;\">Approved</label>";
                                        if (data === "De-Activated")
                                            return "<label class=\"label ng-show-only label-default\" style=\"font-size: 12px;\">De-Activated</label>";

                                        return "";
                                    }
                                },
                            ],
                            "createdRow": function (row, rowData) {
                                switch (rowData.Status) {
                                    case "Approved":
                                        $(row).addClass("success");
                                        break;
                                    case "Pending Approval":
                                        $(row).addClass("warning");
                                        break;
                                    case "De-Activated":
                                        $(row).addClass("blockedSection");
                                        break;
                                }

                            },
                            "deferRender": false,
                            "bScrollCollapse": true,
                            "bPaginate": false,
                            //"scroller": false,
                            "scrollX": response.data.ssiTemplateMaps.length > 0,
                            "scrollY": "350px",
                            //sortable: false,
                            //"sDom": "ift",
                            //pagination: true,
                            "sScrollX": "100%",
                            "sScrollXInner": "100%",
                            "order": [[0, "desc"]],
                            "oLanguage": {
                                "sSearch": "",
                                "sEmptyTable": "No fund accounts are available for the SSI Template",
                                "sInfo": "Showing _START_ to _END_ of _TOTAL_ SSI Templates"
                            }
                        });

                    //window.setTimeout(function () {
                    //    ssiMapTable.columns.adjust().draw(true);
                    //}, 10);
                    window.setTimeout(function () {
                        tblonBoardAccountRow.columns.adjust().draw(true);
                    }, 50);

                    icon.addClass("glyphicon-menu-down").removeClass("glyphicon-menu-right");
                    $("div.slider", row.child()).slideDown(200, function () {
                        tr.addClass("shown");
                    });
                });
            }
        }
    });

    //Toggle Selection
    $(document).on("click", "#ssiTemplateTable tbody tr,.DTFC_Cloned tbody tr", function () {

        event.stopPropagation();
        event.stopImmediatePropagation();
        event.preventDefault();


        $("#ssiTemplateTable tbody tr,.DTFC_Cloned tbody tr").removeClass("info");

        var $tr = $(this);
        var rowIndx = $tr.index() - $tr.prevAll(":not([role='row'])").length;

        //var groupIndex = $tr.prevAll(".navParent").length - 1;
        //var row = ssiTemplateTable.row(rowIndx);
        var row1 = $("#ssiTemplateTable").find("tr:eq(" + (rowIndx + 1) + ")");
        var row2 = $("table.DTFC_Cloned:eq(1)").find("tr:eq(" + (rowIndx + 1) + ")");

        $(row1).addClass("info");
        $(row2).addClass("info");


        $("#btnSSITemplateStatusButtons button").addClass("disabled");
        var rowElement = ssiTemplateTable.row(this).data().SSITemplate;
        $scope.onBoardingSSITemplateId = rowElement.onBoardingSSITemplateId;
        $scope.listSSITemplateStatus = rowElement.SSITemplateStatus;
        $scope.updatedBy = rowElement.UpdatedBy;

        if (rowElement.SSITemplateStatus == pendingStatus && rowElement.CreatedBy != $("#userName").val() && rowElement.UpdatedBy != $("#userName").val()) {
            $("#btnSSITemplateStatusButtons button[id='approve']").removeClass("disabled");
        }
        if (rowElement.SSITemplateStatus == createdStatus) {
            $("#btnSSITemplateStatusButtons button[id='requestForApproval']").removeClass("disabled");
        }
        if (rowElement.SSITemplateStatus != createdStatus) {
            $("#btnSSITemplateStatusButtons button[id='revert']").removeClass("disabled");
        }
        $scope.$apply();
        // $("#btnSSITemplateStatusButtons button").addClass("disabled");
        $("#btnEdit").prop("disabled", false);

        $http.get("/SSITemplate/IsSsiTemplateDocumentExists?ssiTemplateId=" + $scope.onBoardingSSITemplateId).then(function (response) {
            $scope.isExistsDocument = response.data;
        });


        $("#btnDel").prop("disabled", false);

    });


    $(document).on("dblclick", "#ssiTemplateTable tbody tr,.DTFC_Cloned tbody tr", function () {

        var rowElement = ssiTemplateTable.row(this).data().SSITemplate;
        $scope.onBoardingSSITemplateId = rowElement.onBoardingSSITemplateId;
        var searchText = $('#ssiTemplateListDiv input[type="search"]').val();
        var ssiListUrl = "/SSITemplate/Index?searchText=" + searchText;
        window.history.pushState("", "", ssiListUrl);
        window.location.assign("/SSITemplate/SSITemplate?ssiTemplateId=" + $scope.onBoardingSSITemplateId + "&searchText=" + searchText);
    });

    //SSITemplate buttons approve,pending and revert
    $scope.fnUpdateSSITemplateStatus = function (ssiStatus, statusAction) {
        $scope.SSITemplateStatus = ssiStatus;

        if ((statusAction == "Request for Approval" || statusAction == "Approve") && $scope.isExistsDocument == "False") {
            notifyWarning("Please upload document to approve ssi template");
            return;
        }

        var confirmationMsg = "Are you sure you want to " + ((statusAction === "Request for Approval") ? "<b>request</b> for approval of" : "<b>" + (statusAction == "Revert" ? "save changes or sending approval for" : statusAction) + "</b>") + " the selected SSI Template?";
        $("#btnSaveCommentAgreements").show();
        $("#btnSendApproval").hide();
        if (statusAction == "Request for Approval") {
            $("#btnSaveCommentAgreements").hide();
            $("#btnSendApproval").show();
            $("#btnSaveCommentAgreements").addClass("btn-warning").removeClass("btn-success").removeClass("btn-primary");
            $("#btnSaveCommentAgreements").html('<i class="glyphicon glyphicon-share-alt"></i>&nbsp;Request for approval');
        } else if (statusAction == "Approve") {
            $("#btnSaveCommentAgreements").removeClass("btn-warning").addClass("btn-success").removeClass("btn-primary");
            $("#btnSaveCommentAgreements").html('<i class="glyphicon glyphicon-ok"></i>&nbsp;Approve');
        }
        else if (statusAction == "Revert") {
            $("#btnSaveCommentAgreements").removeClass("btn-warning").removeClass("btn-success").addClass("btn-primary");
            $("#btnSaveCommentAgreements").html('<i class="glyphicon glyphicon-floppy-save"></i>&nbsp;Save Changes');
            $("#btnSendApproval").show();
        }

        $("#pMsg").html(confirmationMsg);
        $("#updateSSITemplateModal").modal("show");
    }

    $scope.fnSaveSSITemplateStatus = function () {
        $http.post("/SSITemplate/UpdateSsiTemplateStatus", { ssiTemplateStatus: $scope.SSITemplateStatus, ssiTemplateId: $scope.onBoardingSSITemplateId, comments: $("#ssiTemplateCommentTextArea").val().trim() }).then(function () {
            notifySuccess("SSI template " + $scope.SSITemplateStatus.toLowerCase() + " successfully");
            window.location.href = "/SSITemplate/Index";
        });
        $("#btnSendApproval").hide();
        $("#updateSSITemplateModal").modal("hide");
    }
    $scope.fnSendApprovalSSITemplateStatus = function () {
        $scope.SSITemplateStatus = pendingStatus;
        $scope.fnSaveSSITemplateStatus();
    }

    $scope.fnEditSSITemplate = function () {
        var searchText = $('#ssiTemplateListDiv input[type="search"]').val();
        var ssiListUrl = "/SSITemplate/Index?searchText=" + searchText;
        window.history.pushState("", "", ssiListUrl);
        window.location.assign("/SSITemplate/SSITemplate?ssiTemplateId=" + $scope.onBoardingSSITemplateId + "&searchText=" + searchText);
    }

    $scope.fnCreateSSITemplate = function () {
        window.location.assign("/SSITemplate/SSITemplate");
    }

    $scope.fnDeleteSSITemplate = function () {
        showMessage("Are you sure do you want to delete ssi template? ", "Delete ssi template", [
            {
                label: "Delete",
                className: "btn btn-sm btn-danger",
                callback: function () {
                    $http.post("/SSITemplate/DeleteSsiTemplate", { ssiTemplateId: $scope.onBoardingSSITemplateId }).then(function () {
                        ssiTemplateTable.row(".info").remove().draw();
                        notifySuccess("Delete successfull");
                        $scope.onBoardingAccountId = 0;
                        $("#btnEdit").prop("disabled", true);
                        $("#btnDel").prop("disabled", true);
                    });
                }
            },
            {
                label: "Cancel",
                className: "btn btn-sm btn-default"
            }
        ]);
    }

    //Export all Accounts
    $scope.fnExportAllSSITemplate = function () {
        window.location.assign("/SSITemplate/ExportAllSsiTemplatelist");
    }

    $scope.fnGetSSITemplates();

    $scope.downloadSSITemplateSample = function () {
        window.location.href = "/SSITemplate/ExportSampleSsiTemplatelist";
    }

    Dropzone.options.myAwesomeDropzone = false;
    Dropzone.autoDiscover = false;

    $("#uploadFiles").dropzone({
        url: "/SSITemplate/UploadSsiTemplate",
        dictDefaultMessage: "<span>Drag/Drop SSI template files to add/update here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".csv,.xls,.xlsx",
        maxFiles: 6,
        previewTemplate: "<div class='row col-sm-2' style='padding: 15px;'><div class='panel panel-success panel-dz'> <div class='panel-heading'> <h3 class='panel-title' style='text-overflow: ellipsis;white-space: nowrap;overflow: hidden;'><span data-dz-name></span> - (<span data-dz-size></span>)</h3> " +
            "</div> <div class='panel-body'> <span class='dz-upload' data-dz-uploadprogress></span>" +
            "<div class='progress'><div data-dz-uploadprogress class='progress-bar progress-bar-warning progress-bar-striped active dzFileProgress' style='width: 0%'></div>" +
            "</div></div></div></div>",

        maxfilesexceeded: function (file) {
            this.removeAllFiles();
            this.addFile(file);
        },
        init: function () {

            this.on("processing", function (file) {
                this.options.url = "/SSITemplate/UploadSsiTemplate";
            });
        },
        processing: function (file, result) {
            $("#uploadFiles").animate({ "min-height": "140px" });
        },
        success: function (file, result) {
            $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
            $(".dzFileProgress").html("Upload Successful");
            $("#loading").show();
            //fnDestroyDataTable("#ssiTemplateTable");
            $scope.fnGetSSITemplates();
        },
        queuecomplete: function () {
        },
        complete: function (file, result) {
            myDropZone = this;
            $("#uploadFiles").removeClass("dz-drag-hover");

            if (this.getRejectedFiles().length > 0 && this.getAcceptedFiles().length === 0 && this.getQueuedFiles().length === 0) {
                showMessage("File format is not supported to upload.", "Status");
                return;
            }

            if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
                notifySuccess("Files Uploaded successfully");
            }
        }
    });

    $("#btnUploadSource").click(function () {
        if (myDropZone != undefined) {
            myDropZone.removeAllFiles(true);
        }
    });

});