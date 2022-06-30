using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware.Models
{
    public class FundAccountClearingBrokerData
    {
        public hmsFundAccountClearingBroker ClearingBroker { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public int ExposureTypeId { get; set; }
        public string AgreementName { get; set; }
        public string CounterpartyName { get; set; }
        public string ClientName { get; set; }
        public string AccountType { get; set; }
        public string FFCName { get; set; }
        public string FFCNumber { get; set; }

        public string RecCreatedBy { get; set; }
    }
}
