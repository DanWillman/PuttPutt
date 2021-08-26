using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using PuttPutt.DataAccess;
using PuttPutt.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuttPutt.Commands
{
    [RequireRoles(RoleCheckMode.Any, new string[] { "modz", "Queen of Hell" })]
    class AdminCommands : BaseCommandModule
    {
        private MongoDataAccess mongo = new MongoDataAccess();
        private const string SCORE_MATCH = @"(\[|\{)[+-]*(\d)+(\]|\})";

        [Command("sync")]
        [Description("Modz only. Sync current user nicknames to the database. Overwrites any scores modified with !fore")]        
        public async Task SyncScores(CommandContext ctx)
        {
            var members = await ctx.Guild.GetAllMembersAsync();
            int success = 0;
            int fail = 0;

            Console.WriteLine($"Attempting to sync {members.Count} participants");

            foreach (var member in members)
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
                            UserId = member.Id,
                            DisplayName = SanitizeName(member.DisplayName, scoreMatch.Value)
                        };
                    }

                    scoreInfo.Score = score;
                    scoreInfo.DisplayName = SanitizeName(member.DisplayName, scoreMatch.Value);
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
            await ctx.RespondAsync($"All done, I updated {success} member{(success > 1 ? "s" : "")}");
        }

        /// <summary>
        /// Strip score and extra whitespace from username
        /// </summary>
        private string SanitizeName(string userName, string scoreValue)
        {
            var s = userName.Replace(scoreValue, "");
            return Regex.Replace(s, @"\s{2,}", " ");
        }
    }
}
