using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using LbhNCCApi.Actions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace LbhNCCApiTest.Controller.Actions
{
    public class ContactDetailsApiTests
    {
        private Mock<HttpMessageHandler> _messageHandler;
        private ContactDetailsApi _classUnderTest;

        public ContactDetailsApiTests()
        {
            var uri = new Uri("http://test-domain-name.com/");
            _messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            Environment.SetEnvironmentVariable("CONTACT_DETAILS_API_TOKEN", "super secret token");

            var httpClient = new HttpClient(_messageHandler.Object)
            {
                BaseAddress = uri,
            };
            _classUnderTest = new ContactDetailsApi(httpClient);
        }

        [Fact]
        public void PostContactDetailsCanParseAndPostContactDetails()
        {
            var contactId = "CBWU-47983-CWHUGH-86";
            var telephones = new List<string>{"02746284628"};
            var mobiles = new List<string>{"02746554628"};
            var emails = new List<string>{"hello@email.com"};
            var commsDetails = SerializeToCommsDetailsString(telephones, mobiles, emails, telephones.First(), "1111111", emails.First());

            var expectedPosts = new List<ContactDetailsApi.ContactDetailsPostRequest>
            {
                new ContactDetailsApi.ContactDetailsPostRequest
                {
                    Active = true,
                    Default = true,
                    Value = "02746284628",
                    TypeId = 1,
                    NccContactId = contactId
                },
                new ContactDetailsApi.ContactDetailsPostRequest
                {
                    Active = true,
                    Default = false,
                    Value = "02746554628",
                    TypeId = 3,
                    NccContactId = contactId
                },
                new ContactDetailsApi.ContactDetailsPostRequest
                {
                    Active = true,
                    Default = true,
                    Value = "hello@email.com",
                    TypeId = 2, // TODO: find out what the correct values for these are
                    NccContactId = contactId
                }
            };
            expectedPosts.ForEach(post =>
            {
                var expectedBody = JsonConvert.SerializeObject(post);
                SetUpMessageHandlerToReturnJson(_messageHandler, "/api/v1/contact-details", "POST", expectedBody);
            });

            _classUnderTest.PostContactDetails(contactId, commsDetails);
            _messageHandler.Verify();
        }

        private static string SerializeToCommsDetailsString(List<string> telephones, List<string> mobiles, List<string> emails,
            string defaultTelephone, string defaultMobile, string defaultEmail)
        {
            return
                $"{{ telephone: [\"{string.Join("\", \"", telephones)}\"], mobile: [\"{string.Join("\", \"", mobiles)}\"]," +
                $" email: [\"{string.Join("\", \"", emails)}\"], Default: {{ telephone: \"{defaultTelephone}\"," +
                $" mobile: \"{defaultMobile}\", email: \"{defaultEmail}\" }}  }}";
        }

        private static void SetUpMessageHandlerToReturnJson(Mock<HttpMessageHandler> messageHandler, string endpoint, string method, string
            expectedBody = null)
        {
            var stubbedResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
            };

            messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => CheckRequest(req, endpoint, method, expectedBody)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(stubbedResponse)
                .Verifiable();
        }

        private static bool CheckRequest(HttpRequestMessage req, string endpoint, string method, string expectedBody)
        {
            return req.Method.ToString() == method
                   && HttpUtility.UrlDecode(req.RequestUri.ToString()) == $"http://test-domain-name.com{endpoint}"
                   && req.Content.ReadAsStringAsync().Result == expectedBody;
        }
    }
}