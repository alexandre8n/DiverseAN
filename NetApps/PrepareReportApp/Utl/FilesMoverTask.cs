using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrepareReport.Utl
{
    public class FilesMoverTask
    {
        public string TaskPath { get; private set; }
        public string Name => Path.GetFileName(TaskPath);
        public bool IsActive = false;
        public bool IsToMove = false;
        public bool IsToReplaceIfExisits = false;
        public bool IsToIgnoreIfExisits = false;
        public bool IsToNewVersionIfExisits = true;
        public string FromFolder;
        public List<string> ToFolders = new List<string>();
        public List<string> FilePatterns = new List<string>();
        public string ArchiveFolder;
        public string ResultingMessage;

        public string FilePatternPrepared;

        public List<string> MatchingFiles = new List<string>();

        // supported endings: (<number>), <date-time> (-yyyy-MM-dd hh mm ss)
        public string AddEndingForNewCopy = "(<number>)";

        public FilesMoverTask(string taksPath)
        {
            TaskPath = taksPath;
        }

        public bool PrepareListOfMatchingFilesFromFolder()
        {
            foreach (var filePattern in FilePatterns)
            {
                FilePatternPrepared = PrcPatternKeyWords(filePattern);
                MatchingFiles.Clear();
                List<FileInfo> filesAll = utls.BuildListOfAllFiles(FromFolder, false);
                List<FileInfo> filesToMove = filesAll.Where(x => MatchPattern(x.Name)).ToList<FileInfo>();

                if (filesToMove.Count == 0)
                {
                    ResultingMessage = "No files to move were found!";
                    return false;
                }
                MatchingFiles = filesToMove.Select(x => x.FullName).ToList();
            }
            return true;
        }
        public bool MatchPattern(string fileName)
        {
            Match m = Regex.Match(fileName, FilePatternPrepared, RegexOptions.IgnoreCase);
            if (m.Success)
                return true;
            return false;
        }
        static public string PrcPatternKeyWords(string ptrnToProcess)
        {
            if (ptrnToProcess.Contains(AppInfo.strEndOfThisWeekDate))
            {
                DateTime nextSunday = utls.NearestSunday(DateTime.Today);
                string strDate = nextSunday.ToString("yyyy-MM-dd");
                ptrnToProcess = ptrnToProcess.Replace(AppInfo.strEndOfThisWeekDate, strDate);
            }
            if (ptrnToProcess.Contains(AppInfo.strTodayDate))
            {
                string strDate = DateTime.Today.ToString("yyyy-MM-dd");
                ptrnToProcess = ptrnToProcess.Replace(AppInfo.strTodayDate, strDate);
            }
            if (ptrnToProcess.Contains(AppInfo.strEndOfThisMonthDate))
            {
                DateTime endOfMonth = utls.EndOfMonthDate(DateTime.Today);
                string strDate = endOfMonth.ToString("yyyy-MM-dd");
                ptrnToProcess = ptrnToProcess.Replace(AppInfo.strEndOfThisMonthDate, strDate);
            }
            return ptrnToProcess;
        }

        public bool ReadTaskInfo(List<string> taskLines)
        {
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
                    FromFolder = line1.Substring(keyWord.Length + 1).Trim();
                }
                else if (keyWord == "tofolder")
                {
                    var toFolderline = line1.Substring(keyWord.Length + 1).Trim();
                    ToFolders = toFolderline.Split(';').ToList();
                }
                else if (keyWord == "arcivefolder")
                {
                    ArchiveFolder = line1.Substring(keyWord.Length + 1).Trim();
                }
                else if (keyWord == "filepatterns")
                {
                    string strPatterns = line1.Substring(keyWord.Length + 1).Trim();
                    var filePatterns1 = strPatterns.Split(';').ToList();
                    FilePatterns = ProcessFilePatterns(filePatterns1);
                }
            }
            return true;
        }

        private List<string> ProcessFilePatterns(List<string> filePatterns)
        {
            List<string> processedPatterns = new List<string>();
            foreach (string ptrnToProcess in filePatterns.Select(x => x.Trim()))
            {
                string ptrnRes = PrcPatternKeyWords(ptrnToProcess);
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
            if (!PrepareListOfMatchingFilesFromFolder())
                return false;
            if (MatchingFiles.Count==0)
            {
                return false;
            }
            foreach (var matchingFile in MatchingFiles)
            {
                string fileName = Path.GetFileName(matchingFile);
                foreach (var targetFolder in ToFolders)
                {
                    string moveTarget = Path.Combine(targetFolder, fileName);
                    if (File.Exists(moveTarget) && IsToNewVersionIfExisits)
                    {
                        moveTarget = utls.ModifyFileNameToMakeItUniq(moveTarget, "_copy_");
                    }
                    try
                    {
                        File.Copy(matchingFile, moveTarget);
                        ResultingMessage += $"Copying file: {matchingFile} => {moveTarget}\n";
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
    }
}
