using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeMark.Operations.Secure.DataModel;

namespace HMOSecureMiddleware.Models
{
    public class SwiftGroupData
    {
        public hmsSwiftGroup SwiftGroup { get; set; }
        public string Broker { get; set; }
        public string SwiftGroupStatus { get; set; }
        public bool IsAssociatedToAccount { get; set; }
    }
}
