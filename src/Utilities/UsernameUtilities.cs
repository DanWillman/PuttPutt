using System.Text.RegularExpressions;

namespace PuttPutt.Utilities
{
    public static class UsernameUtilities
    {
        private const string SCORE_MATCH = @"(\[|\{|\()[+-]*(\d)+(\]|\}|\))";

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
