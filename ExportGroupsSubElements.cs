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
    public class ExportGroupsSubElements : IExternalCommand
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

      // GET MODEL GROUPS IN THE ACTIVE VIEW
			ICollection<Element> groupsInView= new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_IOSModelGroups).WhereElementIsNotElementType().ToElements();
			
			
			  StringBuilder sb = new StringBuilder();

        //MAY NEED TO BE UPDATED TO YOUR LOCAL FILE PATH
        string outputFile = @"C:\Temp\RevitGroups.csv";
        
        string headers = "GROUP NAME, GROUP ID, GROUP ELEMENT IDS";
           
			  //FILTER REBARS AND STRUCTURAL CONNECTIONS ONLY			  
			  List<BuiltInCategory> builtInCats = new List<BuiltInCategory>(){BuiltInCategory.OST_Rebar, BuiltInCategory.OST_StructConnections};
			  
			  ElementMulticategoryFilter multiFilter = new ElementMulticategoryFilter(builtInCats);
			              
            
			foreach (var group in groupsInView) {
			
				Group currentGroup = group as Group;
				
				string eids = "";
				
				foreach (var element in currentGroup.GetDependentElements(multiFilter)) {
						
						string uniqueId = doc.GetElement(element).UniqueId;
					
						eids += uniqueId + " ";

				}
				
				
				if (eids != "")
					sb.AppendLine(group.Name + ',' + group.Id + ',' + eids);
			}
			
            
                File.WriteAllText(outputFile, headers + "\n");


                File.AppendAllText(outputFile, sb.ToString());
                
                //UNCOMMENT TO LAUNCH EXCEL
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = outputFile;
                process.Start();
		
                
			
                return Result.Succeeded;

        }
    }
}