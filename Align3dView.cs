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
	public class Align3dView : IExternalCommand
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

			BoundingBoxXYZ elementBbox = fa.get_BoundingBox(doc.ActiveView);

			double height = elementBbox.Max.Z - elementBbox.Min.Z;

			//TaskDialog.Show("R", height.ToString());

			GeometryObject geoObj = doc.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef);
			Face face = geoObj as Face;

			FamilySymbol fs = Helpers.GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");

			// Assuming 'face' is a planar Face object you've selected or found
			PlanarFace planarFace = face as PlanarFace;
			XYZ faceNormal = planarFace.FaceNormal.Normalize();


			//UV faceCenter = (box.Max + box.Min) / 2;

			//https://forums.autodesk.com/t5/revit-api-forum/how-to-find-center-of-face/td-p/9210188
			UV faceCenter = Helpers.GetCenterOfFace(face);

			XYZ centerish = face.Evaluate(faceCenter);




			XYZ computedFaceNormal = face.ComputeNormal(faceCenter).Normalize();

			Transform trans = (selectedElement as FamilyInstance).GetTransform();

			double dist1 = centerish.DistanceTo(XYZ.Zero);
			double dist2 = trans.OfPoint(centerish).DistanceTo(XYZ.Zero);

			XYZ end = centerish + computedFaceNormal;

			if (dist1 < dist2)
			{
				computedFaceNormal = trans.OfVector(computedFaceNormal);
				faceNormal = trans.OfVector(faceNormal);
				end = trans.OfPoint(centerish) + computedFaceNormal;
			}





			// Create an orthonormal basis for the section
			XYZ xDir = computedFaceNormal.CrossProduct(XYZ.BasisZ).Normalize();

			//if (xDir.IsZeroLength()) xDir = computedFaceNormal.CrossProduct(XYZ.BasisY).Normalize(); // Avoid parallel case

			XYZ yDir = computedFaceNormal.CrossProduct(xDir).Normalize();

			// Create transform from face
			//Transform sectionTransform = Transform.Identity;
			//sectionTransform.Origin = faceOrigin;

			//XYZ up = XYZ.BasisZ;

			XYZ upFace = xDir.CrossProduct(computedFaceNormal);

			XYZ viewdir = xDir.CrossProduct(upFace);


			XYZ boxCenter = XYZ.Zero;



			if (dist1 > dist2)
			{
				boxCenter = centerish;
			}
			else
			{
				boxCenter = trans.OfPoint(centerish);
			}






			//double elementMinZ = elementBbox.Min.Z;
			double width = 8000 / 304.8;

			//double elementMaxZ = elementBbox.Max.Z;
			//double height = 8000 / 304.8;



			using (Transaction t = new Transaction(doc, "Section Face"))
			{

				t.Start();

														
								FamilyInstance familyInstance2 = doc.Create.NewFamilyInstance(
						                boxCenter, // Location point
						                fs, // FamilySymbol
						                
						                StructuralType.NonStructural // Specify if it's structural or non-structural
						            );

				
				    


				Transform transf3D = Transform.Identity;
				transf3D.Origin = boxCenter;
				transf3D.BasisX = xDir;
				transf3D.BasisY = upFace;
				transf3D.BasisZ = viewdir; // normal is Z-direction of section box


				

					Plane planeXY = Plane.CreateByNormalAndOrigin(upFace, transf3D.Origin);
					Plane planeXZ = Plane.CreateByNormalAndOrigin(viewdir, transf3D.Origin);


			doc.Create.NewModelCurve(
				Line.CreateBound(transf3D.Origin, transf3D.Origin+ upFace.Normalize()*(5000/304.8)),
					SketchPlane.Create(doc, planeXZ)
				);

						doc.Create.NewModelCurve(
				Line.CreateBound(transf3D.Origin, transf3D.Origin+ upFace.Normalize()*(-5000/304.8)),
					SketchPlane.Create(doc, planeXZ)
				);


				BoundingBoxXYZ sectionBox3D = new BoundingBoxXYZ();
				sectionBox3D.Transform = transf3D;


				sectionBox3D.Min = new XYZ(-width / 2, -100/304.8, -5000/304.8); // height is already in feet
				sectionBox3D.Max = new XYZ(width / 2, 3000/304.8, 5000/304.8);

				View3D current3DView = doc.ActiveView as View3D;

				current3DView.SetSectionBox(sectionBox3D);

				//https://sharpbim.hashnode.dev/aligning-3d-views
				var ori = new ViewOrientation3D(transf3D.Origin, upFace, viewdir);

				current3DView.SetOrientation(ori);

				t.Commit();

				//uidoc.ActiveView = sectionView;



				
			}
			return Result.Succeeded;
		}
			


    }
}