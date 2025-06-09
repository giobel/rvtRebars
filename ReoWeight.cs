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
using Autodesk.Revit.DB.Structure;

namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class ReoWeight : IExternalCommand
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

            Dictionary<string, double> bbarWeight = new Dictionary<string, double>
            {
                { "N10", 0.66 },
                { "N12", 0.928 },
                { "N16", 1.649 },
                { "N20", 2.577 },
                { "N24", 3.711 },
                { "N28", 5.051 },
                { "N32", 6.597 },
                { "N36", 8.35 },
                { "N40", 10.309 }
            };
			
			IList<Element> rebars = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();
			
			double totalW = 0;
			double totalL = 0;
			
			using (Transaction t = new Transaction(doc, "Add weight"))
			{

				t.Start();



				foreach (var element in rebars)
				{

					double diameter = UnitUtils.ConvertFromInternalUnits(element.LookupParameter("Bar Diameter").AsDouble(), UnitTypeId.Millimeters);
					//
					//TaskDialog.Show("R", diameter.ToString());

					string diameterName = "N" + Math.Round(diameter, 0).ToString();

					double length = UnitUtils.ConvertFromInternalUnits(element.LookupParameter("Total Bar Length").AsDouble(), UnitTypeId.Meters);

					double rebarWeight = bbarWeight[diameterName] * length;

					totalW += rebarWeight;
					totalL += length;

					//element.LookupParameter("Weight (SRC_FBA)").Set(rebarWeight);
				}

				t.Commit();

			}
			
			
			TaskDialog.Show("Results", $"Total Weight: {Math.Round(totalW,3)}kg\nTotal Length: {Math.Round(totalL)}m");
            
            return Result.Succeeded;


        }
    }
}
