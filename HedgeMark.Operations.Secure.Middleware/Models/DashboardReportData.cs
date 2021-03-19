using System.Collections.Generic;

namespace HedgeMark.Operations.Secure.Middleware.Models
{

    public class Select2Type
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class Select2HeaderComparer : IEqualityComparer<Select2Type>
    {
        public bool Equals(Select2Type x, Select2Type y)
        {
            return x.id.Equals(y.id);
        }
        public int GetHashCode(Select2Type obj)
        {
            return obj.id.GetHashCode();
        }
    }
    public class DashboardReport
    {
        public enum PreferenceCode
        {
            Clients = 1, Funds, Counterparties, AgreementTypes, MessageTypes, Currencies, Stats, Status
        }

        public class Preferences
        {
            public string Preference { get; set; }
            public List<Select2Type> Options { get; set; }
            public List<string> SelectedIds { get; set; }
        }
    }
}
