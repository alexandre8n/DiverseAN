using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace ScriptRunnerLib
{
    static public class ScrSysFuncList
    {
        // specific names: createObject, setProperty, getProperty, getAt, 
        static Dictionary<string, Func<List<ExprVar>, ExprVar>> funcDict =
            new Dictionary<string, Func<List<ExprVar>, ExprVar>>()
            {
                {"$",  StrFormatting},
                {"out",  ToConsole},
                {"min",  Min},
                {"max",  Max},
                {"renameFiles", RenameFiles },
                {"listFiles", getListOfFiles },
                {"deleteFile", DeleteFileByPath},
                {"createObject", CreateObj},
                {"List.create", ListCreate}, 
                {"List.toStr",  ListToStr},
                {"List.getProperty", ListGetProperty}, // this way get_<propName> one can get/set properties
                {"List.getAt", ListGetAt},
                {"List.add", ListAdd},
                {"List.addRange", ListAddRange},
                {"List.deleteAt",  ListDeleteAt},
                {"List.indexOf",  ListIndexOf},
                {"Dictionary.create",  DictionaryCreate},
                {"Dictionary.length",  DictionaryLength},
                {"Dictionary.has",  DictionaryHas},      // contains key,bool
                {"Dictionary.add",  DictionaryAdd},
                {"Dictionary.setProperty",  DictionarySetAt},
                {"Dictionary.getProperty",  DictionaryGetAt},
                {"Dictionary.getAt",  DictionaryGetAt},
                {"Dictionary.deleteAt",  DictionaryDeleteAt},
                {"Dictionary.getKeys",  DictionaryGetKeysAsArr},
                {"Dictionary.getValues",  DictionaryGetValuesAsArr},
                {"Dictionary.toStr",  DictionaryToStr},
                {"FileMover.create",  FileMoverCreate},
                {"FileMover.copy",  FileMoverCopy},
                {"FileMover.clear",  FileMoverClear},
                {"FileMover.setProperty",  FileMoverSetAt},
                {"FileMover.getProperty",  FileMoverGetAt},
                {"FileMover.renameFiles",  FileMoverRenameFiles},
                {"FileMover.initialFilesList",  FileMoverInitialFilesList},
                {"FileMover.renamedFilesList",  FileMoverRenamedFilesList},
                {"Integer.toStr",  ToStr},
                {"Double.toStr",  ToStr},
                {"String.toStr",  ToStr},
                {"String.left",  StringLeft},
                {"String.indexOf",  StringIndexOf},
                {"String.substr",  StringSubstr},
                {"String.fileName",  StringFileName},
                {"openExcelFile", OpenExcelFile},
                {"ExcelPackage.close", CloseExcelFile},
                {"ExcelPackage.save", SaveExcelFile},
                {"ExcelPackage.findRowNumber", FindRowNumberInExcelFile},
                {"ExcelPackage.setCellValue", SetCellInExcelFile},
                {"ExcelPackage.getCellValue", GetCellInExcelFile},
            };

        private static ExprVar DeleteFileByPath(List<ExprVar> list)
        {
            if (list.Count < 1)
                throw new Exception("Error: incorrect call of DeleteFile, shoud be 1 parameter,\n"
                    + "usage: deleteFile(filePath)");
            string sPath = list[0].ToStr();
            if (File.Exists(sPath))
            {
                try
                {
                    File.Delete(sPath);
                    return ExprVar.CrtVar(1);
                }
                catch
                {
                    return ExprVar.CrtVar(0);
                }
            }
            return ExprVar.CrtVar(0);
        }

        private static ExprVar FileMoverRenamedFilesList(List<ExprVar> list)
        {
            if (list.Count < 1)
                throw new Exception("Error: incorrect call of RenamedFilesList, shoud be 1 parameter,\n"
                    + "usage: fileMover.renamedFilesList()");
            var fmObj = (FileMover)list[0].GetObj().GetVal();
            var lst = fmObj.GetListOfRenamedFiles();
            return lst;
        }

        private static ExprVar FileMoverInitialFilesList(List<ExprVar> list)
        {
            if (list.Count < 1)
                throw new Exception("Error: incorrect call of initialFilesList, shoud be 1 parameter,\n"
                    + "usage: fileMover.initialFilesList()");
            var fmObj = (FileMover)list[0].GetObj().GetVal();
            var lst = fmObj.GetListOfInitialFilesToRename();
            return lst;
        }

        private static ExprVar FileMoverRenameFiles(List<ExprVar> list)
        {
            if (list.Count < 3)
                throw new Exception("Error: incorrect call of RenameFiles, shoud be 2 string paramenters,\n"
                    + "usage: fileMover.renameFiles(strFolder, strRenamePattern)");
            // rename pattern: srcPtrn -> targetPtrn
            var fmObj = (FileMover)list[0].GetObj().GetVal();
            string sFolder = list[1].ToStr();
            string sPtrn = list[2].ToStr();
            sPtrn = sPtrn.EndsWith(";") ? UtlParserHelper.SubsRng(sPtrn, 0, -1) : sPtrn;

            // renameFiles returns: true - Ok, false - failed
            var res1 = fmObj.RenameFiles(sFolder, sPtrn);

            return ExprVar.CrtVar(res1?1:0);
        }

        private static ExprVar StrFormatting(List<ExprVar> list)
        {
            // $(" ... {1} ...{2} ...", v1, v2, ... )
            var str = list[0].ToStr();
            for(int i=1; i< list.Count; i++)
            {
                var v1 = list[i].ToStr();
                str = str.Replace($"{{{i}}}", v1);
            }
            return ExprVar.CrtVar(str);
        }

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
        private static ExprVar ListAddRange(List<ExprVar> list)
        {
            if (list.Count < 2 || list[0].GetTypeOfObj() != "List")
                throw new Exception("ListAdd: incorrect parameter(s), usage: list.addRange(list)");
            var listVar = list[0];
            var lstObj = (List<ExprVar>)listVar.GetObjBody();
            var lstToAdd = list[1];
            var lstToAddObj = (List<ExprVar>)lstToAdd.GetObjBody();
            lstObj.AddRange(lstToAddObj);
            return listVar;
        }

        private static ExprVar ListGetProperty(List<ExprVar> list)
        {
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of getProperty, 2 paramenters are expected,\n"
                    + "usage: list.<propertyName>");
            var listVar = (List<ExprVar>)(list[0].GetObjBody());
            var propName = list[1].ToStr();
            if(propName=="length") return ExprVar.CrtVar(listVar.Count);
            throw new Exception($"Error: List.GetProperty(propName), undefined property: {propName}");
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
                    + "usage: listFiles(strFolder, strPattern); or listFiles(ListOfFolders, strPattern)");
            var par1 = list[0];
            string sPtrn = list[1].ToStr();
            string strType = par1.GetTypeOfObj();
            var folders = new List<string>();
            if (strType == "String")
            {
                folders.Add(par1.ToStr());
            }
            else if(strType == "List")
            {
                folders = ((List<ExprVar>)par1.GetObjBody()).Select(x => x.ToStr()).ToList();
            }
            List<string> files= new List<string>();
            foreach (var sFolder in folders)
            {
                var filesToAdd = utls.GetListFiles(sFolder, sPtrn, false);
                files.AddRange(filesToAdd);
            }
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
            var renDict = new Dictionary<string, string>();
            if (sPtrn.Contains(";")) // it means that we have also renaming dictionary
            {
                var arr2 = sPtrn.Split(';');
                sPtrn = arr2[0];
                renDict = Utl.StrToDict(arr2[1]);

            }
            var res1 = utls.RenameFiles(sFolder, sPtrn, renDict);

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
            obj.SetVal(new FileMover());
            return par1;
        }
        private static ExprVar FileMoverGetAt(List<ExprVar> list)
        {
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of FileMover.GetProperty, shoud be at least 2 parameters");
            var fmVar = list[0];
            var propName = list[1].ToStr();
            var fmObj = (FileMover)fmVar.GetObj().GetVal();
            if (propName == "FromFolders")
            {
                return fmObj.FromFolders;
            }
            else if (propName == "FromFilePatterns")
            {
                return fmObj.FromFilePatterns;
            }
            else if (propName == "ToFolders")
            {
                return fmObj.ToFolders;
            }
            else if (propName == "ResultingMessage")
            {
                return fmObj.ResultingMessage;
            }
            else if (propName == "ListOfCopiedFiles")
            {
                return fmObj.ListOfCopiedFiles;
            }
            else if (propName == "CopyOption")
            {
                return fmObj.CopyOption;
            }
            else if (propName == "RenameOption")
            {
                return fmObj.RenameOption;
            }

            throw new Exception($"Error in FileMover.GetProperty: incorrect property name: {propName}");
        }

        private static ExprVar FileMoverSetAt(List<ExprVar> list)
        {
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of FileMover.SetProperty, shoud be at least 2 parameters");
            var fmVar = list[0];
            var propName = list[1].ToStr();
            var fmObj = (FileMover)fmVar.GetObj().GetVal();
            if (propName == "FromFolders")
            {
                var varToAssign = (list.Count < 3) ? ExprVar.CrtVar("") : list[2];
                fmObj.FromFolders = varToAssign;
                return varToAssign;
            }
            else if (propName == "FromFilePatterns")
            {
                var varToAssign = (list.Count < 3) ? ExprVar.CrtVar("") : list[2];
                fmObj.FromFilePatterns = varToAssign;
                return varToAssign;
            }
            else if (propName == "ToFolders")
            {
                var varToAssign = (list.Count < 3) ? ExprVar.CrtVar("") : list[2];
                fmObj.ToFolders = varToAssign;
                return varToAssign;
            }
            else if (propName == "RenameDictionary")
            {
                var varToAssign = (list.Count < 3) ? ExprVar.CrtVar("") : list[2];
                fmObj.RenameDict = varToAssign;
                return varToAssign;
            }
            else if(propName == "CopyOption")
            {
                var varToAssign = (list.Count < 3) ? ExprVar.CrtVar("") : list[2];
                fmObj.CopyOption = varToAssign;
                return varToAssign;
            }
            else if (propName == "RenameOption")
            {
                var varToAssign = (list.Count < 3) ? ExprVar.CrtVar("") : list[2];
                fmObj.RenameOption = varToAssign;
                return varToAssign;
            }

            throw new Exception($"Error in FileMover.SetProperty: incorrect property name: {propName}");
        }

        private static ExprVar FileMoverClear(List<ExprVar> list)
        {
            var fmVar = list[0];
            var fm = (FileMover)fmVar.GetObj().GetVal();
            fm.Clear();
            return ExprVar.CrtVar(1);

        }

        private static ExprVar FileMoverCopy(List<ExprVar> list)
        {
            if (list.Count < 1) 
                throw new Exception("FileMoverCopy: incorrect parameters, expected: fileMover");
            var fmVar = list[0];
            var fm = (FileMover)fmVar.GetObj().GetVal();
            var bRes = fm.MoveCopyFiles();
            return ExprVar.CrtVar(bRes?1:0);
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

        public static ExprVar CloseExcelFile(List<ExprVar> list)
        {
            var xlsPkg = (ExcelPackage)list[0].GetObjBody();
            UtlXls.ClsXlsFile(xlsPkg);
            return ExprVar.CrtVar(1);
        }

        public static ExprVar OpenExcelFile(List<ExprVar> list)
        {
            var v1 = list[0];
            string filePath = v1.ToStr();
            var xlsFile = UtlXls.OpnXlsFile(filePath);
            var vXls = ExprVar.CrtVar(xlsFile);
            return vXls;
        }
        private static ExprVar GetCellInExcelFile(List<ExprVar> list)
        {
            //GetCellVal(ExcelPackage package, int workSheetId, int row, int col)
            if (list.Count < 4)
                throw new Exception("Error: incorrect call of getCellValue,\n"
                    + "usage: xlsPkgObj.getCellValue(wrkSheetNumber, row, col)");
            var xlsPkg = (ExcelPackage)list[0].GetObjBody();
            var wksheetNo = list[1].ToInt();
            var row = list[2].ToInt();
            var col = list[3].ToInt();
            var vRes = UtlXls.GetCellVal(xlsPkg, wksheetNo, row, col);
            return vRes;
        }

        public static ExprVar SetCellInExcelFile(List<ExprVar> list)
        {
            if (list.Count < 5)
                throw new Exception("Error: incorrect call of setCellValue,\n"
                    + "usage: xlsPkgObj.setCellValue(wrkSheetN, row, col, valueToSet)");
            var xlsPkg = (ExcelPackage)list[0].GetObjBody();
            var wksheetNo = list[1].ToInt();
            var row = list[2].ToInt();
            var col = list[3].ToInt();
            var valToSet = list[4];
            UtlXls.SetCellVal(xlsPkg, wksheetNo, row, col, valToSet);
            return list[0];
        }

        public static ExprVar FindRowNumberInExcelFile(List<ExprVar> list)
        {
            if (list.Count < 5)
                throw new Exception("Error: incorrect call of FindRowNumberInExcelFile, \n"
                    + "usage: xlsPkgObj.findRowNumber(wrkSheetN, startRow, columNumber, strToFind)");
            // return rowNumber(0 based) or -1 if not found.

            var xlsPkg = (ExcelPackage)list[0].GetObjBody();
            var wksheetNo = list[1].ToInt();
            var startRow = list[2].ToInt();
            var iCol = list[3].ToInt();
            var strValToFind = list[4].ToStr();
            var wksheet = UtlXls.GetWorksheet(xlsPkg, wksheetNo);
            if (startRow<1 || iCol<1)
                throw new Exception("Error: FindRowNumber, Excel sheet number should be >=0, row,col > 0 (1,2...)");
            var iRowFound = UtlXls.FindRowNumber(wksheet, startRow, iCol, strValToFind);
            return ExprVar.CrtVar(iRowFound);
        }

        public static ExprVar SaveExcelFile(List<ExprVar> list)
        {
            var xlsPkg = (ExcelPackage)list[0].GetObjBody();
            xlsPkg.Save();
            return list[0];
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
        private static ExprVar StringFileName(List<ExprVar> list)
        {
            // extract file name from the string with full file path
            // usage: fileStr.fileName(); returns name.Ext
            string str = list[0].ToStr();
            if(str.Length==0) return ExprVar.CrtVar("");
            var val = Path.GetFileName(str);
            return ExprVar.CrtVar(val);
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
            return ExprVar.CrtVar(arr);
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
            return ExprVar.CrtVar(arr);
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
            if (list.Count < 2)
                throw new Exception("Error: incorrect call of Dictionary.has, usage: dict.has(key)");
            var dictVar = list[0];
            var propName = list[1].ToStr();
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            var has = dict.ContainsKey(propName);
            return ExprVar.CrtVar(has? 1 : 0);
        }

        private static ExprVar DictionaryLength(List<ExprVar> list)
        {
            if (list.Count < 1)
                throw new Exception("Error: incorrect call of Dictionary.Length, usage: dict.length()");
            var dictVar = list[0];
            var dict = (Dictionary<string, ExprVar>)dictVar.GetObj().GetVal();
            return ExprVar.CrtVar(dict.Count);
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
