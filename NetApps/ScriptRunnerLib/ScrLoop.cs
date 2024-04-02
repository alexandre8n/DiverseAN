using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            string exp1 = "";
            string exp2 = "";
            string exp3 = "";

            if (loopName == ScriptRunner.defFor)
            {
                var lst = UtlParserHelper.SplitSafe(header, ";", false, true);
                if (lst.Count != 3)
                {
                    throw new Exception($"Error: incorrect header of for-operator:\n{header}...\n" +
                        "Expected: for(exp1;exp2;exp3), exp1,exp1,exp1 - may be empty");
                }
                exp1 = lst[0];
                exp2 = lst[1];
                exp3 = lst[2];
            }
            else if(loopName == ScriptRunner.defWhile)
            {
                exp2 = header;
            }
            if (exp1 != "")
            {
                loopIniCmd = new ScrCmd(exp1, runOwner);
                loopIniCmd.Compile();
            }
            if (exp2 != "")
            {
                conditionCmd = new ScrCmd(exp2, runOwner);
                conditionCmd.Compile();
            }
            if (exp3 != "")
            {
                loopEndOfIterCmd = new ScrCmd(exp3, runOwner);
                loopEndOfIterCmd.Compile();
            }
        }
        internal override ExprVar Run(ScrMemory globalMemMngr)
        {
            if (loopIniCmd != null)
            {
                loopIniCmd.Run(globalMemMngr);
            }
            while(true)
            {
                if(conditionCmd != null)
                {
                    var isToLoop = conditionCmd.Run(globalMemMngr);
                    if (isToLoop.ToInt() == 0) break; //if false -> break;
                }
                var isToBreakContinue = base.Run(globalMemMngr);
                if (isToBreakContinue.ToInt() == afxBreak) break;
                if (loopEndOfIterCmd!=null)
                {
                    loopEndOfIterCmd.Run(globalMemMngr);
                }
            }
            return null;
        }
    }
}