using System.Text.RegularExpressions;

namespace PuttPutt.Utilities
{
    public static class UsernameUtilities
    {
        private const string SCORE_MATCH = @"[\[\{\(][+-]*(\d)+[\]\}\)]";

        /// <summary>
        /// Parses the raw integer score from the username
        /// </summary>
        public static int GetScore(string source)
        {
            var scoreMatch = Regex.Match(source, SCORE_MATCH);
            return int.Parse(scoreMatch.Value.Substring(1, scoreMatch.Value.Length - 2));
        }

        /// <summary>
        /// Strip score and extra whitespace from username
        /// </summary>
        public static string SanitizeUsername(string source)
        {
            var scoreMatch = Regex.Match(source, SCORE_MATCH);

            if (scoreMatch.Success)
            {
                source = source.Replace(scoreMatch.Value, "");                
            }
            return Regex.Replace(source, @"\s{2,}", " ").Trim();
        }

        /// <summary>
        /// Replaces the existing int score in a user name, if present, and returns a trimmed string of the new username.
        /// <para/>
        /// If no existing score is found, the original name is returned after being trimmed
        /// </summary>
        /// <param name="source">Original username</param>
        /// <param name="newScore">New int score to replace</param>
        public static string UpdateUsernameScore(string source, int newScore)
        {
            var scoreMatch = Regex.Match(source, SCORE_MATCH);

            if (scoreMatch.Success)
            {
                var scoreString = scoreMatch.Value.Replace(scoreMatch.Value.Substring(1, scoreMatch.Value.Length - 2), newScore.ToString());
                source = source.Replace(scoreMatch.Value, scoreString);
            }

            return source.Trim();
        }
    }
}
