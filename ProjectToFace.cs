

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class ProjectToFace : IExternalCommand
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

            FamilySymbol fs = Helpers.GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");


            Reference faceRef = uidoc.Selection.PickObject(ObjectType.Face, "Pick a face");

            GeometryObject geoObj = doc.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef);
            Face face = geoObj as Face;

			XYZ computedFaceNormal = face.ComputeNormal(Helpers.GetCenterOfFace(face)).Normalize();
            XYZ centerFace = face.Evaluate(Helpers.GetCenterOfFace(face));

            //TaskDialog.Show("R", $"origin {centerFace}");

            Reference eleRef = uidoc.Selection.PickObject(ObjectType.Element, "Pick an element");

            Location loc = doc.GetElement(eleRef).Location;

            LocationPoint locpt = loc as LocationPoint;
            XYZ pt = locpt.Point;
            XYZ pointOnFace = ProjectPointOntoFace(face, pt, computedFaceNormal);
            //TaskDialog.Show("R", $"point {pt}");

            double xDist = centerFace.X - pointOnFace.X;
            double yDist = centerFace.Y - pointOnFace.Y;

            //TaskDialog.Show("R", $"{xDist},{yDist}");

            double angle = -7.64081;
            double radAngle = Math.PI / 180 * angle;

            double transXdist = xDist * Math.Cos(radAngle) - yDist * Math.Sin(radAngle);
            double transYdist = xDist * Math.Sin(radAngle) + yDist * Math.Cos(radAngle);

            TaskDialog.Show("Result", $"offset X {UnitUtils.ConvertFromInternalUnits(transXdist, UnitTypeId.Millimeters)}\n offset Y {UnitUtils.ConvertFromInternalUnits(transYdist, UnitTypeId.Millimeters)}");

            XYZ axisDirection = new XYZ(0, 0, 1); // Z-axis
            Line axis = Line.CreateUnbound(pointOnFace, axisDirection);

            using (Transaction t = new Transaction(doc, "Project to Face"))
            {

                t.Start();

                FamilyInstance projectedLocationFamily = doc.Create.NewFamilyInstance(
                        pointOnFace, // Location point
                        fs, // FamilySymbol

                        StructuralType.NonStructural // Specify if it's structural or non-structural
                    );


                ElementTransformUtils.RotateElement(doc, projectedLocationFamily.Id, axis, -radAngle);

                t.Commit();

            }

            return Result.Succeeded;
        }

        public XYZ ProjectPointOntoFace(Face face, XYZ point, XYZ direction)
        {
            // Create a ray from the point in the given direction
            Line projectionLine = Line.CreateUnbound(point, direction);

            // Intersect the ray with the face
            IntersectionResult result = face.Project(projectionLine.Origin);

            if (result != null)
            {
                // This gives the closest point on the face along a normal projection (not custom direction)
                return result.XYZPoint;
            }

            // If projection via face.Project fails (it's normal projection), try geometric intersection
            IntersectionResultArray resultArray;
            SetComparisonResult compareResult = face.Intersect(projectionLine, out resultArray);

            if (compareResult == SetComparisonResult.Overlap && resultArray != null && resultArray.Size > 0)
            {
                return resultArray.get_Item(0).XYZPoint;
            }

            // Projection failed
            return null;
        }

    }
}