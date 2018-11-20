using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class Payments
    {
        public string InteractionId { get; set; }
        public string Username { get; set; }
        public string Reference { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
