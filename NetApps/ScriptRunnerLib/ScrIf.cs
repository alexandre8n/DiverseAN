using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public enum enIfType
{
    IF = 0,
    ELSEIF = 1,
    ELSE = 2
}

namespace ScriptRunnerLib
{
    public class IfBlockPart
    {
        static string[] afxTypeStr = new string[]{ "if", "elseif", "else" };
        
        public enIfType typeOfIfPart = enIfType.IF; 
        public string condition="";
        ScrCmd conditionExpr = null;
        ScrBlock ifBodyBlock = null;
        ScriptRunner runOwner = null;
        static public string TypeToStr(enIfType ifType) { return afxTypeStr[(int)ifType]; }
        public IfBlockPart(enIfType typeOfIfPart, string condition, string body, ScriptRunner runOwner)
        {
            this.runOwner = runOwner;
            this.typeOfIfPart = typeOfIfPart;
            this.condition = condition;
            conditionExpr = new ScrCmd(condition, runOwner.GetRunOwner());
            this.ifBodyBlock = new ScrBlock(body, runOwner.GetRunOwner());
            this.ifBodyBlock.SetName(TypeToStr(this.typeOfIfPart));
        }
        public override string ToString()
        {
            string s = $"{TypeToStr(typeOfIfPart)}({condition})\n" +
                $"{ifBodyBlock.ToString()}";
            return s;
        }
        internal string Dump(int infoLevel, int nestingLvl)
        {
            string OffsInChar = new string(' ', 3 * nestingLvl);
            string strCond = (typeOfIfPart == enIfType.ELSE) ? "" : $"({condition})";
            string idStr = (typeOfIfPart == enIfType.ELSE) ? ifBodyBlock.IdStr() : conditionExpr.IdStr();
            string s1 = $"{idStr}{OffsInChar}{TypeToStr(typeOfIfPart)}{strCond}\n";
            string s2 = $"{ifBodyBlock.Dump(infoLevel, nestingLvl+1)}";
            return s1+s2;
        }

        public void Compile()
        {
            try
            {
                conditionExpr.Compile();
                ifBodyBlock.CompileBlockBody();
            }
            catch (Exception ex)
            {
                string info = UtlParserHelper.Subs(ifBodyBlock.ToString(), 0, 100);
                throw new Exception($"Error: while compiling if-block body:\n{info}...\n{ex.Message}");
            }
        }

        internal bool IsConditionOk(ScrMemory globalMemMngr, ScrBlock scrBlock)
        {
            if (typeOfIfPart == enIfType.ELSE) return true;
            // calculate logical expression
            conditionExpr.SetBlockOwner(scrBlock);
            var res = conditionExpr.Run(globalMemMngr);
            return res.ToInt() != 0;
        }

        internal ExprVar Run(ScrMemory globalMemMngr, ScrBlock scrBlock)
        {
            ifBodyBlock.SetBlockOwner(scrBlock);
            var vRes = ifBodyBlock.Run(globalMemMngr);
            return vRes;
        }
    }
    public class ScrIf : ScrBlock
    {
        public List<IfBlockPart> IfBlockParts = new List<IfBlockPart>();
        private ScrIf(ScriptRunner runOwner):base(runOwner)
        {
            body = "";
            this.type = CmdType.Undefined;
        }
        public ScrIf(enIfType type, string condition, string body, ScriptRunner runOwner)
            : this(runOwner)
        {
            SetIfBlk(type, condition, body);
            this.runOwner = runOwner;
        }

        public void SetIfBlk(enIfType type, string condition, string body)
        {
            var ifObj = new IfBlockPart(type, TrimHeader(condition), body, runOwner);
            IfBlockParts.Add(ifObj);
        }

        public override string Dump(int infoLevel, int nestingLvl)
        {
            string OffsInChar = new string(' ', 3 * nestingLvl+ IdStr().Length);

            string info = string.Empty;
            foreach (var ifObj in IfBlockParts)
            {
                info += ifObj.Dump(infoLevel, nestingLvl);
            }
            return $"{info}\n{OffsInChar}End of if\n";
        }

        public override string ToString()
        {
            string info = string.Empty;
            foreach(var ifObj in IfBlockParts)
            {
                info += ifObj.ToString();
            }
            return $"{IdStr()}{info}\n";
        }
        public override void Compile()
        {
            foreach(var ifObj in IfBlockParts)
            {
                ifObj.Compile();
            }
        }
        internal override ExprVar Run(ScrMemory globalMemMngr)
        {
            if (IfBlockParts.Count == 0) return null;
            var ifBase = IfBlockParts[0];
            if(ifBase.IsConditionOk(globalMemMngr, GetOwner()))
            {
                var vRes = ifBase.Run(globalMemMngr, GetOwner());
                return vRes;
            }
            for(int i=1; i<IfBlockParts.Count; i++)
            {
                var ifPart = IfBlockParts[i];
                if(ifPart.IsConditionOk(globalMemMngr, GetOwner()))
                {
                    var vRes = ifPart.Run(globalMemMngr, GetOwner());
                    return vRes;
                }
            }
            return null;
        }

    }
}
