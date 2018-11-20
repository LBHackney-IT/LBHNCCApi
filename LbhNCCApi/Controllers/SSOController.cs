using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LbhNCCApi.Actions;
using LbhNCCApi.Helpers;
using LbhNCCApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/SSO")]
    public class SSOController : Controller
    {
        private ICRMClientActions _client = null;
        public SSOController(ICRMClientActions client)
        {
            _client = client;
        }

        [HttpGet]
        [Route("Authenticate")]
        public async Task<IActionResult> Authenticate(string userdata)
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(false);

                return Ok(await SSOAction.AuthenticateUser(userdata, hclient));
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }

        }

    }
}