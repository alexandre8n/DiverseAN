using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efa
{
    public class GroupOfEqualFiles
    {
        public  string Name;
        public bool AllFilesAreEqual;
        List<Afile> filesOfGroup = new List<Afile>();

        public GroupOfEqualFiles()
        {
            AllFilesAreEqual = false;
        }

        public int Count
        {
            get { return filesOfGroup.Count; }
        
        }

        public void Add(Afile a)
        {
            filesOfGroup.Add(a);
        }

        /// <summary>
        ///  Add files equal to the files of this group, removing from listOfFiles
        /// </summary>
        /// <param name="listOfFiles"></param>
        public void AddFilesFromList(List<Afile> listOfFiles)
        {
            Afile a = filesOfGroup[0];

            int i = 0;
            while (i < listOfFiles.Count)
            {
                Afile b = listOfFiles[i];
                if (a.IsEqualTo(b))
                {
                    listOfFiles.RemoveAt(i);
                    Add(b);
                }
                else
                {
                    i++;
                }
            }

        }

        public string PrepareOutString()
        {
            string s = string.Format("Group: {0}, {1}-files, file length: {2}, check sum: {3}\n", Name, filesOfGroup.Count,
                filesOfGroup[0].fileSize, filesOfGroup[0].checkSum);
                ;
            foreach (var af in filesOfGroup)
            {
                string s1 = af.PrepareOutString();
                s += s1;
            }
            s += "\n";
            return s;
        }

    }
}
