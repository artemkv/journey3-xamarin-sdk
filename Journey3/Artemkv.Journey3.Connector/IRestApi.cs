using System.Threading.Tasks;

namespace Artemkv.Journey3.Connector
{
    public interface IRestApi
    {
        Task PostSessionHeaderAsync(SessionHeader header);
        Task PostSessionAsync(Session session);
    }
}
