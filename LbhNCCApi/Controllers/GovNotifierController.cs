using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Models;
using Notify.Models.Responses;
using System.Configuration;
using LbhNCCApi.Helpers;
using LbhNCCApi.Models;
using LbhNCCApi.Actions;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/GovNotifier")]
    public class GovNotifierController : Controller
    {
        string _apiKey = Environment.GetEnvironmentVariable("GOV_NOTIFIER_API_KEY");

        /// <summary>
        /// Send Email with specified template using template data
        /// </summary>
        /// <param name="EmailTo">To email id</param>
        /// <param name="TemplateId">Gov Notifier Template Id</param>
        /// <param name="TemplateData">key value pair of the data in template</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendEmail")]
        public async Task<IActionResult> SendEmail(string EmailTo, string TemplateId, string TemplateData)
        {
            try
            {
                NotificationClient client = new NotificationClient(_apiKey);
                var TemplateDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(TemplateData);

                Dictionary<string, dynamic> personalisation = new Dictionary<string, dynamic>();
                foreach (KeyValuePair<string, string> pair in TemplateDataDict)
                {
                    personalisation.Add(pair.Key.ToString(), pair.Value.ToString());
                }
                EmailNotificationResponse response = client.SendEmail(EmailTo, TemplateId, personalisation);
                var result = new Dictionary<string, object>
                {
                    {"response", response}
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Send Email with Pdf statements with specified template using template data
        /// Even though this controller is designed for Gov Notify but this function works like a 
        /// message queue where a standalone process called NCCGovNotifyAttachments creates pdf statements and calls the GovNotifier API to send emails
        /// </summary>
        /// <param name="ContactId">ContactId for the Pdf statement to be created</param>
        /// <param name="StartDate">Statement/Transaction start date</param>
        /// <param name="EndDate">Statement/Transaction end date</param>
        /// <param name="EmailTo">To email id</param>
        /// <param name="TemplateId">Gov Notifier Template Id</param>
        /// <param name="TemplateData">key value pair of the data in template</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendEmailPdfStatements")]
        public async Task<IActionResult> SendEmailPdfStatements(GovNotifierEmailPdfInParams inParams)
        {
            try
            {
                var response = GovNotifierAction.SendGovNotifyEmailStatements(inParams);
                var result = new Dictionary<string, object>
                {
                    {"response", response}
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }


        /// <summary>
        /// Send SMS with specified template using template data
        /// </summary>
        /// <param name="MobileNumber">Mobile number to send the message</param>
        /// <param name="TemplateId">Gov Notifier Template Id</param>
        /// <param name="TemplateData">key value pair of the data in template</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendSMS")]
        public async Task<IActionResult> SendSMS(string MobileNumber, string TemplateId, string TemplateData)
        {
            try
            {
                NotificationClient client = new NotificationClient(_apiKey);

                var TemplateDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(TemplateData);
                Dictionary<string, dynamic> personalisation = new Dictionary<string, dynamic>();
                foreach (KeyValuePair<string, string> pair in TemplateDataDict)
                {
                    personalisation.Add(pair.Key.ToString(), pair.Value.ToString());
                }
                SmsNotificationResponse response = client.SendSms(MobileNumber, TemplateId, personalisation);
                var result = new Dictionary<string, object>
                {
                    {"response", response}
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        ///  Send Letter with specified template using template data
        /// </summary>
        /// <param name="TemplateId">Gov Notifier Template Id</param>
        /// <param name="TemplateData">key value pair of the data in template</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendLetter")]
        public async Task<IActionResult> SendLetter(string TemplateId, string TemplateData)
        {
            try
            {
                NotificationClient client = new NotificationClient(_apiKey);

                var TemplateDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(TemplateData);
                Dictionary<string, dynamic> personalisation = new Dictionary<string, dynamic>();
                foreach (KeyValuePair<string, string> pair in TemplateDataDict)
                {
                    personalisation.Add(pair.Key.ToString(), pair.Value.ToString());
                }

                LetterNotificationResponse response = client.SendLetter(TemplateId, personalisation);
                var result = new Dictionary<string, object>
                {
                    {"response", response}
                };
                return Ok(result);

            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Gets all the templates designed in Gov Notifier by this for the service 
        /// </summary>
        /// <param name="TemplateType">Templates for sms, email or letter to be specified</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllTemplates")]
        public async Task<IActionResult> GetAllTemplates(string TemplateType)
        {
            try
            {
                NotificationClient client = new NotificationClient(_apiKey);

                TemplateList response = client.GetAllTemplates(TemplateType);
                var result = new Dictionary<string, object>
                {
                    {"response", response}
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Get Template by Id and Version
        /// </summary>
        /// <param name="TemplateId">Gov Notifier Template Id</param>
        /// <param name="Version">Tempalte version</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemplateByIdAndVersion")]
        public async Task<IActionResult> GetTemplateByIdAndVersion(string TemplateId, int Version)
        {
            try
            {
                NotificationClient client = new NotificationClient(_apiKey);

                TemplateResponse response = client.GetTemplateByIdAndVersion(TemplateId, Version);
                var result = new Dictionary<string, object>
                {
                    {"response", response}
                };
                return Ok(result);

            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }

        }

        /// <summary>
        /// Generates template for Preview functionality on the frontend.
        /// </summary>
        /// <param name="olderThanId">Template Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateTemplatePreview")]
        public async Task<IActionResult> GetReceivedTexts(string olderThanId)
        {
            try
            {
                NotificationClient client = new NotificationClient(_apiKey);

                ReceivedTextListResponse response = client.GetReceivedTexts(olderThanId);
                var result = new Dictionary<string, object>
                {
                    {"response", response}
                };
                return Ok(result);

            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

    }
}