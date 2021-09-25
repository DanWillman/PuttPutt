using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;

namespace PuttPutt.Models
{
    /// <summary>
    /// Wrapper model for DSharpDiscordMember
    /// </summary>
    public class Member
    {
        /// <summary>
        /// User unique ID
        /// </summary>
        public ulong Id { get; set; }
        
        /// <summary>
        /// Display name in guild
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Member()
        {
        }
    }
}
