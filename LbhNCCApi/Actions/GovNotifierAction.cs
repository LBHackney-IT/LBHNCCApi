using System;
using LbhNCCApi.Models;
using Dapper;
using System.Data.SqlClient;
using LbhNCCApi.Helpers;

namespace LbhNCCApi.Actions
{
    public class GovNotifierAction
    {
        private static string _connstring = Environment.GetEnvironmentVariable("CRM365BISQLCloudConnString");
        public GovNotifierAction()
        {
        }

        public static object SendGovNotifyEmailStatements(GovNotifierEmailPdfInParams inParams)
        {
            try
            {
                string insertQuery = $@"SET LANGUAGE British; INSERT INTO [dbo].[LBH_Ext_GovNotifyEmailStatements]
                                            ([ContactId], [TenancyAgreementRef], [StartDate], [EndDate], [GovTemplateId], [GovTemplateData],
                                            [EmailId], [Status], [StatusDescription], [DebugErrorMessage]) 
                                            VALUES ('{inParams.ContactId}', '{inParams.TenancyAgreementRef}','{inParams.StartDate}', '{inParams.EndDate}', '{inParams.TemplateId}', '{inParams.TemplateData}', 
                                            '{inParams.EmailTo}', '1', 'Initiate', 'Initiated for Pdf generation')";
                int result = -1;
                using (var conn = new SqlConnection(_connstring))
                {
                    result = conn.Execute(insertQuery);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }
    }
}
