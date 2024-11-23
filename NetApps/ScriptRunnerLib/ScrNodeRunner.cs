using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection.Emit;

namespace ScriptRunnerLib
{
    internal class ScrNodeRunner
    {
        private ExprNode expressionNode;    // we have to execute this node
        private ScrCmd scrCmd;              // we have this command, that started this execution

        public ScrNodeRunner(ExprNode expressionNode, ScrCmd scrCmd)
        {
            this.expressionNode = expressionNode;
            this.scrCmd = scrCmd;
        }
        internal ExprVar Run()
        {
            return ExecuteCmd(expressionNode);
        }
        protected ExprVar ExecuteCmd(ExprNode node)
        {
            if (node == null) return null;
            ExprVar result = null;
            if (node.IsOperationNode())
            {
                result = ExecuteOperation(node);
            }
            else if (node.IsFunctionNode())
            {
                result = ExecuteFunction(node);
            }
            else if (node.IsBreakOrContinue())
            {
                result = new ExprVar();
                int bORc = (node.nodeType == NodeType.BREAK) ?
                        ScrBlock.afxBreak : ScrBlock.afxContinue;
                result.SetVal(bORc);
            }
            else if (node.IsVarNode())
            {
                string varName = node.GetVarName();
                return scrCmd.GetVarFromMemory(varName);
            }
            return result;
        }
        private ExprVar ExecuteOperation(ExprNode node)
        {
            ExprOperatorDsc opr = node.GetOperationDescriptor();
            var operands = node.GetOperands(); 
            // todo: assign brakets case a[b] = something, should be processed like a.b=something
            if (opr.m_Code == OperationCode.OPR_POINT && operands.Count == 2)
            {
                ExprVar exprVar = ExecuteDotCall(operands, opr.m_OperationText);
                return exprVar;
            }
            var vars = PrepareOperands(operands, opr.m_OperationText);
            if(opr.m_Code == OperationCode.OPR_BRACKETS)
            {
                // todo: check the case: dict[var] = x, this should work as setProp, if prop var is not avail
                var parentNode = node.GetParent();
                if (parentNode != null && parentNode.IsAssignOperatorNode())
                {
                    vars.Add(ExprVar.CrtVar(1)); // add one more parameter, indication of parent Assign operation
                }
            }
            ExprVar result = Calculate(opr, vars);
            return result;
        }
        private ExprVar ExecuteFunction(ExprNode node)
        {
            string funcName = node.GetFuncName();
            var operands = node.GetOperands();
            var vars = PrepareOperands(operands, "Function: " + funcName);
            var func = ScrSysFuncList.GetFunc(funcName);
            if (func != null)
            {
                var vRes = func(vars);
                return vRes;
            }
            // todo: try to find function in definitions
            ScrFuncDef scrFuncDef = scrCmd.GetFuncDef(funcName);
            if (scrFuncDef == null) 
                throw new Exception($"Error: Failed to find the function: {funcName}");
            return scrFuncDef.Execute(vars, scrCmd);
        }
        private ExprVar ExecuteDotCall(List<ExprNode> operands, string oprText)
        {

            ExprVar var1 = PrepareSubNode(operands[0], oprText);
            ExprNode op2 = operands[1];
            string funcName = op2.GetFuncName();
            List<ExprVar> vars = PrepareOperands(op2.GetOperands(), ".");
            vars.Insert(0, var1);
            string typeOfObj = var1.GetTypeOfObj();
            if (funcName.Length == 0)
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
        private List<ExprVar> PrepareOperands(List<ExprNode> operands, string oprText)
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
                var = ExecuteOperation(subNode);
            }
            else if (subNode.IsFunctionNode())
            {
                var = ExecuteFunction(subNode);
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
                var = scrCmd.GetVarFromMemory(varName);
                if (var == null) throw new Exception($"Error: variable {varName} is not defined");
            }
            else
            {
                string info = UtlParserHelper.Subs(scrCmd.GetBody(), 0, 100);
                throw new Exception($"Error: Unexpected type of operand for {oprText} operation:\n{info}...");
            }
            return var;
        }
        private ExprVar CreateArray(ExprNode subNode)
        {
            var vars = PrepareOperands(subNode.GetOperands(), "Create Array");
            return ExprVar.CrtVar(vars);
        }
        internal ExprVar Calculate(ExprOperatorDsc opr, List<ExprVar> args)
        {
            var oprCode = opr.m_Code;
            if (args.Count < 1)
            {
                throw new Exception("Internal Error: Calculate, too few operands");
            }

            ExprVar var = new ExprVar();
            if (oprCode == OperationCode.OPR_PLUS_UNAR)
            {
                var.Assign(args[0]);
            }
            else if (oprCode == OperationCode.OPR_MINUS_UNAR)
            {
                var.Assign(OprMinusUnar(args[0]));
            }
            else if (oprCode == OperationCode.OPR_NOT_UNAR)
            {
                var.SetVal(OprNot(args[0]));
            }
            else if (oprCode == OperationCode.OPR_RETURN)
            {
                var ret1 = ExprVar.CrtVar(ScrBlock.afxReturn);
                var ret2 = args[0];
                var arr = new List<ExprVar>() { ret1, ret2 };
                return ExprVar.CrtVar(arr);
            }
            if (opr.m_IsUnary) return var;

            if (args.Count < 2)
            {
                throw new Exception("Internal Error: Calculate, too few operands");
            }

            if (oprCode == OperationCode.OPR_PLUS)
            {
                var.Assign(OprPlus(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_MIN)
            {
                var.Assign(OprMinus(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_MULT)
            {
                var.Assign(OprMult(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_DEV)
            {
                var.Assign(OprDiv(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_ASSIGN)
            {
                var v1 = args[0];
                var v2 = args[1];
                v1.Assign(v2);
                var.Assign(v1);
            }
            else if (oprCode == OperationCode.OPR_EQ)
            {
                var.SetVal(OprIsEq(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_NE)
            {
                var.SetVal(OprIsNOTeq(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_GE)
            {
                var.SetVal(OprGE(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_LE)
            {
                var.SetVal(OprLE(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_GT)
            {
                var.SetVal(OprGT(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_LT)
            {
                var.SetVal(OprLT(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_AND)
            {
                var.SetVal(OprAND(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_OR)
            {
                var.SetVal(OprOR(args[0], args[1]));
            }
            else if (oprCode == OperationCode.OPR_BRACKETS)
            {
                return OprBrackets(args);
            }
            else if (oprCode == OperationCode.OPR_POINT)
            {
                return OprPoint(args[0], args[1]);
            }
            else
            {
                throw new Exception($"Internal Error: Failed to Calculate, unknown operation {oprCode}");
            }
            return var;
        }
        private ExprVar OprPoint(ExprVar exprVar1, ExprVar exprVar2)
        {
            //if (exprVar1.m_Type == EType.E_ARRAY)
            //{
            //    var vRes = ((ExprVarArray)exprVar1).GetProperty(exprVar2);
            //    return vRes;
            //}
            //else if (exprVar1.m_Type == EType.E_OBJECT)
            //{
            //}
            return exprVar1.GetProperty(exprVar2);
        }
        private ExprVar OprBrackets(List<ExprVar> args)
        {
            ExprVar exprVar1 = args[0];
            ExprVar exprVar2 = args[1];
            bool oprBeforeAssign = (args.Count <= 2) ? false : (args[2].ToInt() == 1);
            string className = Utl.GetClassName(exprVar1);
            switch (exprVar1.m_Type)
            {
                //case EType.E_ARRAY:
                //    ExprVarArray vRes = (className == "ExprVarArray") ? (ExprVarArray)exprVar1 : exprVar1.ToExprVarArray();
                //    return vRes.GetAt(exprVar2.ToInt());
                case EType.E_STRING:
                    string s1 = exprVar1.ToStr();
                    int idx = exprVar2.ToInt();
                    return ExprVar.CrtVar(UtlParserHelper.Subs(s1, idx, 1));
                case EType.E_OBJECT:
                    // todo if this is the case: dict[var] = val, 
                    // one should do: setProperty(var);
                    if (oprBeforeAssign && exprVar1.GetTypeOfObj() == "Dictionary")
                    {
                        var hasKey = exprVar1.GetObj().MethodCall("has", new List<ExprVar>() { exprVar2 });
                        if(hasKey.ToInt()==0)
                            exprVar1.GetObj().MethodCall("add", new List<ExprVar>() { exprVar2, ExprVar.CrtVar("") });
                    }
                    var vObj = exprVar1.GetObj().GetAt(exprVar2);
                    return vObj;
            }
            throw new Exception($"Error: attempt to get element using [] from {exprVar1.GetStringType()}-type variable");
        }

        private int OprOR(ExprVar compVar1, ExprVar compVar2)
        {
            return (compVar1.ToInt() != 0 || compVar2.ToInt() != 0) ? 1 : 0;
        }

        private int OprAND(ExprVar compVar1, ExprVar compVar2)
        {
            return (compVar1.ToInt() != 0 && compVar2.ToInt() != 0) ? 1 : 0;
        }

        private int OprLT(ExprVar compVar1, ExprVar compVar2)
        {
            int iRes = compVar1.Compare(compVar2);
            return iRes < 0 ? 1 : 0;
        }

        private int OprGT(ExprVar compVar1, ExprVar compVar2)
        {
            int iRes = compVar1.Compare(compVar2);
            return iRes > 0 ? 1 : 0;
        }

        private int OprLE(ExprVar compVar1, ExprVar compVar2)
        {
            int iRes = compVar1.Compare(compVar2);
            return iRes <= 0 ? 1 : 0;
        }

        private int OprGE(ExprVar compVar1, ExprVar compVar2)
        {
            int iRes = compVar1.Compare(compVar2);
            return iRes >= 0 ? 1 : 0;
        }
        private int OprIsNOTeq(ExprVar compVar1, ExprVar compVar2)
        {
            int iRes = compVar1.Compare(compVar2);
            return iRes != 0 ? 1 : 0;
        }
        private int OprIsEq(ExprVar compVar1, ExprVar compVar2)
        {
            int iRes = compVar1.Compare(compVar2);
            return iRes == 0 ? 1 : 0;
        }
        private ExprVar OprDiv(ExprVar compVar1, ExprVar compVar2)
        {
            var var1 = compVar1.Divide(compVar2);
            return var1;
        }
        private ExprVar OprMult(ExprVar compVar1, ExprVar compVar2)
        {
            var var1 = compVar1.Multiply(compVar2);
            return var1;
        }

        private ExprVar OprPlus(ExprVar compVar1, ExprVar compVar2)
        {
            var var1 = compVar1.Plus(compVar2);
            return var1;
        }
        private ExprVar OprMinus(ExprVar compVar1, ExprVar compVar2)
        {
            return compVar1.Minus(compVar2);
        }
        private int OprNot(ExprVar var)
        {
            int iRes = (var.ToInt() != 0) ? 0 : 1;
            return iRes;
        }
        private ExprVar OprMinusUnar(ExprVar var)
        {
            var var1 = new ExprVar("", var.m_Type, "", null);
            if (var.m_Type == EType.E_INT) var1.SetVal(-var1.ToInt());
            else if (var.m_Type == EType.E_DOUBLE) var1.SetVal(-var.ToDouble());
            else throw new Exception("Error: unexpected type of unary minus operation");
            return var1;
        }
    }
}