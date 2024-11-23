using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscoursesFileProcessing
{
    public class Utl
    {
        public static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        internal static bool IsPartOfDict(Dictionary<string, string> subDict, Dictionary<string, string> mainDic)
        {
            foreach(var key in  subDict.Keys)
            {
                string val;
                if (mainDic.TryGetValue(key, out val) && val == subDict[key])
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        internal static Dictionary<string, string> ParseInRegxVars(string rg1, string sToParse)
        {
            // returns NULL if nothing matched, otherwise name,value dict
            // parse rg1 string to find rgx variables: ?<varName>
            // parse sToParse to get variables values to dict
            // Example:
            // rg1 = @"^\s+(?<number>\d+)[\r\n]+\s+(?<name>(\w|['.,?!: ])+)[\r\n]+\s+.+Játaka";
            // sToParse = "   109\r\n In His Majesty's Service\r\n Supatta Játaka";
            // -> number = "109" name = "In His Majesty's Service"
            var aMatch = Regex.Match(sToParse, rg1, RegexOptions.IgnoreCase);
            if(!aMatch.Success)
                return null;

            var dict = RgxNamesToDict(rg1);
            if (dict == null)
                return new Dictionary<string,string>(); // passed Ok, but no vars

            var foundKeys = dict.Keys.ToArray();

            foreach (var key in foundKeys)
            {
                dict[key] = aMatch.Groups[key].Value;
            }
            return dict;
        }

        internal static Dictionary<string, string> RgxNamesToDict(string rg1)
        {
            // parse rg1 string to find rgx variables: ?<varName>
            // save all found as keys in dictionary
            var dict = new Dictionary<string, string>();
            string text = rg1;
            string pat = @"[?]<(\w+)>";

            // Instantiate the regular expression object.
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.
            Match m = r.Match(text);
            if (!m.Success)
            {
                return null;    // it means not matched at all
            }
            int matchCount = 0;
            while (m.Success)
            {
                ++matchCount;
                Group g = m.Groups[1];
                CaptureCollection cc = g.Captures;
                for (int j = 0; j < cc.Count; j++)
                {
                    Capture c = cc[j];
                    dict[c.Value] = "";
                }
                m = m.NextMatch();
            }
            return dict;
        }


    }
}
