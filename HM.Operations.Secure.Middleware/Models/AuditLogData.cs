﻿namespace HM.Operations.Secure.Middleware.Models
{
    public class AuditLogData
    {
        public string ModuleName { get; set; }
        public string Action { get; set; }
        public string Purpose { get; set; }
        public string FundName { get; set; }
        public string TemplateName { get; set; }
        public bool IsFundTransfer { get; set; }
        public string AgreementName { get; set; }
        public string SendingAccount { get; set; }
        public string ReceivingAccount { get; set; }
        public string MessageType { get; set; }
        public long AssociationId { get; set; }
        public string TransferType { get; set; }
        public string[][] changes { get; set; }
    }
}
