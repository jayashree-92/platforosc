using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using HedgeMark.Operations.Secure.DataModel.Models;
using HMOSecureWeb.Filters;
using Web.Filters;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web.UI;
using Com.HedgeMark.Commons.Extensions;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware;
using HedgeMark.Operations.Secure.Middleware.Util;

namespace HMOSecureWeb.Controllers
{
    public enum OpsSecureSessionVars
    {
        UserCommitId,
        AuthorizedUserData,
        AuthorizedFundData,
        UserPreferencesInSession,
        WiresDashboardData
    }

    public class AuthorizedRolesAttribute : AuthorizeAttribute
    {
        public AuthorizedRolesAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }


    [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)]
    [Authorize, RedirectToHttps, Compress]
    public abstract class BaseController : Controller
    {
        private string thisUserName;
        public string UserName
        {
            get { return thisUserName ?? User.Identity.Name; }
            set { thisUserName = value; }
        }

        public int UserId
        {
            get { return UserDetails.Id; }
        }

        public UserAccountDetails UserDetails
        {
            get
            {
                var userDetails = GetSessionValue("UserDetails") as UserAccountDetails;

                if (userDetails != null)
                    return userDetails;

                userDetails = AccountController.GetUserDetails(Session[OpsSecureSessionVars.UserCommitId.ToString()].ToString(), User);
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
        public bool IsWireApprover
        {
            get { return User.IsInRole(OpsSecureUserRoles.WireApprover); }
        }
        public bool IsWireAdmin
        {
            get { return User.IsInRole(OpsSecureUserRoles.WireAdmin); }
        }


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
            var validFileFullName = FileSystemManager.GetValidatedConfigPath(fileInfo.FullName);

            if (!System.IO.File.Exists(validFileFullName))
                return null;

            FileContentResult fileContent;
            using (var fileStream = System.IO.File.Open(validFileFullName, FileMode.Open, FileAccess.Read))
            {
                var returnBytes = new byte[fileStream.Length];
                fileStream.Read(returnBytes, 0, returnBytes.Length);
                fileContent = File(returnBytes, MediaTypeNames.Application.Octet, string.IsNullOrWhiteSpace(downloadName) ? fileInfo.Name : downloadName);
            }

            //delete will happen only if the file is in the Temp directory - which is often the case of file creation - this is to avoid junk file creation
            if (fileInfo.Directory != null && FileSystemManager.UploadTemporaryFilesPath.Contains(fileInfo.Directory.FullName))
                fileInfo.Delete();

            return fileContent;
        }

        protected FileResult DownloadFile(FileInfo fileInfo, string downloadName)
        {
            var validFileFullName = FileSystemManager.GetValidatedConfigPath(fileInfo.FullName);

            if (!System.IO.File.Exists(validFileFullName))
                return null;

            using (var fileStream = System.IO.File.Open(validFileFullName, FileMode.Open, FileAccess.Read))
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

    [AuthorizedRoles(OpsSecureUserRoles.WireInitiator, OpsSecureUserRoles.WireApprover)]
    public abstract class WireUserBaseController : BaseController
    {
        public List<dmaUserPreference> UserPreferencesInSession
        {
            get
            {
                var preferncesKey = string.Format("{0}{1}", UserId, OpsSecureSessionVars.UserPreferencesInSession);
                if (GetSessionValue(preferncesKey) != null)
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
            var preferencesKey = string.Format("{0}{1}", UserId, OpsSecureSessionVars.UserPreferencesInSession);
            SetSessionValue(preferencesKey, null);
        }


        public PreferencesManager.FundNameInDropDown PreferredFundNameInSession
        {
            get { return (PreferencesManager.FundNameInDropDown)GetPreferenceInSession(PreferencesManager.ShowRiskOrShortFundNames).ToInt(0); }
        }

        public AuthorizedData AuthorizedSessionData
        {
            get
            {
                var preferncesKey = string.Format("{0}{1}", UserName, OpsSecureSessionVars.AuthorizedUserData);
                if (GetSessionValue(preferncesKey) != null)
                    return (AuthorizedData)GetSessionValue(preferncesKey);

                var authorizedData = AuthorizationManager.GetAuthorizedData(UserDetails.Id, UserName, User.IsInRole(OpsSecureUserRoles.DMAAdmin) ? OpsSecureUserRoles.DMAAdmin : User.IsInRole(OpsSecureUserRoles.DMAUser) ? OpsSecureUserRoles.DMAUser : string.Empty);
                SetSessionValue(preferncesKey, authorizedData);
                return authorizedData;
            }
            protected set
            {
                var preferncesKey = string.Format("{0}{1}", UserName, OpsSecureSessionVars.AuthorizedUserData);
                SetSessionValue(preferncesKey, value);
            }
        }

        private void ResetAuthorizedFundData()
        {
            var preferncesKey = string.Format("{0}{1}", UserName, OpsSecureSessionVars.AuthorizedFundData);
            var authorizedData = AdminFundManager.GetFundData(AuthorizedSessionData, PreferredFundNameInSession);
            SetSessionValue(preferncesKey, authorizedData);
        }

        public List<HFundBasic> AuthorizedDMAFundData
        {
            get
            {
                var preferncesKey = string.Format("{0}{1}", UserName, OpsSecureSessionVars.AuthorizedFundData);
                var authorizedData = new List<HFundBasic>();
                if (GetSessionValue(preferncesKey) != null)
                    authorizedData = (List<HFundBasic>)GetSessionValue(preferncesKey);

                if (authorizedData.Count > 0)
                    return authorizedData;

                authorizedData = AdminFundManager.GetFundData(AuthorizedSessionData, PreferredFundNameInSession);
                SetSessionValue(preferncesKey, authorizedData);
                return authorizedData;
            }
            private set
            {
                var preferncesKey = string.Format("{0}{1}", UserName, OpsSecureSessionVars.AuthorizedFundData);
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