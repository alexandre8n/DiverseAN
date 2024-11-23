using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptRunnerLib
{
    
    public class FilesMoverTask
    {
        // copy/move options:
        public const string OptError = "Error";
        public const string OptReplace = "Replace";
        public const string OptNewCopy = "NewCopy";
        public const string OptOldCopy = "OldCopy";
        private string[] copyOptions = new string[] { OptError, OptNewCopy, OptOldCopy, OptReplace };

        public string TaskPath { get; private set; }
        public string Name => Path.GetFileName(TaskPath);
        public bool IsActive = false;
        public bool IsToMove = false;
        public bool IsToReplaceIfExisits = false;
        public bool IsToIgnoreIfExisits = false;
        public List<string> FromFolders = new List<string>();
        public List<string> ToFolders = new List<string>();
        public List<string> FilePatterns = new List<string>();
        public string ArchiveFolder;
        public string ResultingMessage;

        private string FilePatternPrepared;

        public List<string> MatchingFiles = new List<string>();
        
        // Resulting list of files that were moved/copied
        public List<string> ListOfMovedFiles = new List<string>(); 

        // supported endings: (<number>), <date-time> (-yyyy-MM-dd hh mm ss)
        public string AddEndingForNewCopy = "(<number>)";
        public string CopyOption;

        public FilesMoverTask(string taksPath)
        {
            TaskPath = taksPath;
        }
        public FilesMoverTask()
        {
        }
        public void LoadTaskFromDict(Dictionary<string, string> dict)
        {
            IsActive = true;
            string fromFldrs = dict["FromFolders"];
            var fldrs = fromFldrs.Split(new char[]{ ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            FromFolders = fldrs.Select(x => ProcessFolderName(x)).ToList();
            string fromFilesPtrns = dict["FromFilePatterns"];
            FilePatterns = ProcessFilePatterns(fromFilesPtrns.Split(new char[] { ';' }, 
                StringSplitOptions.RemoveEmptyEntries).ToList());
            string toFldrs = dict["ToFolders"];
            fldrs = toFldrs.Split(new char[] { ';' }, 
                StringSplitOptions.RemoveEmptyEntries).ToList();
            ToFolders = fldrs.Select(x => ProcessFolderName(x)).ToList();
            string renFldr = dict["RenameFilesInFolder"];
        }

        public bool AssignTaskText(string taskBodyStr)
        {
            char[] separators = { '\n', '\r' };
            List<string> lines = taskBodyStr.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();
            bool bRes = ReadTaskInfo(lines);
            return bRes;
        }

        public bool PrepareListOfMatchingFilesFromFolder(string fromFldr)
        {
            var lstFromFldr = utls.GetFilteredFilesList(fromFldr, FilePatterns, false);
            MatchingFiles.AddRange(lstFromFldr);
            return true;
        }
        public bool MatchPattern(string fileName)
        {
            Match m = Regex.Match(fileName, FilePatternPrepared, RegexOptions.IgnoreCase);
            if (m.Success)
                return true;
            return false;
        }

        public bool ReadTaskInfo(List<string> taskLines)
        {
            var filePtrns = new List<string>();
            foreach (var line in taskLines)
            {
                string line1 = line.Trim();
                if (line1.Length < 3 || line1.Substring(0, 2) == "//")
                    continue;
                string[] words = line1.Split(':');
                string keyWord = words[0].ToLower();
                
                if (keyWord == "isactive")
                {
                    string valOfProp = line1.Substring(keyWord.Length + 1).Trim();
                    IsActive = (valOfProp.ToLower() == "true");
                }
                else if (keyWord == "istomove")
                {
                    string valOfProp = line1.Substring(keyWord.Length + 1).Trim();
                    IsToMove = (valOfProp.ToLower() == "true");
                }
                else if (keyWord == "fromfolder")
                {
                    var fromFolder = line1.Substring(keyWord.Length + 1).Trim();
                    var f = ProcessFolderName(fromFolder);
                    FromFolders.Add(f);
                }
                else if (keyWord == "tofolder")
                {
                    var toFolder = line1.Substring(keyWord.Length + 1).Trim();
                    var f = ProcessFolderName(toFolder);
                    ToFolders.Add(f);
                }
                else if (keyWord == "arcivefolder")
                {
                    ArchiveFolder = line1.Substring(keyWord.Length + 1).Trim();
                }
                else if (keyWord == "filepattern")
                {
                    string strPattern = line1.Substring(keyWord.Length + 1).Trim();
                    filePtrns.Add(strPattern);
                }
            }
            FilePatterns = ProcessFilePatterns(filePtrns);
            return true;
        }

        private string ProcessFolderName(string folderName)
        {
            string exePathLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exePath = Path.GetDirectoryName(exePathLoc);
            string res = folderName.Replace(FilePatternKeywords.strExe, exePath);
            return res;
        }

        private List<string> ProcessFilePatterns(List<string> filePatterns)
        {
            List<string> processedPatterns = new List<string>();
            foreach (string ptrnToProcess in filePatterns.Select(x => x.Trim()))
            {
                string ptrnRes = utls.PrcPatternKeyWords(ptrnToProcess);
                processedPatterns.Add(ptrnRes);
            }
            return processedPatterns;
        }

        public bool MoveCopyFiles()
        {
            if (!IsActive)
            {
                return false;
            }
            // check copy option value
            CopyOption = CopyOptChecked();

            var fldrList = FromFolders.Concat(ToFolders);
            if(!CheckFoldersExistance(fldrList))
            { 
                return false; 
            }

            MatchingFiles.Clear(); // here all matching files will be collected...
            foreach (string fromFldr in FromFolders) 
            {
                if (!PrepareListOfMatchingFilesFromFolder(fromFldr))
                    return false;
            }
            if (MatchingFiles.Count==0)
            {
                ResultingMessage += "No files to move or copy found\n";
                return false;
            }
            foreach (var matchingFile in MatchingFiles)
            {
                string fileName = Path.GetFileName(matchingFile);
                foreach (var targetFolder in ToFolders)
                {
                    string moveTarget = Path.Combine(targetFolder, fileName);
                    if (File.Exists(moveTarget))
                    {
                        if(EQ2S(CopyOption,OptError))
                        {
                            ResultingMessage += $"Failed to copy, matched file to existing target file:\n{matchingFile} => {moveTarget}\nCopyOption: {CopyOption}\n";
                            return false; 
                        } 
                        else if(EQ2S(CopyOption, OptNewCopy))
                        {
                            moveTarget = utls.ModifyFileNameToMakeItUniq(moveTarget, "_NewCopy_");
                        }
                    }
                    try
                    {
                        if (EQ2S(CopyOption, OptReplace))
                        {
                            File.Delete(moveTarget);
                        }
                        else if(EQ2S(CopyOption, OptOldCopy))
                        {
                            string moveTargetBackup = utls.ModifyFileNameToMakeItUniq(moveTarget, "_OldCopy_");
                            File.Move(moveTarget, moveTargetBackup);
                        }
                        File.Copy(matchingFile, moveTarget);
                        ResultingMessage += $"Copying file: {matchingFile} => {moveTarget}\n";
                        ListOfMovedFiles.Add(matchingFile);
                    }
                    catch (Exception ex)
                    {
                        ResultingMessage += $"Error! Failed to copy file: {matchingFile} to {moveTarget}\n{ex.Message}\n";
                    }
                }
                if(IsToMove)
                {
                    try
                    {
                        File.Delete(matchingFile);
                        ResultingMessage += $"Source file: {matchingFile} has been deleted\n";
                    }
                    catch (Exception ex)
                    {
                        ResultingMessage += $"Error! Failed to delete file: {matchingFile}\n{ex.Message}\n";
                    }
                }
            }
            return true;
        }

        private string CopyOptChecked()
        {
            if(CopyOption.Trim().Length == 0)
            {
                return OptError;
            }
            foreach(var opt in copyOptions)
            {
                if (EQ2S(CopyOption, opt)) return opt;
            }
            throw new Exception($"Error: FileMover - incorrect velue of the CopyOption: {CopyOption}");
        }

        private bool EQ2S(string copyOpt, string str)
        {
            return String.Equals(copyOpt, str, StringComparison.OrdinalIgnoreCase);
        }

        private bool CheckFoldersExistance(IEnumerable<string> fldrList)
        {
            bool allFoldersExist = true;
            foreach (var fldr in fldrList)
            {
                if (!Directory.Exists(fldr))
                {
                    allFoldersExist = false;
                    ResultingMessage += $"Folder does not exist: {fldr}\n";
                }
            }
            return allFoldersExist;
        }
    }
}
