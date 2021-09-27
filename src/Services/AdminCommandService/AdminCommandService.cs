using PuttPutt.DataAccess;
using PuttPutt.Models;
using PuttPutt.Utilities;
using System;
using System.Collections.Generic;

namespace PuttPutt.Services.AdminCommandService
{
    /// <inheritdoc/>
    public class AdminCommandService : IAdminCommandService
    {
        private readonly IMongoDataAccess mongo;

        /// <summary>
        /// Default IoC constructor
        /// </summary>
        /// <param name="mongo"></param>
        public AdminCommandService(IMongoDataAccess mongo)
        {
            this.mongo = mongo;
        }

        public int SyncScores(List<Member> members, ulong serverId)
        {
            int successCount = 0;
            int failCount = 0;

            Console.WriteLine($"Attempting to sync {members.Count} participants");

            foreach (var member in members)
            {
                try
                {
                    var score = UsernameUtilities.GetScore(member.DisplayName);

                    var scoreInfo = mongo.GetParticipantInfo(member.Id, serverId);

                    if (scoreInfo == null)
                    {
                        scoreInfo = new Participant()
                        {
                            ServerId = serverId,
                            UserId = member.Id,
                            DisplayName = UsernameUtilities.SanitizeUsername(member.DisplayName)
                        };
                    }

                    scoreInfo.Score = score;
                    scoreInfo.DisplayName = UsernameUtilities.SanitizeUsername(member.DisplayName);
                    mongo.UpsertParticipant(scoreInfo);
                    successCount++;
                }
                catch (ArgumentNullException)
                {
                    failCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update score! {member.DisplayName} - {ex.GetType()}|{ex.Message}");
                    failCount++;
                }
            }

            Console.WriteLine($"Finished syncing. {members.Count} total, {successCount} succeeded, {failCount} failed");

            return successCount;
        }

        public int StartSeason(List<Member> members, ulong serverId) => StartSeason(members, serverId, DateTime.UtcNow.ToString());

        public int StartSeason(List<Member> members, ulong serverId, string archiveName)
        {
            int success = 0;
            int fail = 0;
            
            mongo.ArchiveSeason(serverId, archiveName);

            foreach (var member in members)
            {
                try
                {
                    var scoreInfo = new Participant()
                    {
                        ServerId = serverId,
                        UserId = member.Id,
                        DisplayName = UsernameUtilities.SanitizeUsername(member.DisplayName),
                        Score = 0
                    };

                    mongo.UpsertParticipant(scoreInfo);
                    success++;
                }
                catch (ArgumentNullException)
                {
                    fail++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update score! {member.DisplayName} - {ex.GetType()}|{ex.Message}");
                    fail++;
                }
            }

            return success;
        }

        public void UpdateUsername(Participant entry, string newName)
        {
            entry.DisplayName = UsernameUtilities.SanitizeUsername(newName);
            mongo.UpsertParticipant(entry);
        }
    }
}
