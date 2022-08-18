using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;

namespace Artemkv.Journey3.Connector
{
    [DataContract()]
    public class Stage
    {
        public ITimeline Timeline { get; private set; }

        [DataMember(Name = "ts")]
        public DateTime Ts { get; private set; }

        [DataMember(Name = "stage")]
        public int Index { get; private set; }

        [DataMember(Name = "name")]
        public string Name { get; private set; }

        public Stage(int index, string name, ITimeline timeline)
        {
            Timeline = timeline;

            Ts = timeline.GetUtcNow();
            Index = index;
            Name = name;
        }

        public static Stage NewUser(ITimeline timeline)
        {
            return new Stage(1, "new_user", timeline);
        }

        public static Stage FromJson(string json, ITimeline timeline)
        {
            var node = JsonNode.Parse(json);

            DateTime ts =
                node["ts"] != null ?
                DateTime.Parse(node["ts"].ToString(), null, DateTimeStyles.RoundtripKind) :
                timeline.GetUtcNow();
            int index = node["stage"] != null ? node["stage"].GetValue<int>() : 1;
            string name = node["name"] != null ? node["name"].ToString() : "new_user";

            return new Stage(index, name, timeline) { Ts = ts };
        }

        public override bool Equals(object obj)
        {
            return obj is Stage stage &&
                   Ts == stage.Ts &&
                   Index == stage.Index &&
                   Name == stage.Name;
        }

        // Don't use Stage as a key in a dictionary
        public override int GetHashCode()
        {
            return Index;
        }
    }
}
