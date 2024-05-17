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

        public void SetBlockOwner(ScrBlock owner)
        {
            this.blockOwner = owner;
        }
        public ScrBlock GetOwner()
        {
            return this.blockOwner;
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
            expressionNode = new ExprNode(body);
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
                ExprVar res = ExecuteCmd(expressionNode);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception in cmdId: {id}, {ex.Message}");
            }
        }

        protected ExprVar ExecuteCmd(ExprNode node)
        {
            if (node == null) return null;
            ExprVar result = null;
            if (node.IsOperationNode())
            {
                result = ExecuteOperation(node.GetOperationDescriptor(), node.GetOperands());
            }
            else if(node.IsFunctionNode())
            {
                result = ExecuteFunction(node.GetFuncName(), node.GetOperands());
            }
            else if (node.IsBreakOrContinue())
            {
                result = new ExprVar();
                int bORc = (node.nodeType==NodeType.BREAK) ? 
                        ScrBlock.afxBreak : ScrBlock.afxContinue;
                result.SetVal(bORc);
            }
            else if(node.IsVarNode())
            {
                string varName = node.GetVarName();
                return GetVarFromMemory(varName);
            }
            return result;
        }

        private ExprVar ExecuteFunction(string funcName, ArrayList operands)
        {
            var vars = PrepareOperands(operands, "Function: "+funcName);
            var func = ScrSysFuncList.GetFunc(funcName);
            var vRes = func(vars);
            return vRes;
        }

        private ExprVar ExecuteOperation(ExprOperatorDsc opr, ArrayList operands)
        {
            // todo: assign brakets case a[b] = something, should be processed like a.b=something
            if (opr.m_Code == OperationCode.OPR_POINT && operands.Count == 2) 
            {
                ExprVar exprVar = ExecuteDotCall(operands, opr.m_OperationText);
                return exprVar;
            }
            var vars = PrepareOperands(operands, opr.m_OperationText);
            ExprVar result = opr.Calculate(vars);
            return result;
        }

        private ExprVar ExecuteDotCall(ArrayList operands, string oprText)
        {

            ExprVar var1 = PrepareSubNode((ExprNode)operands[0], oprText);
            ExprNode op2 = (ExprNode)operands[1];
            string funcName = op2.GetFuncName();
            List<ExprVar> vars = PrepareOperands(op2.GetOperands(), ".");
            vars.Insert(0, var1);
            string typeOfObj = var1.GetTypeOfObj();
            if(funcName.Length == 0)
            {
                funcName = (op2.IsPropertyToBeAssigned()) ? "setProperty" : "getProperty";
                string propName = op2.GetVarName();
                vars.Add(ExprVar.CrtVar(propName));
            }
            var func = ScrSysFuncList.GetFunc($"{typeOfObj}.{funcName}");
            if (func == null) 
                throw new Exception($"Error: failed to find a function {funcName} for {typeOfObj}-object");
            var vRes = func(vars);
            return vRes;
        }

        private List<ExprVar> PrepareOperands(ArrayList operands, string oprText)
        {
            List<ExprVar> vars = new List<ExprVar>();
            foreach (ExprNode subNode in operands)
            {
                ExprVar var = PrepareSubNode(subNode, oprText);
                vars.Add(var);
            }
            return vars;
        }

        private ExprVar PrepareSubNode(ExprNode subNode, string oprText)
        {
            ExprVar var;
            if (subNode.IsOperationNode())
            {
                var = ExecuteOperation(subNode.GetOperationDescriptor(), subNode.GetOperands());
            }
            else if (subNode.IsFunctionNode())
            {
                var = ExecuteFunction(subNode.GetFuncName(), subNode.GetOperands());
            }
            else if (subNode.IsArray())
            {
                var = CreateArray(subNode);
            }
            else if (subNode.IsConstNode())
            {
                var = subNode.GetVar();
            }
            else if (subNode.IsProperty())
            {
                string varName = subNode.GetVarName();
                var = new ExprVar(varName, EType.E_STRING, varName, null);
            }
            else if (subNode.IsVarNode())
            {
                string varName = subNode.GetVarName();
                var = GetVarFromMemory(varName);
                if (var == null) throw new Exception($"Error: variable {varName} is not defined");
            }
            else
            {
                string info = UtlParserHelper.Subs(body, 0, 100);
                throw new Exception($"Error: Unexpected type of operand for {oprText} operation:\n{info}...");
            }
            return var;
        }

        private ExprVar CreateArray(ExprNode subNode)
        {
            var vars = PrepareOperands(subNode.GetOperands(), "Create Array");
            return ExprVar.CrtVar(vars);
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
    }
}
