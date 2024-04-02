using StockAnalyzerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test1
{
    class FileInstrumentSaver : FileSaver
    {
        public void SetPath(string path)
        {
            FilePath = path;
        }
        public override bool SaveLine(string Name, string buffer)
        {
            string fileName = Path.Combine(FilePath, Name + ".csv");

            if (!File.Exists(fileName))
            {
                var fw = File.Create(fileName);
                fw.Close();
            }
            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(buffer);
            }
            return true;
        }
        public override bool SaveToConsole(string Name, string buffer)
        {
            Console.WriteLine($"{Name}: {buffer}");
            return true;
        }

    }
}
