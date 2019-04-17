using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;

namespace Com.HedgeMark.Commons
{
    public static class ConfigurationManagerWrapper
    {
        public static int IntegerSetting(Config key, int defaultValue = 0)
        {
            int value;
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            var parseSucess = int.TryParse(setting, out value);
            return parseSucess ? value : defaultValue;
        }

        public static string StringSetting(Config key, string defaultValue = "")
        {
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            return string.IsNullOrEmpty(setting) ? defaultValue : setting;
        }

        public static List<string> StringListSetting(Config key, string defaultValue = "")
        {
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            var value = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            return value
                .Split(',')
                .Select(x => x.Trim())
                .ToList();
        }

        public static bool BooleanSetting(Config key, bool defaultValue = false)
        {
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            return string.IsNullOrEmpty(setting) ? defaultValue : bool.Parse(setting);
        }


        public static double DoubleSetting(Config key, double defaultValue = 0)
        {
            double value;
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            var parseSucess = double.TryParse(setting, out value);
            return parseSucess ? value : defaultValue;
        }


        public static decimal DecimalSetting(Config key, decimal defaultValue = 0M)
        {
            decimal value;
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            var parseSucess = decimal.TryParse(setting, out value);
            return parseSucess ? value : defaultValue;
        }


        public static DateTime DateSetting(Config key, string defaultValue = "", string format = "yyyyMMdd")
        {
            var setting = StringSetting(key, defaultValue);
            return DateTime.ParseExact(setting, format, CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static DateTime DateSetting(string key, string defaultValue = "", string format = "yyyyMMdd")
        {
            var setting = StringSetting(key, defaultValue);
            return DateTime.ParseExact(setting, format, CultureInfo.InvariantCulture.DateTimeFormat);
        }

        public static List<string> StringListSetting(string key, string defaultValue = "")
        {
            var setting = ConfigurationManager.AppSettings[key];
            var value = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            return value
                .Split(',')
                .Select(x => x.Trim())
                .ToList();
        }

        public static List<long> IntegerListSetting(string key, string defaultValue = "")
        {
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            var value = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            return value
                .Split(',')
                .Select(x => x.Trim().ToLong())
                .ToList();
        }

        public static bool BooleanSetting(string key, bool defaultValue = false)
        {
            var setting = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(setting) ? defaultValue : bool.Parse(setting);
        }


        public static double DoubleSetting(string key, double defaultValue = 0)
        {
            double value;
            var setting = ConfigurationManager.AppSettings[key];
            var parseSucess = double.TryParse(setting, out value);
            return parseSucess ? value : defaultValue;
        }


        public static string StringSetting(string key, string defaultValue = "")
        {
            var setting = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(setting) ? defaultValue : setting;
        }


        public static int IntegerSetting(string key, int defaultValue = 0)
        {
            int value;
            var setting = ConfigurationManager.AppSettings[key];
            var parseSucess = int.TryParse(setting, out value);
            return parseSucess ? value : defaultValue;
        }


        public static Dictionary<string, string> StringToStringMapSetting(Config key, string defaultValue = "")
        {
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            if (string.IsNullOrEmpty(setting))
                return new Dictionary<string, string> { };
            else
            {
                setting = setting.Substring(1, setting.Length - 2);
                string[] keyValuePairsCluster = setting.Split(new[] { "},{" }, StringSplitOptions.None);
                Dictionary<string, string> returnDict = new Dictionary<string, string>();

                foreach (var keyValuePairCluster in keyValuePairsCluster)
                {
                    string[] keyValuePair = keyValuePairCluster.Split(':');
                    if (!returnDict.ContainsKey(keyValuePair[0]))
                        returnDict.Add(keyValuePair[0], keyValuePair[1]);
                }

                return returnDict;
            }
        }


        public static Dictionary<string, List<string>> StringToStringListMapSetting(Config key, string defaultValue = "")
        {
            var setting = ConfigurationManager.AppSettings[key.ToString()];
            if (string.IsNullOrEmpty(setting))
                return new Dictionary<string, List<string>> { };
            else
            {
                setting = setting.Substring(1, setting.Length - 2);
                string[] keyValuePairsCluster = setting.Split(new[] { "},{" }, StringSplitOptions.None);
                Dictionary<string, List<string>> returnDict = new Dictionary<string, List<string>>();

                foreach (var keyValuePairCluster in keyValuePairsCluster)
                {
                    string[] keyValuePair = keyValuePairCluster.Split(':');
                    returnDict.Add(keyValuePair[0], keyValuePair[1].Split(',').Select(x => x.Trim()).ToList());
                }

                return returnDict;
            }
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