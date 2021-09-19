﻿using GenFu;
using MongoDB.Driver;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using PuttPutt.DataAccess;
using PuttPutt.Models;
using PuttPutt.Services.AdminCommandService;
using System;
using System.Collections.Generic;

namespace UnitTests.Services
{
    [TestFixture]
    public class AdminCommandServiceTests
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
        public void StartSeason_HappyPath_UpdatesAllMembers()
        {
            var mock = new AutoMocker();
            var participant = A.New<Participant>();
            var members = A.ListOf<Member>();

            mock.GetMock<IMongoDataAccess>().Setup(x => x.UpsertParticipant(It.IsAny<Participant>())).Returns((participant, null));

            var adminService = mock.CreateInstance<AdminCommandService>();

            var actual = adminService.StartSeason(members, participant.ServerId);

            Assert.AreEqual(members.Count, actual);
        }

        [Test]
        public void StartSeason_ExceptionEncounter_NoExceptionThrown_UpdatesWhatItCan()
        {
            var mock = new AutoMocker();
            var participant = A.New<Participant>();
            var members = A.ListOf<Member>(2);

            mock.GetMock<IMongoDataAccess>()
                .SetupSequence(x => x.UpsertParticipant(It.IsAny<Participant>()))
                .Returns((participant, null))
                .Throws(new Exception("Potato"));

            var adminService = mock.CreateInstance<AdminCommandService>();

            var actual = adminService.StartSeason(members, participant.ServerId);

            Assert.AreEqual(1, actual);
        }

        [Test]
        public void SyncScores_HappyPath_UpdatesAllMembers()
        {
            GenFu.GenFu.Configure<Member>()
                .Fill(m => m.Id)
                .WithRandom(RandomULong())
                .Fill(m => m.DisplayName)
                .WithRandom(RandomDisplayNames());
            var mock = new AutoMocker();
            var participant = A.New<Participant>();
            var members = A.ListOf<Member>();

            mock.GetMock<IMongoDataAccess>().Setup(x => x.UpsertParticipant(It.IsAny<Participant>())).Returns((participant, null));
            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetParticipantInfo(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(participant);

            var adminService = mock.CreateInstance<AdminCommandService>();

            var actual = adminService.SyncScores(members, participant.ServerId);

            Assert.AreEqual(members.Count, actual);
        }

        [Test]
        public void SyncScores_ExceptionEncounter_NoExceptionThrown_UpdatesWhatItCan()
        {
            GenFu.GenFu.Configure<Member>()
                .Fill(m => m.Id)
                .WithRandom(RandomULong())
                .Fill(m => m.DisplayName)
                .WithRandom(RandomDisplayNames());
            var mock = new AutoMocker();
            var participant = A.New<Participant>();
            var members = A.ListOf<Member>(2);

            mock.GetMock<IMongoDataAccess>()
                .SetupSequence(x => x.UpsertParticipant(It.IsAny<Participant>()))
                .Returns((participant, null))
                .Throws(new Exception("Potato"));
            mock.GetMock<IMongoDataAccess>().Setup(x => x.GetParticipantInfo(It.IsAny<ulong>(), It.IsAny<ulong>())).Returns(participant);

            var adminService = mock.CreateInstance<AdminCommandService>();

            var actual = adminService.SyncScores(members, participant.ServerId);

            Assert.AreEqual(1, actual);
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
