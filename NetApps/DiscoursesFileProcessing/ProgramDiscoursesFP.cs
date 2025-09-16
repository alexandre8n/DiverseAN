using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Words;
using Aspose.Cells;
using Aspose.Words.Saving;
using Aspose.Words.Layout;
using Aspose.Words.Tables;
using System.IO;

//todo: wrong interpretation: 60,000 years  to     60,[FN-379-000] 
// todo: header of the page like: Glossary of Personal Names should be ignored as header.
// todo: Glossary of Personal Names




namespace DiscoursesFileProcessing
{
    class ProgramDiscoursesFP
    {
        static void Main(string[] args)
        {
            // the path to the folder, where located all input files, matching pattern
            string inputDocPath = Properties.Settings.Default.InputFolder;

            // the name of the output file, or the name of the root for output files
            // example: Discourses-UA.docx is the name of the output file,
            // where all extracted source file right parts of the table will be placed
            // but for the case if every discourse is planned to be in sep.file allSrc2One=NO,
            // this will be the root.
            // example: src files: look like, EN-UA_01-1.docx,EN-UA_01-2.docx,EN-UA_02-1.docx ...
            // outputDoc: UA.doc, in this case resulting files will be: UA-01-1.docx, UA-01-2, EN-UA_02-1, ...
            string outputDoc = Properties.Settings.Default.OutputFileName;

            // pattern of the files to take as srs: EN-UA*.docx, or TAG *.docx, etc.
            string selectFilePattern = Properties.Settings.Default.DocPattern;

            // template file, the styles of it will be used in generated output doc(s).
            string templateName = Properties.Settings.Default.OutputFileTemplate;

            // flag indicating desired output: YES - all sources(theirs right translation cells) 
            // are extracted into one file: outputDoc, if allSrc2One=NO, every src gets sep. file
            // sep.file names will be generated as mentioned above, see outputDoc desc.
            string allSrc2One = Properties.Settings.Default.AllSrc2OneOut;
            string oneSrc2ManyOut = Properties.Settings.Default.OneSrc2ManyOut;
                        

            Aspose.Words.License licenseWords = new Aspose.Words.License();
            licenseWords.SetLicense(@"Lib\Aspose.Total.lic");

            if(oneSrc2ManyOut.ToUpper() == "YES")
            {
                Console.WriteLine("One source to many output...");
                Discourses2ManySplitter discourses2ManySplitter = new Discourses2ManySplitter(inputDocPath,
                    outputDoc, selectFilePattern, templateName);
                bool res1 = discourses2ManySplitter.ProcessSource();
                return;
            }

            DiscourseTranslationExtractor dte = new DiscourseTranslationExtractor(inputDocPath, 
                    outputDoc, selectFilePattern, templateName, allSrc2One);
            bool res = dte.ProcessFilesByPattern();
            if (res && allSrc2One.ToUpper()=="YES")
            {
                Console.WriteLine($"Removing empty paragraphs from file: {outputDoc}...");
                DiscourseTranslationExtractor.RemoveEmptyParagraphs(Path.Combine(inputDocPath,outputDoc));
            }
        }

    }
}
