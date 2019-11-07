using System;
using System.Threading.Tasks;
using LbhNCCApi.Models;
using Microsoft.AspNetCore.Mvc;
using LbhNCCApi.Actions;
using LbhNCCApi.Helpers;
using System.Net.Http;
using LbhNCCApi.Interfaces;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Callback")]
    public class CallbackController : Controller
    {
        private ICRMClientActions _client = null;
        public CallbackController(ICRMClientActions client)
        {
            _client = client;
        }

        [HttpPost]
        [Route("SendCallbackEmail")]
        public async Task<IActionResult> SendCallbackEmail([FromBody]CallbackRequest callback)
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

        [HttpGet]
        [Route("GetCallbackDetails")]
        public async Task<IActionResult> GetCallbackDetails(string CallbackId)
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(false);
                return Json(await CallBackActions.GetCallBackDetails(hclient, CallbackId));
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        [HttpGet]
        [Route("GetUsersListFromActiveDirectory")]
        public async Task<IActionResult> GetUsersListFromActiveDirectory(string username)
        {
            try
            {
                return Json(await CallBackActions.SearchUsersFromActiveDirectory(username));
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

    }
}