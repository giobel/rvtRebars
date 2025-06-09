using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class RebarsSections : IExternalCommand
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

            FamilySymbol fs = Helpers.GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");
            Reference barRefs = uidoc.Selection.PickObject(ObjectType.Element, "Pick bar in slice");

            ViewFamilyType sectionViewType = new FilteredElementCollector(doc)
    .OfClass(typeof(ViewFamilyType))
    .Cast<ViewFamilyType>()
    .FirstOrDefault(x => x.ViewFamily == ViewFamily.Section);


            using (Transaction t = new Transaction(doc, "Addpoint"))
            {

                t.Start();

                Rebar rebar = doc.GetElement(barRefs) as Rebar;

                IList<Curve> curves = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0);


                List<XYZ> vertex = new List<XYZ>();

                foreach (Curve curve in curves)
                {

                    vertex.Add(curve.GetEndPoint(0));
                    vertex.Add(curve.GetEndPoint(1));
                }

                PolyLine pl = PolyLine.Create(vertex);




                Plane p = Helpers.GetRebarSketchPlane(rebar);

                TaskDialog.Show("R", p.ToString());

                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();

                Transform sectionTransform = Transform.Identity;
                sectionTransform.Origin = p.Origin;
                sectionTransform.BasisX = p.XVec;
                sectionTransform.BasisY = p.YVec;
                sectionTransform.BasisZ = p.Normal; // normal is Z-direction of section box


                sectionBox.Transform = sectionTransform;

                //double elementMinZ = elementBbox.Min.Z;
                double width = 8000 / 304.8;

                //double elementMaxZ = elementBbox.Max.Z;
                double height = 8000 / 304.8;



                sectionBox.Min = new XYZ(-width / 2, -height / 2, 0); // 10' x 10' section, 1' depth
                sectionBox.Max = new XYZ(width / 2, height / 2, 500 / 304.8);

                ViewSection sectionView = ViewSection.CreateSection(doc, sectionViewType.Id, sectionBox);



                t.Commit();
            }


            return Result.Succeeded;
        }
    }
}