using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ScriptRunnerLib
{
    public class ScrLoop : ScrBlock
    {
        string loopName;
        string header;
        ScrCmd loopIniCmd = null;
        ScrCmd conditionCmd = null;
        ScrCmd loopEndOfIterCmd = null;
        bool isInOperInHeader = false;
        // for(a in arr) info
        ScrCmd varExpr4In = null;
        ScrCmd arrExpr4In = null;
        ExprVarArray arr4in = null;
        ExprVar currentElement = null;
        int arrLen = 0;
        int loopCount = 0;

        public ScrLoop(ScriptRunner runOwner):base(runOwner)
        {
            body = "";
            this.type = CmdType.LoopBlock;
        }
        public ScrLoop(string loopName, string header, string body, ScriptRunner runOwner) :
            this(runOwner)
        {
            SetLoopBlk(loopName, header, body);
        }

        public void SetLoopBlk(string loopName, string header, string body)
        {
            this.loopName = loopName;
            this.name = loopName;
            this.header = TrimHeader(header);
            this.body = body;
        }
        public override string ToString()
        {
            string sRes =  $"{IdStr()}loop-block {loopName}, header: {header}\n";
            sRes += base.ToString();
            return sRes;
        }
        public  override string Dump(int infoLevel, int nestingLvl)
        {
            string OffsInChar = new string(' ', 3 * nestingLvl);
            string OffsInChar2 = new string(' ', 3 * nestingLvl+ IdStr().Length);
            string s1 = $"{IdStr()}{OffsInChar}{loopName}({header})\n";
            string s2 = base.Dump(infoLevel, nestingLvl + 1);
            string s3 = $"{OffsInChar2}End of {loopName}\n";
            return s1+s2+s3;
        }

        public override void Compile()
        {
            operators = new List<ScrCmd>();
            try
            {
                PrepareHeaderExpressions();
                CompileBlockBody();
            }
            catch (Exception ex)
            {
                string info = UtlParserHelper.Subs(body, 0, 100);
                throw new Exception($"Error: while compiling {loopName}-block body:\n"+
                    $"{info}...\n{ex.Message}");
            }
        }
        private void PrepareHeaderExpressions()
        {
            // while(expr) or for(exp1;exp2;exp3)
            if (loopName == ScriptRunner.defFor)
            {
                if (IsForEachLoop())
                {
                    PrepareForEachHeader();
                }
                else
                {
                    PrepareForHeader();
                }
            }
            else if(loopName == ScriptRunner.defWhile)
            {
                conditionCmd = new ScrCmd(header, runOwner);
                conditionCmd.Compile();
            }
        }

        private bool IsForEachLoop()
        {
            Match m1 = Regex.Match(header, @"^((var|let) )?[a-z]\w* in .+", RegexOptions.IgnoreCase);
            return m1.Success;
        }

        private void PrepareForEachHeader()
        {
            isInOperInHeader = true;
            // should be like: let elm in <expr>
            var lst = UtlParserHelper.SplitSafe(header, " in ", true, true);
            if (lst.Count != 2)
                throw new Exception("$Error: while compiling header of the loop: {header}");
            string exp1 = lst[0];
            string exp2 = lst[1];
            varExpr4In = new ScrCmd(exp1, runOwner);
            varExpr4In.Compile();
            arrExpr4In = new ScrCmd(exp2, runOwner);
            arrExpr4In.Compile();
        }

        private void PrepareForHeader()
        {
            string exp1 = "";
            string exp2 = "";
            string exp3 = "";
            var lst = UtlParserHelper.SplitSafe(header, ";", false, true);
            if (lst.Count != 3)
            {
                throw new Exception($"Error: incorrect header of for-operator:\n{header}...\n" +
                    "Expected: for(exp1;exp2;exp3), exp1,exp1,exp1 - may be empty");
            }
            exp1 = lst[0];
            exp2 = lst[1];
            exp3 = lst[2];
            if (!string.IsNullOrEmpty(exp1))
            {
                loopIniCmd = new ScrCmd(exp1, runOwner);
                loopIniCmd.Compile();
            }
            if (!string.IsNullOrEmpty(exp2))
            {
                conditionCmd = new ScrCmd(exp2, runOwner);
                conditionCmd.Compile();
            }
            if (!string.IsNullOrEmpty(exp3))
            {
                loopEndOfIterCmd = new ScrCmd(exp3, runOwner);
                loopEndOfIterCmd.Compile();
            }
        }

        internal override ExprVar Run(ScrMemory globalMemMngr)
        {
            this.globalMemMngr = globalMemMngr;

            RunLoopIni();
            while(true)
            {
                if (!RunConditionCmd()) break;
                var isToBreakContinue = base.Run(globalMemMngr);
                if (isToBreakContinue.ToInt() == afxBreak) break;
                RunEndOfIterationCmd();
            }
            return null;
        }

        private void RunEndOfIterationCmd()
        {
            if (isInOperInHeader)
            {
                // loopEndOfIterCmd: sys<id>_i++
            }
            else if (loopEndOfIterCmd != null)
            {
                loopEndOfIterCmd.Run(globalMemMngr);
            }
            loopCount++;
        }

        private bool RunConditionCmd()
        {
            if (isInOperInHeader)
            {
                if (loopCount >= arrLen) return false;
                // assign var value of a[i];
                currentElement.Assign(arr4in.GetAt(loopCount));
            }
            else if (conditionCmd != null)
            {
                var isToLoop = conditionCmd.Run(globalMemMngr);
                if (isToLoop.ToInt() == 0) return false;
            }
            return true;
        }

        private void RunLoopIni()
        {
            loopCount = 0;

            if (isInOperInHeader)
            {
                varExpr4In.Run(globalMemMngr);
                string varName = varExpr4In.GetExprNode().GetVarName();
                arr4in = (ExprVarArray)arrExpr4In.Run(globalMemMngr);
                arrLen = arr4in.Length().ToInt();
                currentElement = GetVarFromMemory(varName);
            }
            else if (loopIniCmd != null)
            {
                loopIniCmd.Run(globalMemMngr);
            }
        }
    }
}