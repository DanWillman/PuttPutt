using System;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
namespace PuttPutt.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        [Command("sync")]
        [Hidden]
        //[RequireRoles(RoleCheckMode.All, "admin")]
        public async Task SyncScores(CommandContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}