using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Artemkv.Journey3.Connector
{
    [DataContract()]
    public class SessionHeader
    {
        public ITimeline Timeline { get; private set; }

        public IIdGenerator IdGenerator { get; private set; }

        [DataMember(Name = "t")]
        public string T { get { return "shead"; } }

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

        [DataMember(Name = "since")]
        public DateTime Since { get; set; }

        [DataMember(Name = "start")]
        public DateTime Start { get; private set; }

        [DataMember(Name = "fst_launch")]
        public bool FirstLaunch { get; set; }

        [DataMember(Name = "fst_launch_hour")]
        public bool FirstLaunchThisHour { get; set; }

        [DataMember(Name = "fst_launch_day")]
        public bool FirstLaunchToday { get; set; }

        [DataMember(Name = "fst_launch_month")]
        public bool FirstLaunchThisMonth { get; set; }

        [DataMember(Name = "fst_launch_year")]
        public bool FirstLaunchThisYear { get; set; }

        [DataMember(Name = "fst_launch_version")]
        public bool FirstLaunchThisVersion { get; set; }

        [DataMember(Name = "prev_stage")]
        public Stage PrevStage { get; set; }

        public SessionHeader(
            string accountId,
            string appId,
            string version,
            bool isRelease,
            ITimeline timeline,
            IIdGenerator idGenerator)
        {
            Timeline = timeline;
            IdGenerator = idGenerator;

            Id = idGenerator.GetNewId();

            Start = timeline.GetUtcNow();
            Since = timeline.GetUtcNow();

            AccountId = accountId;
            AppId = appId;
            Version = version;
            IsRelease = isRelease;

            PrevStage = Stage.NewUser(timeline);
        }

        public override bool Equals(object obj)
        {
            return obj is SessionHeader header &&
                   T == header.T &&
                   V == header.V &&
                   Id == header.Id &&
                   AccountId == header.AccountId &&
                   AppId == header.AppId &&
                   Version == header.Version &&
                   IsRelease == header.IsRelease &&
                   Since == header.Since &&
                   Start == header.Start &&
                   FirstLaunch == header.FirstLaunch &&
                   FirstLaunchThisHour == header.FirstLaunchThisHour &&
                   FirstLaunchToday == header.FirstLaunchToday &&
                   FirstLaunchThisMonth == header.FirstLaunchThisMonth &&
                   FirstLaunchThisYear == header.FirstLaunchThisYear &&
                   FirstLaunchThisVersion == header.FirstLaunchThisVersion &&
                   EqualityComparer<Stage>.Default.Equals(PrevStage, header.PrevStage);
        }

        // Don't use SessionHeader as a key in a dictionary
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
