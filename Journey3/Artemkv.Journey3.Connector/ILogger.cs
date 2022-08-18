namespace Artemkv.Journey3.Connector
{
    public interface ILogger
    {
        void Info(string tag, string msg);
        void Warn(string tag, string msg);
    }
}
