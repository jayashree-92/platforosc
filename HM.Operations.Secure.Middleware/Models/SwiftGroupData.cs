using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware.Models
{
    public class SwiftGroupData
    {
        public hmsSwiftGroup SwiftGroup { get; set; }
        public string Broker { get; set; }
        public string SwiftGroupStatus { get; set; }
        public bool IsAssociatedToAccount { get; set; }
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
    }
}
