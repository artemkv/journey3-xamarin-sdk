using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Artemkv.Journey3.Connector
{
    /// <summary>
    /// Journey3 connector
    /// </summary>
    public class Journey
    {
        private static readonly int MAX_SEQ_LENGTH = 100;

        private static Journey _instance = new Journey(
            new RestApi(), new Persistence(), new Timeline(), new IdGenerator(), new Logger());

        private readonly IRestApi _restApi;
        private readonly IPersistence _persistence;
        private readonly ITimeline _timeline;
        private readonly IIdGenerator _idGenerator;
        private readonly ILogger _logger;

        private Session _currentSession;
        private object _currentSessionLock = new object();

        /// <summary>
        /// Creates a new instance of Journey connector.
        /// Prefer using <code>GetInstance</code> instead.
        /// </summary>
        /// <param name="restApi">Rest Api</param>
        /// <param name="persistence">Persistence</param>
        /// <param name="timeline">Timeline</param>
        /// <param name="idGenerator">Id generator</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Journey(
            IRestApi restApi,
            IPersistence persistence,
            ITimeline timeline,
            IIdGenerator idGenerator,
            ILogger logger)
        {
            _restApi = restApi ?? throw new ArgumentNullException(nameof(restApi));
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Returns the instance of Journey connector.
        /// </summary>
        /// <returns>The instance of Journey connector</returns>
        public static Journey GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// Initializes new session.
        /// </summary>
        /// <param name="accountId">Your account id</param>
        /// <param name="appId">Your application id</param>
        /// <param name="version">The application version (e.g. 1.2.3)</param>
        /// <param name="isRelease">Used to separate debug sessions from release sessions</param>
        public async Task InitializeAsync(string accountId, string appId, string version, bool isRelease)
        {
            try
            {
                // start new session
                var header = new SessionHeader(accountId, appId, version, isRelease, _timeline, _idGenerator);
                var currentSession = new Session(header.Id, accountId, appId, version, isRelease, header.Start, _timeline);
                _logger.Info("Journey3", $"Started new session {currentSession.Id}");

                // report previous session
                var session = _persistence.LoadLastSession();
                if (session != null)
                {
                    _logger.Info("Journey3", "Report the end of the previous session");
                    try
                    {
                        await _restApi.PostSessionAsync(session);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Journey3", $"Failed to report the end of the previous session: {ex}");
                    }
                }

                // update current session based on the previous one
                if (session == null)
                {
                    header.FirstLaunch = true;
                    currentSession.FirstLaunch = true;

                    header.FirstLaunchThisHour = true;
                    header.FirstLaunchToday = true;
                    header.FirstLaunchThisMonth = true;
                    header.FirstLaunchThisYear = true;
                    header.FirstLaunchThisVersion = true;
                }
                else
                {
                    var today = _timeline.GetUtcNow();
                    var lastSessionStart = session.Start;

                    if (!lastSessionStart.IsSameHour(today))
                    {
                        header.FirstLaunchThisHour = true;
                    }
                    if (!lastSessionStart.IsSameDay(today))
                    {
                        header.FirstLaunchToday = true;
                    }
                    if (!lastSessionStart.IsSameMonth(today))
                    {
                        header.FirstLaunchThisMonth = true;
                    }
                    if (!lastSessionStart.IsSameYear(today))
                    {
                        header.FirstLaunchThisYear = true;
                    }
                    if (session.Version != version)
                    {
                        header.FirstLaunchThisVersion = true;
                    }

                    currentSession.PrevStage = session.NewStage;
                    currentSession.NewStage = session.NewStage;
                    header.PrevStage = session.NewStage;

                    header.Since = session.Since;
                    currentSession.Since = session.Since;
                }

                // save current session
                lock (_currentSessionLock)
                {
                    _currentSession = currentSession;
                    _persistence.SaveSession(_currentSession);
                }

                // report the new session (header)
                _logger.Info("Journey3", "Report the start of a new session");
                await _restApi.PostSessionHeaderAsync(header);
            }
            catch (Exception ex)
            {
                _logger.Warn("Journey3", $"Failed to initialize Journey: {ex}");
            }
        }

        /// <summary>
        /// Registers the event in the current session.
        /// 
        /// Events are distinguished by name, for example "click_play",
        /// "add_to_library" or "use_search".
        /// Short and clear names are recommended.
        /// 
        /// Do not include any personal data as an event name!
        /// 
        /// Specify whether event is collapsible.
        /// Collapsible events will only appear in the sequence once.
        /// Make events collapsible when number of times it is repeated is not
        /// important. For example, if your application is music play app, where the
        /// users normally browse through the list of albums before clicking "play",
        /// "scroll_to_next_album" event would probably be a good candidate to be
        /// made collapsible, while "click_play" event would probably not.
        ///
        /// Collapsible event names appear in brackets in the sequence,
        /// for example "(scroll_to_next_album)".
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="isCollapsible">Whether event is collapsible</param>
        public void ReportEvent(string eventName, bool isCollapsible = false)
        {
            ReportEvent(eventName, isCollapsible: isCollapsible, isError: false, isCrash: false);
        }

        /// <summary>
        /// Registers the error event in the current session.
        /// 
        /// Errors are just special types of events.
        /// Events are distinguished by name, for example "error_fetching_data"
        /// or "error_playing_song".
        /// Short and clear names are recommended.
        /// 
        /// Do not include any personal data as an event name!
        /// </summary>
        /// <param name="eventName">The event name</param>
        public void ReportError(string eventName)
        {
            ReportEvent(eventName, isCollapsible: false, isError: true, isCrash: false);
        }

        /// <summary>
        /// Registers the crash event in the current session.
        /// 
        /// Crashes are just special types of events.
        /// Events are distinguished by name, you can simply specify "crash" for a crash event.
        /// Short and clear names are recommended.
        /// 
        /// Do not include any personal data as an event name!
        /// </summary>
        /// <param name="eventName">The event name</param>
        public void ReportCrash(string eventName)
        {
            ReportEvent(eventName, isCollapsible: false, isError: true, isCrash: true);
        }

        /// <summary>
        /// Reports the stage transition, e.g. "engagement", "checkout", "payment".
        /// Stage transitions are used to build funnels.
        /// 
        /// Stage is an ordinal number [1..10] that defines the stage.
        /// Stage transitions must be increasing. If the current session is already
        /// at the higher stage, the call will be ignored.
        /// This means you don't need to keep track of a current stage.
        ///
        /// Stage name provides the stage name for informational purposes.
        ///
        /// It is recommended to define stages upfront as the numbers used to build
        /// conversion funnel.
        /// If you sumbit the new name for the same stage, that new name will be used
        /// in all future reports.
        /// </summary>
        /// <param name="stage">An ordinal number that defines the stage</param>
        /// <param name="stageName">The stage name</param>
        public void ReportStageTransition(int stage, string stageName)
        {
            if (string.IsNullOrWhiteSpace(stageName))
                throw new ArgumentException(nameof(stageName));

            if (stage < 1 || stage > 10)
            {
                throw new ArgumentException("Invalid value $stage for stage, must be between 1 and 10");
            }

            lock (_currentSessionLock)
            {
                if (_currentSession == null)
                {
                    _logger.Warn("Journey3", "Cannot update session. Journey have not been initialized.");
                    return;
                }

                try
                {
                    if (_currentSession.NewStage.Index < stage)
                    {
                        _currentSession.NewStage = new Stage(stage, stageName, _timeline);
                    }

                    // update endtime
                    _currentSession.End = _timeline.GetUtcNow();

                    // save session
                    _persistence.SaveSession(_currentSession);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Journey3", $"Cannot update session: {ex}");
                }
            }
        }

        private void ReportEvent(
            string eventName,
            bool isCollapsible = false,
            bool isError = false,
            bool isCrash = false)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentException(nameof(eventName));

            lock (_currentSessionLock)
            {
                if (_currentSession == null)
                {
                    _logger.Warn("Journey3", "Cannot update session. Journey have not been initialized.");
                    return;
                }

                try
                {
                    // count events
                    _currentSession.EventCounts.TryGetValue(eventName, out int count);
                    _currentSession.EventCounts[eventName] = count + 1;

                    // set error
                    if (isError)
                    {
                        _currentSession.HasError = true;
                    }
                    if (isCrash)
                    {
                        _currentSession.HasCrash = true;
                    }

                    // sequence events
                    var seq = _currentSession.EventSequence;
                    if (seq.Count < MAX_SEQ_LENGTH)
                    {
                        var seqEventName = isCollapsible ? $"({eventName})" : eventName;
                        if (!(seq.Count > 0 && seq.Last() == seqEventName && isCollapsible))
                        {
                            seq.Add(seqEventName);
                        }
                        else
                        {
                            // ignore the event for the sequence
                        }
                    }

                    // update endtime
                    _currentSession.End = _timeline.GetUtcNow();

                    // save session
                    _persistence.SaveSession(_currentSession);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Journey3", $"Cannot update session: {ex}");
                }
            }
        }
    }
}
