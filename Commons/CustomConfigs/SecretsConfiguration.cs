using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Com.HedgeMark.Commons.CustomConfigs
{
    public class SecretsConfiguration
    {
        public static string GetSecret(string key, string defaultVal = "")
        {
            var secret = AllSecrets.FirstOrDefault(s => s.Key == key);
            return secret == null ? defaultVal : secret.Value;
        }

        public static List<Secret> AllSecrets
        {
            get { return SecretSection.Secrets.Cast<Secret>().ToList(); }
        }

        public static SecretSection SecretSection
        {
            get { return (SecretSection)ConfigurationManager.GetSection("secretSettings"); }
        }
    }

    public class SecretSection : ConfigurationSection
    {
        [ConfigurationProperty("secrets")]
        public SecretsCollection Secrets
        {
            get { return this["secrets"] as SecretsCollection; }
        }
    }

    [ConfigurationCollection(typeof(Secret))]
    public class SecretsCollection : ConfigurationElementCollection
    {
        public Secret this[int index]
        {
            get
            {
                return BaseGet(index) as Secret;
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
            return new Secret();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Secret)element).Key;
        }
    }

    public class Secret : ConfigurationElement
    {
        [ConfigurationProperty("key", DefaultValue = "", IsRequired = true)]
        public string Key
        {
            get { return this["key"] as string; }
        }

        [ConfigurationProperty("value", DefaultValue = "", IsRequired = true)]
        public string Value
        {
            get { return this["value"] as string; }
        }
    }
}