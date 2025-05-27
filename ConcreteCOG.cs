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
    public class ConcreteCOG : IExternalCommand
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

	        Reference elementsInView= uidoc.Selection.PickObject(ObjectType.Element, "Select Elemen");
				       
			FamilySymbol fs = GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");

            


            double concreteDensity = 2400;
            
			Options options = new Options { ComputeReferences = true, View = doc.ActiveView };

            using (Transaction t = new Transaction(doc, "Find Segment COG"))
            {

                t.Start();

                Element concrete = doc.GetElement(elementsInView);

                FamilyInstance fi = concrete as FamilyInstance;

                ICollection<ElementId> eids = fi.GetSubComponentIds();

                //TaskDialog.Show("R", eids.Count.ToString());



                double cogX = 0;
                double cogY = 0;
                double cogZ = 0;
                double totalMass = 0;


                //try to get the geomery element first, if it fails use the element subcomponents
                foreach (var eid in eids)
                {


                    FamilyInstance subConcrete = doc.GetElement(eid) as FamilyInstance;

                    if (subConcrete != null)
                    {



                        GeometryElement geometryElement = subConcrete.get_Geometry(options);

                        //TaskDialog.Show("R", geometryElement.Count().ToString());

                        foreach (GeometryObject geoObject in geometryElement)
                        {

                            if (geoObject is Solid)
                            {

                                Solid gisolid = geoObject as Solid;

                                try
                                {

                                    if (gisolid != null && gisolid.Volume > 0)
                                    {

                                        XYZ cogPt = gisolid.ComputeCentroid();

                                        double mass = UnitUtils.ConvertFromInternalUnits(gisolid.Volume, UnitTypeId.CubicMeters) * concreteDensity;

                                        //TaskDialog.Show("R", mass.ToString());


                                        cogX += cogPt.X * mass;
                                        cogY += cogPt.Y * mass;
                                        cogZ += cogPt.Z * mass;
                                        totalMass += mass;
                                    }

                                }

                                catch { }
                            }

                            else
                            {



                                try
                                {

                                    GeometryInstance gi = geoObject as GeometryInstance;

                                    foreach (var giElement in gi.GetInstanceGeometry()) //get instance geometry does not existing in this object
                                    {

                                        try
                                        {
                                            Solid gisolid = giElement as Solid;
                                            if (gisolid != null && gisolid.Volume > 0)
                                            {
                                                XYZ cogPt = gisolid.ComputeCentroid();

                                                double mass = UnitUtils.ConvertFromInternalUnits(gisolid.Volume, UnitTypeId.CubicMeters) * concreteDensity;

                                                //TaskDialog.Show("R", mass.ToString());

                                                cogX += cogPt.X * mass;
                                                cogY += cogPt.Y * mass;
                                                cogZ += cogPt.Z * mass;

                                                totalMass += mass;


                                            }

                                        }
                                        catch
                                        {//TaskDialog.Show("R", "Something wrojng"); 
                                        }

                                    }

                                }

                                catch
                                {

                                }

                            }//close else

                        }





                    }


                }//close foreach


                XYZ centroid = new XYZ(cogX / totalMass, cogY / totalMass, cogZ / totalMass);

                //TaskDialog.Show("R", (totalMass).ToString());

                FamilyInstance familyInstance = doc.Create.NewFamilyInstance(
                   centroid, // Location point
                   fs, // FamilySymbol

                 StructuralType.NonStructural // Specify if it's structural or non-structural
                 );


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