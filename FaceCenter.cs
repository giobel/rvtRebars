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
					XYZ faceNormal = planarFace.FaceNormal.Normalize();
					
					XYZ faceOrigin = planarFace.Origin;
					
					
					
					
					BoundingBoxUV box = face.GetBoundingBox();
					
                    UV faceCenter = (box.Max + box.Min) / 2;
                    
					XYZ centerish = face.Evaluate(faceCenter);
                    
					
					XYZ computedFaceNormal = face.ComputeNormal(faceCenter).Normalize();
					
	                Transform trans = (selectedElement as FamilyInstance).GetTransform();
	                

            using (Transaction t = new Transaction(doc, "Section Face"))
            {

                t.Start();


                						
                				FamilyInstance familyInstance2 = doc.Create.NewFamilyInstance(
                		                trans.OfPoint(centerish), // Location point
                		                fs, // FamilySymbol
                		                
                		                StructuralType.NonStructural // Specify if it's structural or non-structural
                		            );


                t.Commit();

            }
                return Result.Succeeded;
            }
    }
}