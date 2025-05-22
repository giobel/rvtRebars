using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq; 
using Autodesk.Revit.DB.Structure;
using System;

namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class CreateSections : IExternalCommand
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


            ViewFamilyType vft = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);


            ICollection<ElementId> selectedbars = uidoc.Selection.GetElementIds();

            using (Transaction t = new Transaction(doc, "Create Sections"))
            {

                t.Start();


                foreach (var bar in selectedbars)
                {

                    Rebar r = doc.GetElement(bar) as Rebar;

                    IList<Curve> centerlines = r.GetCenterlineCurves(false, true, true, MultiplanarOption.IncludeOnlyPlanarCurves, 0);

                    List<XYZ> pts = new List<XYZ>();

                    foreach (var curve in centerlines)
                    {

                        pts.Add(curve.GetEndPoint(0));
                        pts.Add(curve.GetEndPoint(1));
                    }

                    Line line = Line.CreateBound(pts.First(), pts.Last());

                    XYZ xyzDir = line.Direction.Normalize();

                    if (((Math.Abs(xyzDir.X) >= Math.Abs(xyzDir.Y)) && xyzDir.X < 0) || ((Math.Abs(xyzDir.Y) > Math.Abs(xyzDir.X)) && xyzDir.Y < 0))
                    {
                        line = line.CreateReversed() as Line;

                    }



                    XYZ p = line.GetEndPoint(0);
                    XYZ q = line.GetEndPoint(1);
                    XYZ v = p - q; // p point 0 - q point 1 - view direction up. 

                    double halfLength = v.GetLength() / 2;
                    //double offset = 0; // offset by 3 feet. 
                    //double farClipOffset = 1;

                    double horizontalOffset = 2;
                    double bottomLevel = 25 / 304.8 * 1000;
                    double topLevel = 35 / 304.8 * 1000;
                    double sectionPosition = 10;
                    double farClipOffset = 500;

                    //Max/Min X = Section line Length, Max/Min Y is the height of the section box, Max/Min Z far clip
                    XYZ min = new XYZ(-halfLength - horizontalOffset, bottomLevel, -sectionPosition);
                    XYZ max = new XYZ(halfLength + horizontalOffset, topLevel, farClipOffset);

                    XYZ midpoint = q + 0.5 * v; // q get lower midpoint. 
                    XYZ walldir = v.Normalize();
                    XYZ up = XYZ.BasisZ;
                    XYZ viewdir = walldir.CrossProduct(up);

                    Transform transf = Transform.Identity;
                    transf.Origin = midpoint;
                    transf.BasisX = walldir;
                    transf.BasisY = up;
                    transf.BasisZ = viewdir;

                    BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
                    sectionBox.Transform = transf;
                    sectionBox.Min = min; // scope box start 
                    sectionBox.Max = max; // scope box end


                    ViewSection vs = ViewSection.CreateSection(doc, vft.Id, sectionBox);

                    vs.Name = "Test100";

                }//close foreach

                t.Commit();
            }

            return Result.Succeeded;
        }//close create sections
    }
}