using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace PrepareReport.Utl
{
    public class FilesCopyMover
    {
        public string PathWithTasks { get; private set; }
        private List<FilesMoverTask> tasksOfFileMover = new List<FilesMoverTask>();
        public string Message { get; private set; }
        public string ErrorMessage { get; private set; }

        public FilesCopyMover(string pathWithTasks)
        {
            PathWithTasks = pathWithTasks;
        }

        public bool ExecuteTasks()
        {
            Message = "";
            LoadTasks();
            foreach (var tsk in tasksOfFileMover)
            {
                Message += $"FileMoveCopy Task: {tsk.Name}\n";
                tsk.MoveCopyFiles();
                Message += tsk.ResultingMessage;
            }
            return true;
        }

        private void LoadTasks()
        {

            List<FileInfo> fiList = GetListOfTasks(PathWithTasks);
            foreach (var fi in fiList)
            {
                var lines = utls.ReadAllLinesFromTextFile(fi.FullName);
                var tsk = new FilesMoverTask(fi.FullName);
                tsk.ReadTaskInfo(lines.ToList());
                if (tsk.IsActive)
                {
                    tasksOfFileMover.Add(tsk);
                }

            }
        }
        public static List<FileInfo> GetListOfTasks(string path)
        {
            List<FileInfo> lst = utls.BuildListOfAllFiles(path, false);
            var filtered = lst.Where(x => MatchTaskFileName(x.Name)).ToList<FileInfo>();
            return filtered;
        }
        static public bool MatchTaskFileName(string name)
        {
            Match m = Regex.Match(name.ToLower(), @"\.filesmover$");
            return m.Success;
        }


    }
}
