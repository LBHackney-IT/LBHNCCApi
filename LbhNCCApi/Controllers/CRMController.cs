using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LBH.Utils;
using LbhNCCApi.Actions;
using LbhNCCApi.Exceptions;
using LbhNCCApi.Exceptions.Helpers;
using LbhNCCApi.Helpers;
using LbhNCCApi.Interfaces;
using LbhNCCApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/CRM")]
    public class CRMController : Controller
    {
        private const int XCALLSDEFAULT = 10;
        private ICRMClientActions _client = null;
        public CRMController(ICRMClientActions client)
        {
            _client = client;
        }

        /// <summary>
        /// Adding Notes on to any entities eg. incident, contact, account or custom entities
        /// </summary>
        /// <param name="objectId">Pass the Id of the object/entity you are creating the notes</param>
        /// <param name="objectName">Entity name eg: "incident", "contact" or custom entity like "hackney_nccinteractions"</param>
        /// <param name="NotesMessage">Actual message or notes you want to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddCRMNotes")]
        public async Task<IActionResult> AddCRMNotes(string objectId, string objectName, string NotesMessage)
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(false);

                string annotationid = new CRMActions().CreateAnnotation(objectId, objectName, NotesMessage, hclient).ToString();
                var result = new Dictionary<string, object>
                {
                    {"response", new Dictionary<string, object>{
                                {"annotationid", annotationid}
                            }
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Grab all the Interactions for the contact.
        /// </summary>
        /// <param name="contactId">ContactId for the contact</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllNCCInteractions")]
        public async Task<IActionResult> GetAllNCCInteractions(string contactId)
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(true);

                object nccInteractions = null;
                nccInteractions = await CRMActions.GetAllNCCInteractions(contactId, hclient);
                var json = Json(nccInteractions);
                json.StatusCode = Json(nccInteractions).StatusCode;
                json.ContentType = "application/json";
                return json;

            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Create the NCC Interactions in CRM 365
        /// </summary>
        /// <param name="ncc">Interaction Details to be populated at the backend</param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateNCCInteractions")]
        public async Task<IActionResult> CreateNCCInteractions(NCCInteraction ncc)
        {
            try
            {
                var sr = new JObject();
                var incidentid = string.Empty;
                var ticketnumber = string.Empty;
                HttpClient hclient = _client.GetCRMClient(false);

                var nccJObject = CRMActions.GenerateNCCInteractionsObject(ncc);

                try
                {
                    var nccquery = CRMAPICall.PostNCCInteractionQuery();
                    var createResponseInteraction =
                            await new CRMAPICall().sendAsyncAPI(hclient, HttpMethod.Post, nccquery, nccJObject);

                    if (createResponseInteraction != null)
                    {
                        if (!createResponseInteraction.IsSuccessStatusCode)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, Validate.ReturnMessage(false, createResponseInteraction.ReasonPhrase.ToString()));
                        }

                        var interactionUri = createResponseInteraction.Headers.GetValues("OData-EntityId")
                            .FirstOrDefault();
                        Guid tmInteractionId = Guid.Empty;
                        if (interactionUri != null)
                        {
                            tmInteractionId = Guid.Parse(interactionUri.Split('(', ')')[1]);
                        }

                        var nccInteraction = new NCCInteraction
                        {
                            InteractionId = tmInteractionId.ToString(),
                            CallReasonId = ncc.CallReasonId,
                            ServiceRequest = new CRMServiceRequest
                            {
                                TicketNumber = ticketnumber,
                                Id = incidentid,
                                Subject = ncc.ServiceRequest.Subject,
                                ContactId = ncc.ServiceRequest.ContactId,
                                Title = ncc.ServiceRequest.Title,
                                Description = ncc.ServiceRequest.Description
                            }
                        };

                        var result = new Dictionary<string, object>
                        {
                            {
                                "response", new Dictionary<string, object>{
                                    {"NCCInteraction", nccInteraction}
                                }
                            }
                        };
                        return Ok(result);
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, Validate.ReturnMessage(false, "Call for Create Interaction failed"));
                    }
                }
                catch (Exception e)
                {
                    return new Trap().ThrowErrorMessage(e);
                }
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Create Service Request in CRM 365 in order to bind the Contact and Subject entity with appropriate 
        /// </summary>
        /// <param name="crmsr">Service Request details to be populated at the backend</param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateServiceRequests")]
        public async Task<IActionResult> CreateServiceRequests(CRMServiceRequest crmsr)
        {
            try
            {
                var incidentid = string.Empty;
                var ticketnumber = string.Empty;
                var sr = CRMActions.GenerateServiceRequestObject(crmsr);
                try
                {
                    HttpClient hclient = _client.GetCRMClient(true);

                    var incidentquery = CRMAPICall.PostIncidentQuery();

                    var createResponseIncident =
                        await new CRMAPICall().sendAsyncAPI(hclient, HttpMethod.Post, incidentquery, sr);

                    if (createResponseIncident.StatusCode == HttpStatusCode.Created)
                    {
                        JObject createdServiceRequest = JsonConvert.DeserializeObject<JObject>(
                            await createResponseIncident.Content.ReadAsStringAsync());
                        incidentid = createdServiceRequest["incidentid"].ToString();
                        ticketnumber = createdServiceRequest["ticketnumber"].ToString();
                        crmsr.Id = incidentid;
                        crmsr.TicketNumber = ticketnumber;
                        var result = new Dictionary<string, object>
                        {
                            {"response", new Dictionary<string, object>{
                                        {"servicerequest", crmsr}
                                    }
                            }
                        };
                        return Ok(result);
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, Validate.ReturnMessage(false, "Unable create Service Request failed = " + createResponseIncident.ReasonPhrase));
                    }
                }
                catch (Exception ex)
                {
                    return new Trap().ThrowErrorMessage(ex);
                }
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Sets the Default Communication details for the contact in CRM 365
        /// </summary>
        /// <param name="contactid">Contact Id for the customer</param>
        /// <param name="CommObject">Json object to store the default communications</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetDefaultComms")]
        public async Task<IActionResult> SetCitizenCommunication(string contactid, string CommObject)
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(false);
                var result = new CRMActions().SetCitizenCommunication(contactid, CommObject, hclient).ToString();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Get the Default communication details for the contact in CRM 365
        /// </summary>
        /// <param name="contactid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCitizenCommunication")]
        public async Task<IActionResult> GetCitizenCommunication(string contactid)
        {
            try
            {
                if (contactid != null)
                {
                    HttpClient hclient = _client.GetCRMClient(false);
                    object commsdetail = await CRMActions.GetCitizenCommunication(contactid, hclient);
                    var json = Json(commsdetail);
                    json.StatusCode = Json(commsdetail).StatusCode;
                    json.ContentType = "application/json";
                    return json;
                }
                return Json(Validate.ErrorMessage("contactid cannot be null"));
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Sets the Editorial Details for the NCC Team leaders in annotation entity in CRM 365.
        /// </summary>
        /// <param name="content">Content to be stored in annotation entity</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetEditorialDetails")]
        public async Task<IActionResult> SetEditorialDetails(string content)
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(false);
                return Json(await CRMActions.SetEditorContents(content, hclient));
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Gets the Editorial Content to be displayed on the NCC frontend.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEditorialDetails")]
        public async Task<IActionResult> GetEditorialDetails()
        {
            try
            {
                HttpClient hclient = _client.GetCRMClient(false);
                return Json(await CRMActions.GetEditorContents(hclient));
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Gets the Last X number of calls
        /// </summary>
        /// <param name="XCalls">Number of rows to be fetched from CRM 365.  Shouldnt be less than 10.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetLastXCalls")]
        public async Task<IActionResult> GetLastXCalls(int XCalls)
        {
            try
            {
                if (XCalls >= XCALLSDEFAULT)
                {
                    List<dynamic> notesAD = null;

                    HttpClient hclient = _client.GetCRMClient(true);

                    List<dynamic> xCalls = CRMActions.GetLastXCalls(XCalls, hclient);

                    var result = new List<dynamic>
                    {
                        xCalls
                    };
                    return Ok(result);

                }
                return StatusCode(StatusCodes.Status500InternalServerError, "XCalls need to be provided and should be more than " + XCALLSDEFAULT.ToString());
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Grab all the CRM Enquiry types.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCRMEnquirySubTypes")]
        public async Task<IActionResult> GetCRMEnquirySubTypes()
        {
            try
            {
                HttpClient client = _client.GetCRMClient(true);

                object CRMEnquirySubTypes = null;
                CRMEnquirySubTypes = await CRMActions.GetCRMEnquirySubTypes(client);
                var json = Json(CRMEnquirySubTypes);
                json.StatusCode = Json(CRMEnquirySubTypes).StatusCode;
                json.ContentType = "application/json";
                return json;

            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Grab all the CRM Enquiry Call types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCRMEnquiryCallTypes")]
        public IActionResult GetCRMEnquiryCallTypes()
        {
            try
            {
                var request = CRMActions.GetCRMEnquiryCallTypes(_client.GetCRMClient(true));
                return Json(new { Result = request });
            }
            catch (Exception ex)
            {
                var errors = new List<ApiErrorMessage>
                {
                    new ApiErrorMessage
                    {
                        developerMessage = ex.Message,
                        userMessage = "We had some problems processing your request"
                    }
                };

                var json = Json(errors);
                json.StatusCode = 500;
                return json;
            }
        }
    }
}