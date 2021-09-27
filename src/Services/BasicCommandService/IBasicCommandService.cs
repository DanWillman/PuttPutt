using DSharpPlus.Entities;
using System.Collections.Generic;

namespace PuttPutt.Services.BasicCommandService
{
    public interface IBasicCommandService
    {
        /// <summary>
        /// Returns a list of strings formatted for discord messages
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="headerEmoji"></param>
        /// <returns></returns>
        List<string> ReportScoreboard(ulong serverId, DiscordEmoji headerEmoji);

        /// <summary>
        /// Returns a list of strings formatted for discord messages
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="limit"></param>
        /// <param name="headerEmoji"></param>
        /// <returns></returns>
        List<string> ReportScoreboard(ulong serverId, int limit, DiscordEmoji headerEmoji);

        /// <summary>
        /// Returns a formatted string of all archives for a given server
        /// </summary>
        string GetArchives(ulong serverId);

        /// <summary>
        /// Reports an archive's scoreboard
        /// </summary>
        List<string> ReportArchiveScoreboard(ulong serverId, string archiveName, DiscordEmoji headerEmoji);

        /// <summary>
        /// Returns a list of strings formatted for discord messages representing the user's score history
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<string> ReportHistory(ulong serverId, ulong userId);

        /// <summary>
        /// Returns a list of strings formatted for discord messages representing the user's score history
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        List<string> ReportHistory(ulong serverId, ulong userId, int limit);

        /// <summary>
        /// Modifies a users score in the database
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="modifier"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        (string Response, int Score) UpdateUserScore(ulong serverId, ulong userId, int modifier, string displayName);

        /// <summary>
        /// Modifies a users score in the database
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="modifier"></param>
        /// <param name="displayName"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        (string Response, int Score) UpdateUserScore(ulong serverId, ulong userId, int modifier, string displayName, string note);

        /// <summary>
        /// Sets a users score in the database
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="score"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        string SetUserScore(ulong serverId, ulong userId, int score, string displayName);

        /// <summary>
        /// Sets a users score in the database
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="score"></param>
        /// <param name="displayName"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        string SetUserScore(ulong serverId, ulong userId, int score, string displayName, string note);
    }
}