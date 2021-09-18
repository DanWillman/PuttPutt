using NUnit.Framework;
using PuttPutt.Utilities;

namespace UnitTests
{
    public class UsernameUtilitiesTests
    {
        [Test]
        [TestCase("Dan [-52]", "Dan")]
        [TestCase("Dan [52]", "Dan")]
        [TestCase("Dan (He/Him) [-52]", "Dan (He/Him)")]
        [TestCase("Dan {-52}", "Dan")]
        [TestCase("Dan {52}", "Dan")]
        [TestCase("Dan (-52)", "Dan")]
        [TestCase("Dan", "Dan")]
        public void SanitizeUsername_HappyPath(string input, string expected)
        {
            var actual = UsernameUtilities.SanitizeUsername(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("Dan [-52]", -52)]
        [TestCase("Dan [52]", 52)]
        [TestCase("Dan (He/Him) [-52]", -52)]
        [TestCase("Dan {-52}", -52)]
        [TestCase("Dan {52}", 52)]
        [TestCase("Dan (-52)", -52)]
        public void GetScore_HappyPath(string input, int expected)
        {
            var actual = UsernameUtilities.GetScore(input);

            Assert.AreEqual(expected, actual);
        }

        [TestCase("Dan [-52]", -69, "Dan [-69]")]
        [TestCase("Dan [52]", 69, "Dan [69]")]
        [TestCase("Dan (He/Him) [-52]", -69, "Dan (He/Him) [-69]")]
        [TestCase("Dan {-52}", -69, "Dan {-69}")]
        [TestCase("Dan {52}", 69, "Dan {69}")]
        [TestCase("Dan (-52)", -69, "Dan (-69)")]
        [TestCase("Dan", 69, "Dan")]
        [Test]
        public void UpdateUsernameScore_HappyPath(string input, int newScore, string expected)
        {
            Assert.AreEqual(expected, UsernameUtilities.UpdateUsernameScore(input, newScore));
        }
    }
}
