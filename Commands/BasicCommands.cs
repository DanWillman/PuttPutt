using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PuttPutt.DataAccess;
using PuttPutt.Models;

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

            StringBuilder sb = new StringBuilder($"Current Scores!{Environment.NewLine}");

            foreach (var res in results)
            {
                var user = await ctx.Guild.GetMemberAsync(res.UserId);
                var displayName = user.DisplayName.Substring(0, user.DisplayName.IndexOf("["));
                sb.AppendLine($"{displayName.PadRight(15)} {res.Score}");
            }

            await ctx.RespondAsync(sb.ToString());
        }

        [Command("fore")]
        [Description("Updates a users score. Example use: `!fore -5` or `!fore 5`")]
        public async Task UpdateUserScore(CommandContext ctx, 
            [Description("Amount to modify current score")]int modifier)
        {
            var data = mongo.GetParticipantInfo(ctx.User, ctx.Guild);
            int originalScore = data.Score;
            data.Score += modifier;

            var updatedData = mongo.UpsertParticipant(data);

            await ctx.RespondAsync($"Ok, I've updated your score from {originalScore} to {updatedData.Score}, {ctx.User.Mention}");
        }
    }
}