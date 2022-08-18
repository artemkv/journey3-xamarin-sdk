using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Artemkv.Journey3.Connector.Test
{
    [TestClass]
    public class JourneyTest
    {
        private static readonly string PREV_SESSION_ID = "SESSION0";
        private static readonly string SESSION_ID = "SESSION1";
        private static readonly string ACCOUNT_ID = "accid";
        private static readonly string APP_ID = "appid";
        private static readonly string PREV_VERSION = "1.0";
        private static readonly string VERSION = "2.0";
        private static readonly bool RELEASE_BUILD = true;
        private static readonly string CLICK_PLAY = "click_play";
        private static readonly string CLICK_PAUSE = "click_pause";
        private static readonly string NAVIGATE = "navigate";
        private static readonly string ERROR = "error";
        private static readonly string CRASH = "crash";
        private static readonly DateTime LAST_YEAR = new DateTime(2021, 1, 1);
        private static readonly DateTime NOW = new DateTime(2022, 1, 1);
        private static readonly DateTime LATER = new DateTime(2023, 1, 1);

        [TestMethod]
        public async Task TestReportTheVeryFirstSession()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);

            // Act
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Verify
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
            };
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Once());

            var expectedHeader = new SessionHeader(
                ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, timelineMock.Object, idGeneratorMock.Object)
            {
                FirstLaunch = true,
                FirstLaunchThisHour = true,
                FirstLaunchToday = true,
                FirstLaunchThisMonth = true,
                FirstLaunchThisYear = true,
                FirstLaunchThisVersion = true,
            };
            restApiMock.Verify(x => x.PostSessionHeaderAsync(expectedHeader), Times.Once());
        }

        [TestMethod]
        public async Task TestRestoreAndReportPreviousSession()
        {
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(LAST_YEAR);

            var stage2 = new Stage(2, "Stage 2", timelineMock.Object);
            var stage3 = new Stage(3, "Stage 3", timelineMock.Object);
            var prevSession = new Session(
                PREV_SESSION_ID, ACCOUNT_ID, APP_ID, PREV_VERSION, RELEASE_BUILD, LAST_YEAR, timelineMock.Object)
            {
                PrevStage = stage2,
                NewStage = stage3,
            };

            persistenceMock.Setup(x => x.LoadLastSession()).Returns(prevSession);
            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);

            // Act
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Verify
            persistenceMock.Verify(x => x.LoadLastSession(), Times.Once());

            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                PrevStage = stage3,
                NewStage = stage3,
                Since = LAST_YEAR
            };
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Once());

            var expectedHeader = new SessionHeader(
                ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, timelineMock.Object, idGeneratorMock.Object)
            {
                FirstLaunchThisHour = true,
                FirstLaunchToday = true,
                FirstLaunchThisMonth = true,
                FirstLaunchThisYear = true,
                FirstLaunchThisVersion = true,
                PrevStage = stage3,
                Since = LAST_YEAR,
            };
            restApiMock.Verify(x => x.PostSessionHeaderAsync(expectedHeader), Times.Once());
        }

        [TestMethod]
        public async Task TestReportEvent()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Act
            journey.ReportEvent(CLICK_PLAY);
            journey.ReportEvent(CLICK_PAUSE);
            journey.ReportEvent(CLICK_PLAY);

            // Verify
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
            };
            expected.EventCounts[CLICK_PLAY] = 2;
            expected.EventCounts[CLICK_PAUSE] = 1;
            expected.EventSequence.AddRange(new List<string>() { CLICK_PLAY, CLICK_PAUSE, CLICK_PLAY });
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Exactly(4));
        }

        [TestMethod]
        public async Task TestReportCollapsibleEvent()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Act
            journey.ReportEvent(NAVIGATE, isCollapsible: true);
            journey.ReportEvent(NAVIGATE, isCollapsible: true);
            journey.ReportEvent(NAVIGATE, isCollapsible: true);

            // Verify
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
            };
            expected.EventCounts[NAVIGATE] = 3;
            expected.EventSequence.AddRange(new List<string>() { $"({NAVIGATE})" });
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Exactly(4));
        }

        [TestMethod]
        public async Task TestReportError()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Act
            journey.ReportEvent(ERROR);

            // Verify
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
            };
            expected.EventCounts[ERROR] = 1;
            expected.EventSequence.AddRange(new List<string>() { ERROR });
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TestReportCrash()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Act
            journey.ReportCrash(CRASH);

            // Verify
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
                HasError = true,
                HasCrash = true,
            };
            expected.EventCounts[CRASH] = 1;
            expected.EventSequence.AddRange(new List<string>() { CRASH });
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TestReportingEventUpdatesEndTime()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Act
            timelineMock.Setup(x => x.GetUtcNow()).Returns(LATER);
            journey.ReportEvent(CLICK_PLAY);

            // Verify
            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
                End = LATER,
            };
            expected.EventCounts[CLICK_PLAY] = 1;
            expected.EventSequence.AddRange(new List<string>() { CLICK_PLAY });
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TestReportStageTransition()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Act
            journey.ReportStageTransition(2, "new_stage");

            // Verify
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
                NewStage = new Stage(2, "new_stage", timelineMock.Object),
            };
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TestReportStageTransitionIgnoredWhenNewStageIsLower()
        {
            // Setup
            var restApiMock = new Mock<IRestApi>();
            var persistenceMock = new Mock<IPersistence>();
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();
            var loggerMock = new Mock<ILogger>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(NOW);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(SESSION_ID);

            Journey journey = new Journey(
                restApiMock.Object, persistenceMock.Object, timelineMock.Object, idGeneratorMock.Object, loggerMock.Object);
            await journey.InitializeAsync(ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD);

            // Act
            journey.ReportStageTransition(3, "stage3");
            journey.ReportStageTransition(2, "stage2");

            // Verify
            var expected = new Session(
                SESSION_ID, ACCOUNT_ID, APP_ID, VERSION, RELEASE_BUILD, NOW, timelineMock.Object)
            {
                FirstLaunch = true,
                NewStage = new Stage(3, "stage3", timelineMock.Object),
            };
            persistenceMock.Verify(x => x.SaveSession(expected), Times.Exactly(3));
        }
    }
}