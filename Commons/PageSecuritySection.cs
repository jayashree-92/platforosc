using System;
using System.Configuration;

namespace Com.HedgeMark.Commons
{
    public class PageSecuritySection : ConfigurationSection
    {
        [ConfigurationProperty("enableSecuredConnection", DefaultValue = "false", IsRequired = true)]
        public Boolean EnableSecuredConnection
        {
            get { return (Boolean) this["enableSecuredConnection"]; }
            set { this["enableSecuredConnection"] = value; }
        }

        [ConfigurationProperty("excludedPages")]
        public ExcludedPagesCollection ExcludedPages
        {
            get { return ((ExcludedPagesCollection)(base["excludedPages"])); }
            set { base["excludedPages"] = value; }
        }
    }

    [ConfigurationCollection(typeof(ExcludePageElement))]
    public class ExcludedPagesCollection : ConfigurationElementCollection
    {
        private const string PropertyName = "ExcludePage";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExcludePageElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExcludePageElement)(element)).Path;
        }
    }

    public class ExcludePageElement : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public String Path
        {
            get
            {
                return (String)this["path"];
            }
        }
    }
}