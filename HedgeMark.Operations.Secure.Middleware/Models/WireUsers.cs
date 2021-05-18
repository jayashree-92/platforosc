using System;
using HedgeMark.Operations.Secure.DataModel;
using HedgeMark.Operations.Secure.Middleware.Util;
using Humanizer;

namespace HedgeMark.Operations.Secure.Middleware.Models
{

    public enum UserAuthorizationCode
    {
        AuthorizedToHandleAllWires = 1, AuthorizedToHandleSystemGeneratedWiresOnly, NotAuthorizedToHandleWires
    }

    public class WireUsers
    {
        public hmsUser User { get; set; }
        public string Email { get; set; }
        public string UserName { get { return Email.HumanizeEmail(); } }
        public string UserGroup { get; set; }
        public string Role { get { return User.LdapRole; } }
        public UserAuthorizationCode AuthorizationCode { get; set; }
        public string AuthorizationCodeStr { get { return AuthorizationCode.ToString().Humanize(); } }
        public int TotalWiresInitiated { get; set; }
        public int TotalWiresApproved { get; set; }
        public DateTime LastAccessedOn { get; set; }
    }
}
