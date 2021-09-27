using DSharpPlus.Entities;
using PuttPutt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuttPutt.Utilities
{
    public static class MessageFormatter
    {
        private const string LINE_BREAK = "----------------------------------------";
        private const int CHARACTER_LIMIT = 2000;

        /// <summary>
        /// Formats the golfer scoreboard to a better discord message. With columns and everything!
        /// </summary>
        /// <param name="results">Scoreboard data to report</param>
        /// <param name="headerEmoji">Emoji to wrap header message in chat</param>
        public static List<string> FormatGolfersToDiscordMessage(List<Participant> results, DiscordEmoji headerEmoji)
        {
            return FormatGolfersToDiscordMessage(results, headerEmoji, "Scoreboard results!");
        }

        /// <summary>
        /// Formats the golfer scoreboard to a better discord message. With columns and everything!
        /// </summary>
        /// <param name="results">Scoreboard data to report</param>
        /// <param name="headerEmoji">Emoji to wrap header message in chat</param>
        /// <param name="headerMessage">Message displayed in chat</param>
        public static List<string> FormatGolfersToDiscordMessage(List<Participant> results, DiscordEmoji headerEmoji, string headerMessage)
        {
            List<string> messages = new List<string>();
            AddToDiscordMessages(messages, $"{headerEmoji}{headerEmoji}{headerEmoji}");

            foreach (var section in GetGolferResultsSections(results))
            {
                AddToDiscordMessages(messages, section, true);
            }

            return messages;
        }

        /// <summary>
        /// Formats the golfers event history into a better discord message
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public static List<string> FormatHistoryToDiscordMessage(List<Event> events)
        {
            List<string> messages = new();
            AddToDiscordMessages(messages, "Your history");

            foreach(var section in GetEventResultsSections(events))
            {
                AddToDiscordMessages(messages, section, true);
            }

            return messages;
        }

        /// <summary>
        /// Helpfully formats scoreboard into a single string
        /// </summary>
        private static List<string> GetGolferResultsSections(List<Participant> golfers)
        {
            List<string> results = new List<string>();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{PadToMaxWidth("Golfer", "Score")}");
            sb.AppendLine($"{LINE_BREAK}");

            foreach (var golfer in golfers)
            {
                if (sb.Length + golfer.DisplayName.Length >= CHARACTER_LIMIT - 100)
                {
                    results.Add(sb.ToString());
                    sb.Clear();
                }
                sb.AppendLine($"{PadToMaxWidth(golfer.DisplayName, golfer.Score.ToString())}");
            }

            results.Add(sb.ToString());

            return results;
        }
        
        /// <summary>
        /// Helpfully formats historical entries into a scoreboard style display
        /// </summary>
        private static List<string> GetEventResultsSections(List<Event> events)
        {
            List<string> results = new List<string>();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{PadToMaxWidth("Timestamp", "Modification")}");
            sb.AppendLine($"{LINE_BREAK}");

            var orderedEvents = events.OrderBy(e => e.EventTimeUTC).ToList();

            foreach (var e in orderedEvents)
            {
                var timestamp = $"{e.EventTimeUTC.ToShortDateString()} {e.EventTimeUTC.ToShortTimeString()} UTC";
                if (sb.Length + timestamp.Length >= CHARACTER_LIMIT - 100)
                {
                    results.Add(sb.ToString());
                    sb.Clear();
                }
                sb.AppendLine($"{PadToMaxWidth(timestamp, e.ScoreModifier.ToString())}");
            }

            results.Add(sb.ToString());

            return results;
        }

        /// <summary>
        /// Helper method pads to max width for two column layout
        /// </summary>
        private static string PadToMaxWidth(string firstColumn, string secondColumn)
        {
            const int TWO_COLUMN_MAX_LENGTH = 30;

            string format = "{0,-31} {1,8}";
            
            if (firstColumn.Length > TWO_COLUMN_MAX_LENGTH)
            {
                firstColumn = $"{firstColumn.Substring(0, 27)}...";
            }

            return string.Format(format, firstColumn, secondColumn);
        }

        /// <summary>
        /// Handles pagination for messages that may exceed the discord character limit
        /// </summary>
        /// <param name="messages">List of message strings currently being sent. Additions will be added to the last value, or appended to the list</param>
        /// <param name="addition">New string being added</param>
        /// <param name="codeBlock">Whether or not to wrap the addition string in codeblock markdown</param>
        private static void AddToDiscordMessages(List<string> messages, string addition, bool codeBlock = false)
        {            
            if (codeBlock)
            {
                addition = $"```{addition}```";
            }
            addition += Environment.NewLine;

            AddOrExtendDiscordStrings(messages, addition);
        }

        /// <summary>
        /// Paginates a string into a list to fit in a discord message
        /// </summary>
        /// <param name="messages">List of message strings currently being sent. Additions will be added to the last value, or appended to the list</param>
        /// <param name="addition">New string being added</param>
        public static void AddOrExtendDiscordStrings(List<string> messages, string addition)
        {
            if (messages.Count == 0)
            {
                messages.Add(addition);
            }
            else
            {
                if (messages[^1].Length + addition.Length > CHARACTER_LIMIT)
                {
                    messages.Add(addition);
                }
                else
                {
                    messages[^1] += addition;
                }
            }
        }
    }
}
