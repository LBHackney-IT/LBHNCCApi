using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LbhNCCApi.Helpers;
using LbhNCCApi.Models;
using NCCSSO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LbhNCCApi.Actions
{
    public class SSOAction
    {
        static string _DomainController = Environment.GetEnvironmentVariable("LDAP");
        public static async Task<object> AuthenticateUser(string userData, HttpClient client)
        {
            try
            {
                SSOUserData ssouser = GetUsername(userData);
                if (ssouser.success)
                {
                    GetUserEmailfromActiveDirectory(ssouser);
                    if(ssouser.success)
                    {
                        GetCRMUserDetails(ssouser, client);
                    }
                }
                var result = new Dictionary<string, object>
                {
                    {"response", new Dictionary<string, object>{
                            {"UserData", ssouser}
                        }
                    }
                };
                return result;
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        public static void GetUserEmailfromActiveDirectory(SSOUserData ssouser)
        {
            DirectoryEntry dEntry = new DirectoryEntry(_DomainController);
            DirectorySearcher dSearcher = new DirectorySearcher();
            dSearcher.Filter = string.Format("(&(objectcategory=user)(sAMAccountName={0}))", ssouser.username);
            var sResults = dSearcher.FindOne();
            if (sResults != null)
            {
                ssouser.success = true;
                ssouser.useremail = sResults.GetDirectoryEntry().Properties["mail"].Value.ToString();
            }
            else
            {
                ssouser.success = false;
                ssouser.message = "User not found Hackneys Active Directory";
            }
        }

        public static void GetCRMUserDetails(SSOUserData ssouser, HttpClient client)
        {
            HttpResponseMessage result = null;
            try
            {
                var query = CRMAPICall.getUserDetails(ssouser.useremail);

                result = CRMAPICall.getAsyncAPI(client, query).Result;
                if (result != null)
                {
                    if (result.StatusCode == HttpStatusCode.OK) //200 
                    {
                        var resp = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result);
                        if (resp?["value"] != null)
                        {
                            var retUserDetails = resp["value"].ToList();
                            if (retUserDetails.Count > 0)
                            {
                                ssouser.userid = retUserDetails[0]["systemuserid"].ToString();
                                ssouser.fullname = retUserDetails[0]["fullname"].ToString();
                                ssouser.roles = new List<string>();
                                foreach (var userDetails in retUserDetails)
                                {
                                    ssouser.roles.Add(userDetails["role2_x002e_name"].ToString());
                                }
                            }
                            else
                            {
                                ssouser.success = false;
                                ssouser.message =  string.Format("Couldnt find user {0} with email id {1} in CRM 365", 
                                                                        ssouser.username, ssouser.useremail);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError($"Get Tenancy Management Group Tray Interaction Error " + ex.Message);
                throw ex;

            }


        }

        private static SSOUserData GetUsername(string userData)
        {
            SSOUserData duserData = new SSOUserData();
            var data = NCCCrypto.Decrypt(userData);
            var splitdata = data.Split("$");
            if(splitdata.Length>0)
            {
                var username = splitdata[0];
                var datestamp = splitdata[1];
                DateTime dtReturned = DateTime.Parse(datestamp);
                var diff = DateTime.Now.Subtract(dtReturned);
                if (diff.Minutes > 5)
                {
                    duserData.success = false;
                    duserData.message = "Session for user login has been expired";
                }
                else
                {
                    duserData.success = true;
                    duserData.username = username;
                }
            }
            else
            {
                duserData.success = false;
                duserData.message = "Username has been malformed";
            }
            return duserData;
        }
    }
}
