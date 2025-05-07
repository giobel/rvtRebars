
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


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class RegroupBars : IExternalCommand
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

			
			//THIS FILE PATH SHOULD MATCH THE EXPORTED FILE FROM THE PREVIOUS MACRO
      string inputFile = @"C:\Temp\RevitGroups.csv";
			    
			           
        		var counts = new Dictionary<string, int>();

        				string errors = "Groups throwing errors: \n";
                using (Transaction t = new Transaction(doc, "Regroup Bars"))
                {
                    t.Start();

                    using (TextFieldParser parser = new TextFieldParser(inputFile))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");

                        
                        List<string> parameters = parser.ReadLine().Split(',').ToList();

                        while (!parser.EndOfData)
                        {
                            var line = parser.ReadLine();

                            var values = line.Split(',').ToList();
                            
                            string groupName = values[0];
                            
                            var eids = values[2].Split(' ');
                            
                            //TaskDialog.Show("R", eids.Count().ToString());
                            
                            List<ElementId> explodedIds = new List<ElementId>();
                            
                            foreach (var element in eids) {
                            	try{
                            		explodedIds.Add(doc.GetElement(element).Id);
                            	}
                            	catch{}
                            }
                            
                            //TaskDialog.Show("R", explodedIds.Count().ToString());
                            
                            try{
	                            Group regroup = doc.Create.NewGroup(explodedIds);
	                            regroup.GroupType.Name = groupName;                            	
                            }
                            catch{
                            	errors += groupName+"\n";
                            	//throw new SystemException("Error "+ groupName);
                            }
                            
                            //TaskDialog.Show("R", groupName);


                        }
                    }
                    
                    
                    t.Commit();
                    
                    TaskDialog.Show("R", errors);
                    
                    
                    
                    
                }
                
            return Result.Succeeded;        
			
        }
        }
        }