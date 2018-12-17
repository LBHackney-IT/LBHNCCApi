using LbhNCCApi.Models;
using System;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Dynamic;
using System.Net.Http;

namespace LbhNCCApi.Actions
{
    public class UHActions
    {
        private readonly SqlConnection conn;
        private string _connstring = Environment.GetEnvironmentVariable("UHConnectionString");

        public UHActions()
        {
            conn = new SqlConnection(_connstring);
            conn.Open();
        }

        public List<ActionDiaryHistory> GetAllActionDiary(string tenancyAgreementRef)
        {
            var results = conn.Query<ActionDiaryHistory>(
                " select tag_ref TenancyRef, raaction.act_name ActionName, action_comment Comment, username Username, action_date CreationDate " +
                " from araction " +
                " inner join raaction on raaction.act_code = araction.action_code " +
                " where tag_ref = '"+ tenancyAgreementRef + "' " +
                " and action_date > DATEADD(Year, -1, GETDATE()) " +
                " and action_code <> 'INC' " +
                " order by araction.action_no desc ",
                new { allRefs = tenancyAgreementRef }
            ).ToList();
            return results;
        }

        public List<RentBreakdowns> GetAllRentBreakDowns(string tenancyAgreementRef)
        {
            var results = conn.Query<RentBreakdowns>(
                $@" SELECT  dt.deb_desc Description, dt.deb_code Code, deb_value Value, di.eff_date EffectiveDate, di.deb_last_charge LastChargeDate    
                    FROM debitem di 
                    inner join debtype dt on dt.deb_code = di.deb_code
                    WHERE debitem_sid IN
                    (
	                    SELECT Max(debitem_sid)
	                    FROM debitem di
	                    where (di.tag_ref = '{tenancyAgreementRef}' or di.prop_ref = (select prop_ref from tenagree where tag_ref = '{tenancyAgreementRef}'))     
	                    GROUP BY deb_code
                    ) ",
                new { allRefs = tenancyAgreementRef }
            ).ToList();
            return results;
        }

        public List<dynamic> GetAllActionDiaries(string tenancyAgreementRef)
        {
            var results = conn.Query<ADNotes>(
                " select action_date Date, 'Action Diary' Type, username Username, raaction.act_name Reason,  action_comment Notes " +
                " from araction " +
                " inner join raaction on raaction.act_code = araction.action_code " +
                " where tag_ref like '" + tenancyAgreementRef + "%' " +
                " and action_date > DATEADD(Year, -1, GETDATE()) " +
                " and action_code <> 'INC' " +
                " order by araction.action_no desc ",
                new { allRefs = tenancyAgreementRef }
            ).ToList();

            var nccList = new List<dynamic>();

            foreach (dynamic response in results)
            {
                dynamic interactionsObj = new ExpandoObject();
                interactionsObj.notes = response.Notes;
                interactionsObj.notesType = response.Type;
                interactionsObj.callType = "";
                interactionsObj.callReasonType = response.Reason;
                interactionsObj.createdBy = response.Username;
                interactionsObj.clientName = "";
                if (response.Date != null)
                {
                    interactionsObj.createdOn = response.Date;
                }
                nccList.Add(interactionsObj);
            }
            return nccList;
        }

    }
}
