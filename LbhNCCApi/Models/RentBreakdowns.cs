using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class RentBreakdowns
    {
        public string Description { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string EffectiveDate { get; set; }
        public string LastChargeDate { get; set; }
    }
}
