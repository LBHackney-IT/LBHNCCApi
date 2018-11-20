using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LbhNCCApi.Actions;
using LbhNCCApi.Helpers;
using LbhNCCApi.Interfaces;
using LbhNCCApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Payment")]
    public class PaymentController : Controller
    {
        private ICRMClientActions _client = null;
        public PaymentController(ICRMClientActions client)
        {
            _client = client;
        }

        /// <summary>
        /// Fetching all the Interaction, User and Payment details once coming back from the Paris.
        /// </summary>
        /// <param name="payment">Payment details for Interaction</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPaymentInteractions")]
        public async Task<IActionResult> GetPaymentInteractions(Payments payment)
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(true);

                object paymentdetails = null;
                paymentdetails = await CRMActions.GetPaymentNCCInteractions(payment, hclient);
                
                return Ok(paymentdetails);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

    }
}