using System;

namespace Artemkv.Journey3.Connector
{
    public class IdGenerator : IIdGenerator
    {
        public string GetNewId()
        {
            return Guid.NewGuid().ToString("D");
        }
    }
}
