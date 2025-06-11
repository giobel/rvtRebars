using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class ColorBySlice : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector elementsInView = new FilteredElementCollector(doc);
            FillPatternElement solidFillPattern = elementsInView.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);


            List<BuiltInCategory> builtInCats = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Rebar
            };




            ElementMulticategoryFilter filter1 = new ElementMulticategoryFilter(builtInCats);


            IList<Element> visibleElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(filter1).WhereElementIsNotElementType().ToElements();

            string colorParam = "FBA_Slice";
            
            var grouped = visibleElements.GroupBy(x => x.LookupParameter(colorParam).AsValueString());

            //Random pRand = new Random();
            var md5 = MD5.Create();
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceForegroundPatternId(solidFillPattern.Id);
            

            string error = "";
            using (Transaction t = new Transaction(doc, "Override Colors by Slice Number"))
            {
                t.Start();
                foreach (var element in grouped)
                {


                    var firstElement = element.First();
                    
                    string colorName = "xx";
                    	
     
                    	colorName = firstElement.LookupParameter(colorParam).AsValueString();
                    	
 

                    if (colorName == null)
                    {
                        colorName = "null";
                    }
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(colorName));
                    
                    
                    Autodesk.Revit.DB.Color pcolor = new Autodesk.Revit.DB.Color(hash[0], hash[1], hash[2]);

                    ogs.SetSurfaceForegroundPatternColor(pcolor);

                    ogs.SetProjectionLineColor(pcolor);
                    

                    try
                    {
                        //foreach (FamilyInstance item in element)
                        foreach (var item in element)
                        {
                            doc.ActiveView.SetElementOverrides(item.Id, ogs);

                        }

                    }
                    catch (System.Exception ex)
                    {
                        error = ex.Message;
                    }
                }

                t.Commit();
            }

            if (error != "")
            {
                TaskDialog.Show("Error", error);
            }

            
            return Result.Succeeded;


        }//close execute
    }
}