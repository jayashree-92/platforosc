using HM.Operations.Secure.DataModel;

namespace HM.Operations.Secure.Middleware.Models
{
    public class WirePortalCutOffData
    {
        public hmsWirePortalCutoff WirePortalCutoff { get; set; }
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
    }
}
