using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace PrepareReport
{
    class Tests
    {
        private void Test1()
        {
            InitializeWorkbook();

            ISheet sheet = hssfWorkbook.GetSheetAt(0);
            ICell cell12_0 = sheet.GetRow(12).GetCell(0);
            ICell cell12_1 = sheet.GetRow(12).GetCell(6);
            ICell cell12_2 = sheet.GetRow(12).GetCell(7);
            ICell cell1_2 = sheet.GetRow(1).GetCell(2);
            string cell12_2_Fml;
            if (cell12_2.CellType == CellType.Formula)
                cell12_2_Fml = cell12_2.CellFormula;



            //cell.CopyCellTo(3); // Copy B5 to D5

            //IRow row = sheet.GetRow(3);
            //row.CopyCell(0, 1); // Copy A4 to B4
            IRow row13 = InsertRow(sheet, 13);
            ICell cell0 = row13.CreateCell(0);
            cell0.SetCellValue("CleverCad 2");
            ICell cell6 = row13.CreateCell(6);
            cell6.SetCellValue(44);
            ICell cell7 = row13.CreateCell(7);
            cell7.SetCellFormula("$H$11*G14");
            ICell c1 = sheet.GetRow(13).GetCell(7);

            int iTotal = -1;
            for (int i = 12; i < 100; i++)
            {
                var row = sheet.GetRow(i);
                ICell cI = row.GetCell(0);
                if (cI.StringCellValue == "Total")
                {
                    iTotal = i;
                    break;
                }
            }

            if (iTotal > 0)
            {
                var row = sheet.GetRow(iTotal);
                var ICell0 = row.GetCell(0);
                var ICell6 = row.GetCell(6);
                var ICell7 = row.GetCell(7);
            }
            //sheet.CopyRow(12, 13); // Copy row A to row B; row B will be moved to row C automatically
            ICell cell2_0 = sheet.GetRow(2).GetCell(0);
            ICell cell2_1 = sheet.GetRow(2).GetCell(1);
            ICell cell2_2 = sheet.GetRow(2).GetCell(2);
            WriteToFile();
        }

        private IRow InsertRow(ISheet worksheet, int rowNoToInsert)
        {
            var newRow = worksheet.GetRow(rowNoToInsert);
            //var sourceRow = worksheet.GetRow(sourceRowNum);

            // If the row exist in destination, push down all rows by 1 else create a new row
            if (newRow != null)
            {
                worksheet.ShiftRows(rowNoToInsert, rowNoToInsert + 1, 1); //worksheet.LastRowNum
            }
            else
            {
                newRow = worksheet.CreateRow(rowNoToInsert);
            }
            return newRow;
        }

        static HSSFWorkbook hssfWorkbook;

        string fileNameToTest = @"F:\CleverCAD\Reporting\Inv\NG.SoftwareAces.2021-08-08 Invoice 210808-01-tst.xls";
        private void InitializeWorkbook()
        {
            using (var fs = File.OpenRead(fileNameToTest))
            {
                hssfWorkbook = new HSSFWorkbook(fs);
            }
        }
        void WriteToFile()
        {
            //Write the workbook’s data stream to the root directory
            FileStream file = new FileStream(fileNameToTest, FileMode.Create);
            hssfWorkbook.Write(file);
            file.Close();
        }


    }
}
