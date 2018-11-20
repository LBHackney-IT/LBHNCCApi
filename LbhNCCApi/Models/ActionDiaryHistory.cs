using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class ActionDiaryHistory
    {
        public string TenancyRef { get; set; }
        public string ActionName { get; set; }
        public string Comment { get; set; }
        public string Username { get; set; }
        public string CreationDate { get; set; }
    }
}
