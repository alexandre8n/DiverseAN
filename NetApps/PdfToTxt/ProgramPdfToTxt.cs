using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aspose.Pdf;

namespace DiscoursesFileProcessing
{
    class ProgramPdfToTxt
    {
        public static string exePath = "";

        static void Main(string[] args)
        {
            Console.WriteLine("PdfToText started...");
            //TestRegX();

            string exePathLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exePath = Path.GetDirectoryName(exePathLoc);

            var pe = new PdfExtractor(Properties.Settings.Default.ParagraphSize);
            pe.LibPath = Properties.Settings.Default.LibPath;
            pe.InFolder = Properties.Settings.Default.InputFolder;
            pe.FilePattern = Properties.Settings.Default.FilePattern;
            pe.SetPages(Properties.Settings.Default.Pages);
            pe.ReportLinesByPattern = Properties.Settings.Default.ReportLinesByPattern;
            pe.FootnoteRefPattern = Properties.Settings.Default.FootnoteRefPattern;
            pe.FootnotePattern = Properties.Settings.Default.FootnotePattern;
            pe.StartOfAppendix = Properties.Settings.Default.StartOfAppendix;

            // this regexes specify one or several lines to be ignored, delimiter |
            pe.SetLinesToIgnoreRegx(Properties.Settings.Default.LinesToIgnore);

            // this regex specifies one or several lines going one after another
            pe.SetBeginStoryRegx(Properties.Settings.Default.StartOfNewStoryFile);

            // this is the pattern for the output files for Stories
            pe.OutputFileNamePattern = Properties.Settings.Default.OutputFileName;

            pe.NLOnlyForParagraph = Properties.Settings.Default.NLOnlyForParagraph;

            Aspose.Pdf.License licensePdf = new Aspose.Pdf.License();
            var licPath = Path.Combine(pe.LibPath, @"Aspose\Lic\Aspose.Total.lic");
            licensePdf.SetLicense(licPath);

            //divTests(pe);
            pe.SaveAllStoriesToFiles();
        }

        private static void divTests(PdfExtractor pe)
        {
            string s = "                        109\r\n" +
                "             In His Majesty's Service\r\n" +
                "                 Supatta Játaka \r\n\r\n\r\n" +
                "Buddha told this story about\r\na serving of red fish.1\r\n" +
                "(109)            In His Majesty's Service\r\n\r\n" +
                "  Once, Venerab....\r\nWhen  Ve....told\r\nVe...\r\n" +
                "mother what she wanted. Long ago, he did the same.\" Then he told this\r\nstory of the past.\r\n\r\n" +
                "                        448\r\n" +
                "\r\n  Long, long ago, when Brahmadatta was reigning in Báránasi, the\r\n" +
                "Bodhisatta was born as a crow named2 Supatta. He became the leader of\r\n"+
                "   1. this is the footnote one.\r\n"+
                "   2. this is the footnote 2.\r\n"+
                "                 999";

            var pageLines = PdfExtractor.GetLinesFromStr(s);
            var BeginStoryPattern = @"^\s+(?<number>\d+)[\r\n]+\s+(?<name>(\w|[’'.,?!-: ])+)[\r\n]+\s+.+Játaka[ \r\n]*";
            var storyHeader = PdfExtractor.FindNextStoryHelper((0, 0), pageLines, BeginStoryPattern);

            pe.ProcessFootNotes(pageLines, 109, storyHeader);
            // setExtracted...
            return;
            //List<string> lines = s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            //pe.FilterFromLinesToIgnore(lines);
            //string resJoinPrg = pe.JoinLinesToParagraphsPlusFootnotes(lines, 2);
            pe.ProcessPdfPages();
            bool isToReport = pe.ReportLinesByPattern != "" && pe.ReportLinesByPattern.Substring(2) != "//";
            pe.SaveStrToFile(pe.ExtractedText);
            if (isToReport)
                pe.ReportRequestedLines();
        }

        private static void TestRegX()
        {
            string rg1 = @"^\s+(?<number>\d+)[\r\n]+\s+(?<name>(\w|['.,?!: ])+)[\r\n]+\s+.+Játaka[ \r\n]*";
            var dict = new Dictionary<string, string>();
            string sToParse = "   109\r\n" +
            "           In His Majesty's Service\r\n" +
            "                 Supatta Játaka";
            dict = Utl.ParseInRegxVars(rg1, sToParse);
        }
    }
}
