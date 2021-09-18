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
    }
}
