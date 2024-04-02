using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace PrepareReport.Utl
{
    public enum InvoiceType 
    { 
        Unknown = 0,
        Company = 1,
        Manager = 2
    }
    public partial class InvoiceGenerator
    {
        public const string xlsFileExtension = ".xlsx";

        public string targetPathBase = "";
        public string invoicePath = "";
        public string invoiceArchivePath = "";
        public string CompanyInvoiceNamePattern = "NG.SoftwareAces.{EndOfThisWeekDate} Invoice {EndOfThisWeekDateYYMMDD}-01";
        public string ManagerInvoiceNamePattern = "AN.SoftwareAces.{EndOfThisWeekDate} Invoice-{YearWeekNumber}";
        public int YearWeekNumberCorrection = 1;

        public Dictionary<string, double> devHours = new Dictionary<string, double>();
        public Dictionary<string, double> devRates = new Dictionary<string, double>();
        public List<string> reportFiles = null;
        public string InvoicesGeneratedMessage;

        const string companyInvoicePattern = @"^[a-zA-Z][a-zA-Z][.]SoftwareAces[.](\d{4}-\d{2}-\d{2})[ ]Invoice \d{6}-\d{2}[.]XLS[X]?$";
        const string managerInvoicePattern = @"^[a-zA-Z][a-zA-Z][.]SoftwareAces[.](\d{4}-\d{2}-\d{2})[ ]Invoice-\d{3}[.]XLS[X]?$";
        private List<string> filesOfReports;
        public string managerInvoiceFilePath;
        public string companyInvoiceFilePath;
        public InvoiceGenerator(List<string> filesOfReports)
        {
            this.filesOfReports = filesOfReports;
            this.targetPathBase = AppInfo.TargetPath;
            invoicePath = AppInfo.TargetPathInvoices;
            invoiceArchivePath = AppInfo.TargetPathInvoicesArchive;
        }

        public void GenerateInvoices(DateTime reportDate)
        {
            InvoicesGeneratedMessage = "Effort of developers for invoices:\n";
            if (filesOfReports.Count < 1)
            {
                InvoicesGeneratedMessage += $"No reports are discovered and processed\r\n";
                return;
            }
            //1) get list of total hours for every developer from reports
            foreach (string rptFile in filesOfReports)
            {
                DeveloperRecordsContainer devRecCont = new DeveloperRecordsContainer();
                devRecCont.ProcessFile(rptFile);
                string devName = DeveloperRecordsContainer.DeveloperNameFromFileName(rptFile);
                devHours[devName] = devRecCont.TotalEffortForDeveloper(devName);
                InvoicesGeneratedMessage += $"{devName}: {devHours[devName]}\r\n";
            }
            //2) fill hours/amount lines of invoice NG.... save it
            FillCompanyInvoice(reportDate);

            //3) get total hours of all
            double totalDeveloperHours = devHours.Values.Sum();

            //4) fill total hours in manager invoice... save it
            FillManagerInvoice(totalDeveloperHours, reportDate);

            //5) generate summary text
            InvoicesGeneratedMessage += $"{devHours.Count} reports processed\n";
        }

        private void FillManagerInvoice(double totalDeveloperHours, DateTime reportDate)
        {
            string strDate = reportDate.ToString("yyyy-MM-dd");
            int iYearWeek = utls.GetYearWeekNumber(reportDate) + YearWeekNumberCorrection;
            string yearWeek = iYearWeek.ToString("D3");
            string fileNameTmp = ManagerInvoiceNamePattern.Replace(AppInfo.strEndOfThisWeekDate, strDate);
            string fileName = fileNameTmp.Replace(AppInfo.strYearWeekNumber, yearWeek);
            managerInvoiceFilePath = Path.Combine(invoicePath, fileName) + xlsFileExtension;
            if (File.Exists(managerInvoiceFilePath))
                File.Delete(managerInvoiceFilePath);

            bool savedOk = utls.LoadManifestResourceToFile("PrepareReport.ManagerInvoiceTemplate.xlsx", managerInvoiceFilePath);
            if (!savedOk)
            {
                throw new Exception($"Failed to save file {managerInvoiceFilePath}");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo file = new FileInfo(managerInvoiceFilePath);
            using (var excelPackage = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];
                FillInvoiceDetails(worksheet);
                excelPackage.Save();
                InvoicesGeneratedMessage += $"Invoice: {managerInvoiceFilePath} is generated\r\nTotal developer hours: {totalDeveloperHours}\r\n";

                /*ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];
                int iStartRow = FindRowNumber(worksheet, "A1", positionLinesHeader);
                if (iStartRow < 0)
                {
                    throw new Exception($"Failed to generate {managerInvoiceFilePath}\nIncorrect template format! Header {positionLinesHeader} is not found");
                }
                worksheet.Cells[iStartRow + 1, 1].Value = $"Clever CAD 1 - {devHours.Count}";
                worksheet.Cells[iStartRow + 1, 7].Value = totalDeveloperHours;

                excelPackage.Save();
                InvoicesGeneratedMessage += $"Invoice: {managerInvoiceFilePath} is generated\r\nTotal developer hours: {totalDeveloperHours}\r\n";
                */
            }
        }

        string positionLinesHeader = "Description";
        string ratesLineHeader = "[Rates]";
        private void FillCompanyInvoice(DateTime reportDate)
        {
            string strDate = reportDate.ToString("yyyy-MM-dd");
            string strDateYYMMDD = reportDate.ToString("yyMMdd");
            string fileNameTmp = CompanyInvoiceNamePattern.Replace(AppInfo.strEndOfThisWeekDate, strDate);
            string fileName = fileNameTmp.Replace(AppInfo.strEndOfThisWeekDateYYMMDD, strDateYYMMDD);
            companyInvoiceFilePath = Path.Combine(invoicePath, fileName) + xlsFileExtension;
            if (File.Exists(companyInvoiceFilePath))
                File.Delete(companyInvoiceFilePath);
            bool savedOk = utls.LoadManifestResourceToFile("PrepareReport.CompanyInvoiceTemplate.xlsx", companyInvoiceFilePath);
            if (!savedOk)
            {
                throw new Exception($"Failed to save file {companyInvoiceFilePath}");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo file = new FileInfo(companyInvoiceFilePath);
            using (var excelPackage = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];
                FillInvoiceDetails(worksheet);
                excelPackage.Save();
                InvoicesGeneratedMessage += $"Invoice: {companyInvoiceFilePath} is generated\r\n";
            }
        }

        private void FillInvoiceDetails(ExcelWorksheet worksheet)
        {
            int iStartRow = 1 + FindRowNumber(worksheet, "A1", positionLinesHeader);
            if (iStartRow <= 0)
            {
                throw new Exception($"Failed to generate {companyInvoiceFilePath}\nIncorrect template format! Header {positionLinesHeader} is not found");
            }

            int lineCount = devHours.Count;
            if (lineCount > 1)
                worksheet.InsertRow(iStartRow + 1, lineCount - 1);
            var devList = devHours.Keys.ToList();
            var hours = devHours.Values.ToList();
            devRates = GetDevRates(worksheet); // normally they are at K column.... see template

            // Assume the 1st position line is template to be copied to further positions
            for (int i = 0; i < devList.Count; i++)
            {
                string curDev = devList[i];
                var curHours = devHours[curDev];
                var curRate = devRates[curDev];
                worksheet.Cells[iStartRow + i, 1].Value = $"Clever CAD {curDev}";
                worksheet.Cells[iStartRow + i, 6].Value = curRate;
                worksheet.Cells[iStartRow + i, 7].Value = curHours;
                if (i > 0)
                {
                    worksheet.Cells[iStartRow, 8, iStartRow, 8].
                        Copy(worksheet.Cells[iStartRow + i, 8, iStartRow + i, 8]);
                }
            }
        }

        private Dictionary<string, double> GetDevRates(ExcelWorksheet worksheet)
        {
            Dictionary<string, double> devRates1 = new Dictionary<string, double>();
            int iStartRow = 1 + FindRowNumber(worksheet, "K1", ratesLineHeader);
            if(iStartRow <= 0)
            {
                throw new Exception($"Failed to generate {companyInvoiceFilePath}\nIncorrect template format! Rate Header {ratesLineHeader} is not found");
            }

            for (int i = 0; i < 1000; i++)
            {
                string devCellCode = $"K{iStartRow+i}";
                string rateCellCode = $"L{iStartRow+i}";
                var curDevVal = worksheet.Cells[devCellCode].Value;
                if (curDevVal == null)
                {
                    break;
                }
                var curDev = curDevVal.ToString();
                var curRate = worksheet.Cells[rateCellCode].Value;
                if (string.IsNullOrEmpty(curDev) || (double)curRate <= 0.00001)
                    break;
                devRates1[curDev] = (double)curRate;
            }
            return devRates1;
        }

        private int FindRowNumber(ExcelWorksheet worksheet, string startingColRow, string valueToFind)
        {
            string firstLetter = startingColRow.Substring(0, 1);
            int startingRow = int.Parse(startingColRow.Substring(1));
            for (int i = startingRow; i < startingRow + 1000; i++)
            {
                string cellCode = $"{firstLetter}{i}";
                var curRowVal = worksheet.Cells[cellCode].Value;
                if (curRowVal == null)
                    continue;
                string strCurRowVal = curRowVal.ToString();
                if (strCurRowVal == valueToFind)
                    return i;
            }
            return -1;
        }

        public static (InvoiceType InvoiceType, DateTime DateOfInvoice) ParseInvoiceFile(string fileName)
        {
            var companyRes = ParseCompanyInvoiceFile(fileName);
            if (companyRes.Success)
            { 
                return (InvoiceType: InvoiceType.Company, DateOfInvoice: companyRes.DateOfInvoice);
            }
            var managerRes = ParseManagerInvoiceFile(fileName);
            if (managerRes.Success)
            {
                return (InvoiceType: InvoiceType.Manager, DateOfInvoice: managerRes.DateOfInvoice);
            }
            return (InvoiceType: InvoiceType.Unknown, DateOfInvoice: DateTime.MinValue);
        }

        public static (bool Success, DateTime DateOfInvoice) ParseCompanyInvoiceFile(string fileName)
        {
            (bool Success, DateTime DateOfInvoice) res = (Success: false, DateOfInvoice: DateTime.MinValue);
            var match = Regex.Match(fileName, companyInvoicePattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return res;
            }
            res = (Success: true, DateOfInvoice: utls.DateFromStrEnUs(match.Groups[1].Value));
            return res;
        }
        public static (bool Success, DateTime DateOfInvoice) ParseManagerInvoiceFile(string fileName)
        {
            (bool Success, DateTime DateOfInvoice) res = (Success: false, DateOfInvoice: DateTime.MinValue);
            var match = Regex.Match(fileName, managerInvoicePattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return res;
            }
            res = (Success: true, DateOfInvoice: utls.DateFromStrEnUs(match.Groups[1].Value));
            return res;
        }

        internal static List<string> DiscoverLatestInvoices(string targetFolder)
        {
            //NG.SoftwareAces.2021-08-15 Invoice 210815-01.xlsx
            //AN.SoftwareAces.2021-08-15 Invoice-32.xlsx
            List<string> discovered = new List<string>();
            DirectoryInfo dir = new DirectoryInfo(targetFolder);
            string maxDate = "1900-01-01";
            foreach (FileInfo f in dir.GetFiles("*.*"))
            {
                string fileName = f.Name.ToUpper(); //[ ]Invoice \d{6}-d{2}[.]XLS[X]?$
                var match = Regex.Match(fileName, companyInvoicePattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    discovered.Add(f.FullName);
                    var datePart = match.Groups[1].Value;
                    maxDate = string.Compare(maxDate, datePart) < 0 ? datePart : maxDate;
                    continue;
                }
                var matchMngr = Regex.Match(fileName, managerInvoicePattern, RegexOptions.IgnoreCase);
                if (matchMngr.Success)
                {
                    discovered.Add(f.FullName);
                    var datePart = matchMngr.Groups[1].Value;
                    maxDate = string.Compare(maxDate, datePart) < 0 ? datePart : maxDate;
                }
            }
            var lastDated = discovered.Where(x => x.LastIndexOf(maxDate) > targetFolder.Length).ToList();

            return lastDated;
        }
    }
}