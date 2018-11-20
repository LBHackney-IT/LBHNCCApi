using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LbhNCCApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/CallTypes")]
    public class CallTypesController : Controller
    {
        private readonly ICRMTokenActions _CRMToken;
        private readonly ICRMLookup _CRMLookup;

        CallTypesController(ICRMTokenActions iCRMToken, ICRMLookup iCRMLookup)
        {
            _CRMToken = iCRMToken;
            _CRMLookup = iCRMLookup;
        }
        [HttpGet]
        public async Task<IActionResult> Get(string tenancyRef)
        {
            var token = _CRMToken.GetToken();
            var calltypes = _CRMLookup.Execute(token);
            var result = new Dictionary<string, object>
            {
                {"calltypes", calltypes}
            };

            return Ok(result);
        }
    }
}