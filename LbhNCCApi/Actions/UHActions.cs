using LbhNCCApi.Models;
using System;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Dynamic;
using System.Net.Http;
using LBH.Utils;

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
	                    and deb_last_charge <> '1900-01-01 00:00:00' 
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

        public List<TenancyTransactions> GetAllTenancyTransactions(string tenancyAgreementRef, string startdate)
        {
            string fstartDate = Utils.FormatDate(startdate);
            string fendDate = DateTime.Now.ToString("yyyy-MM-dd"); 
            string query = $@" select  transno, rtrans.real_value as Amount, rtrans.post_date as date, rtrans.trans_type as Type,  RTRIM(rectype.rec_desc) AS Description
                    from rtrans join rectype on rtrans.trans_type = rectype.rec_code 
                    where tag_ref<> '' and tag_ref<> 'ZZZZZZ' 
                    and post_date BETWEEN '{fstartDate}' AND '{fendDate}' 
                    and tag_ref = '{tenancyAgreementRef}' 
                    and trans_type in 
                    (select rec_code from rectype where rec_group <= 8 or rec_code = 'RIT') 
                    union all 
                    select  999999999999999999 as transno,  sum(rtrans.real_value) as Amount, post_date as Date,'RNT' as Type, 'Total Charge' AS Description 
                    from rtrans 
                    where tag_ref <> '' and tag_ref<> 'ZZZZZZ' 
                    and tag_ref = '{tenancyAgreementRef}' 
                    and post_date BETWEEN '{fstartDate}' AND '{fendDate}' and rtrans.trans_type like 'D%' and post_date = post_date group by tag_ref,post_date,prop_ref,house_ref 
                    order by post_date desc, transno asc";
            var results = conn.Query<TenancyTransactions>(query, new { allRefs = tenancyAgreementRef }).ToList();
            return results;
        }

        public TenancyAgreementDetials GetTenancyAgreementDetails(string tenancyAgreementRef)
        {
            var result = conn.QueryFirstOrDefault<TenancyAgreementDetials>(
                $@" select cur_bal as CurrentBalance, (cur_bal*-1) as DisplayBalance, (rent+service+other_charge)  as Rent, cot as StartDate,  RTRIM(house_ref) as HousingReferenceNumber, RTRIM(prop_ref) as PropertyReferenceNumber,
                    RTRIM(u_saff_rentacc) as PaymentReferenceNumber, terminated as IsAgreementTerminated, tenure as  TenureType 
                    from tenagree
                    where  tag_ref = '{tenancyAgreementRef}' "
                    );
            return result;

        }

        public List<TenancyTransactionStatements> GetAllTenancyTransactionStatements(string tenancyAgreementId, string startdate)
        {
            TenancyAgreementDetials tenantDet = GetTenancyAgreementDetails(tenancyAgreementId);
            List<TenancyTransactions> lstTransactions = GetAllTenancyTransactions(tenancyAgreementId, startdate);
            List<TenancyTransactionStatements> lstTransactionsState = new List<TenancyTransactionStatements>();
            float RecordBalance = 0;
            RecordBalance = float.Parse(tenantDet.CurrentBalance);

            foreach (TenancyTransactions trans in lstTransactions)
            {
                TenancyTransactionStatements statement = new TenancyTransactionStatements();
                var DebitValue = "";
                var CreditValue = "";
                float fDebitValue = 0F;
                float fCreditValue = 0F;
                var realvalue = trans.Amount;
                string DisplayRecordBalance = (-RecordBalance).ToString("c2");

                if (realvalue.IndexOf("-") != -1)
                {
                    DebitValue = realvalue;
                    fDebitValue = float.Parse(DebitValue);
                    RecordBalance = (RecordBalance - fDebitValue);
                    DebitValue = (-fDebitValue).ToString("c2");
                }
                else
                {
                    CreditValue = realvalue;
                    fCreditValue = float.Parse(CreditValue);
                    RecordBalance = (RecordBalance - fCreditValue);
                    CreditValue = (-fCreditValue).ToString("c2");
                }
                statement.Date = trans.Date;
                statement.Description = trans.Description;
                statement.In = DebitValue;
                statement.Out = CreditValue;
                statement.Balance = DisplayRecordBalance;
                lstTransactionsState.Add(statement);
            }

            return lstTransactionsState;

        }
    }
}
