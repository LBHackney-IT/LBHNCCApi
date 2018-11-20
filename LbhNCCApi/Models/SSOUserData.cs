using System.Collections.Generic;

namespace LbhNCCApi.Models
{
    public class SSOUserData
    {
        public string fullname { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string useremail { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public List<string> roles { get; set; }
    }
}