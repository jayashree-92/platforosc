

var localStorageSessionTimeOutKey = "hm-ops-secure-session-timeout-local";
var localStorageTotalNoOfTabsOpened = "hm-ops-secure-total-tabs-opened-local";

$(document).ready(function () {

    //________________________________________SESSION MANAGEMENT__________________________________________//


    localStorage.removeItem(localStorageSessionTimeOutKey);

    window.addEventListener("load", function (e) {
        var totalTabsOpened = localStorage.getItem(localStorageTotalNoOfTabsOpened);
        if (totalTabsOpened) {
            totalTabsOpened++;
            localStorage.setItem(localStorageTotalNoOfTabsOpened, totalTabsOpened);
        } else {
            localStorage.setItem(localStorageTotalNoOfTabsOpened, 1);
        }
        //render();
    });

    window.addEventListener("unload", function (e) {
        e.preventDefault();
        var totalTabsOpened = localStorage.getItem(localStorageTotalNoOfTabsOpened);
        if (totalTabsOpened && totalTabsOpened > 0) {
            totalTabsOpened--;
            localStorage.setItem(localStorageTotalNoOfTabsOpened, totalTabsOpened);
        }
        e.returnValue = "";
    });

    var idleSecondsCounter = 0;

    var idleTimeout = 60 * 20; //20 minutes
    var idleTimeoutWarning = 60 * 18; //18th minute - 2 mins before expiry

    //var idleTimeout = 20; //20 minutes
    //var idleTimeoutWarning = 10; //18th minute - 2 mins before expiry

    var isWarningMessgeShown = false;

    function setLocalSessionIdleTime(idleSecond, isContinue) {
        if (isContinue == undefined)
            isContinue = false;

        if (isContinue)
            isWarningMessgeShown = false;

        //if warning message is shown - the counter will not reset to zero and force u  ser to logout unless a reload button is hit
        if (idleSecond === 0 && isWarningMessgeShown)
            return;

        localStorage.setItem(localStorageSessionTimeOutKey, idleSecond);
        idleSecondsCounter = parseFloat(localStorage.getItem(localStorageSessionTimeOutKey));
    }

    setLocalSessionIdleTime(0);

    document.onclick = function () {
        setLocalSessionIdleTime(0);
        //heartbeat();
    };
    document.onmousemove = function () {
        setLocalSessionIdleTime(0);
        //heartbeat();
    };
    document.onkeypress = function () {
        setLocalSessionIdleTime(0);
        //heartbeat();
    };

    //Set interval every second
    var myInterval = window.setInterval(checkIdleTime, 1000);
    var thisBootbox;

    function checkIdleTime() {

        var totalTabsOpened = localStorage.getItem(localStorageTotalNoOfTabsOpened);

        if (totalTabsOpened == null || totalTabsOpened <= 0) {
            totalTabsOpened = 1;
            localStorage.setItem(localStorageTotalNoOfTabsOpened, totalTabsOpened);
        }

        idleSecondsCounter = parseFloat(localStorage.getItem(localStorageSessionTimeOutKey));
        setLocalSessionIdleTime(idleSecondsCounter + (1 / totalTabsOpened));

        var oPanel = document.getElementById("SecondsUntilExpire");
        if (oPanel) {
            var timeInSeconds = idleTimeout - idleSecondsCounter;
            oPanel.innerHTML = (parseInt((timeInSeconds / 60)) + ":" + parseInt((timeInSeconds % 60))) + "";
        }

        if (idleSecondsCounter >= idleTimeout) {
            window.clearInterval(myInterval);
            window.location.href = "/Account/LogOff?returnUrl=" + escape(window.location.pathname + window.location.search);
        }
        else if (idleSecondsCounter >= idleTimeoutWarning) {

            if (isWarningMessgeShown)
                return;

            var message = "Your session will expire in another <span id=\"SecondsUntilExpire\"></span> minute(s). <br/ > If you have any un-saved data, please click continue to extend the session <i>(Ctrl+Shift+R)</i>.";

            thisBootbox = bootbox.dialog({
                message: message,
                title: "Session Expiry Warning",
                buttons: {
                    confirm: {
                        label: "Continue Session",
                        className: "btn-primary",
                        callback: function () {
                            //window.location.reload();
                            setLocalSessionIdleTime(0, true);
                        }
                    },
                    cancel: {
                        label: "Cancel",
                        className: "btn-default",
                        callback: function () {
                        }
                    }
                }
            });

            isWarningMessgeShown = true;
        }
        else {

            if (thisBootbox)
                thisBootbox.modal("hide");
        }
    }
    //__________________________________________________________________________________//

    $("#btnLogOff").on("click", function () {
        window.location.href = "/Account/LogOff";
    });
});

HmOpsApp.controller("layoutSettingsCtrl", function ($scope, $http, $timeout) {

    $scope.fnGetAllowedWireAmount =function() {
         $http.get("/Home/GetUserDetails").then(
            function (response) {
                 $scope.AllowedWireAmountLimit = $.convertToCurrency(response.data.AllowedWireAmountLimit, true);;
                 $scope.UserDetail = response.data;
            });
    }
    $scope.fnGetAllowedWireAmount();

    $scope.fnHideUserSettings = function() {
        $("#userSettings").collapse("hide");
    }

    $scope.fnShowUserSettings = function () {

        //$scope.fnGetAllowedWireAmount();
        if ($("#liMainNavUserSettings").hasClass("active"))
            $("#userSettings").collapse("toggle");
        else
            $("#userSettings").collapse("show");

        $("#liMainNavUserSettings").parent().find("li").removeClass("active");
        $("#liMainNavUserSettings").addClass("active");

        $("#userSettings .nav-tabs li:eq(2) a").tab("show");
    }

});

