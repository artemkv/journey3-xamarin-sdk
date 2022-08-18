using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemkv.Journey3.Connector.Test
{
    [TestClass]
    public class DomainTest
    {
        private static readonly DateTime DATE_1 = new DateTime(2020, 1, 1);
        private static readonly DateTime DATE_2 = new DateTime(2020, 2, 1);

        private static readonly string ID_1 = "id001";
        private static readonly string ID_2 = "id002";

        [TestMethod]
        public void TestStageToJsonRoundtrip()
        {
            var timelineMock = new Mock<ITimeline>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(DATE_1);
            var stage = new Stage(3, "stage3", timelineMock.Object);

            var json = JsonConvert.SerializeObject(stage);

            timelineMock.Setup(x => x.GetUtcNow()).Returns(DATE_2);
            var deserialized = Stage.FromJson(json, timelineMock.Object);

            Assert.AreEqual(stage, deserialized);
        }

        [TestMethod]
        public void TestStageFromEmptyJson()
        {
            var timelineMock = new Mock<ITimeline>();
            timelineMock.Setup(x => x.GetUtcNow()).Returns(DATE_1);

            var expected = new Stage(1, "new_user", timelineMock.Object);
            var deserialized = Stage.FromJson("{}", timelineMock.Object);

            Assert.AreEqual(expected, deserialized);
        }

        [TestMethod]
        public void TestSessionToJsonRoundtrip()
        {
            var timelineMock = new Mock<ITimeline>();
            var idGeneratorMock = new Mock<IIdGenerator>();

            timelineMock.Setup(x => x.GetUtcNow()).Returns(DATE_1);
            var session = new Session(ID_1, "accid", "appid", "1.0", false, DATE_1, timelineMock.Object)
            {
                Since = DATE_1,
                Start = DATE_1,
                End = DATE_1,
            };

            var json = JsonConvert.SerializeObject(session);

            timelineMock.Setup(x => x.GetUtcNow()).Returns(DATE_2);
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(ID_2);
            var deserialized = Session.FromJson(json, timelineMock.Object, idGeneratorMock.Object);

            Assert.AreEqual(session, deserialized);
        }

        [TestMethod]
        public void TestSessionFromEmptyJson()
        {
            var timelineMock = new Mock<ITimeline>();
            timelineMock.Setup(x => x.GetUtcNow()).Returns(DATE_1);

            var idGeneratorMock = new Mock<IIdGenerator>();
            idGeneratorMock.Setup(x => x.GetNewId()).Returns(ID_1);

            var expected = new Session(ID_1, "", "", "", false, DATE_1, timelineMock.Object)
            {
                Since = DATE_1,
                Start = DATE_1,
                End = DATE_1,
            };
            var deserialized = Session.FromJson("{}", timelineMock.Object, idGeneratorMock.Object);

            Assert.AreEqual(expected, deserialized);
        }

    }
}
