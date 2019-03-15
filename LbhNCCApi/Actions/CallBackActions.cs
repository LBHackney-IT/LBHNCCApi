using System;
using System.Net.Mail;
using System.Threading.Tasks;
using LbhNCCApi.Models;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using LbhNCCApi.Helpers;
using System.Net.Http;
using LbhNCCApi.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.DirectoryServices;
using LBH.Utils;

namespace LbhNCCApi.Actions
{
    internal class CallBackActions
    {
        private string _MailHost = Environment.GetEnvironmentVariable("MailHost");
        private string _CallBackEmailSubject = Environment.GetEnvironmentVariable("CallBackEmailSubject");
        private string _CallBackEmailFrom = Environment.GetEnvironmentVariable("CallBackEmailFrom");
        private string _CallBackUrl = Environment.GetEnvironmentVariable("CallBackUrl");
        static string _DomainController = Environment.GetEnvironmentVariable("LDAP");

        public CallBackActions()
        {
        }

        public async Task<object> SendCallbackEmail(CallbackRequest callback)
        {
            try
            {
                bool retVal = SendEmail(callback);
                if (retVal)
                {
                    var result = new Dictionary<string, object>
                    {
                        {
                            "response", new Dictionary<string, object>{
                                    { "message", "Successfully sent email messages"}
                            }
                        }
                    };
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        bool SendEmail(CallbackRequest callback)
        {
            bool breturn = false;
            try
            {
                SmtpClient client = new SmtpClient();
                client.Port = 25;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = _MailHost;
                string strHTMLBodyOriginal = File.ReadAllText(Environment.CurrentDirectory + "/Templates/CallBackEmail.html");
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[CALLBACKID]", callback.CallBackId);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[CALLERFULLNAME]", callback.CallersFullname);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[REFERENCE]", callback.HousingTagRef);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[PHONENUMBER]", callback.PhoneNumber);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[ADDRESS]", callback.Address);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[AGENTNAME]", callback.AgentName);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[MESSAGE]", callback.MessageForEmail);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[CALLBACKURL]", _CallBackUrl);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[TOADDRESSES]", callback.RecipientEmailId);
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[CCADDRESSES]", callback.ManagerEmailId);
                string[] OfficersRecipeints = callback.RecipientEmailId.Split(';');
                foreach(var recipient in OfficersRecipeints)
                {
                    string strHTMLBody = strHTMLBodyOriginal.Replace("[EMAILRECIPIENT]", recipient);
                    MailMessage mail = new MailMessage(_CallBackEmailFrom, recipient, _CallBackEmailSubject, strHTMLBody);
                    mail.IsBodyHtml = true;

                    client.Send(mail);
                }

                if(callback.ManagerEmailId!=null)
                {
                    string[] ManagerRecipeints = callback.ManagerEmailId.Split(';');
                    foreach (var recipient in ManagerRecipeints)
                    {
                        string strHTMLBody = strHTMLBodyOriginal.Replace("[EMAILRECIPIENT]", recipient);
                        MailMessage mail = new MailMessage(_CallBackEmailFrom, recipient, _CallBackEmailSubject, strHTMLBody);
                        mail.IsBodyHtml = true;

                        client.Send(mail);
                    }
                }
                breturn = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return breturn;
        }

        public static async Task<object> SearchUsersFromActiveDirectory(string username)
        {
            DirectoryEntry dEntry = new DirectoryEntry(_DomainController);
            DirectorySearcher dSearcher = new DirectorySearcher();
            dSearcher.Filter = string.Format("(&(objectcategory=user)(name={0}*))", username);
            SearchResultCollection sResultsList = dSearcher.FindAll();

            List<ActiveDirectory> ADList = new List<ActiveDirectory>();
            if (sResultsList != null)
            {
                for (int i = 0; i < sResultsList.Count; i++)
                {
                    SearchResult crt = sResultsList[i];
                    PropertyCollection properties = crt.GetDirectoryEntry().Properties;
                    if(properties["mail"].Value!=null)
                    {
                        string email = Utils.NullToString(properties["mail"].Value);
                        if (email.ToLower().EndsWith("hackney.gov.uk"))
                        {
                            ActiveDirectory ad = new ActiveDirectory();
                            ad.Name = Utils.NullToString(properties["name"].Value);
                            ad.Email = email;
                            ad.Username = Utils.NullToString(properties["sAMAccountName"].Value);
                            ADList.Add(ad);
                        }
                    }
                }
            }
            var retResult = new Dictionary<string, object>
            {
                {
                    "response", new Dictionary<string, object>{
                            { "ADList", ADList}
                    }
                }
            };
            return retResult;
        }

        public static async Task<object> GetCallBackDetails(HttpClient hclient, string callbackId)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.GetCallBackDetails(callbackId);
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
                        var retResult = new Dictionary<string, object>
                        {
                            {
                                "response", new Dictionary<string, object>{
                                        { "nccinteractionsid", response["hackney_nccinteractionsid"]},
                                        { "housingtagref", response["hackney_housingtagref"]},
                                        { "calltypereasonid", response["_hackney_enquirytypeid_value"]},
                                        { "calltypeotherreason", response["hackney_otherreason"]},
                                        { "callbackmanageremailid", response["hackney_callbackmanageremailid"]},
                                        { "callbackofficeremailid", response["hackney_callbackofficeremailid"]},
                                        { "callbackphonenumber", response["hackney_callbackphonenumber"]},
                                        { "notes", response["hackney_notes"]},
                                        { "servicerequestid", response["_hackney_servicerequestid_value"]},
                                        { "contactid", response["_hackney_contactid_value"]},
                                        { "ticketnumber", response["hackney_name"]}

                                }
                            }
                        };
                        return retResult;
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

    }
}