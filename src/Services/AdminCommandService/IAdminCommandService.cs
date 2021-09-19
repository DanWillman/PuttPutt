using PuttPutt.Models;
using System.Collections.Generic;

namespace PuttPutt.Services.AdminCommandService
{
    public interface IAdminCommandService
    {
        /// <summary>
        /// Starts a new season using the current timestamp as the archive name
        /// </summary>
        /// <returns>Number of successfully created members</returns>
        int StartSeason(List<Member> members, ulong serverId);

        /// <summary>
        /// Starts a new season with the provided archive name
        /// </summary>
        /// <returns>Number of successfully created members</returns>
        int StartSeason(List<Member> members, ulong serverId, string archiveName);

        /// <summary>
        /// Parses provided collection of DiscordMembers and upserts them to the database
        /// </summary>
        /// <returns>Number of successfully synced members</returns>
        int SyncScores(List<Member> members, ulong serverId);
    }
}