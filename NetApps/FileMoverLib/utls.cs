using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace ScriptRunnerLib
{
    public class FilePatternKeywords
    {
        // supported file patterns keywords
        public const string strEndOfThisWeekDate = "{EndOfThisWeekDate}"; // YYYY-MM-DD
        public const string strEndOfThisWeekDateYYMMDD = "{EndOfThisWeekDateYYMMDD}";
        public const string strTodayDate = "{TodayDate}";
        public const string strDate = "{Date}";                     // YYYY-MM-DD
        public const string strThisYear = "{ThisYear}";             // YYYY
        public const string strThisMonth = "{ThisMonth}";         // MM (01,...,12)
        public const string strEffYear = "{EffYear}";     // YYYY - if before 15th of Jan, Prev Year
        public const string strEffMonth = "{EffMonth}";   // MM (01,...,12), if before 15th day of month, take prev. month 
        public const string strThisMonthDate = "{ThisMonthDate}";   // YYYY-MM-dd
        public const string strEndOfThisMonthDate = "{EndOfThisMonthDate}";
        public const string strYearWeekNumber = "{YearWeekNumber}";
        public const string strExe = "{exe}";
        // day number of this month, to consider the date of interest to be last day of previous month.
        public const int daysCurToPrev = 15; 
    }

    public class utls
    {
        public const string sRegExp = "RegEx:";

        public static DateTime DateFromStrEnUs(string dateStr)
        {
            DateTime dateToTest;
            bool bTry = DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateToTest);
            if (!bTry)
            {
                return DateTime.MinValue;
            }
            return dateToTest;
        }
        public static string StrDate(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        static public List<string> MatchingFiles(string folder, Predicate<string> MatchOk)
        {
            DirectoryInfo dir = new DirectoryInfo(folder);

            List<string> fileFullPathes = new List<string>();
            foreach (FileInfo f in dir.GetFiles("*.*"))
            {
                if (MatchOk(f.Name))
                    fileFullPathes.Add(f.FullName);
            }
            return fileFullPathes;
        }

        static public List<FileInfo> BuildListOfAllFiles(string folder, bool scanSubFolders)
        {
            DirectoryInfo d = new DirectoryInfo(folder);
            List<FileInfo> list = new List<FileInfo>();
            return BuildListOfAllFiles4Directory(d, list, scanSubFolders);
        }

        static public List<FileInfo> BuildListOfAllFiles4Directory(DirectoryInfo dirToReadFrom, List<FileInfo> list, bool scanSubFolders)
        {
            // get all files
            FileInfo[] Files = dirToReadFrom.GetFiles("*.*");
            foreach (FileInfo file in Files)
            {
                list.Add(file);
            }

            if (!scanSubFolders)
                return list;

            DirectoryInfo[] dirInfoArray = dirToReadFrom.GetDirectories();
            foreach (DirectoryInfo d in dirInfoArray)
            {
                BuildListOfAllFiles4Directory(d, list, scanSubFolders);
            }
            return list;
        }

        static public bool JoinTextFiles(List<FileInfo> files, string pathToResultingFile)
        {
            // This text is added only once to the file.
            if (File.Exists(pathToResultingFile))
                return false;
            using (StreamWriter sw = File.AppendText(pathToResultingFile))
            {
                foreach (var file in files)
                {
                    string fileContent = ReadTextFile(file.FullName);
                    if (fileContent != null)
                    {
                        sw.WriteLine(fileContent);
                    }
                }
            }
            return true;
        }

        static public string ReadTextFile(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    String line = sr.ReadToEnd();
                    return line;
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static IEnumerable<string> ReadAllLinesFromTextFile(string fileName)
        {
            return File.ReadLines(fileName);
        }

        public static string ModifyFileNameToMakeItUniq(string filePath, string suffix)
        {
            string folder = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);
            string resPath = filePath;
            for (int i = 0; i < 10000; i++)
            {
                if (i > 0)
                {
                    resPath = Path.Combine(folder, string.Format("{0}{1}{2}{3}", fileName, suffix, i, ext));
                }
                if (!File.Exists(resPath))
                {
                    return resPath;
                }
            }
            return null;
        }

        // returns bool - true = ok, false = failed, and Error message in string, if a problem
        public static Tuple<bool, string> ZipToArchive(List<string> filePaths, string ZipFileToCreate, string pwd)
        {
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    if (!string.IsNullOrEmpty(pwd))
                        zip.Password = pwd;
                    int iFile = 1;
                    foreach (String filename in filePaths)
                    {
                        ZipEntry e = zip.AddFile(filename, "");
                        e.Comment = string.Format("File {0}", iFile++);
                    }

                    zip.Comment = "No comments";

                    zip.Save(ZipFileToCreate);
                }

            }
            catch (System.Exception ex1)
            {
                string ErrorMsg = "exception: " + ex1.Message;
                return Tuple.Create(false, ErrorMsg);
            }

            return Tuple.Create(true, "");
        }
        public static DateTime EndOfMonthDate(DateTime curDate)
        {
            var now = curDate;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var DaysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            var lastDay = new DateTime(now.Year, now.Month, DaysInMonth);
            return lastDay;
        }
        static public int GetYearWeekNumber(DateTime date)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }
        
        public static DateTime NearestSunday(DateTime curDate)
        {
            DateTime nextSunday = curDate;
            if (curDate.DayOfWeek != DayOfWeek.Sunday)
                nextSunday = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek) + 7);
            return nextSunday.Date;
        }

        static public bool LoadManifestResourceToFile(string resourceName, string outputFilePath)
        {
            // Example of resosurce name: "PrepareReport.ReportTemplateFile.xls"
            Assembly _assembly = Assembly.GetExecutingAssembly();
            BinaryReader _reader;

            if (string.IsNullOrEmpty(outputFilePath))
                return false;

            if (File.Exists(outputFilePath))
                return false;
            _reader = new BinaryReader(_assembly.GetManifestResourceStream(resourceName));
            int len = (int)_reader.BaseStream.Length;
            byte[] body = new byte[len];
            len = _reader.Read(body, 0, (int)len);
            using (BinaryWriter binWriter =
                       new BinaryWriter(File.Open(outputFilePath, FileMode.Create)))
            {
                binWriter.Write(body, 0, len);
            }
            return true;
        }

        static public (int,string) RenameFiles(string folder, string pattern, Dictionary<string, string> renDict)
        {
            // returns dictionary: n files were renamed, message...
            // folder where the files are located.
            // pattern: "[RegEx:]<what to rename> -> <newfileNamePattern>
            // patterns may contain {Counter}, {Counter:0..0}
            // example: "RegEx:IMG_\d+.jpg -> Zabolotn6-47_{Counter:000}.jpg"
            // $1,$2... where $N is a caught part in pattern
            // abc(\w\w\d+)(.docx) -> $1-file$2 (i.e. abcXY11.docx -> XY11-file.docx)

            var splitDelim = new string[] { " -> " };
            string[] ptrns = pattern.Split(splitDelim, StringSplitOptions.RemoveEmptyEntries);
            string ptrnSrc = PrcPatternKeyWords(ptrns[0]);
            string ptrRen = PrcPatternKeyWords(ptrns[1], true);

            List<string> files = GetListFiles(folder, ptrnSrc, false);

            if (ptrnSrc.StartsWith(sRegExp))
            {
                ptrnSrc = ptrnSrc.Substring(sRegExp.Length);
            }

            string msg = "";
            int count = 0;
            int count1 = 0; // used to support {Counter:000} 
            foreach (string filePath in files)
            {
                count1++;
                (int cnt, string msg) res = RenameFileByPattern(filePath, ptrnSrc, ptrRen, count1, renDict);
                msg += res.msg;
                if(res.cnt < 0)
                {
                    continue;
                }
                count1 = res.cnt;   // last counter used, if any
                count++;
            }
            return (count, msg);
        }

        public static List<string> GetFilteredFilesList(string folder, List<string> ptrns, bool searchInSubFolders)
        {
            var ptrnsProcessed = ptrns.Select(x=>PrcPatternKeyWords(x)).ToList();
            var listAll = new List<string>();
            foreach(var ptrn in ptrnsProcessed)
            {
                var lst = GetListFiles(folder, ptrn, searchInSubFolders);
                listAll.AddRange(lst);
            }
            return listAll;
        }
        public static List<string> GetListFiles(string folder, string ptrnSrc, bool inSubFolders)
        {
            var ptrnProcessed = PrcPatternKeyWords(ptrnSrc);
            Predicate<string> MatchOk = (file) => { return CheckFileNameByPtrn(file, ptrnProcessed); };
            List<string> files = MatchingFiles(folder, MatchOk);
            if (inSubFolders)
            {
                var subFolders = Directory.GetDirectories(folder, "*", SearchOption.AllDirectories);
                foreach(var subFolder in subFolders)
                {
                    var filesInSub = GetListFiles(subFolder, ptrnProcessed, false);
                    files.AddRange(filesInSub);
                }
            }
            return files;
        }

        public static (int cnt, string msg) RenameFileByPattern(string filePath, 
            string ptrnSrc, 
            string ptrRen, 
            int count,
            Dictionary<string, string> replacementDict)
        {
            string file = Path.GetFileName(filePath);
            string fileFolder = Path.GetDirectoryName(filePath);
            string newName = ReplaceWithDict(file, ptrnSrc, ptrRen, replacementDict);
            // if count is contained update the name
            string ptrnCount = "{Counter(:0+)?}";
            Match mtch = Regex.Match(newName, ptrnCount, RegexOptions.IgnoreCase);
            if (!mtch.Success)
            {
                MoveRename(filePath, Path.Combine(fileFolder, newName), false);
                return (count, $"{filePath} -> {newName}\n");
            }
            // while exists file count++
            int idx = mtch.Index;
            int len = mtch.Length;
            string countFmt = string.Empty;
            if (mtch.Groups.Count == 2 && mtch.Groups[1].Value.Length>0)
            {
                countFmt = mtch.Groups[1].Value.Substring(1);
            }
            var nmCount = GetUniquFileName(fileFolder, newName, idx, len, countFmt, count);
            if (nmCount.name.Length == 0)
                return (-1, $"Failed to rename file: {filePath}, rename patters {ptrnSrc} -> {ptrRen}\n");
            newName = nmCount.name;
            count = nmCount.count;
            // rename file:
            MoveRename(filePath, Path.Combine(fileFolder, newName), false);
            return (count, $"{filePath} -> {newName}\n");
        }

        public static void MoveRename(string filePath, string newFilePath, bool removeExisting)
        {
            try
            {
                if (removeExisting && File.Exists(newFilePath))
                {
                    File.Delete(newFilePath);
                }
                File.Move(filePath, newFilePath);
            }
            catch (Exception e)
            {
                throw new Exception("Error: Failed to rename file:\n" +
                    $"from: {filePath}\n" +
                    $"to:   {newFilePath}\n" +
                    $"{e.Message}");
            }
        }

        public static string ReplaceWithDict(string str1, string ptrnRgxSrc, string ptrnRgxRepl, Dictionary<string, string> replacementDict)
        {
            if (replacementDict == null || replacementDict.Count == 0)
            {
                return Regex.Replace(str1, ptrnRgxSrc, ptrnRgxRepl);
            }
            Match match1 = Regex.Match(str1, ptrnRgxSrc);
            var grps = match1.Groups;
            string sRes = ptrnRgxRepl;
            var arrOfReplParts = new List<string>();
            if (grps.Count >= 2)
            {
                for (int i = 0; i < grps.Count; i++)
                {
                    var group = grps[i];
                    string key = group.Value;
                    string value = replacementDict.ContainsKey(key) ? replacementDict[key] : "";
                    arrOfReplParts.Add(value);
                }
            }
            for (int i = 0; i < arrOfReplParts.Count; i++)
            {
                var ptrn = new Regex($"\\${i}(?=[^0-9]|$)");
                sRes = ptrn.Replace(sRes, arrOfReplParts[i]);
            }
            return sRes;
        }

        public static (string name, int count) GetUniquFileName(string fileFolder, string newName, int idx, int len, string countFmt, int count)
        {
            // namePart1{Count:000}namePart2
            int cnt = count;
            for(int i=0; i<1000; i++)
            {
                cnt = count + i;
                string countStr = cnt.ToString(countFmt);
                string rest = newName.Substring(idx+len);
                string pureName = newName.Substring(0, idx) + countStr + rest;
                string pth = Path.Combine(fileFolder, pureName);
                if (!File.Exists(pth)) return (pureName, cnt);
            }
            return ("", cnt);
        }

        private static bool CheckFileNameByPtrn(string fileName, string ptrnSrc)
        {
            if (ptrnSrc.StartsWith(sRegExp))
            {
                ptrnSrc = ptrnSrc.Substring(sRegExp.Length);
            }
            else
            {
                var areEq = String.Compare(fileName, ptrnSrc, true) == 0;
                return areEq;
            }
            return Regex.IsMatch(fileName, ptrnSrc);
        }

        static public string PrcPatternKeyWords(string ptrnToProcess, bool forReplaceUsage=false)
        {
            if (ptrnToProcess.Contains(FilePatternKeywords.strEndOfThisWeekDate))
            {
                DateTime nextSunday = NearestSunday(DateTime.Today);
                string strDate = nextSunday.ToString("yyyy-MM-dd");
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strEndOfThisWeekDate, strDate);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strDate))
            {
                string ptrnDate = forReplaceUsage ? DateTime.Today.ToString("yyyy-MM-dd") :
                    @"\d\d\d\d-\d\d-\d\d";
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strDate, ptrnDate);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strTodayDate))
            {
                string strDate = DateTime.Today.ToString("yyyy-MM-dd");
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strTodayDate, strDate);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strEndOfThisMonthDate))
            {
                DateTime endOfMonth = EndOfMonthDate(DateTime.Today);
                string strDate = endOfMonth.ToString("yyyy-MM-dd");
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strEndOfThisMonthDate, strDate);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strThisMonthDate))
            {
                DateTime endOfMonth = EndOfMonthDate(DateTime.Today);
                string strDate = endOfMonth.ToString("yyyy-MM-dd");
                string ptrn = "YYYY[-_/.]?MM[-_/.]?(\\d\\d)?"; //for matching 2024_05, 2024-05-22
                strDate = ptrn.Replace("YYYY", strDate.Substring(0, 4)).Replace("MM", strDate.Substring(5, 2));
                //if (!forReplaceUsage) strDate = strDate.Substring(0, 8) + @"\d\d";
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strThisMonthDate, strDate);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strThisYear))
            {
                DateTime endOfMonth = EndOfMonthDate(DateTime.Today);
                string strYear = endOfMonth.ToString("yyyy-MM-dd").Substring(0, 4);
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strThisYear, strYear);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strThisMonth))
            {
                DateTime endOfMonth = EndOfMonthDate(DateTime.Today);
                string strMM = endOfMonth.ToString("yyyy-MM-dd").Substring(5,2);
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strThisMonth, strMM);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strEffYear))
            {
                var dtOfInterest = DateTime.Today;
                int dayNo = dtOfInterest.Day;
                if (dayNo <= FilePatternKeywords.daysCurToPrev) dtOfInterest = dtOfInterest.AddDays(-FilePatternKeywords.daysCurToPrev);
                DateTime endOfMonth = EndOfMonthDate(dtOfInterest);
                string strYear = endOfMonth.ToString("yyyy-MM-dd").Substring(0, 4);
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strEffYear, strYear);
            }
            if (ptrnToProcess.Contains(FilePatternKeywords.strEffMonth))
            {
                var dtOfInterest = DateTime.Today;
                int dayNo = dtOfInterest.Day;
                if (dayNo <= FilePatternKeywords.daysCurToPrev) dtOfInterest = dtOfInterest.AddDays(-FilePatternKeywords.daysCurToPrev);
                DateTime endOfMonth = EndOfMonthDate(dtOfInterest);
                string strMM = endOfMonth.ToString("yyyy-MM-dd").Substring(5, 2);
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strEffMonth, strMM);
            }

            if (ptrnToProcess.Contains(FilePatternKeywords.strYearWeekNumber))
            {
                int iYearWeekNumber = GetYearWeekNumber(DateTime.Today);
                string strDate = DateTime.Today.ToString("yyyy-MM-dd");
                string yearWeek = strDate.Substring(0, 5) + iYearWeekNumber.ToString("D3");
                ptrnToProcess = ptrnToProcess.Replace(FilePatternKeywords.strYearWeekNumber, yearWeek);
            }
            return ptrnToProcess;
        }

    }
}
