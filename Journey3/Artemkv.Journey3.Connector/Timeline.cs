using System;

namespace Artemkv.Journey3.Connector
{
    public class Timeline : ITimeline
    {
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
