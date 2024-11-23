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
        OPR_NOT_UNAR,
        OPR_AND,
        OPR_OR,
        OPR_RETURN,
        OPR_UNDEF
    }

    public enum EType
    {
        E_UNDEF,
        E_INT,              // is used also as boolean 1 = true, 0 = false
        E_STRING,
        E_DOUBLE,
        E_DATE,
        E_ARRAY,
        E_OBJECT
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
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_NOT_UNAR, 5, "!", "( ", "( ", true, false));    //"NOT"
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
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_AND, 6, "&&", ") ", "( ", false, true)); //"AND"
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_OR, 7, "||", ") ", "( ", false, true));  // OR
            afxOperators.Add(new ExprOperatorDsc(OperationCode.OPR_RETURN, 8, "return", "", " (", true, true));  // return
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
        public int m_posAfterOperator;
		
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
                    bool bRes = ExprNode.FindClosingParenthesis(sBuf, iScanPos, ref m_posAfterOperator);
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
            m_posAfterOperator = iScanPos + iLen;

            if (! m_LooksLikeIdentifier && ! m_IsUnary)
			{
				return true;
			}
			if (! m_LooksLikeIdentifier) // but it is IsUnary
			{
				// check if is unary operation
				iPosOfNonEmptyBefore = ExprNode.FindNotEmptyBefore(sBuf, iScanPos);
				if (iPosOfNonEmptyBefore == -1)
					return true; // unary operator and nothing before it.
				ch = sBuf[iPosOfNonEmptyBefore];
				if (m_LeftDelimitersAllowed.IndexOf(ch) != - 1)
				{
					return true;
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
    }
}
