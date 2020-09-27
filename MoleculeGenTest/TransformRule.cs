using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MoleculeGenTest
{
    public class TransformRule
    {
        public int index;
        public string fromElm;
        public List<string> toElements = new List<string>();

        public static TransformRule PrcTransformLine(string line, int idx)
        {
            string[] words = line.Split("=>", System.StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 2)
                return null;
            var mdl = new TransformRule();
            mdl.index = idx;
            mdl.fromElm = words[0].Trim();
            mdl.AddToElementsFromStr(words[1]);
            return mdl;

        }

        public static List<string> GetElementsFromString(string strElements)
        {
            string regExpElm = "([A-Z][a-z])|[A-Z]";
            MatchCollection matches = Regex.Matches(strElements, regExpElm,
                                                          RegexOptions.IgnorePatternWhitespace);
            List<string> chemicalElements = new List<string>();
            foreach (Match match in matches)
            {
                string elm = match.Value;
                chemicalElements.Add(elm);
            }
            return chemicalElements;
        }
        private void AddToElementsFromStr(string strElements)
        {
            toElements = GetElementsFromString(strElements);
        }

        internal string Verbose()
        {
            string s1 = $"Rule: from={fromElm}, to={string.Join("", toElements)}";
            return s1;
        }
    }
}