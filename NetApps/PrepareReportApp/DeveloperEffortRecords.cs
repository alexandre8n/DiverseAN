using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PrepareReport.Utl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrepareReport
{
    public class DeveloperEffortRecords
    {
        public string Name;
        List<EffortRecord> effortRecords = new List<EffortRecord>();

        public DeveloperEffortRecords(string name)
        {
            Name = name;
        }
        public void AddRec(EffortRecord er)
        {
            effortRecords.Add(er);
        }

 
        public double CalcTotal(DateTime dtFrom, DateTime dtTo)
        {
            double sum = 0;
            foreach (var rec in effortRecords)
            {
                if (rec.date < dtFrom || rec.date > dtTo)
                    continue;
                sum += rec.effort;
            }
            return sum;
        }

        public void Clear()
        {
            effortRecords.Clear();
        }

        public void ProcessFileExtractEffrotRecords(string filePath)
        {
            var workBook = InitializeWorkbook(filePath);
            DateTime fileDate = GetDateFromFileName(filePath);
            if (fileDate == DateTime.MinValue || fileDate.DayOfWeek != DayOfWeek.Sunday)
                throw new Exception($"Incorrect file name format: {Path.GetFileNameWithoutExtension(filePath)}");
            var sheet1 = workBook.GetSheetAt(0);
            var cell1 = sheet1.GetRow(13).GetCell(5);
            string s = cell1.StringCellValue;
            DateTime lastMonday = fileDate.Add(new TimeSpan(-6, 0, 0, 0));
            int iRow = FindMonday(sheet1);
            GetTaskList(sheet1, iRow, lastMonday);
        }

        private DateTime GetDateFromFileName(string filePath)
        {
            string file = Path.GetFileNameWithoutExtension(filePath);
            Match m = Regex.Match(file, @".Timebooking report (\d{4}-\d{2}-\d{2})", RegexOptions.IgnoreCase);
            if (!m.Success)
                return DateTime.MinValue;
            string dateEnd = m.Groups[1].Value;
            return utls.DateFromStrEnUs(dateEnd);
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

        public static HSSFWorkbook InitializeWorkbook(string xlsFile)
        {
            FileStream file = new FileStream(xlsFile, FileMode.Open, FileAccess.Read);
            HSSFWorkbook hssfworkbook;
            hssfworkbook = new HSSFWorkbook(file);

            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "NPOI SDK Example";
            hssfworkbook.SummaryInformation = si;
            return hssfworkbook;
        }

        void GetTaskList(ISheet sheet1, int iRow, DateTime lastMonday)
        {
            int iDay = 0;
            for (int i = iRow; i < iRow + 7; i++)
            {
                //var cellDate = sheet1.GetRow(i).GetCell(3);
                var cellEfforts = sheet1.GetRow(i).GetCell(4);
                var cell1 = sheet1.GetRow(i).GetCell(5);
                EffortRecord er = new EffortRecord();
                //er.date = cellDate.DateCellValue;
                er.date = lastMonday.AddDays((double)iDay);
                iDay++;
                if (cellEfforts.CellType == CellType.Numeric)
                {
                    er.effort = cellEfforts.NumericCellValue;
                }
                else if (cellEfforts.CellType == CellType.Formula && cellEfforts.CachedFormulaResultType == CellType.Numeric)
                {
                    er.effort = cellEfforts.NumericCellValue;
                }
                else 
                {
                    er.effort = 0.0;
                }
                er.description = cell1.StringCellValue;
                effortRecords.Add(er);
            }
        }

        internal string GetDevEffortSummary(DateTime dtFrom, DateTime dtTo)
        {
            string sDevSummary = Name + ", " + CalcTotal(dtFrom, dtTo).ToString();
            return sDevSummary;
        }

        internal string GetSummaryDetails(DateTime dtFrom, DateTime dtTo)
        {
            string sDevSummary = "Name: " + Name + ", Total Efforts: " + CalcTotal(dtFrom, dtTo).ToString() + "\n";
            foreach (var rec in effortRecords.OrderBy(x => x.date))
            {
                if (rec.date < dtFrom || rec.date > dtTo)
                    continue;
                sDevSummary += string.Format("{0} Effort: {1}\n", rec.date.ToString("yyyy-MM-dd"), rec.effort);
            }
            return sDevSummary;
        }
    }

    public class DeveloperRecordsContainer
    {
        Dictionary<string, DeveloperEffortRecords> container = new Dictionary<string, DeveloperEffortRecords>();

        internal void ProcessFile(string file)
        {
            string devName = DeveloperNameFromFileName(file);
            DeveloperEffortRecords devEffRecs = null;
            if (container.ContainsKey(devName))
            {
                devEffRecs = container[devName];
            }
            else
            {
                devEffRecs = new DeveloperEffortRecords(devName);
                container[devName] = devEffRecs;
            }
            devEffRecs.ProcessFileExtractEffrotRecords(file);
        }

        static public string DeveloperNameFromFileName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath).Substring(0, 2).ToUpper();
        }
        public double TotalEffortForDeveloper(string devName)
        {
            if (!container.ContainsKey(devName))
                return 0.0;
            return container[devName].CalcTotal(DateTime.MinValue, DateTime.MaxValue);
        }

        internal string GetSummaryString(DateTime dtFrom, DateTime dtTo)
        {
            string sRes = "";
            foreach (var dev in container.Values)
            {
                sRes += dev.GetDevEffortSummary(dtFrom, dtTo) + "\n";
            }
            return sRes;
        }

        internal string GetSummaryDetails(DateTime dtFrom, DateTime dtTo)
        {
            string SummaryDetails = "";
            foreach (var dev in container.Values)
            {
                string sDevSummary = dev.GetSummaryDetails(dtFrom, dtTo) + "\n";
                SummaryDetails += sDevSummary;
            }

            return SummaryDetails;
        }
    }
    public class EffortRecord
    {
        public DateTime date;
        public double effort;
        public string description;
    }
}
