using DSharpPlus.Entities;
using PuttPutt.DataAccess;
using PuttPutt.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuttPutt.Services.BasicCommandService
{
    /// <inheritdoc/>
    public class BasicCommandService : IBasicCommandService
    {
        private readonly IMongoDataAccess mongo;

        public BasicCommandService(IMongoDataAccess mongo)
        {
            this.mongo = mongo;
        }

        public string GetArchives(ulong serverId)
        {
            return string.Join(", ", mongo.GetArchivalNames(serverId));
        }

        public Task<string> ReportArchiveScoreboard(ulong serverId, string archiveName)
        {
            throw new NotImplementedException();
        }

        public List<string> ReportHistory(ulong serverId, ulong userId)
        {
            return ReportHistory(serverId, userId, -1);
        }

        public List<string> ReportHistory(ulong serverId, ulong userId, int limit)
        {
            var response = new List<string>();
            var events = mongo.GetParticipantInfo(userId, serverId).EventHistory.OrderBy(e => e.EventTimeUTC).ToList();

            if (limit != -1 && events.Count > limit)
            {
                events = events.GetRange(0, limit);
            }

            foreach (string message in MessageFormatter.FormatHistoryToDiscordMessage(events))
            {
                MessageFormatter.AddOrExtendDiscordStrings(response, message);
            }

            return response;
        }

        public List<string> ReportScoreboard(ulong serverId, DiscordEmoji headerEmoji)
        {
            return ReportScoreboard(serverId, -1, headerEmoji);
        }

        public List<string> ReportScoreboard(ulong serverId, int limit, DiscordEmoji headerEmoji)
        {
            var response = new List<string>();
            var results = mongo.GetParticipants(serverId).OrderBy(p => p.Score).ToList();

            if (limit != -1 && results.Count > limit)
            {
                results = results.GetRange(0, limit);
            }

            foreach (string message in MessageFormatter.FormatGolfersToDiscordMessage(results, headerEmoji))
            {
                MessageFormatter.AddOrExtendDiscordStrings(response, message);
            }

            return response;
        }

        public Task<string> SetUserScore(ulong serverId, ulong userId, int score)
        {
            throw new NotImplementedException();
        }

        public Task<string> SetUserScore(ulong serverId, ulong userId, int score, string note)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier, string note)
        {
            throw new NotImplementedException();
        }
    }
}
