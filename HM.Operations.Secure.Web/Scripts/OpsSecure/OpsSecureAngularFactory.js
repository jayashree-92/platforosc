/// <reference path="../data.js" />
var HmOpsApp = angular.module("HmOperationsSecureApp", ["ngAnimate", "ngMessages", "ngSanitize"]);

HmOpsApp.directive("onLastRepeat", function () {
    return function (scope, element, attrs) {
        if (scope.$last)
            setTimeout(function () {
                scope.$emit("onRepeatLast", element, attrs);
            }, 1);
    };
});

HmOpsApp.directive("multipleEmails", function () {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, element, attrs, ctrl) {
            function validateMultipleEmails(value) {
                var emailRgx = /^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*[,]{0,1}\s*)+$/;
                var validity = ctrl.$isEmpty(value) || value.split(",").every(
                    function (email) {
                        return emailRgx.test(email.trim());
                    });
                ctrl.$setValidity("multipleEmails", validity);
                return validity ? value : undefined;
            }
            ctrl.$formatters.push(validateMultipleEmails);
            ctrl.$parsers.push(validateMultipleEmails);
        }
    }
})

HmOpsApp.factory("$opsSharedScopes", function ($rootScope) {
    var mem = {};

    return {
        store: function (key, value) {
            mem[key] = value;
        },
        get: function (key) {
            return mem[key];
        }
    };
});


HmOpsApp.factory("$opsSignalHq", function ($window, $rootScope) {

    var localStorageKey = "hm-ops-secure-signal-storage-local";
    var sessionStorageKey = "hm-ops-secure-signal-storage-session";
    var localMessageKey = "hm-ops-secure-signal-message-store";

    var getSessionDataInFactory = function () {
        var storeData = $window.sessionStorage && $window.sessionStorage.getItem(sessionStorageKey);
        return JSON.parse(storeData);
    }
    var getLocalDataInFactory = function () {
        var localData = $window.localStorage && $window.localStorage.getItem(localStorageKey);
        if (localData == "" || localData == "\"\"" || localData == undefined || localData == "0")
            return [];

        return JSON.parse(localData);
    }

    var fnBroadCastToAllTabs = function () {
        var localData = JSON.stringify(new UniqueIdGenerator().getId());
        $window.localStorage && $window.localStorage.setItem(localMessageKey, localData);
    }

    angular.element($window).on("storage", function (event) {
        var allTabs = getLocalDataInFactory();

        if (event.originalEvent.key === localStorageKey || event.originalEvent.key === sessionStorageKey) {
            $rootScope.$apply();

            //Check if sentinal is lost and contact next available sentinal

            var isSentinalAvailable = false;
            for (var i = 0; i < allTabs.length; i++) {
                if (allTabs[i].IsConnected && allTabs[i].UserId == $("#userName").val())
                    isSentinalAvailable = true;
            }

            if (!isSentinalAvailable) {

                for (var i = 0; i < allTabs.length; i++) {
                    if (!allTabs[i].IsConnected) {
                        $rootScope.$broadcast("assignSentinal-" + allTabs[i].TabId);
                        return;
                    }
                }
            }
        }

        if (event.originalEvent.key === localMessageKey) {
            for (var i = 0; i < allTabs.length; i++) {
                if (!allTabs[i].IsConnected) {
                    $rootScope.$broadcast("generalReceiver-" + allTabs[i].TabId);
                }
            }
        }

    });

    return {

        RemoveLocalData: function () {
            $window.localStorage && $window.localStorage.removeItem(localStorageKey);
        },

        RemoveSessionData: function () {
            $window.sessionStorage && $window.sessionStorage.removeItem(sessionStorageKey);
        },

        SetLocalData: function (object) {
            var localData = JSON.stringify(object);
            $window.localStorage && $window.localStorage.setItem(localStorageKey, localData);
            return this;
        },

        SetSessionData: function (object) {
            var sessionData = JSON.stringify(object);
            $window.sessionStorage && $window.sessionStorage.setItem(sessionStorageKey, sessionData);
            return this;
        },
        IsCurrentTabConnected: function () {
            var allTabs = getLocalDataInFactory();

            for (var i = 0; i < allTabs.length; i++) {
                if (allTabs[i].IsConnected) {
                    return allTabs[i].TabId === getSessionDataInFactory();
                }
            }
            return false;
        },
        GetLocalData: getLocalDataInFactory,
        GetSessionData: getSessionDataInFactory,
        BroadCastToAllTabs: fnBroadCastToAllTabs
    };
});
