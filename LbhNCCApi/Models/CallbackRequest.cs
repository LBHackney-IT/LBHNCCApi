﻿using System;
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
        public string CallersFullname { get; set; }
        public string AgentName { get; set; }
        public string HousingTagRef { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string MessageForEmail { get; set; }
        public CallbackResponse Response { get; set; }
        public string ResponseBy { get; set; }
    }
}
