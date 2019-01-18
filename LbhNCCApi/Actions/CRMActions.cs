using LBH.Utils;
using LbhNCCApi.Exceptions;
using LbhNCCApi.Helpers;
using LbhNCCApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace LbhNCCApi.Actions
{
    public class CRMActions
    {

        public async Task<object> CreateAnnotation(string objectName, string objectId, string Message, HttpClient client)
        {
            try
            {
                HttpResponseMessage response;
                string annotationId = string.Empty;
                JObject note = new JObject();
                note["notetext"] = Message;
                note["objectid_" + objectName + "@odata.bind"] = "/" + objectName + "s (" + objectId + ")";
                string requestUrl = "/api/data/v8.2/annotations?$select=annotationid";

                CRMAPICall api = new CRMAPICall();
                response = await api.sendAsyncAPI(client, HttpMethod.Post, requestUrl, note);
                if (response == null)
                {
                    throw new NullResponseException();
                }
                else if (response.StatusCode == HttpStatusCode.Created) //201
                {
                    JObject createdannotation = JsonConvert.DeserializeObject<JObject>(
                    await response.Content.ReadAsStringAsync());
                    annotationId = createdannotation["annotationid"].ToString();
                    return annotationId;
                }
                else
                {
                    throw new NCCServiceException();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static List<dynamic> GetAllHousingRefNotes(string housingRef, HttpClient _client)
        {
            HttpResponseMessage result = null;
            try
            {
                    var query = CRMAPICall.getAllContactsWithHousingRef(housingRef);
                    result = CRMAPICall.getAsyncAPI(_client, query).Result;
                    if (result != null)
                    {
                        if (!result.IsSuccessStatusCode)
                        {
                            throw new ServiceException();
                        }

                        var accounts = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                        if (accounts?["value"] != null)
                        {
                            var accountsColl = accounts["value"].ToList();

                            if (accountsColl.Count > 0)
                            {
                                var groupContacts = (from response in accountsColl
                                                     group response by new
                                                     {
                                                         contactId = response["contact1_x002e_contactid"]
                                                     } into grp
                                                     select new
                                                     {
                                                         grp.Key.contactId
                                                     });

                                var nccList = new List<dynamic>();
                                foreach (dynamic response in groupContacts)
                                {
                                    GetAllADNotes(response.contactId.ToString(), _client, nccList);
                                }
                                return nccList;
                            }
                        }
                    }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static object GenerateNCCInteractionsObject(NCCInteraction ncc)
        {
            var nccJObject = new JObject();

            //incidentid
            if (Utils.NullToString(ncc.ServiceRequest.Id) != "")
            {
                nccJObject["hackney_servicerequestid@odata.bind"] = "/incidents(" + ncc.ServiceRequest.Id + ")";
            }
            //Ticket number
            if (Utils.NullToString(ncc.ServiceRequest.TicketNumber) != "")
            {
                nccJObject.Add("hackney_name", ncc.ServiceRequest.TicketNumber);
            }
            if (Utils.NullToInteger(ncc.notestype) != 0)
            {
                nccJObject.Add("hackney_notestype", ncc.notestype);
            }
            else { nccJObject.Add("hackney_notestype", 1); } //set as automatic

            nccJObject.Add("hackney_notes", Utils.NullToString(ncc.notes));
            if (Utils.NullToString(ncc.ServiceRequest.ContactId) != "")
            {
                nccJObject["hackney_contactid@odata.bind"] = "/contacts(" + ncc.ServiceRequest.ContactId + ")";
            }
            if (!string.IsNullOrEmpty(ncc.callReasonId))
            {
                nccJObject["hackney_enquirytypeid@odata.bind"] = "/housing_housingenquirytypes(" + ncc.callReasonId + ")";
            }
            if (!string.IsNullOrEmpty(ncc.GovNotifyTemplateType))
            {
                nccJObject["hackney_govnotifier_templatename"] = ncc.GovNotifyTemplateType;
            }
            if (ncc.GovNotifyChannelType != GovNotifierChannelTypes.DontNeed)
            {
                nccJObject["hackney_govnotifier_channeltype"] = Convert.ChangeType(ncc.GovNotifyChannelType, ncc.GovNotifyChannelType.GetTypeCode()).ToString();
            }
            if (!string.IsNullOrEmpty(ncc.PaymentReference))
            {
                nccJObject["hackney_paymentreference"] = ncc.PaymentReference;
            }
            if (ncc.PaymentStatus != PaymentStatus.DontNeed)
            {
                nccJObject["hackney_paymentstatus"] = Convert.ChangeType(ncc.PaymentStatus, ncc.PaymentStatus.GetTypeCode()).ToString();
            }
            nccJObject["hackney_calltransferred"] = ncc.callTransferred.ToString();

            if (!string.IsNullOrEmpty(ncc.housingTagRef))
            {
                nccJObject["hackney_housingtagref"] = ncc.housingTagRef;
            }
            if (!string.IsNullOrEmpty(ncc.otherReason))
            {
                nccJObject["hackney_otherreason"] = ncc.otherReason;
            }

            return nccJObject;
        }

        public static void GetAllADNotes(string contactId, HttpClient _client, List<dynamic> nccList)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.getAllADNotes(contactId);
                result = CRMAPICall.getAsyncAPI(_client, query).Result;
                if (result != null)
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ServiceException();
                    }

                    var notes = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                    if (notes?["value"] != null)
                    {
                        var notesRet = notes["value"].ToList();

                        if (notesRet.Count > 0)
                        {
                             prepareADNotesResultObject(notesRet, nccList);
                        }
                    }
                }
                else
                {
                    throw new NullResponseException();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static object GenerateServiceRequestObject(CRMServiceRequest crmsr)
        {
            var sr = new JObject();
            if (!string.IsNullOrEmpty(crmsr.Subject))
            {
                sr["subjectid@odata.bind"] = "/subjects(" + crmsr.Subject + ")";
            }
            if (!string.IsNullOrEmpty(crmsr.ContactId))
            {
                sr["customerid_contact@odata.bind"] = "/contacts(" + crmsr.ContactId + ")";
            }
            if (!string.IsNullOrEmpty(crmsr.Title))
            {
                sr.Add("title", crmsr.Title);
            }
            if (!string.IsNullOrEmpty(crmsr.Description))
            {
                sr.Add("description", crmsr.Description);
            }
            return sr;
        }

        public static object GetContactHousingRef(string contactId, HttpClient client)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.getContactHouseRef(contactId);

                result = CRMAPICall.getAsyncAPI(client, query).Result;
                if (result != null)
                {
                    if (result.StatusCode == HttpStatusCode.OK) //200 
                    {
                        var resp = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                        if (resp?["value"] != null)
                        {
                            var ret = resp["value"].ToList();

                            if (ret.Count > 0)
                            {
                                return ret[0]["hackney_houseref"].ToString();
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public static async Task<object> GetAllNCCInteractions(string contactId, HttpClient _client)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.getAllNCCInteractions(contactId);

                result = CRMAPICall.getAsyncAPI(_client, query).Result;
                if (result != null)
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ServiceException();
                    }

                    var nccRetrieveResponse = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                    if (nccRetrieveResponse?["value"] != null)
                    {
                        var nccResponse = nccRetrieveResponse["value"].ToList();

                        if (nccResponse.Count > 0)
                        {
                            return new
                            {
                                results = prepareNCCResultObject(nccResponse)
                            };

                        }
                    }
                    return null;
                }
                else
                {
                    throw new NullResponseException();
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        public static List<dynamic> prepareNCCResultObject(List<JToken> nccResponse)
        {
            var groupIncident = (from response in nccResponse
                                 group response by new
                                 {
                                     interactionId = response["hackney_nccinteractionsid"],
                                     serviceRequestId = response["_hackney_servicerequestid_value"],
                                     enquiryTypeId = response["_hackney_enquirytypeid_value"],
                                     ticketNumber = response["hackney_name"],
                                     createdOn = response["createdon"],
                                     contactId = response["_hackney_contactid_value"],
                                     notes = response["hackney_notes"],
                                     notesType = response["hackney_notestype@OData.Community.Display.V1.FormattedValue"],
                                     callType = response["housing_housingenquirytype@OData.Community.Display.V1.FormattedValue"],
                                     callReasonType = response["housing_housingenquirytype2_x002e_housing_name"],
                                     channeltype = response["hackney_govnotifier_channeltype@OData.Community.Display.V1.FormattedValue"],
                                     templatename = response["hackney_govnotifier_templatename"]

                                 } into grp
                                 select new
                                 {
                                     grp.Key.interactionId,
                                     grp.Key.serviceRequestId,
                                     grp.Key.enquiryTypeId,
                                     grp.Key.ticketNumber,
                                     grp.Key.createdOn,
                                     grp.Key.contactId,
                                     grp.Key.notes,
                                     grp.Key.notesType,
                                     grp.Key.callType,
                                     grp.Key.callReasonType,
                                     grp.Key.channeltype,
                                     grp.Key.templatename
                                 });

            var nccList = new List<dynamic>();

            foreach (dynamic response in groupIncident)
            {
                dynamic interactionsObj = new ExpandoObject();
                interactionsObj.interactionId = response.interactionId;
                interactionsObj.serviceRequestId = response.serviceRequestId;
                interactionsObj.ticketNumber = response.ticketNumber;
                interactionsObj.notes = response.notes;
                interactionsObj.notesType = response.notesType;
                interactionsObj.callType = response.callType;
                interactionsObj.callReasonType = response.callReasonType;
                interactionsObj.createdOn = response.createdOn;
                interactionsObj.channeltype = response.channeltype;
                interactionsObj.templatename = response.templatename;
                interactionsObj.contactId = response.contactId;
                nccList.Add(interactionsObj);
            }
            
            return nccList;
        }

        public static List<dynamic> prepareADNotesResultObject(List<JToken> nccResponse, List<dynamic> nccList)
        {
            var groupIncident = (from response in nccResponse
                                 group response by new
                                 {
                                     createdBy = response["_createdby_value@OData.Community.Display.V1.FormattedValue"],
                                     createdOn = response["createdon"],
                                     notesType = response["hackney_notestype@OData.Community.Display.V1.FormattedValue"],
                                     notes = response["hackney_notes"],
                                     callType = response["housing_housingenquirytype2_x002e_housing_enquirycalltype@OData.Community.Display.V1.FormattedValue"],
                                     callReasonType = response["housing_housingenquirytype2_x002e_housing_name"],
                                     otherCallReasons = response["hackney_otherreason"],
                                     clientName = response["contact1_x002e_fullname"],
                                     channeltype = response["hackney_govnotifier_channeltype@OData.Community.Display.V1.FormattedValue"],
                                     templatename = response["hackney_govnotifier_templatename"]
                                 } into grp
                                 select new
                                 {
                                     grp.Key.createdBy,
                                     grp.Key.createdOn,
                                     grp.Key.notesType,
                                     grp.Key.notes,
                                     grp.Key.callType,
                                     grp.Key.callReasonType,
                                     grp.Key.otherCallReasons,
                                     grp.Key.clientName,
                                     grp.Key.channeltype,
                                     grp.Key.templatename
                                 });

            string strSummaryType = "";
            string strSummaryReason = "";
            string strCreatedByDateTime = "";
            dynamic interactionsObjsumm = null;

            foreach (dynamic response in groupIncident)
            {
                
                if (response.notes == "SUMMARY")
                {
                    if (string.IsNullOrEmpty(strCreatedByDateTime))
                    {
                        interactionsObjsumm = new ExpandoObject();
                        //coming in firsttime
                        interactionsObjsumm.notes = response.notes;

                        if(response.callType!=null && !strSummaryType.Contains(response.callType.ToString()))
                            strSummaryType += response.callType + "<br/>";
                        if (response.otherCallReasons!=null)
                            strSummaryReason += response.otherCallReasons.ToString() + "<br/>";
                        else
                            strSummaryReason += response.callReasonType + "<br/>";
                        strSummaryReason += response.callReasonType + "<br/>";
                        strCreatedByDateTime = response.createdOn.ToString();

                        interactionsObjsumm.notesType = response.notesType;
                        interactionsObjsumm.callType = strSummaryType;
                        interactionsObjsumm.callReasonType = strSummaryReason;
                        interactionsObjsumm.createdBy = response.createdBy;
                        interactionsObjsumm.clientName = response.clientName;
                        interactionsObjsumm.createdOn = response.createdOn;
                        interactionsObjsumm.channeltype = response.channeltype;
                        interactionsObjsumm.templatename = response.templatename;
                    }
                    else if (DateTime.Compare(DateTime.Parse(response.createdOn.ToString()), DateTime.Parse(strCreatedByDateTime))==0)
                    {
                        if (response.callType != null && !strSummaryType.Contains(response.callType.ToString()))
                            strSummaryType += response.callType + "<br/>";
                        if (response.otherCallReasons != null)
                            strSummaryReason += response.otherCallReasons.ToString() + "<br/>";
                        else
                            strSummaryReason += response.callReasonType + "<br/>";
                        strCreatedByDateTime = response.createdOn.ToString();
                    }
                    else
                    {
                        interactionsObjsumm.callType = strSummaryType;
                        interactionsObjsumm.callReasonType = strSummaryReason;
                        interactionsObjsumm.createdBy = response.createdBy;
                        nccList.Add(interactionsObjsumm);

                        interactionsObjsumm = new ExpandoObject();
                        interactionsObjsumm.notes = response.notes;
                        if (response.callType != null && !strSummaryType.Contains(response.callType.ToString()))
                            strSummaryType += response.callType + "<br/>";
                        if (response.otherCallReasons != null)
                            strSummaryReason += response.otherCallReasons.ToString() + "<br/>";
                        else
                            strSummaryReason += response.callReasonType + "<br/>";
                        strCreatedByDateTime = response.createdOn.ToString();

                        interactionsObjsumm.notesType = response.notesType;
                        interactionsObjsumm.callType = strSummaryType;
                        interactionsObjsumm.callReasonType = strSummaryReason;
                        interactionsObjsumm.createdBy = response.createdBy;
                        interactionsObjsumm.clientName = response.clientName;
                        interactionsObjsumm.createdOn = response.createdOn;
                        interactionsObjsumm.channeltype = response.channeltype;
                        interactionsObjsumm.templatename = response.templatename;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(strSummaryReason))
                    {
                        interactionsObjsumm.callType = strSummaryType;
                        interactionsObjsumm.callReasonType = strSummaryReason;
                        interactionsObjsumm.createdBy = response.createdBy;
                        nccList.Add(interactionsObjsumm);
                        strSummaryType = "";//ends here
                        strSummaryReason = "";
                        strCreatedByDateTime = "";
                    }
                    dynamic interactionsObjnormal = new ExpandoObject();
                    interactionsObjnormal.notes = response.notes;
                    interactionsObjnormal.notesType = response.notesType;
                    interactionsObjnormal.callType = response.callType;
                    interactionsObjnormal.callReasonType = response.callReasonType;
                    interactionsObjnormal.createdBy = response.createdBy;
                    interactionsObjnormal.clientName = response.clientName;
                    interactionsObjnormal.createdOn = response.createdOn;
                    interactionsObjnormal.channeltype = response.channeltype;
                    interactionsObjnormal.templatename = response.templatename;
                    nccList.Add(interactionsObjnormal);
                }
            }
            return nccList;
        }

        public static async Task<object> SetEditorContents(string content, HttpClient hclient)
        {
            try
            {
                var query = CRMAPICall.GetKBArticleForNCCInteraction();
                var articlexml = $@"<articledata><section id='0'><content><![CDATA[{ content }]]></content></section>
                                    <section id='1'><content></content></section></articledata>";

                JObject comms = new JObject();
                comms.Add("articlexml", articlexml);
                return await CRMAPICall.UpdateObject(hclient, query, comms);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<dynamic> GetLastXCalls(int xCalls, HttpClient hclient)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.getLastXCalls(xCalls);
                result = CRMAPICall.getAsyncAPI(hclient, query).Result;
                if (result != null)
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ServiceException();
                    }

                    var XCalls = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                    if (XCalls?["value"] != null)
                    {
                        var xCallsColl = XCalls["value"].ToList();

                        if (xCallsColl.Count > 0)
                        {
                            var groupCalls = (from response in xCallsColl
                                              group response by new
                                              {
                                                  createdon = response["createdon"],
                                                  name = response["contact1_x002e_fullname"],
                                                  parentcontactId = response["contact1_x002e_parentcustomerid"],
                                                  callreason = response["housing_housingenquirytype2_x002e_housing_name"],
                                                  otherreason = response["hackney_otherreason"],
                                                  callreasonId = response["_hackney_enquirytypeid_value"],
                                                  ticketnumber = response["hackney_name"],
                                                  servicerequestid = response["_hackney_servicerequestid_value"],
                                                  housingtagref = response["hackney_housingtagref"],
                                                  contactid = response["_hackney_contactid_value"]
                                              } into grp
                                              select new
                                              {
                                                  grp.Key.createdon,
                                                  grp.Key.name,
                                                  grp.Key.parentcontactId,
                                                  grp.Key.callreason,
                                                  grp.Key.otherreason,
                                                  grp.Key.callreasonId,
                                                  grp.Key.ticketnumber,
                                                  grp.Key.servicerequestid,
                                                  grp.Key.housingtagref,
                                                  grp.Key.contactid
                                              });
                            //
                           
                            var xcallsList = new List<dynamic>();
                            foreach (dynamic response in groupCalls)
                            {
                                dynamic callsObj = new ExpandoObject();
                                callsObj.createdon = response.createdon;
                                callsObj.name = response.name;
                                callsObj.callreason = response.callreason;
                                callsObj.otherreason = response.otherreason;
                                callsObj.callreasonId = response.callreasonId;
                                callsObj.ticketnumber = response.ticketnumber;
                                callsObj.servicerequestid = response.servicerequestid;
                                callsObj.contactid = response.contactid;
                                if (response.housingtagref!=null && !string.IsNullOrEmpty(response.housingtagref.ToString()))
                                {
                                    callsObj.housingref = response.housingtagref;
                                }
                                else if (response.parentcontactId != null)
                                {
                                    var housingRef = GetHousingRefFromAccount(response.parentcontactId.ToString(), hclient);
                                    callsObj.housingref = housingRef;
                                }
                                else
                                {
                                    callsObj.housingref = "";
                                }
                                xcallsList.Add(callsObj);
                            }
                            return xcallsList;
                        }
                    }
                }
                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<object> RecordGovNotifierMessage(string nccinteractionId, GovNotifierChannelTypes channelTypes, string templateType, HttpClient hclient)
        {
            try
            {
                var query = CRMAPICall.UpdateCRMGovNotifierFields(nccinteractionId);
                JObject comms = new JObject();
                comms.Add("hackney_govnotifier_channeltype", Convert.ChangeType(channelTypes, channelTypes.GetTypeCode()).ToString());
                comms.Add("hackney_govnotifier_templatename", templateType);
                return await CRMAPICall.UpdateObject(hclient, query, comms);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static string GetHousingRefFromAccount(string parentcontactId, HttpClient hclient)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.GetHousingRefFromAccount(parentcontactId);
                result = CRMAPICall.getAsyncAPI(hclient, query).Result;
                if (result != null)
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ServiceException();
                    }

                    var response = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                    if (response != null)
                    {
                        return response["housing_tag_ref"].ToString();
                    }
                    return null;
                }
                else
                {
                    throw new NullResponseException();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static async Task<object> GetEditorContents(HttpClient hclient)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.GetKBArticleForNCCInteraction();
                result = CRMAPICall.getAsyncAPI(hclient, query).Result;
                if (result != null)
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ServiceException();
                    }

                    var response = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                    if (response != null)
                    {
                        return prepareEditorContentResultObject(response);
                    }
                    return null;
                }
                else
                {
                    throw new NullResponseException();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static object prepareEditorContentResultObject(JToken response)
        {
            var articlexml = "<xml>" + response["articlexml"].ToString() + "</xml>";

            if (articlexml != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(articlexml);
                XmlNode sectionnode = doc.DocumentElement.SelectSingleNode(@"articledata/section");

                var result = new Dictionary<string, object>
                {
                    {
                        "response", new Dictionary<string, object>{
                                { "contents", sectionnode.InnerText}
                        }
                    }
                };
                return result;
            }
            return null;
        }

        public async Task<object> SetCitizenCommunication(string contactid, string CommsDetail, HttpClient _client)
        {
            try
            {
                var query = CRMAPICall.UpdateContactComms(contactid);
                JObject comms = new JObject();
                comms.Add("hackney_communicationdetails", CommsDetail);
                return  await CRMAPICall.UpdateObject(_client, query, comms);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<object> GetCitizenCommunication(string contactid, HttpClient _client)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.UpdateContactComms(contactid);
                result = CRMAPICall.getAsyncAPI(_client, query).Result;
                if (result != null)
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ServiceException();
                    }

                    var commsResponse = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                    if (commsResponse != null)
                    {
                        return prepareCommsResultObject(commsResponse);
                    }
                    return null;
                }
                else
                {
                    throw new NullResponseException();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
}

        private static object prepareCommsResultObject(JToken commsresponse)
        {
            var result = new Dictionary<string, object>
            {
                {
                    "response", new Dictionary<string, object>{
                            {"communicationdetails", commsresponse["hackney_communicationdetails"]}
                    }
                }
            };
            return result;
        }

        public static async Task<object> GetPaymentNCCInteractions(Payments payment, HttpClient _client)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.GetNCCInteractionQueryById(payment.InteractionId);

                result = CRMAPICall.getAsyncAPI(_client, query).Result;
                if (result != null)
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new ServiceException();
                    }

                    var nccresult = new ExpandoObject();
                    var nccRetrieveResponse = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                    if (nccRetrieveResponse != null)
                    {
                        nccresult = preparePaymentNCCResult(nccRetrieveResponse);
                    }

                    var ssouser = new SSOUserData();
                    ssouser.username = payment.Username;
                    SSOAction.GetUserEmailfromActiveDirectory(ssouser);
                    if (ssouser.success)
                    {
                        SSOAction.GetCRMUserDetails(ssouser, _client);
                    }

                    //record the payment details
                    var payresult = SetPaymentStatus(payment, _client);

                    var endresult = new Dictionary<string, object>
                    {
                        {"response", new Dictionary<string, object>{
                                {"InteractionData", nccresult},
                                {"UserData", ssouser},
                                {"PaymentRecorded", payresult},
                            }
                        }
                    };
                    return endresult;
                }
                else
                {
                    throw new NullResponseException();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static object SetPaymentStatus(Payments payment, HttpClient _client)
        {
            var query = CRMAPICall.UpdatePaymentRef(payment);
            JObject pay = new JObject();
            if(!string.IsNullOrEmpty(payment.Reference))
                pay.Add("hackney_paymentreference", payment.Reference);
            if(payment.Status != PaymentStatus.DontNeed)
                pay.Add("hackney_paymentstatus", Convert.ChangeType(payment.Status, payment.Status.GetTypeCode()).ToString());

            return CRMAPICall.UpdateObject(_client, query, pay);
        }

        public static dynamic preparePaymentNCCResult(JToken response)
        {
            dynamic interactionsObj = new ExpandoObject();
            interactionsObj.interactionId = response["hackney_nccinteractionsid"].ToString();
            interactionsObj.serviceRequestId = response["_hackney_servicerequestid_value"];
            interactionsObj.ticketNumber = response["hackney_name"];
            var enquirytype = response["hackney_enquirytypeid"];
            if(enquirytype!=null)
            {
                interactionsObj.callType = enquirytype["housing_enquirycalltype@OData.Community.Display.V1.FormattedValue"];
                interactionsObj.callReasonType = enquirytype["housing_name"];
            }

            var contactid = response["hackney_contactid"];
            if (contactid != null)
            {
                interactionsObj.contactId = contactid["contactid"];
                interactionsObj.fullname = contactid["fullname"];
            }
            return interactionsObj;
        }
    }
}
