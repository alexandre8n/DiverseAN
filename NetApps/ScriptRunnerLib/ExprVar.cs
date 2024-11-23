using ExprParser1;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace ScriptRunnerLib
{
    public class ExprVar
	{
		public string m_Name = "";
		public EType m_Type = EType.E_UNDEF;
		string m_Value = "";
		int m_intVal = 0;
		double m_doubleVal = 0;
		protected ScrObj m_obj = null;

        public ScrMemory m_MemoryScope = null;
        public ExprVar() { }
        public ExprVar(string name, EType varType, string value, ScrMemory scrMemory)
		{
			m_Name = name;
			m_Type = varType;
			m_Value = value;
            m_MemoryScope = scrMemory;

            if (varType == EType.E_INT) m_intVal=int.Parse(value);
			if(varType==EType.E_DOUBLE) m_doubleVal=Utl.StrToDouble(value);
		}
		public void Clear()
		{
			m_Name = "";
			m_Type = EType.E_UNDEF;
			m_Value = "";
		}
		public string GetStringType()
		{
			if (m_Type == EType.E_STRING)
			{
				return "String";
			}
			if (m_Type == EType.E_DATE)
			{
				return "DateTime";
			}
			if (m_Type == EType.E_DOUBLE)
			{
				return "Double";
			}
			if (m_Type == EType.E_INT)
			{
				return "Integer";
			}
            if (m_Type == EType.E_ARRAY)
            {
                return "Array";
            }
            return "Undef";
		}
		public ExprVar Clone()
		{
			var v1 = new ExprVar();
			v1.Assign(this);
			return v1;
		}

        public void Assign(ExprVar var)
        {
			m_Type = var.m_Type;
			if(m_Type == EType.E_STRING) m_Value = var.m_Value;
            else if (m_Type == EType.E_INT) m_intVal = var.m_intVal;
			else if(m_Type==EType.E_DOUBLE) m_doubleVal = var.m_doubleVal;
            else m_obj = var.m_obj;
        }


        public int ToInt()
        {
            if (m_Type == EType.E_DOUBLE) return (int)m_doubleVal;
            if (m_Type == EType.E_INT) return m_intVal;
            if (m_Type == EType.E_STRING && m_Value!="") return int.Parse(m_Value);
            throw new Exception("Error: Failed to convert to int the variable of unexpected type");
        }

        public double ToDouble()
        {
            if(m_Type == EType.E_DOUBLE) return m_doubleVal;
			if (m_Type == EType.E_INT) return m_intVal;
			if (m_Type == EType.E_STRING) return double.Parse(m_Value);
			throw new Exception("Error: Failed to convert to double the variable of unexpected type");
        }
        public object ToObj()
        {
            if (m_Type == EType.E_DOUBLE) return m_doubleVal;
            if (m_Type == EType.E_INT) return m_intVal;
            if (m_Type == EType.E_STRING) return m_Value;
            if (m_Type == EType.E_OBJECT || m_Type==EType.E_ARRAY) 
                return m_obj.GetVal();
            throw new Exception("Error: Failed to convert to object the variable of unexpected type");
        }

        static public ExprVar CrtVar(object obj)
        {
            var v = new ExprVar();
            if (obj == null) return v;
            string sName = obj.GetType().Name;
            if (sName == "Double") v.SetVal((double)obj);
            else if (sName == "Int32") v.SetVal((int)obj);
            else if (sName == "String") v.SetVal((string)obj);
            else
            {
                ScrObj scrObj = new ScrObj(sName, obj);
                v.SetVal(scrObj);
            }
            return v;
        }

        public void SetVal(string v)
		{
			m_Type = EType.E_STRING;
			m_Value = v;
			m_intVal = 0;
			m_doubleVal = 0;
			m_obj = null;
		}
        public void SetVal(int v)
        {
            m_Type = EType.E_INT;
            m_Value = "";
            m_intVal = v;
            m_doubleVal = 0;
            m_obj = null;
        }
        public void SetVal(double v)
        {
            m_Type = EType.E_DOUBLE;
            m_Value = "";
            m_intVal = 0;
            m_doubleVal = v;
            m_obj = null;
        }
        public void SetVal(ScrObj obj)
        {
            m_Type = EType.E_OBJECT;
            m_Value = "";
            m_intVal = 0;
            m_doubleVal = 0;
            m_obj = obj;
        }

        public ExprVar Plus(ExprVar v1)
    {
        ExprVar v2 = new ExprVar();
        if (m_Type == EType.E_STRING || v1.m_Type == EType.E_STRING)
            v2.SetVal(ToStr() + v1.ToStr());
        else if (m_Type == EType.E_DOUBLE || v1.m_Type == EType.E_DOUBLE)
            v2.SetVal(ToDouble() + v1.ToDouble());
        else v2.SetVal(ToInt() + v1.ToInt());
        return v2;
    }

    public ExprVar Minus(ExprVar v1)
        {
            ExprVar v2 = new ExprVar();
			if (m_Type == EType.E_STRING || v1.m_Type == EType.E_STRING)
				throw new Exception("Error: failed operation - for strings");
			else if (m_Type == EType.E_DOUBLE || v1.m_Type == EType.E_DOUBLE)
				v2.SetVal(ToDouble() - v1.ToDouble());
			else v2.SetVal(ToInt() - v1.ToInt());
            return v2;
        }
        public ExprVar Multiply(ExprVar v1)
        {
            ExprVar v2 = new ExprVar();
            if (m_Type == EType.E_STRING || v1.m_Type == EType.E_STRING)
                throw new Exception("Error: failed operation * for strings");
            else if (m_Type == EType.E_DOUBLE || v1.m_Type == EType.E_DOUBLE)
                v2.SetVal(ToDouble() * v1.ToDouble());
            else v2.SetVal(ToInt() * v1.ToInt());
            return v2;
        }
        public ExprVar Divide(ExprVar v1)
        {
            ExprVar v2 = new ExprVar();
            if (m_Type == EType.E_STRING || v1.m_Type == EType.E_STRING)
                throw new Exception("Error: failed operation / for strings");
            else if (m_Type == EType.E_DOUBLE || v1.m_Type == EType.E_DOUBLE)
                v2.SetVal(ToDouble() / v1.ToDouble());
            else v2.SetVal(ToInt() / v1.ToInt());
            return v2;
        }
        public string ToStr()
        {
            string val = m_Value;
            switch (m_Type)
            {
                case EType.E_DOUBLE:
                    val = $"{m_doubleVal}";
                    break;
                case EType.E_INT:
                    val = $"{m_intVal}";
                    break;
                case EType.E_UNDEF:
                    val = $"Undef.Obj.";
                    break;
            }
            return val;
        }
        public override string ToString()
        {
            string nm = (string.IsNullOrEmpty(m_Name)) ? "NoN" : m_Name;
            string val = ToStr();
            return $"Name: {nm}, type: {GetStringType()}, val: {val}";
        }

        public int Compare(ExprVar v1)
        {
            // returns: 0 ==, 1 this>v1, -1 this<v1;
            if (m_Type == EType.E_STRING || v1.m_Type == EType.E_STRING)
                return m_Value.CompareTo(v1.m_Value);
            else if (m_Type != v1.m_Type)
            {
                return this.ToDouble().CompareTo(v1.ToDouble());
            }
            else if (m_Type == EType.E_INT)
            {
                return m_intVal.CompareTo(v1.m_intVal);
            }
            throw new Exception("Error: Failed to compare, the variable of unexpected types");
        }

        //internal ExprVarArray ToExprVarArray()
        //{
        //    ExprVarArray v = new ExprVarArray();
        //    v.Assign(this);
        //    v.m_Name = m_Name;
        //    v.m_MemoryScope = m_MemoryScope;
        //    if (m_MemoryScope != null)
        //    {
        //        if(!m_MemoryScope.UpdateVar(v))
        //        {
        //            // strange var name is empty?
        //            throw new Exception($"ToExprVarArray: Internal Error to check: var name:[{m_Name}] is not found in mem-scope");
        //        }
        //    }
        //    return v;
        //}

        public virtual ExprVar GetProperty(ExprVar exprVar2)
        {
            ExprVar v = new ExprVar();
            string propName = exprVar2.ToStr();
            if (m_Type == EType.E_STRING && propName == "length") 
            {
                int iLen = m_Value.Length;
                v.SetVal(iLen);
            }
            else if(m_Type == EType.E_OBJECT)
            {
                throw new Exception("Error: GetProperty, not expected case of EType.E_OBJECT");
            }
            else throw new Exception($"Error: undefined property {propName}, variable: {this.ToString()}");
            return v;
        }

        internal string GetTypeOfObj()
        {
            if(m_Type == EType.E_OBJECT && m_obj!=null)
            {
                return m_obj.TypeName;
            }
            return GetStringType();
        }

        internal ScrObj GetObj()
        {
            return m_obj;
        }
        internal object GetObjBody()
        {
            return (m_obj==null)? null : m_obj.GetVal();
        }

        internal ExprVar GetAt(int idx)
        {
            if (m_Type == EType.E_STRING)
                return ExprVar.CrtVar(UtlParserHelper.Subs(m_Value, idx, 1));
            if(m_Type == EType.E_OBJECT && m_obj.TypeName == "List")
            {
                var lstObj = (List<ExprVar>)GetObj().GetVal();
                if (idx >= lstObj.Count) 
                    throw new Exception($"Out of range for the list. idx={idx}, length={lstObj.Count}");
                return lstObj[idx];
            }
            if (m_Type == EType.E_OBJECT && m_obj.TypeName == "Dictionary")
            {
                var dict = (Dictionary<string, ExprVar>)GetObj().GetVal();
                List<string> keyList = new List<string>(dict.Keys);
                if (idx >= keyList.Count)
                    throw new Exception($"Out of range for the dictionary. idx={idx}, length={keyList.Count}");
                return ExprVar.CrtVar(keyList[idx]);
            }
            throw new Exception($"ExprVar:GetAt(idx): Unsupported Object Type: {GetTypeOfObj()}");
        }

        internal int Length()
        {
            if (m_Type == EType.E_STRING)
                return m_Value.Length;
            if (m_Type == EType.E_OBJECT && m_obj.TypeName == "List")
            {
                var lstObj = (List<ExprVar>)GetObj().GetVal();
                return lstObj.Count;
            }
            if (m_Type == EType.E_OBJECT && m_obj.TypeName == "Dictionary")
            {
                var dict = (Dictionary<string, ExprVar>)GetObj().GetVal();
                List<string> keyList = new List<string>(dict.Keys);
                return keyList.Count;
            }
            return -1;
        }
    }
}
