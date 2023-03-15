using Com.HedgeMark.Commons.Extensions;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.DataModel.Models;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Middleware.Util;
using HM.Operations.Secure.Web.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Web.Mvc;

namespace HM.Operations.Secure.Web.Controllers
{
    public enum OpsSecureSessionVars
    {
        AuthorizedUserData,
        AuthorizedFundData,
        UserPreferencesInSession,
        WiresDashboardData,
        WireUserGroupData,
        ClearingBrokersData
    }


    [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)]
    [Authorize, RedirectToHttps, Compress]
    public abstract class BaseController : Controller
    {
        private string thisUserName;
        public string UserName
        {
            get => thisUserName ?? User.Identity.Name;
            set => thisUserName = value;
        }

        public int UserId => UserDetails.Id;

        public UserAccountDetails UserDetails
        {
            get
            {
                if(GetSessionValue("UserDetails") is UserAccountDetails userDetails)
                    return userDetails;

                userDetails = AccountController.GetUserDetails(User);
                SetSessionValue("UserDetails", userDetails);
                return userDetails;
            }
        }

        public object GetSessionValue(string key)
        {
            return Session[key];
        }

        public void SetSessionValue(string key, object value)
        {
            Session[key] = value;
        }

        public bool IsWireApprover => User.IsInRole(OpsSecureUserRoles.WireApprover);
        public bool IsWireReadOnly => User.IsInRole(OpsSecureUserRoles.WireReadOnly);
        public bool IsWireAdmin => User.IsInRole(OpsSecureUserRoles.WireAdmin);


        protected static readonly string JsonContentType = "application/json";
        protected static readonly Encoding JsonContentEncoding = Encoding.UTF8;
        protected new JsonResult Json(object data)
        {
            return Json(data, JsonContentType, JsonContentEncoding);
        }

        protected new JsonResult Json(object data, JsonRequestBehavior behavior)
        {
            return Json(data, JsonContentType, JsonContentEncoding);
        }

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType, //  "application/json"
                ContentEncoding = contentEncoding, // Encoding.UTF8,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue,
            };
        }

        public FileContentResult DownloadAndDeleteFile(FileInfo fileInfo, string downloadName = "")
        {
            var validFileFullName = fileInfo.FullName.GetValidatedConfigPath();

            if(!System.IO.File.Exists(validFileFullName))
                return null;

            FileContentResult fileContent;
            using(var fileStream = System.IO.File.Open(validFileFullName, FileMode.Open, FileAccess.Read))
            {
                var returnBytes = new byte[fileStream.Length];
                fileStream.Read(returnBytes, 0, returnBytes.Length);
                fileContent = File(returnBytes, MediaTypeNames.Application.Octet, string.IsNullOrWhiteSpace(downloadName) ? fileInfo.Name : downloadName);
            }

            //delete will happen only if the file is in the Temp directory - which is often the case of file creation - this is to avoid junk file creation
            if(fileInfo.Directory != null && FileSystemManager.UploadTemporaryFilesPath.Contains(fileInfo.Directory.FullName))
                fileInfo.Delete();

            return fileContent;
        }

        protected FileResult DownloadFile(FileInfo fileInfo, string downloadName)
        {
            var validFileFullName = fileInfo.FullName.GetValidatedConfigPath();

            if(!System.IO.File.Exists(validFileFullName))
                return null;

            using(var fileStream = System.IO.File.Open(validFileFullName, FileMode.Open, FileAccess.Read))
            {
                var returnBytes = new byte[fileStream.Length];
                fileStream.Read(returnBytes, 0, returnBytes.Length);
                return File(returnBytes, MediaTypeNames.Application.Octet, string.IsNullOrWhiteSpace(downloadName) ? fileInfo.Name : downloadName);
            }
        }
    }

    [AuthorizedRoles(OpsSecureUserRoles.WireAdmin)]
    public abstract class WireAdminBaseController : BaseController
    {

    }

    [AuthorizedRoles(OpsSecureUserRoles.WireInitiator, OpsSecureUserRoles.WireApprover, OpsSecureUserRoles.WireReadOnly, OpsSecureUserRoles.WireAdmin)]
    public abstract class WireUserBaseController : BaseController
    {
        public List<dmaUserPreference> UserPreferencesInSession
        {
            get
            {
                var preferncesKey = $"{UserId}{OpsSecureSessionVars.UserPreferencesInSession}";
                if(GetSessionValue(preferncesKey) != null)
                    return (List<dmaUserPreference>)GetSessionValue(preferncesKey);

                var allPreferences = PreferencesManager.GetAllUserPreferences(UserId);
                SetSessionValue(preferncesKey, allPreferences);
                return allPreferences;
            }
        }

        protected string GetPreferenceInSession(string key, string defaultValue = "")
        {
            var allPreferences = UserPreferencesInSession;
            return allPreferences.Any(s => s.Key == key) ? allPreferences.First(s => s.Key == key).Value : defaultValue;
        }

        public void SavePreferenceInSession(string key, string value)
        {
            PreferencesManager.SaveUserPreferences(UserDetails.Id, key, value);
            ResetPreferencesInSession();
        }

        private void ResetPreferencesInSession()
        {
            var preferencesKey = $"{UserId}{OpsSecureSessionVars.UserPreferencesInSession}";
            SetSessionValue(preferencesKey, null);
        }


        public PreferencesManager.FundNameInDropDown PreferredFundNameInSession => (PreferencesManager.FundNameInDropDown)GetPreferenceInSession(PreferencesManager.ShowRiskOrShortFundNames).ToInt(0);

        public AuthorizedData AuthorizedSessionData
        {
            get
            {
                var preferncesKey = $"{UserName}{OpsSecureSessionVars.AuthorizedUserData}";
                if(GetSessionValue(preferncesKey) != null)
                    return (AuthorizedData)GetSessionValue(preferncesKey);
                var userRole = ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, OpsSecureUserRoles.DMAAdmin) ? OpsSecureUserRoles.DMAAdmin : OpsSecureUserRoles.DMAUser;
                var authorizedData = AuthorizationManager.GetAuthorizedData(UserDetails.Id, UserName, userRole);//User.IsInRole(OpsSecureUserRoles.DMAAdmin) ? OpsSecureUserRoles.DMAAdmin : User.IsInRole(OpsSecureUserRoles.DMAUser) ? OpsSecureUserRoles.DMAUser : string.Empty);
                SetSessionValue(preferncesKey, authorizedData);
                return authorizedData;
            }
            protected set
            {
                var preferncesKey = $"{UserName}{OpsSecureSessionVars.AuthorizedUserData}";
                SetSessionValue(preferncesKey, value);
            }
        }

        private void ResetAuthorizedFundData()
        {
            var preferncesKey = $"{UserName}{OpsSecureSessionVars.AuthorizedFundData}";
            var authorizedData = AdminFundManager.GetFundData(AuthorizedSessionData, PreferredFundNameInSession);
            SetSessionValue(preferncesKey, authorizedData);
        }

        public List<HFundBasic> AuthorizedDMAFundData
        {
            get
            {
                var preferncesKey = $"{UserName}{OpsSecureSessionVars.AuthorizedFundData}";
                var authorizedData = new List<HFundBasic>();
                if(GetSessionValue(preferncesKey) != null)
                    authorizedData = (List<HFundBasic>)GetSessionValue(preferncesKey);

                if(authorizedData.Count > 0)
                    return authorizedData;

                authorizedData = AdminFundManager.GetFundData(AuthorizedSessionData, PreferredFundNameInSession);
                SetSessionValue(preferncesKey, authorizedData);
                return authorizedData;
            }
            private set
            {
                var preferncesKey = $"{UserName}{OpsSecureSessionVars.AuthorizedFundData}";
                SetSessionValue(preferncesKey, value);
            }
        }

        protected const string DefaultExportFileFormat = ".xlsx";

        public JsonResult GetContextDatesOfTodayAndYesterday()
        {
            var thisContextDate = DateTime.Now.Date.GetContextDate();
            return Json(new { thisContextDate, previousContextDate = thisContextDate.GetContextDate() });
        }
    }
}