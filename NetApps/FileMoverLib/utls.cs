using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Ionic.Zip;

namespace ScriptRunnerLib
{
    public class utls
    {
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
            var now = DateTime.Now;
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
        
    }
}
