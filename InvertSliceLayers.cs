
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
    public class InvertSliceLayers : IExternalCommand
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
			
      //these are bars in view, what happens when the view changes? need to update this
		    IList<Element> rebars = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();
	
            var uniqueIds = rebars
            .Select(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString())
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

      uniqueIds.Sort();
      // rebars.GroupBy(x=> x.LookupParameter("LOR_UniqueID (SRC_FBA)").AsValueString());

      var form = new Window1(uidoc, doc, rebars);

            form.UniqueIds = new List<string>(uniqueIds);                    

            form.Show();

      
            return Result.Succeeded;
		  }
    }
}