using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    public class ExprVarArray : ExprVar
    {
        public ExprVarArray() 
        {
            m_Type = EType.E_ARRAY;
        }
        public void CreateArray(List<ExprVar> vars)
        {
            m_obj = new ScrObj();
            m_obj.SetVal(vars);
        }
        public new ExprVar Length()
        {
            ExprVar v = new ExprVar();
            int iLen = ((List<ExprVar>)m_obj.GetVal()).Count;
            v.SetVal(iLen);
            return v;
        }
        public new ExprVar GetAt(int index)
        {
            int iLen = ((List<ExprVar>)m_obj.GetVal()).Count;
            int idx = index;
            if (index < 0)
            {
                idx = iLen + index;
            }
            if (idx >= iLen || idx<0)
                throw new Exception($"Error: getting element using [], index: {index}, array size: {iLen}");
            return ((List<ExprVar>)m_obj.GetVal())[idx];
        }
        public void SetAt(int index, ExprVar v)
        {
            ((List<ExprVar>)m_obj.GetVal())[index] = v;
        }
        public override string ToString()
        {
            int iLen = ((List<ExprVar>)m_obj.GetVal()).Count;
            return $"Name: {m_Name}, type: {GetStringType()}, length: {iLen}";
        }

        public override ExprVar GetProperty(ExprVar exprVar2)
        {
            string propName = exprVar2.ToStr();
            if (propName == "length") return Length();
            throw new Exception($"Error: undefined property {propName}, array variable: {this.ToString()}");
        }
    }
}
