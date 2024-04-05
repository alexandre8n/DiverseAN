using ExprParser1;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ScriptRunnerLib
{
    public class ExprVar
	{
		public string m_Name = "";
		public EType m_Type = EType.E_UNDEF;
		public string m_Value = "";
		public int m_intVal = 0;
		public double m_doubleVal = 0;
		public Object m_objVal = null;

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
            else m_objVal = var.m_objVal;
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
			if (m_Type != EType.E_INT) return m_intVal;
			if (m_Type != EType.E_STRING) return double.Parse(m_Value);
			throw new Exception("Error: Failed to convert to double the variable of unexpected type");
        }

        public void SetVal(string v)
		{
			m_Type = EType.E_STRING;
			m_Value = v;
			m_intVal = 0;
			m_doubleVal = 0;
			m_objVal = null;
		}
        public void SetVal(int v)
        {
            m_Type = EType.E_INT;
            m_Value = "";
            m_intVal = v;
            m_doubleVal = 0;
            m_objVal = null;
        }
        public void SetVal(double v)
        {
            m_Type = EType.E_DOUBLE;
            m_Value = "";
            m_intVal = 0;
            m_doubleVal = v;
            m_objVal = null;
        }

    public ExprVar Plus(ExprVar v1)
    {
        ExprVar v2 = new ExprVar();
        if (m_Type == EType.E_STRING || v1.m_Type == EType.E_STRING)
            v2.SetVal(ToString() + v1.ToString());
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
        public string AsString()
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
            string val = AsString();
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

        internal ExprVarArray ToExprVarArray()
        {
            ExprVarArray v = new ExprVarArray();
            v.Assign(this);
            v.m_Name = m_Name;
            v.m_MemoryScope = m_MemoryScope;
            if (m_MemoryScope != null)
            {
                if(!m_MemoryScope.UpdateVar(v))
                {
                    // strange var name is empty?
                    throw new Exception($"ToExprVarArray: Internal Error to check: var name:[{m_Name}] is not found in mem-scope");
                }
            }
            return v;
        }

        public virtual ExprVar GetProperty(ExprVar exprVar2)
        {
            ExprVar v = new ExprVar();
            string propName = exprVar2.AsString();
            if (m_Type == EType.E_STRING && propName == "length") 
            {
                int iLen = m_Value.Length;
                v.SetVal(iLen);
            }
            else throw new Exception($"Error: undefined property {propName}, variable: {this.ToString()}");
            return v;
        }

    }
}
