using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    public enum CmdType
    {
        Undefined,
        FuncDef,
        Expression,
        ClassDef,
        IfBlock,
        LoopBlock,
    }

    public class ScrCmd
    {
        public int id = 0;
        protected string body;
        protected CmdType type = CmdType.Undefined;
        protected string name="";
        ExprNode expressionNode = null;
        protected ScrMemory globalMemMngr = null;
        ScrBlock blockOwner = null;
        protected ScriptRunner runOwner = null;
        protected ScrCmd(ScriptRunner runOwner) 
        { 
            this.runOwner = runOwner;
            if(this.runOwner!=null)
                id = runOwner.GetNextCmdId();
        }
        public ScrCmd(string cmd, ScriptRunner runOwner) : this(runOwner) 
        {
            this.body = cmd.Trim();
        }

        public ScrMemory GetGlobalMemMngr() { return  globalMemMngr; }
        public void SetBlockOwner(ScrBlock owner)
        {
            this.blockOwner = owner;
        }
        public ScrBlock GetOwner()
        {
            return this.blockOwner;
        }
        public string GetBody()
        {
            return this.body;
        }
        public ExprNode GetExprNode()
        {
            return expressionNode;
        }
        public string IdStr()
        {
            return $"{id.ToString().PadLeft(5)}: ";
        }
        public override string ToString()
        {
            return $"{IdStr()}{body}";
        }
        public virtual string Dump(int infoLevel, int nestingLvl)
        {
            string OffsInChar = new string(' ', 3 * nestingLvl);
            return $"{IdStr()}{OffsInChar}{body}";
        }
        public static string TrimHeader(string header)
        {
            string str = header.Trim();
            if (str.Length == 0) return str;
            if (str[0]=='(' && str[str.Length - 1] == ')')
            {
                str = str.Substring(1, str.Length-2).Trim();
            }
            return str;
        }
        public virtual void Compile()
        {
            // check if this: let a = expr;
            expressionNode = new ExprNode(body, null);
            try
            {
                //parseVarDef();
                expressionNode.BuildExprTree();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: while compiling:\n{body}\n{ex.Message}");
            }
        }

        internal virtual ExprVar Run(ScrMemory globalMemMngr)
        {
            this.globalMemMngr = globalMemMngr;
            if( expressionNode == null ) return null;

            // collect variables:
            CollectDefinedVariable();
            try
            {
                var runner = new ScrNodeRunner(expressionNode, this);
                return runner.Run();
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception in cmdId: {id}, {ex.Message}");
            }
        }
        public ExprVar GetVarFromMemory(string varName)
        {
            ExprVar definedVar = null;
            var owner = blockOwner;
            while (definedVar == null && owner != null)
            {
                definedVar = owner.GetVarFromBlock(varName);
                if (definedVar == null) owner = owner.blockOwner;
            }
            if (definedVar == null)
            {
                definedVar = globalMemMngr.GetVar(varName);
            }
            return definedVar;
        }

        private void CollectDefinedVariable()
        {
            string varName = expressionNode.MemDefinedVarName();
            if (string.IsNullOrEmpty(varName)) return;
            bool isAlreadyDefined = false;
            if (blockOwner != null)
            {
                isAlreadyDefined = !blockOwner.AddDefinedVar(varName);
            }
            else 
            {
                isAlreadyDefined = !globalMemMngr.AddVar(varName, EType.E_UNDEF, ""); 
            }
            if(isAlreadyDefined)
            {
                string info = UtlParserHelper.Subs(body, 0, 100);
                throw new Exception($"Error: variable {varName} is already defined:\n{info}...");
            }
        }

        internal ScrFuncDef GetFuncDef(string funcName)
        {
            return runOwner.GetFuncDef(funcName);
        }
    }
}
