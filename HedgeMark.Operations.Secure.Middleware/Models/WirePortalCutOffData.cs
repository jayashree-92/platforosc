using HedgeMark.Operations.Secure.DataModel;

namespace HedgeMark.Operations.Secure.Middleware.Models
{
    public class WirePortalCutOffData
    {
        public hmsWirePortalCutoff WirePortalCutoff { get; set; }
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
    }
}
