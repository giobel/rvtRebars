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
            FamilyInstance fa = selectedElement as FamilyInstance;

            GeometryObject geoObj = doc.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef);
            Face face = geoObj as Face;

            FamilySymbol fs = Helpers.GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");

            // Assuming 'face' is a planar Face object you've selected or found
            PlanarFace planarFace = face as PlanarFace;
            //XYZ faceNormal = planarFace.FaceNormal.Normalize();

            //XYZ faceOrigin = planarFace.Origin;




            BoundingBoxUV box = face.GetBoundingBox();

            UV faceCenter = (box.Max + box.Min) / 2;

            //https://forums.autodesk.com/t5/revit-api-forum/how-to-find-center-of-face/td-p/9210188
            UV faceCentere = GetCenterOfFace(face);

            //XYZ centerish = face.Evaluate(faceCentere);

            //TaskDialog.Show("R", $"{centerish.X}, {centerish.Y}");

            XYZ centerFace = face.Evaluate(faceCentere);

            //TaskDialog.Show("R", $"{centerFace.X}, {centerFace.Y}");

            //XYZ computedFaceNormal = face.ComputeNormal(faceCenter).Normalize();

            Transform trans = (selectedElement as FamilyInstance).GetTransform();


            
            using (Transaction t = new Transaction(doc, "Section Face"))
            {

                t.Start();

                FamilyInstance familyInstance3 = doc.Create.NewFamilyInstance(
                        centerFace, // Location point
                        fs, // FamilySymbol

                        StructuralType.NonStructural // Specify if it's structural or non-structural
                    );
                t.Commit();

            }
            return Result.Succeeded;
        }
            
            public UV GetCenterOfFace(Face myFace)
{
    double uMin = double.MaxValue;
    double uMax = double.MinValue;
    double vMin = double.MaxValue;
    double vMax = double.MinValue;

    foreach (EdgeArray edgeLoop in myFace.EdgeLoops)
    {
        foreach (Edge edge in edgeLoop)
        {
            IList<UV> edgeUVs = edge.TessellateOnFace(myFace);
            foreach (UV uv in edgeUVs)
            {
                uMin = Math.Min(uMin, uv.U);
                uMax = Math.Max(uMax, uv.U);
                vMin = Math.Min(vMin, uv.V);
                vMax = Math.Max(vMax, uv.V);
            }
        }
    }

    // Compute center as midpoint (not size difference)
    UV center = new UV((uMin + uMax) / 2, (vMin + vMax) / 2);

    return center;
}

    }
}