using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;



namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class ClashDetection : IExternalCommand
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

            IList<Reference> hoseRef = uidoc.Selection.PickObjects(ObjectType.Element, "Select Hose");
			
			
			
			
			string  results = "";
		
			IList<Element> rebars = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();
			
			FamilySymbol fs = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfType<FamilySymbol>().Where(x => x.FamilyName == "Sphere").First();
			
            List<XYZ> clashes = new List<XYZ>();
            

			using (Transaction t = new Transaction(doc,"Centroid")){
				
				t.Start();
				
			
			foreach (Reference sourceRefs in hoseRef) {
				
					Element sourceElement = doc.GetElement(sourceRefs);
					List<Solid> sourceSolids = GetRebarSolid(doc, sourceElement);	
				
					Rebar sourceRebar = sourceElement as Rebar;
					IList<Curve> sourceRebarCurves = sourceRebar.GetCenterlineCurves(true, false,false, MultiplanarOption.IncludeAllMultiplanarCurves,0);
					
					string sourceRebarName = sourceElement.Name.Split(':')[0];
					
					double sourceRebarRadius = UnitUtils.ConvertFromInternalUnits(sourceElement.LookupParameter("Bar Diameter").AsDouble()*0.5, UnitTypeId.Millimeters);
					
					
					
			foreach (Solid sourceSolid in sourceSolids) {
					
				
						
					

			foreach (Element element in rebars)
            {
				if (element.Id != sourceElement.Id){
	
				List<Solid> solids = GetRebarSolid(doc, element);
                
                if (solids.Count>0)
                {
                	foreach (Solid solid2 in solids) {
                	
                		try{
                		
                    Solid interSolid = BooleanOperationsUtils.ExecuteBooleanOperation(sourceSolid,solid2, BooleanOperationsType.Intersect);
					
					if (Math.Abs(interSolid.Volume)>0.001/304.8) 
					{
											
						Rebar targetRebar = element as Rebar;
						
						string targetRebarName = element.Name.Split(':')[0];
						
						double targetRebarRadius = UnitUtils.ConvertFromInternalUnits(targetRebar.LookupParameter("Bar Diameter").AsDouble()*0.5,UnitTypeId.Millimeters);
						
						XYZ cent = interSolid.ComputeCentroid();
						
						//doc.Create.NewFamilyInstance(cent, fs, StructuralType.NonStructural);
													
						XYZ cpSourceBar = GetClosestPoint(cent, sourceRebarCurves);
						
						IList<Curve> targetRebarCurves = targetRebar.GetCenterlineCurves(true, false,false, MultiplanarOption.IncludeAllMultiplanarCurves,0);
						
						XYZ cpTargetBar = GetClosestPoint(cent, targetRebarCurves);
						
						double distance = UnitUtils.ConvertFromInternalUnits(cpSourceBar.DistanceTo(cpTargetBar),UnitTypeId.Millimeters);
						
						double hardClashDistance = (sourceRebarRadius + targetRebarRadius);
						
						//TaskDialog.Show("R", distance.ToString() + ">" + hardClashDistance.ToString());
						
						if (distance > hardClashDistance){
							//not a clash
							results += "not a clash \n";
						}
						else{
							
							results += element.LookupParameter("Schedule Mark").AsValueString() + "\n";
							
                            
							//TaskDialog.Show("R", distance.ToString());
							
							//TaskDialog.Show("R",distanceToPrev.ToString());
							FamilyInstance fi = doc.Create.NewFamilyInstance(cent, fs, StructuralType.NonStructural);
                            // LocationPoint fiLocPt = fi.Location as LocationPoint
                            // XYZ filPt = fiLocPt.Point;

							if (!IsPointInList(clashes, cent)) {
									//TaskDialog.Show("R", "Place sphere");
                                    doc.Create.NewFamilyInstance(cent, fs, StructuralType.NonStructural);	    	
									clashes.Add(cent);
							    }
							
							
						}
						
						
						
						
					}
                	}
                	catch{}
                		
                	}
                }
            }
			}
								}
			}	
			t.Commit();
            
            }
			
            TaskDialog.Show("R", results);


            return Result.Succeeded;


        }//close execute


bool IsPointInList(List<XYZ> points, XYZ testPoint)
{
    double tolerance = UnitUtils.ConvertToInternalUnits(1000,UnitTypeId.Millimeters);
    return points.Any(p => p.IsAlmostEqualTo(testPoint, tolerance));
}



            public static XYZ GetClosestPoint(XYZ targetPoint, IList<Curve> curves)
    {
        XYZ closestPoint = null;
        double minDistance = double.MaxValue;

        foreach (Curve curve in curves)
        {
            // Project the target point onto the curve
            IntersectionResult result = curve.Project(targetPoint);

            if (result != null)
            {
                XYZ projectedPoint = result.XYZPoint;
                double distance = targetPoint.DistanceTo(projectedPoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = projectedPoint;
                }
                
                
            }
        }

        return closestPoint;
    }
		    
		
	private static List<Solid> GetRebarSolid(Document doc, Element element)
    
	{
        Options options = new Options { ComputeReferences = true };
        
        Rebar bar = element as Rebar;
        
        GeometryElement ge = bar.GetFullGeometryForView(doc.ActiveView);

        List<Solid> rebarSolids = new List<Solid>();
        
	    foreach (GeometryInstance gi in ge)
	    {
	        foreach (var item in gi.GetInstanceGeometry())
	        {
	            if (item is Solid)
	            {
	                
	            	rebarSolids.Add(item as Solid);
	            }
	        }
	    }
	    

	    
	    return rebarSolids;

    }
    
		    
    private static Solid GetSolid(Element element)
    {
        Options options = new Options { ComputeReferences = true };
        GeometryElement geometryElement = element.get_Geometry(options);

        if (geometryElement != null)
        {        	
            foreach (GeometryObject geoObject in geometryElement)
            {
            	Solid solid = geoObject as Solid;
            	
                if (solid != null && solid.Volume > 0)
                {
                    return solid;
                }
            }
        }
        return null;
    }
    }
}