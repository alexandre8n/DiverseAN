using Aspose.Pdf;
using Aspose.Pdf.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace DiscoursesFileProcessing
{
    public class PdfExtractor
    {
        public const string constEndOfpage = "<End-Of-Page>";
        public string InFolder = "";
        public string LibPath = "";
        public string FilePattern = "";
        public int PageStart = 0;
        public int PageEnd = int.MaxValue;
        private string curFilePath = "";
        public string ReportLinesByPattern = "";
        public string FootnoteRefPattern = "";
        public string FootnotePattern = "";
        public string StartOfAppendix = "";                 // indicates the end of Stories...

        private int iParagraphSize = 2;
        private List<string> LinesToIgnorePatterns = new List<string>();

        // this is the pattern for the name of Story in the text
        public string BeginStoryPattern = "";
        //private List<string> beginStoryPatterns = new List<string>();
        private string StoryOrAppendixPtrn = null;

        // this is the pattern for the output files for Stories
        public string OutputFileNamePattern = "";

        private List<string> arrLines = new List<string>();
        public string ExtractedText => string.Join("\r\n", arrLines); 
        public bool NLOnlyForParagraph { get; internal set; }

        private Dictionary<string, string> dictCurrStoryAttributes = new Dictionary<string, string>();
        private Dictionary<string, string> footNoteNumbToFootnoteContent = new Dictionary<string, string>();

        public PdfExtractor(int paragraphSize)
        {
            this.iParagraphSize = paragraphSize;
        }

        public void SetBeginStoryRegx(string beginStory)
        {
            BeginStoryPattern = beginStory;
            // maybe in future one can do more advanced??
            // format example: ^\s*{number1}\r\n{name1}\r\n{name2}\s+Játaka[ ]*[\r\n]
            // if {nummer} with regex: (\d+) is contained: the var nummer in dict is set
            // if {nameX} with regex: the var nameX in dict is set
            // nameX may contain (\w+[ -,.'"?!]+)+

        }
        public void SetLinesToIgnoreRegx(string strLinesToIgnore)
        {
            LinesToIgnorePatterns = strLinesToIgnore.
                Split(new string[] { "<>" }, StringSplitOptions.None).ToList();
        }

        public Aspose.Pdf.Document SetCurPdfDoc(string filename)
        {
            curFilePath = filename;
            var p = new Aspose.Pdf.Document(curFilePath);
            return p;
        }
        public bool PdfToTxtAll(string inFile, string outFile)
        {
            // Open document
            Document pdfDocument = new Document(inFile);
            // https://docs.aspose.com/pdf/net/extract-text-from-all-pdf/
            // Create TextAbsorber object to extract text
            TextAbsorber textAbsorber = new TextAbsorber();
            // Accept the absorber for all the pages
            pdfDocument.Pages.Accept(textAbsorber);
            // Get the extracted text
            string extractedTxt = textAbsorber.Text;
            // Create a writer and open the file
            TextWriter tw = new StreamWriter(outFile);
            // Write a line of text to the file
            tw.WriteLine(extractedTxt);
            // Close the stream
            tw.Close();
            return true;
        }

        public bool SaveStrToFile(string strToSave, string fileNameModifier = "")
        {
            string pth = Path.GetDirectoryName(curFilePath);
            string nm = Path.GetFileNameWithoutExtension(curFilePath);
            string outFile = Path.Combine(pth, nm+ fileNameModifier + ".txt");
            string outputLine = $"Extracting Text to file: {outFile}\n";
            Console.Write(outputLine);

            // Create a writer and open the file
            TextWriter tw = new StreamWriter(outFile);
            // Write a line of text to the file
            tw.WriteLine(strToSave);
            // Close the stream
            tw.Close();
            return true;
        }


        public void ProcessPdfPages()
        {
            StoryOrAppendixPtrn = BeginStoryPattern;
            if (!string.IsNullOrEmpty(StartOfAppendix))
                StoryOrAppendixPtrn = $"({BeginStoryPattern})|({StartOfAppendix})";
            arrLines.Clear();
            footNoteNumbToFootnoteContent.Clear();
            string fileNm = Path.Combine(this.InFolder, FilePattern);
            Document pdfDocument = SetCurPdfDoc(fileNm);
            if(pdfDocument == null) 
            { 
                return;
            }
            PageEnd = Math.Min(PageEnd, pdfDocument.Pages.Count);
            for (int i=PageStart; i<=PageEnd; i++)
            {
                string s = PdfPageToTxt(pdfDocument, i);
                string sProcessed = processDefects4Page(s);
                var pageLines = GetLinesFromStr(sProcessed);
                if (sProcessed.Contains("1251"))
                {

                }


                var storyHeader = FindNextStoryHelper((0, 0), pageLines, StoryOrAppendixPtrn);
                if (!string.IsNullOrEmpty(FootnoteRefPattern))
                    pageLines = ProcessFootNotes(pageLines, i, storyHeader);
                RemoveLinesToIgnoreHelper(pageLines);

                arrLines.AddRange(pageLines);
                arrLines.Add(constEndOfpage);
            }
            //RemoveLinesToIgnore();
        }

        public List<string> ProcessFootNotes(List<string> pageLines, int iPgNo, StoryHeader storyHead)
        {
            // step1: - check if there are FN in the page
            // step7: ... later on ... integrate all pending footnotes after each paragrpah, if referenced...


            var pageContentStr = string.Join("\r\n", pageLines);

            var matchCollection = Regex.Matches(pageContentStr, FootnoteRefPattern);
            foreach (var mtch in matchCollection)
            {
                string fnKey = BuildFootnoteKey(mtch.ToString(), iPgNo);
                footNoteNumbToFootnoteContent[fnKey] = "";
            }
            if (matchCollection.Count == 0) return pageLines;

            // now find footNotes starting from the last line
            Regex r = new Regex(FootnotePattern);

            //var pageLines = GetLinesFromStr(pageContentStr);
            int lastFootNoteIdx = pageLines.Count;
            int stroryNameLinesChecked = CheckStoryName(pageLines, 0);
            int iStoryStart = storyHead.begin;
            int iStoryHeadEnd = (iStoryStart < 0)? -1 : iStoryStart + GetLinesFromStr(storyHead.name).Count;

            for (int i = pageLines.Count-1; i >= 0; i--)
            {
                var line = pageLines[i].Trim();
                if (IsLineToIgnore(pageLines, i) && (iStoryStart == -1 || i<iStoryStart || i>iStoryHeadEnd))
                {
                    pageLines.RemoveAt(i);
                    lastFootNoteIdx--;
                    continue;
                }
                // is the beginning of footnote?
                Match m = r.Match(line);
                if (m.Success)
                {
                    string fnKey = BuildFootnoteKey(m.Value, iPgNo);
                    
                    // if footnote is not contained in the dict, -> is something wrong, stop fn processing
                    if (!footNoteNumbToFootnoteContent.ContainsKey(fnKey))
                    {
                        // suspicious line: looks like footnote, but no ref. to it in this page
                        continue;
                    }
                    var fnLines = pageLines.GetRange(i, lastFootNoteIdx-i).ToList();
                    var fnContent = string.Join("\n",fnLines);
                    footNoteNumbToFootnoteContent[fnKey] = fnContent;
                    lastFootNoteIdx = i;
                }
            }
            // remove footnotes from the text
            var arrWoFn = pageLines.GetRange(0, lastFootNoteIdx);

            // replace footnote-ref.numbers in text by [FN-<fnKey>]
            UpdateFN_refs(arrWoFn, footNoteNumbToFootnoteContent, iPgNo);
            return arrWoFn;
            //todo: in this function...
            // somewhere "\n", maybe \r\n
            // case of aaa NNN. - falslely considered as footnote...
        }

        private void UpdateFN_refs(List<string> arrWoFn, Dictionary<string, string> fnNumbToFootnoteContent, int iPgNo)
        {
            // footnotes are updated: 1. txt -> FN-1. txt

            for (int i = 0; i < arrWoFn.Count; i++)
            {
                var lineToUpd = arrWoFn[i];
                string lineUpdated = UpdateFN1Line(lineToUpd, fnNumbToFootnoteContent, iPgNo);
                arrWoFn[i] = lineUpdated;
            }
        }

        private string UpdateFN1Line(string lineToUpd, Dictionary<string, string> fnNumbToFootnoteContent, int iPgNo)
        {
            string lineUpdated = lineToUpd;
            var match1 = Regex.Match(lineUpdated, FootnoteRefPattern);
            if (match1.Success)
            {
                var key = BuildFootnoteKey(match1.Value, iPgNo);
                lineUpdated = lineUpdated.Substring(0,match1.Index) + $"[FN-{key}]" + lineUpdated.Substring(match1.Index+match1.Length);
            }
            return lineUpdated;
        }

        public static string BuildFootnoteKey(string fnNo, int iPgNo)
        {
            return iPgNo + "-" + fnNo;
        }

        public static List<string> GetLinesFromStr(string str)
        {
            var splitElms = new char[] { '\r', '\n' };
            return str.Split(splitElms, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        //todo: to remove
        //private void RemoveLinesToIgnore()
        //{
        //    for(int i=0; i<lines.Count; i++)
        //    {
        //        var line = lines[i].Trim();
        //        int nLinesOfStoryName = CheckStoryName(lines, i);
        //        if(nLinesOfStoryName > 0)
        //        {
        //            i += nLinesOfStoryName - 1;
        //            continue;
        //        }
        //        if(IsLineToIgnore(lines, i))
        //        {
        //            lines.RemoveAt(i);
        //            i--;
        //        }
        //    }
        //}
        private void RemoveLinesToIgnoreHelper(List<string> lines)
        {
            //todo: one should clear why dictCurrStoryAttributes are Ok for normal story
            // and why not for Apendix!!
            //dictCurrStoryAttributes = new Dictionary<string, string>();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                int nLinesOfStoryName = CheckStoryName(lines, i);
                if (nLinesOfStoryName > 0)
                {
                    i += nLinesOfStoryName - 1;
                    continue;
                }
                if (IsLineToIgnore(lines, i))
                {
                    lines.RemoveAt(i);
                    i--;
                }
            }
        }

        private bool IsLineToIgnore(List<string> arrLines, int i)
        {
            string line = arrLines[i];
            foreach(var ptrn in LinesToIgnorePatterns)
            {
                var dict1 = Utl.ParseInRegxVars(ptrn, line);
                if(dict1 == null || dictCurrStoryAttributes == null)
                {
                    continue;
                }
                if(dict1.Count == 0)
                {
                    return true;
                }

                if(Utl.IsPartOfDict(dict1, dictCurrStoryAttributes))
                {
                    return true;
                }
            }
            return false;
        }

        private int CheckStoryName(List<string> arrLines, int i)
        {
            // returns: -1 - is not a story name
            // 
            string line = arrLines[i];
            int nLinesCheckedOk = CheckLines(arrLines, i, StoryOrAppendixPtrn); 
            //todo: this line is to remove int nLinesCheckedOk = CheckLines(lines, i, BeginStoryPattern);
            if (nLinesCheckedOk == -1) // line is not of requested to report
            {
                return -1;
            }
            var storyNameLines = arrLines.GetRange(i, nLinesCheckedOk);
            string sToParse = string.Join("\r\n", storyNameLines);
            dictCurrStoryAttributes = Utl.ParseInRegxVars(StoryOrAppendixPtrn, sToParse);
            //todo: this line is to remove dictCurrStoryAttributes = Utl.ParseInRegxVars(BeginStoryPattern, sToParse);
            return nLinesCheckedOk;
        }

        private string processDefects4Page(string sPage)
        {
            StringBuilder strB = new StringBuilder(sPage);
            var lastTenChars = "";
            for (int i=0; i<strB.Length; i++)
            {
                char c = strB[i];
                if(c< '\u0009')
                {
                    lastTenChars = strB.ToString(Math.Max(0,i - 10), 10);
                }
                if(c == '\u0001')       // ' as in One's
                {
                    strB[i] = '\'';
                }
                else if(c == '\u0002')
                {
                    strB[i] = '-'; 
                }
                else if (c == '\u0003')
                {
                    strB[i] = '\'';         // as in farmers'
                }
                else if (c == '\u0004' || c == '\u0005')
                {
                    // 4 open quatation, 5 closing quatation mark
                    strB[i] = '"';
                }
                else if (c == '\u0006')
                {
                    // 6 open `
                    strB[i] = '`';
                }
                else if (c == '\u0007')
                {
                    // 7 ...
                    strB[i] = '.';
                    strB.Insert(i+1, '.');
                    strB.Insert(i+2, '.');
                    i += 2;
                }
                else if (c < '\u0009')
                {
                    strB[i] = '*';
                    Console.WriteLine($"after: [{lastTenChars}] U+{(ushort)c:X4}");
                }
            }
            return strB.ToString();
        }

        public string PdfPageToTxt(Document pdfDocument, int pgNo)
        {
            TextAbsorber textAbsorber = new TextAbsorber();
            if(pdfDocument.Pages.Count < pgNo)
            {
                return "";
            }
            // Accept the absorber for a particular page
            pdfDocument.Pages[pgNo].Accept(textAbsorber);

            // Get the extracted text
            string extractedText = textAbsorber.Text;
            // process text here

            return extractedText;
        }

        internal void SetPages(string pagesStr)
        {
            // input 1-99999
            var arr = pagesStr.Split('-');
            if(arr.Length < 2 )
            { 
                PageStart = 0;
                PageEnd = int.MaxValue;
                return;
            }
            PageStart = int.Parse(arr[0]);
            int p2 = 0;
            PageEnd = int.TryParse(arr[1], out p2) ? p2: int.MaxValue;
        }

        internal void ReportRequestedLines()
        {
            if (ReportLinesByPattern == "")
            {
                return;
            }
            arrLines = TextAsArrLines();
            var arrLinesToReport = new List<string>();
            for(int i=0; i<arrLines.Count; i++) 
            {
                int nLinesCheckedOk = CheckLines(arrLines, i, ReportLinesByPattern);
                if(nLinesCheckedOk == -1) // line is not of requested to report
                {
                    continue;
                }
                for(int j=i; j<i+nLinesCheckedOk; j++)
                {
                    arrLinesToReport.Add(arrLines[j]);
                }
                i += nLinesCheckedOk - 1;
            }
            string res = string.Join("\r\n", arrLinesToReport);
            SaveStrToFile(res, "_Report");
        }

        private List<string> TextAsArrLines()
        {
            if (ExtractedText == "")
                ProcessPdfPages();
            return arrLines;
        }

        private static int CheckLines(List<string> arrLines, int idxToStart, string regExPattern)
        {
            // this func is used to identify beginning of the story
            // returns: number of story name lines
            // after story name goes the story content

            const int lineStackToCheckSize = 10;
            string lineStack = string.Empty;
            int idxEnd = Math.Min(idxToStart + lineStackToCheckSize, arrLines.Count);
            for(int i= idxToStart; i< idxEnd; i++)
            {
                lineStack += arrLines[i] + "\r\n";
            }

            Regex r = new Regex(regExPattern);
            Match m = r.Match(lineStack);
            if (m.Success)
            {
                string fnd = m.Value.TrimEnd(" \r\n".ToCharArray());
                int iPos = m.Index;
                int iLen = fnd.Length;
                string strWithNLtoCount = lineStack.Substring(0, iPos + iLen);
                int linesToSkip = Regex.Matches(strWithNLtoCount, "\n").Count;
                return linesToSkip + 1;
            }
            return -1;
        }

        public void SaveAllStoriesToFiles()
        {
            // go through pages and process text and footnotes
            arrLines = TextAsArrLines();

            // find beginning and end of the next story
            // save the story to file
            (int begin, int end) beg_end = FindNextStory((0,0));
            for (; beg_end.begin >= 0; beg_end = FindNextStory(beg_end))
            {
                SaveStoryAsFile(beg_end);
                beg_end = (beg_end.end+1, 0);
            }
        }

        private void SaveStoryAsFile((int begin, int end) range)
        {
            string fileName1 = arrLines[range.begin].Trim();
            fileName1 = Utl.MakeValidFileName(fileName1);
            var linesToSave = arrLines.GetRange(range.begin, range.end- range.begin+1);
            // test area:
            string lineStory = linesToSave[0].Trim();
            // already done... FilterFromLinesToIgnore(linesToSave);
            string res;
            if(!NLOnlyForParagraph) 
                res = string.Join("\r\n", linesToSave);
            else 
                res = JoinLinesToParagraphsPlusFootnotes(linesToSave, iParagraphSize);

            SaveStrToFile(res, fileName1);
        }

        internal string JoinLinesToParagraphsPlusFootnotes(List<string> linesToSave, int nParagraphSize)
        {
            StringBuilder sb = new StringBuilder();
            int nLinesCheckedOk = CheckLines(linesToSave, 0, StoryOrAppendixPtrn);
            //todo: remove int nLinesCheckedOk = CheckLines(linesToSave, 0, BeginStoryPattern);
            if (nLinesCheckedOk == -1) // line is not of requested to report
            {
                throw new Exception("Expected begin of the story is not found");
            }
            for(int j = 0; j< nLinesCheckedOk; j++)
            {
                sb.AppendLine(linesToSave[j].Trim());
            }
            sb.Append("\r\n");
            sb.Append(linesToSave[nLinesCheckedOk]);
            
            var linesOfCurParagr = new List<string>();

            for (int i = nLinesCheckedOk; i < linesToSave.Count; i++)
            {
                var sLine = linesToSave[i];
                if(sLine.Contains(constEndOfpage))
                {
                    continue;
                }
                if (sLine.Substring(0, nParagraphSize).TrimStart().Length == 0)
                {
                    // new paragraph found
                    sb.Append("\r\n");
                    AddFootnotesAfterParagr(linesOfCurParagr, sb);
                    linesOfCurParagr.Clear();
                    linesOfCurParagr.Add(sLine);
                }
                else
                {
                    linesOfCurParagr.Add(sLine);
                    sb.Append(" ");
                }
                sb.Append(sLine);
            }
            return sb.ToString();
        }

        private void AddFootnotesAfterParagr(List<string> curParagrList, StringBuilder sb)
        {
            // find 999-11 in blabla[FN-999-11]blabla
            string regEx4FN = @"(?<=(\[FN-))\d+-\d+(?=\])";      // -> before [FN-, after ]

            string paragrContent = string.Join("\n", curParagrList);
            var matchCollection = Regex.Matches(paragrContent, regEx4FN);
            if (matchCollection.Count == 0) return;
            foreach (var match in matchCollection) 
            { 
                string footNoteKey = match.ToString();
                string footNoteValue = footNoteNumbToFootnoteContent[footNoteKey];
                footNoteValue = Regex.Replace(footNoteValue, @"^\s*\d+\.\s*", "");            // if   NNN.   - remove
                sb.Append($"\r\nFN-{footNoteKey}.{footNoteValue.Trim()}\r\n");
            }
            sb.Append("\r\n");
        }

        private (int begin, int end) FindNextStory((int begin, int end) range)
        {
            int iBeg = range.begin;
            int iEnd = arrLines.Count - 1;
            for (int i=iBeg; i<arrLines.Count; i++)
            {
                int nLinesCheckedOk = CheckLines(arrLines, i, StoryOrAppendixPtrn);
                if (nLinesCheckedOk == -1) // line is not Begin of Story or Begin of Appendix
                {
                    continue;
                }
                for(int j = i+nLinesCheckedOk; j<arrLines.Count; j++)
                {
                    int nLinesCheckedOk2 = CheckLines(arrLines, j, StoryOrAppendixPtrn);
                    if(nLinesCheckedOk2 == -1)
                    {
                        continue;
                    }
                    return (i, j - 1);
                }
                return (i, iEnd);
            }
            return (-1,-1);
        }
        public static StoryHeader FindNextStoryHelper((int begin, int end) range, List<string> lines, string ptrn)
        {
            int iBeg = range.begin;
            int iEnd = lines.Count - 1;
            for (int i = iBeg; i < lines.Count; i++)
            {
                int nLinesCheckedOk = CheckLines(lines, i, ptrn);
                if (nLinesCheckedOk == -1) // yes = line is NOT begin of story
                {
                    continue;
                }
                string nameOfStory = string.Join("\r\n", lines.GetRange(i, nLinesCheckedOk).ToList());

                for (int j = i + nLinesCheckedOk; j < lines.Count; j++)
                {
                    int nLinesCheckedOk2 = CheckLines(lines, j, ptrn);
                    if (nLinesCheckedOk2 == -1)
                    {
                        continue;
                    }
                    return new StoryHeader(i, j - 1, nameOfStory);
                }
                return new StoryHeader(i, iEnd, nameOfStory);
            }
            return new StoryHeader(-1, -1, "");
        }
    }

    public class StoryHeader
    {
        public int begin;
        public int end;
        public string name;
        public StoryHeader(int begin, int end, string name)
        {
            this.begin = begin;
            this.end = end;
            this.name = name;
        }
    }
}
