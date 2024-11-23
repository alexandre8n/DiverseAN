using ScriptRunnerLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    public class Utl
    {
        static public double StrToDouble(string dblValStr)
        {
            double value;
            dblValStr = dblValStr.Trim();
            bool bParsedOk = double.TryParse(dblValStr, NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture("en-US"), out value); //CultureInfo.InvariantCulture
            if (!bParsedOk)
            {
                return double.MinValue;
            }
            return value;
        }
        static public string DblToStrDotN(double dbl, int afterDot)
        {
            string dotHolders = new String('#', afterDot);
            string str = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:0." + dotHolders + "}", dbl);
            return str;
        }
        static public string DtToStr(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        static public string DblToStr(double dbl)
        {
            string str = dbl.ToString(CultureInfo.InvariantCulture.NumberFormat);
            return str;
        }

        static public DateTime StrToDt(string dtVal)
        {
            DateTime value;
            dtVal = dtVal.Trim();
            bool bParsedOk = DateTime.TryParse(dtVal, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
            if (!bParsedOk)
            {
                return DateTime.MinValue;
            }
            return value;
        }
        static public Dictionary<string, string> StrToDict(string inpStr)
        {
            // delimiters: Dict:{key->val,key->val,....}
            if (inpStr.StartsWith("Dict:")){
                inpStr = inpStr.Substring(5);
            }
            if (inpStr.StartsWith("{"))
            {
                inpStr = inpStr.Substring(1);
                if (inpStr.EndsWith("}")) inpStr = inpStr.Substring(0,inpStr.Length - 1);
            }
            var dict = new Dictionary<string, string>();
            if (inpStr.Length == 0) return dict;
            var arrPairs = inpStr.Split(',');
            foreach ( var pair in arrPairs )
            {
                var arrKeyVal = pair.Split(new string[] {"->" }, StringSplitOptions.None);
                if (arrKeyVal.Length < 2) 
                    throw new Exception($"Error: incorrect dictionary specified: {inpStr}");
                dict[arrKeyVal[0]]= arrKeyVal[1];
            }
            return dict;
        }
        static public string GetClassName(Object obj)
        {
            string className = obj.GetType().ToString();
            int idx = className.LastIndexOf('.');
            if (idx == -1) return className;
            return className.Substring(idx+1);
        }
    }
}
