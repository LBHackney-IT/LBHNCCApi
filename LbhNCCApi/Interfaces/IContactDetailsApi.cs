using System.Threading.Tasks;

namespace LbhNCCApi.Interfaces
{
    public interface IContactDetailsApi
    {
        Task PostContactDetails(string contactId, string commsDetail);
    }
}