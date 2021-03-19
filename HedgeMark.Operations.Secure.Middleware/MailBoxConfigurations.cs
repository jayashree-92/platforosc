using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Com.HedgeMark.Commons.Extensions;

namespace HedgeMark.Operations.Secure.Middleware
{
    public class MailBoxConfigurations
    {
        public static List<MailBoxConfig> AllMailBoxConfigs
        {
            get { return MailBoxSettings.MailBox.Cast<MailBoxConfig>().ToList(); }
        }

        public static MailBoxSection MailBoxSettings
        {
            get { return (MailBoxSection)ConfigurationManager.GetSection("mailBoxSettings"); }
        }
    }

    public class MailBoxSection : ConfigurationSection
    {
        [ConfigurationProperty("mailBoxes")]
        public MailBoxCollection MailBox
        {
            get { return this["mailBoxes"] as MailBoxCollection; }
        }
    }

    [ConfigurationCollection(typeof(MailBoxConfig))]
    public class MailBoxCollection : ConfigurationElementCollection
    {
        public MailBoxConfig this[int index]
        {
            get
            {
                return BaseGet(index) as MailBoxConfig;
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MailBoxConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MailBoxConfig)element).MailBoxName;
        }
    }

    public class MailBoxConfig : ConfigurationElement
    {
        [ConfigurationProperty("mailBoxName", DefaultValue = "", IsRequired = true)]
        public string MailBoxName
        {
            get { return this["mailBoxName"] as string; }
        }

        [ConfigurationProperty("username", DefaultValue = "", IsRequired = true)]
        public string UserName
        {
            get { return this["username"] as string; }
        }

        [ConfigurationProperty("password", DefaultValue = "", IsRequired = true)]
        public string Password
        {
            get { return this["password"] as string; }
        }

        [ConfigurationProperty("enableSsl", DefaultValue = true, IsRequired = false)]
        public bool EnableSsl
        {
            get { return Convert.ToBoolean(this["enableSsl"] ?? "true"); }
        }
    }
}