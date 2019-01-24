using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class CallbackRequest
    {
        public string CallBackId { get; set; }
        public string RecipientEmailId { get; set; }
        public string ManagerEmailId { get; set; }
        public string PhoneNumber { get; set; }
    }
}
