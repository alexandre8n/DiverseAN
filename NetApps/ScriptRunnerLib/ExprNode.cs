using ExprParser1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptRunnerLib
{
    public enum NodeType
    {
        UNDEF,
        VARIABLE,
        PROPERTY,
        CONSTANT,
        OPERATOR,
        FUNCTION,
        ARRAY,
        BREAK,
		CONTINUE
    }

    public class DiagnosticMessages
	{
		public const string msgUnbQuatMark = "Unbalanced quatation marks";
		public const string msgUnbParenth = "Unbalanced paranthesis";
		public const string msgNoOpInExpr = "No operation found in expression [{0}]\nPossible reason missing operator delimiter(;)";
		public const string msgInvParLstInFunc = "Invalid parameter list for function [{0}]";
	}

    public class ExprNode
	{
		// types of node: operation, variable, const, function, array, object
		public static ArrayList afxOperators = null;
        public static List<string> afxMemDefOperators = new List<string>() { "let", "var" };
        public static string afxAssignOperator = "=";

        private string m_ExprBuff;
		private ExprNode parent = null;
        public NodeType nodeType { get; private set; }


		private bool m_AssignPropValue = false;		// true if: a.b=x, or a.b.c=x

        // Operator descriptor is for +,-,*,/,and,or,etc. is NULL for Functions
        private ExprOperatorDsc m_pOperDsc = null;

		// if the expression like "var a,b,c" or "let a=<expression>"
		private string m_MemDefinitionTerm = ""; // let or var or const
		private string m_MemoryDefinedVar = "";

        // m_Function is function name (only for functions),
        // is empty if this node is an operation or any other leaf 
        // is empty if m_Var is not empty
        private string m_Function = "";
		
		// m_Var is to save variable name or constant
		private ExprVar m_Var = new ExprVar();

        // is a DotFollower or property - if this node follows . (DOT) operator
        bool isDotFollower = false;				
		
		private List<ExprNode> m_Nodes = new List<ExprNode>();
		
		private ExprNode(ExprNode parent)
		{
			nodeType = NodeType.UNDEF;
			this.parent = parent;
        }
		
		public ExprNode(string sExpr, ExprNode parent) : this(parent)
		{
			SetExpression(sExpr);
		}
		
		// initialize expression parsing
		public void SetExpression(string sExpr)
		{
            afxOperators = ExprOperatorDsc.afxOperators; 
            ClearContent();
            m_ExprBuff = sExpr.Trim();
            ParseVarDefinitions();
        }

		public ExprNode GetParent() { return  parent; }
        private void ParseVarDefinitions()
        {
			m_MemDefinitionTerm = "";
			m_MemoryDefinedVar = "";

            // check: var var1 = something
            string strMemDef = string.Join("|", afxMemDefOperators);
            //afxAssignOperator

            string pureMemDef = $@"^({strMemDef})\s+([a-zA-Z_][\w_]*)($|\s*[=])";
            Match matchPure = Regex.Match(m_ExprBuff, pureMemDef);
			if (!matchPure.Success) return;
			
			m_MemDefinitionTerm = matchPure.Groups[1].Value;
            m_MemoryDefinedVar = matchPure.Groups[2].Value;
			int iLen = m_MemDefinitionTerm.Length;
			string sExpr = m_ExprBuff.Substring(iLen);
			m_ExprBuff = sExpr.Trim();
        }


        // Member for node building
        public void BuildExprTree()
		{
			if (m_ExprBuff.Length == 0) return;

			// open parenthesis if these are not needed, (something) -> something
			RemoveUnneededParenthesis();
			if (m_ExprBuff == "")
			{
				return;
			}
			if (CheckBreakContinue(m_ExprBuff))
			{
				return;
			}
			
			// check if parenthesis and quatation mars are balanced
			int rc = ParenthQuatMarkBalance(m_ExprBuff);
			if (rc == - 1)
			{
				Exception ex = new Exception(DiagnosticMessages.msgUnbQuatMark);
				throw (ex);
			}
			else if (rc == 1)
			{
				Exception ex = new Exception(DiagnosticMessages.msgUnbParenth);
				throw (ex);
			}
			
			if (IsName(m_ExprBuff))
			{
				m_Var.m_Name = m_ExprBuff;
				nodeType = (isDotFollower) ? NodeType.PROPERTY : NodeType.VARIABLE;
                return;
			}
			if (IsFormattedStringLiteral(m_ExprBuff))
			{
				// checked the following: $"smth{expr1}..."
				// transform to $("smth{1}...", expr1,...)
				m_ExprBuff = TransformFormattedStrLiteral(m_ExprBuff);
				// the next step is to process like a function.
            }
			if (CheckConstant(ref m_ExprBuff))
			{
                nodeType = NodeType.CONSTANT;
                return;
			}
			if(CheckArray())
			{
                nodeType = NodeType.ARRAY;
                return;
			}
			if (IsFunctionToParce())
			{
				PrcFunction();
                nodeType = NodeType.FUNCTION;
                return;
			}
			
			// the case we have some expression
			ExprNode pNode;
			string Operand1 = "";
			string Operand2 = "";
			
			if (! SplitOn2Operands(ref Operand1, ref Operand2))
			{
				string s = string.Format(DiagnosticMessages.msgNoOpInExpr, m_ExprBuff, null);
				throw (new Exception(s));
			}
			
			if (Operand1 != "")
			{
				pNode = new ExprNode(Operand1, this);
				m_Nodes.Add(pNode);
				pNode.BuildExprTree();
			}
			
			if (Operand2 != "")
			{
				pNode = new ExprNode(Operand2, this);
				if(m_pOperDsc.m_Code == OperationCode.OPR_POINT)
				{
					pNode.isDotFollower = true;
                }
				m_Nodes.Add(pNode);
				pNode.BuildExprTree();
			}
			nodeType = NodeType.OPERATOR;
			if(m_pOperDsc.m_Code == OperationCode.OPR_ASSIGN)
			{
                ((ExprNode)m_Nodes[0]).CheckPropertyAssign();
            }
		}

        private string TransformFormattedStrLiteral(string input)
        {
			input = input.Substring(1);
            // str1 looks like: "....{exp1}....{exp2}..."
            string ptrn = "{[^{}]+}";
            var mts = Regex.Matches(input, ptrn);
			if(mts.Count==0)
				return input;
            int i = 1;
			var lstOfMatches = new List<string>();
            foreach (var m2 in mts)
            {
                var mtch = m2 as Match;
                string s1 = (mtch == null) ? "" : mtch.Value;
				lstOfMatches.Add(s1.Substring(1,s1.Length-2));
                input = input.Replace(s1, $"{{{i++}}}");
            }
			string str2 = string.Join(",",lstOfMatches);
			string sRes = $"$({input},{str2})";
			return sRes;
        }

        private bool IsFormattedStringLiteral(string m_ExprBuff)
        {
			if (m_ExprBuff.Length < 3) return false;
			string pureVal="";
			if (m_ExprBuff[0] == '$' && IsTextConst(m_ExprBuff.Substring(1), ref pureVal))
				return true;
			return false;
        }

        private void CheckPropertyAssign()
        {
			if (m_pOperDsc==null || m_pOperDsc.m_Code != OperationCode.OPR_POINT 
				|| m_Nodes.Count < 2) return;
            var propNode = (ExprNode)m_Nodes[1];
            if (propNode.IsProperty())
            {
				propNode.SetAssignPropertyFlag();
            }
        }

        private bool CheckBreakContinue(string m_ExprBuff)
        {
			if (m_ExprBuff == "break")
			{
				nodeType = NodeType.BREAK;
				return true;
			}
            if (m_ExprBuff == "continue")
            {
                nodeType = NodeType.CONTINUE;
                return true;
            }
			return false;
        }

        private bool CheckArray()
        {
			if (m_ExprBuff[0] != '[' || m_ExprBuff[m_ExprBuff.Length - 1] != ']') return false;
			// should be [a,b,...]
			PrcInsideBrackets(1, "[]");
			return true;
        }

        private void RemoveUnneededParenthesis()
        {
			if (m_ExprBuff.Substring(0, 1) != "(") return;
            int iEnd = 0;
            if (FindClosingParenthesis(m_ExprBuff, 0, ref iEnd) && iEnd == m_ExprBuff.Length - 1)
            {
                m_ExprBuff = m_ExprBuff.Substring(1, iEnd - 1).Trim();
            }
        }

        public static int iEntry = 0;

        public void InitDump()
		{
			iEntry = 0;
		}
		
		public bool IsConstNode()
		{
            // m_Function == "" && m_pOperDsc == null && GetVarName() == "";
            return nodeType == NodeType.CONSTANT;

        }

        public ExprVar GetVar() 
		{ 
			return m_Var; 
		}

        public string GetVarName()
		{
			return m_Var.m_Name;
		}
		
		public string GetOperationText()
		{
			if (m_Function != "")
			{
				return m_Function;
			}
			if (m_pOperDsc == null)
			{
				return "";
			}
			return m_pOperDsc.m_OperationText;
		}
		
		public string GetNodeType()
		{
			if (GetOperationText() == ".") return "Dot";
			if (IsVarNode()) return "Variable";
			if (IsProperty()) return "Property";
			if (IsFunctionNode()) return "Function";
			if (IsConstNode()) return "Const:" + m_Var.GetStringType();
            if (IsArray()) return "Array";
            return "Expression";
		}
		
		public bool IsVarNode()
		{
            //m_Function == "" && m_pOperDsc == null && GetVarName() != "";
            return nodeType == NodeType.VARIABLE;
        }
        public bool IsProperty()
        {
            return nodeType == NodeType.PROPERTY;
        }
        public bool IsFunctionNode()
		{
			return m_Function != "";
		}
		public bool IsOperationNode()
		{
			return nodeType == NodeType.OPERATOR;
        }
        public bool IsArray()
        {
            return nodeType == NodeType.ARRAY;
        }
		public bool IsBreakOrContinue()
		{
			return nodeType == NodeType.BREAK || nodeType == NodeType.CONTINUE;
        }

        public static bool FindSafeNextOperation(string sBuf, int iBeginScan, FindOprRes findOprRes)
		{
			int ip = iBeginScan;
			int iLen = sBuf.Length;
			while (ip < iLen)
			{
				char ch = sBuf[ip];
				// is quatation mark
				if ("\'\"".IndexOf(ch) != - 1)
				{
					// find closing quatation mark
					ip = sBuf.IndexOf(ch, ip + 1);
					
					if (ip == - 1)
					{
						return false;
					}
				}
				else if (ch == '(')
				{
					int iEnd = 0;
					if (! FindClosingParenthesis(sBuf, ip, ref iEnd))
					{
						return false;
					}
					ip = iEnd;
				}
                if (IsOperation(sBuf, ip, findOprRes))
				{
					goto endOfDoLoop;
				}
				ip++;
			}
			endOfDoLoop:

			if (ip == iLen)
			{
				return false;
			}
			return true;
		}
		
		public static bool IsOperation(string sBuf, int iBeginScan, FindOprRes findOprRes)
		{
			
			if (sBuf[iBeginScan] == ' ')
			{
				return false;
			}
			
			foreach (ExprOperatorDsc curOper in ExprNode.afxOperators)
			{
                findOprRes.opr = curOper;
				if (curOper.IsFound(sBuf, iBeginScan))
				{
                    findOprRes.oprBeginPos = iBeginScan;
                    findOprRes.oprEndPos = curOper.m_posAfterOperator;
                    return true;
				}
			}
			return false;
		}
		
		public static bool FindClosingParenthesis(string sBuf, int iBeg, ref int iEnd)
		{
			//FindClosingParenthesis - scan string beginning from iBeg,
			//till eol or finding corresponding parenthesis.
			//	in case of success
			//	iEnd - contains the offset of corresponding closing Parenthesis ) or ] or }.
			//     Returns TRUE if corresponding ")" is found
			//     remark: this function works in safe mode, e.g. it process
			//     correctly the case of meetimg quatation marks (' and ")
			
			int iPos = iBeg;
			int iFinish = sBuf.Length - 1;

			string startingParnths = sBuf.Substring(iPos, 1);

            if (!IsOpnParanthesis(startingParnths[0]))
			{
				return false;
			}
			string currentPrn = startingParnths;
            string expectedClosingPrn = GetClosingParenthesis(startingParnths);
            Stack prnStack = new Stack();
			prnStack.Push(expectedClosingPrn);
            int iLev = 1;
			
			for (iPos = iPos + 1; iPos <= iFinish; iPos++)
			{
				char s = sBuf[iPos];
				if (s == '\'' || s == '\"')
				{
					int iFnd = sBuf.IndexOf(s, iPos + 1);
					if (iFnd == - 1)
					{
						return false;
					}
					iPos = iFnd;
				}
				else if (IsOpnParanthesis(s))
				{
					expectedClosingPrn = GetClosingParenthesis(s.ToString());
					prnStack.Push(expectedClosingPrn);
                    iLev++;
				}
				else if (IsClsParanthesis(s))
				{
					string expectedCls = (string)prnStack.Pop();
					if(s.ToString() != expectedCls)
					{
						return false;
					}
					iLev--;
					if (iLev == 0)
					{
						break;
					}
				}
			}
			iEnd = iPos;
			return iLev == 0;
		}

        private static string GetClosingParenthesis(string startingParnths)
        {
			if (startingParnths == "(") return ")";
			if (startingParnths == "[") return "]";
			if (startingParnths == "{") return "}";
			return "";
        }

        public static bool IsName(string sVal)
		{
			int len = sVal.Length;
			if (! IsLetter(sVal[0]) || len > 64 || len == 0)
			{
				return false;
			}
			int i;
			for (i = 1; i <= len - 1; i++)
			{
				char s = sVal[i];
				if (! IsLetter(s) && ! IsDigit(s))
				{
					return false;
				}
			}
			return true;
		}
		
		public static bool IsLetter(char sVal)
		{
			if (sVal >= 'A'&& sVal <= 'Z')
			{
				return true;
			}
			if (sVal >= 'a'&& sVal <= 'z')
			{
				return true;
			}
			if (sVal == '_'|| sVal == '@'|| sVal == '#' || sVal=='$')
			{
				return true;
			}
			return false;
		}
		
		public static bool IsDigit(char sVal)
		{
			if (sVal >= '0'&& sVal <= '9')
			{
				return true;
			}
			return false;
		}
		
		public static int ParenthQuatMarkBalance(string sBuf)
		{
			//This function returns
			//0 - Ok,
			//-1 - unbalanced quatation mark,
			//1 - unbalanced parenthesis
			
			int iLev = 0;
			int iPos;
			int iFinish = sBuf.Length - 1;
			for (iPos = 0; iPos <= iFinish; iPos++)
			{
				char s = sBuf[iPos];
				if (s == '\'' || s == '\"')
				{
					int iFnd = sBuf.IndexOf(s, iPos + 1);
					if (iFnd == - 1)
					{
						return - 1;
					}
					iPos = iFnd;
				}
				else if (s == '(')
				{
					iLev++;
				}
				else if (s == ')')
				{
					iLev--;
				}
				else if (iLev < 0)
				{
					return 1;
				}
			}
			if (iLev > 0)
			{
				return 1;
			}
			return 0;
		}
		
		public static int FindNotEmptyBefore(string sBuf, int iStartPos)
		{
			int i = iStartPos - 1;
			while (i > 0 && sBuf[i] == ' ')
			{
				i--;
			}
			if (i <= 0)
			{
				return - 1;
			}
			return i;
		}
		
		public static int FindSafeOneOf(string sBuf, int iStart, string sDelims)
		{
			// FindSafeOneOf - scan string pStrIni and find one of delimiters pDelims
			//	Scan till end
			//	'Safe' means that quatation marks and parenthesis (),[],{} are
			//	taken into account and their content will be ignored.
			//   Returns -1 if not found or offset of some of spcified delimiters
			//   from the beginning of pStrIni
			int ip = iStart;
			int iLen = sBuf.Length;
			while (ip < iLen)
			{
				char ch = sBuf[ip];
				
				if (sDelims.IndexOf(ch) != - 1)
				{
					goto endOfDoLoop;
				}
				
				if ("\'\"".IndexOf(ch) != - 1)
				{
					ip = sBuf.IndexOf(ch, ip + 1);
					
					if (ip == - 1)
					{
						return - 1;
					}
				}
				//else if (ch == '(')
				else if (IsOpnParanthesis(ch))
				{
					int iEnd = 0;
					if (! FindClosingParenthesis(sBuf, ip, ref iEnd))
					{
						return - 1;
					}
					ip = iEnd;
				}
				ip++;
			}
            endOfDoLoop:
			
			if (ip == iLen)
			{
				return - 1;
			}
			
			return ip;
			
		}

        private static bool IsOpnParanthesis(char ch)
        {
			if (ch == '(' || ch == '[' || ch == '{') return true;
			return false;
        }
        private static bool IsClsParanthesis(char ch)
        {
            if (ch == ')' || ch == ']' || ch == '}') return true;
            return false;
        }

        // private:
        private void ClearContent()
		{
			m_ExprBuff = "";
			m_pOperDsc = null;
			m_Function = "";
			m_Var.Clear();
			if (m_Nodes != null)
    			{
				m_Nodes.Clear();
				m_Nodes.Capacity = 0;
			}
		}
		
		private bool CheckConstant(ref string sVal)
		{
			string sPureVal = "";
			if (IsInteger(sVal))
			{
				m_Var.SetVal(int.Parse(sVal));
			}
			else if (IsDouble(sVal))
			{
				m_Var.SetVal(Utl.StrToDouble(sVal));
				if (m_Var.ToDouble() == double.MinValue)
				{
					throw new Exception("Error: incorrect format of the number value: {sVal}");
				}
			}
			else if (IsDateTimeConst(sVal, ref sPureVal))
			{
				m_Var.SetVal(sPureVal);
				m_Var.m_Type = EType.E_DATE;
			}
			else if (IsTextConst(sVal, ref sPureVal))
			{
                m_Var.SetVal(sPureVal);
			}
			else
			{
				return false;
			}
			return true;
		}
		
		public static bool IsInteger(string sVal)
		{
			try
			{
				if(!Regex.IsMatch(sVal, "^[-+]?\\d+$"))
				{
					return false;
				}
				int i = int.Parse(sVal);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
		
		public static bool IsDouble(string sVal)
		{
			try
			{
				if ("+-0123456789".IndexOf(sVal[0]) == - 1)
				{
					return false;
				}
                double v = ExprParser1.Utl.StrToDouble(sVal);
                if (v == double.MinValue)
                    return false;
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
		
		public static bool IsDateTimeConst(string sVal, ref string sPureVal)
		{
			object temp_object = sPureVal;
			string temp_string =  (string) temp_object;
			if (!IsTextConst(sVal, ref temp_string) || !Regex.IsMatch(sVal, @"^\d\d\d\d-\d\d-\d\d"))
			{
				return false;
			}
			try
			{
				CultureInfo en = new CultureInfo("en-US");
				string[] sFormat = new string[] { "yyyy-MM-dd", "yyyy-MM-dd hh:mm:ss" };
				DateTime MyDateTime = DateTime.ParseExact(sPureVal, sFormat, en.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
		
		public static bool IsTextConst(string sValInput, ref string sPureVal)
		{
            string sVal = sValInput.Trim();
            if (sVal.Length < 2)
            {
                return false;
            }

            string sChar = sVal[0].ToString();
			
            string sLastChar = sVal[sVal.Length - 1].ToString();
			if (sChar != sLastChar) 
				return false;
			sPureVal = sVal.Substring(1,sVal.Length - 2);
			// case: "somethingWoQM"
			int idxQM = sPureVal.IndexOf(sChar);

            if (idxQM == -1) 
				return true;

            //case: "abc""...""abc"
            // now replace "" by empty
            string sPur1Test = sPureVal.Replace(sChar + sChar, "");
			if (sPur1Test.IndexOf(sChar) != -1)
				return false;
            sPureVal = sPureVal.Replace(sChar + sChar, sChar);
            return true;
        }

        private bool IsFunctionToParce()
		{
			int i = m_ExprBuff.IndexOf("(");
			int iEnd = 0;
			
			if (i < 0)
			{
				return false;
			}
			if (! FindClosingParenthesis(m_ExprBuff, i, ref iEnd) || m_ExprBuff.Length > iEnd + 1 || ! IsName(m_ExprBuff.Substring(0, i)))
			{
				return false;
			}
			return true;
		}
		
		private void PrcFunction()
		{
			int iBeg = m_ExprBuff.IndexOf("(");
			m_Function = m_ExprBuff.Substring(0, iBeg).Trim();
			int ip = iBeg + 1;
			PrcInsideBrackets(ip, "()");
		}

        private void PrcInsideBrackets(int ip, string brackets)
        {
			char opn = brackets[0];
			char cls = brackets[1];

            ExprNode pNode;
            while (m_ExprBuff[ip] != cls)
            {
                int iComma = FindSafeOneOf(m_ExprBuff, ip, ","+cls);
                if (iComma == -1)
                {
                    string s = string.Format(DiagnosticMessages.msgInvParLstInFunc, m_Function, null);
                    Exception ex = new Exception(s);
                    throw (ex);
                }
                string parExpr = m_ExprBuff.Substring(ip, iComma - ip);
                if (parExpr.Trim() == "")
                {
                    // check situation f() as ok, and f(a,), f(a,,b) as error
					// or [] as ok, and [a,,] as error
                    char ch = m_ExprBuff[iComma];
                    if (ch == ',' || (ch == cls && m_Nodes.Count > 0))
                    {
                        string s = string.Format(DiagnosticMessages.msgInvParLstInFunc, m_Function, null);
                        Exception ex = new Exception(s);
                        throw (ex);
                    }
                    goto endOfDoLoop;
                }
                pNode = new ExprNode(parExpr, this);
                m_Nodes.Add(pNode);
                ip = iComma;
                if (m_ExprBuff[ip] == ',')
                {
                    ip++;
                }
            }
            endOfDoLoop:
            foreach (ExprNode tempLoopVar_pNode in m_Nodes)
            {
                pNode = tempLoopVar_pNode;
                pNode.BuildExprTree();
            }
        }

        private bool SplitOn2Operands(ref string sOperand1, ref string sOperand2)
		{
            ExprOperatorDsc pMaxLevOper = null;
			//int iMaxOpLeng = 0;
			int iMaxOpEndPos = 0;		// postion, where the matching operator ends
			int iMaxLevPosFound = -1;
			int iMaxOpLevel = - 1;
			int iBeginScan = 0;
			//int iPosFound = - 1;
			
			int iLen = m_ExprBuff.Length;
			while (true)
			{
				FindOprRes findOprRes = new FindOprRes();
                if (! FindSafeNextOperation(m_ExprBuff, iBeginScan, findOprRes))
				{
					goto endOfDoLoop;
				}
				
				// it is importent to find the most right operator !!
				
				if (findOprRes.opr.m_LevelOfPrecedence >= iMaxOpLevel)
				{
					iMaxOpLevel = findOprRes.opr.m_LevelOfPrecedence;
					pMaxLevOper = findOprRes.opr;
                    iMaxOpEndPos = findOprRes.oprEndPos;
                    iMaxLevPosFound = findOprRes.oprBeginPos;
				}
				iBeginScan = findOprRes.oprEndPos;
			}
            endOfDoLoop:

            if (pMaxLevOper != null)
            {
                sOperand1 = m_ExprBuff.Substring(0, iMaxLevPosFound);
				if (pMaxLevOper.m_IsBrackets)
				{
					int iOp2Len = pMaxLevOper.m_posAfterOperator - iMaxLevPosFound - 1;
                    sOperand2 = m_ExprBuff.Substring(iMaxLevPosFound+1, iOp2Len);
                }
				else
				{
					sOperand2 = m_ExprBuff.Substring(iMaxOpEndPos);
				}
                m_pOperDsc = pMaxLevOper;
                return true;
            }
            string s = string.Format(DiagnosticMessages.msgNoOpInExpr, m_ExprBuff, null);
            throw (new Exception(s));
		}
			
		public string DumpToString(bool bTruncateDots)
		{
			string OffsInChar = new string(' ', iEntry * 3 + 1);
			string DumpBuff = "";
			
			// bMayGoDeeper is used only to avoid going deeper if the node is of type dot.
			bool bMayGoDeeper = true;
			
			iEntry++;
			if (iEntry == 1)
			{
				DumpBuff = "Begin: Expression parsing dump" + "\r\n";
			}
			DumpBuff += OffsInChar + "- Node: [" + m_ExprBuff + "] (type:" + GetNodeType() + ")" + "\r\n";
			if (m_Nodes.Count > 0)
			{
				if (bTruncateDots && GetOperationText() == ".")
				{
					bMayGoDeeper = false;
				}
				if (bMayGoDeeper)
				{
					DumpBuff += OffsInChar + "  Operator : [" + GetOperationText() + "] has subnodes: " + m_Nodes.Count.ToString() + "\r\n";
				}
			}
			if (bMayGoDeeper)
			{
				ExprNode pNode;
				foreach (ExprNode tempLoopVar_pNode in m_Nodes)
				{
					pNode = tempLoopVar_pNode;
					DumpBuff += OffsInChar + "  >" + pNode.m_ExprBuff + "\r\n";
				}
				foreach (ExprNode tempLoopVar_pNode in m_Nodes)
				{
					pNode = tempLoopVar_pNode;
					DumpBuff += pNode.DumpToString(bTruncateDots);
				}
			}
			iEntry--;
			if (iEntry == 0)
			{
				DumpBuff += "End: Expression parsing dump" + "\r\n";
			}
			return DumpBuff;
		}

        internal string MemDefinedVarName()
        {
			return m_MemoryDefinedVar;
        }

        internal ExprOperatorDsc GetOperationDescriptor()
        {
			return m_pOperDsc;
        }

        internal List<ExprNode> GetOperands()
        {
			return m_Nodes;
        }

        internal string GetFuncName()
        {
            return m_Function;
        }

        internal void SetAssignPropertyFlag()
        {
			if (IsProperty())
			{
				m_AssignPropValue = true;
			}
        }
		public bool IsPropertyToBeAssigned()
		{
			return m_AssignPropValue;
		}

        public bool IsAssignOperatorNode()
        {
            return IsOperationNode() && m_pOperDsc.m_Code==OperationCode.OPR_ASSIGN;
        }
    }
	public class FindOprRes
	{
		public int oprBeginPos = -1;
		public int oprEndPos = -1;
		public ExprOperatorDsc opr = null;
	}
}
