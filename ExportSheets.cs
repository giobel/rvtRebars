using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class ExportSheets : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            
            try {

                string outputFile = @"C:\Temp\ExportedData.csv";

                StringBuilder sb = new StringBuilder();


                ICollection<ElementId> allSheetsIds = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).ToElementIds();

                // allSheetsIds.OrderByDescending(r => doc.GetElement(r).LookupParameter("Sheet Number").AsValueString()).ToList();


                //uidoc.ActiveView = uidoc.ActiveGraphicalView;

                string headers = "ElementId,Drg Number,Drg Name,FBA_DrawingSegmentIds,Revision";


                foreach (ElementId eid in allSheetsIds)
                {
                    Element e = doc.GetElement(eid);

                    ViewSheet vs = e as ViewSheet;

                    string segIds = e.LookupParameter("FBA_DrawingSegmentIds")?.AsValueString() ?? "";;

                    segIds = Regex.Replace(segIds, @"\r\n?|\n", ", ");

                    string docCode = vs.LookupParameter("DOC CODE")?.AsString() ?? "";
                    string currentRev = vs.LookupParameter("Current Revision")?.AsValueString() ?? "";

                    string csvLine = $"{eid},{docCode}-{vs.SheetNumber},\"{vs.Name}\",\"{segIds}\",{currentRev}";

                    sb.AppendLine(csvLine);
                                    
                }
                    

                

                File.WriteAllText(outputFile, headers + "\n");


                File.AppendAllText(outputFile, sb.ToString());

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = outputFile;
                process.Start();

          
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }


			
            TaskDialog.Show("R", $"Done");


            return Result.Succeeded;


        }//close execute

    private string SplitSegmentsText(List<string> values){

        string previousPrefix = "";

        string formatted = "\n";

        foreach (var item in values)
        {
            string cleanedItem = item.Replace("\"","").Trim();

            string currentPrefix = cleanedItem.Substring(0, 3); // Get first 3 characters

            if (currentPrefix != previousPrefix && previousPrefix != "")
            {
                formatted = formatted.Substring(0, formatted.Length - 2) + "\n";                
            }

            formatted += cleanedItem + ", ";

            //Console.WriteLine(item);
            previousPrefix = currentPrefix;
        }

        formatted = formatted.Substring(0, formatted.Length -2); // Remove the last comma and space


        return formatted;
    }

    }//close class
}