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
    public class FaceCenter : IExternalCommand
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

            Reference faceRef = uidoc.Selection.PickObject(ObjectType.Face, "Pick a face");
            
            Element selectedElement = doc.GetElement(faceRef);

            long rootElement = selectedElement.Id.Value - 1;

            FamilyInstance fa = selectedElement as FamilyInstance;
            XYZ rotation = fa.HandOrientation;

            GeometryObject geoObj = doc.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef);
            Face face = geoObj as Face;

            FamilySymbol fs = Helpers.GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");

            // Assuming 'face' is a planar Face object you've selected or found
            //PlanarFace planarFace = face as PlanarFace;

            //XYZ faceNormal = planarFace.FaceNormal.Normalize();
            XYZ faceNormal = face.ComputeNormal(Helpers.GetCenterOfFace(face)).Normalize();

            //XYZ faceOrigin = planarFace.Origin;
            
            BoundingBoxUV box = face.GetBoundingBox();

            //UV faceCenter = (box.Max + box.Min) / 2;

            //https://forums.autodesk.com/t5/revit-api-forum/how-to-find-center-of-face/td-p/9210188
            UV faceCentere = Helpers.GetCenterOfFace(face);

            //XYZ centerish = face.Evaluate(faceCentere);

            //TaskDialog.Show("R", $"{centerish.X}, {centerish.Y}");

            XYZ centerFace = face.Evaluate(faceCentere);

           

            //TaskDialog.Show("R", $"{centerFace.X}, {centerFace.Y}");

            //XYZ computedFaceNormal = face.ComputeNormal(faceCenter).Normalize();

            Transform trans = (selectedElement as FamilyInstance).GetTransform();

            XYZ axisDirection = new XYZ(0, 0, 1); // Z-axis
            Line axis = Line.CreateUnbound(centerFace, axisDirection);

            double radAngle = Math.PI - Helpers.SignedAngle(XYZ.BasisX, rotation, faceNormal);

            using (Transaction t = new Transaction(doc, "Section Face"))
            {

                t.Start();

                double dist1 = centerFace.DistanceTo(XYZ.Zero);
                double dist2 = trans.OfPoint(centerFace).DistanceTo(XYZ.Zero);

                TaskDialog.Show("R", $"{dist1} vs {dist2}");

               


                if (dist1 > dist2)
                {
                    FamilyInstance sphere = doc.Create.NewFamilyInstance(
                        centerFace, // Location point
                        fs, // FamilySymbol
                        StructuralType.NonStructural // Specify if it's structural or non-structural
                    );
                }
                else
                {
                    FamilyInstance sphere1 = doc.Create.NewFamilyInstance(
                        trans.OfPoint(centerFace), // Location point
                        fs, // FamilySymbol

                        StructuralType.NonStructural // Specify if it's structural or non-structural
                    );
                }



                    





                //ElementTransformUtils.RotateElement(doc, sphere.Id, axis, -radAngle);
                
                // Create color (Red in this case)
                Color redColor = new Color(255, 0, 0); // RGB 0-255

                // Create override settings
                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                ogs.SetSurfaceForegroundPatternColor(redColor);

                FilteredElementCollector fillCollector = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).WhereElementIsNotElementType();

                FillPatternElement solidFill = fillCollector.Cast<FillPatternElement>().FirstOrDefault(f => f.GetFillPattern().IsSolidFill);

                ogs.SetSurfaceForegroundPatternId(solidFill.Id);

                //doc.ActiveView.SetElementOverrides(sphere.Id, ogs);

                t.Commit();

            }
            return Result.Succeeded;
        }
            
public static XYZ GetBoundingBoxCenter(BoundingBoxXYZ bbox)
{
    if (bbox == null) return null;

    XYZ min = bbox.Min;
    XYZ max = bbox.Max;

    return new XYZ(
        (min.X + max.X) / 2,
        (min.Y + max.Y) / 2,
        (min.Z + max.Z) / 2
    );
}

    }
}