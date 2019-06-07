$("#liAccounts").addClass("active");
HmOpsApp.controller("SSITemplateListController", function ($scope, $http, $timeout) {
    $("#onboardingMenu").addClass("active");
    $("#loading").show();
    var ssiTemplateTable;
    var myDropZone;
    var approvedStatus = "Approved";
    //var rejectedStatus = "Rejected";
    var pendingStatus = "Pending Approval";
    var createdStatus = "Saved As Draft";
    $scope.isExistsDocument = "False";
    $("#btnSSITemplateStatusButtons button").addClass("disabled");
    $scope.fnGetSSITemplates = function () {
        $http.get("/Accounts/GetAllBrokerSsiTemplates").then(function (response) {

            if (response.data.BrokerSsiTemplates.length > 0)
                $("#btnSSITemplateStatusButtons").show();

            ssiTemplateTable = $("#ssiTemplateTable").DataTable(
           {
               aaData: response.data.BrokerSsiTemplates,
               "bDestroy": true,
               "fixedColumns": {
                   leftColumns: 2
               },
               "columns": [
                  { "mData": "onBoardingSSITemplateId", "sTitle": "onBoardingSSITemplateId", visible: false },
                  { "mData": "TemplateName", "sTitle": "Template Name" },
                  {
                      "mData": "SSITemplateType",
                      "sTitle": "SSI Template Type",
                      "mRender": function (tdata) {
                          if (tdata === "Broker")
                              return "<label class=\"label ng-show-only shadowBox label-info\" style=\"font-size: 12px;\">Broker</label>";
                          if (tdata === "Fee/Expense Payment")
                              return "<label class=\"label ng-show-only shadowBox label-default\" style=\"font-size: 12px;\">Fee/Expense Payment</label>";
                          return "";
                      }
                  },
                  { "mData": "Broker", "sTitle": "Legal Entity" },
                  { "mData": "AgreementType", "sTitle": "Account Type" },
                  { "mData": "ServiceProvider", "sTitle": "Service Provider" },
                  { "mData": "Currency", "sTitle": "Currency" },
                  { "mData": "ReasonDetail", "sTitle": "Payment/Receipt Reason" },
                  { "mData": "MessageType", "sTitle": "Message Type" },
                 // { "mData": "InstructionType", "sTitle": "Instruction Type" },
                 //// { "mData": "Account", "sTitle": "Account" },

                 // { "mData": "InstName", "sTitle": "Account with Inst Name" },
                 // { "mData": "InstBIC", "sTitle": "Account with Inst BIC" },
                 // //{ "mData": "InstABA", "sTitle": "Account with Inst ABA" },
                 // { "mData": "BeneficiaryName", "sTitle": "Beneficiary Name" },
                 // { "mData": "BeneficiaryBIC", "sTitle": "Beneficiary BIC" },
                 // { "mData": "BeneficiaryAccount", "sTitle": "Beneficiary Account" },

                  //{ "mData": "FurtherCredit", "sTitle": "FFC Name" },
                  //{ "mData": "FurtherCreditNumber", "sTitle": "FFC Number" },
                  { "mData": "BeneficiaryType", "sTitle": "Beneficiary Type" },
                  { "mData": "BeneficiaryBICorABA", "sTitle": "Beneficiary BIC or ABA" },
                  { "mData": "BeneficiaryBankName", "sTitle": "Beneficiary Bank/Account Name" },
                  { "mData": "BeneficiaryBankAddress", "sTitle": "Beneficiary Bank Address" },
                  { "mData": "BeneficiaryAccountNumber", "sTitle": "Beneficiary Account Number" },
                  { "mData": "IntermediaryType", "sTitle": "Intermediary Beneficiary Type" },
                  { "mData": "IntermediaryBICorABA", "sTitle": "Intermediary BIC or ABA" },
                  { "mData": "IntermediaryBankName", "sTitle": "Intermediary Bank/Account Name" },
                  { "mData": "IntermediaryBankAddress", "sTitle": "Intermediary Bank Address" },
                  { "mData": "IntermediaryAccountNumber", "sTitle": "Intermediary Account Number" },
                  { "mData": "UltimateBeneficiaryType", "sTitle": "Ultimate Beneficiary Type" },
                  { "mData": "UltimateBeneficiaryBICorABA", "sTitle": "Ultimate Beneficiary BIC or ABA" },
                  { "mData": "UltimateBeneficiaryBankName", "sTitle": "Ultimate Beneficiary Bank Name" },
                  { "mData": "UltimateBeneficiaryBankAddress", "sTitle": "Ultimate Beneficiary Bank Address" },
                  { "mData": "UltimateBeneficiaryAccountName", "sTitle": "Ultimate Beneficiary Account Name" },
                  { "mData": "AccountNumber", "sTitle": "Ultimate Beneficiary Account Number" },

                  { "mData": "FFCName", "sTitle": "FFC Name" },
                  { "mData": "FFCNumber", "sTitle": "FFC Number" },
                  { "mData": "Reference", "sTitle": "Reference" },
                  {
                      "mData": "SSITemplateStatus", "sTitle": "SSI Template Status",
                      "mRender": function (tdata) {
                          if (tdata != null && tdata != "undefinied") {
                              switch (tdata) {
                                  case "Approved": return "<label class='label label-success'>" + tdata + "</label>";
                                  case "Pending Approval": return "<label class='label label-warning'>" + tdata + "</label>";
                                  case "Saved As Draft": return "<label class='label label-default'>" + "Saved As Draft" + "</label>";
                              }
                              return "<label class='label label-default'>" + tdata + "</label>";
                          }
                          return "";
                      }
                  },
                  { "mData": "StatusComments", "sTitle": "Comments" },
                  {
                      "mData": "CreatedBy", "sTitle": "Created By",
                      "mRender": function (data) {
                          return humanizeEmail(data);
                      }
                  },
                 {
                     "mData": "CreatedAt",
                     "sTitle": "Created Date",
                     "type": "dotnet-date",
                     "mRender": function (tdata) {
                         return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                     }
                 },
                 {
                     "mData": "UpdatedBy", "sTitle": "Last Modified By",
                     "mRender": function (data) {
                         return humanizeEmail(data);
                     }
                 },
                 {
                     "mData": "UpdatedAt",
                     "sTitle": "Last Modified",
                     "type": "dotnet-date",
                     "mRender": function (tdata) {
                         return "<div  title='" + getDateForToolTip(tdata) + "' date='" + tdata + "'>" + getDateForToolTip(tdata) + "</div>";
                     }
                 }
               ],
               "createdRow": function (row, data) {
                   switch (data.SSITemplateStatus) {
                       case "Approved":
                           $(row).addClass("success");
                           break;
                       case "Pending Approval":
                           $(row).addClass("warning");
                           break;
                   }

               },
               "oLanguage": {
                   "sSearch": "",
                   "sInfo": "&nbsp;&nbsp;Showing _START_ to _END_ of _TOTAL_ Onboarded SSI Templates",
                   "sInfoFiltered": " - filtering from _MAX_ Onboarded SSI Templates"
               },
               "deferRender": true,
               "scroller": true,
               "orderClasses": false,
               //"scrollX": false,
               "sScrollX": "100%",
               //sDom: "ift",
               "sScrollY": $("#ssiTemplateListDiv").offset().top + 350,
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
                }, 100);
            }
            $("#loading").hide();
        });
    }

    $(document).on("click", "#ssiTemplateTable tbody tr ", function () {
        $("#ssiTemplateTable tbody tr").removeClass("info");
        if (!$(this).hasClass("info")) {
            $(this).addClass("info");
        }
        $("#btnSSITemplateStatusButtons button").removeClass("disabled");
        var rowElement = ssiTemplateTable.row(this).data();
        $scope.onBoardingSSITemplateId = rowElement.onBoardingSSITemplateId;
        $scope.listSSITemplateStatus = rowElement.SSITemplateStatus;
        $scope.updatedBy = rowElement.UpdatedBy;
        $scope.$apply();
        // $("#btnSSITemplateStatusButtons button").addClass("disabled");
        $("#btnEdit").prop("disabled", false);

        //var selectedRow = agreementTable.row('.info').data();
        //if (rowElement.SSITemplateStatus == pendingStatus && rowElement.UpdatedBy != $("#userName").val()) {
        //    $("#btnSSITemplateStatusButtons button[id='approve']").removeClass("disabled");
        //}
        //if (rowElement.SSITemplateStatus == createdStatus) {
        //    $("#btnSSITemplateStatusButtons button[id='requestForApproval']").removeClass("disabled");
        //}
        //if (rowElement.SSITemplateStatus != createdStatus) {
        //    $("#btnSSITemplateStatusButtons button[id='revert']").removeClass("disabled");
        //}

        $http.get("/Accounts/IsSsiTemplateDocumentExists?ssiTemplateId=" + $scope.onBoardingSSITemplateId).then(function (response) {
            $scope.isExistsDocument = response.data;
        });


        $("#btnDel").prop("disabled", false);
    });

    $(document).on("dblclick", "#ssiTemplateTable tbody tr", function () {

        var rowElement = ssiTemplateTable.row(this).data();
        $scope.onBoardingSSITemplateId = rowElement.onBoardingSSITemplateId;
        var searchText = $('#ssiTemplateListDiv input[type="search"]').val();
        var ssiListUrl = "/Accounts/SSITemplateList?searchText=" + searchText;
        window.history.pushState("", "", ssiListUrl);
        window.location.assign("/Accounts/SSITemplate?ssiTemplateId=" + $scope.onBoardingSSITemplateId + "&searchText=" + searchText);
    });

    //SSITemplate buttons approve,pending and revert
    $scope.fnUpdateSSITemplateStatus = function (ssiStatus, statusAction) {
        $scope.SSITemplateStatus = ssiStatus;

        if ((statusAction == "Request for Approval" || statusAction == "Approve") && $scope.isExistsDocument == "False") {
            notifyWarning("Please upload document to approve ssi template");
            return;
        }

        var confirmationMsg = "Are you sure you want to " + ((statusAction === "Request for Approval") ? "<b>request</b> for approval of" : "<b>" + (statusAction == "Revert" ? "save changes or sending approval for" : statusAction) + "</b>") + " the selected SSI Template?";
        if (statusAction == "Request for Approval") {
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
        $http.post("/Accounts/UpdateSsiTemplateStatus", { ssiTemplateStatus: $scope.SSITemplateStatus, ssiTemplateId: $scope.onBoardingSSITemplateId, comments: $("#ssiTemplateCommentTextArea").val().trim() }).then(function () {
            notifySuccess("SSI template " + $scope.SSITemplateStatus.toLowerCase() + " successfully");
            window.location.href = "/Accounts/SSITemplateList";
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
        var ssiListUrl = "/Accounts/SSITemplateList?searchText=" + searchText;
        window.history.pushState("", "", ssiListUrl);
        window.location.assign("/Accounts/SSITemplate?ssiTemplateId=" + $scope.onBoardingSSITemplateId + "&searchText=" + searchText);
    }

    $scope.fnCreateSSITemplate = function () {
        window.location.assign("/Accounts/SSITemplate");
    }

    $scope.fnDeleteSSITemplate = function () {
        showMessage("Are you sure do you want to delete ssi template? ", "Delete ssi template", [
                 {
                     label: "Delete",
                     className: "btn btn-sm btn-danger",
                     callback: function () {
                         $http.post("/Accounts/DeleteSsiTemplate", { ssiTemplateId: $scope.onBoardingSSITemplateId }).then(function () {
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
        window.location.assign("/Accounts/ExportAllSsiTemplatelist");
    }

    $scope.fnGetSSITemplates();

    $scope.downloadSSITemplateSample = function () {
        window.location.href = "/Accounts/ExportSampleSsiTemplatelist";
    }

    Dropzone.options.myAwesomeDropzone = false;
    Dropzone.autoDiscover = false;

    $("#uploadFiles").dropzone({
        url: "/Accounts/UploadSsiTemplate",
        dictDefaultMessage: "<span>Drag/Drop SSI template files to add/update here&nbsp;<i class='glyphicon glyphicon-download-alt'></i></span>",
        autoDiscover: false,
        acceptedFiles: ".csv,.xls,.xlsx",
        maxFiles: 6,
        previewTemplate: "<div class='row col-sm-2'><div class='panel panel-success panel-dz'> <div class='panel-heading'> <h3 class='panel-title' style='text-overflow: ellipsis;white-space: nowrap;overflow: hidden;'><span data-dz-name></span> - (<span data-dz-size></span>)</h3> " +
            "</div> <div class='panel-body'> <span class='dz-upload' data-dz-uploadprogress></span>" +
            "<div class='progress'><div data-dz-uploadprogress class='progress-bar progress-bar-warning progress-bar-striped active dzFileProgress' style='width: 0%'></div>" +
            "</div></div></div></div>",

        maxfilesexceeded: function (file) {
            this.removeAllFiles();
            this.addFile(file);
        },
        init: function () {

            this.on("processing", function (file) {
                this.options.url = "/Accounts/UploadSsiTemplate";
            });
        },
        processing: function (file, result) {
            $("#uploadFiles").animate({ "min-height": "140px" });
        },
        success: function (file, result) {
            $(".dzFileProgress").removeClass("progress-bar-striped").removeClass("active").removeClass("progress-bar-warning").addClass("progress-bar-success");
            $(".dzFileProgress").html("Upload Successful");
            $("#loading").show();
            fnDestroyDataTable("#ssiTemplateTable");
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