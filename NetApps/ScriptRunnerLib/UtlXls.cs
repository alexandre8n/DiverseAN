using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace ScriptRunnerLib
{
    public class UtlXls
    {
        public const string afxRegExp = "RegEx:";
        public static ExcelPackage OpnXlsFile(string filePath)
        {
            string sExt = Path.GetExtension(filePath);
            if (sExt.ToUpper() != ".XLSX") throw new Exception($"Incorrect file extention {sExt}, xlsx expected ");
            if(!File.Exists(filePath))
                throw new Exception($"File \"{filePath}\" does not exist");
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            FileInfo existingFile = new FileInfo(filePath);
            ExcelPackage package = new ExcelPackage(existingFile);
            return package;
        }
        public static void ClsXlsFile(ExcelPackage package)
        {
            package.Dispose();
        }
        public static void SaveXls(ExcelPackage package)
        {
            package.Save(); 
        }
        public static void SaveXlsAs(ExcelPackage package, string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            package.SaveAs(file);
        }

        public static ExcelWorksheet GetWorksheet(ExcelPackage exlsPkg,  int iWrk) 
        { 
            return exlsPkg.Workbook.Worksheets[iWrk];
        }
        public static int FindRowNumber(ExcelWorksheet worksheet, int startingRow, int col, string valueToFind)
        {
            string ptrn = "";
            if (UtlParserHelper.Subs(valueToFind, 0, afxRegExp.Length) == afxRegExp)
            {
                ptrn = UtlParserHelper.Subs(valueToFind, afxRegExp.Length, valueToFind.Length);
            }
            for (int i = startingRow; i < startingRow + 1000; i++)
            {
                var curRowVal = worksheet.Cells[i, col].Value;
                if (curRowVal == null)
                    continue;
                string strCurRowVal = curRowVal.ToString() ?? "";
                if (ptrn.Length > 0 && Regex.IsMatch(strCurRowVal, ptrn))
                    return i;
                else if (ptrn.Length == 0 && strCurRowVal == valueToFind)
                    return i;
            }
            return -1;
        }
        public static void SetCellVal(ExcelPackage package, int workSheetId, int row, int col, ExprVar valToAssign)
        {
            object val = valToAssign.ToObj();
            var worksheet = package.Workbook.Worksheets[workSheetId];
            worksheet.Cells[row, col].Value = val;
        }
        public static ExprVar GetCellVal(ExcelPackage package, int workSheetId, int row, int col)
        {
            var worksheet = package.Workbook.Worksheets[workSheetId];
            object val = worksheet.Cells[row, col].Value;
            return ExprVar.CrtVar(val);
        }
    }
}
