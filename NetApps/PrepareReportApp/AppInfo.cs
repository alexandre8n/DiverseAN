using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrepareReport
{
    public class AppInfo
    {
        // supported file patterns keywords
        public const string strEndOfThisWeekDate = "{EndOfThisWeekDate}";
        public const string strEndOfThisWeekDateYYMMDD = "{EndOfThisWeekDateYYMMDD}";
        public const string strTodayDate = "{TodayDate}";
        public const string strEndOfThisMonthDate = "{EndOfThisMonthDate}";
        public const string strYearWeekNumber = "{YearWeekNumber}";

        public static string SourcePath;
        public static string TargetPath;
        public static string TargetPathArchive;
        public static string TargetPathInvoices;
        public static string TargetPathInvoicesArchive;
        public static string FilesMoverTasksFolder;
        public static string SummarizeReportFolder;
        public static List<string> FilesMoverTasks;
    }
}
