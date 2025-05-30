using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;


namespace rvtRebars
{
    class Helpers
    {
        
 public static void ProcessGeometryObject(
            GeometryObject obj,
            double density,
            ref double cogX,
            ref double cogY,
            ref double cogZ,
            ref double totalMass)
        {
            Solid solid = obj as Solid;
            if (solid != null && solid.Volume > 0)
            {
                AddSolidMass(solid, density, ref cogX, ref cogY, ref cogZ, ref totalMass);
                return;
            }

            GeometryInstance geomInstance = obj as GeometryInstance;
            if (geomInstance != null)
            {
                GeometryElement instGeom = geomInstance.GetInstanceGeometry();
                foreach (GeometryObject instObj in instGeom)
                {
                    Solid instSolid = instObj as Solid;
                    if (instSolid != null && instSolid.Volume > 0)
                    {
                        AddSolidMass(instSolid, density, ref cogX, ref cogY, ref cogZ, ref totalMass);
                    }
                }
            }
        }

        private static void AddSolidMass(
            Solid solid,
            double density,
            ref double cogX,
            ref double cogY,
            ref double cogZ,
            ref double totalMass)
        {
            XYZ centroid = solid.ComputeCentroid();
            double volume = UnitUtils.ConvertFromInternalUnits(solid.Volume, UnitTypeId.CubicMeters);
            double mass = volume * density;

            cogX += centroid.X * mass;
            cogY += centroid.Y * mass;
            cogZ += centroid.Z * mass;
            totalMass += mass;
        }

    public static List<Solid> GetRebarSolid(Document doc, Element element)
    
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
    
            public static FamilySymbol GetFamilySymbolByName(Document doc, string familyName, string symbolName)
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
		
    }
}