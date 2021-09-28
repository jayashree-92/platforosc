using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Com.HedgeMark.Commons.CustomConfigs;
using HM.Operations.Secure.Middleware.UserManagementService;
using log4net;

namespace HM.Operations.Secure.Middleware.Util
{
    public class UmsLibrary
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UmsLibrary));

        public List<SearchResultUser> SearchByFilter(string userType, List<KeyValuePair<string, string>> attribs)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            var service = new LDAPServiceDelegateClient();
            SearchResultUser[] result;
            try
            {
                using (new OperationContextScope((IContextChannel)service.InnerChannel))
                {
                    SetSecurityHeader();
                    var criteria = new SearchCriteria
                    {
                        filterAttrbs = attribs.Select(item => new UserManagementService.Attribute() { key = item.Key, value = item.Value }).ToArray(),
                        userType = userType
                    };

                    result = service.searchByFilter(criteria);
                }
            }
            catch (FaultException ex)
            {
                Logger.Error($"{methodName} - Error Message : {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} - Error Message : {ex.Message}");
                throw ex;
            }
            return result.ToList();
        }

        /// <summary>
        /// ASP.NET's inbuilt methods don't support the wsse version used by UMS
        /// </summary>
        public static void SetSecurityHeader()
        {
            var messageHeadersElement = OperationContext.Current.OutgoingMessageHeaders;
            var securityHeader = new SecurityHeader
            {
                userName = SecretsConfiguration.GetSecret("UMS_username"),
                password = SecretsConfiguration.GetSecret("UMS_password")
            };

            messageHeadersElement.Add(securityHeader);
        }

        public FaultException CreateException(string errorMessage, string errorCode, int UmsCode)
        {
            var ex = new FaultException(errorMessage)
            {
                Data =
                {
                    ["UmsCode"] = UmsCode,
                    ["ErrorCode"] = errorCode
                }
            };
            return ex;
        }

        public static SearchResultUser LookupUserByUserId(string userName, List<string> attribNames)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            var result = new SearchResultUser();
            var service = new LDAPServiceDelegateClient();
            try
            {
                using (new OperationContextScope((IContextChannel)service.InnerChannel))
                {
                    SetSecurityHeader();

                    result = service.lookupUserByUserId(userName, attribNames.ToArray());
                }
            }
            catch (FaultException<UMSSystemFault> fex)
            {
                if (fex.Detail.errorCode == 101)
                {
                    // no info available
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{methodName} - Error Message : {ex.Message}");
                throw;
            }
            return result;
        }


        public static List<string> GetLdapGroupsOfLdapUser(string userName)
        {
            var attrbs = new List<string>() { "MELLONECOMMERCEAPPACCESS" };
            var result = LookupUserByUserId(userName, attrbs);
            if (result == null)
                return new List<string>();

            var groups = (result.userAttributes[0].value != null) ? result.userAttributes[0].value.ToList() : new List<string>();
            return groups;
        }

    }
}
