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

        [Command("season")]
        [Description("Modz only. Ends the current season, archiving the current scoreboard and begins anew.")]
        public async Task StartSeasonAsync(CommandContext ctx,
            [Description("Optional. Archival name for current season.")] string archiveName = "")
        {
            archiveName = string.IsNullOrWhiteSpace(archiveName) ? DateTime.UtcNow.ToString() : archiveName; //Use current timestamp if not provided an archive name
            mongo.ArchiveSeason(ctx.Guild, archiveName);

            await ctx.RespondAsync($"I've finished archiving the season! Reach out to Dan if you have some changes to make still due to your timezone");

            var members = await ctx.Guild.GetAllMembersAsync();
            int success = 0;
            int fail = 0;

            Console.WriteLine($"Attempting to add {members.Count} participants");

            foreach (var member in members)
            {
                try
                {
                    var scoreInfo = new Participant()
                    {
                        ServerId = ctx.Guild.Id,
                        UserId = member.Id,
                        DisplayName = UsernameUtilities.SanitizeUsername(member.DisplayName),
                        Score = 0
                    };

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

            await ctx.RespondAsync($"All done, I created {success} new member{(success > 1 ? "s" : "")} for the season. Good luck with the new season!");
        }

        [Command("updatenames")]
        [Description("Modz only. Updates all user names on scoreboard.")]
        public async Task UpdateNames(CommandContext ctx)
        {
            var scores = mongo.GetParticipants(ctx.Guild);

            foreach(var entry in scores)
            {
                var user = await ctx.Guild.GetMemberAsync(entry.UserId);
                var newName = UsernameUtilities.SanitizeUsername(user.DisplayName);
                var oldName = entry.DisplayName;

                entry.DisplayName = newName;
                mongo.UpsertParticipant(entry);

                Console.WriteLine($"{oldName} --> {newName}");
            }

            await ctx.RespondAsync($"All done!");
        }
    }
}
