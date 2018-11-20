using LbhNCCApi.Actions;
namespace LbhNCCApi.Interfaces
{
    public interface ICRMLookup
    {
         Lookup Execute(string token);
    }
}