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
    public class ExportSchedules : IExternalCommand
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
                    string outputFile = @"C:\Temp\ExportedSchedules.csv";
			
			string headers = "Drg Number,Schedule Filter,Filtered Drg Number";
			
			StringBuilder sb = new StringBuilder();

		// Step 1: Find the sheet with the given name
        var allSheets = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet))
            .Cast<ViewSheet>()
            .Where(sheet => sheet.Name.Contains("SEGMENT SHOP DRAWING INDEX"))
        	.ToList();


        
        foreach (var sheet in allSheets)
        {
        	
            // Step 2: Get schedule instances placed on the sheet
            var scheduleInstances = new FilteredElementCollector(doc, sheet.Id)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>();

            // Step 3: Get the corresponding ViewSchedules
            ViewSchedule schedule = scheduleInstances
                .Select(inst => doc.GetElement(inst.ScheduleId) as ViewSchedule)
                .FirstOrDefault(vs => vs != null);
                
            
            ScheduleDefinition definition = schedule.Definition;
            
            string filterOutput = "";
            
            foreach (ScheduleFilter filter in definition.GetFilters())
        	{
            string fieldName = GetFieldName(definition, filter.FieldId);
            string filterType = filter.FilterType.ToString();
            string ruleValue = filter.GetStringValue(); // or .GetDoubleValue() depending on data type

            filterOutput = "Field: " + fieldName + "Type: " + filterType + "Rule: " + ruleValue;
        	}	
                    
                    
            TableData tableData = schedule.GetTableData();
        	TableSectionData sectionData = tableData.GetSectionData(SectionType.Body);
            
        	int nRows = sectionData.NumberOfRows;
	        int nCols = sectionData.NumberOfColumns;
        
	        //TaskDialog.Show("R", "Rows: " + nRows.ToString() +"\n" + "Cols: " + nCols.ToString());
	        string docCode = sheet.LookupParameter("DOC CODE").AsValueString();
        
	        for (int row = 1; row < nRows; row++)
	        {

	        	string cellText = sectionData.GetCellText(row, 0);
	        	
	        	    if (string.IsNullOrWhiteSpace(cellText))
        						continue; // likely a filtered-out row
	            
	        	sb.Append( docCode + '-' + sheet.SheetNumber.ToString() +',' + filterOutput + ',' +cellText);
	            
	            sb.AppendLine();
	        }
             
            
            
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


			
            // TaskDialog.Show("R", $"Done");


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
    
		    private static string GetFieldName(ScheduleDefinition definition, ScheduleFieldId fieldId)
        {
            ScheduleField field = definition.GetField(fieldId);

            if (field != null)
            {
                return field.GetName();
            }
            else
            {
                return "Unknown";
            }
        }
    }//close class
}