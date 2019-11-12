using System.Text;
using System.Web.Mvc;
using HedgeMark.Operations.Secure.DataModel.Models;
using HMOSecureWeb.Filters;
using Web.Filters;
using System.IO;
using System.Net.Mime;
using HMOSecureMiddleware;

namespace HMOSecureWeb.Controllers
{

    public enum OpsSecureSessionVars
    {
        AuthorizedUserData
    }

    public class AuthorizedRolesAttribute : AuthorizeAttribute
    {
        public AuthorizedRolesAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }

    [AuthorizedRoles(OpsSecureUserRoles.WireInitiator, OpsSecureUserRoles.WireApprover)]
    [Authorize, RedirectToHttps, Compress]
    public abstract class BaseController : Controller
    {
        private string thisUserName;
        public string UserName
        {
            get { return thisUserName ?? User.Identity.Name; }
            set { thisUserName = value; }
        }

        public UserAccountDetails UserDetails
        {
            get
            {
                var userDetails = GetSessionValue("UserDetails") as UserAccountDetails;

                if (userDetails != null)
                    return userDetails;

                userDetails = AccountController.GetUserDetails(UserName, User);
                SetSessionValue("UserDetails", userDetails);
                return userDetails;
            }
        }

        public bool IsWireApprover
        {
            get { return User.IsInRole(OpsSecureUserRoles.WireApprover); }
        }

        public object GetSessionValue(string key)
        {
            return Session[key];
        }
        public void SetSessionValue(string key, object value)
        {
            Session[key] = value;
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
}