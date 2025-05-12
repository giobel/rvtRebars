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
using System.Threading.Tasks;



namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class UpdateFromExcel : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            string inputFile = @"C:\Temp\ExportedData.csv";
            
            int counter = 0;

			using (Transaction t = new Transaction(doc,"Update SegmentId Text")){
				
				t.Start();
				
			      using (TextFieldParser parser = new TextFieldParser(inputFile))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");

                        
                        //ElementId,FBA_DrawingSegmentIds

                        List<string> parameters = parser.ReadLine().Split(',').ToList();

                        // TaskDialog.Show("R", parameters.Count.ToString());

                        // TaskDialog.Show("R", parameters[0]);
                        // TaskDialog.Show("R", parameters[1]);
                        



                        while (!parser.EndOfData)
                        {
                            var line = parser.ReadLine().Replace("\"","").Replace(" ","");

                            List<string> values = line.Split(',').ToList();

                                //TaskDialog.Show("R", values[0]);
                                //TaskDialog.Show("R", values[1]);

                                Int64 id = Convert.ToInt32(values[0]);

                                ElementId currentId = new ElementId(id);
                                
                                values.RemoveAt(0);

                                try
                                {

                                    Element e = doc.GetElement(currentId);
                                    
                                    //TaskDialog.Show("R", e.ToString());

                                    //FBA Drawing Segment Ids
                                    Parameter p = e.LookupParameter(parameters[1].Trim());

                                    //TaskDialog.Show("R", p.ToString());

                                    if (p.StorageType == StorageType.String)
                                    {
                                        

                                        p.Set(SplitSegmentsText(values));
                                        

                                        counter ++;
                                    }
                                    
                                    
                                        
                                    
                                }
                                catch
                                {
                                    //TaskDialog.Show("Error", ex.Message); 
                                }

                            
                            
                        }
                    }

						
    
			t.Commit();
            
            }
			
            TaskDialog.Show("R", $"{counter} modified");


            return Result.Succeeded;


        }//close execute

    private string SplitSegmentsText(List<string> data){

        //sort by first 3 characters

        string previousPrefix = "";

        string formatted = "\n";
        
        var sortNS = data.OrderBy(item => item.Substring(4, 3)) // Sort by first 3 characters
            .ToList();

        var sortEW = sortNS.OrderBy(item => item.Substring(0, 3)).ToList();

        foreach (var item in sortEW )
        {
            //TaskDialog.Show("R", item.Substring(0,3));
            
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