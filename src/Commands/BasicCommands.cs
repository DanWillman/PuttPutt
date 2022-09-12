using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using MongoDB.Bson;
using PuttPutt.DataAccess;
using PuttPutt.Models;
using PuttPutt.Utilities;

namespace PuttPutt.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        private MongoDataAccess mongo = new MongoDataAccess();

        [Command("scoreboard")]
        [Description("Displays the current scoreboard. Optionally can limit results. Example: `!scoreboard` or `!scoreboard 5`")]
        public async Task ReportScoreboard(CommandContext ctx, int limit = -1)
        {
            var results = mongo.GetParticipants(ctx.Guild).OrderBy(p => p.Score).ToList();

            if (limit != -1 && results.Count > limit)
            {
                results = results.GetRange(0, limit);
            }

            var golferEmoji = DiscordEmoji.FromName(ctx.Client, ":golfer:");

            foreach (string message in MessageFormatter.FormatGolfersToDiscordMessage(results, golferEmoji))
            {
                await ctx.RespondAsync(message);
            }
        }

        [Command("archives")]
        [Description("Gets a list of all archive names, allowing the user to then pull up a scoreboard for that season")]
        public async Task GetArchives(CommandContext ctx)
        {
            var results = mongo.GetArchivalNames(ctx.Guild);

            await ctx.RespondAsync(string.Join(",", results));
        }

        [Command("seasonscores")]
        [Description(@"Displays a scoreboard for a previous season. Example: `!seasonscores ""Summer2021""`")]
        public async Task ReportOldScoreboard(CommandContext ctx, [RemainingText] string archive)
        {
            var results = mongo.GetArchive(ctx.Guild, archive).Participant;
            var golferEmoji = DiscordEmoji.FromName(ctx.Client, ":golfer:");

            foreach (string message in MessageFormatter.FormatGolfersToDiscordMessage(results, golferEmoji, $"{archive} results!"))
            {
                await ctx.RespondAsync(message);
            }
        }

        [Command("myscore")]
        [Description("Reports calling user's current score")]
        public async Task ReportScore(CommandContext ctx)
        {
            var result = mongo.GetParticipantInfo(ctx.User, ctx.Guild);

            await ctx.RespondAsync($"Looks like you're sitting at {result.Score}, {ctx.User.Mention}");
        }

        [Command("history")]
        [Description("Reports calling user's score history. Optionally can limit results. Example: `!history` or `!history 5`")]
        public async Task ReportHistory(CommandContext ctx, int limit = -1)
        {
            var events = mongo.GetParticipantInfo(ctx.User, ctx.Guild).EventHistory.OrderByDescending(e => e.EventTimeUTC).ToList();

            if (limit != -1 && events.Count > limit)
            {
                events = events.GetRange(0, limit);
            }

            foreach (string message in MessageFormatter.FormatHistoryToDiscordMessage(events))
            {
                await ctx.RespondAsync(message);
            }
        }

        [Command("fore")]
        [Description("Updates a users score. Example use: `!fore -5` or `!fore 5`")]
        public async Task UpdateUserScore(CommandContext ctx,
            [Description("Amount to modify current score")] int modifier,
            [RemainingText, Description("Optional reason for what you did, displayed in history")] string reason = "")
        {
            var data = mongo.GetParticipantInfo(ctx.User, ctx.Guild);
            var response = "";

            if (data == null)
            {
                string displayName = string.Empty;
                int score = 0;
                try
                {
                    displayName = UsernameUtilities.SanitizeUsername(ctx.Member.DisplayName);
                    score = UsernameUtilities.GetScore(ctx.Member.DisplayName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to parse score from name: {ctx.Member.DisplayName} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.StackTrace}");
                }

                data = new Participant()
                {
                    ServerId = ctx.Guild.Id,
                    UserId = ctx.Member.Id,
                    DisplayName = string.IsNullOrWhiteSpace(displayName) ? ctx.Member.DisplayName : displayName,
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
                Notes = reason
            });

            int originalScore = data.Score;
            data.Score += modifier;

            var updatedData = mongo.UpsertParticipant(data);

            response = string.IsNullOrWhiteSpace(response) ? $"Ok, I've updated your score from {originalScore} to {updatedData.Score}" : response;

            try
            {
                var newName = await UsernameUtilities.UpdateUserName(ctx.Member, data.Score);

                if (!ctx.Member.DisplayName.Equals(newName, StringComparison.InvariantCultureIgnoreCase))
                {
                    response += $"{Environment.NewLine}I updated your name as well";
                }
            }
            catch (Exception ex)
            {
                response += $"{Environment.NewLine}I tried to update your display name, but something went wrong: {ex.Message}";
            }


            await ctx.RespondAsync(response);
        }

        [Command("setscore")]
        [Description("Sets a users score. Example use: `!setscore -5` or `!setscore 5`")]
        public async Task SetUserScore(CommandContext ctx,
            [Description("New score")] int score,
            [RemainingText, Description("Optional reason for why you're setting this, displayed in history")] string reason = "")
        {
            string response = $"Ok, I've set your score to {score}";
            string newDisplayName = string.Empty;
            try
            {
                newDisplayName = await UsernameUtilities.UpdateUserName(ctx.Member, score);
                if (!ctx.Member.DisplayName.Equals(newDisplayName, StringComparison.InvariantCultureIgnoreCase))
                {
                    response += $"{Environment.NewLine}I updated your name as well";
                }
            }
            catch (Exception ex)
            {
                response += $"{Environment.NewLine}I tried to update your display name, but something went wrong: {ex.Message}";
            }

            try
            {
                var priorData = mongo.GetParticipantInfo(ctx.User, ctx.Guild);
                var data = new Participant()
                {
                    Id = string.IsNullOrWhiteSpace(priorData?.Id) ? string.Empty : priorData.Id,
                    ServerId = ctx.Guild.Id,
                    UserId = ctx.Member.Id,
                    DisplayName = string.IsNullOrWhiteSpace(newDisplayName) ? ctx.Member.DisplayName : newDisplayName,
                    Score = score,
                    EventHistory = (priorData?.EventHistory == null) ? new() : priorData.EventHistory
                };

                data.EventHistory.Add(new()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    EventTimeUTC = DateTime.UtcNow,
                    ScoreModifier = 0,
                    ScoreSnapshot = score,
                    Notes = reason,
                    PriorScore = (priorData == null) ? 0 : priorData.Score
                });

                mongo.UpsertParticipant(data);
            }
            catch (Exception ex)
            {
                response = $"Oops, something went wrong: {ex.Message}";
            }

            await ctx.RespondAsync(response);
        }
    }
}