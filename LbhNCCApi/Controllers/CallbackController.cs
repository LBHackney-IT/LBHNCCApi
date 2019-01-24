using System;
using System.Threading.Tasks;
using LbhNCCApi.Models;
using Microsoft.AspNetCore.Mvc;
using LbhNCCApi.Actions;
using LbhNCCApi.Helpers;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Callback")]
    public class CallbackController : Controller
    {
        [HttpPost]
        [Route("SendCallbackEmail")]
        public async Task<IActionResult> SendCallbackEmail(CallbackRequest callback)
        {
            try
            {
                var result = new CallBackActions().SendCallbackEmail(callback);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

    }
}