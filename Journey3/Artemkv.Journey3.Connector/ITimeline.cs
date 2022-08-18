using System;

namespace Artemkv.Journey3.Connector
{
    public interface ITimeline
    {
        DateTime GetUtcNow();
    }
}
