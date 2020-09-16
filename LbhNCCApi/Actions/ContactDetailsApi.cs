using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LbhNCCApi.Interfaces;
using Newtonsoft.Json;

namespace LbhNCCApi.Actions
{
    public class ContactDetailsApi : IContactDetailsApi
    {
        private readonly HttpClient _httpClient;

        public ContactDetailsApi(HttpClient httpClient)
        {

            _httpClient = httpClient;
        }

        public async Task PostContactDetails(string contactId, string commsDetail)
        {
            var deserializedDetails = JsonConvert.DeserializeObject<CommunicationDetails>(commsDetail);
            var telephones = deserializedDetails.telephone.Select(t => new ContactDetailsPostRequest
            {
                Active = true,
                Default = deserializedDetails.Default.telephone == t,
                Value = t,
                TypeId = 1,
                NccContactId = contactId
            });
            var mobiles = deserializedDetails.mobile.Select(m => new ContactDetailsPostRequest
            {
                Active = true,
                Default = deserializedDetails.Default.mobile == m,
                Value = m,
                TypeId = 3,
                NccContactId = contactId
            });
            var emails = deserializedDetails.email.Select(e => new ContactDetailsPostRequest
            {
                Active = true,
                Default = deserializedDetails.Default.email == e,
                Value = e,
                TypeId = 2,
                NccContactId = contactId
            });
            var allContacts = telephones.Concat(mobiles).Concat(emails);
            foreach (var contact in allContacts)
            {
                var jsonRequestBody = JsonConvert.SerializeObject(contact);
                await _httpClient.PostAsync("/api/v1/contact-details", new StringContent(jsonRequestBody));
            }
        }

        public class ContactDetailsPostRequest
        {
            public string NccContactId { get; set; }
            public string Value { get; set; }
            public bool Active { get; set; }
            public bool Default { get; set; }
            public int TypeId { get; set; }
            public int SubtypeId { get; set; }
        }

        public class CommunicationDetails
        {
            public List<string> telephone { get; set; }
            public List<string> mobile { get; set; }
            public List<string> email { get; set; }
            public DefaultContacts Default { get; set; }
        }

        public class DefaultContacts
        {
            public string telephone;
            public string mobile;
            public string email;
        }
    }
}