

// Cross-frame scripting prevention: This code will prevent this page from being encapsulated within HTML frames. Remove, or comment out, this code if the functionality that is contained in this SiteMinder page is to be included within HTML frames. -->


if (self == top) {
    document.documentElement.style.display = 'block';
    document.documentElement.style.visibility = 'visible';
}
else {
    top.location = self.location;
}

//<!--   end - frame busting script -->

//<!--redirect to error.html if not true -->

function initForm() {
    if (!validate()) {
        top.location = ("/login/forms/Error.html");
    }
}


//<!-- Validate all the function before proceeding -->	
function validate() {

    if (!isValidURL(window.location.toString())) {
        return false;
    }
    if (!validateSMAUTHREASON()) {
        return false;
    }
    else if (!validateMETHOD()) {
        return false;
    }
    else {
        resetCredFields();
        return true;
    }
}


//<!-- reset the fields -->
function resetCredFields() {
    document.Login.PASSWRD.value = "";
    document.Login.USER.value = "";
}


function validateSMAUTHREASON() {
    if ("$$SMAUTHREASON$$" == "") {
        return true;
    }
    else if ("$$SMAUTHREASON$$" <= 51 && "$$SMAUTHREASON$$" >= 0) {

        return true;
    }
    else {
        return false;
    }
}

//<!--   METHOD validation script.  Add additional methods if approved by IRM and policy updated. -->
function validateMETHOD() {

    if ("$$METHOD$$" == "") {
        return true;
    }
    else if ("$$METHOD$$" == "GET") {
        return true;
    }
    else if ("$$METHOD$$" == "POST") {
        return true;
    }
    else {
        return false;
    }
}


//<!-- validate the UserID for Bad CSS chars 	--> 
function validateInput(input) {
    return !input.match(/^[<>'\"]+$/);
}

//<!-- validate the requested url has bad chars -->  
function isValidURL(data) {
    return !data.match(/.*[~<>'\"\\s].*/) && !data.match(/^\\p{Cntrl}*$/) && !data.match(/[\\x7F-\\xFF]/);
}


//<!-- validate the form data for empty String or alphaneumeric userid --> 
function validateForm() {

    if (document.Login.USER.value == "") {
        alert("Field entry is required.");
        document.Login.USER.focus();
        return false;
    }
    else if (document.Login.PASSWRD.value == "") {
        alert("Field entry is required.");
        document.Login.PASSWRD.focus();
        return false;
    }
    else if (!validateInput(document.Login.USER.value)) {
        alert('Please enter a vaild user id');

        return false;
    }
    return true;
}



//<!-- validate the form data and submit if valid -->
function submitForm() {
    localStorage.setItem("SM-HM-Ops-Secure-CommitId", document.forms[0].USER.value);
    if (validateForm()) {
        document.Login.submit();
        return true;
    }
    else {
        return false;
    }
}

//<!-- Display Pop-Up window for help info. -->


function displayWindow(url, width, height) {
    var Win = window.open(url, "displayWindow", 'width=' + width + ',height=' + height + ',left=50,top=50,resizable=yes,scrollbars=yes,menubar=no,status=no');
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

var app = angular.module("loginApp", ["ngAnimate"]);
app.controller('loginController', function ($scope) {
    $scope.footerOpts = {
        displaySysRequirements: false,
        displayCookieCompliance: true,
        linkPrivacyPolicy: 'https://www.bnymellon.com/us/en/privacy.jsp',
        linkTermsOfUse: 'https://www.bnymellon.com/us/en/terms-of-use.jsp',
        linkCookieCompliance: '/assets/bnym/cookie.landing.html'
    };
});

function setFocus(loginFailed, rememberUsernameCookieExists, enableRememberUsername, isChainedUsernameAvailable) {
    var platform = navigator.platform;
    if (platform != null && platform.indexOf("iPhone") == -1) {
        if (loginFailed || (rememberUsernameCookieExists && enableRememberUsername) || isChainedUsernameAvailable)
            document.getElementById('hmOpsPassword').focus();
        else
            document.getElementById('username').focus();
    }
}

function postForgotPassword(enableRememberUsername, passwordReset, forgotPasswordUrl) {
    var target = forgotPasswordUrl;
    if (enableRememberUsername)
        if (document.getElementById('rememberUsername').checked) {
            target += "&prEnableRememberUsername";
        }

    document.forms[0].action = target;
    document.forms[0][passwordReset].value = 'clicked';
    document.forms[0].submit();
    document.Login.submit();
    return true;
}

function postOk(ok) {
    //trimming the username
    document.getElementById('username').value = document.getElementById('username').value.trim();
    localStorage.setItem("SM-HM-Ops-Secure-CommitId", document.forms[0].USER.value);
    document.forms[0]["signInBtn"].disabled = true;
    document.forms[0].submit();
}

var wasSubmitted = false;

function postOnReturn(e) {
    if (document.getElementById('username').value == '') {
        return true;
    }
    if (document.getElementById('hmOpsPassword').value == '') {
        return true;
    }
    var keycode;
    if (window.event) keycode = window.event.keyCode;
    else if (e) keycode = e.which;
    else return true;
    if (keycode == 13) {
        if (!wasSubmitted) {
            wasSubmitted = true;
            //trimming the username
            postOk();
            return false;
        }
    } else {
        return true;
    }
}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

$(document).ready(function () {

    var arr = [];

    for (var i = 0; i < localStorage.length; i++) {
        if (localStorage.key(i).indexOf('hm-ops-signal-') === 0) {
            arr.push(localStorage.key(i));
        }
    }

    for (var i = 0; i < arr.length; i++) {
        localStorage.removeItem(arr[i]);
    }


    $("#hmOpsPassword").on("keypress", function (e) {
        var s = String.fromCharCode(e.which);
        if ((s.toUpperCase() === s && s.toLowerCase() !== s && !e.shiftKey) || (s.toUpperCase() !== s && s.toLowerCase() === s && e.shiftKey)) {
            $("#hmOpsPassword").popover("show");
        } else if ((s.toLowerCase() === s && s.toUpperCase() !== s && !e.shiftKey) || (s.toLowerCase() !== s && s.toUpperCase() === s && e.shiftKey)) {
            $("#hmOpsPassword").popover("hide");
        }
    });

    //Toggle warning message on Caps-Lock toggle (with some limitation)
    $("#hmOpsPassword").keydown(function (e) {
        if (e.which == 20) {
            $("#hmOpsPassword").popover("toggle");
        }
    });

    $("#hmOpsPassword").on("focusout", function () {
        $("#hmOpsPassword").popover("hide");
    });

});

