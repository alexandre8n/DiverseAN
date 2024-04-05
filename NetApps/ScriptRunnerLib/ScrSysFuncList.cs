using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    static public class ScrSysFuncList
    {
        static Dictionary<string, Func<List<ExprVar>, ExprVar>> funcDict =
            new Dictionary<string, Func<List<ExprVar>, ExprVar>>()
            {
                {"min",  Min},
                {"max",  Max}
            };

        public static ExprVar Min(List<ExprVar> vars)
        {
            var vMin = vars[0];
            for (int i = 1; i < vars.Count; i++)
            {
                var vI = vars[i];
                if (vMin.Compare(vI) > 0)
                    vMin = vI;
            }
            return vMin;
        }
        public static ExprVar Max(List<ExprVar> vars)
        {
            var vMax = vars[0];
            for (int i = 1; i < vars.Count; i++)
            {
                var vI = vars[i];
                if (vMax.Compare(vI) < 0)
                    vMax = vI;
            }
            return vMax;
        }
        public static Func<List<ExprVar>, ExprVar> GetFunc(string name)
        {
            if (funcDict.ContainsKey(name)) return funcDict[name];
            return null;
        }
    }
}
