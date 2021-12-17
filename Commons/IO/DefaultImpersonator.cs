using System;

namespace Com.HedgeMark.Commons.IO
{
    public class DefaultImpersonator : IDisposable
    {
        private readonly Impersonator impersonator;

        private string UserName => ConfigurationManagerWrapper.StringSetting(Config.UserName);

        private string Domain => ConfigurationManagerWrapper.StringSetting(Config.Domain);

        private string Password => ConfigurationManagerWrapper.StringSetting(Config.Password);

        public DefaultImpersonator()
        {
            if (string.IsNullOrEmpty(UserName))
                return;
            impersonator = new Impersonator(UserName, Domain, Password);
        }

        public void Dispose()
        {
            if (impersonator != null)
                impersonator.Dispose();
        }
    }
}