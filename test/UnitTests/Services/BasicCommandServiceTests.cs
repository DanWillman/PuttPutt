using GenFu;
using MongoDB.Driver;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using PuttPutt.DataAccess;
using PuttPutt.Models;
using PuttPutt.Services.BasicCommandService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UnitTests.Services
{
    [TestFixture]
    public class BasicCommandServiceTests
    {
        private Random random;
        [SetUp]
        public void Setup()
        {
            random = new Random();
            GenFu.GenFu.Configure<Participant>()
                .Fill(p => p.ServerId)
                .WithRandom(RandomULong())
                .Fill(p => p.UserId)
                .WithRandom(RandomULong());

            GenFu.GenFu.Configure<Member>()
                .Fill(m => m.Id)
                .WithRandom(RandomULong());
        }

        [Test]
        public void ReportScoreboard_HappyPath_ReturnsExpectedString()
        {
            var mock = new AutoMocker();
            var participants = A.ListOf<Participant>();

            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetParticipants(It.IsAny<ulong>())).Returns(participants);

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.ReportScoreboard(RandomULong(1)[0], null);

            foreach(var p in participants)
            {
                Assert.IsTrue(actual.Exists(a => a.Contains(p.DisplayName)));
            }
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(13)]
        [TestCase(20)]
        public void ReportScoreboard_Limit_ReturnsExpectedCount(int limit)
        {
            var mock = new AutoMocker();
            var participants = A.ListOf<Participant>();

            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetParticipants(It.IsAny<ulong>())).Returns(participants);

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.ReportScoreboard(RandomULong(1)[0], limit, null);
            int actualCount = Regex.Matches(actual[0], $"[{Environment.NewLine}]+").Count;

            Assert.AreEqual(limit + 4, actualCount);
        }

        [Test]
        public void GetArchives_HappyPath_ReturnsExpectedString()
        {
            var input = new List<string> { "a", "b", "c" };
            var mock = new AutoMocker();

            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetArchivalNames(It.IsAny<ulong>())).Returns(input);

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.GetArchives(1);

            Assert.AreEqual("a, b, c", actual);
        }

        [Test]
        public void ReportHistory_HappyPath_ReturnsUserHistory()
        {
            var events = RandomEvents(25);
            var participant = new Participant() { EventHistory = events };
            var mock = new AutoMocker();

            mock.GetMock<IMongoDataAccess>().Setup(m => m.GetParticipantInfo(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(participant);

            var service = mock.CreateInstance<BasicCommandService>();
            var actual = service.ReportHistory(1, 2);

            foreach (var e in events)
            {
                Assert.IsTrue(actual.Exists(a => a.Contains(e.EventTimeUTC.ToShortDateString()) 
                                                        && a.Contains(e.EventTimeUTC.ToShortTimeString())));
            }
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(14)]
        public void ReportHistory_Limit_ReturnsLimitedHistory(int limit)
        {
            var events = RandomEvents(25);
            var participant = new Participant() { EventHistory = events };
            var eventsSortedLimited = participant.EventHistory.OrderBy(e => e.EventTimeUTC).ToList().GetRange(0, limit);
            var mock = new AutoMocker();

            mock.GetMock<IMongoDataAccess>().Setup(m => m.GetParticipantInfo(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(participant);

            var service = mock.CreateInstance<BasicCommandService>();
            var actual = service.ReportHistory(1, 2);

            foreach (var e in eventsSortedLimited)
            {
                Assert.IsTrue(actual.Exists(a => a.Contains(e.EventTimeUTC.ToShortDateString())
                                                        && a.Contains(e.EventTimeUTC.ToShortTimeString())));
            }
        }

        private List<ulong> RandomULong(int count = 1)
        {
            List<ulong> result = new();

            for(int i = 0;i < count; i++)
            {
                result.Add((ulong)(random.NextDouble() * ulong.MaxValue));
            }
            return result;
        }

        private List<string> RandomDisplayNames()
        {
            return new List<string>{
                "Dan [-52]",
                "Dan [52]",
                "Dan (He/Him) [-52]",
                "Dan {-52}",
                "Dan {52}",
                "Dan (-52)",
                "Dan"
            };
        }

        private List<Event> RandomEvents(int limit)
        {
            List<Event> results = new List<Event>();
            Random rand = new Random();
            DateTime start = DateTime.UtcNow;

            for (int i = 0; i < limit; i++)
            {
                results.Add(new Event
                {
                    Id = Guid.NewGuid().ToString(),
                    Notes = Guid.NewGuid().ToString(),
                    EventTimeUTC = start.AddHours(rand.Next(-900, 900))
                });
            }

            return results;
        }
    }
}
