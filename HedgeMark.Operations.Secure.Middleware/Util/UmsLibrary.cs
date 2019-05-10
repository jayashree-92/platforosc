using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using HMOSecureMiddleware.UserManagementService;

namespace HMOSecureMiddleware
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
                    SearchCriteria criteria = new SearchCriteria();
                    List<UserManagementService.Attribute> attrbs = new List<UserManagementService.Attribute>();
                    foreach (KeyValuePair<string, string> item in attribs)
                    {
                        attrbs.Add(new UserManagementService.Attribute() { key = item.Key, value = item.Value });
                    }

                    criteria.filterAttrbs = attrbs.ToArray();
                    criteria.userType = userType;
                    result = service.searchByFilter(criteria);
                }
            }
            catch (FaultException ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message));
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message));
                throw ex;
            }
            return result.ToList();
        }

        /// <summary>
        /// ASP.NET's inbuilt methods don't support the wsse version used by UMS
        /// </summary>
        public void SetSecurityHeader()
        {
            MessageHeaders messageHeadersElement = OperationContext.Current.OutgoingMessageHeaders;
            var securityHeader = new SecurityHeader();
            securityHeader.userName = ConfigurationManager.AppSettings["UMS_username"]; 
            securityHeader.password = ConfigurationManager.AppSettings["UMS_password"]; 
            messageHeadersElement.Add(securityHeader);
        }

        public FaultException CreateException(string errorMessage, string errorCode, int UmsCode)
        {
            var ex = new FaultException(errorMessage);
            ex.Data["UmsCode"] = UmsCode;
            ex.Data["ErrorCode"] = errorCode;
            return ex;
        }

        public SearchResultUser LookupUserByUserId(string userName, List<string> attribNames)
        {
            var methodName = MethodBase.GetCurrentMethod().Name;
            SearchResultUser result = new SearchResultUser(); 
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
                Logger.Error(string.Format("{0} - Error Message : {1}", methodName, ex.Message));
                throw;
            }
            return result;
        }

        public void AddUserToLdapGroup(string userId, List<string> ldapGroups)
        {
            try
            {
                //Call UMS
                List<string> successfulGroupAddition = new List<string>();
                List<string> failedGroupAddition = new List<string>();
                List<string> correctedGroupNameList = new List<string>();
                List<string> availableCorrectedLdapGroups = new List<string>();
                List<string> availableLdapGroups = new List<string>();

                var service = new UserManagementService.LDAPServiceDelegateClient();
                SearchResultUser userDetails = new SearchResultUser();
                using (new OperationContextScope((IContextChannel)service.InnerChannel))
                {
                    SetSecurityHeader();

                    availableLdapGroups = GetLdapGroupsofLdapUser(userId);
                    /*appending " Users" because addUserToLDAPGroup method needs '<group name> Users' as parameter
                    /*but lookupUserByUserId method will return '<group name>' alone
                    /*We have only csv of <group name> in web config */
                    availableCorrectedLdapGroups = availableLdapGroups.Select(x => { x = x + " Users"; return x; }).ToList();
                    correctedGroupNameList = ldapGroups.Select(x => { x = x + " Users"; return x; }).ToList();
                    List<string> requiredLdapGroups = correctedGroupNameList.Except(availableCorrectedLdapGroups).ToList();
                    if (requiredLdapGroups.Count() > 0)
                    {
                        successfulGroupAddition = service.addUserToLDAPGroup(userId, requiredLdapGroups.ToArray()).ToList();
                    }
                }

            }
            catch (FaultException<UMSServiceFault> fex)
            {
                string errorMsg = fex.Detail.errorCode == 63 ? "Cannot assign user to given groups. Please contact system admin." : fex.Detail.errorMessage;
                var ex = CreateException(errorMsg, Constants.LdapUserCreationErrorCode, fex.Detail.errorCode);
                throw ex;
            }
            catch (FaultException<UMSSystemFault> fex)
            {
                var ex = CreateException(fex.Detail.errorMessage, Constants.LdapUserCreationErrorCode, fex.Detail.errorCode);
                throw ex;
            }
            catch (FaultException<UMSServiceException> fex)
            {
                var ex = CreateException(fex.Detail.errorMessage, Constants.LdapUserCreationErrorCode, fex.Detail.errorCode);
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<string> GetLdapGroupsofLdapUser(string userName)
        {
            SearchResultUser result = new SearchResultUser();
            List<string> attrbs = new List<string>() { "MELLONECOMMERCEAPPACCESS" };
            result = LookupUserByUserId(userName, attrbs);
            List<string> groups = (result.userAttributes[0].value != null) ? result.userAttributes[0].value.ToList() : new List<string>();
            return groups;
        }

        public void RemoveUserFromLdapGroups(string userId, List<string> ldapGroups)
        {
            try
            {
                //Call UMS
                List<string> successfulGroupRemoval = new List<string>();
                List<string> correctedGroupNameList = new List<string>();

                var service = new UserManagementService.LDAPServiceDelegateClient();
                SearchResultUser userDetails = new SearchResultUser();
                using (new OperationContextScope((IContextChannel)service.InnerChannel))
                {
                    SetSecurityHeader();
                    correctedGroupNameList = ldapGroups.Select(x => { x = x + " Users"; return x; }).ToList();
                    successfulGroupRemoval = service.removeUserFromLDAPGroup(userId, correctedGroupNameList.ToArray()).ToList();
                }

            }
            catch (FaultException<UMSServiceFault> fex)
            {
                var ex = CreateException(fex.Detail.errorMessage, Constants.OperationLdapGroupRemove, fex.Detail.errorCode);
                throw ex;
            }
            catch (FaultException<UMSSystemFault> fex)
            {
                var ex = CreateException(fex.Detail.errorMessage, Constants.OperationLdapGroupRemove, fex.Detail.errorCode);
                throw ex;
            }
            catch (FaultException<UMSServiceException> fex)
            {
                var ex = CreateException(fex.Detail.errorMessage, Constants.OperationLdapGroupRemove, fex.Detail.errorCode);
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ResetPassword(string userId, string newPassword, bool forcePasswordChange)
        {
            var service = new UserManagementService.LDAPServiceDelegateClient();

            using (new OperationContextScope((IContextChannel)service.InnerChannel))
            {
                SetSecurityHeader();
                try
                {
                    service.resetPassword(userId, newPassword, forcePasswordChange);
                }
                catch (FaultException<UMSServiceFault> ex)
                {
                    if (ex.Detail.errorCode == 9)
                    {
                        throw new Exception("Password in history. Please select new password.");
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// SetAccountStatus
        /// </summary>
        /// <param name="userId">LDAP User Id</param>
        /// <param name="statusFlag">"UNLOCK" or "LOCK</param>
        public void SetAccountStatus(string userId, string statusFlag)
        {
            var service = new UserManagementService.LDAPServiceDelegateClient();

            using (new OperationContextScope((IContextChannel)service.InnerChannel))
            {
                SetSecurityHeader();

                service.setAccountStatus(userId, statusFlag);
            }
        }
    }
}
