using System;
using HM.Operations.Secure.DataModel;
using HM.Operations.Secure.Middleware.Util;
using Humanizer;

namespace HM.Operations.Secure.Middleware.Models
{

    public enum UserAuthorizationCode
    {
        AuthorizedToHandleAllWires = 1, AuthorizedToHandleSystemGeneratedWiresOnly, NotAuthorizedToHandleWires
    }

    public class WireUsers
    {
        public hmsUser User { get; set; }
        public string CommitId { get; set; }
        public string Email { get; set; }
        public string UserName => Email.HumanizeEmail();
        public string UserGroup { get; set; }
        public string Role => User.LdapRole;
        public UserAuthorizationCode AuthorizationCode { get; set; }
        public string AuthorizationCodeStr => AuthorizationCode.ToString().Humanize();
        public int TotalWiresInitiated { get; set; }
        public int TotalWiresApproved { get; set; }
        public DateTime LastAccessedOn { get; set; }
    }
}
