using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using System.Reflection;
using NPOI.SS.UserModel;
using PrepareReport.Utl;

namespace PrepareReport
{
    public enum ErrorType { reportNotFound = 1, severalReportsNotFound=2, Other = 3 }
    public enum ReportEventHandlingResult { resContinue = 1, resStop = 2  }

    public delegate void ReportMoverEventHandler(object sender, ReportMoverEventArgs e);
    public class ReportMoverEventArgs : EventArgs
    {
        public ErrorType errType;
        public string ErrorMsg;

        // special information is set in this field, 
        // reportNotFound - path to folder, being processed
        // severalReportsNotFound - list of paths where reports where not found
        public object Val;              

        public ReportEventHandlingResult handlingResult = ReportEventHandlingResult.resContinue;
        public ReportMoverEventArgs(ErrorType et, string errMsg, object val)
        {
            errType = et;
            ErrorMsg = errMsg;
            Val = val; 
        }
    }

    public class ReportsMover
    {
        readonly string[] tasksToIgnore = {"vacation", "vacations", "dayoff", "day-off", "day off", "holiday", "holidays", "Independence Day",
                                        "Christmas", "Easter", "victory", "Constitution", "Women", "Woman" };
        const int maxLengOfTaskToIgnore = 33;
        public event ReportMoverEventHandler ReportMoverEvent;
        public const string rptManagerFileNameTemplate = "AN.CleverCAD.Timebooking report {0}.xls";
        private string mngrReportFileToMove = string.Empty;
        public List<string> MovedReportFiles = new List<string>();
        private string srcFolder;
        private string trgFolder;
        private string trgFolderArchive;
        private string managerReportPhraseToAdd;
        private string managerFolder;
        private bool generateManagerReport = true;
        private bool isToSaveTasksToFiles = true;

        private List<string> directories = new List<string>();
        public List<string> directoriesNoReportsFound = new List<string>();
        MainWindow.MsgToMainWnd MessageSender = null;
        
        HSSFWorkbook hssfworkbook;
        Dictionary<string, string> DeveloperToTasks = new Dictionary<string, string>();
        Dictionary<int, string> dayToTasks = new Dictionary<int, string>();
        string devPrefix;
        static DateTime dtMinExpected = DateTime.MinValue;
        static DateTime dtMaxExpected = DateTime.MaxValue;
        public int nReprotsMoved = 0;
        public string ErrorMsg = "";

        public ReportsMover(string srcFldr, string trgFldr, string trgFldrArchive, string managerFldr,
            string mngrRptPhraseToAdd, bool mngrGenerateManagerReport, bool isToSaveTasksToFiles)
        {
            this.srcFolder = srcFldr;
            this.trgFolder = trgFldr;
            this.trgFolderArchive = trgFldrArchive;
            this.managerFolder = managerFldr;
            this.managerReportPhraseToAdd = mngrRptPhraseToAdd;
            this.generateManagerReport = mngrGenerateManagerReport;
            this.isToSaveTasksToFiles = isToSaveTasksToFiles;
        }
        protected virtual ReportMoverEventArgs OnReportEvent(ReportMoverEventArgs e)
        {
            if (ReportMoverEvent != null)
            {
                ReportMoverEvent(this, e);//Raise the event
            }
            return e;
        }

        public string DtMaxExpected()
        {
            DateTime dtNow = DateTime.Now;
            int nDay = (int)dtNow.DayOfWeek;
            nDay = (nDay == 0) ? 7 : nDay;
            dtMinExpected = dtNow.AddDays(-nDay).Date;
            dtMaxExpected = dtMinExpected.AddDays(7);
            return dtMaxExpected.ToString("yyyy-MM-dd");
        }

        public void CollectLastMoved()
        {
            DtMaxExpected();
            DirectoryInfo dir = new DirectoryInfo(trgFolder);
            List<string> files = new List<string>();
            foreach (System.IO.FileInfo f in dir.GetFiles("*.*"))
            {
                string fileName = f.Name.ToUpper();
                string userReporter = GetUserNameFromFileName(fileName);
                if (fileName.StartsWith(string.Format("{0}.CLEVERCAD.TIMEBOOKING REPORT", userReporter)))
                {
                    if (IsThisWeekFile(fileName))
                    {
                        string fullPathOfRptFile = Path.Combine(trgFolder, fileName);
                        MovedReportFiles.Add(fullPathOfRptFile);
                    }
                }
            }
        }

        public string GetUserNameFromFileName(string fn)
        {
            int iNameEnd = fn.IndexOf(".");
            if (iNameEnd < 1)
                return string.Empty;
            return fn.Substring(0, iNameEnd);
        }

        public bool CheckBeforeMoveReports(MainWindow.MsgToMainWnd dlgMsg)
        {
            ErrorMsg = "";
            if (!string.IsNullOrEmpty(managerFolder))
                managerFolder = Path.Combine(srcFolder, managerFolder);
            DtMaxExpected();
            MovedReportFiles.Clear();
            directoriesNoReportsFound.Clear();
            nReprotsMoved = 0;
            MessageSender = dlgMsg;
            CheckManagerReport();
            GetDirectoriesToMoveFrom(); // -> directories
            foreach (string dirFrom in directories)
            {
                if (!CheckIfReportOk(dirFrom))
                {
                    directoriesNoReportsFound.Add(dirFrom);
                    string msg = string.Format("No report found in {0}\n", dirFrom);
                    AddErrorMsg(msg);
                    dlgMsg(msg);

                    ReportMoverEventArgs res = OnReportEvent(new ReportMoverEventArgs(ErrorType.reportNotFound, msg, dirFrom));
                    if(res.handlingResult == ReportEventHandlingResult.resStop)
                        return false;
                }
            }
            if (directoriesNoReportsFound.Count > 0)
            {
                ReportMoverEventArgs res = OnReportEvent(new ReportMoverEventArgs(ErrorType.severalReportsNotFound, "The following folders do not have reports", directoriesNoReportsFound));
                if (res.handlingResult == ReportEventHandlingResult.resStop)
                    return false;
            }
            return true;
        }

        private void AddErrorMsg(string msg)
        {
            ErrorMsg += msg + "\r\n";
        }
        
        public bool CheckIfReportOk(string dir)
        {
            string reportFileToMove = GetFileNameToMove(dir, null);
            return !string.IsNullOrEmpty(reportFileToMove);
        }

        public bool MoveReports(MainWindow.MsgToMainWnd dlgMsg)
        {
            MoveOldFilesToArchive();

            if (!CheckFolders(srcFolder, trgFolder))
                return false;
            foreach (string dirFrom in directories)
            {
                MoveReportFrom(dirFrom);
            }
            if(isToSaveTasksToFiles)
                SaveTasksToFiles();
            if(generateManagerReport)
                AutoFillManagerFile();
            return true;
        }

        void GetDirectoriesToMoveFrom()
        {
            directories.Clear();
            DirectoryInfo dir = new DirectoryInfo(srcFolder);
            foreach (System.IO.DirectoryInfo g in dir.GetDirectories())
            {
                if (managerFolder.ToUpper() == g.FullName.ToUpper() && generateManagerReport == false)
                {
                    continue;
                }

                directories.Add(g.FullName);
            }
        }

        bool MoveOldFilesToArchive()
        {
            if (!Directory.Exists(trgFolderArchive))
            {
                try
                {
                    Directory.CreateDirectory(trgFolderArchive);
                }
                catch(Exception ex1)
                {
                    string msg = string.Format("Archive folder [{0}] does not exist and cannot be created\n{1}", trgFolderArchive, ex1.Message);
                    System.Windows.MessageBox.Show(msg);
                    return false;
                }

            }

            DirectoryInfo dir = new DirectoryInfo(trgFolder);
            foreach (System.IO.FileInfo f in dir.GetFiles("*.*"))
            {
                string filePath = f.FullName;
                string filePathTarget = Path.Combine(trgFolderArchive, f.Name);
                // check target
                if (File.Exists(filePathTarget))
                {
                    string msg = string.Format("Failed to archive the file [{0}],\nit exists in the target folder", filePathTarget);
                    System.Windows.MessageBox.Show(msg);
                    return false;
                }
                if (IsOldFileToArchive(filePath))
                { 
                    f.CopyTo(Path.Combine(trgFolderArchive, f.Name), false);
                    f.Delete();
                }
            }
            return true;
        }

        bool AutoFillManagerFile()
        {
            string mngrReportFileTarget = Path.Combine(trgFolder, ManagerFileName());
            FileStream file = new FileStream(mngrReportFileTarget, FileMode.Open, FileAccess.ReadWrite);
            hssfworkbook = new HSSFWorkbook(file);
            //create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "AN-Company";
            hssfworkbook.DocumentSummaryInformation = dsi;
            //create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "AN Report";
            hssfworkbook.SummaryInformation = si;
            var sheet1 = hssfworkbook.GetSheetAt(0);
            int iMondayRow = FindMonday(sheet1);
            if (iMondayRow < 0)
                return false;
            for (int i = 0; i < 5; i++)
            {
                UpdateMngrRow(sheet1, iMondayRow, i);
            }
            file.Close();
            file = new FileStream(mngrReportFileTarget, FileMode.Open, FileAccess.ReadWrite);
            hssfworkbook.Write(file);
            file.Close();
            return true;
        }

        void UpdateMngrRow(ISheet sheet1, int iMondayRow, int iDay)
        {
            string sDayTasks;
            if (!dayToTasks.TryGetValue(iDay, out sDayTasks))
                return;
            if(string.IsNullOrEmpty(sDayTasks))
                return;
            string phraseToAdd = managerReportPhraseToAdd;
            if (!string.IsNullOrEmpty(phraseToAdd) && !string.IsNullOrEmpty(sDayTasks))
            {
                string lastCh = sDayTasks.Substring(sDayTasks.Length - 1, 1);
                if (".,;!?".IndexOf(lastCh) == -1)
                    sDayTasks += ". ";
                sDayTasks += phraseToAdd;
            }
            sheet1.GetRow(iMondayRow + iDay).GetCell(5).SetCellValue(sDayTasks);

        }

        void CheckManagerReport()
        {
            if (!generateManagerReport)
                return;
            //
            // check if manager report exists, and if no create it from template
            //

            Assembly _assembly = Assembly.GetExecutingAssembly();
            BinaryReader _reader;

            if (string.IsNullOrEmpty(managerFolder) || !Directory.Exists(managerFolder))
                return;
            mngrReportFileToMove = GetFileNameToMove(managerFolder, null);
            if (string.IsNullOrEmpty(mngrReportFileToMove))
            {
                mngrReportFileToMove = Path.Combine(managerFolder, ManagerFileName());
            }
            
            if (File.Exists(mngrReportFileToMove))
                return;
            _reader = new BinaryReader(_assembly.GetManifestResourceStream("PrepareReport.ReportTemplateFile.xls"));
            int len = (int)_reader.BaseStream.Length;
            byte[] body = new byte[len];
            len = _reader.Read(body, 0, (int)len);
            using (BinaryWriter binWriter =
                       new BinaryWriter(File.Open(mngrReportFileToMove, FileMode.Create)))
            {
                binWriter.Write(body, 0, len);
            }

        }

        bool CheckFolders(string src, string trg)
        {
            if (!Directory.Exists(src) || !Directory.Exists(trg))
            {
                System.Windows.MessageBox.Show(
                    string.Format("Please verify that both folders exist:\nSource: {0}\nTarget: {1}", 
                    src, trg));
                return false;
            }
            return true;
        }

        public bool MoveReportFrom(string path)
        {
            string reportFileToMove = GetFileNameToMove(path, null);
            if (string.IsNullOrEmpty(reportFileToMove))
            {
                // nothing todo
                return true;
            }
            if (!File.Exists(reportFileToMove))
            {
                // nothing todo
                MessageSender(string.Format("Cannot find the File {0}, skipped...\n", reportFileToMove));
                return true;
            }
            string targetDeveloperFile = Path.Combine(trgFolder, Path.GetFileName(reportFileToMove));
            if (!File.Exists(targetDeveloperFile))
            {
                File.Copy(reportFileToMove, targetDeveloperFile);
                MessageSender(string.Format("Copy File {0}\n", reportFileToMove));
                nReprotsMoved++;
                MovedReportFiles.Add(targetDeveloperFile);
            }
            else
            {
                MessageSender(string.Format("File {0} already exists\nFile {1} was not copied\n", targetDeveloperFile, reportFileToMove));
            }
            // AN:1 here we extract all reported tasks from specified report, that was already moved
            ExtractTasks(targetDeveloperFile);

            return true;
        }

        private bool IsOldFileToArchive(string path)
        {
            string name = Path.GetFileName(path);
            Match m1 = Regex.Match(name, @"^(\d{4}-\d{2}-\d{2})_(TasksListByDay|TasksListByDeveloper).txt",RegexOptions.IgnoreCase);
            if (m1.Success)
            {
                DateTime dt = utls.DateFromStrEnUs(m1.Groups[1].Value);
                if(dt == DateTime.MinValue)
                    return false;
                if (dt <= dtMinExpected)
                    return true;
            }

            // check, possibly we have user report?
            // like AC.CleverCAD.Timebooking report 2013-12-22.xls
            m1 = Regex.Match(name, @"^\w{2,4}.CleverCAD.Timebooking report (\d{4}-\d{2}-\d{2}).xls[x]?",RegexOptions.IgnoreCase);
            if (m1.Success)
            {
                DateTime dt = utls.DateFromStrEnUs(m1.Groups[1].Value);
                if (dt == DateTime.MinValue)
                    return false;
                if (dt <= dtMinExpected)
                    return true;
            }
            return false;
        }


        public string GetFileNameToMove(string path, string userName)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            string userReporter = (string.IsNullOrEmpty(userName)) ? Path.GetFileName(path) : userName;
            userReporter = userReporter.Replace("_", "");
            List<string> files = new List<string>();
            foreach (System.IO.FileInfo f in dir.GetFiles("*.*"))
            {
                string fileName = f.Name.ToUpper();
                if(fileName.StartsWith(string.Format("{0}.CLEVERCAD.TIMEBOOKING REPORT", userReporter)))
                {
                    if(IsThisWeekFile(fileName))
                        files.Add(fileName);
                }
            }
            if (files.Count == 1)
            {
                return Path.Combine(path, files[0]);
            }
            if (files.Count > 1)
            { 
                // Ask about file to move...
                string strFiles = string.Join("\n", files.ToArray());
                System.Windows.MessageBox.Show("Multiple files are found for this week:" +
                    strFiles + "\nPlease remove extra files before running this command.");
                return "";
            }
            path = Path.Combine(path, "Archive");
            if (!Directory.Exists(path))
                return "";

            return GetFileNameToMove(path, userReporter);
        }

        static List<string> GetThisWeekFiles(List<string> files)
        {
            var res = files.Where(l => IsThisWeekFile(l)).ToList();
            return res;
        }

        static bool IsThisWeekFile(string file)
        {
            Match m = Regex.Match(file, @".Timebooking report \d{4}-\d{2}-\d{2}", RegexOptions.IgnoreCase);
            if (!m.Success)
                return false;
            return IsThisWeekDate(m.Value.Substring(20));
        }

        static bool IsThisWeekDate(string dateStr)
        {
            DateTime dateToTest = utls.DateFromStrEnUs(dateStr);
            if (dateToTest <= dtMaxExpected && dateToTest > dtMinExpected)
                return true;
            return false;
        }

        static string ManagerFileName()
        {
            return string.Format(rptManagerFileNameTemplate, dtMaxExpected.ToString("yyyy-MM-dd"));
        }

        void ExtractTasks(string path)
        {
            // Extract tasks from developer report file
            InitializeWorkbook(path);
            string fileName = Path.GetFileName(path);
            devPrefix = GetStringItem(fileName, 0, '.');
            var sheet1 = hssfworkbook.GetSheetAt(0);
            //sheet1.GetRow(1).CreateCell(1).SetCellValue(200);
            var cell1 = sheet1.GetRow(13).GetCell(5);
            string s = cell1.StringCellValue;
            int iRow = FindMonday(sheet1);
            GetTaskList(sheet1, iRow);

        }

        bool IsTaskToIgnore(string taskText)
        {
            if (string.IsNullOrEmpty(taskText))
                return true;

            if (taskText.Length <= maxLengOfTaskToIgnore)
            {
                taskText = taskText.ToLower();
                foreach (string s in tasksToIgnore)
                { 
                    if(taskText.Contains(s.ToLower()))
                        return true;
                }
            }
            return false;
        }

        void GetTaskList(ISheet sheet1, int iRow)
        {
            for (int i = iRow; i < iRow + 6; i++)
            {
                var cell1 = sheet1.GetRow(i).GetCell(5);
                string s = CleanTaskText(cell1.StringCellValue);
                if (IsTaskToIgnore(s))
                    continue;
                string sTasks = "";
                DeveloperToTasks.TryGetValue(devPrefix, out sTasks);
                if (!string.IsNullOrEmpty(sTasks))
                {
                    sTasks += ";\r\n";
                }
                sTasks += devPrefix + ", " + s;
                DeveloperToTasks[devPrefix] = sTasks;
                sTasks = "";
                dayToTasks.TryGetValue(i - iRow, out sTasks);
                //strWoHours = Regex.Replace(strWoHours, @"([.] *(,| +|;)[.])|([,.] *;)", ".");
                //strWoHours = Regex.Replace(strWoHours, @"(, ;)|([.];)|( ;)", ";");
                //strWoHours = Regex.Replace(strWoHours, @"( ,)", ",");
                if (!string.IsNullOrEmpty(sTasks))
                {
                    sTasks += "; ";
                }
                sTasks += devPrefix + ", " + s;
                dayToTasks[i - iRow] = sTasks;
            }
        }

        string CleanTaskText(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            
            str = RemoveQuatMarksAndBlanks(str); 
            // old expression was ((\d{1}|\d{2}) *(h|hour|hours|h)(;|,|$| |\n|\.))|(\((\d{1}|\d{2})h\))
            string strWoHours = Regex.Replace(str, @"[(]*(\d+[.]\d+|\d[.]|\d{2}|\d{1}) *(hours|hour|h|mins|min|m)[)]*", "");

            // remove \n with blanks in strings like CCAD-12345\n \nSome text, replacing them by blank
            // for this we use lookaround ?<= (lookbefore) and ?= for look after
            strWoHours = Regex.Replace(strWoHours, @"(?<=\d)(\n[ ]*)+(?=\w)", " ");

            // remove all cr, nl characters
            strWoHours = strWoHours.Replace("\n", "; ");

            // remove combination of dots and other delimiters, replace them by dots
            strWoHours = Regex.Replace(strWoHours, @"([.] *(,| +|;)[.])|([,.] *;)", ".");
            strWoHours = Regex.Replace(strWoHours, @"(, ;)|([.];)|( ;)", ";");
            strWoHours = Regex.Replace(strWoHours, @"( ,)", ",");
            strWoHours = Regex.Replace(strWoHours, @"([ ][ ]+)", ", ");
            // ,, -> ,        ;, -> ,
            strWoHours = Regex.Replace(strWoHours, @"(,,)|(;,)|([.],)", ",");
            strWoHours = RemoveEndingDelimiter(strWoHours);
            return strWoHours;
        }

        string RemoveEndingDelimiter(string str)
        { 
            string delims = ".,; ";
            for (int i = str.Length - 1; i > 0; i--)
            { 
                string ch = str.Substring(i,1);
                if (delims.IndexOf(ch) < 0)
                {
                    return str.Substring(0, i + 1).Trim();
                }
            }
            return str;            
        }

        string RemoveQuatMarksAndBlanks(string str)
        {
            string s = str.Trim();
            if(string.IsNullOrEmpty(s))
                return "";
            if (s.Substring(0, 1) == "\"" && s.Substring(s.Length - 1, 1) == "\"")
                s = s.Substring(1, s.Length - 2);
            return s.Trim();
        }

        int FindMonday(ISheet sheet1) //HSSFSheet sheet1)
        {
            for (int i = 10; i < 21; i++)
            {
                var cell1 = sheet1.GetRow(i).GetCell(1);
                if (cell1 == null)
                    continue;
                string s = cell1.StringCellValue;
                if (s == "Monday")
                    return i;
            }
            return -1;
        }
        
        void InitializeWorkbook(string xlsFile)
        {
            FileStream file = new FileStream(xlsFile, FileMode.Open, FileAccess.Read);

            hssfworkbook = new HSSFWorkbook(file);

            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "NPOI SDK Example";
            hssfworkbook.SummaryInformation = si;
        }

        public static string GetStringItem(string line, int nItem, char delimiter)
        {
            string[] items = line.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            if (nItem < items.Length)
                return items[nItem];
            return string.Empty;
        }

        public void SaveTasksToFiles()
        {
            string sOut = "";
            foreach (string dvlp in DeveloperToTasks.Keys)
            {
                sOut += DeveloperToTasks[dvlp] + "\r\n";
            }
            string strDtMax = dtMaxExpected.ToString("yyyy-MM-dd");
            string tasksByDeveloper = string.Format("{0}_TasksListByDeveloper.txt", strDtMax);
            string tasksByDay = string.Format("{0}_TasksListByDay.txt", strDtMax);

            string outPath = Path.Combine(trgFolder, tasksByDeveloper);
            File.WriteAllText(outPath, sOut);

            sOut = "";
            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            foreach (int iDay in dayToTasks.Keys)
            {
                sOut += days[iDay] + "\r\n" + dayToTasks[iDay] + "\r\n";
            }
            string outPath2 = Path.Combine(trgFolder, tasksByDay);
            File.WriteAllText(outPath2, sOut);
        }

        public string DtMinMaxLine(DateTime datefrom, DateTime dateTo)
        {
            dtMinExpected = datefrom;
            dtMaxExpected = dateTo;
            return string.Format("Report Summary from: {0} to {1}"
                , dtMinExpected.ToString("yyyy-MM-dd"), dtMaxExpected.ToString("yyyy-MM-dd"));
        }

        public string GetSummary()
        {
            // Get list of files in range from dtMinExpected - dtMaxExpected
            List<string> filesToReport = new List<string>();
            GetFilesInRange(trgFolder, dtMinExpected, dtMaxExpected, filesToReport);
            GetFilesInRange(trgFolderArchive, dtMinExpected, dtMaxExpected, filesToReport);

            DeveloperRecordsContainer derContainer = new DeveloperRecordsContainer();
            foreach (string file in filesToReport)
            {
                derContainer.ProcessFile(file);
            }

            string res = derContainer.GetSummaryString(dtMinExpected, dtMaxExpected);

            res += "\n" + derContainer.GetSummaryDetails(dtMinExpected, dtMaxExpected);
            return res;
        }

        private void GetFilesInRange(string trgFolder, DateTime dtFrom, DateTime dtTo, List<string> filesToReport)
        {
            DirectoryInfo dir = new DirectoryInfo(trgFolder);
            List<string> files = new List<string>();
            foreach (System.IO.FileInfo f in dir.GetFiles("*.*"))
            {
                string fileName = f.Name.ToUpper();
                if (!IsMatchedFileName(fileName, dtFrom, dtTo))
                    continue;
                filesToReport.Add(f.FullName);
            }
        }

        private bool IsMatchedFileName(string fileName, DateTime dtFrom, DateTime dtTo)
        {
            Match m1 = Regex.Match(fileName, @"^[a-z]{2}.CleverCAD.Timebooking report \d{4}-\d{2}-\d{2}.XLS", RegexOptions.IgnoreCase);
            if (!m1.Success)
                return false;
            int skipToDate = "AN.CleverCAD.Timebooking report ".Length;
            string dtStr = fileName.Substring(skipToDate, 10);
            DateTime dtOfFile = utls.DateFromStrEnUs(dtStr);
            if (dtOfFile == DateTime.MinValue)
                return false;
            if (dtOfFile < dtFrom || dtOfFile.AddDays(-6) > dtTo)
                return false;
            return true;
        }

        public static (bool success, DateTime reportDate) ParseReportFileName(string fileName)
        {
            string reportFileNamePattern = @"^[a-zA-Z][a-zA-Z][.]CLEVERCAD[.]TIMEBOOKING REPORT (\d{4}-\d{2}-\d{2})[ ]?[.]XLS[X]?$";
            Regex rg = new Regex(reportFileNamePattern);
            var match = rg.Match(fileName.ToUpper());
            if (!match.Success)
                return (success: false, reportDate: DateTime.MinValue);
            var datePartStr = match.Groups[1].Value;
            DateTime rptDate = utls.DateFromStrEnUs(datePartStr); 
            if(rptDate == DateTime.MinValue)
                return (success: false, reportDate: DateTime.MinValue);
            return (success: true, reportDate: rptDate);
        }
        public static List<string> DiscoverLatestReports(string targetFolder)
        {
            List<string> discovered = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(targetFolder);
            string maxDate = "1900-01-01";
            DateTime dtMax = DateTime.MinValue;
            foreach (FileInfo f in dir.GetFiles("*.*"))
            {
                string fileName = f.Name.ToUpper();
                (bool success, DateTime reportDate) rptFileNameMatch = ParseReportFileName(f.Name);
                if (rptFileNameMatch.success)
                {
                    discovered.Add(f.FullName);
                    if (dtMax < rptFileNameMatch.reportDate) // potentially if you like in one line it could be
                        dtMax = rptFileNameMatch.reportDate; // dtMax = new[] { dtMax, rptFileNameMatch.reportDate }.Max(); 
                }
                //to do remove this
                /* 
                                var match = rg.Match(fileName);
                                if (match.Success)
                                {
                                    discovered.Add(f.FullName);
                                    var datePart = match.Groups[1].Value;
                                    maxDate = string.Compare(maxDate, datePart) < 0 ? datePart : maxDate;
                                }
                */
            }
            maxDate = dtMax.ToString("yyyy-MM-dd");
            var lastDated = discovered.Where(x => x.LastIndexOf(maxDate) > targetFolder.Length).ToList();
            return lastDated;
        }
    }
}
