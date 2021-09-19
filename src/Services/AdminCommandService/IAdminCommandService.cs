using DSharpPlus.Entities;
using PuttPutt.Models;
using System.Collections.Generic;

namespace PuttPutt.Services.AdminCommandService
{
    public interface IAdminCommandService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="members"></param>
        /// <param name="serverId"></param>
        /// <param name="archiveName"></param>
        /// <returns>Number of successfully created members</returns>
        int StartSeason(IReadOnlyCollection<Member> members, ulong serverId, string archiveName = "");

        /// <summary>
        /// Parses provided collection of DiscordMembers and upserts them to the database
        /// </summary>
        /// <param name="members"></param>
        /// <param name="serverId"></param>
        /// <returns>Number of successfully synced members</returns>
        int SyncScores(IReadOnlyCollection<Member> members, ulong serverId);
    }
}