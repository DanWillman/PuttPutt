using DSharpPlus.Entities;
using PuttPutt.Models;
using System.Collections.Generic;

namespace PuttPutt.DataAccess
{
    /// <summary>
    /// Provides access to underlying data store in Mongo
    /// </summary>
    public interface IMongoDataAccess
    {
        /// <summary>
        /// Moves records from scores collection to a named archive collection entry
        /// </summary>
        /// <param name="server">Discord server for season</param>
        /// <param name="archiveName">Archival name given to season</param>
        void ArchiveSeason(DiscordGuild server, string archiveName);

        /// <summary>
        /// Gets all archive names from the specified server
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        List<string> GetArchivalNames(DiscordGuild server);

        /// <summary>
        /// Gets the archival entry for the specified server and name
        /// </summary>
        /// <param name="server"></param>
        /// <param name="archiveName"></param>
        /// <returns></returns>
        Archive GetArchive(DiscordGuild server, string archiveName);

        /// <summary>
        /// Get participant info for a specified user/server combination
        /// </summary>
        /// <param name="user">DiscordUser being searched for</param>
        /// <param name="server">DiscordGuild (server) tied to record</param>
        /// <returns></returns>
        Participant GetParticipantInfo(DiscordUser user, DiscordGuild server);

        Participant GetParticipantInfo(ulong userId, ulong serverId);

        /// <summary>
        /// Returns an unsorted list of all particpant entries for a specified server
        /// </summary>
        List<Participant> GetParticipants(DiscordGuild server);

        /// <summary>
        /// Updates the participant info, inserting if missing.
        /// <para/>
        /// Returns updated participant value
        /// </summary>
        Participant UpsertParticipant(Participant participant);
    }
}