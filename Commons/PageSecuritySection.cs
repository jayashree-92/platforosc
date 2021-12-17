using System;
using System.Configuration;

namespace Com.HedgeMark.Commons
{
    public class PageSecuritySection : ConfigurationSection
    {
        [ConfigurationProperty("enableSecuredConnection", DefaultValue = "false", IsRequired = true)]
        public bool EnableSecuredConnection
        {
            get => (bool) this["enableSecuredConnection"];
            set => this["enableSecuredConnection"] = value;
        }

        [ConfigurationProperty("excludedPages")]
        public ExcludedPagesCollection ExcludedPages
        {
            get => ((ExcludedPagesCollection)(base["excludedPages"]));
            set => base["excludedPages"] = value;
        }
    }

    [ConfigurationCollection(typeof(ExcludePageElement))]
    public class ExcludedPagesCollection : ConfigurationElementCollection
    {
        private const string PropertyName = "ExcludePage";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

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
        public string Path => (string)this["path"];
    }
}