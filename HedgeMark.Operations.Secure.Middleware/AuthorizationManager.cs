using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.HedgeMark.Commons;
using HedgeMark.Operations.Secure.DataModel;

namespace HMOSecureMiddleware
{
    public class AuthorizedEntity
    {
        public long Id { get; set; }
        public int Level { get; set; }
        public string LevelDescription
        {
            get
            {
                switch (this.Level)
                {
                    case 0:
                        return "No Access";
                    case 1:
                        return "Read Only";
                    case 2:
                        return "Full Access";
                    default:
                        return "Unknown";
                }
            }
        }
    }

    public class AuthorizedData
    {
        public AuthorizedData(List<AuthorizedEntity> onBoardFundIds, List<AuthorizedEntity> hmFundIds)
        {
            OnBoardFundIds = onBoardFundIds;
            HMFundIds = hmFundIds;
            IsPrivilegedUser = false;
        }

        private AuthorizedData(bool isPrivilegedUser)
        {
            HMFundIds = new List<AuthorizedEntity>();
            OnBoardFundIds = new List<AuthorizedEntity>();
            IsPrivilegedUser = isPrivilegedUser;
        }
        public List<AuthorizedEntity> HMFundIds { get; private set; }
        public List<AuthorizedEntity> OnBoardFundIds { get; private set; }
        public string UserName { get; set; }
        public bool IsPrivilegedUser { get; private set; }

        public static AuthorizedData GetPrivilegedAccess(string userName)
        {
            return new AuthorizedData(true)
            {
                UserName = userName
            };
        }
    }

    public static class OpsSecureUserRoles
    {
        public const string WireInitiator = "hm-wire-initiator";
        public const string WireApprover = "hm-wire-approver";
        public const string DMAUser = "DMAUser";
        public const string DMAAdmin = "DMAAdmin";
    }


    public class AuthorizationManager
    {
        public static readonly List<string> AuthorizedDmaUserRoles = ConfigurationManagerWrapper.StringListSetting("AllowedDmaUserRoles", "DMAUser,DMAAdmin");

        public static AuthorizedData GetAuthorizedData(int userId, string userName, string userRole)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new UnauthorizedAccessException("Unknown User");
            if (string.IsNullOrWhiteSpace(userRole))
                return new AuthorizedData(new List<AuthorizedEntity>(), new List<AuthorizedEntity>()) { UserName = userName };

            if (userRole == OpsSecureUserRoles.DMAAdmin)
                return AuthorizedData.GetPrivilegedAccess(userName);

            var authHMfund = GetAuthorizedHMFundsEntities(userId).Where(up => up.Level > 0).ToList();
            var authOnBoardfund = GetAuthorizedOnboardingFundsEntities(userId).Where(up => up.Level > 0).ToList();
            return new AuthorizedData(authOnBoardfund, authHMfund) { UserName = userName };
        }

        private static List<AuthorizedEntity> GetAuthorizedHMFundsEntities(int userId)
        {
            using (var context = new AdminContext())
            {
                var hmFunds = (from vf in context.vw_HFund
                               where vf.dmaFundOnBoardId != null
                               join obP in context.dmaFundOnBoardPermissions on vf.dmaFundOnBoardId equals obP.dmaFundOnBoardId
                               where obP.userId == userId
                               select new AuthorizedEntity()
                               {
                                   Id = vf.intFundID,
                                   Level = obP.dmaPermissionLevelId
                               }).ToList();
                return hmFunds;
            }
        }

        private static List<AuthorizedEntity> GetAuthorizedOnboardingFundsEntities(int userId)
        {
            using (var context = new AdminContext())
            {
                var hmFunds = (from obFid in context.vw_HFund
                               where obFid.dmaFundOnBoardId != null
                               join obP in context.dmaFundOnBoardPermissions on obFid.dmaFundOnBoardId equals obP.dmaFundOnBoardId
                               where obP.userId == userId
                               select new AuthorizedEntity()
                               {
                                   Id = obP.dmaFundOnBoardId,
                                   Level = obP.dmaPermissionLevelId
                               }).ToList();
                return hmFunds;
            }
        }
    }
}
