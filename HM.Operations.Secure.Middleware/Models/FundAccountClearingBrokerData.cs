using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware.Models
{
    public class FundAccountClearingBrokerData
    {
        public hmsFundAccountClearingBroker ClearingBroker { get; set; }
        public string AccountName { get; set; }
        public string RecCreatedBy { get; set; }
    }
}
