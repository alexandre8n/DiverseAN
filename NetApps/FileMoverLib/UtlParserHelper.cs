using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (iStart >= str.Length) return null;
            for (int i = iStart; i < str.Length;)
            {
                if (Array.IndexOf(delims, Subs(str, i, 1)) != -1)
                {
                    return new ParseRes(i);
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
            return Subs(s, iFrom, iTo - iFrom);
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
                if (script[i] == qMark && script[i-1] != '\\')
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
        private static bool IsQuatMark(string qMark)
        {
            return QuatMarks.IndexOf(qMark) != -1;
        }


    }
}
