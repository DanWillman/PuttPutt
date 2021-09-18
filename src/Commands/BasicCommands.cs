using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
        public async Task ReportOldScoreboard(CommandContext ctx, [RemainingText]string archive)
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
            var events = mongo.GetParticipantInfo(ctx.User, ctx.Guild).EventHistory.OrderBy(e => e.EventTimeUTC).ToList();

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
            [Description("Amount to modify current score")]int modifier)
        {
            var data = mongo.GetParticipantInfo(ctx.User, ctx.Guild);
            var response = "";

            if (data == null)
            {
                data = new Participant()
                {
                    ServerId = ctx.Guild.Id,
                    UserId = ctx.Member.Id,
                    DisplayName = UsernameUtilities.SanitizeUsername(ctx.Member.DisplayName),
                    Score = UsernameUtilities.GetScore(ctx.Member.DisplayName),
                    EventHistory = new List<Event>()
                };

                response = $"I couldn't find you in my records, so I started you at {data.Score} and you're now at {data.Score + modifier}";
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
                ScoreSnapshot = data.Score + modifier
            });

            int originalScore = data.Score;
            data.Score += modifier;

            var updatedData = mongo.UpsertParticipant(data);

            response = string.IsNullOrWhiteSpace(response) ? $"Ok, I've updated your score from {originalScore} to {updatedData.Score}" : response;

            await ctx.RespondAsync(response);
        }
    }
}