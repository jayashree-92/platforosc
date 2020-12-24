using HedgeMark.Operations.Secure.DataModel;

namespace HedgeMark.Operations.Secure.Middleware.Models
{
    public class SwiftGroupData
    {
        public hmsSwiftGroup SwiftGroup { get; set; }
        public string Broker { get; set; }
        public string SwiftGroupStatus { get; set; }
        public bool IsAssociatedToAccount { get; set; }
    }
}
