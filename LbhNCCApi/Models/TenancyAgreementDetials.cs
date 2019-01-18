using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class TenancyAgreementDetials
    {
        public string CurrentBalance { get; set; }
        public string DisplayBalance { get; set; }
        public string Rent { get; set; }
        public string HousingReferenceNumber { get; set; }
        public string PropertyReferenceNumber { get; set; }
        public string PaymentReferenceNumber { get; set; }
        public bool IsAgreementTerminated { get; set; }
        public string TenureType { get; set; }
    }
}
