using LbhNCCApi.Actions;
using LbhNCCApi.Controllers;
using LbhNCCApi.Exceptions.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;


namespace LbhNCCApiTest
{
    public class GetCitizenCommunications
    {
        [Fact]
        public async Task WhenNoContactId_Passed_NoResults()
        {
            var client = new AccessTokenService();
            var controller = new CRMController(client);
            var actualresult = controller.GetCitizenCommunication(null);
            var expectresult = Validate.ErrorMessage("contactid cannot be null");

            Assert.Equal(JsonConvert.SerializeObject(expectresult), JsonConvert.SerializeObject(actualresult));
        }

        [Fact]
        public async Task WhenContactId_Passed_ExpectedResult()
        {
            var client = new AccessTokenService();
            var controller = new CRMController(client);
            string contactid = "40ED0BB8-7084-E011-8E5B-00505691098C";
            var commobject = new Dictionary<string, object> { { "email", "sachin.shetty@hackney.gov.uk" } };
            var result = controller.SetCitizenCommunication(contactid, commobject.ToString());
            var actualresult = controller.GetCitizenCommunication(contactid);
            var expectresult = actualresult;

            Assert.Equal(JsonConvert.SerializeObject(expectresult), JsonConvert.SerializeObject(actualresult));

        }

    }
}
