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

        [TestCase(-1)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(14)]
        public void ReportHistory_Limit_ReturnsLimitedHistory(int limit)
        {
            var events = RandomEvents(25);
            var participant = new Participant() { EventHistory = events };
            var eventsSortedLimited = participant.EventHistory.OrderBy(e => e.EventTimeUTC).ToList().GetRange(0, limit > -1 ? limit 
                                                                                                    : participant.EventHistory.Count);
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

        [Test]
        public void ReportArchiveScoreboard_HappyPath_ReturnsExpectedStrings()
        {
            var mock = new AutoMocker();
            var participants = A.ListOf<Participant>();
            var archive = A.New<Archive>();

            archive.Participant = participants;

            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetArchive(It.IsAny<ulong>(), It.IsAny<string>())).Returns(archive);

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.ReportArchiveScoreboard(RandomULong(1)[0], "test", null);

            foreach (var p in participants)
            {
                Assert.IsTrue(actual.Exists(a => a.Contains(p.DisplayName)));
            }
        }

        [Test]
        public void SetUserScore_HappyPath_ReturnsHappyString()
        {
            var mock = new AutoMocker();
            var participant = A.New<Participant>();

            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetParticipantInfo(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(participant);

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.SetUserScore(1, 2, 3, "test");

            Assert.AreEqual(string.Empty, actual);
        }

        [Test]
        public void SetUserScore_EncounteredException_ReturnsExceptionMessage()
        {
            var mock = new AutoMocker();
            var expected = "Test Exception";

            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetParticipantInfo(It.IsAny<ulong>(), It.IsAny<ulong>())).Throws(new Exception(expected));

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.SetUserScore(1, 2, 3, "test");

            Assert.IsTrue(actual.Contains(expected));
        }

        [TestCase(3)]
        [TestCase(-3)]
        public void UpdateUserScore_HappyPath_ReturnsExpectedStringAndScore(int modifier)
        {
            var mock = new AutoMocker();
            var participant = A.New<Participant>();
            var originalScore = participant.Score;
            var updatedParticipant = new Participant
            {
                Score = originalScore + modifier
            };

            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetParticipantInfo(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(participant);
            mock.GetMock<IMongoDataAccess>().Setup(x => x.UpsertParticipant(It.IsAny<Participant>())).Returns((updatedParticipant, null));

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.UpdateUserScore(1, 2, modifier, "Potato");

            string expected = $"Ok, I've updated your score from {originalScore} to {actual.Score}";
            Assert.AreEqual(expected, actual.Response);
            Assert.AreEqual(updatedParticipant.Score, actual.Score);
        }

        [TestCase("Dan [-52]", -52, 5)]
        [TestCase("Dan [52]", 52, 5)]
        [TestCase("Dan (He/Him) [-52]", -52, 7)]
        [TestCase("Dan {-52}", -52, 3)]
        [TestCase("Dan {52}", 52, -5)]
        [TestCase("Dan (-52)", -52, -9)]
        [TestCase("Dan", 0, -2)]
        public void UpdateUserScore_NotInDatabase_CreatesRecord(string name, int score, int modifier)
        {
            var mock = new AutoMocker();
            var updatedParticipant = new Participant
            {
                Score = score + modifier
            };

            mock.GetMock<IMongoDataAccess>().Setup(x => x.UpsertParticipant(It.IsAny<Participant>())).Returns((updatedParticipant, null));

            var service = mock.CreateInstance<BasicCommandService>();

            var actual = service.UpdateUserScore(1, 2, modifier, name);

            string expectedStartString = $" I couldn't find you in my records, so I started you at {score} and you're now at {actual.Score}";

            Assert.IsTrue(actual.Response.Contains(expectedStartString));
            Assert.AreEqual(updatedParticipant.Score, actual.Score);
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

        private static List<Event> RandomEvents(int limit)
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
