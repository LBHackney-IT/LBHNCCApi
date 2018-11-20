using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LbhNCCApi.Helpers
{
    public class Trap : ControllerBase
    {
        public IActionResult ThrowErrorMessage(Exception ex)
        {
            var exception = new Dictionary<string, object>
                {
                    {"error",
                            new Dictionary<string, object>{
                                {"message", ex.Message},
                            }
                    }
                };
            return StatusCode(StatusCodes.Status500InternalServerError, exception);
        }
    }
}
