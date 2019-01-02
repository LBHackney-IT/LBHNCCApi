using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class TenancyTransactionStatements
    {
        public string Date { get; set; }
        public string Description { get; set; }
        public string In { get; set; }
        public string Out { get; set; }
        public string Balance { get; set; }
    }
}
