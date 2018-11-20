using System.Net.Http;

namespace LbhNCCApi.Interfaces
{
    public interface ICRMClientActions
    {
        HttpClient GetCRMClient(bool formatter);
    }
}