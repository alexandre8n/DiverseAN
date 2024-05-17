using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptRunnerLib
{
    static public class ScrSysFuncList
    {
        static Dictionary<string, Func<List<ExprVar>, ExprVar>> funcDict =
            new Dictionary<string, Func<List<ExprVar>, ExprVar>>()
            {
                {"out",  ToConsole},
                {"min",  Min},
                {"max",  Max},
                {"renameFiles", RenameFiles },
                {"listFiles", getListOfFiles },
                {"createObject", CreateObj},
                {"Array.create", null}, 
                {"Array.get_length", null}, // this way get_<propName> one can get/set properties
                {"Array.add", null},
                {"Array.deleteAt",  null},
                {"Array.indexOf",  null},
                {"List.create", ListCreate}, 
                {"List.toStr",  ListToStr},
                {"List.getProperty", ListGetProperty}, // this way get_<propName> one can get/set properties
                {"List.getAt", ListGetAt},
                {"List.add", ListAdd},
                {"List.deleteAt",  ListDeleteAt},
                {"List.indexOf",  ListIndexOf},
                {"Dictionary.create",  DictionaryCreate},
                {"Dictionary.get_length",  DictionaryLength},
                {"Dictionary.has",  DictionaryHas},      // contains key,bool
                {"Dictionary.add",  DictionaryAdd},
                {"Dictionary.setProperty",  DictionarySetAt},
                {"Dictionary.getProperty",  DictionaryGetAt},
                {"Dictionary.getAt",  DictionaryGetAt},
                {"Dictionary.deleteAt",  DictionaryDeleteAt},
                {"Dictionary.getKeysAsArr",  DictionaryGetKeysAsArr},
                {"Dictionary.getValuesAsArr",  DictionaryGetValuesAsArr},
                {"Dictionary.toStr",  DictionaryToStr},
                {"FileMover.create",  FileMoverCreate},
                {"FileMover.run",  FileMoverRun},
                {"Integer.toStr",  ToStr},
                {"Double.toStr",  ToStr},
                {"String.toStr",  ToStr},
                {"String.left",  StringLeft},
                {"String.indexOf",  StringIndexOf},
                {"String.substr",  StringSubstr},
                {"openExcelFile", OpenExcelFile},
                {"ExcelPackage.close", CloseExcelFile},
                {"ExcelPackage.save", SaveExcelFile},
                {"ExcelPackage.findRowNumber", FindRowNumberInExcelFile},
                {"ExcelPackage.setCellValue", SetCellInExcelFile},
                {"ExcelPackage.getCellValue", GetCellInExcelFile},
            };

        private static ExprVar ToStr(List<ExprVar> list)
        {
            var par1 = list[0];
            if (par1.m_Type == EType.E_STRING) return ExprVar.CrtVar(par1.ToStr());
            if (par1.m_Type == EType.E_INT) return ExprVar.CrtVar(par1.ToStr());
            if (par1.m_Type == EType.E_DOUBLE) return ExprVar.CrtVar(par1.ToStr());
            return ExprVar.CrtVar(par1.ToString());
        }

        private static ExprVar ListGetAt(List<ExprVar> list)
        {
            if (list.Count < 2 || list[0].GetTypeOfObj() != "List")
                throw new Exception("ListGetAt: incorrect parameter(s), usage: list.getAt(index) or list[index]");
            var listVar = list[0];
            var idx = list[1].ToInt();
            var lstObj = (List<ExprVar>)listVar.GetObj().GetVal();
            return lstObj[idx];
        }

        private static ExprVar ListToStr(List<ExprVar> list)
        {
            if (list.Count < 1 || list[0].GetTypeOfObj() != "List")
                throw new Exception("ListToStr: incorrect parameter, expected: List");
            var listVar = list[0];
            var lstObj = (List<ExprVar>)listVar.GetObj().GetVal();
            List<string> lines = lstObj.Select(x => x.ToStr()).ToList();
            string str = "[" + string.Join(", ", lines) + "]";
            return ExprVar.CrtVar(str);
        }

        private static ExprVar ListIndexOf(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        private static ExprVar ListDeleteAt(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        private static ExprVar ListAdd(List<ExprVar> list)
        {
            if (list.Count < 2 || list[0].GetTypeOfObj() != "List")
                throw new Exception("ListAdd: incorrect parameter(s), usage: list.add(var)");
            var listVar = list[0];
            var vToAdd = list[1];
            var lstObj = (List<ExprVar>)listVar.GetObj().GetVal();
            lstObj.Add(vToAdd);
            return listVar;
        }

        private static ExprVar ListGetProperty(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        private static ExprVar ListCreate(List<ExprVar> list)
        {
            // param: var, in this var one should create an array(List)
            // gets a var, sets to this var the FilesMoverTask Obj;
            ExprVar par1 = list[0];
            ScrObj obj = par1.GetObj();
            obj.SetVal(new List<ExprVar>());
            return par1;
        }

        private static ExprVar getListOfFiles(List<ExprVar> list)
        {
            // params: folder, patternToFilter
            // returns: files (array)
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of getListOfFiles, shoud be 2 string paramenters,\n"
                    + "usage: listFiles(strFolder, strPattern)");
            string sFolder = list[0].ToStr();
            string sPtrn = list[1].ToStr();
            List<string> files = utls.GetListFiles(sFolder, sPtrn);
            List<ExprVar> vars = files.Select(x=>ExprVar.CrtVar(x)).ToList();
            return ExprVar.CrtVar(vars);
        }

        private static ExprVar RenameFiles(List<ExprVar> list)
        {
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of RenameFiles, shoud be 2 string paramenters,\n"
                    +"usage: renameFiles(strFolder, strRenamePattern)");
            string sFolder = list[0].ToStr();
            sFolder = UtlParserHelper.Right(sFolder, 1) == ";" ? UtlParserHelper.SubsRng(sFolder,0, -1): sFolder;
            string sPtrn = list[1].ToStr();
            sPtrn = sPtrn.EndsWith(";")? UtlParserHelper.SubsRng(sPtrn, 0, -1):sPtrn;
            
            // renameFiles returns: int n = number of files renamed, string msg= message
            var res1 = utls.RenameFiles(sFolder, sPtrn);

            var dictRes = new Dictionary<string, ExprVar>();
            dictRes["filesCount"] = ExprVar.CrtVar(res1.Item1);
            dictRes["message"] = ExprVar.CrtVar(res1.Item2);
            return ExprVar.CrtVar(dictRes);
        }

        private static ExprVar FileMoverCreate(List<ExprVar> list)
        {
            // gets a var, sets to this var the FilesMoverTask Obj;
            ExprVar par1 = list[0];
            ScrObj obj = par1.GetObj();
            obj.SetVal(new FilesMoverTask());
            return par1;
        }

        private static ExprVar FileMoverRun(List<ExprVar> list)
        {
            if (list.Count < 2) 
                throw new Exception("FileMoverRun: incorrect parameters, expected: paramsDictionary");
            var fmVar = list[0];
            var dictVar = list[1];
            var fm = (FilesMoverTask)fmVar.GetObj().GetVal();
            var dictTmp = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            var dict = dictTmp.ToDictionary(k => k.Key, k => k.Value.ToStr());
            fm.LoadTaskFromDict(dict);
            fm.MoveCopyFiles();
            string resMsg = fm.ResultingMessage;
            return fmVar;
        }

        public static ExprVar ToConsole(List<ExprVar> list)
        {
            if (list.Count < 1) throw new Exception("out or ToConsole: incorrect parameters, expected: stringToOutput");
            string str = list[0].ToStr();
            // to allow \n
            str = str.Replace("\\n", "\n");
            Console.WriteLine(str);
            return ExprVar.CrtVar(str);
        }

        private static ExprVar GetCellInExcelFile(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        public static ExprVar SetCellInExcelFile(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        public static ExprVar FindRowNumberInExcelFile(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        public static ExprVar SaveExcelFile(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        public static ExprVar CloseExcelFile(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        public static ExprVar OpenExcelFile(List<ExprVar> list)
        {
            var v1 = list[0];
            string filePath = v1.ToStr();
            var xlsFile = UtlXls.OpnXlsFile(filePath);
            var vXls = ExprVar.CrtVar(xlsFile);
            return vXls;
        }

        private static ExprVar StringSubstr(List<ExprVar> list)
        {
            if (list.Count < 2) throw new Exception("StringSubstr: incorrect parameters, expected: idxFirstChar[, CountMax]");
            string str = list[0].ToStr();
            int idxStart = list[1].ToInt();
            int len = (list.Count == 2) ? list[2].ToInt() : str.Length;
            string res = UtlParserHelper.Subs(str, idxStart, len);
            return ExprVar.CrtVar(res);
        }

        private static ExprVar StringIndexOf(List<ExprVar> list)
        {
            string str = list[0].ToStr();
            string strToFind = list[1].ToStr();
            int idx = str.IndexOf(strToFind);
            return ExprVar.CrtVar(idx);
        }

        private static ExprVar StringLeft(List<ExprVar> list)
        {
            string str = list[0].ToStr();
            int len = list[1].ToInt();
            var val = UtlParserHelper.Subs(str, 0, 2);
            var vRes = new ExprVar("", EType.E_STRING, val, null);
            return vRes;
        }

        private static ExprVar DictionaryToStr(List<ExprVar> list)
        {
            if (list.Count < 1 || list[0].GetTypeOfObj() != "Dictionary")
                throw new Exception("DictionaryToStr: incorrect parameter, expected: Dictionary");
            var dictVar = list[0];
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            List<string> lines = dict.Select(kv => kv.Key + "=" + kv.Value.ToStr()).ToList();
            string str = "{" + string.Join(",",lines) + "}";
            return ExprVar.CrtVar(str);
        }
        private static ExprVar DictionaryGetValuesAsArr(List<ExprVar> list)
        {
            var dictVar = list[0];
            var arr = new List<ExprVar>();
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            foreach ( var v in dict.Values)
            {
                arr.Add(v);
            }
            ExprVarArray arrArr = new ExprVarArray();
            arrArr.CreateArray(arr);
            return arrArr;
        }

        private static ExprVar DictionaryGetKeysAsArr(List<ExprVar> list)
        {
            var dictVar = list[0];
            var arr = new List<ExprVar>();
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            foreach (var key in dict.Keys)
            {
                arr.Add(ExprVar.CrtVar(key));
            }
            ExprVarArray arrArr = new ExprVarArray();
            arrArr.CreateArray(arr);
            return arrArr;
        }

        private static ExprVar DictionaryDeleteAt(List<ExprVar> list)
        {
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of Dictionary.DeleteAt, shoud be at least 2 parameters");
            var dictVar = list[0];
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            string sKey = list[1].ToStr();
            bool bRes = dict.Remove(sKey);
            return ExprVar.CrtVar(bRes?1:0);
        }

        private static ExprVar DictionaryGetAt(List<ExprVar> list)
        {
            var dictVar = list[0];
            var propName = list[1].ToStr();
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            return dict[propName];
        }

        private static ExprVar DictionarySetAt(List<ExprVar> list)
        {
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of Dictionary.SetProperty, shoud be at least 2 parameters");
            var dictVar = list[0];
            var propName = list[1].ToStr();
            var varToAssign = (list.Count < 3) ? ExprVar.CrtVar(0) : list[2];
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            dict[propName] = ExprVar.CrtVar(0);
            return dict[propName];
        }

        private static ExprVar DictionaryHas(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        private static ExprVar DictionaryLength(List<ExprVar> list)
        {
            throw new NotImplementedException();
        }

        private static ExprVar DictionaryAdd(List<ExprVar> list)
        {
            ExprVar dictVar = list[0];
            ExprVar keyVar = list[1];
            ExprVar valVar = list[2];
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            dict[keyVar.ToStr()] = valVar;
            return valVar;
        }

        private static ExprVar DictionaryCreate(List<ExprVar> list)
        {
            // gets a var, sets to this var the Dictionary Obj;
            ExprVar par1 = list[0];
            ScrObj obj = par1.GetObj();
            obj.SetVal(new Dictionary<string, ExprVar>());
            return par1;
        }

        private static ExprVar CreateObj(List<ExprVar> list)
        {
            if(list.Count != 1)
                throw new Exception("Error: incorrect call of CreateObj, shoud be one string parameter, the typeName");
            ExprVar par1 = list[0];
            string typeName = par1.ToStr();
            ExprVar obj = new ExprVar();
            obj.SetVal(new ScrObj(typeName));
            string key = $"{typeName}.create";
            if (funcDict.ContainsKey(key))
            {
                List<ExprVar> crtParams = new List<ExprVar>() { obj };
                return funcDict[key](crtParams);
            }
            return obj;
        }

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
