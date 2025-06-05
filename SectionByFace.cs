using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class SectionByFace : IExternalCommand
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
					XYZ faceNormal = planarFace.FaceNormal.Normalize();
					
					XYZ faceOrigin = planarFace.Origin;
					
					
					
					
					BoundingBoxUV box = face.GetBoundingBox();
					
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
	            		    
					
					Transform sectionTransform = Transform.Identity;
					


			if (dist1 > dist2)
			{
				sectionTransform.Origin = centerish;
			}
			else
			{
				sectionTransform.Origin = trans.OfPoint(centerish);	
			}


			sectionTransform.BasisX = xDir;
					sectionTransform.BasisY = upFace;
					sectionTransform.BasisZ = viewdir; // normal is Z-direction of section box
					
					// Define the section box size (in feet)
					BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
					sectionBox.Transform = sectionTransform;
					
					 //Max/Min X = Section line Length, Max/Min Y is the height of the section box, Max/Min Z far clip          
            
					 BoundingBoxXYZ elementBbox = fa.get_BoundingBox(doc.ActiveView);
					
					 
					 
					 //double elementMinZ = elementBbox.Min.Z;
					 double width = 8000/304.8; 
					 					 
					 //double elementMaxZ = elementBbox.Max.Z;
					 double height = 8000/304.8;
					 
					 
					 
					sectionBox.Min = new XYZ(-width/2, -height/2, 0); // 10' x 10' section, 1' depth
					sectionBox.Max = new XYZ(width/2, height/2, 500/304.8);
					
					// Get a ViewFamilyType for a section
					ViewFamilyType sectionViewType = new FilteredElementCollector(doc)
					    .OfClass(typeof(ViewFamilyType))
					    .Cast<ViewFamilyType>()
					    .FirstOrDefault(x => x.ViewFamily == ViewFamily.Section);

            using (Transaction t = new Transaction(doc, "Section Face"))
            {

                t.Start();


                //						
                //				FamilyInstance familyInstance2 = doc.Create.NewFamilyInstance(
                //		                trans.OfPoint(centerish), // Location point
                //		                fs, // FamilySymbol
                //		                
                //		                StructuralType.NonStructural // Specify if it's structural or non-structural
                //		            );



                XYZ p1 = trans.OfPoint(centerish);
                XYZ p2 = p1 + xDir * 2;

                XYZ p3 = p1 + yDir * 3;

                // Create the section view
                ViewSection sectionView = ViewSection.CreateSection(doc, sectionViewType.Id, sectionBox);


                // View3D v3d = doc.ActiveView as View3D;
                // v3d.SetSectionBox(sectionView.get_BoundingBox(null));

                t.Commit();

            }
                return Result.Succeeded;
            }
    }
}