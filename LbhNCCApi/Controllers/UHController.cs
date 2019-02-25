using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using ArrearsAgreementService;
using LbhNCCApi.Actions;
using LbhNCCApi.Helpers;
using LbhNCCApi.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LbhNCCApi.Controllers
{
    [Produces("application/json")]
    [Route("api/UH")]
    public class UHController : Controller
    {

        private ICRMClientActions _client = null;
        private string UHWWebservice = Environment.GetEnvironmentVariable("UHWWebservice");
        private string UHWSUsername = Environment.GetEnvironmentVariable("UHWSUsername");
        private string UHWPassword = Environment.GetEnvironmentVariable("UHWPassword");
        public UHController(ICRMClientActions client)
        {
            _client = client;
        }

        [HttpPost]
        [Route("CreateArearsActionDiary")]
        public async Task<IActionResult> CreateArearsActionDiary(string tenancyAgreementId, string notes)
        {
            try
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                EndpointAddress address = new EndpointAddress(UHWWebservice);
                var client = new ArrearsAgreementServiceClient(binding, address);

                var request = new ArrearsActionCreateRequest
                {
                    ArrearsAction = new ArrearsActionInfo
                    {
                        ActionCode = "INC",
                        Comment = notes,
                        TenancyAgreementRef = tenancyAgreementId
                    },
                    DirectUser = new UserCredential
                    {
                        UserName = UHWSUsername,
                        UserPassword = UHWPassword
                    },
                    SourceSystem = "HackneyAPI"
                };
                var response = await client.CreateArrearsActionAsync(request);
                if (response.Success)
                {
                    return Ok(response.ArrearsAction.ActionBalance.ToString());
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        [HttpPost]
        [Route("AddTenancyAgreementNotes")]
        public async Task<IActionResult> AddTenancyAgreementNotes(string TenancyAgreementId, string Notes, string Username)
        {
            try
            {
                UHActions uh = new UHActions();
                var result = uh.AddTenancyAgreementNotes(TenancyAgreementId, Notes, Username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }


        [HttpGet]
        [Route("GetAllArearsActionDiary")]
        public async Task<IActionResult> GetAllArearsActionDiary(string tenancyAgreementId)
        {
            try
            {
                UHActions uh = new UHActions();
                var result = uh.GetAllActionDiary(tenancyAgreementId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        [HttpGet]
        [Route("GetAllRentBreakdowns")]
        public async Task<IActionResult> GetAllRentBreakdowns(string tenancyAgreementId)
        {
            try
            {
                UHActions uh = new UHActions();
                var result = uh.GetAllRentBreakDowns(tenancyAgreementId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        [HttpGet]
        [Route("GetAllActionDiaryAndNotes")]
        public async Task<IActionResult> GetAllActionDiaryAndNotes(string housingRef)
        {
            try
            {
                if (!string.IsNullOrEmpty(housingRef))
                {
                    List<dynamic> notesAD = null;
                    List<dynamic> notes = null;

                    HttpClient hclient = _client.GetCRMClient(true);

                    List<dynamic> notesCRM = CRMActions.GetAllHousingRefNotes(housingRef, hclient);

                    UHActions uh = new UHActions();
                    notesAD = uh.GetAllActionDiaries(housingRef.ToString());
                    if (notesAD != null)
                    {
                        foreach (var note in notesAD)
                        {
                            notesCRM.Add(note);
                        }
                    }

                    notes = uh.GetAllNotes(housingRef.ToString());
                    if (notes != null)
                    {
                        foreach (var note in notes)
                        {
                            notesCRM.Add(note);
                        }
                    }

                    var result = new List<dynamic>
                    {
                        notesCRM
                    };
                    return Ok(result);

                }
                return StatusCode(StatusCodes.Status500InternalServerError, "Housing Ref not provided.");
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        [HttpGet]
        [Route("GetAllTenancyTransactions")]
        public async Task<IActionResult> GetAllTenancyTransactions(string tenancyAgreementId, string startdate)
        {
            try
            {
                UHActions uh = new UHActions();
                var result = uh.GetAllTenancyTransactions(tenancyAgreementId, startdate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        [HttpGet]
        [Route("GetAllTenancyTransactionStatements")]
        public async Task<IActionResult> GetAllTenancyTransactionStatements(string tenancyAgreementId, string startdate)
        {
            try
            {
                UHActions uh = new UHActions();
                var result = uh.GetAllTenancyTransactionStatements(tenancyAgreementId, startdate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

        [HttpGet]
        [Route("GetTenancyAgreementDetails")]
        public async Task<IActionResult> GetTenancyAgreementDetails(string tenancyAgreementId)
        {
            try
            {
                UHActions uh = new UHActions();
                var result = uh.GetTenancyAgreementDetails(tenancyAgreementId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new Trap().ThrowErrorMessage(ex);
            }
        }

    }
}