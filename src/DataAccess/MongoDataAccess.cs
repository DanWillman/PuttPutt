﻿using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using PuttPutt.Models;

namespace PuttPutt.DataAccess
{
    /// <inheritdoc/>
    public class MongoDataAccess : IMongoDataAccess
    {
        private readonly string ParticipantCollectionName = "scores";
        private readonly string ArchiveCollectionName = "archive";
        private readonly string ConnectionString = "mongodb://192.168.1.69:27018";
        private readonly string DatabaseName = "shame_golf";

        private readonly IMongoCollection<Participant> particpantCollection;
        private readonly IMongoCollection<Archive> archiveCollection;

        /// <summary>
        /// IoC constructor
        /// </summary>
        public MongoDataAccess()
        {
            var client = new MongoClient(ConnectionString);
            var database = client.GetDatabase(DatabaseName);

            particpantCollection = database.GetCollection<Participant>(ParticipantCollectionName);
            particpantCollection.Indexes.CreateOne(new CreateIndexModel<Participant>(Builders<Participant>.IndexKeys.Ascending(x => x.UserId)));

            archiveCollection = database.GetCollection<Archive>(ArchiveCollectionName);
            archiveCollection.Indexes.CreateOne(new CreateIndexModel<Archive>(Builders<Archive>.IndexKeys.Ascending(x => x.Id)));
        }

        public List<Participant> GetParticipants(DiscordGuild server) => particpantCollection.Find(x => x.ServerId == server.Id).ToList();

        public Participant GetParticipantInfo(DiscordUser user, DiscordGuild server) =>
            particpantCollection.Find(x => x.UserId == user.Id && x.ServerId == server.Id).FirstOrDefault();

        public Participant GetParticipantInfo(ulong userId, ulong serverId) =>
            particpantCollection.Find(x => x.UserId == userId && x.ServerId == serverId).FirstOrDefault();

        public Participant UpsertParticipant(Participant participant)
        {
            if(string.IsNullOrWhiteSpace(participant.Id))
            {
                participant.Id = ObjectId.GenerateNewId().ToString();
            }

            particpantCollection.ReplaceOne(x => x.UserId == participant.UserId && x.ServerId == participant.ServerId, participant, new ReplaceOptions() { IsUpsert = true });

            return GetParticipantInfo(participant);
        }

        public List<string> GetArchivalNames(DiscordGuild server) => archiveCollection.Find(x => x.ServerId == server.Id).ToList().Select(a => a.ArchiveName).Distinct().ToList();

        public Archive GetArchive(DiscordGuild server, string archiveName) => archiveCollection.Find(x => x.ServerId == server.Id && x.ArchiveName.Equals(archiveName)).FirstOrDefault();

        public void ArchiveSeason(DiscordGuild server, string archiveName)
        {
            var archive = new Archive()
            {
                ArchiveName = archiveName,
                Id = ObjectId.GenerateNewId().ToString(),
                ServerId = server.Id,
                Participant = GetParticipants(server)
            };

            Console.WriteLine($"Archiving season. {JsonConvert.SerializeObject(archive)}");

            archiveCollection.InsertOne(archive);

            particpantCollection.DeleteMany(x => x.ServerId == server.Id);
        }

        /// <summary>
        /// Helper method fetches participant by object
        /// </summary>
        private Participant GetParticipantInfo(Participant participant) =>
            particpantCollection.Find(x => x.UserId == participant.UserId && x.ServerId == participant.ServerId).FirstOrDefault();
    }
}
