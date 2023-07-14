using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware;
using HM.Operations.Secure.Web.Controllers;

namespace HM.Operations.Secure.Web.Filters
{
    public class AuthorizedRolesAttribute : AuthorizeAttribute
    {
        public AuthorizedRolesAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if(!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }
            if(!(ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, OpsSecureUserRoles.DMAAdmin) || ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, OpsSecureUserRoles.DMAUser)))
            {
                var userRole = httpContext.Session["userRole"].ToString();
                if(string.IsNullOrWhiteSpace(userRole))
                {
                    userRole = GetUserRole(httpContext.User.Identity.Name);
                    httpContext.Session["userRole"] = userRole;
                }
                else
                {
                    var claimsIdentity = ClaimsPrincipal.Current.Identities.First();


                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, userRole));
                }
            }

            //return this.Roles.Split(',').Any(role => ClaimsPrincipal.Current.HasClaim(ClaimTypes.Role, AccountController.AuthorizeRoleObjectMap[role]));

            return this.Roles.Split(',').Any(role => ClaimsPrincipal.Current.Claims.Any(claim => claim.Value == AccountController.AuthorizeRoleObjectMap[role]));
        }

        private static string GetUserRole(string userName)
        {
            using(var context = new AdminContext())
            {
                return (from aspUser in context.aspnet_Users
                        where aspUser.UserName == userName && aspUser.aspnet_Roles.Any(r => AuthorizationManager.AuthorizedDmaUserRoles.Contains(r.RoleName)) //&& !usr.isDeleted
                        let role = aspUser.aspnet_Roles.Any(r => r.RoleName == OpsSecureUserRoles.DMAAdmin) ? OpsSecureUserRoles.DMAAdmin : OpsSecureUserRoles.DMAUser
                        select role).FirstOrDefault() ?? string.Empty;
            }
        }
    }
}