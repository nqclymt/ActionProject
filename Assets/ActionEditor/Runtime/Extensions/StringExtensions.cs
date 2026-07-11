using Unity.VisualScripting;

namespace PKC.ActionEditor
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            str = char.ToUpper(str[0]) + str.Substring(1);
            return System.Text.RegularExpressions.Regex.Replace(str, "(?<=[a-z])([A-Z])", " $1").Trim();
        }
    }
}