using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class ParisParameters
    {
        public string CustomerFullName { get; set; }
        public string AddressLine1 { get; set; }
        public string Postcode { get; set; }
        public string ParisReference { get; set; }
        public string FundCode { get; set; }
        public string Amount { get; set; }
        public string InteractionId { get; set; }
        public string UserId { get; set; }
    }
}
