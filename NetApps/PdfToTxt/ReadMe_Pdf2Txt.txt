Params:
InputFolder: - source folder containing the pdf file(s) to process
FilePattern: - FilePattern is the pdf file name to process; (in future may be: pattern of the pdf files to be processed.)

Pages: 2-3 or 3-End	(in future should be 1,3,5 also 5 ) - means the pages to be processed.
	currently only in format: a-b, N-End

LinesToIgnore: regEx patterns of the lines to be ignored. Several patterns are allowed. separator between patterns: <>
	what can be used: 
	example1: ^\s+\d+$<>Játaka Tales of the Buddha<>\(\d+\)\s+(?<name>(\w|[ '.,?!:+-/*()])+)<>^$
	first: ^\s+\d+$
	second: <>Játaka Tales of the Buddha<>            it means just this text - to ignore
	third: \(\d+\)\s+(?<name>(\w|[ '.,?!:+-/*()])+)
		<name> will be used as var in some configurations (see below)
	forth: ^$	- just empty line is to ignore.
EmptyLinesTo1: Yes or No, works only if yes, otherwise does not.
OutputFileName: the formula of output file name for every story.	{number}.txt means that variable number can be used
	from the StartOfNewStoryFile
StartOfNewStoryFile: pattern specifying the start of new story to be stored in separate output file.
	example: ^\s+(?<number>\d+)[\r\n]+\s+(?<name>(\w|[’'.,?!-: ])+)[\r\n]+\s+.+Játaka[ \r\n]*

StartOfAppendix: indicates the End of All stories, and beginning of the appendix, i.e. The Conclusion of the Book...
	example: ^\s+(?<name>(APPENDIX))[\r\n]+\s+(Glossary of Terms)
	<name> - is essential, because it is used to know what story is currently used...
ReportLinesByPattern: this pattern specifies the pattern of the report line in output 
NLOnlyForParagraph: true
LibPath: path to used aspose library, both lib and lic file should be possible to find there


--------------------------------
experiments- now to ignore: StartOfNewStoryFile2: ^\s*{number}\r\n{name}\r\n{Text}\s+Játaka[ ]*[\r\n]


// processing footnotes:
- check if there are FN in the page
	- in the text of some paragraph: text<.!,?><number> or word<number>
		example: reborn in the Vehapphala heaven.1 After staying there for five hundred
	- in the end of page: 1.<prg>nl 2.<prg>nl ... <blanks>number<eol>

  1. See The Thirty-one Planes of Existence in Volume III
  2. Something
  3. something
                        605
<number><.><some-text><nl><blanks><number><eol>