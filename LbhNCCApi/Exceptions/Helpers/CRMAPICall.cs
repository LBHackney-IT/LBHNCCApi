using LbhNCCApi.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LbhNCCApi.Exceptions.Helpers;
using LbhNCCApi.Models;

namespace LbhNCCApi.Helpers
{
    public class CRMAPICall
    {
        public async Task<HttpResponseMessage> sendAsyncAPI<T>(HttpClient client, HttpMethod method, string requestUri, T value)
        {
            var response = new HttpResponseMessage();
            try
            {
                var content = value.GetType().Name.Equals("JObject") ?
                value.ToString() :
                JsonConvert.SerializeObject(value, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });

                HttpRequestMessage request = new HttpRequestMessage(method, requestUri) { Content = new StringContent(content) };
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                response = client.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new ServiceException();
                }
            }
            catch (Exception ex)
            {

                response.StatusCode = HttpStatusCode.BadRequest;
                throw new ServiceException();
            }
            return response;
        }

        public static string getUserDetails(string username)
        {
            var query = $@"/api/data/v8.2/systemusers?fetchXml=
                            <fetch version = '1.0' output-format='xml-platform' distinct='true' >
                            <entity name='systemuser'>
                            <attribute name='fullname' />
                            <attribute name='domainname' />
                            <attribute name='systemuserid' />
                            <filter>
                            <condition attribute='domainname' operator='eq' value=""{ username }"" />
                            </filter>
                            <link-entity name='systemuserroles' from='systemuserid' to='systemuserid' >
                            <link-entity name='role' from='roleid' to='roleid' >
                            <attribute name='name' />
                            </link-entity>
                            </link-entity>
                            </entity>
                            </fetch> ";
            return query;
        }

        public static async Task<HttpResponseMessage> postAsyncAPI(HttpClient client, string query, JObject jObject)
        {
            var response = new HttpResponseMessage();
            try
            {

                var content = new StringContent(jObject.ToString(), Encoding.UTF8, "application/json");
                response = client.PostAsync(query, content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new ServiceException();
                }
            }
            catch (Exception ex)
            {

                response.StatusCode = HttpStatusCode.BadRequest;
                throw new ServiceException();
            }
            return response;

        }

        public static async Task<HttpResponseMessage> getAsyncAPI(HttpClient httpClient, string query)
        {
            var response = new HttpResponseMessage();
            try
            {
                response = httpClient.GetAsync(query).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new ServiceException();
                }
            }
            catch (Exception ex)
            {

                response.StatusCode = HttpStatusCode.BadRequest;
                throw new ServiceException();
            }
            return response;
        }

        public static async Task<object> UpdateObject(HttpClient client, string requestUri, JObject updateObject)
        {
            HttpResponseMessage updateResponse;
            var method = new HttpMethod("PATCH");
            string jsonString = JsonConvert.SerializeObject(updateObject);
            HttpRequestMessage request = new HttpRequestMessage(method, requestUri) { Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json") };
            try
            {
                updateResponse = await client.SendAsync(request);

                if (updateResponse.IsSuccessStatusCode)
                {
                    return Validate.ReturnMessage(true, "Successfully updated");
                }
                else
                {
                    return Validate.ReturnMessage(false, updateResponse.ReasonPhrase);
                }
                    
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string UpdateContactComms(string contactid)
        {
            return "/api/data/v8.2/contacts(" + contactid + ")?$select=hackney_communicationdetails";
        }

        public static string getContactHouseRef(string contactid)
        {
            return "/api/data/v8.2/contacts?$select=hackney_houseref&$filter=contactid eq " + contactid;
        }

        public static string getAllNCCInteractions(string contactId)
        {
            var query = $@"
                /api/data/v8.2/hackney_nccinteractionses?fetchXml=
                <fetch version = '1.0' output-format = 'xml-platform' distinct = 'true' >
                <entity name='hackney_nccinteractions' >
                <attribute name='hackney_name' />
                <attribute name='createdon' />
                <attribute name='hackney_nccinteractionsid' />
                <attribute name='hackney_contactid' />
                <attribute name='hackney_contactidname' />
                <attribute name='hackney_servicerequestid' />
                <attribute name='hackney_servicerequestidname' />
                <attribute name='hackney_notes' />
                <attribute name='hackney_notestype' />
                <attribute name='hackney_govnotifier_channeltype' />
                <attribute name='hackney_govnotifier_templatename' />
                <filter>
                <condition attribute='hackney_contactid' operator='eq' value='{contactId}' />
                </filter>
                <link-entity name='contact' from='contactid' to='hackney_contactid' link-type='inner' >
                <attribute name='fullname' />
                </link-entity>
                <link-entity name='housing_housingenquirytype' from='housing_housingenquirytypeid' to='hackney_enquirytypeid' link-type='inner' >
                <attribute name='housing_name' />
                <attribute name='housing_enquirycalltypename' />
                <attribute name='housing_enquirycalltype' />
                </link-entity>
                </entity>
                </fetch>";
            return query.ToString();

        }

        public static string getAllADNotes(string contactId)
        {
            var query = $@"
                /api/data/v8.2/hackney_nccinteractionses?fetchXml=
                <fetch version = '1.0' output-format = 'xml-platform' distinct = 'true' >
                <entity name='hackney_nccinteractions' >
                <attribute name='hackney_contactid' />
                <attribute name='hackney_name' />
                <attribute name='createdon' />
                <attribute name='createdby' />
                <attribute name='hackney_notes' />
                <attribute name='hackney_notestype' />
                <attribute name='hackney_govnotifier_channeltype' />
                <attribute name='hackney_govnotifier_templatename' />
                <attribute name='hackney_otherreason' />
                <attribute name='ownerid' />
                <filter>
                <condition attribute='hackney_contactid' operator='eq' value='{ contactId }' />
                </filter>
                <link-entity name='contact' from='contactid' to='hackney_contactid' link-type='inner' >
                <attribute name='fullname' />
                </link-entity>
                <link-entity name='housing_housingenquirytype' from='housing_housingenquirytypeid' to='hackney_enquirytypeid' link-type='outer' >
                <attribute name='housing_name' />
                <attribute name='housing_enquirycalltypename' />
                <attribute name='housing_enquirycalltype' />
                </link-entity>
                </entity>
                </fetch>";
            return query.ToString();

        }
        public static string getAllNonTenantADNotes(string contactId, string housingref)
        {
            var query = $@"
                /api/data/v8.2/hackney_nccinteractionses?fetchXml=
                <fetch version = '1.0' output-format = 'xml-platform' distinct = 'true' >
                <entity name='hackney_nccinteractions' >
                <attribute name='hackney_contactid' />
                <attribute name='hackney_name' />
                <attribute name='createdon' />
                <attribute name='createdby' />
                <attribute name='hackney_notes' />
                <attribute name='hackney_notestype' />
                <attribute name='hackney_govnotifier_channeltype' />
                <attribute name='hackney_govnotifier_templatename' />
                <attribute name='hackney_otherreason' />
                <attribute name='ownerid' />
                <filter>
                <condition attribute='hackney_contactid' operator='eq' value='{ contactId }' />
                <condition attribute='hackney_housingtagref' operator='eq' value='{ housingref }' />
                </filter>
                <link-entity name='contact' from='contactid' to='hackney_contactid' link-type='inner' >
                <attribute name='fullname' />
                </link-entity>
                <link-entity name='housing_housingenquirytype' from='housing_housingenquirytypeid' to='hackney_enquirytypeid' link-type='outer' >
                <attribute name='housing_name' />
                <attribute name='housing_enquirycalltypename' />
                <attribute name='housing_enquirycalltype' />
                </link-entity>
                </entity>
                </fetch>";
            return query.ToString();

        }

        public static string getAllContactsWithHousingRef(string housingRef)
        {
            var query = $@"
                /api/data/v8.2/accounts?fetchXml=
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                <entity name='account'>
                <attribute name='accountid' />
                <attribute name='housing_tag_ref' />        
                <filter>
                <condition attribute = 'housing_tag_ref' operator= 'eq' value='{housingRef}' />
                </filter>
                <link-entity name='contact' from='parentcustomerid' to='accountid' link-type='inner' >
                <attribute name='contactid' />
                <attribute name='fullname' />
                </link-entity>
                </entity>
                </fetch>";

            return query.ToString();
        }

        public static string PostIncidentQuery()
        {
            return "/api/data/v8.2/incidents()?$select=_customerid_value,description,_subjectid_value,ticketnumber,title";
        }

        public static string PostNCCInteractionQuery()
        {
            return "/api/data/v8.2/hackney_nccinteractionses?$select=_hackney_contactid_value,_hackney_servicerequestid_value,hackney_name,_hackney_enquirytypeid_value,hackney_nccinteractionsid";
        }

        public static string GetNCCInteractionQueryById(string id)
        {
            return $@"/api/data/v8.2/hackney_nccinteractionses({id})?$select=_hackney_contactid_value,_hackney_servicerequestid_value,hackney_name,_hackney_enquirytypeid_value,hackney_nccinteractionsid&$expand=hackney_contactid($select=fullname),hackney_enquirytypeid($select=housing_enquirycalltype, housing_name)";
        }

        public static string GetKBArticleForNCCInteraction()
        {
            return "/api/data/v8.2/kbarticles("+ new Guid("c033c05c-07cb-e811-a96c-002248072cc3") + ")?$select=articlexml";
        }

        public static string getLastXCalls(int xCalls)
        {
            var query = $@"/api/data/v8.2/hackney_nccinteractionses?fetchXml=
                    <fetch version = '1.0' output-format = 'xml-platform' distinct = 'true' top='{xCalls}'>
                    <entity name='hackney_nccinteractions' >
                    <attribute name='hackney_name' />
                    <attribute name='createdon' />
                    <attribute name='createdby' />
                    <attribute name='hackney_servicerequestid' />
                    <attribute name='hackney_otherreason' />
                    <attribute name='hackney_contactid' />
                    <attribute name='hackney_enquirytypeid' />
                    <attribute name='hackney_housingtagref' />
                    <order descending = 'true' attribute = 'createdon' />
                    <link-entity name='contact' from='contactid' to='hackney_contactid' link-type='inner' >
                    <attribute name='fullname' />
                    <attribute name='parentcustomerid' />
                    </link-entity>
                    <link-entity name='housing_housingenquirytype' from='housing_housingenquirytypeid' to='hackney_enquirytypeid' link-type='outer' >
                    <attribute name='housing_name' />
                    <attribute name='housing_enquirycalltypename' />
                    <attribute name='housing_enquirycalltype' />
                    </link-entity>
                    </entity>
                    </fetch> ";
            return query.ToString();
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

        public static string GetCallBackDetails(string id)
        {
            return $@"/api/data/v8.2/hackney_nccinteractionses({id})?$select=hackney_callbackmanageremailid,hackney_callbackofficeremailid,hackney_callbackphonenumber,hackney_notes, hackney_housingtagref, _hackney_enquirytypeid_value, hackney_name, _hackney_servicerequestid_value, _hackney_contactid_value, hackney_otherreason";
        }

        public static string GetHousingRefFromAccount(string parentCustomerid)
        {
            return $@"/api/data/v8.2/accounts({parentCustomerid})?$select=housing_tag_ref";
        }

        public static string UpdateCRMGovNotifierFields(string nccinteractionid)
        {
            return $@"/api/data/v8.2/hackney_nccinteractionses({nccinteractionid})?$select=hackney_nccinteractionsid, hackney_govnotifier_channeltype, hackney_govnotifier_templatename";
        }

        public static string UpdatePaymentRef(Payments payment)
        {
            return $@"/api/data/v8.2/hackney_nccinteractionses({payment.InteractionId})?$select=hackney_paymentreference, hackney_paymentstatus";
        }
        public static string getCRMEnquiryTypes()
        {
            return "/api/data/v8.2/housing_housingenquirytypes?$select=housing_housingenquirytypeid,housing_name,housing_servicepattern,housing_enquirycalltype";
        }


    }
}
