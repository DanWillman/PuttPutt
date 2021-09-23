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

        public async Task ReportScoreboard_HappyPath_ReturnsExpectedString()
        {
            var mock = new AutoMocker();
            var participant = A.New<Participant>();
            var members = A.ListOf<Member>();

            mock.GetMock<IMongoDataAccess>().Setup(x => x.UpsertParticipant(It.IsAny<Participant>())).Returns((participant, null));

            var adminService = mock.CreateInstance<BasicCommandService>();

            var actual = adminService.ReportScoreboard(RandomULong(1)[0], null);

            foreach(var member in members)
            {
                Assert.IsTrue(actual.Exists(a => a.Contains(member.DisplayName)));
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
    }
}
