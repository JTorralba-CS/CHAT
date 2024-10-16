using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Standard.Functions
{
    public static class Core
    {
        public static string[] SplitSpaceInput(string input)
        {
            Regex Pattern = new Regex("(?:^| )(\"(?:[^\"]+|\"\")*\"|[^ ]*)", RegexOptions.Compiled);

            List<string> list = new List<string>();

            string current = null;

            foreach (Match match in Pattern.Matches(input))
            {
                current = match.Value;
                if (0 == current.Length)
                {
                    list.Add("");
                }

                list.Add(current.TrimStart(' ').TrimStart('"').TrimEnd('"').Trim());
            }

            return list.ToArray();
        }

        public static string[] SplitCommaInput(string input)
        {
            Regex Pattern = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

            List<string> list = new List<string>();

            string current = null;

            foreach (Match match in Pattern.Matches(input))
            {
                current = match.Value;
                if (0 == current.Length)
                {
                    list.Add("");
                }

                list.Add(current.TrimStart(',').TrimStart('"').TrimEnd('"').Trim());
            }

            return list.ToArray();
        }
    }
}
