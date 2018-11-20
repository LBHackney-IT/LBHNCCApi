using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public class GovNotifierEmailPdfInParams
    {
        public string ContactId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EmailTo { get; set; }
        public string TemplateId { get; set; }
        public string TemplateData { get; set; }
    }
}
