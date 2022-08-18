using Android.Util;

namespace Artemkv.Journey3.Connector
{
    public class Logger : ILogger
    {
        public void Info(string tag, string msg)
        {
            Log.Info(tag, msg);
        }

        public void Warn(string tag, string msg)
        {
            Log.Warn(tag, msg);
        }
    }
}
