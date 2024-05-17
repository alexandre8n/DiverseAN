using OfficeOpenXml;

// See https://aka.ms/new-console-template for more information
// introduce NodeRunner
//- Dictionary with [] operator. .length, for(a in b)
//+- list instead of array [] operator. .length, 
//+ multi-rename
//+ sysClasses: 
// for array: add, deleteAt, indexOf, sort(asc/desc: for strings, int, double)
// for dict: add, setAt(key, val), getAt(key), deleteAt(key), getKeysAsArr(), getValuesAsArr()
// for file, for Directory
// for regExp

// other helpful funcs: pow(n, m), %-reminder
// strToInt, strToDouble, substr, indexOf, 
// make working a.b, a.func(x,y,...)
// todo: make xlsx files modification from script to work
// object = dictionary (key, value): d={pr1: x, pr2: y, pr3="abc"}; v=d.pr1; v2=d["pr3"];v2=d[a+b]
// array adding, deleting, finding, sorting
// -------------------- Step2 ------------------------
// todo: make defined functions to work
// class
// connecting scripts in other files
// script libraries
//+ todo: if
//+ array: a = [x, y, 5, "abc"]; v=a[1]; v2=a[i]
//+ array - at run, creation, accessing,
//+ ignore comments like: //
//+ break, continue;
//+ while, for, for(let a in arr)
//+ make working with a first standard functions: min(a,b), max(a,b), min(a,....,)
//+ make working a construct: a.func(x,y,...) for standard objects, like string, array, dictionary

using ExprParser1;
using ScriptRunnerLib;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

if (args.Length > 0)
{
    Console.WriteLine("ScriptRunnerTest. The following arguments are specified:");
    int i = 1;
    foreach (var arg in args)
    {
        Console.WriteLine($"Argument{i}={arg}");
        i++;
    }
}
else Console.WriteLine("ScriptRunnerTest. Started...");

//TestExcel();
//testParser();
testScriptRunner();
Console.WriteLine("ScriptRunnerTest. Ended...");

void testParser()
{
    string folder = @"F:\Work\Temp\PrepareReportTest\CopyTo2";
    string patern = "RegEx:IMG_\\d+.jpg -> Zabolotn6-47_{Counter}.jpg";
    string patern1 = "RegEx:.+.jpg -> IMG_{EndOfThisWeekDate}_{Counter}.jpg";
    string patern2 = "IMG_{YearWeekNumber}_{Counter}.jpg";
    string res1 = utls.PrcPatternKeyWords(patern2, false);
    var res = utls.RenameFiles(folder, patern1);
    //var ep = new ExprNode("a+func(a,[x,y],b)");//let a=[1,2]
    var ep = new ExprNode("a.b.c=x");//"a=[x,y]",let b = a[1];s = a.length
    ep.BuildExprTree();
    string s = ep.DumpToString(false);
    Console.WriteLine(s);
}

void testScriptRunner()
{

    var sr = new ScriptRunner();

    //string script0 = "let b='Hello'; let a = b.left(2);";
    string fName = "scrTestLoopsOut.txt";
    string fPath = @"F:\Work\GitHub\DiverseAN\NetApps\ScriptRunnerTest\ScriptExamples\";
    string script = File.ReadAllText(fPath + fName);
    if (!sr.Parse(script))
    {
        Console.WriteLine(sr.message);
        return;
    }
    string sDump = sr.Dump(1);
    Console.WriteLine(sDump);
    if (!sr.Run())
    {
        Console.WriteLine(sr.message);
        return;
    }
    Console.WriteLine($"ScripRunner finished...");
}
//var a = sr.GetVar("a");
//var s = sr.GetVar("s");
//var b = sr.GetVar("b");
////var b1 = sr.GetVar("five2");
//Console.WriteLine($"a={a.ToString()}, five2=b1.ToString(), b={b}");
//var res = UtlParserHelper.FindSafeOneOf("//abc//bb,aa", 2, new string[] {"//x", ", "});
//string s1 = UtlParserHelper.ReplaceManyToOne("abc\n\n\n\nABC\n\n\n", "\n");
//ExprVar v1 = new ExprVar("a", EType.E_INT, "2", null);
//ExprVar v2 = new ExprVar("b", EType.E_DOUBLE, "1.2", null);
//ExprVar v3 = v2.Multiply(v1);
//string script0 = "{let a=0; a=a+1;out=a;} let a; a=5; let five2 = a*a;let out=0;";
//string script0 = "let a=1.5; let five2=1; if(a<=0)a=0;else if(a<=1)a=1;else {let a=2.1; if(a<=2)a=2;else a=3;five2 = a*a;} let out=a;";
//    string script0 = "let a = [2,3,4];let s = 0; for(let i=0; i<3; i=i+1) {s=s+a[i];}";
//"while(i<3) {s=s+a[i];if(i==2)break;else if(i/2==1){s=s+0;s=s+0;}else{s=s+0;} i=i+1;}\n" +
//"{s=s+a[i];if(i==2)break;//i=i+1;}";
//"for(let i=0; i<3; i=i+1) {if(i==0)continue;s=s+a[i];}";
//string script1 = "let a = (17+3*6);\n";
//script1 += "let s=0; for (let i=1; i<10;i=i+1) {s=s+i; }\n";
//script1 += "a = 1; while(   a != 2) { a++;}\n";
//script1 += "  if( a==2) { return;}\n";
//script1 += " function f1(  a, b){let c = a*10;\nlet d=b+10; return c*d;}\n let res = f1(10, 20);";
//string script0 = "let a = [2,3,4];let s = a.length; let b =\"abc\"; b = b.length; ";
//string script2 = "let a; a=5; let five2 = pow(a,2);";
//script2 += "function pow(a, n){let res=1; for (let i=0; i<n; i=i+1) {res=res*a; } return res;\n}";
//string script0 = "let b=-1; let a = min(2,3,b);";

void TestExcel()
{
    string fn = @"F:\Work\GitHub\DiverseAN\NetApps\ScriptRunnerTest\ExcelFiles\AC.VPro.Timebooking report 2024-04-07.xlsx";
    string fnInv = @"F:\Work\GitHub\DiverseAN\NetApps\ScriptRunnerTest\ExcelFiles\NG.SoftwareAces.2024-04-07-VproDev.xlsx";

    ExprVar p1 = ExprVar.CrtVar(fnInv);
    List<ExprVar> lst = new List<ExprVar>() { p1};
    var v1 = ScrSysFuncList.OpenExcelFile(lst);

    //readXLS(fn);
    //WriteExcel(fnInv);
}


void readXLS(string FilePath)
{
    if (!File.Exists(FilePath)) return;
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    FileInfo existingFile = new FileInfo(FilePath);
    using (ExcelPackage package = new ExcelPackage(existingFile))
    {
        //get the first worksheet in the workbook
        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
        int colCount = worksheet.Dimension.End.Column;  //get Column Count
        int rowCount = worksheet.Dimension.End.Row;     //get row count
        string s1 = "";

        object c2 = worksheet.Cells[1, 2].Value;

        if (c2 is not null)
        {
            s1 = c2.ToString()?? "";
            s1 = s1.Trim();
        } 
        for (int row = 1; row <= rowCount; row++)
        {
            for (int col = 1; col <= colCount; col++)
            {
                object c1 = worksheet.Cells[row, col].Value;
                string cellTxt = (c1 == null) ? "" : c1.ToString() ?? "";
                cellTxt = cellTxt.Trim();
                Console.WriteLine($" Row:{row} column: {col} Value: {cellTxt}");
            }
        }
    }
}
void WriteExcel(string FilePath)
{
    if (!File.Exists(FilePath)) return;
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    FileInfo existingFile = new FileInfo(FilePath);
    ExcelPackage package = new ExcelPackage(existingFile);
    
    //using (ExcelPackage package = new ExcelPackage(existingFile))
    //{
        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
        int iRow = FindRowNumber(worksheet, 10, 1, "RegEx:^VPro - \\w\\w");
        if(iRow>=0)
        {
            SetCellVal(package, 0, iRow, 7, ExprVar.CrtVar(13.7));
        }
        package.Save();
    //}
    package.Dispose();
}

void SetCellVal(ExcelPackage package, int workSheetId, int row, int col, ExprVar valToAssign)
{
    object val = valToAssign.ToDouble();
    var worksheet = package.Workbook.Worksheets[workSheetId];
    worksheet.Cells[row, col].Value = val;
}


const string afxRegExp = "RegEx:";
int FindRowNumber(ExcelWorksheet worksheet, int startingRow, int col, string valueToFind)
{
    string ptrn = "";
    if(UtlParserHelper.Subs(valueToFind, 0, afxRegExp.Length) == afxRegExp)
    {
        ptrn = UtlParserHelper.Subs(valueToFind, afxRegExp.Length, valueToFind.Length);
    }
    for (int i = startingRow; i < startingRow + 1000; i++)
    {
        var curRowVal = worksheet.Cells[i, col].Value;
        if (curRowVal == null)
            continue;
        string strCurRowVal = curRowVal.ToString()??"";
        if(ptrn.Length>0 && Regex.IsMatch(strCurRowVal, ptrn))
            return i;
        else if (ptrn.Length==0 && strCurRowVal == valueToFind)
            return i;
    }
    return -1;
}

// diverse tests...............
//var v = ExprVar.CrtVar(new Dictionary<string, ScrObj>());
//v = ExprVar.CrtVar(new Dictionary<string, ExprVar>());
//string s1 = UtlParserHelper.Left("Hello, World", 5);
//string s2 = UtlParserHelper.Left("Hello, World", 55);
//string s3 = UtlParserHelper.Right("Hello, World", 5);
//string s4 = UtlParserHelper.Right("Hello, World", 25);
//string s5 = UtlParserHelper.SubsRng("Hello, World", 5, -1);
//string s6 = UtlParserHelper.SubsRng("Hello, World", 25, 1);
//string s7 = UtlParserHelper.SubsRng("Hello, World", -25, -1);
//string s8 = UtlParserHelper.SubsRng("Hello, World", 10, 2);
//string s9 = UtlParserHelper.SubsRng("Hello, World", 2, 10);
// replace RegExp:
//string input = "DermoVisionInv.xlsx"; // "deceive relieve achieve belief fierce receive";
////DermoVision(Inv).(xlsx)

//string pattern = @"\w*(inv)[. ]*(xlsx)";
//string replacement = "NGSA-$1-2024-12-31.$2";
//Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
//string result = rgx.Replace(input, replacement);