namespace Artemkv.Journey3.Connector
{
    public interface IPersistence
    {
        Session LoadLastSession();

        void SaveSession(Session session);
    }
}
