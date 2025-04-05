using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aspose.Words;
using Aspose.Words.Saving;
using Aspose.Words.Layout;
using Aspose.Words.Tables;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.ComTypes;
using Aspose.Pdf.Text;
using System.Collections;

namespace DiscoursesFileProcessing
{
    public class DiscourseTranslationExtractor
    {
        string inputPath = "";
        string outFileName = "";
        string selectFilePattern = "";
        string templateName = "";

        string outFileFullPath = "";
        string templateNamePath = "";
        bool allSrcIntoOne = false;
        Document outputDoc = null;
        Document inputDoc = null;

        List<string> filesToProcess = new List<string>();
        double normalFontSize = 10;

        public DiscourseTranslationExtractor(string inputPath, string outFileName, string selectFilePattern,
            string templateName, string allSrc2OneYn)
        {
            this.inputPath = inputPath;
            this.outFileName = outFileName;
            this.selectFilePattern = selectFilePattern;
            this.templateName = templateName;
            this.allSrcIntoOne = allSrc2OneYn.ToUpper() == "YES";
            outFileFullPath = Path.Combine(this.inputPath, this.outFileName);
            templateNamePath = Path.Combine(this.inputPath, this.templateName);
        }

        public double GetFontNormal(Document doc)
        {
            double fntSize = 10;
            foreach (Style style in doc.Styles)
            {
                if (style.Name != "Normal")
                    continue;
                if (style.Font != null)
                {
                    fntSize = style.Font.Size;
                    return fntSize;
                }
            }
            return fntSize;
        }
        public bool ProcessFilesByPattern()
        {
            outputDoc = CreateOutputDoc();

            normalFontSize = GetFontNormal(outputDoc);

            filesToProcess.Clear();
            string[] filePaths = Directory.GetFiles(inputPath, selectFilePattern);
            filesToProcess = filePaths.OrderBy(x => x).ToList();

            foreach (var filePath in filesToProcess)
            {
                try
                {
                    ProcessOneFile(filePath);
                    if (!allSrcIntoOne)
                    {
                        string outFile1 = GenOutFileName(filePath);
                        Console.WriteLine($"Saving the file: {outFile1}...");
                        RemoveEmptyParagraphs(outputDoc);
                        UpdateFieldsAndSave(outputDoc, outFile1);
                        // this one is done, create empty new...
                        outputDoc = CreateOutputDoc();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to process the file {filePath}\nException: '{e}'");
                }
            }
            if (allSrcIntoOne)
            {
                outputDoc.Save(outFileFullPath);
            }
            return true;
        }

        private void UpdateFieldsAndSave(Document outputDoc, string outFile1)
        {
            outputDoc.Save(outFile1);
            outputDoc = new Document(outFile1);
            outputDoc.UpdateFields();
            outputDoc.Save(outFile1);
        }

        private string GenOutFileName(string filePath)
        {
            string sOutFileNameBase = Path.GetFileNameWithoutExtension(outFileName);
            string ext = Path.GetExtension(filePath);
            string sName = Path.GetFileNameWithoutExtension(filePath);
            string strPtrn = "\\d+((-|_| )\\d+)?";
            string sFileOut = sName + "_out"+ext;
            Match m = Regex.Match(sName, strPtrn);
            if (m.Success)
            {
                sFileOut = sOutFileNameBase+"-"+m.Value+ext;
            }
            return Path.Combine(inputPath, sFileOut);
        }

        private Document CreateOutputDoc()
        {
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

        private void ProcessOneTable(Table table)
        {
            //Row row = table.FirstRow;
            foreach (Row row in table.Rows)
            {
                if (row.Cells.Count > 2)
                {
                    Cell cell0 = row.Cells[0];
                    string sCell0 = cell0.GetText();
                    Cell cell1 = row.Cells[1];
                    string sCell1 = cell1.GetText();
                    throw new Exception("Unexpected document Format, only 2 columns are allowed");
                }

                Cell cell = row.LastCell;
                string sCell = cell.GetText();
                int iLen = Math.Min(sCell.Length, 100);
                string outputLine = $"First 100 chars of the table: {sCell.Substring(0, iLen)}\n";
                //Console.Write(outputLine);
                int len = sCell.Length;
                ProcessOneCell(cell);
            }

        }

        private void ProcessOneCell(Cell cell)
        {
            //NodeImporter imp = new NodeImporter(outputDoc, cell, ImportFormatMode.KeepSourceFormatting);
            //builder.InsertParagraph()

            List<Paragraph> paragraphsToProcessTogether = new List<Paragraph>();
            foreach (Paragraph paragr in cell.Paragraphs)
            {
                string s1 = paragr.GetText();
                if (s1 == "\r")
                {
                    continue;
                }
                if (JointParagraphs(paragr, paragraphsToProcessTogether))
                {
                    continue;
                }
                ProcessParagraph(paragr);
                if (para111 != null)
                {
                    string stl = para111.ParagraphFormat.StyleName;
                    if(stl != "Heading 1")
                    {
                        Console.WriteLine("Error: after paragr:"+s1);
                    }
                }
                //nParagr++;
                //if (nParagr > 20)
                //    break;
            }
        }

        private void ProcessOneFile(string filePath)
        {
            string outputLine = $"Processing the file: {filePath} ...\n";
            Console.Write(outputLine);
            inputDoc = new Document(filePath);
            DocumentBuilder builder = new DocumentBuilder(inputDoc);
            NodeCollection nodes = inputDoc.GetChildNodes(NodeType.Table, true);
            foreach (var node1 in nodes)
            {
                Table table1 = (Table)node1;
                ProcessOneTable(table1);
            }
        }

        private bool JointParagraphs(Paragraph paragr, List<Paragraph> paragraphsToProcessTogether)
        {
            string sPrgr = paragr.GetText();
            if (Is2ColTable(paragr))
            {
                paragraphsToProcessTogether.Add(paragr);
                return true;
            }
            if (paragraphsToProcessTogether.Count > 0)
            {
                Paragraph p0 = paragraphsToProcessTogether[0];
                string s0 = p0.GetText();
                for (int i = 1; i < paragraphsToProcessTogether.Count; i++)
                {
                    Paragraph prgr = paragraphsToProcessTogether[i];
                    string sprgr = prgr.GetText();
                    MergeParagraphs(p0, prgr);
                    string s2 = p0.GetText();
                }
                //            Node impNode = imp.ImportNode(paragr, true);
                string sMerged = p0.GetText();
                InsertTable(p0);
                paragraphsToProcessTogether.Clear();
                return true;
                //Run nd;//nd.Clone(true);//paragr.Runs.Add()
            }
            return false; // it means continue for paragr ...
        }

        private void MergeParagraphs(Paragraph p0, Paragraph prgr)
        {
            string sP0ini = p0.GetText();
            string sPrgrIni = prgr.GetText();
            string[] lastCharsToRemove = { "\r", "\v" };
            Run lastRun = p0.Runs[p0.Runs.Count - 1];
            lastRun.Text = RemoveFromEndOfString2(lastRun.Text, lastCharsToRemove) + "\v";
            //Run lastRunPin = prgr.Runs[prgr.Runs.Count - 1];
            //lastRunPin.Text = RemoveFromEndOfString(lastRunPin.Text, "\r") + "\v";

            foreach (Node node in prgr.ChildNodes.ToArray())
            {
                string sIn1 = node.GetText();
                p0.AppendChild(node);
                string sp01 = p0.GetText();
            }
            string sP0Out = p0.GetText();
        }

        Paragraph para111 = null;
        private void ProcessParagraph(Paragraph paragr)
        {
            var s3 = paragr.GetText();
            if (s3.Length < 2)
                return;
            Paragraph para = new Paragraph(outputDoc);
            Aspose.Words.Font fnt1 = paragr.ParagraphBreakFont;
            var isDayHeader = IsDayHeader(paragr);
            if (IsDayHeader(paragr))
            {
                para111 = para;
                para.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
                Aspose.Words.Font font = outputDoc.Styles[StyleIdentifier.Heading1].Font;
                para.ParagraphFormat.Style.Font.Bold = font.Bold;
                //para.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            }
            else if (IsEndOfDayLine(paragr))
            {
                para.ParagraphFormat.StyleName = "Normal";
                //para.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            }
            else if (IsSubTitle(paragr))
            {
                para.ParagraphFormat.StyleName = "Heading 2";
                para.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            }
            else if (Is2ColTable(paragr))
            {
                InsertTable(paragr);
                return;
            }
            else
            {
                para.ParagraphFormat.StyleName = "Normal";
                para.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            }
            AddAllRunsToParagraph(para, paragr);
            if (isDayHeader)
            {
                var stl1 = para.ParagraphFormat.Style.Name;
                if(para.ParagraphFormat.StyleIdentifier != StyleIdentifier.Heading1)
                {
                    // very strange!
                    para.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
                }
            }
            //            Node impNode = imp.ImportNode(paragr, true);

            outputDoc.FirstSection.Body.AppendChild(para);
        }

        private void AddAllRunsToParagraph(Paragraph para, Paragraph paragr)
        {
            bool isHeading = (para.ParagraphFormat.StyleIdentifier == StyleIdentifier.Heading1) ||
                (para.ParagraphFormat.StyleIdentifier == StyleIdentifier.Heading2);

            foreach (Run run1 in paragr.Runs.ToArray())
            {
                Run run = new Run(outputDoc);
                if (!isHeading) 
                {
                    run.Font.Bold = run1.Font.Bold;
                    run.Font.Italic = run1.Font.Italic;
                    run.Font.ItalicBi = run1.Font.ItalicBi;
                }
                run.Text = run1.Text;
                para.AppendChild(run);
                var stl = para.ParagraphFormat.StyleName;
                var v1 = para.ParagraphFormat.StyleIdentifier; // = StyleIdentifier.Heading1;
            }
        }

        public static string RemoveFromEndOfString2(string str, string[] stringsToRemove)
        {
            foreach (string strToRemove in stringsToRemove)
                str = RemoveFromEndOfString(str, strToRemove);
            return str;
        }
        public static string RemoveFromEndOfString(string str, string LastCharsToRemove)
        {
            while (str.EndsWith(LastCharsToRemove))
                str = str.Substring(0, str.Length - LastCharsToRemove.Length);
            return str;
        }

        private void InsertTable(Paragraph paragr)
        {
            string s1 = paragr.GetText();
            String[] spearatorTab = { "\t"};
            string[] words = s1.Split(spearatorTab, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
                return;
            // here we process the case:
            // w1 w2 .. wN tab wP1 ... wPN \v
            // w1 w2 .. wN tab wP1 ... wPN \v
            // w1 w2 .. wN tab wP1 ... wPN \r

            String[] spearatorNL = { "\v"};
            string[] lines = s1.Split(spearatorNL, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 1)
                return;
            List<string> LeftParts = new List<string>();
            List<string> RightParts = new List<string>();
            foreach (var line in lines)
            {
                string[] lineWords = line.Split(spearatorTab, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lineWords.Length; i++)
                {
                    if (i == 0)
                        LeftParts.Add(lineWords[i]);
                    else
                        RightParts.Add(lineWords[i]);
                }
            }

            string cellLeft = string.Join("\v", LeftParts);
            string cellRight = RemoveFromEndOfString(string.Join("\v", RightParts), "\r");


            DocumentBuilder builder = new DocumentBuilder(outputDoc);
            builder.MoveToDocumentEnd();
            //builder.ParagraphFormat.Style = outputDoc.Styles["Normal"];
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;

            // We call this method to start building the table.
            Table tbl = builder.StartTable();

            Cell cel1 = builder.InsertCell();
            builder.Write(cellLeft);
            SetItalic4Cell(cel1);

            // Build the second cell
            Cell cel2 = builder.InsertCell();
            builder.Write(cellRight);
            SetItalic4Cell(cel2);
            // Call the following method to end the row and start a new row.
            builder.EndRow();
            tbl.Style.Font.Italic = true;
            tbl.Style.Font.Size = normalFontSize;
            tbl.Alignment = TableAlignment.Left;
            tbl.ClearBorders();
            // Signal that we have finished building the table.
            builder.EndTable();
        }

        private void SetItalic4Cell(Cell cel1)
        {
            foreach (Run run1 in cel1.GetChildNodes(NodeType.Run, true))
            {
                run1.ParentParagraph.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
                run1.Font.Italic = true;
            }
        }

        private bool Is2ColTable(Paragraph paragr)
        {
            string s1 = paragr.GetText();
            if (s1.IndexOf('\t') == -1)
                return false;
            String[] spearatorTab = { "\t" };
            string[] wordsTab = s1.Split(spearatorTab, StringSplitOptions.RemoveEmptyEntries);

            if (wordsTab.Length < 2)
                return false;

            String[] spearator = { ",", ".", " ", "\t", "\r", "\a", "\v" };
            string[] words = s1.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 3)
                return false;
            int nKyrilic = 0;
            foreach (var wrd in words)
            {
                if (ContainsCyrilic(wrd))
                {
                    nKyrilic++;
                }
            }
            if (nKyrilic == 0)
                return true;
            return false;
        }

        private bool ContainsCyrilic(string wrd)
        {
            if (Regex.IsMatch(wrd, @"\p{IsCyrillic}"))
            {
                return true;
            }
            return false;
        }

        private bool IsSubTitle(Paragraph paragr)
        {
            string s1 = paragr.GetText();
            if (s1.Length > 50)
                return false;
            if (paragr.Runs.Count < 1)
                return false;

            if (paragr.Runs[0].Font.Bold && paragr.Runs[paragr.Runs.Count - 1].Font.Bold)
            {
                return true;
            }
            return false;
        }

        private bool IsEndOfDayLine(Paragraph paragr)
        {
            string s1 = paragr.GetText();
            if (s1.Length >= 50)
            {
                return false;
            }
            String[] spearator = { ",", ".", ":", " ", "\t", "\r", "\v", "\a" };
            string[] words = s1.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
                return false;
            string strEndOf = "Конец лекции";
            string strEndOf2 = "Конец главы";
            string strEndOf3 = "Вычитано";
            string sFrst = words[0].ToUpper();
            string sFrst2 = sFrst + " " + words[1].ToUpper();
            if (strEndOf.ToUpper() == sFrst2 ||
                strEndOf2.ToUpper() == sFrst2 ||
                strEndOf3.ToUpper() == sFrst)
                return true;
            return false;
        }
        private bool IsDayHeader(Paragraph paragr)
        {
            string styleName = paragr.ParagraphFormat.StyleName;
            if (styleName.ToUpper() == "HEADING 1")
                return true;
            string s1 = paragr.GetText();
            String[] spearator = { ",", ".", ":", " ", "\t", "\r", "\v", "\a" };
            string[] words = s1.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 1)
                return false;
            string strDay = "День";
            string strDiscourse = "Беседа";

            if (s1.Length < 40)
            {
                string sFrst = words[0].ToUpper();
                string sLast = words[words.Length - 1].ToUpper();
                if (strDay.ToUpper() == sFrst && strDiscourse.ToUpper() == sLast)
                    return true;
            }
            return false;
        }
        private static void RemoveSectionBreaks(Document doc)
        {
            // Loop through all sections starting from the section that precedes the last one
            // and moving to the first section.
            for (int i = doc.Sections.Count - 2; i >= 0; i--)
            {
                // Copy the content of the current section to the beginning of the last section.
                doc.LastSection.PrependContent(doc.Sections[i]);
                // Remove the copied section.
                doc.Sections[i].Remove();
            }
        }
        public static void RemoveEmptyParagraphs(string docPath) 
        {
            Document doc = new Document(docPath);
            RemoveEmptyParagraphs(doc);
            doc.Save(docPath);
            return;
            //Boolean hasImage = false;
            ////Get the paragraphs that start with "Fig".
            //for (Paragraph paragraph : (Iterable<Paragraph>)doc.getChildNodes(NodeType.PARAGRAPH, true))
            //{
            //    if (paragraph.toString(SaveFormat.TEXT).trim().contains("Fig"))
            //    {
            //        Node previousPara = paragraph.getPreviousSibling();
            //        while (previousPara != null
            //                && previousPara.NodeType == NodeType.Paragraph
            //                && previousPara.toString(SaveFormat.Text).trim().length() == 0
            //                && ((Paragraph)previousPara).GetChildNodes(NodeType.Shape, true).Count > 0)
            //        {
            //            previousPara = previousPara.PreviousSibling;
            //            hasImage = true;
            //        }

            //        if (hasImage && previousPara != null)
            //        {
            //            builder.MoveTo(((CompositeNode)previousPara).FirstChild);
            //            builder.StartBookmark("Bookmark" + i);
            //            builder.EndBookmark("Bookmark" + i);

            //            builder.MoveTo(paragraph.Runs[0]);
            //            builder.StartBookmark("FigBookmark" + i);
            //            builder.EndBookmark("FigBookmark" + i);
            //            i++;
            //        }
            //        hasImage = false;
            //    }
            //}
            //for (int b = 1; b < i; b++)
            //{
            //    Node start = doc.getRange().getBookmarks().get("Bookmark" + b).getBookmarkStart();
            //    Node end = doc.getRange().getBookmarks().get("FigBookmark" + b).getBookmarkEnd();
            //    ArrayList images = ExtractContents.extractContent(start, end, false);
            //    Document dstDoc = ExtractContents.generateDocument(doc, images);


            //    if (dstDoc.getFirstSection().getBody().getFirstParagraph().toString(SaveFormat.TEXT).trim().length() > 0)
            //        for (Run run : (Iterable<Run>)dstDoc.getFirstSection().getBody().getFirstParagraph().getChildNodes(NodeType.RUN, true))
            //        {
            //            run.setText("");
            //        }

            //    dstDoc.getRange().replace(ControlChar.PAGE_BREAK, "", new FindReplaceOptions());

            //    dstDoc.save(MyDir + "Fig_" + b + ".docx");
            }

        private static void RemoveEmptyParagraphs(Document doc)
        {
            RemoveSectionBreaks(doc);

            DocumentBuilder builder = new DocumentBuilder(doc);

            //Remove empty paragraphs
            foreach (Paragraph paragraph in doc.GetChildNodes(NodeType.Paragraph, true))
            {
                if (paragraph.ToString(SaveFormat.Text).Trim().Length == 0
                        && paragraph.GetChildNodes(NodeType.Shape, true).Count == 0)
                {
                    paragraph.Remove();
                }
            }
            doc.UpdatePageLayout();
        }
    }

}