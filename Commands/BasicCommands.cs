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
        private const string SCORE_MATCH = @"\[[+-]*(\d)+\]";

        [Command("sync")]
        [Description("Modz only. Sync current user nicknames to the database. Overwrites any scores modified with !fore")]
        [RequireRoles(RoleCheckMode.Any, new string[] {"modz", "Queen of Hell"})]
        public async Task SyncScores(CommandContext ctx)
        {
            var members = await ctx.Guild.GetAllMembersAsync();
            int success = 0;
            int fail = 0;

            Console.WriteLine($"Attempting to sync {members.Count} participants");

            foreach(var member in members)
            {
                try
                {
                    if (!Regex.IsMatch(member.DisplayName, SCORE_MATCH))
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"Scoring {member.DisplayName}");
                    }

                    var scoreMatch = Regex.Match(member.DisplayName, SCORE_MATCH);
                    int score = int.Parse(scoreMatch.Value.Substring(1, scoreMatch.Value.Length - 2));

                    var scoreInfo = mongo.GetParticipantInfo(member, ctx.Guild);

                    if (scoreInfo == null)
                    {
                        scoreInfo = new Participant()
                        {
                            ServerId = ctx.Guild.Id,
                            UserId = member.Id
                        };
                    }

                    scoreInfo.Score = score;
                    mongo.UpsertParticipant(scoreInfo);
                    success++;
                }
                catch (ArgumentNullException)
                {
                    fail++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update score! {member.DisplayName} - {ex.GetType()}|{ex.Message}");
                    fail++;
                }
            }

            Console.WriteLine($"Finished syncing. {members.Count} total, {success} succeeded, {fail} failed");
            await ctx.RespondAsync($"All done, I updated {success} member{(success > 1 ? "s": "")}");
        }

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