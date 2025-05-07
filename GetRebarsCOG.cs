using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB.Structure;

namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class GetRebarsCOG : IExternalCommand
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

            ICollection<Element> rebarsInView = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();
			
	        double totalMass = 0;
	        double xWeightedSum = 0;
	        double yWeightedSum = 0;
	        double zWeightedSum = 0;
			
	        FamilySymbol fs = GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");

	        	
			foreach (var element in rebarsInView) {
				
	        	
	        		
	        	List<Solid> sourceSolids = GetRebarSolid(doc, element);
	        		
				BoundingBoxXYZ bbox = element.get_BoundingBox(doc.ActiveView);
												
	           	//XYZ pt1 = bbox.Min;				           	
	           	//XYZ pt3 = bbox.Max;
				
	           	//bbox center --> incorrect results
	           	//XYZ bboxCenter = (pt3 + pt1)/2;
	           	
	           	//if (sourceSolids.Count == 1){
	           	
	           	foreach (var solidBar in sourceSolids) {
	           		
	           		
	           		try{
	           		
	           	XYZ bboxCenter = solidBar.ComputeCentroid();
	           		
//	           	doc.Create.NewFamilyInstance(
//		                bboxCenter, // Location point
//		                fs, // FamilySymbol
//		                
//		                StructuralType.NonStructural // Specify if it's structural or non-structural
//		            );

				//double volume = element.LookupParameter("Reinforcement Volume").AsDouble();
				double volume = solidBar.Volume;
				
				double convertedVolume = UnitUtils.ConvertFromInternalUnits(volume, UnitTypeId.CubicMeters);
				double density = 7.850;
				double tonnes = convertedVolume*density;
				totalMass += tonnes;
							
				xWeightedSum += tonnes * bboxCenter.X;
	            yWeightedSum += tonnes * bboxCenter.Y;
	            zWeightedSum += tonnes * bboxCenter.Z;
	            
	            
	           		}
	           	
	           	catch{
	           		
	           		//TaskDialog.Show("R", element.Id.ToString());
	           		
	           		throw new Exception ("Error");
	           	}
	           		
	           	}
//	           	}
//
//	           	else{
//	           		TaskDialog.Show("E", sourceSolids.Count.ToString());
//	           		throw new Exception("Too many solids");
//	           	}

	           		
	           	}
	        	

			
		    // Calculate the COG coordinates
	        double xCog = xWeightedSum / totalMass;
	        double yCog = yWeightedSum / totalMass;
	        double zCog = zWeightedSum / totalMass;
	        
	        
	        	
	        XYZ cogPt = new XYZ(xCog, yCog,zCog);
	        
	        using ( Transaction t = new Transaction(doc, "Find COG")) {
	        	
	        	t.Start();
	        	
	        	    // Create the family instance
		            FamilyInstance familyInstance = doc.Create.NewFamilyInstance(
		                cogPt, // Location point
		                fs, // FamilySymbol
		                
		                StructuralType.NonStructural // Specify if it's structural or non-structural
		            );
	        	
	        	
	        	t.Commit();
	        }
	        
	        ProjectLocation pl = doc.ActiveProjectLocation;
	        Transform ttr = pl.GetTotalTransform().Inverse;
	        cogPt = ttr.OfPoint(cogPt);
	       	
	        TaskDialog.Show("R", String.Format("Center of Gravity: X = {0}, Y = {1}, Z = {2}",
	                                           UnitUtils.ConvertFromInternalUnits(cogPt.X, UnitTypeId.Meters), 
	                                           UnitUtils.ConvertFromInternalUnits(cogPt.Y, UnitTypeId.Meters), 
	                                           UnitUtils.ConvertFromInternalUnits(cogPt.Z, UnitTypeId.Meters)
	                                          )
	                                         );

                                             return Result.Succeeded;
        }

    private FamilySymbol GetFamilySymbolByName(Document doc, string familyName, string symbolName)
    {
        FilteredElementCollector collector = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol));

        foreach (FamilySymbol symbol in collector)
        {
            if (symbol.FamilyName == familyName && symbol.Name == symbolName)
            {
                return symbol;
            }
        }

        return null;
    }

    private static List<Solid> GetRebarSolid(Document doc, Element element)
    
	{
        Options options = new Options { ComputeReferences = true };
        
        Rebar bar = element as Rebar;
        
        GeometryElement ge = bar.GetFullGeometryForView(doc.ActiveView);
        
        //TaskDialog.Show("R", ge.ToString());

        List<Solid> rebarSolids = new List<Solid>();
               
        	
        	
	    foreach (var gi in ge)
	    {
	    	
	    	if (gi is GeometryInstance)
	    		
	    	{
	    	
	    		GeometryInstance geoInst = gi as GeometryInstance;
	    	
	        foreach (var item in geoInst.GetInstanceGeometry())
	        {
	            if (item is Solid)
	            {
	                
	            	rebarSolids.Add(item as Solid);
	            }
	        }
	    	}
	    	
	    }
	    
	    return rebarSolids;

    }
    }
}