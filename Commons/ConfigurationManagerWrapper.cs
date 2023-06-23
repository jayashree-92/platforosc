using Com.HedgeMark.Commons.Extensions;
using HM.Azure.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace Com.HedgeMark.Commons
{
    public static class ConfigurationManagerWrapper
    {
        public static string AppName { get { return ConfigurationManager.AppSettings["AppName"] ?? "HMOpsSecure"; } }
        public static string Environment { get { return ConfigurationManager.AppSettings["Environment"] ?? "Local"; } }

        public static AppConfig appSettings { get; set; }
        static ConfigurationManagerWrapper()
        {
            InitializeAzureSettings();
        }

        public static void InitializeAzureSettings()
        {
            appSettings = new AppConfig(AppName, Environment);
        }

        public static string GetAzureConfig(string key)
        {
            var thisSetting = appSettings.Get<string>(key) ?? string.Empty;
            return string.IsNullOrWhiteSpace(thisSetting) ? null : thisSetting;
        }

        public static string GetConnectionString(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }

        private static string GetResolvedValue(string key)
        {
            //First preference is given to app settings at application level
            return ConfigurationManager.AppSettings[key] ?? GetAzureConfig(key);
        }

        public static int IntegerSetting(string key, int defaultValue = 0)
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                return defaultValue;

            var parseSucess = int.TryParse(setting, out var value);
            return parseSucess ? value : defaultValue;
        }

        public static string StringSetting(string key, string defaultValue = "")
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                return defaultValue;

            return string.IsNullOrEmpty(setting) ? defaultValue : setting;
        }

        public static List<long> IntegerListSetting(string key, string defaultValue = "")
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                setting = defaultValue;

            var value = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            return value
                .Split(',')
                .Select(x => x.Trim().ToLong())
                .ToList();
        }


        public static bool BooleanSetting(string key, bool defaultValue = false)
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                return defaultValue;

            var parseSucess = bool.TryParse(setting, out var value);
            return parseSucess ? value : defaultValue;
        }


        public static double DoubleSetting(string key, double defaultValue = 0)
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                return defaultValue;

            var parseSucess = double.TryParse(setting, out var value);
            return parseSucess ? value : defaultValue;
        }


        public static decimal DecimalSetting(string key, decimal defaultValue = 0M)
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                return defaultValue;

            var parseSucess = decimal.TryParse(setting, out var value);
            return parseSucess ? value : defaultValue;
        }


        public static DateTime DateSetting(string key, string defaultValue = "", string format = "yyyyMMdd")
        {
            var setting = StringSetting(key, defaultValue);
            return DateTime.ParseExact(setting, format, CultureInfo.InvariantCulture.DateTimeFormat);
        }



        public static List<string> StringListSetting(string key, string defaultValue = "")
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                setting = defaultValue;

            var value = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            return value
                .Split(',')
                .Select(x => x.Trim())
                .ToList();
        }

        public static Dictionary<string, string> StringToStringMapSetting(string key, string defaultValue = "")
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                setting = defaultValue;

            if(string.IsNullOrEmpty(setting))
                return new Dictionary<string, string>();
            setting = setting.Substring(1, setting.Length - 2);
            var keyValuePairsCluster = setting.Split(new[] { "},{" }, StringSplitOptions.None);
            var returnDict = new Dictionary<string, string>();

            foreach(var keyValuePairCluster in keyValuePairsCluster)
            {
                var keyValuePair = keyValuePairCluster.Split(':');
                if(returnDict.ContainsKey(keyValuePair[0])) continue;
                returnDict.Add(keyValuePair[0], keyValuePair[1]);
            }

            return returnDict;
        }
        public static Dictionary<string, List<string>> StringToStringListMapSetting(string key, string defaultValue = "")
        {
            var setting = GetResolvedValue(key);
            if(string.IsNullOrWhiteSpace(setting))
                setting = defaultValue;

            if(string.IsNullOrEmpty(setting))
                return new Dictionary<string, List<string>>();

            setting = setting.Substring(1, setting.Length - 2);
            var keyValuePairsCluster = setting.Split(new[] { "},{" }, StringSplitOptions.None);
            return keyValuePairsCluster.Select(keyValuePairCluster => keyValuePairCluster.Split(':'))
                .ToDictionary(keyValuePair => keyValuePair[0], keyValuePair => keyValuePair[1].Split(',').Select(x => x.Trim()).ToList());
        }

        public static List<string> StringListSetting(Config key, string defaultValue = "")
        {
            return StringListSetting(key.ToString(), defaultValue);
        }

        public static decimal DecimalSetting(Config key, decimal defaultValue = 0M)
        {
            return DecimalSetting(key.ToString(), defaultValue);
        }

        public static double DoubleSetting(Config key, double defaultValue = 0)
        {
            return DoubleSetting(key.ToString(), defaultValue);
        }
        public static bool BooleanSetting(Config key, bool defaultValue = false)
        {
            return BooleanSetting(key.ToString(), defaultValue);
        }
        public static int IntegerSetting(Config key, int defaultValue = 0)
        {
            return IntegerSetting(key.ToString(), defaultValue);
        }
        public static string StringSetting(Config key, string defaultValue = "")
        {
            return StringSetting(key.ToString(), defaultValue);
        }
        public static List<long> IntegerListSetting(Config key, string defaultValue = "")
        {
            return IntegerListSetting(key.ToString(), defaultValue);
        }
        public static DateTime DateSetting(Config key, string defaultValue = "", string format = "yyyyMMdd")
        {
            var setting = StringSetting(key, defaultValue);
            return DateTime.ParseExact(setting, format, CultureInfo.InvariantCulture.DateTimeFormat);
        }
    }

    public enum Config
    {
        Environment,
        UserName,
        Domain,
        Password,
        MailServer,
        FromMailAddress,
        ToMailAddress,
        SystemNotificationFromMailAddress,
        ShouldAppendMachineInfoInMails,
        DateTimeFormat,
        DefaultDateFormat,
        MessageQueueConnectionString,
        MessageQueueRetryCount,
        RPCEnvironment,
        EnableParallelism,
        ShouldRunInParallel,
        TransactionTimeout,
        BulkCopyTimeout,
        EntityDefaultBatchSize,
        MaxConcurrencyFailureRetries,
        MaxSqlTimeOutRetryCount,
        FileAccessCheckCount,
        FileAccessWaitingDuration,
        GlimpseAllowedUserRoles,
        BlbProxyDownloadTimeOut,
        BlbProxyForceRefresh,
        BlbProxyRequestIdPrefix,
        ShouldEnableGzipCompression,
        ShouldRedirectHttpRequest
    }
}