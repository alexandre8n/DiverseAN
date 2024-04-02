using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efa
{
    public class EqualFilesAnalizer
    {
        string pathToFolderToAnalyze;
        string pathToFolderToSaveResult;
        bool scanSubFolders;
        bool showOnlyDupplicates;
        

        List<Afile> listOfAllFiles = new List<Afile>();
        List<GroupOfEqualFiles> listOfGroups = new List<GroupOfEqualFiles>();

        public void Analyze(string pathIn, string pathOut, bool bSubFolders, bool onlyDupplicates)
        {
            pathToFolderToAnalyze = pathIn;
            pathToFolderToSaveResult = pathOut;
            scanSubFolders = bSubFolders;
            showOnlyDupplicates = onlyDupplicates;

            BuildListOfAllFiles();
            // continue analysis
            // Build groups of potentionaly equal files base on file size
            BuildGroupOfEqFiles();

        }
        
        public void BuildGroupOfEqFiles()
        {
            while (listOfAllFiles.Count > 0)
            {
                Afile af = listOfAllFiles[0];
                GroupOfEqualFiles g = new GroupOfEqualFiles();
                listOfGroups.Add(g);
                g.Name = af.filePath;
                g.Add(af);
                listOfAllFiles.RemoveAt(0);
                g.AddFilesFromList(listOfAllFiles);
            }

        }

        public void BuildListOfAllFiles()
        {
            DirectoryInfo d = new DirectoryInfo(pathToFolderToAnalyze);
            BuildListOfAllFiles4Directory(d);
        }

        public void BuildListOfAllFiles4Directory(DirectoryInfo dirToReadFrom)
        {
            // get all files
            FileInfo[] Files = dirToReadFrom.GetFiles("*.*");
            foreach (FileInfo file in Files)
            {
                Afile af = new Afile();
                af.filePath = file.FullName;
                af.fileSize = file.Length;
                listOfAllFiles.Add(af);
            }

            if (!scanSubFolders)
                return;

            DirectoryInfo[] dirInfoArray = dirToReadFrom.GetDirectories();
            foreach (DirectoryInfo d in dirInfoArray)
            {
                BuildListOfAllFiles4Directory(d);
            }

        }

        public string ResultsOfAnalysis()
        {
            string s = "";
            foreach (GroupOfEqualFiles g in listOfGroups)
            {
                if (showOnlyDupplicates && g.Count < 2)
                    continue;
                s += g.PrepareOutString();
            }
            return s;
        }


	    public void SaveResultsOfAnalysis()
	    {
            foreach (GroupOfEqualFiles g in listOfGroups)
            {
                if (showOnlyDupplicates && g.Count < 2)
                    continue;
                string s = g.PrepareOutString();
                Console.Write(s);
            }
		    return;
	    }
    }
}
