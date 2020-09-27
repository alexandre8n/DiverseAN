using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace MoleculeGenTest
{
    public class Utl
    {
        public static string PathOfLogFile = @"..\Log\";
        public static string logFilePath = "";
        public static void Log(string strLog)
        {
            if(string.IsNullOrEmpty(logFilePath))
                logFilePath = PathOfLogFile + System.DateTime.Today.ToString("yyyy-MM-dd") + "." + "txt";
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fileStream))
                {
                    log.WriteLine(strLog);
                }
            }
        }

        public static void LogToFile(string fileName, string strLog)
        {
            string oldFile = logFilePath;
            logFilePath = PathOfLogFile + fileName;
            Log(strLog);
            logFilePath = oldFile;
        }
    }
}
