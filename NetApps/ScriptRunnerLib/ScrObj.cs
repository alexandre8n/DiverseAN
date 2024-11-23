using System;
using System.Collections;
using System.Collections.Generic;

namespace ScriptRunnerLib
{
    public class ScrObj
    {
        static Dictionary<string, string> SupportedTypes = new Dictionary<string, string>()
        {       // supportedName -> NormalizedName
                {"Dictionary",  "Dictionary"},
                {"Dictionary`2",  "Dictionary"},
                {"List",  "List"},
                {"List`1",  "List"},
                {"FileMover",  "FileMover"},
                {"ExcelPackage",  "ExcelPackage"},
        };
        string typeName = "UnDef";
        object val = null;
        public ScrObj() { }
        public ScrObj(string typeName) 
        { 
            this.TypeName = typeName;
        }
        public ScrObj(string typeName, object obj)
        {
            this.TypeName = typeName;
            this.val = obj;
        }
        public void SetType(string typeName)
        {
            this.TypeName = typeName;
        }

        public void SetVal(object obj)
        {
            this.val = obj;
        }
        public object GetVal() {  return this.val; }

        public string TypeName { get => typeName; private set => typeName = NormalizeType(value); }

        private string NormalizeType(string tpName)
        {
            if (!SupportedTypes.ContainsKey(tpName))
            {
                throw new Exception($"Error: Object type: {tpName} is not supported");
            }
            return SupportedTypes[tpName];
        }

        public ExprVar GetPropertyValue(string propName)
        {
            var func = ScrSysFuncList.GetFunc($"{TypeName}.getProperty");
            if (func == null) 
                throw new Exception($"Failed to find GetProperty function by the object {typeName}");
            return func(new List<ExprVar>() { ExprVar.CrtVar(propName) });
        }

        public ExprVar GetAt(ExprVar varKey)    // brackets [] - operation
        {
            var func = ScrSysFuncList.GetFunc($"{TypeName}.getAt");
            if (func == null)
                throw new Exception($"Failed to find getAt function for [] operation. Object {typeName}");
            var vObj = new ExprVar();
            vObj.SetVal(this);
            return func(new List<ExprVar>() { vObj, varKey });
        }

        public ExprVar MethodCall(string funcName, List<ExprVar> args) 
        {
            var func = ScrSysFuncList.GetFunc($"{TypeName}.{funcName}");
            if (func == null)
                throw new Exception($"Failed to find {funcName} function. Object {typeName}");
            var vObj = new ExprVar();
            vObj.SetVal(this);
            var lstParams = new List<ExprVar>();
            lstParams.Add(vObj);
            lstParams.AddRange(args);
            return func(lstParams);
        }
    }
}