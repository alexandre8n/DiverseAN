using Aspose.Words;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static Aspose.Pdf.Operator;

namespace DiscoursesFileProcessing
{
    // we here split one source discourse file into many files, everyone containing 1 discourse,
    public class Discourses2ManySplitter
    {

        private string inputDocPath;
        private string outputDocName;
        private string srcFileName;
        private string templateName;
        Document outputDoc = null;
        Document inputDoc = null;

        List<string> filesToProcess = new List<string>();

        public Discourses2ManySplitter(string inputDocPath, string outputDoc, string srcFileName, string templateName)
        {
            this.inputDocPath = inputDocPath;
            this.outputDocName = outputDoc;
            this.srcFileName = srcFileName;
            this.templateName = templateName;
        }

        public bool ProcessSource()
        {
            string outputLine = $"Processing the file: {srcFileName} ...\n";
            Console.Write(outputLine);
            inputDoc = new Document(Path.Combine(inputDocPath, srcFileName) );
            NodeCollection paragraphs = inputDoc.GetChildNodes(NodeType.Paragraph, true);
            bool isBeforeFirst = true;
            List<Paragraph> outParagrs = new List<Paragraph>();
            string outFileName = string.Empty;
            string curOutFileName = string.Empty;
            foreach (Paragraph paragr in paragraphs)
            {
                string s1 = paragr.GetText();
                if (!paragr.HasChildNodes || string.IsNullOrWhiteSpace(paragr.GetText()))
                    continue; // skip decorative or empty paragraphs
                if (paragr.ParentNode!=null && paragr.ParentNode.NodeType == NodeType.HeaderFooter)
                    continue;

                if (IsBeginOfDiscourse(s1, ref outFileName))
                {
                    paragr.ParagraphFormat.StyleName = "Heading 1";
                    paragr.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                    if (isBeforeFirst)
                    {
                        outParagrs.Add(paragr);
                        isBeforeFirst = false;
                        curOutFileName = outFileName;
                        continue;
                    }
                    SaveDiscourse(outParagrs, curOutFileName);
                    outParagrs.Clear();
                    curOutFileName = outFileName;
                }
                outParagrs.Add(paragr);
            }
            SaveDiscourse(outParagrs, curOutFileName);
            return true;
        }

        private void SaveDiscourse(List<Paragraph> outParagrs, string outFileName)
        {
            string outputPath = Path.Combine(this.inputDocPath, outFileName);
            // 1. Create a new empty document
            Document newDoc = CreateOutputDoc();
            //Document newDoc = new Document();

            // 2. Get the document builder and remove the empty first paragraph
            DocumentBuilder builder = new DocumentBuilder(newDoc);
            newDoc.FirstSection.Body.RemoveAllChildren(); // remove the default empty paragraph

            // 3. Import each paragraph from the source list
            foreach (Paragraph para in outParagrs)
            {
                // Import the node into the destination document with correct format preservation
                Node importedPara = newDoc.ImportNode(para, true, ImportFormatMode.UseDestinationStyles);
                newDoc.FirstSection.Body.AppendChild(importedPara);
            }

            // 4. Save the document
            newDoc.Save(outputPath);
            Console.WriteLine($"File: {outFileName} is saved...");
        }


        private Document CreateOutputDoc()
        {
            string templateNamePath = Path.Combine(this.inputDocPath, this.templateName);
            if (File.Exists(templateNamePath))
                outputDoc = new Document(templateNamePath);
            else
            {
                Console.WriteLine($"Warning: template file: {templateNamePath} is not found.\n" +
                    " Default will be used");
                outputDoc = new Document();
            }
            return outputDoc;
        }

        private bool IsBeginOfDiscourse(string input, ref string outputFile)
        {
            // DAY ONE: MORNING DISCOURSE to DAY TEN: MORNING DISCOURSE
            Dictionary<string, string> dayMap = new Dictionary<string, string>
            {
                ["ONE"] = "01",
                ["TWO"] = "02",
                ["THREE"] = "03",
                ["FOUR"] = "04",
                ["FIVE"] = "05",
                ["SIX"] = "06",
                ["SEVEN"] = "07",
                ["EIGHT"] = "08",
                ["NINE"] = "09",
                ["TEN"] = "10"
            };

            Dictionary<string, string> sessionMap = new Dictionary<string, string>
            {
                ["MORNING"] = "01",
                ["EVENING"] = "02"
            };

            string rgxPattern = @"DAY\s+(ONE|TWO|THREE|FOUR|FIVE|SIX|SEVEN|EIGHT|NINE|TEN):\s*(MORNING|EVENING)\s*DISCOURSE";

            Match match = Regex.Match(input, rgxPattern);
            if (!match.Success)
                return false;
            string dayText = match.Groups[1].Value;      // Group 1 -> ONE
            string sessionText = match.Groups[2].Value;  // Group 2 -> MORNING
            string dayNum = dayMap.ContainsKey(dayText) ? dayMap[dayText] : "??";
            string sessionNum = sessionMap.ContainsKey(sessionText) ? sessionMap[sessionText] : "??";
            string dayNumText = dayNum + "-" + sessionNum;
            string sOutFileNameBase = Path.GetFileNameWithoutExtension(outputDocName);
            string ext = Path.GetExtension(outputDocName);
            outputFile = sOutFileNameBase + dayNumText + ".docx";
            return true;

        }
    }
}
