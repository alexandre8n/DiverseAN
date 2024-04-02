using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
//using PrepareReport.Utl;
using System.Drawing.Imaging;

//using NPOI.HSSF.UserModel;
//using NPOI.HPSF;
//using NPOI.POIFS.FileSystem;
using NPOI.HSSF.UserModel;
//using NPOI.HPSF;
using PrepareReport.Utl;
using System.Net;
using System.Linq;
using StockAnalyzerLib;
using StockAnalyzerLib.Utl;

namespace Test1
{
    class Program
    {
        static string trgFolder;
        static DateTime dtMinExpected = DateTime.MinValue;
        static DateTime dtMaxExpected = DateTime.MaxValue;

        static void Main(string[] args)
        {
            double d = 123.45678;
            string st1 = Utl.DblToStrDotN(d, 3);
            Test3();
            return;
            TestTiffTo();
            string s = ModifyFileNameToMakeItUniq(@"C:\_AN\an_home1.txt");
            DateTime dtNow = DateTime.Now;
            int nDay = (int)dtNow.DayOfWeek;
            nDay = (nDay == 0) ? 7 : nDay;
            dtMinExpected = dtNow.AddDays(-nDay).Date;
            dtMaxExpected = dtMinExpected.AddDays(7);

            bool b3 = IsOldFileToArchive(@"c:\a\AC.CleverCAD.Timebooking report 2013-12-22.xls");

            //bool b1 = IsOldFileToArchive(@"c:\a\2013-12-30_TasksListByDeveloper.txt");
            //bool b2 = IsOldFileToArchive(@"c:\a\2013-12-30_TasksListByDay.txt");
            string s1 = "a. bbb ddd c. aa .NET aaa asp.net aaa c.com and 7.73 peraaa";
            string strReplaced1 = Regex.Replace(s1, @"(?<=[ ]|\w)([.])(?=[\w]+)", "(&pt)");

            string testString = "abc, ss qq aa. We are Ok - and fine";
            string testString1 = testString.Substring(14, testString.Length - 14);
            string testString2;
            Match r1 = Regex.Match(testString, @"(?<=([.?!\n]))([\w]|[,]|[:]|[;]|[-]|[ ]|[=])+$");
            int index = r1.Index;
            if (r1.Success)
            {
                testString1 = testString.Substring(0, r1.Index);
                testString2 = testString.Substring(r1.Index);
            }                    

            string line = "abc, 5h, ddd, 5 h, eee, 10 hours, ggg, 1 hour";

            Match res = Regex.Match(line, @"(\d{1}|\d{2}) *(h|hour|hours|h)(;|,| |$)");
            int count = res.Groups.Count;
            string runDateStr = res.Groups[1].Value;
            Match r2 = res.NextMatch();

            string s2 = Regex.Replace(line, @"(\d{1}|\d{2}) *(h|hour|hours|h)(;|,| |$)", "");
        }

        //public delegate void MsgToMainWnd(string msg);
        private static void Test3()
        {
            string filePath = Path.Combine(Properties.Settings.Default.ObservationFolder, "test.csv");
            var fi = new FinancialInstrument(null);
            fi.LoadFromFile(filePath);
            fi.AnalyzeObservations(300);
            return;
            string names = Properties.Settings.Default.Instruments;
            char[] delimiterChars = { ' ', ',' };
            List<string> instrNames = names.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            //!! for testing just take only BTC
            instrNames = instrNames.Take(1).ToList();

            var fileSaver = new FileInstrumentSaver();
            fileSaver.SetPath(Properties.Settings.Default.ObservationFolder);
            //var tsa = new TimeSeriesAnalyzer(instrNames, fileSaver);

        }

        private static void TestTiffTo()
        {
            string BasePath = @"C:\Temp\VercendPOC\incoming";
            string inputFolderName = @"13-RAWDUMP121580292";
            string fileName = "20160824_145015_222_20160824_143045_PP_2906367.tif";
            string inputPath = BasePath + @"\" + inputFolderName + @"\" + fileName;
            string outputPath = Path.Combine(BasePath, inputFolderName); // + @"\";
            int[] pgNumbers = new int[] { 3, 4, 6 };
            var images = GraphicsProcessor.TiffTo(inputPath, outputPath, pgNumbers, ImageFormat.Tiff);
        }

        static private bool IsOldFileToArchive(string path)
        {
            string name = Path.GetFileName(path);
            Match m1 = Regex.Match(name, @"^(\d{4}-\d{2}-\d{2})_(TasksListByDay|TasksListByDeveloper).txt");
            if (m1.Success)
            {
                DateTime dt = DateFromStrEnUs(m1.Groups[1].Value);
                if (dt == DateTime.MinValue)
                    return false;
                if (dt < dtMinExpected)
                    return true;
            }

            // check, possibly we have user report?
            // like AC.CleverCAD.Timebooking report 2013-12-22.xls
            m1 = Regex.Match(name, @"^\w{2,4}.CleverCAD.Timebooking report (\d{4}-\d{2}-\d{2}).xls[x]?");
            if (m1.Success)
            {
                DateTime dt = DateFromStrEnUs(m1.Groups[1].Value);
                if (dt == DateTime.MinValue)
                    return false;
                if (dt < dtMinExpected)
                    return true;
            }
            return false;
        }

        static DateTime DateFromStrEnUs(string dateStr)
        {
            DateTime dateToTest;
            bool bTry = DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateToTest);
            if (!bTry)
            {
                return DateTime.MinValue;
            }
            return dateToTest;
        }

        static HSSFWorkbook hssfworkbook;
        static Dictionary<string, string> DeveloperToTasks = new Dictionary<string, string>();
        static Dictionary<int, string> dayToTasks = new Dictionary<int, string>();
        static string devPrefix;

        public static string GetStringItem(string line, int nItem, char delimiter)
        {
            string[] items = line.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            if (nItem < items.Length)
                return items[nItem];
            return string.Empty;
        }


        static string RemoveQuatMarksAndBlanks(string str)
        {
            string s = str.Trim();
            if (string.IsNullOrEmpty(s))
                return "";
            if (s.Substring(0, 1) == "\"" && s.Substring(s.Length - 1, 1) == "\"")
                s = s.Substring(1, s.Length - 2);
            return s.Trim();
        }
        public static string ModifyFileNameToMakeItUniq(string filePath)
        {
            string folder = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);
            string resPath = filePath;
            for (int i = 0; i < 10000; i++)
            {
                if (i > 0)
                {
                    resPath = Path.Combine(folder, string.Format("{0}_{1}{2}", fileName, i, ext));
                }
                if (!File.Exists(resPath))
                {
                    return resPath;
                }
            }
            return null;
        }
        public static void Test2()
        {
            string strBTC = "https://www.eobot.com/api.aspx?coin=BTC";
            //https://www.eobot.com/api.aspx?supportedcoins=true&currency=USD
            // "https://api.github.com/repos/restsharp/restsharp/releases"
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strBTC);

            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 58.0.3029.110 Safari / 537.36";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string content = string.Empty;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    content = sr.ReadToEnd();
                }
            }
            Console.WriteLine($"Result:{content}");
        }

    }
}
