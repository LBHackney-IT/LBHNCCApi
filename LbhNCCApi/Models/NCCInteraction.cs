namespace LbhNCCApi.Models
{
    public class NCCInteraction
    {
        public string interactionId { get; set; }
        public string callReasonId { get; set; } //EnquiryTypeId in CRM
        public int notestype { get; set; }
        public string notes { get; set; }
        public string createdOn { get; set; }
        public string GovNotifyTemplateType { get; set; }
        public GovNotifierChannelTypes GovNotifyChannelType { get; set; }
        public CRMServiceRequest ServiceRequest { get; set; }
        public string PaymentReference { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public bool callTransferred { get; set; }
        public string housingTagRef { get; set; }
        public string otherReason { get; set; }
    }

}