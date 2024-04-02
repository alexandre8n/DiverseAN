using System;
using System.Collections.Generic;

namespace ScriptRunnerLib
{
    public class ScrMemory
    {
        Dictionary<string, ExprVar>   vars = new Dictionary<string, ExprVar>();
        public ScrMemory()
        {

        }

        public bool AddVar(string name, EType tp, string val) 
        {
            if (vars.ContainsKey(name)) return false;

            vars[name] = new ExprVar(name, tp, val, this);
            return true;
        }

        public ExprVar GetVar(string name)
        {
            if (vars.ContainsKey(name))
            {
                var v1 = vars[name];
                v1.m_Name = name;
                return v1;
            }
            return null;
        }

        public bool UpdateVar(ExprVarArray varToUpdate)
        {
            if (!vars.ContainsKey(varToUpdate.m_Name)) return false;
            vars[varToUpdate.m_Name] = varToUpdate;
            return true;
        }
    }
}