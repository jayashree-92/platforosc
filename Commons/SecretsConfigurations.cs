using System;
using System.Collections.Generic;
using System.IO;
using Com.HedgeMark.Commons.Extensions;
using log4net;
using HedgeMark.Secrets.Management.Services;
using HedgeMark.Secrets.Management.Services.Entities;
using HedgeMark.Secrets.Management.Services.Entities.Enum;
using Secret = HedgeMark.Secrets.Management.Services.Entities.Secret;

namespace Com.HedgeMark.Commons
{
    public class Secrets
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Secrets));
        
        public static void InitializeSecrets()
        {
            try
            {
                SecretManagementService.allSecrets = new List<Secret>();
                _logger.InfoFormat("Common.Secrets Calling Initialize Secrets.");
                SecretManagementService.InitializeSecrets(GetInputParam());
                _logger.InfoFormat("Common.Secrets Initialized Secrets. Secrets Count {0}.", SecretManagementService.allSecrets.Count);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Common.Secrets : Error while Fetching All Secrets ::: Exception : {0}", ex);
                throw;
            }
        }

        public static InputParam GetInputParam()
        {
            _logger.InfoFormat("Common.Secrets : Creating Input Parameters for SMS.");

            var inputParam = new InputParam()
            {
                App = AppName.HMOperationsSecure,
                Family = ConfigurationManagerWrapper.StringSetting("SMSFamily", "HedgeMark"),
                OrgID = AppMnemonic.DMO,
                Env = GetSecretServiceEnvironment(),
                AuthAccountName = ConfigurationManagerWrapper.StringSetting("SMSAuthAccountName", SecretsConfiguration.SmsAuthenticationName),
                KeyTabFile = ConfigurationManagerWrapper.StringSetting("SMSKeyTabFile", SecretsConfiguration.KeyTabFileName),
            };

            return inputParam;
        }

        public static SecretEnvironment GetSecretServiceEnvironment()
        {
            var environment = SecretsConfiguration.Environment.ToLower();

            if (environment.Contains("test"))
                return SecretEnvironment.Test;
            if (environment.Contains("qa"))
                return SecretEnvironment.QA;
            if (environment.Contains("prod"))
                return SecretEnvironment.Prod;

            return SecretEnvironment.Dev;
        }
    }

    public class SecretsConfiguration
    {
        public static readonly string Environment = ConfigurationManagerWrapper.StringSetting("Environment");
        private static readonly bool IsLocal = Environment.Equals("Local");

        static SecretsConfiguration()
        {
            if (!Directory.Exists(SmsAuthenticationKeyTabFiles))
                Directory.CreateDirectory(SmsAuthenticationKeyTabFiles);
        }

        private static string ManagedAccountRootDirectory
        {
            get
            {
                var configPath = new DirectoryInfo(ConfigurationManagerWrapper.StringSetting("ManagedAccountRootDirectory", @"C:\ManagedAccountFiles\"));
                return configPath.FullName.GetValidatedConfigPath();
            }
        }

        private static string SmsAuthenticationKeyTabFiles
        {
            get
            {
                var configPath = $@"{ManagedAccountRootDirectory}\ApplConfig\";
                return configPath.GetValidatedConfigPath();
            }
        }

        public static string SmsAuthenticationName
        {
            get
            {
                if (Environment.StartsWith("prod", StringComparison.InvariantCultureIgnoreCase))
                    return "dmo_prod_auth_dmo";
                if (Environment.StartsWith("qa", StringComparison.InvariantCultureIgnoreCase))
                    return "dmo_qa_auth_dmo";
                if (Environment.StartsWith("test", StringComparison.InvariantCultureIgnoreCase))
                    return "dmo_test_auth_dmo";

                return "dmo_dev_auth_dmo";
            }

        }

        public static string KeyTabFileName => IsLocal
                    ? $"{AppDomain.CurrentDomain.BaseDirectory}\\Config\\{SmsAuthenticationName}.kt"
                    : $"{SmsAuthenticationKeyTabFiles}\\{SmsAuthenticationName}.kt";

    }
}