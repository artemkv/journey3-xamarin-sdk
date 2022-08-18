using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;

namespace Artemkv.Journey3.Connector
{
    [DataContract()]
    public class Session
    {
        public ITimeline Timeline { get; private set; }

        [DataMember(Name = "t")]
        public string T { get { return "stail"; } }

        [DataMember(Name = "v")]
        public string V { get { return "1.1.0"; } }

        [DataMember(Name = "id")]
        public string Id { get; private set; }

        [DataMember(Name = "acc")]
        public string AccountId { get; private set; }

        [DataMember(Name = "aid")]
        public string AppId { get; private set; }

        [DataMember(Name = "version")]
        public string Version { get; private set; }

        [DataMember(Name = "is_release")]
        public bool IsRelease { get; private set; }

        [DataMember(Name = "start")]
        public DateTime Start { get; set; }

        [DataMember(Name = "end")]
        public DateTime End { get; set; }

        [DataMember(Name = "since")]
        public DateTime Since { get; set; }

        [DataMember(Name = "fst_launch")]
        public bool FirstLaunch { get; set; }

        [DataMember(Name = "prev_stage")]
        public Stage PrevStage { get; set; }

        [DataMember(Name = "new_stage")]
        public Stage NewStage { get; set; }

        [DataMember(Name = "has_error")]
        public bool HasError { get; set; }

        [DataMember(Name = "has_crash")]
        public bool HasCrash { get; set; }

        [DataMember(Name = "evts")]
        public Dictionary<string, int> EventCounts { get; private set; }

        [DataMember(Name = "evt_seq")]
        public List<string> EventSequence { get; private set; }

        public Session(
            string id,
            string accountId,
            string appId,
            string version,
            bool isRelease,
            DateTime start,
            ITimeline timeline)
        {
            Timeline = timeline;

            Id = id;
            AccountId = accountId;
            AppId = appId;
            Version = version;
            IsRelease = isRelease;

            Start = start;
            End = start;
            Since = start;

            PrevStage = Stage.NewUser(timeline);
            NewStage = Stage.NewUser(timeline);

            EventCounts = new Dictionary<string, int>();
            EventSequence = new List<string>();
        }

        public static Session FromJson(string json, ITimeline timeline, IIdGenerator idGenerator)
        {
            var node = JsonNode.Parse(json);

            string id = node["id"] != null ? node["id"].ToString() : idGenerator.GetNewId();
            DateTime since =
                node["since"] != null ?
                DateTime.Parse(node["since"].ToString(), null, DateTimeStyles.RoundtripKind) :
                timeline.GetUtcNow();
            DateTime start =
                node["since"] != null ?
                DateTime.Parse(node["start"].ToString(), null, DateTimeStyles.RoundtripKind) :
                timeline.GetUtcNow();
            DateTime end =
                node["since"] != null ?
                DateTime.Parse(node["end"].ToString(), null, DateTimeStyles.RoundtripKind) :
                timeline.GetUtcNow();

            string accountId = node["acc"] != null ? node["acc"].ToString() : "";
            string appId = node["aid"] != null ? node["aid"].ToString() : "";
            string version = node["version"] != null ? node["version"].ToString() : "";
            bool isRelease = node["is_release"] != null && node["is_release"].GetValue<bool>();

            bool firstLaunch = node["fst_launch"] != null && node["fst_launch"].GetValue<bool>();
            bool hasError = node["has_error"] != null && node["has_error"].GetValue<bool>();
            bool hasCrash = node["has_crash"] != null && node["has_crash"].GetValue<bool>();

            Dictionary<string, int> eventCounts =
                node["evts"] != null ?
                JsonConvert.DeserializeObject<Dictionary<string, int>>(node["evts"].ToString()) :
                new Dictionary<string, int>();
            List<string> eventSequence =
                node["evt_seq"] != null ?
                JsonConvert.DeserializeObject<List<string>>(node["evt_seq"].ToString()) :
                new List<string>();

            Stage prevStage =
                node["prev_stage"] != null ?
                Stage.FromJson(node["prev_stage"].ToString(), timeline) :
                Stage.NewUser(timeline);
            Stage newStage =
                node["new_stage"] != null ?
                Stage.FromJson(node["new_stage"].ToString(), timeline) :
                Stage.NewUser(timeline);

            return new Session(
                id,
                accountId,
                appId,
                version,
                isRelease,
                start,
                timeline)
            {
                Since = since,
                End = end,
                FirstLaunch = firstLaunch,
                HasError = hasError,
                HasCrash = hasCrash,
                EventCounts = eventCounts,
                EventSequence = eventSequence,
                PrevStage = prevStage,
                NewStage = newStage,
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Session session &&
                   T == session.T &&
                   V == session.V &&
                   Id == session.Id &&
                   AccountId == session.AccountId &&
                   AppId == session.AppId &&
                   Version == session.Version &&
                   IsRelease == session.IsRelease &&
                   Start == session.Start &&
                   End == session.End &&
                   Since == session.Since &&
                   FirstLaunch == session.FirstLaunch &&
                   EqualityComparer<Stage>.Default.Equals(PrevStage, session.PrevStage) &&
                   EqualityComparer<Stage>.Default.Equals(NewStage, session.NewStage) &&
                   HasError == session.HasError &&
                   HasCrash == session.HasCrash &&
                   EventCounts.Count == session.EventCounts.Count &&
                   !EventCounts.Except(session.EventCounts).Any() &&
                   !session.EventCounts.Except(EventCounts).Any() &&
                   EventSequence.SequenceEqual(session.EventSequence);
        }

        // Don't use Session as a key in a dictionary
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
