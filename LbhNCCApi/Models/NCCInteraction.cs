namespace LbhNCCApi.Models
{
    public class NCCInteraction
    {
        public string InteractionId { get; set; }
        public string CallReasonId { get; set; } //EnquiryTypeId in CRM
        public int Notestype { get; set; }
        public string Notes { get; set; }
        public string CreatedOn { get; set; }
        public string GovNotifyTemplateType { get; set; }
        public GovNotifierChannelTypes GovNotifyChannelType { get; set; }
        public CRMServiceRequest ServiceRequest { get; set; }
        public string PaymentReference { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public bool CallTransferred { get; set; }
        public string HousingTagRef { get; set; }
        public string OtherReason { get; set; }
        public CallbackRequest CallbackRequest { get; set; }
    }

}