using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using PuttPutt.DataAccess;
using PuttPutt.Models;
using PuttPutt.Services.AdminCommandService;
using PuttPutt.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PuttPutt.Commands
{
    [RequireRoles(RoleCheckMode.Any, new string[] { "modz", "Queen of Hell" })]
    //[RequireRoles(RoleCheckMode.Any, new string[] { "testing"})]
    class AdminCommands : BaseCommandModule
    {
        private readonly IAdminCommandService adminService;
        private readonly IMongoDataAccess mongo;

        public AdminCommands(IAdminCommandService adminService, IMongoDataAccess mongo)
        {
            this.adminService = adminService;
            this.mongo = mongo;
        }

        [Command("sync")]
        [Description("Modz only. Sync current user nicknames to the database. Overwrites any scores modified with !fore")]
        public async Task SyncScores(CommandContext ctx)
        {
            var members = (await ctx.Guild.GetAllMembersAsync()).Select(m => new Member
                                                                {
                                                                    Id = m.Id,
                                                                    DisplayName = m.DisplayName
                                                                }).ToList();

            int success = adminService.SyncScores(members, ctx.Guild.Id);

            
            await ctx.RespondAsync($"All done, I updated {success} member{(success > 1 ? "s" : "")}");
        }

        [Command("season")]
        [Description("Modz only. Ends the current season, archiving the current scoreboard and begins anew.")]
        public async Task StartSeasonAsync(CommandContext ctx,
            [Description("Optional. Archival name for current season.")] string archiveName = "")
        {

            var members = (await ctx.Guild.GetAllMembersAsync()).Select(m => new Member
                                                                {
                                                                    Id = m.Id,
                                                                    DisplayName = m.DisplayName
                                                                }).ToList();
            int success = adminService.StartSeason(members, ctx.Guild.Id, archiveName);

            await ctx.RespondAsync($"All done, I created {success} new member{(success > 1 ? "s" : "")} for the season. Good luck with the new season!");
        }

        [Command("updatenames")]
        [Description("Modz only. Updates all user names on scoreboard.")]
        public async Task UpdateNames(CommandContext ctx)
        {
            var scores = mongo.GetParticipants(ctx.Guild.Id);

            foreach(var entry in scores)
            {
                var user = await ctx.Guild.GetMemberAsync(entry.UserId);
                var newName = UsernameUtilities.SanitizeUsername(user.DisplayName);

                entry.DisplayName = newName;

                mongo.UpsertParticipant(entry);
            }

            await ctx.RespondAsync($"All done!");
        }
    }
}
