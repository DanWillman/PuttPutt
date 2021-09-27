using DSharpPlus.Entities;
using MongoDB.Bson;
using PuttPutt.DataAccess;
using PuttPutt.Models;
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

        public List<string> ReportArchiveScoreboard(ulong serverId, string archiveName, DiscordEmoji headerEmoji)
        {
            var results = mongo.GetArchive(serverId, archiveName).Participant;

            return FormatScoreboardMessage(results, -1, headerEmoji);
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
            var results = mongo.GetParticipants(serverId).OrderBy(p => p.Score).ToList();

            return FormatScoreboardMessage(results, limit, headerEmoji);
        }

        private List<string> FormatScoreboardMessage(List<Participant> scores, int limit, DiscordEmoji headerEmoji)
        {
            var response = new List<string>();

            if (limit != -1 && scores.Count > limit)
            {
                scores = scores.GetRange(0, limit);
            }

            foreach (string message in MessageFormatter.FormatGolfersToDiscordMessage(scores, headerEmoji))
            {
                MessageFormatter.AddOrExtendDiscordStrings(response, message);
            }

            return response;
        }

        public string SetUserScore(ulong serverId, ulong userId, int score, string displayName)
        {
            return SetUserScore(serverId, userId, score, displayName, string.Empty);
        }

        public string SetUserScore(ulong serverId, ulong userId, int score, string displayName, string note)
        {
            string response = string.Empty;

            try
            {
                var priorData = mongo.GetParticipantInfo(userId, serverId);
                var data = new Participant()
                {
                    Id = string.IsNullOrWhiteSpace(priorData?.Id) ? string.Empty : priorData.Id,
                    ServerId = serverId,
                    UserId = userId,
                    DisplayName = displayName,
                    Score = score,
                    EventHistory = (priorData?.EventHistory == null) ? new() : priorData.EventHistory
                };

                data.EventHistory.Add(new()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    EventTimeUTC = DateTime.UtcNow,
                    ScoreModifier = 0,
                    ScoreSnapshot = score,
                    Notes = note,
                    PriorScore = (priorData == null) ? 0 : priorData.Score
                });

                mongo.UpsertParticipant(data);
            }
            catch (Exception ex)
            {
                response = $"Oops, something went wrong: {ex.Message}";
            }

            return response;
        }

        public (string Response, int Score) UpdateUserScore(ulong serverId, ulong userId, int modifier, string displayName)
        {
            return UpdateUserScore(serverId, userId, modifier, displayName, string.Empty);
        }

        public (string Response, int Score) UpdateUserScore(ulong serverId, ulong userId, int modifier, string displayName, string note)
        {
            string response = string.Empty;
            string newName = string.Empty;
            var data = mongo.GetParticipantInfo(userId, serverId);
            int score = 0;

            try
            {
                newName = UsernameUtilities.SanitizeUsername(displayName);
                score = UsernameUtilities.GetScore(displayName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to parse score from name: {displayName} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.StackTrace}");
            }

            if (data == null)
            {
                data = new Participant()
                {
                    ServerId = serverId,
                    UserId = userId,
                    DisplayName = newName,
                    Score = score,
                    EventHistory = new List<Event>()
                };

                response += $" I couldn't find you in my records, so I started you at {data.Score} and you're now at {data.Score + modifier}";
            }
            else if (data.EventHistory == null)
            {
                data.EventHistory = new List<Event>();
            }

            data.EventHistory.Add(new Event()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                EventTimeUTC = DateTime.UtcNow,
                ScoreModifier = modifier,
                PriorScore = data.Score,
                ScoreSnapshot = data.Score + modifier,
                Notes = note
            });

            int originalScore = data.Score;
            data.Score += modifier;

            (Participant updatedData, var result) = mongo.UpsertParticipant(data);

            return (string.IsNullOrWhiteSpace(response) ? $"Ok, I've updated your score from {originalScore} to {updatedData.Score}" : response,
                    data.Score);
        }
    }
}
