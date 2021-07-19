

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
    $scope.pf = {};

    var commitId = localStorage.getItem("SM-HM-Ops-Secure-CommitId");

    if (commitId != null && commitId.length > 0) {
        $scope.pf.username = commitId;
        document.getElementById('hmOpsPassword').focus();
    }
    else
        document.getElementById('hmOpsUserID').focus();

});

function setFocus(loginFailed, rememberUsernameCookieExists, enableRememberUsername, isChainedUsernameAvailable) {
    var platform = navigator.platform;
    if (platform != null && platform.indexOf("iPhone") == -1) {
        if (loginFailed || (rememberUsernameCookieExists && enableRememberUsername) || isChainedUsernameAvailable)
            document.getElementById('hmOpsPassword').focus();
        else
            document.getElementById('hmOpsUserID').focus();
    }
}

function postOk(ok) {
    //trimming the username
    document.getElementById('hmOpsUserID').value = document.getElementById('hmOpsUserID').value.trim();
    document.PWChange["signInBtn"].disabled = true;
    document.PWChange.submit();
}

var wasSubmitted = false;

function postOnReturn(e) {
    if (document.getElementById('hmOpsUserID').value == '') {
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
            document.getElementById('hmOpsUserID').value = document.getElementById('hmOpsUserID').value.trim();
            document.PWChange["signInBtn"].disabled = true;
            document.PWChange.submit();
            return false;
        }
    } else {
        return true;
    }
}

