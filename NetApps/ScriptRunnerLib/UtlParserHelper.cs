using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    public class ParseRes
    {

        public string message = "";
        public bool isFound = false;
        public string val = "";
        public int start = 0;
        public int end = 0;
        public ParseRes() 
        { 
        }
        public ParseRes(int iStart)
        {
            this.start = iStart;
            this.end = iStart;
        }
        public ParseRes(int iStart, string val)
        {
            this.start = iStart;
            this.end = iStart;
            this.val = val;
            this.isFound = true;
        }


        public void Forward()
        {
            this.end++;
        }
        public ParseRes Clone() 
        {
            var res = new ParseRes();
            res.start = this.start;
            res.end = this.end;
            res.val = this.val;
            res.message = this.message;
            return res;
        }
    }
    public static class UtlParserHelper
    {
        public static string QuatMarks = "'"+'"';

        public static ParseRes GetNextWord(string str, int iStart, string oneChDelims)
        {
            for (int i = iStart; i < str.Length; i++)
            {
                if (oneChDelims.IndexOf(str[i]) >= 0)
                {
                    var res = new ParseRes(iStart);
                    res.end = i;
                    res.val = SubsRng(str, iStart, i);
                    return res;
                }
            }
            return null;
        }
        public static ParseRes FindSafeOneOf(string str, int iStart, string[] delims)
        {
            // consider quatations ",'
            // delims may be one char or several chars like: {";", ",", "//"}
            // it returns parsing result: with: found, end and val set
            // val = substring for sourceStr from iStart to found delimiter

            if (iStart >= str.Length) return null;
            int maxDelimSize = delims.Select(s => s.Length).Max();
            for (int i = iStart; i < str.Length;)
            {
                string strToTest = Subs(str, i, maxDelimSize);
                string foundDelim = delims.FirstOrDefault(c => c == Subs(strToTest, 0, c.Length));
                if (foundDelim != null)
                {
                    return new ParseRes(i, SubsRng(str,iStart,i));
                }
                else if (IsQuatMark(Subs(str, i, 1)))
                {
                    var res = SkipTillClosingQuatMark(str, i);
                    if (res == null)
                    {
                        return null;
                    }
                    i = res.end;
                    continue;
                }
                i++;
            }
            return null;
        }

        public static ParseRes SkipEmpties(string str, int start)
        {
            var res = new ParseRes();
            // skip " ", and comment //
            for (int i = start; i<str.Length; i++) 
            {
                if(str[i] == ' ' || str[i] == '\n' || str[i] == '\r')
                {
                    continue;
                }
                if(str.Substring(i,2) == "//")
                {
                    char[] chars = { '\r', '\n' };
                    int iEol = str.IndexOfAny(chars, i);
                    if (iEol != -1) i = str.Length;
                    else i = iEol;
                }
                res.end = i;
                return res;
            }
            res.end = str.Length;
            return res;
        }
        public static bool IsEmptyDelim(string str)
        {
            var ch = str[0];
            return ch == ' ' || ch == '\n' || ch == '\r';
        }
        public static string Subs(string s, int i1, int len)
        {
            if (i1 >= s.Length) return "";
            if (i1 + len >= s.Length) return s.Substring(i1);
            return s.Substring(i1, len);
        }
        public static string SubsRng(string s, int iFrom, int iTo)
        {
            if(iFrom < 0) iFrom = 0;
            if(iTo < 0) iTo = s.Length+iTo;
            if (iFrom > iTo)
            {
                int iTmp = iFrom;
                iFrom = iTo;
                iTo = iTmp;
            }
            return Subs(s, iFrom, iTo - iFrom);
        }
        public static string Left(string s, int len)
        {
            return Subs(s, 0, len);
        }
        public static string Right(string s, int len)
        {
            int iLen = s.Length;
            return SubsRng(s, iLen - len , iLen);
        }

        public static ParseRes SkipSafeClosingParenthesis(string script, int iStart, string parOpn, string parCls)
        {
            if (iStart >= script.Length || parCls.Length<1) return null;
            int iStart1 = iStart;
            if(Subs(script,iStart, parOpn.Length) == parOpn)
            {
                iStart1 += parOpn.Length;
            }
            int level = 1;
            for(int i=iStart1; i<script.Length; )
            {
                if(Subs(script, i, parCls.Length) == parCls)
                {
                    level--;
                    if(level == 0)
                    {
                        ParseRes res = new ParseRes(i+parCls.Length);
                        return res;
                    }
                }
                else if(Subs(script, i, parOpn.Length) == parOpn)
                {
                    level++;
                    i += parOpn.Length;
                    continue;
                }
                else if (IsQuatMark(Subs(script, i, 1)))
                {
                    var res = SkipTillClosingQuatMark(script, i);
                    if(res == null)
                    {
                        return null;
                    }
                    i = res.end;
                    continue;
                }
                i++;
            }
            return null;
        }

        private static ParseRes SkipTillClosingQuatMark(string script, int iStart)
        {
            // important by calling this function in iStart pos should be a quatation mark
            if(!IsQuatMark(Subs(script, iStart, 1))) return null;
            char qMark = script[iStart];

            for (int i = iStart+1;i < script.Length; i++)
            {
                if (script[i] == qMark)
                {
                    var res = new ParseRes(i+1);
                    return res;
                }
            }
            return null;
        }

        public static List<string> SplitSafe(string str, string delim, bool ignoreEmpty, bool trimFound)
        {
            List<string> resArr = new List<string>();
            var splitDelim = new string[] { delim };
            var parsRes = new ParseRes(0);
            int iCur = 0;
            var pr = FindSafeOneOf(str, iCur, splitDelim);
            while (true)
            {
                int iEnd = (pr == null) ? str.Length : pr.end;
                string elm = str.Substring(iCur, iEnd - iCur);
                if(trimFound) elm = elm.Trim();
                if (elm != "" || !ignoreEmpty)
                {
                    resArr.Add(elm);
                }
                if (pr == null) break;
                iCur = iEnd+delim.Length;
                pr = FindSafeOneOf(str, iCur, splitDelim);
            }
            return resArr;
        }
        public static bool IsQuatMark(string qMark)
        {
            return QuatMarks.IndexOf(qMark) != -1;
        }
        
        public static string ReplaceManyToOne(string sourceStr, string charsToTrim)
        {
            // any double combinations charsToTrim of sourceStr
            // will be trimmed to let only one.
            // Example: charsToTrim=" ", "   a    b  " -> " a b ", or "\n", "a\n\n\nb"->"a\nb"
            string whatToRpl = charsToTrim + charsToTrim;
            while(sourceStr.Contains(whatToRpl)) 
            {
                sourceStr = sourceStr.Replace(whatToRpl, charsToTrim);
            }
            return sourceStr;
        }

        public static string NormaliseNewLine(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
