using System;
using System.Threading.Tasks;
using LbhNCCApi.Actions;
using LbhNCCApi.Helpers;
using LbhNCCApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Paris")]
    public class ParisController : Controller
    {
        /// <summary>
        /// Gets the Editorial Content to be displayed on the NCC frontend.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetParisToken")]
        public async Task<IActionResult> GetParisToken(ParisParameters parisparam)
        {
            try
            {
                //return Json(await ParisActions.MakeParisCall(parisparam));
                return Ok();
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

    }
}