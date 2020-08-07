using System.Collections.Generic;

namespace Helpers
{
    public class ContentReplacer
    {
        public Dictionary<string, Dictionary<string, string>> Replacements { get; set; }

        public ContentReplacer()
        {
            Replacements = new Dictionary<string, Dictionary<string, string>>();
        }

        public string ReplaceContent(string filename, string input)
        {
            string output = input;

            if (Replacements.ContainsKey(filename))
            {
                foreach (string oldValue in Replacements[filename].Keys)
                {
                    output = output.Replace(oldValue, Replacements[filename][oldValue]);
                }
            }

            return output;
        }
    }
}
