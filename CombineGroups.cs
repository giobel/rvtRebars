using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class CombineGroups : IExternalCommand
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

			//ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
			
			ICollection<Element> selectedIds = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_IOSModelGroups).WhereElementIsNotElementType().ToElements();
			
			var grouped = selectedIds.GroupBy(x => x.Name);
			
			//TaskDialog.Show("R", selectedIds.Count().ToString());
			
			//TaskDialog.Show("R", source.Name);
			
			GroupType gt = doc.GetElement(selectedIds.First().Id) as GroupType;
			
			string changed = "";
			
			using (Transaction t = new Transaction(doc,"Combine groups")){
				
				t.Start();
			
				foreach (var group in grouped) {
					
					if (group.Count()>1){
						
						Element source = doc.GetElement(group.First().Id);
					
						
						int counter = 0;
						
						foreach (var element in group) {
				
							element.ChangeTypeId(source.GetTypeId());
							counter ++;
							}
						
						changed += source.Name + " combined "+ counter + " times \n";
					}
				}
				

				
				t.Commit();
				
			}
			
			//GroupType gt = g.GroupType;
			
			TaskDialog.Show("T", changed);
			

            return Result.Succeeded;
			
		

        }
    }
}