using System;

namespace MoleculeGenTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string InputFile = @"F:\Work\GitHub\MoleculeGenTest\InputFile\Tests_Programming Interview_aoc_2015_19.txt";
            Console.WriteLine("MoleculeGenTest started...");
            Utl.Log("MoleculeGenTest started..."+ System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var mdl = new ModelInfo();
            mdl.ReadSourceInfo(InputFile);
            mdl.ProcessToTargetFrom("e");
        }
    }
}
