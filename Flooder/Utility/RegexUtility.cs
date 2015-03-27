using System.Text.RegularExpressions;

namespace Flooder.Utility
{
    public class RegexUtility
    {
        public static string ChangeRegexPattern(string source)
        {
            return Regex.Replace(source, ".", m =>
            {
                var mValue = m.Value;
                if (mValue.Equals("?")) return ".";
                if (mValue.Equals("*")) return ".*";
                return Regex.Escape(mValue);
            });
        }

        public static bool IsLike(string input, string keyword)
        {
            var pattern = ChangeRegexPattern(keyword);
            return Regex.IsMatch(input, pattern);
        }
    }
}
