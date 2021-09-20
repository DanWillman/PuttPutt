using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuttPutt.Services.BasicCommandService
{
    /// <inheritdoc/>
    public class BasicCommandService : IBasicCommandService
    {
        public Task<string> GetArchives(ulong serverId)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReportArchiveScoreboard(ulong serverId, string archiveName)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReportHistory(ulong serverId, ulong userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReportHistory(ulong serverId, ulong userId, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReportScore(ulong serverId, ulong userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> ReportScoreboard(ulong serverId)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> ReportScoreboard(ulong serverId, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<string> SetUserScore(ulong serverId, ulong userId, int score)
        {
            throw new NotImplementedException();
        }

        public Task<string> SetUserScore(ulong serverId, ulong userId, int score, string note)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateUserScore(ulong serverId, ulong userId, int modifier, string note)
        {
            throw new NotImplementedException();
        }
    }
}
