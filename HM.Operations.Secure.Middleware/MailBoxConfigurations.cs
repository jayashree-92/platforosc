using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace HM.Operations.Secure.Middleware
{
    public class MailBoxConfigurations
    {
        public static List<MailBoxConfig> AllMailBoxConfigs => MailBoxSettings.MailBox.Cast<MailBoxConfig>().ToList();

        public static MailBoxSection MailBoxSettings => (MailBoxSection)ConfigurationManager.GetSection("mailBoxSettings");
    }

    public class MailBoxSection : ConfigurationSection
    {
        [ConfigurationProperty("mailBoxes")]
        public MailBoxCollection MailBox => this["mailBoxes"] as MailBoxCollection;
    }

    [ConfigurationCollection(typeof(MailBoxConfig))]
    public class MailBoxCollection : ConfigurationElementCollection
    {
        public MailBoxConfig this[int index]
        {
            get => BaseGet(index) as MailBoxConfig;
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
        public string MailBoxName => this["mailBoxName"] as string;

        [ConfigurationProperty("username", DefaultValue = "", IsRequired = true)]
        public string UserName => this["username"] as string;

        [ConfigurationProperty("password", DefaultValue = "", IsRequired = true)]
        public string Password => this["password"] as string;

        [ConfigurationProperty("enableSsl", DefaultValue = true, IsRequired = false)]
        public bool EnableSsl => Convert.ToBoolean(this["enableSsl"] ?? "true");

        [ConfigurationProperty("port", DefaultValue = 587, IsRequired = false)]
        public int Port => Convert.ToInt32(this["port"] ?? 587);
    }
}