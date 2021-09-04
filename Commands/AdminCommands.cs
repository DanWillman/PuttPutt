using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using PuttPutt.DataAccess;
using PuttPutt.Models;
using PuttPutt.Utilities;
using System;
using System.Threading.Tasks;

namespace PuttPutt.Commands
{
    [RequireRoles(RoleCheckMode.Any, new string[] { "modz", "Queen of Hell" })]
    //[RequireRoles(RoleCheckMode.Any, new string[] { "testing"})]
    class AdminCommands : BaseCommandModule
    {
        private MongoDataAccess mongo = new MongoDataAccess();

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
                    var score = UsernameUtilities.GetScore(member.DisplayName);                     

                    var scoreInfo = mongo.GetParticipantInfo(member, ctx.Guild);

                    if (scoreInfo == null)
                    {
                        scoreInfo = new Participant()
                        {
                            ServerId = ctx.Guild.Id,
                            UserId = member.Id,
                            DisplayName = UsernameUtilities.SanitizeUsername(member.DisplayName)
                        };
                    }

                    scoreInfo.Score = score;
                    scoreInfo.DisplayName = UsernameUtilities.SanitizeUsername(member.DisplayName);
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
    }
}
