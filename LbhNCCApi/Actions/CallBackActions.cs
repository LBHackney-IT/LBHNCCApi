using System;
using System.Net.Mail;
using System.Threading.Tasks;
using LbhNCCApi.Models;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using LbhNCCApi.Helpers;

namespace LbhNCCApi.Actions
{
    internal class CallBackActions
    {
        private string _MailHost = Environment.GetEnvironmentVariable("MailHost");
        private string _CallBackEmailSubject = Environment.GetEnvironmentVariable("CallBackEmailSubject");
        private string _CallBackEmailFrom = Environment.GetEnvironmentVariable("CallBackEmailFrom");

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
                strHTMLBodyOriginal = strHTMLBodyOriginal.Replace("[PHONENUMBER]", callback.CallBackId);
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
    }
}