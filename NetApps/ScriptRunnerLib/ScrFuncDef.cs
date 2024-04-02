using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    public class ScrFuncDef : ScrBlock
    {
        string funcName;
        string funcParams;

        private ScrFuncDef(ScriptRunner runOwner) : base(runOwner)
        {
            body = "";
            this.type = CmdType.FuncDef;
            this.runOwner = runOwner;
        }
        public ScrFuncDef(string name, string sParams,string body, ScriptRunner runOwner):
            this(runOwner)
        {
            SetFuncDev(name, sParams, body);
        }

        public void SetFuncDev(string name, string sParams, string body)
        {
            funcName = name;
            funcParams = TrimHeader(sParams);
            this.body = body.Trim();
        }
        public override void Compile()
        {
            ScriptRunner scriptRunner = new ScriptRunner(runOwner);
            try
            {
                scriptRunner.Parse(body);
                CompileBlockBody();
            }
            catch (Exception ex)
            {
                string info = UtlParserHelper.Subs(body, 0, 100);
                throw new Exception($"Error: while compiling body of function: {funcName} :\n"+
                    $"{info}...\n{ex.Message}");
            }
        }


        public override string ToString()
        {
            return $"{IdStr()}function {funcName} params: {funcParams}\nBody:\n{body}\n--- end of func: {funcName}";
        }
    }
}
