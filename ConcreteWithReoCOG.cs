using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq; 
using Autodesk.Revit.DB.Structure;

namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class ConcreteWithReoCOG : IExternalCommand
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

	IList<Reference> elementsInView= uidoc.Selection.PickObjects(ObjectType.Element, "Select Elements");
				       
			FamilySymbol fs = GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");
				        
				        using ( Transaction t = new Transaction(doc, "Find COG")) {
	        	
	        	t.Start();
	        	
			Element concrete = doc.GetElement(elementsInView.First());
			
			FamilyInstance fi = concrete as FamilyInstance;
			
			FamilyInstance subConcrete = doc.GetElement(fi.GetSubComponentIds().First()) as FamilyInstance;
			
			Options options = new Options { ComputeReferences = true, View = doc.ActiveView };
			
			if (subConcrete != null){
				
				
			

			GeometryElement geometryElement = subConcrete.get_Geometry(options);
		
			foreach (GeometryObject geoObject in geometryElement){
            	
					//Solid solid = geoObject as Solid;
				
					Solid solid = null;
				
					GeometryInstance gi = geoObject as GeometryInstance;
					
					foreach (var giElement in gi.GetInstanceGeometry()) {
					
						try{
						 Solid gisolid = giElement as Solid;
						 if (gisolid != null && gisolid.Volume > 0)
						 	solid = gisolid;
							
						}
						catch{}
						
					}
				
				
				
            	
            
            	
            	
                if (solid != null && solid.Volume > 0)
                {
                	XYZ cogPt = solid.ComputeCentroid();
                	
                		        	    // Create the family instance
		            FamilyInstance familyInstance = doc.Create.NewFamilyInstance(
		                cogPt, // Location point
		                fs, // FamilySymbol
		                
		                StructuralType.NonStructural // Specify if it's structural or non-structural
		            );
                		        	    
                }
            	
            	
            }
            
            }//close foreach
			
				
	        	t.Commit();
	        }

            
            return Result.Succeeded;


        }//close execute

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
    }
}