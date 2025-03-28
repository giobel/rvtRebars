using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class SelectFromRevizto : IExternalCommand
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


        if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
            	{

                string content = Clipboard.GetDataObject().GetData(DataFormats.Text).ToString();
               					
				string paramName = content.Split(':')[0].Trim();
				string paramValue = content.Split(':')[1].Trim(); 
                
				//Revizto: Element ID -> Name
				if (paramName == "Name" && paramValue.Length == 22){
					paramName = "IfcGUID";
				}
				//Revizto: SRC_FBA_rebar -> Slice Number
				else if (paramName == "Slice Number" && paramValue.Length == 4){
					paramName = "FBA_Slice";
				
				}
                else if (paramName == "Comments"){
                    paramName = paramName;
                }
				else{
					paramName = ("FBA_"+paramName).Replace(" ","");
				}
				
				//TaskDialog.Show("R", paramName + "  " + paramValue);
				
				IList<Element> fecElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType().ToElements();

				IList<ElementId> foundElements = new List<ElementId>();
				
				foreach (var element in fecElements) {
					
					Parameter p = element.LookupParameter(paramName);
					
					if (p != null){
						
						if (p.StorageType == StorageType.String)
						{
							if (element.LookupParameter(paramName).AsValueString() == paramValue){
								foundElements.Add(element.Id);
							}
							
						}

                        if (p.StorageType == StorageType.Double)
                        	if (element.LookupParameter(paramName).AsDouble().ToString() == paramValue){
								foundElements.Add(element.Id);
							}
					}
				}
				
				if (foundElements.Count()>0){
					
                uidoc.Selection.SetElementIds(foundElements);
                uidoc.ShowElements(foundElements);
				}
				else{
					TaskDialog.Show("Error", "Could not find any element with this property:\n" +paramName +":"+ paramValue);
				}
                }
            
            return Result.Succeeded;


        }//close execute
    }
}