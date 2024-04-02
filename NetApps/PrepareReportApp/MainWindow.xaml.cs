using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Windows.Controls.Ribbon;
using PrepareReport.Properties;
using PrepareReport.SubForms;
using PrepareReport.Utl;
using System.Text.RegularExpressions;
using Efa;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls;

namespace PrepareReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        //public static RoutedUICommand moveReportsCommand;
        //public static RoutedUICommand MoveReportsCommand
        //{
        //    get { return moveReportsCommand; }
        //}
        public static RoutedCommand MoveReportsCommand = new RoutedCommand();
        public ReportsMover rptMover = null;
        public FilesMoverOld fileMover = null;
        public InvoiceGenerator invoiceGenerator = null;
        public string invoicesConfigFile = "InvoicesGenerator.config";
        PaneSwitcher firstPaneSwitcher = null;
        public MainWindow()
        {
            InitializeComponent();
            if (firstPaneSwitcher == null)
            {
                firstPaneSwitcher = new PaneSwitcher(grid1);
            }

            //MoveReportsCommand = new RelayCommand(MoveReports);

            //moveReportsCommand = new RoutedUICommand("Move Reports", "MoveReports", typeof(MainWindow));
            //this.CommandBindings.Add(new CommandBinding(MoveReportsCommand));

            // Insert code required on object creation below this point.

            InitAppConfigInfo();

        }

        private void InitAppConfigInfo()
        {
            AppInfo.SourcePath = Settings.Default.SourceFolder;
            AppInfo.TargetPath = Settings.Default.TargetFolder;
            AppInfo.TargetPathArchive = Settings.Default.TargetFolderArchive;
            if (Settings.Default.IsTestMode)
            {
                AppInfo.SourcePath = Settings.Default.SourceFolderTest;
                AppInfo.TargetPath = Settings.Default.TargetFolderTest;
                AppInfo.TargetPathArchive = Path.Combine(AppInfo.TargetPath, "Archiv");
            }
            AppInfo.TargetPathInvoices = Path.Combine(AppInfo.TargetPath, Settings.Default.SubFolderInvoices);
            AppInfo.TargetPathInvoicesArchive = Path.Combine(AppInfo.TargetPathInvoices, "Archiv");
            AppInfo.FilesMoverTasksFolder = Settings.Default.FilesMoverTasksFolder;
            AppInfo.SummarizeReportFolder = Settings.Default.SummarizeReportFolder;
            AppInfo.FilesMoverTasks = Settings.Default.FilesMoverTasksToDo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public delegate void MsgToMainWnd(string msg);


        private void InitiateReportMover()
        {
            rptMover = new ReportsMover(AppInfo.SourcePath, AppInfo.TargetPath, AppInfo.TargetPathArchive,
                Settings.Default.ManagerFolder, Settings.Default.ManagerReportPhraseToAdd, 
                Settings.Default.GenerateManagerReport, Settings.Default.IsToSaveTasksToFiles);
            rptMover.ReportMoverEvent += rptMover_ReportMoverEvent;

        }

        void rptMover_ReportMoverEvent(object sender, ReportMoverEventArgs e)
        {
            ReportMoverEventArgs args = (ReportMoverEventArgs)e;
            if (args.errType == ErrorType.reportNotFound)
            {
                e.handlingResult = ReportEventHandlingResult.resContinue;
                return;
            }
            else if (args.errType == ErrorType.severalReportsNotFound)
            {
                if (!ResolveMissedReportsProblem())
                {
                    e.handlingResult = ReportEventHandlingResult.resStop;
                    return;
                }
                e.handlingResult = ReportEventHandlingResult.resContinue;
            }
        }

        MsgToMainWnd dlgMsg = null; 
        private void MoveReports(object sender, RoutedEventArgs e)
        {
            dlgMsg = AddMessage;

            // this part is temp for debug - just comment
            /*
            List<string> reportFilesTest = new List<string>(){
                @"F:\Work\Temp\PrepareReportTest\Src\AD\AD.CleverCAD.Timebooking report 2021-11-14.xls",
                @"F:\Work\Temp\PrepareReportTest\Src\VM\VM.CleverCAD.Timebooking report 2021-11-14.xls",
                @"F:\Work\Temp\PrepareReportTest\Src\VR\VR.CleverCAD.Timebooking report 2021-11-14.xls"
            };
            var invoiceGenerator1 = new InvoiceGenerator(reportFilesTest);
            var dtSunday1 = utls.NearestSunday(DateTime.Now);

            invoiceGenerator1.GenerateInvoices(dtSunday1);
            // this part above is temp for debug - just comment
            */

            TextSelection a = logBox.Selection;
            string t = a.Text;
            InitiateReportMover();
            // AN:1 date of files to send 
            string strDtMax = rptMover.DtMaxExpected();

            // AN:1 before moving here we check if all files are ok and no conflicts
            //

            grid1.Visibility = System.Windows.Visibility.Visible;
            grid2.Visibility = System.Windows.Visibility.Collapsed;

            bool reportsToMoveOK = rptMover.CheckBeforeMoveReports(dlgMsg);
            if (!reportsToMoveOK)
            {
                dlgMsg(string.Format("\nFailed to validate the reports-to-move\nDetails: {0}\n", 
                    rptMover.ErrorMsg));
                // call a dialog to resolve problem?
                return;
            }

            fileMover = new FilesMoverOld(Settings.Default.TargetFolder);

            if (Settings.Default.IsToDoFileMoverTasks)
            {
                if (!DoFileMoverChecks())
                    return;
            }

            if (reportsToMoveOK)
                rptMover.MoveReports(dlgMsg);
            dlgMsg(string.Format("\n{0} reports moved.\n", rptMover.nReprotsMoved));

            if (rptMover.nReprotsMoved == 0)
            {
                if (!WndUtl.AskYN("No reports were moved! Do you want to continue?"))
                    return;
            }
            DateTime reportDate = DateTime.MinValue;
            if (Settings.Default.IsToGenerateInvoices)
            {
                List<string> reportFiles = new List<string>(rptMover.MovedReportFiles);
                if (reportFiles.Count == 0)
                {
                    reportFiles = ReportsMover.DiscoverLatestReports(AppInfo.TargetPath);
                }
                if (reportFiles.Count == 0)
                {
                    dlgMsg("\nNo reports found.\nCheck your source and target folders\n"+
                        $"Source: {AppInfo.SourcePath}\nTarget: {AppInfo.TargetPath}");
                    return;
                }
                var nextSunday = utls.NearestSunday(DateTime.Now);
                string fileName = Path.GetFileName(reportFiles[0]);
                var rptDate = ReportsMover.ParseReportFileName(fileName);
                if (nextSunday != rptDate.reportDate)
                {
                    string msg = $"The latest reports are dated: {utls.StrDate(rptDate.reportDate)}, expected: {utls.StrDate(nextSunday)}.\nDo you want to proceed?";
                    if (!WndUtl.AskYN(msg))
                    {
                        return;
                    }
                }
                reportDate = rptDate.reportDate;

                var invoiceGenerator = new InvoiceGenerator(reportFiles);
                invoiceGenerator.GenerateInvoices(reportDate);

                dlgMsg($"{invoiceGenerator.InvoicesGeneratedMessage}");
            }

            string messageToAddInClipboard = Settings.Default.MessageToAddInClipboard.Replace("{Date}", strDtMax);
            string s = messageToAddInClipboard.Trim(' ', '\n', '\r');
            System.Windows.Clipboard.SetText(messageToAddInClipboard);

            dlgMsg("User message is copied to clipboard\r\n");
            dlgMsg(messageToAddInClipboard);

            //ReportsMover.ArchiveOld();
            //InvoiceGenerator.ArchiveOld();
            return;
        }

        private bool DoFileMoverChecks()
        {
            // Now move files specified by file-mover task
            List<FileInfo> listOfTasks = FilesCopyMover.GetListOfTasks(AppInfo.FilesMoverTasksFolder);
            if (!fileMover.CheckTasksBeforeDoTasks(listOfTasks))
            {
                string msg = string.Format("\nFailed to validate the file-move tasks\nDetails: {0}\n", fileMover.ErrorMsg);
                dlgMsg(msg);
                string msgToAsk = string.Format("FilesMover: {0}\n{1}", msg, "Do you want to resolve files mover problem?");
                if (!WndUtl.AskYN(msgToAsk))
                    return false;
                if (!ResolveFileMoverProblem())
                    return false;
            }
            if (listOfTasks.Count < 1)
            {
                return true;
            }
            foreach (var taskPath in listOfTasks)
            {
                if (!fileMover.DoTask(taskPath.FullName))
                {
                    dlgMsg(string.Format("\nFailed to execute the file-move task {0}.\nDetails: {1}\n", taskPath, fileMover.ErrorMsg));
                }
                else
                {
                    dlgMsg(fileMover.InfoMsg);
                }
            }

            return true;
        }

        private void MoveReportsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void AddMessage(string msg)
        {
            string msg1 = msg.Replace("\r\n", "\r");
            msg = msg1.Replace("\n", "\r");
            logBox.AppendText(msg);
        }
        private void BtnMoveReports_Click(object sender, RoutedEventArgs e)
        {
        }

        private void showTasks_Click(object sender, RoutedEventArgs e)
        {
            if (grid1.Visibility == System.Windows.Visibility.Visible)
            {
                firstPaneSwitcher.Show1("grid2");
                DrawRectangles(10);
            }
            else
            {
                firstPaneSwitcher.Show1("grid1");
            }
        }

        private void btnJoinTxtFiles_Click(object sender, RoutedEventArgs e)
        {
            var filesToJoinParams = new WndAskJoinParams();
            bool? isOk = filesToJoinParams.ShowDialog();
            if (isOk != true)
            {
                return;
            }
            List<FileInfo> listOfFiles = utls.BuildListOfAllFiles(filesToJoinParams.SrcFolder, true);
            utls.JoinTextFiles(listOfFiles, filesToJoinParams.TargetFilePath);
            if (File.Exists(filesToJoinParams.TargetFilePath))
            {
                string msg = string.Format("{0} Files were joint to the file {1}", listOfFiles.Count, filesToJoinParams.TargetFilePath);
                System.Windows.MessageBox.Show(msg);
            }

        }

        private void btnCheckDuplicateFiles_Click(object sender, RoutedEventArgs e)
        {
            MsgToMainWnd dlgMsg = AddMessage;
            EqualFilesAnalizer efa = new EqualFilesAnalizer();
            WndAskJoinParams filesToJoinParams = new WndAskJoinParams();
            bool? isOk = filesToJoinParams.ShowDialog();
            if (isOk != true)
            {
                return;
            }
            bool bIncludeSubfolder = true;
            bool onlyDupplicates = true;
            efa.Analyze(filesToJoinParams.SrcFolder, filesToJoinParams.TargetFilePath, bIncludeSubfolder, onlyDupplicates);
            string s = efa.ResultsOfAnalysis();
            dlgMsg(s);
        }

        private void ZipReports(object sender, ExecutedRoutedEventArgs e)
        {
            DateTime rptDate = utls.NearestSunday(DateTime.Now);
            string rptDateStr = utls.StrDate(rptDate);
            string zipFileName = rptDateStr + ".zip";
            List<string> reportsAndInvoices = DiscoverLatestReportsAndInvoices();
            var divDateFiles = reportsAndInvoices.Where(x => !x.Contains(rptDateStr)).ToList();
            if (divDateFiles.Count > 0)
            {
                string divFiles = string.Join("\n", divDateFiles);
                string msg = "The files of unexpected dates are discovered!\n" +
                    $"Expected date: {rptDateStr}\n" +
                    $"Actual discovered files of other dates:\n{divFiles}\n"+
                    "Do you want to proceed?";
                if (!WndUtl.AskYN(msg))
                    return;
            }

            // zip all report files and invoices
            string outputArchive = Path.Combine(AppInfo.TargetPath, zipFileName);
            Tuple<bool, string> res = utls.ZipToArchive(reportsAndInvoices, outputArchive, Settings.Default.PasswordForZip);
            if (res.Item1 == true)
            {
                AddMessage(string.Format("\r\nArchive file created: {0}", outputArchive));
            }
            else
            {
                AddMessage(string.Format("\r\nFailed to create an archive file: {0}\n{1}", outputArchive, res.Item2));
            }
            return;
        }

        private List<string> DiscoverLatestReportsAndInvoices()
        {
            List<string> discovered = ReportsMover.DiscoverLatestReports(AppInfo.TargetPath);
            List<string> discoveredInvoices = InvoiceGenerator.DiscoverLatestInvoices(AppInfo.TargetPathInvoices);

            discovered.AddRange(discoveredInvoices);
            return discovered;
        }

        private void ZipReportsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
//            e.CanExecute = bMoveReportDone;
            e.CanExecute = true;
        }

        private bool ResolveMissedReportsProblem()
        {
            var ctrlMissedReportFolders = new HelperCtlReportMover(rptMover.directoriesNoReportsFound);
            ProblemCheckWnd problemCheck = new ProblemCheckWnd();
            problemCheck.CtlPlaceGrid.Children.Add(ctrlMissedReportFolders);

            // Show the folder of the first missing report
            
            bool? isOk = problemCheck.ShowDialog();
            if (isOk != true)
            {
                return false;
            }
            foreach (var folderResolved in ctrlMissedReportFolders.MissedFoldersSelectedAsDone)
            {
                string filePath = rptMover.GetFileNameToMove(folderResolved.Name, null);
                if(!string.IsNullOrEmpty(filePath))
                    rptMover.MovedReportFiles.Add(filePath);
            }

            return true;
        }

        private bool ResolveFileMoverProblem()
        {

            var ctrlMovedFiles = new HelperCtlFileMover(fileMover.FailedToFolder);
            ProblemCheckWnd problemCheck = new ProblemCheckWnd();
            problemCheck.CtlPlaceGrid.Children.Add(ctrlMovedFiles);


            bool? isOk = problemCheck.ShowDialog();
            if (isOk != true)
            {
                return false;
            }
            List<string> filesMarkedAsMoved = new List<string>();
            foreach(var moved in ctrlMovedFiles.movedFilesSelectedAsDone)
            {
                string path = System.IO.Path.Combine(fileMover.FailedToFolder, moved.Name);
                filesMarkedAsMoved.Add(path);
            }
            fileMover.MarkAsMovedOK(filesMarkedAsMoved);

            return true;
        }

        private void RegExpTester(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void RegExpTesterCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SummarizeReports(object sender, ExecutedRoutedEventArgs e)
        {
            InitiateReportMover();
            DateTime dtNow = DateTime.Now;

            DateTime dtPrevMonth = dtNow.AddMonths(-1);
            DateTime dtFrom = new DateTime(dtPrevMonth.Year, dtPrevMonth.Month, 1);
            DateTime dtTo = dtFrom.AddMonths(1).AddDays(-1);
            string strDtMax = rptMover.DtMinMaxLine(dtFrom, dtTo);
            string strSummary = rptMover.GetSummary();
            AddMessage(string.Format("\n{0}\n{1}\n", strDtMax, strSummary));
            if(AppInfo.SummarizeReportFolder != "")
            {
                // save summary to folder
                string sMonth = dtPrevMonth.Month.ToString("00");
                string fileName = $"{dtPrevMonth.Year}-{sMonth} Efforts.txt";
                string filePpath1 = Path.Combine(AppInfo.SummarizeReportFolder, fileName);
                File.WriteAllText(filePpath1, strSummary);
            }
        }

        private void SummarizeReportsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void OpenReprotsFolderCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenReprotsFolder(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(AppInfo.TargetPath);
        }

        private void BtnArchiveOldReportInvoicesFiles_Click(object sender, RoutedEventArgs e)
        {
            var nextSunday = utls.NearestSunday(DateTime.Now);
            List<string> oldeReports = utls.MatchingFiles(AppInfo.TargetPath, x => IsReportFileName(x) && ReportFileDate(x) < nextSunday);
            List<string> olderInvoices = utls.MatchingFiles(AppInfo.TargetPathInvoices, x => IsInvoiceFile(x) && InvoiceFileDate(x) < nextSunday);
            FilesMoverOld.MoveFiles(oldeReports, AppInfo.TargetPathArchive);
            FilesMoverOld.MoveFiles(olderInvoices, AppInfo.TargetPathInvoicesArchive);
            string msg = $"Moved to archive\nReports: { oldeReports.Count}, Invoices: { olderInvoices.Count}";
            if(oldeReports.Count>0)
                msg += $"\nReports list:\n" + string.Join("\n", oldeReports);
            if (olderInvoices.Count > 0)
                msg += $"\nInvoices list:\n" + string.Join("\n", olderInvoices);

            AddMessage(msg+"\n");
        }

        private DateTime InvoiceFileDate(string fileName)
        {
            return InvoiceGenerator.ParseInvoiceFile(fileName).DateOfInvoice;
        }

        private bool IsInvoiceFile(string fileName)
        {
            return InvoiceGenerator.ParseInvoiceFile(fileName).InvoiceType != InvoiceType.Unknown;
        }

        private bool IsReportFileName(string fileName)
        {
            var rptParseRes = ReportsMover.ParseReportFileName(fileName).success;
            return rptParseRes;
        }

        private DateTime ReportFileDate(string fileName)
        {
            var rptParseRes = ReportsMover.ParseReportFileName(fileName).reportDate;
            return rptParseRes;
        }

        private void BtnFilesMoverTasks_Click(object sender, RoutedEventArgs e)
        {
            var fcm = new FilesCopyMover(AppInfo.FilesMoverTasksFolder);
            fcm.ExecuteTasks();
            if (!string.IsNullOrEmpty(fcm.ErrorMessage))
            {
                AddMessage(fcm.ErrorMessage);
            }
            AddMessage(fcm.Message);
        }

        private void DrawRectangles(int sizeHorisontal)
        {
            // todo: remove or move to another proj...
            if(sizeHorisontal>-111111) return;
            var topOfBtn = Canvas.GetTop(playBtn);
            int rectSize = 50;
            int delta = 10;
            for (int i = 0; i < sizeHorisontal; i++)
            {
                for (int j = 0; j < sizeHorisontal; j++)
                {
                    int xPos = i * rectSize;
                    int yPos = j * rectSize;
                    if (i > 0)
                    {
                        xPos += i * delta;
                    }
                    if(j>0)
                        yPos += j * delta;

                    DrawRectangle1(xPos, yPos);
                }
            }
        }

        private void DrawRectangle1(int x, int y)
        {
            
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.Fill = new SolidColorBrush(Colors.Black);
            rect.Width = 50;
            rect.Height = 50;
            Canvas.SetLeft(rect, 10+x);
            Canvas.SetTop(rect, 50 + y);
            canvasToDraw.Children.Add(rect);
        }
    }
}
