using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PuttPutt.Services.BasicCommandService
{
    public interface IBasicCommandService
    {
        List<string> ReportScoreboard(ulong serverId, DiscordEmoji headerEmoji);
        List<string> ReportScoreboard(ulong serverId, int limit, DiscordEmoji headerEmoji);

        string GetArchives(ulong serverId);

        Task<string> ReportArchiveScoreboard(ulong serverId, string archiveName);

        List<string> ReportHistory(ulong serverId, ulong userId);

        List<string> ReportHistory(ulong serverId, ulong userId, int limit);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="modifier"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        (string Response, int Score) UpdateUserScore(ulong serverId, ulong userId, int modifier, string displayName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="modifier"></param>
        /// <param name="displayName"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        (string Response, int Score) UpdateUserScore(ulong serverId, ulong userId, int modifier, string displayName, string note);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="userId"></param>
        /// <param name="score"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        string SetUserScore(ulong serverId, ulong userId, int score, string displayName);

        /// <summary>
        /// 
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