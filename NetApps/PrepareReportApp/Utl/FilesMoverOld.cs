using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace PrepareReport.Utl
{
    // Tasks are saved in ManagerFolder. They have file extensions: .filesMover
    // it processes the task of moving files, it moves the files from FromFolder to ToFolder, and if there are old files
    // in ToFolder then these old files are moved to ArchiveFolder. If nothing to move from FromFolder to ToFolder
    // nothing is moved to Archive.
    // Task params:
    // FromFolder:
    // ToFolder:
    // ArciveFolder: // folder to move the old files from ToFolder to ArchiveFolder
    // FilePatterns:  // like AN\.SoftwareAces\.\d{4}-\d{2}-\d{2} Invoice-\d{3}\.xls;NG\.SoftwareAces\.\d{4}-\d{2}-\d{2} Invoice \d{6}-\d{2}\.xls;
    public class FilesMoverOld
    {
        public string ErrorMsg;
        public string FailedToFolder;
        public string InfoMsg;
        private string fromFolder;
        private string toFolderDefault;
        private string toFolder;
        private string arciveFolder;
        private List<string> filePatterns;
        private List<string> taskLines;
        public List<string> FilesToMove;
        public List<string> FilesMoved;
        private List<string> FilesMarkedAsMoved; // files that were moved before the task, and marked by user as moved Ok

        public FilesMoverOld(string strToFolderDefault)
        {
            toFolderDefault = strToFolderDefault;
            FilesToMove = new List<string>();
            FilesMarkedAsMoved = new List<string>();
        }

        public void ClearAll()
        { 
            ErrorMsg = "";
            InfoMsg = "";
            fromFolder = "";
            toFolder = "";
            arciveFolder = "";
            filePatterns.Clear();
            taskLines.Clear();
            FilesToMove.Clear();
            FilesMoved.Clear();
            FilesToMove.Clear();
        }

        public bool CheckTasksBeforeDoTasks(List<FileInfo> listOfTasks)
        {
            ErrorMsg = string.Empty;
            InfoMsg = string.Empty;
            foreach (var tsk in listOfTasks)
            {
                if (!CheckOneTask(tsk.FullName))
                    return false;
            }
            return true;
        }
        
        public bool CheckOneTask(string taskPath)
        {
            LoadTask(taskPath);
            // collect all files to move
            // enumerate all files in the sourse folder and move to target
            List<FileInfo> filesAll = utls.BuildListOfAllFiles(fromFolder, false);
            List<FileInfo> filesMatchingToMove = filesAll.Where(x => MatchPattern(x.Name)).ToList<FileInfo>();
            foreach (var fi in filesMatchingToMove)
            {
                string moveTarget = Path.Combine(toFolder, fi.Name);
                FilesToMove.Add(moveTarget);
            }
            if (FilesToMove.Count == 0)
            {
                ErrorMsg = "No files to move were found!";
                FailedToFolder = toFolder;
                return false;
            }

            return true;
        }



        public bool DoTask(string taskPath)
        {
            ErrorMsg = string.Empty;
            InfoMsg = string.Empty;
            LoadTask(taskPath);

            FilesMoved = new List<string>();
            // enumerate all files in the sourse folder and move to target
            List<FileInfo> filesAll = utls.BuildListOfAllFiles(fromFolder, false);
            List<FileInfo> filesToMove = filesAll.Where(x => MatchPattern(x.Name)).ToList<FileInfo>();

            if (filesToMove.Count == 0)
            {
                ErrorMsg = "No files to move were found!";
                return false;
            }

            if (!MoveOldMatchingFilesToArchive())
            {
                return false;
            }

            int nErr = 0;
            foreach (var fi in filesToMove)
            {
                string moveTarget = Path.Combine(toFolder, fi.Name);
                try
                {
                    fi.CopyTo(moveTarget, false);
                    fi.Delete();
                    FilesMoved.Add(moveTarget);
                }
                catch (Exception ex)
                {
                    nErr++;
                    ErrorMsg += string.Format("\nFailed to move the file {0}, {1}\n", fi.FullName, ex.Message);
                }
            }

            InfoMsg = string.Format("Task: {0},\n{1} files moved.", taskPath, filesToMove.Count);
            if (nErr == 0)
            {
                return true;
            }
            string str = ErrorMsg;
            ErrorMsg = string.Format("{0} Errors: {1}", nErr, str);
            return false;
        }

        public bool MatchPattern(string fileName)
        {
            foreach (string ptrn in filePatterns)
            {
                Match m = Regex.Match(fileName.ToLower(), ptrn.ToLower());
                if (m.Success)
                    return true;
            }
            return false;
        }

        private bool MoveOldMatchingFilesToArchive()
        {
            string msgTargetNotExist = "";
            if (!Directory.Exists(toFolder))
            {
                string msg = string.Format("Target folder [{0}] does not exist", toFolder);
                msgTargetNotExist += msg + "\n";
            }
            if (!Directory.Exists(arciveFolder))
            {
                string msg = string.Format("Target archive folder [{0}] does not exist", arciveFolder);
                msgTargetNotExist += msg + "\n";
            }

            if (!string.IsNullOrEmpty(msgTargetNotExist))
            {
                MessageBox.Show(msgTargetNotExist);
                return false;
            }
            List<FileInfo> filesAll = utls.BuildListOfAllFiles(toFolder, false);
            List<FileInfo> filesToMove = filesAll.Where(x => MatchPattern(x.Name)).ToList<FileInfo>();
            // move all matching files to archive, without replacing, just with special names like name, name_1, ...
            MoveFiles(filesToMove.Select(x => x.FullName).ToList(), arciveFolder);
            return true;
        }

        public static void MoveFiles(List<string> filesToMove, string folder)
        {
            foreach (var filePath in filesToMove)
            {
                string fileName = Path.GetFileName(filePath);
                string moveTarget = Path.Combine(folder, fileName);
                if (File.Exists(moveTarget))
                {
                    moveTarget = utls.ModifyFileNameToMakeItUniq(moveTarget, "_copy_");
                }
                File.Copy(filePath, moveTarget);
                File.Delete(filePath);
            }
        }

        public void LoadTask(string path)
        {
            taskLines = new List<string>();
            foreach (string line in utls.ReadAllLinesFromTextFile(path))
            {
                string line1 = line.Trim();
                if (line1.Length < 3 || line1.Substring(0, 2) == "//")
                    continue;
                string[] words = line1.Split(':');
                string keyWord = words[0].ToLower();
                if (keyWord == "fromfolder")
                {
                    fromFolder = line1.Substring(keyWord.Length + 1).Trim();
                    if(fromFolder == ".")
                        fromFolder = Path.GetDirectoryName(path);
                }
                else if (keyWord == "tofolder")
                {
                    toFolder = line1.Substring(keyWord.Length + 1).Trim();
                    if (toFolder.Substring(0, 2) == ".\\")
                        toFolder = Path.Combine(toFolderDefault, toFolder.Substring(2));
                }
                else if (keyWord == "arcivefolder")
                {
                    arciveFolder = line1.Substring(keyWord.Length + 1).Trim();
                    if (arciveFolder.Substring(0, 2) == ".\\")
                        arciveFolder = Path.Combine(toFolderDefault, arciveFolder.Substring(2)); 
                }
                else if (keyWord == "filepatterns")
                {
                    string strPatterns = line1.Substring(keyWord.Length + 1).Trim();
                    string[] arrPatterns = strPatterns.Split(';');
                    filePatterns = ProcessFilePatterns(arrPatterns);
                        
                }
            }
        }

        private List<string> ProcessFilePatterns(string[] arrPatterns)
        {
            List<string> processedPatterns = new List<string>();
            foreach (string ptrnToProcess in arrPatterns.Select(x => x.Trim()))
            {
                string ptrnRes = PrcPatternKeyWords(ptrnToProcess);
                processedPatterns.Add(ptrnRes);
            }
            return processedPatterns;
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

        public void MarkAsMovedOK(List<string> filesMarkedAsMoved)
        {
            FilesMarkedAsMoved.AddRange(filesMarkedAsMoved);
        }

        public List<string> GetAllMovedAndMarkedAsMoved()
        {
            List<string> all = new List<string>();
            Dictionary<string, string> allUniq = new Dictionary<string, string>();
            foreach(string s in FilesMoved)
            {
                allUniq[s.ToUpper()] = s;
            }
            foreach (string s in FilesMarkedAsMoved)
            {
                allUniq[s.ToUpper()] = s;
            }
            return allUniq.Values.ToList();
            
        }


    }
}
