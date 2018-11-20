using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Exceptions.Helpers
{
    public class Validate
    {
        public static object ErrorMessage(string message)
        {
            var errormessage = new Dictionary<string, object>
                {
                    {"error",
                            new Dictionary<string, object>{
                                {"message", message},
                                {"valid", false},
                            }
                    }
                };
            return errormessage ;
        }


        public static object ReturnMessage(bool success, string message)
        {
            var errormessage = new Dictionary<string, object>
                {
                    {"response",
                            new Dictionary<string, object>{
                                {"message", message},
                                {"success", success},
                            }
                    }
                };
            return errormessage;
        }

    }
}
