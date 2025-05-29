using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq; 
using Autodesk.Revit.DB.Structure;
using System;

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


            try
            {
                Reference selectedRef = uidoc.Selection.PickObject(ObjectType.Element, "Select Element");
                Element selectedElement = doc.GetElement(selectedRef);

                FamilyInstance parentInstance = selectedElement as FamilyInstance;
                if (parentInstance == null)
                {
                    TaskDialog.Show("Error", "Selected element is not a FamilyInstance.");
                    return Result.Failed;
                }

                FamilySymbol sphereSymbol = Helpers.GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");
                if (sphereSymbol == null)
                {
                    TaskDialog.Show("Error", "Sphere family not found in the project.");
                    return Result.Failed;
                }


                Options options = new Options { ComputeReferences = true, View = doc.ActiveView };
                double concreteDensity = 2400; // kg/m³

                double cogX = 0, cogY = 0, cogZ = 0, totalMass = 0;


                using (Transaction tx = new Transaction(doc, "Place COG Marker"))
                {
                    tx.Start();

                    ICollection<ElementId> subComponentIds = parentInstance.GetSubComponentIds();

                    foreach (ElementId id in subComponentIds)
                    {
                        Element subElement = doc.GetElement(id);
                        FamilyInstance subInstance = subElement as FamilyInstance;
                        if (subInstance == null) continue;

                        GeometryElement geom = subInstance.get_Geometry(options);
                        if (geom == null) continue;

                        foreach (GeometryObject obj in geom)
                        {
                            Helpers.ProcessGeometryObject(obj, concreteDensity, ref cogX, ref cogY, ref cogZ, ref totalMass);
                        }

                        if (totalMass == 0)
                        {
                            TaskDialog.Show("Error", "No valid geometry found to compute COG.");
                            return Result.Failed;
                        }

                        XYZ centroid = new XYZ(cogX / totalMass, cogY / totalMass, cogZ / totalMass);
                        doc.Create.NewFamilyInstance(centroid, sphereSymbol, StructuralType.NonStructural);

                    }


                    tx.Commit();
                }

                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }




        }

    }
}