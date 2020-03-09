using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMOSecureMiddleware.Models
{
    public class SwiftGroupData
    {
        public long hmsSwiftGroupId { get; set; }
        public string SwiftGroup { get; set; }
        public string SendersBIC { get; set; }
        public long BrokerLegalEntityId { get; set; }
        public string Broker { get; set; }
        public int SwiftGroupStatusId { get; set; }
        public string SwiftGroupStatus { get; set; }
        public string AcceptedMessages { get; set; }
        public string Notes { get; set; }
        public bool IsDeleted { get; set; }
        public string RecCreatedBy { get; set; }
        public DateTime RecCreatedAt { get; set; }
        public int AccountsAssociated { get; set; }

    }

    public class SwiftGroupInformation
    {
        public List<SwiftGroupData> SwiftGroupData { get; set; }
        public Dictionary<long, string> Brokers { get; set; }
        public Dictionary<int, string> SwiftGroupStatus { get; set; }

    }
}
