#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class SelectRebars : IExternalCommand
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

            ISelectionFilter beamFilter = new CategorySelectionFilter("Structural Rebar");

            IList<Reference> refs = uidoc.Selection.PickObjects(ObjectType.Element, beamFilter, "Select some rebars");

            uidoc.Selection.SetElementIds(refs.Select(x => doc.GetElement(x).Id).ToList());

            return Result.Succeeded;
        }
    }

        public class CategorySelectionFilter : ISelectionFilter
    {

        public string catNameChosen { get; set; }

        public CategorySelectionFilter(string catName)
        {
            this.catNameChosen = catName;
        }

        public bool AllowElement(Element e)
        {

            //if (e.Category.Name == "Structural Framing")
            if (e.Category != null & e.Category.Name == catNameChosen)
            {
                return true;
            }
            return false;
        }


        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }

    }//close class
}