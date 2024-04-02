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
            m_objVal = vars;
        }
        public ExprVar Length()
        {
            ExprVar v = new ExprVar();
            int iLen = ((List<ExprVar>)m_objVal).Count;
            v.SetVal(iLen);
            return v;
        }
        public ExprVar GetAt(int index)
        {
            int iLen = ((List<ExprVar>)m_objVal).Count;
            int idx = index;
            if (index < 0)
            {
                idx = iLen + index;
            }
            if (idx >= iLen || idx<0)
                throw new Exception($"Error: getting element using [], index: {index}, array size: {iLen}");
            return ((List<ExprVar>)m_objVal)[idx];
        }
        public void SetAt(int index, ExprVar v)
        {
            ((List<ExprVar>)m_objVal)[index] = v;
        }
        public override string ToString()
        {
            int iLen = ((List<ExprVar>)m_objVal).Count;
            return $"Name: {m_Name}, type: {GetStringType()}, length: {iLen}";
        }
    }
}
