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

        Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier);
        Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier, string note);

        Task<string> SetUserScore(ulong serverId, ulong userId, int score);
        Task<string> SetUserScore(ulong serverId, ulong userId, int score, string note);
    }
}