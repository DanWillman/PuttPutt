using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PuttPutt.DataAccess;

namespace PuttPutt.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        private MongoDataAccess mongo = new MongoDataAccess();
        private const string SCORE_MATCH = @"\[[+-]*(\d)+\]";

        [Command("sync")]
        [Hidden]
        //[RequireRoles(RoleCheckMode.All, "admin")]
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
                    var scoreInfo = mongo.GetParticipantInfo(member, ctx.Guild);
                    var scoreMatch = Regex.Match(member.Nickname, SCORE_MATCH);
                    int score = int.Parse(scoreMatch.Value);

                    scoreInfo.Score += score;
                    mongo.UpsertParticipant(scoreInfo);
                    success++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update score! {member.Nickname} - {ex.GetType()}|{ex.Message}");
                    fail++;
                }
            }

            Console.WriteLine($"Finished syncing. {members.Count} total, {success} succeeded, {fail} failed");
        }
    }
}