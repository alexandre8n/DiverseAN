using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptRunnerLib
{
    public class FileMover
    {
        public FileMover()
        {
        }
        public ExprVar FromFolders { get; set; }
        public ExprVar ToFolders { get; set; }
        public ExprVar FromFilePatterns { get; set; }
        public ExprVar ListOfCopiedFiles { get; set; }
        public ExprVar ResultingMessage => ExprVar.CrtVar(resultingMsg);
        private string resultingMsg = "";
        public ExprVar RenameDict { get; internal set; } = null; // str to build dict of the rules by renaming (if any)

        public ExprVar CopyOption { get; set; }
        public ExprVar RenameOption { get; set; }

        // string of the following format:
        // Dict:{EINS->01,ZWEI->02,DREI->03,VIER->04,SECHS->06,SIEBEN->07,ACHT->08,ZEHN->10,ABEND->2,MORGEN->1}
        // Dict:{ and } are optional
        // EINS->01,ZWEI->02,DREI->03,VIER->04,SECHS->06,SIEBEN->07,ACHT->08,ZEHN->10,ABEND->2,MORGEN->1
        int renameCounter = 0; // used to support {Counter:000} 


        FilesMoverTask fmTask = null;
        Dictionary<string, string> replacementDict = null;  // dictionary of the renaming rules, if any
        List<string> listOfInitialFilesToRename = new List<string>();
        List<string> listOfRenamedFiles = new List<string>();

        public ExprVar GetListOfInitialFilesToRename()
        {
            var lst = listOfInitialFilesToRename.Select(x=>ExprVar.CrtVar(x)).ToList();
            return ExprVar.CrtVar(lst);
        }
        public ExprVar GetListOfRenamedFiles()
        {
            var lst = listOfRenamedFiles.Select(x => ExprVar.CrtVar(x)).ToList();
            return ExprVar.CrtVar(lst);
        }
        public ExprVar GetRefFromFolders()
        {
            if (FromFolders == null)
            {
                //var list = new List<ExprVar>();
                FromFolders = ExprVar.CrtVar("list");
            }
            return FromFolders;
        }

        public bool MoveCopyFiles()
        {
            resultingMsg = "";
            fmTask = new FilesMoverTask();
            fmTask.IsActive = true;
            fmTask.FromFolders = ExprVar2StrList(FromFolders);
            fmTask.FilePatterns = ExprVar2StrList(FromFilePatterns);
            fmTask.ToFolders = ExprVar2StrList(ToFolders);
            fmTask.CopyOption = CopyOption==null? "": CopyOption.ToStr();
            bool bRes = fmTask.MoveCopyFiles();
            resultingMsg = fmTask.ResultingMessage;
            var lstMoved = fmTask.ListOfMovedFiles.Select(x => ExprVar.CrtVar(x)).ToList();
            ListOfCopiedFiles = ExprVar.CrtVar(lstMoved);
            return bRes;
        }

        static public List<string> ExprVar2StrList(ExprVar obj)
        {
            var lst = new List<ExprVar>();
            if (obj.GetTypeOfObj() == "String")
            {
                lst = new List<ExprVar>() { obj };
            }
            else
            {
                lst = (List<ExprVar>)(obj.GetObj().GetVal());
            }
            return lst.Select(x => x.ToStr()).ToList();
        }

        public void Clear()
        {
            FromFolders = null;
            ToFolders = null;
            FromFilePatterns = null;
            ListOfCopiedFiles = null;
            CopyOption = null;
            RenameOption = null;
            resultingMsg = "";
            fmTask = null;
        }

        public bool RenameFiles(string sFolder, string sPtrn)
        {
            // results: resultingMessage -> messages about renamed files;
            // returns: true if renamed, false - otherwise/
            // folder where the files are located.
            // pattern: "[RegEx:]<what to rename> -> <newfileNamePattern>
            // patterns may contain {Counter}, {Counter:0..0}
            // example: "RegEx:IMG_\d+.jpg -> Zabolotn6-47_{Counter:000}.jpg"
            // $1,$2... where $N is a caught part in pattern
            // abc(\w\w\d+)(.docx) -> $1-file$2 (i.e. abcXY11.docx -> XY11-file.docx)
            var splitDelim = new string[] { " -> " };
            string[] ptrns = sPtrn.Split(splitDelim, StringSplitOptions.RemoveEmptyEntries);
            string ptrnSrc = utls.PrcPatternKeyWords(ptrns[0]);
            string ptrRen = utls.PrcPatternKeyWords(ptrns[1], true);
            replacementDict = Utl.StrToDict(RenameDict==null?"":RenameDict.ToStr());
            listOfInitialFilesToRename = new List<string>();
            listOfRenamedFiles = new List<string>();


            List<string> files = utls.GetListFiles(sFolder, ptrnSrc, false);

            if (ptrnSrc.StartsWith(utls.sRegExp))
            {
                ptrnSrc = ptrnSrc.Substring(utls.sRegExp.Length);
            }

            int count = 0;
            renameCounter = 1;
            foreach (string filePath in files)
            {
                bool renOk = RenameFileByPattern(filePath, ptrnSrc, ptrRen);
                if (renOk)
                {
                    continue;
                }
                // renameCounter is updated automatically
                count++;
            }
            return true;
        }
        public bool RenameFileByPattern(string filePath,string ptrnSrc,string ptrRen)
        {
            string file = Path.GetFileName(filePath);
            string fileFolder = Path.GetDirectoryName(filePath);
            string newName = utls.ReplaceWithDict(file, ptrnSrc, ptrRen, replacementDict);
            // if count is contained update the name
            string ptrnCount = "{Counter(:0+)?}";
            Match mtch = Regex.Match(newName, ptrnCount, RegexOptions.IgnoreCase);
            if (mtch.Success)
            {
                // case with counter! while exists file count++
                int idx = mtch.Index;
                int len = mtch.Length;
                string countFmt = string.Empty;
                if (mtch.Groups.Count == 2 && mtch.Groups[1].Value.Length > 0)
                {
                    countFmt = mtch.Groups[1].Value.Substring(1);
                }
                var nmCount = utls.GetUniquFileName(fileFolder, newName, idx, len, countFmt, renameCounter);
                if (nmCount.name.Length == 0)
                {
                    resultingMsg += $"Failed to rename file: {filePath}, rename patters {ptrnSrc} -> {ptrRen}\n";
                    return false;
                }
                newName = nmCount.name;
                renameCounter = nmCount.count;
            }
            // rename file:
            var newNamePath = Path.Combine(fileFolder, newName);
            listOfInitialFilesToRename.Add(filePath);
            listOfRenamedFiles.Add(newNamePath);
            bool removeExisting = RenameOption != null && RenameOption.ToStr().ToUpper() == "REPLACE";
            utls.MoveRename(filePath, newNamePath, removeExisting);
            resultingMsg += $"renamed: {filePath} -> {newName}\n";
            return true;
        }
    }
}
