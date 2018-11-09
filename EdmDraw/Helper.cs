using NXOpen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EdmDraw
{
    public static class Helper
    {
        static string _pdfFilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"Temp\EACTPDF");
        public static void ExportPDF(NXOpen.Drawings.DrawingSheet ds)
        {
            if (Directory.Exists(_pdfFilePath))
            {
                Directory.Delete(_pdfFilePath, true);
            }
            Directory.CreateDirectory(_pdfFilePath);

            Session theSession = Session.GetSession();
            Part workPart = theSession.Parts.Work;
            Part displayPart = theSession.Parts.Display;
            PrintPDFBuilder printPDFBuilder1;
            printPDFBuilder1 = workPart.PlotManager.CreatePrintPdfbuilder();

            NXObject[] sheets1 = new NXObject[1];
            NXOpen.Drawings.DrawingSheet drawingSheet1 = ds;
            sheets1[0] = drawingSheet1;
            printPDFBuilder1.SourceBuilder.SetSheets(sheets1);
            var fileName = string.Format("{0}{1}", ds.Name, ".pdf");
            printPDFBuilder1.Filename = System.IO.Path.Combine(_pdfFilePath, fileName);

            NXObject nXObject1;
            nXObject1 = printPDFBuilder1.Commit();

            printPDFBuilder1.Destroy();
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static object GetPropertyValue<T>(T obj,string name)
        {
            foreach (var item in obj.GetType().GetProperties())
            {
                if (item.Name != name)
                {
                    var v = ((System.ComponentModel.DisplayNameAttribute[])item.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false)).ToList();
                    if (!(v.Count > 0 && v.First().DisplayName == name))
                    {
                        continue;
                    }
                }

                return item.GetValue(obj, null)??string.Empty;

            }

            return string.Empty;
        }
    }
}
