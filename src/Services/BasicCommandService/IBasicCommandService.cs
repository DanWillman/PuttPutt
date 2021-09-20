using System.Collections.Generic;
using System.Threading.Tasks;

namespace PuttPutt.Services.BasicCommandService
{
    public interface IBasicCommandService
    {
        Task<List<string>> ReportScoreboard(ulong serverId);
        Task<List<string>> ReportScoreboard(ulong serverId, int limit);

        Task<string> GetArchives(ulong serverId);

        Task<string> ReportArchiveScoreboard(ulong serverId, string archiveName);

        Task<string> ReportScore(ulong serverId, ulong userId);

        Task<string> ReportHistory(ulong serverId, ulong userId);

        Task<string> ReportHistory(ulong serverId, ulong userId, int limit);

        Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier);
        Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier, string note);

        Task<string> SetUserScore(ulong serverId, ulong userId, int score);
        Task<string> SetUserScore(ulong serverId, ulong userId, int score, string note);
    }
}