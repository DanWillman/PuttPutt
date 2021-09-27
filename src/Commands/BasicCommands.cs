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
using PuttPutt.Services.BasicCommandService;
using PuttPutt.Utilities;

namespace PuttPutt.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        private MongoDataAccess mongo = new MongoDataAccess();
        private readonly IBasicCommandService commandService;

        public BasicCommands(IBasicCommandService commandService)
        {
            this.commandService = commandService;
        }

        [Command("scoreboard")]
        [Description("Displays the current scoreboard. Optionally can limit results. Example: `!scoreboard` or `!scoreboard 5`")]
        public async Task ReportScoreboard(CommandContext ctx, int limit = -1)
        {
            var golfer = DiscordEmoji.FromName(ctx.Client, ":golfer:");
            var messageStrings = limit >= 0 ? commandService.ReportScoreboard(ctx.Guild.Id, limit, golfer)
                                            : commandService.ReportScoreboard(ctx.Guild.Id, golfer);

            foreach (string message in messageStrings)
            {
                await ctx.RespondAsync(message);
            }
        }

        [Command("archives")]
        [Description("Gets a list of all archive names, allowing the user to then pull up a scoreboard for that season")]
        public async Task GetArchives(CommandContext ctx)
        {
            await ctx.RespondAsync(commandService.GetArchives(ctx.Guild.Id));
        }

        [Command("seasonscores")]
        [Description(@"Displays a scoreboard for a previous season. Example: `!seasonscores ""Summer2021""`")]
        public async Task ReportOldScoreboard(CommandContext ctx, [RemainingText] string archive)
        {
            var golferEmoji = DiscordEmoji.FromName(ctx.Client, ":golfer:");
            var messageStrings = commandService.ReportArchiveScoreboard(ctx.Guild.Id, archive, golferEmoji);

            foreach (string message in messageStrings)
            {
                await ctx.RespondAsync(message);
            }
        }

        [Command("myscore")]
        [Description("Reports calling user's current score")]
        public async Task ReportScore(CommandContext ctx)
        {
            var result = mongo.GetParticipantInfo(ctx.User.Id, ctx.Guild.Id);

            await ctx.RespondAsync($"Looks like you're sitting at {result.Score}, {ctx.User.Mention}");
        }

        [Command("history")]
        [Description("Reports calling user's score history. Optionally can limit results. Example: `!history` or `!history 5`")]
        public async Task ReportHistory(CommandContext ctx, int limit = -1)
        {
            var messageStrings = limit >= 0 ? commandService.ReportHistory(ctx.Guild.Id, ctx.User.Id, limit)
                                            : commandService.ReportHistory(ctx.Guild.Id, ctx.User.Id);

            foreach (string message in messageStrings)
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
            string response = string.Empty;
            string displayName = string.Empty;
            int score = 0;

            (response, score) = string.IsNullOrWhiteSpace(reason) ? commandService.UpdateUserScore(ctx.Guild.Id, ctx.User.Id, modifier, ctx.Member.DisplayName)
                                : commandService.UpdateUserScore(ctx.Guild.Id, ctx.User.Id, modifier, displayName, reason);
            
            try
            {
                if (!ctx.Member.IsOwner) //Nobody can change an owner's nickname
                {
                    var newName = await UpdateUserName(ctx.Member, score);

                    if (!ctx.Member.DisplayName.Equals(newName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        response += $"{Environment.NewLine}I updated your name as well";
                    }
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
                if (!ctx.Member.IsOwner)
                {
                    newDisplayName = await UpdateUserName(ctx.Member, score);
                    if (!ctx.Member.DisplayName.Equals(newDisplayName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        response += $"{Environment.NewLine}I updated your name as well";
                    }
                }                
            }
            catch (Exception ex)
            {
                response += $"{Environment.NewLine}I tried to update your display name, but something went wrong: {ex.Message}";
            }

            newDisplayName = string.IsNullOrWhiteSpace(newDisplayName) ? ctx.Member.DisplayName : newDisplayName;

            response += string.IsNullOrWhiteSpace(reason) ? commandService.SetUserScore(ctx.Guild.Id, ctx.User.Id, score, newDisplayName)
                                    : commandService.SetUserScore(ctx.Guild.Id, ctx.User.Id, score, newDisplayName, reason);
                        

            await ctx.RespondAsync(response);
        }

        private async Task<string> UpdateUserName(DiscordMember member, int score)
        {
            string displayName = member.DisplayName;
            var newDisplayName = UsernameUtilities.UpdateUsernameScore(member.DisplayName, score);

            if (!newDisplayName.Equals(member.DisplayName))
            {
                await member.ModifyAsync(x =>
                {
                    x.Nickname = newDisplayName;
                    x.AuditLogReason = $"Changed by PuttPutt, new score";
                });

                displayName = newDisplayName;
            }

            return displayName;
        }
    }
}