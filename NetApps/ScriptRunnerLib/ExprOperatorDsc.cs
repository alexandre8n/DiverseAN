using ExprParser1;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ScriptRunnerLib
{
    public enum OperationCode
    {
        OPR_POINT,
        OPR_BRACKETS,
        OPR_MULT,
        OPR_DEV,
        OPR_PLUS_UNAR,
        OPR_MINUS_UNAR,
        OPR_PLUS,
        OPR_MIN,
        OPR_CONCATAN,
        OPR_ASSIGN,
        OPR_EQ,
        OPR_NE,
        OPR_LIKE,
        OPR_GE,
        OPR_LE,
        OPR_GT,
        OPR_LT,
        OPR_NOT,
        OPR_AND,
        OPR_OR,
        OPR_UNDEF
    }

    public enum EType
    {
        E_UNDEF,
        E_INT,              // is used also as boolean 1 = true, 0 = false
        E_STRING,
        E_DOUBLE,
        E_DATE,
        E_ARRAY
    }

    public class ExprOperatorDsc
	{
        public static ArrayList afxOperators = new ArrayList();
		static ExprOperatorDsc()
		{
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_MULT, 1, "*", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_DEV, 1, "/", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_PLUS_UNAR, 2, "+", "(", "", true, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_MINUS_UNAR, 2, "-", "(", "", true, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_PLUS, 3, "+", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_MIN, 3, "-", "", "", false, false));
            //afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_CONCATAN, 3, "||", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_EQ, 4, "==", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_ASSIGN, 4, "=", "", "", false, false));
            //				afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_NE, 4, "<>", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_NE, 4, "!=", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_LIKE, 4, "LIKE", ") ", "( ", false, true));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_GE, 4, ">=", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_LE, 4, "<=", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_GT, 4, ">", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_LT, 4, "<", "", "", false, false));
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_NOT, 5, "!", "( ", "( ", false, true));    //"NOT"
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_AND, 6, "&&", ") ", "( ", false, true)); //"AND"
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_OR, 7, "||", ") ", "( ", false, true));  // OR
			afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_BRACKETS, 0, "[]", "", "", false, false, true));
			afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_POINT, 0, ".", "", "", false, false));
        }

        public OperationCode m_Code;
		public int m_LevelOfPrecedence;
		public string m_OperationText;
		public string m_LeftDelimitersAllowed;
		public string m_RightDelimitersAllowed;
		public bool m_IsUnary;
		public bool m_LooksLikeIdentifier;
		public bool m_IsBrackets;
        public int m_posOfClosingBracket;
		
		// methods
		public ExprOperatorDsc()
		{
            m_OperationText = "";
		}
		
		public ExprOperatorDsc(OperationCode Code, int LevelOfPrecedence, string OperationText, 
            string LeftDelimitersAllowed, string RightDelimitersAllowed, bool IsUnary, 
            bool LooksLikeIdentifier, bool brackets=false)
		{
			m_Code = Code;
			m_LevelOfPrecedence = LevelOfPrecedence;
			m_OperationText = OperationText;
			m_LeftDelimitersAllowed = LeftDelimitersAllowed;
			m_RightDelimitersAllowed = RightDelimitersAllowed;
			m_IsUnary = IsUnary;
			m_LooksLikeIdentifier = LooksLikeIdentifier;
            m_IsBrackets = brackets;

        }
		
		public bool IsFound(string sBuf, int iScanPos)
		{
			int iLen = m_OperationText.Length;
			if (iScanPos + iLen > sBuf.Length)
			{
				return false;
			}
            if (m_IsBrackets)
            {
                if (sBuf[iScanPos]== m_OperationText[0])
                {
                    bool bRes = ExprNode.FindClosingParenthesis(sBuf, iScanPos, ref m_posOfClosingBracket);
                    return bRes;
                }
            }

            int iPosOfNonEmptyBefore;
			char ch;
            string OpTextToTest = sBuf.Substring(iScanPos, iLen);
			if (string.Compare(m_OperationText, OpTextToTest, true) != 0)
			{
				return false;
			}
			if (! m_LooksLikeIdentifier && ! m_IsUnary)
			{
				return true;
			}
			else if (! m_LooksLikeIdentifier)
			{
				// check if is unary operation
				iPosOfNonEmptyBefore = ExprNode.FindNotEmptyBefore(sBuf, iScanPos);
				if (iPosOfNonEmptyBefore != - 1)
				{
					ch = sBuf[iPosOfNonEmptyBefore];
					if (m_LeftDelimitersAllowed.IndexOf(ch) != - 1)
					{
						return true;
					}
				}
			}
			//operator looks like identifier
			//next check inserted by LB
			if (iScanPos + iLen == sBuf.Length) //end of the expression
			{
                if(iScanPos == 0) 
                    return true;
				ch = sBuf[iScanPos - 1];
				if (ch == ' ' || m_LeftDelimitersAllowed.IndexOf(ch) != - 1)
				{
					return true;
				}
				return false;
			}
			//end of check
			
			// operator looks like identifier. check the characters before and after
			ch = sBuf[iScanPos + iLen];
			if (m_RightDelimitersAllowed.IndexOf(ch) == - 1)
			{
				return false;
			}
            if (iScanPos == 0)
                return true;
			ch = sBuf[iScanPos - 1];
			if (ch == ' ' || m_LeftDelimitersAllowed.IndexOf(ch) != - 1)
			{
				return true;
			}
			return false;
		}
		
		public int GetLength()
		{
			return m_OperationText.Length;
		}

        internal ExprVar Calculate(List<ExprVar> args)
        {
			if(args.Count<1)
			{
				throw new Exception("Internal Error: Calculate, too few operands");
			}

            ExprVar var = new ExprVar();
			if(m_Code == OperationCode.OPR_PLUS_UNAR)
			{
				var.Assign(args[0]);
			} else if(m_Code == OperationCode.OPR_MINUS_UNAR)
			{
				var.Assign(OprMinusUnar(args[0]));
            } else if(m_Code == OperationCode.OPR_NOT)
			{
				var.SetVal(OprNot(args[0]));
			}
			if(m_IsUnary) return var;

            if (args.Count < 2)
            {
                throw new Exception("Internal Error: Calculate, too few operands");
            }

            if (m_Code == OperationCode.OPR_PLUS)
            {
				var.Assign(OprPlus(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_MIN)
            {
                var.Assign(OprMinus(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_MULT)
            {
                var.Assign(OprMult(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_DEV)
            {
                var.Assign(OprDiv(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_ASSIGN)
            {
				var v1 = args[0];
                var v2 = args[1];
                if (v2.m_Type == EType.E_ARRAY && Utl.GetClassName(v1) != "ExprVarArray" &&
                    v1.m_MemoryScope!=null && v1.m_Name!="") 
                {
                    v1 = v1.ToExprVarArray();
                }
                v1.Assign(v2);
                var.Assign(v1);
            }
            else if (m_Code == OperationCode.OPR_EQ)
            {
                var.SetVal(OprIsEq(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_NE)
            {
                var.SetVal(OprIsNOTeq(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_GE)
            {
                var.SetVal(OprGE(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_LE)
            {
                var.SetVal(OprLE(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_GT)
            {
                var.SetVal(OprGT(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_LT)
            {
                var.SetVal(OprLT(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_AND)
            {
                var.SetVal(OprAND(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_OR)
            {
                var.SetVal(OprOR(args[0], args[1]));
            }
            else if (m_Code == OperationCode.OPR_BRACKETS)
            {
                return OprBrackets(args[0], args[1]);
            }
            else if (m_Code == OperationCode.OPR_POINT)
            {
                return OprPoint(args[0], args[1]);
            }
            else
            {
                throw new Exception($"Internal Error: Failed to Calculate, unknown operation {m_Code}");
            }
            return var;
        }

        private ExprVar OprPoint(ExprVar exprVar1, ExprVar exprVar2)
        {
            if (exprVar1.m_Type == EType.E_ARRAY)
            {
                var vRes = ((ExprVarArray)exprVar1).GetProperty(exprVar2);
                return vRes;
            }
            return exprVar1.GetProperty(exprVar2);
        }

        private ExprVar OprBrackets(ExprVar exprVar1, ExprVar exprVar2)
        {
            if (exprVar1.m_Type != EType.E_ARRAY)
                throw new Exception($"Error: attempt to get element using [] from {exprVar1.GetStringType()}-type variable");
            string className = Utl.GetClassName(exprVar1);
            ExprVarArray vRes = (className == "ExprVarArray") ? (ExprVarArray)exprVar1 : exprVar1.ToExprVarArray();
            return vRes.GetAt(exprVar2.ToInt());
        }

        private int OprOR(ExprVar compVar1, ExprVar compVar2)
        {
            return (compVar1.ToInt() != 0 || compVar2.ToInt() != 0) ? 1 : 0;
        }

        private int OprAND(ExprVar compVar1, ExprVar compVar2)
        {
            return (compVar1.ToInt() != 0 && compVar2.ToInt() != 0)? 1: 0;
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
            return iRes<=0 ? 1 : 0;
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
            int iRes = (var.m_intVal != 0) ? 0 : 1;
            return iRes;
        }
        private ExprVar OprMinusUnar(ExprVar var)
        {
            var var1 = new ExprVar("",var.m_Type, "", null);
            if (var.m_Type == EType.E_INT) var1.m_intVal = -var.m_intVal;
            else if (var.m_Type == EType.E_DOUBLE) var1.m_doubleVal = -var.m_doubleVal;
            else throw new Exception("Error: unexpected type of unary minus operation");
            return var1;
        }
    }
}
