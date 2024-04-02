using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    public class ScriptRunner
    {
        ScriptRunner parent = null;
        int cmdId = 0;
        public const string defFunction = "function";
        public const string defIf = "if";
        public const string defElseIf = "elseif";
        public const string defElse = "else";
        public const string defElseIfPattern = @"^(else\s*if)([ ({])";
        public const string defElsePattern = @"^(else\s*)([ (;{])";
        public const string defFor = "for";
        public const string defWhile = "while";
        public string message = "";
        public List<ScrCmd> definitions = new List<ScrCmd>();
        public List<ScrCmd> commands = new List<ScrCmd>();
        
        private string script;
        private ScrMemory memManager = null;
        
        public ScriptRunner() { }
        public ScriptRunner(ScriptRunner parent) 
        {
            this.parent = parent;
        }

        public int GetNextCmdId()
        {
            if (parent == null) return ++cmdId;
            return parent.GetNextCmdId();
        }
        public bool Parse(string strScript) 
        {
            script = CleanScript(strScript); //from \r, and from comments like://
            try
            {
                ParseCommands();
            } 
            catch (Exception ex)
            { 
                message += ex.Message;
                return false;            
            }

            return true;
        }

        public static string CleanScript(string strScript)
        {
            List<string> arr = UtlParserHelper.SplitSafe(strScript, "\n", true, true);
            arr = arr.Where(s => UtlParserHelper.Subs(s, 0, 2) != "//").ToList();
            var splitDelim = new string[] { "//" };
            string outStr = "";
            foreach (string str in arr)
            {
                var pr = UtlParserHelper.FindSafeOneOf(str, 0, splitDelim);
                outStr += (pr == null) ? str : pr.val;
                outStr += "\n";
            }
            return outStr;
        }

        private void ParseCommands()
        {
            // split script into operators
            var res = new ParseRes();
            //var res = findEndOfCmd(null)
            for (; res != null && res.end<script.Length;)
            {
                string test = script.Substring(res.end);
                if (IsFuncDef(res))
                {
                    res = ParseFuncDef(res);
                }
                else if (IsInternalBlock(res))
                {
                    res = ParseInternalBlock(res);
                }
                else if (IsIfBlock(res))
                {
                    res = ParseIfBlock(res);
                }
                else if (IsLoopBlock(res))
                {
                    res = ParseLoopBlock(res);
                }
                else
                {
                    res = ParseCmd(res);
                }
            }
        }

        private ParseRes ParseInternalBlock(ParseRes prevRes)
        {
            var res = prevRes.Clone();
            if (!IsInternalBlock(prevRes))
                throw new Exception("Error: internal block is expected, but encountered:\n" +
                    script.Substring(prevRes.end, 80));

            res = GetBodyBlock(res); // from { till }
            if (res == null)
            {
                string info = UtlParserHelper.Subs(script, prevRes.end, 80);
                throw new Exception($"Error: while processing internal block-body.\n{info}\n" +
                        "incorrect body, should be {...}, possibly {} or quatation mark error...");
            }

            string body = res.val;
            var blk = new ScrBlock(body, GetRunOwner());
            commands.Add(blk);
            blk.Compile();
            string sCurTest = script.Substring(res.end);
            return res;
        }

        private bool IsInternalBlock(ParseRes res)
        {
            return IsGeneralBlock(res, "");
        }

        private bool IsFuncDef(ParseRes res)
        {
            var resSkipped = UtlParserHelper.SkipEmpties(script, res.end);
            res.end = resSkipped.end;
            if(UtlParserHelper.Subs(script,res.end, defFunction.Length) == defFunction)
            {
                string nextDelim = UtlParserHelper.Subs(script, res.end + defFunction.Length, 1);
                return UtlParserHelper.IsEmptyDelim(nextDelim);
            }
            return false;
        }

        private ParseRes ParseFuncDef(ParseRes prevRes)
        {
            var res = prevRes.Clone();
            if (!IsFuncDef(prevRes))
                throw new Exception("Error: function is expected, but encountered:\n" +
                    script.Substring(prevRes.end, 80));
            res.end += defFunction.Length+1;
            // func header: name(p1, p2, ...pn) {

            string strAfterFunc = script.Substring(res.end);
            var pattern = @"((?<=^|[ ]+)[a-zA-Z_][\w_]*(?=\s?[(]))";
            var matches = Regex.Matches(strAfterFunc, pattern);
            if (matches.Count <= 0)
            {
                string info = UtlParserHelper.Subs(script,prevRes.end, 80);
                throw new Exception($"Error: while processing function definition.\n{info}\n"+
                        "Expected correct function name ( and parameters...");
            }
            // match param list
            string funcName = matches[0].ToString();
            res.end += matches[0].Index + funcName.Length;
            res = GetConditionBlock(res);
            if(res == null)
            {
                string info = UtlParserHelper.Subs(script, res.end, 80);
                throw new Exception($"Error: while processing function parameters definition.\n{info}\n" +
                        "Expected parameters in parantheses...");
            }
            string funcParams = res.val;

            res = GetBodyBlock(res); // from { till }
            if (res == null)
            {
                string info = UtlParserHelper.Subs(script, prevRes.end, 80);
                throw new Exception($"Error: while processing function body.\n{info}\n" +
                        "Failed to find closing parenthesis or quatation mark...");
            }
            string funcBody = res.val;
            var funcDef = new ScrFuncDef(funcName, funcParams, funcBody, GetRunOwner());
            definitions.Add(funcDef);
            funcDef.Compile();
            return res;
        }

        public ScriptRunner GetRunOwner()
        {
            if (parent == null) return this;
            return parent.GetRunOwner();
        }

        private bool IsGeneralBlock(ParseRes prevRes, string blockName)
        {
            // currently used for if, for(...), while(...) or general {}
            var resSkipped = UtlParserHelper.SkipEmpties(script, prevRes.end);
            var res = resSkipped.Clone();
            if (UtlParserHelper.Subs(script, res.end, blockName.Length) == blockName)
            {
                string nextDelim = UtlParserHelper.Subs(script, res.end + blockName.Length, 1);
                if (nextDelim == "{" && blockName.Length == 0) return true; // internal block
                int iNex = " (\n".IndexOf(nextDelim);
                return " (\n".IndexOf(nextDelim) != -1;
            }
            return false;
        }

        private bool IsIfBlock(ParseRes prevRes)
        {
            return IsGeneralBlock(prevRes, defIf);
        }

        private ParseRes ParseIfBlock(ParseRes prevRes)
        {
            string info = UtlParserHelper.Subs(script, prevRes.end, 80);
            var res = prevRes.Clone();
            if (!IsIfBlock(prevRes))
                throw new Exception("Error: if-block is expected, but encountered:\n" +
                    script.Substring(prevRes.end, 80));
            res.end += defIf.Length;
            // IF header: if(logical-expr) {
            res = ProcessExpressionInParentheses(res, "if-block", info);
            string strIfCondition = res.val;
            res = ProcessBlockBody(res, "if-block-body", info);
            string body = res.val;
            var ifBlk = new ScrIf(enIfType.IF, strIfCondition, body, GetRunOwner());
            while(ParceIf4SubBlock(ifBlk, enIfType.ELSEIF, res, info));
            ParceIf4SubBlock(ifBlk, enIfType.ELSE, res, info);
            commands.Add(ifBlk);
            ifBlk.Compile();
            return res;
        }

        private ParseRes ProcessBlockBody(ParseRes res, string strBlkDescr, string info)
        {
            res = GetBodyBlock(res); // from { till }
            if (res == null)
            {
                throw new Exception($"Error: while processing {strBlkDescr}.\n{info}\n" +
                        "incorrect body, possibly {} or quatation mark error...");
            }
            return res;
        }

        private ParseRes ProcessExpressionInParentheses(ParseRes res, string strBlkDescr, string info)
        {
            res = GetConditionBlock(res);
            if (res == null)
            {
                throw new Exception($"Error: while processing {strBlkDescr}.\n{info}\n" +
                        "No ( condition block found...");
            }
            return res;
        }

        private bool ParceIf4SubBlock(ScrIf ifBlk, enIfType eifT, ParseRes prevRes, string info)
        {
            string sElseIf = IfBlockPart.TypeToStr(eifT);
            prevRes.end = UtlParserHelper.SkipEmpties(script, prevRes.end).end;
            string strAfter = UtlParserHelper.Subs(script, prevRes.end, 100);
            string pattern = (sElseIf == defElse) ? defElsePattern : defElseIfPattern;
            var matches = Regex.Matches(strAfter, pattern);
            if (matches.Count<1)
            {
                return false;
            }
            string found = matches[0].Groups[1].Value;
            prevRes.end = prevRes.end+ found.Length;
            found = Regex.Replace(found, " ", "");
            if (found != sElseIf) return false;

            string strIfCondition="";
            if (eifT == enIfType.ELSEIF)
            {
                // process condition
                var res1 = ProcessExpressionInParentheses(prevRes, "elseif-block", info);
                strIfCondition = res1.val;
                prevRes.end = res1.end;
            }
            // process block of operators
            var res = ProcessBlockBody(prevRes, "elseif/else-block-body", info);
            string body = res.val;
            prevRes.end = res.end;
            ifBlk.SetIfBlk(eifT, strIfCondition, body);
            return true;
        }

        private ParseRes GetBodyBlock(ParseRes prevRes)
        {
            var resSkipped = UtlParserHelper.SkipEmpties(script, prevRes.end);
            string strAfter = script.Substring(resSkipped.end);
            if (resSkipped.end >= script.Length)
            {
                return null;
            }
            var res = resSkipped.Clone();
            if(script[res.end] != '{') // possibly block is a simple instruction?
            {
                res = UtlParserHelper.FindSafeOneOf(script, prevRes.end, new string[] { ";" });
                if(res!=null)res.Forward();
                res.val = UtlParserHelper.SubsRng(script, prevRes.end, res.end);
                return res;
            }
            var resEndOfBody = UtlParserHelper.SkipSafeClosingParenthesis(script, res.end, "{", "}");
            if (resEndOfBody == null)
            {
                return null;
            }
            resEndOfBody.val = UtlParserHelper.SubsRng(script,res.end + 1, resEndOfBody.end - 1);
            return resEndOfBody;
        }

        private ParseRes GetConditionBlock(ParseRes resPrev)
        {
            var resSkipped = UtlParserHelper.SkipEmpties(script, resPrev.end);
            var res = UtlParserHelper.SkipSafeClosingParenthesis(script, resSkipped.end, "(", ")");
            if (res == null || resSkipped.end >= script.Length || script[resSkipped.end] != '(')
            {
                return null;
            }
            res.val = UtlParserHelper.SubsRng(script, resSkipped.end, res.end);
            return res;
        }

        private ParseRes ParseLoopBlock(ParseRes prevRes)
        {
            if(!IsLoopBlock(prevRes))
                throw new Exception("Error: loop-block is expected, but encountered:\n" +
                script.Substring(prevRes.end, 80));
            var res = UtlParserHelper.GetNextWord(script, prevRes.end, " (");
            string loopName = res.val;
            // IF header: if(logical-expr) {
            res = GetConditionBlock(res);
            if (res == null)
            {
                string info = UtlParserHelper.Subs(script, prevRes.end, 80);
                throw new Exception($"Error: while processing header of {loopName}-block.\n{info}\n" +
                        "check syntax and ( ) ...");
            }

            string strHeader = res.val;
            res = GetBodyBlock(res); // from { till }
            if (res == null)
            {
                string info = UtlParserHelper.Subs(script, prevRes.end, 80);
                throw new Exception($"Error: while processing {loopName}-block.\n{info}\n" +
                        "incorrect body, possibly {} or quatation mark error...");
            }

            string body = res.val;
            var loopBlk = new ScrLoop(loopName, strHeader, body, GetRunOwner());
            commands.Add(loopBlk);
            loopBlk.Compile();
            return res;
        }

        private bool IsLoopBlock(ParseRes prevRes)
        {
            if (IsGeneralBlock(prevRes, defFor)) return true;
            if (IsGeneralBlock(prevRes, defWhile)) return true;
            return false;
        }

        private ParseRes ParseCmd(ParseRes prevRes)
        {
            if (prevRes.end >= script.Length) return null;
            var res = UtlParserHelper.FindSafeOneOf(script, prevRes.end, new string[] {";"});
            if(res == null)
            {
                string info = UtlParserHelper.Subs(script, prevRes.end, 80);
                throw new Exception($"Error: while processing command.\n{info}...\n" +
                        "Expected to find (;) - the end of instruction...");
            }
            var cmd = new ScrCmd(res.val, GetRunOwner());
            cmd.Compile();
            commands.Add(cmd);
            res.Forward();
            return res;
        }

        private ParseRes findEndOfCmd(ParseRes prevRes)
        {
            // all commands are ended by ; or by }
            // all literals are "" or '' quatation marks inside are \" or '\, \ are \\
            if(prevRes == null)
            {
                return null;
            }
            string[] delims = new string[] {";"};
            ParseRes res = UtlParserHelper.FindSafeOneOf(script, prevRes.end, delims);
            return res;
        }

        public bool Run()
        {
            memManager = new ScrMemory();
            try
            {
                RunCommands();
            }
            catch (Exception ex)
            {
                message += ex.Message;
                return false;
            }
            return true;
        }

        private void RunCommands()
        {
            foreach (var cmd in commands)
            {
                cmd.Run(memManager);
            }
        }

        public ExprVar GetVar(string name)
        {
            var v = memManager.GetVar(name);
            return v;
        }

        public string Dump(int infoLevel)
        {
            // infoLevel = 1 short, 2 - detailed

            string strOut = "*** Commands:\n";
            // commmands
            foreach( var cmd in commands)
            {
                strOut += cmd.Dump(infoLevel, 0) + "\n";
            }
            // definitions
            strOut += "*** Definitions:\n";
            foreach (var def in definitions)
            {
                strOut += def.ToString() + "\n";
            }
            strOut = UtlParserHelper.ReplaceManyToOne(strOut, "\n");
            strOut += "*** End of Dump ***\n";
            return strOut;
        }
    }
}
