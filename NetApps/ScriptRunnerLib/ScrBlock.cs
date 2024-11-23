
using System.Collections.Generic;
using System;

namespace ScriptRunnerLib
{
    public class ScrBlock : ScrCmd
    {
        public static int afxContinue = int.MinValue + 100;
        public static int afxBreak = int.MinValue + 101;
        public static int afxReturn = int.MinValue + 102;
        protected List<ScrCmd> operators = null;
        protected ScrMemory scrMemory = null;
        public ScrBlock(ScriptRunner runOwner):base(runOwner)
        { 
        }
        public ScrBlock(string body, ScriptRunner runOwner):this(runOwner)
        {
            this.body = body;
        }
        public override string Dump(int infoLevel, int nestingLvl)
        {
            string OffsInChar = new string(' ', 3*nestingLvl);

            var sRes = (infoLevel == 1)? "" : ToString();
            foreach (ScrCmd cmd in operators)
            {
                var sRes1 = cmd.Dump(infoLevel, nestingLvl) + "\n"; //OffsInChar + 
                sRes += sRes1;
            }
            return sRes;
        }

        public override string ToString()
        {
            string OffsInChar = new string(' ', 3);
            string strName = (name == "") ? "block" : name;
            string sRes =  $"{IdStr()} {strName}\n{OffsInChar}Body:\n{OffsInChar}{body}\n";
            sRes+=$"{OffsInChar}---end of {strName}\n";
            return sRes;
        }
        public override void Compile()
        {
            try
            {
                CompileBlockBody();
            }
            catch (Exception ex)
            {
                string info = UtlParserHelper.Subs(body, 0, 100);
                throw new Exception($"Error: while compiling internal block body:\n{info}...\n{ex.Message}");
            }
        }

        public void CompileBlockBody()
        {
            operators = new List<ScrCmd>();
            ScriptRunner scriptRunner = new ScriptRunner(runOwner.GetRunOwner());
            if (!scriptRunner.Parse(body))
            {
                throw new Exception(scriptRunner.message);
            }
            foreach (ScrCmd cmd in scriptRunner.commands)
            {
                cmd.SetBlockOwner(this);
                operators.Add(cmd);
            }
        }

        internal bool AddDefinedVar(string varName)
        {
            if(scrMemory==null) scrMemory = new ScrMemory();
            return scrMemory.AddVar(varName, EType.E_UNDEF, "");
        }
        internal ExprVar GetVarFromBlock(string varName)
        {
            if (scrMemory == null) return null;
            return scrMemory.GetVar(varName);
        }

        internal override ExprVar Run(ScrMemory globalMemMngr)
        {
            ExprVar vCmdRes = null;
            foreach (var cmd in operators)
            {
                vCmdRes = cmd.Run(globalMemMngr);
                if (IsReturn(vCmdRes) || IsBreak(vCmdRes) || IsContinue(vCmdRes)) break;
            }
            // clean block memory
            if(scrMemory!=null) scrMemory.Clear();
            return vCmdRes;
        }

        public static bool IsReturn(ExprVar vCmdRes)
        {
            if (vCmdRes == null || vCmdRes.GetTypeOfObj() != "List") return false;
            var lst = (List<ExprVar>)vCmdRes.GetObjBody();
            if(lst.Count<2) return false;
            var var1 = lst[0];
            if (var1.m_Type != EType.E_INT) return false;
            return (var1.ToInt() == afxReturn);
        }

        public static bool IsContinue(ExprVar vCmdRes)
        {
            return vCmdRes != null && vCmdRes.m_Type == EType.E_INT 
                && vCmdRes.ToInt() == afxContinue;
        }

        public static bool IsBreak(ExprVar vCmdRes)
        {
            return vCmdRes!=null && vCmdRes.m_Type == EType.E_INT 
                && vCmdRes.ToInt() == afxBreak;
        }

        public void SetName(string name)
        {
            this.name = name;
        }
    }
}