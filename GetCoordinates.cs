using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class GetCoordinates : IExternalCommand
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

            Reference eleRef = uidoc.Selection.PickObject(ObjectType.Element, "Pick an element");

            Location loc = doc.GetElement(eleRef).Location;

            if (loc != null)
            {
                LocationPoint locpt = loc as LocationPoint;
                XYZ pt = locpt.Point;
                Clipboard.SetText($"{UnitUtils.ConvertFromInternalUnits(pt.X,UnitTypeId.Millimeters)},{UnitUtils.ConvertFromInternalUnits(pt.Y,UnitTypeId.Millimeters)},{UnitUtils.ConvertFromInternalUnits(pt.Z,UnitTypeId.Millimeters)}");
                TaskDialog.Show("Messgae", "Copied to clipboard!");
                
            }

            return Result.Succeeded;
        }
            


    }
}