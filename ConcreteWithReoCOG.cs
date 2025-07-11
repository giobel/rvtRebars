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

            Dictionary<string, double> bbarWeight = new Dictionary<string, double>
            {
                { "N10", 0.66 },
                { "N12", 0.928 },
                { "N16", 1.649 },
                { "N20", 2.577 },
                { "N24", 3.711 },
                { "N28", 5.051 },
                { "N32", 6.597 },
                { "N36", 8.35 },
                { "N40", 10.309 }
            };

            double rebarDensity = 7852; // kg/m3
            double concreteDensity = 2400; // kg/m³


            try
            {


                Reference selectedRef = uidoc.Selection.PickObject(ObjectType.Element, "Select Segment");
                Element selectedElement = doc.GetElement(selectedRef);

                ISelectionFilter rebarFilter = new CategorySelectionFilter("Structural Rebar");

                IList<Reference> selectedReoRef = uidoc.Selection.PickObjects(ObjectType.Element,rebarFilter, "Select Reinforcement");


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

                double cogX = 0, cogY = 0, cogZ = 0, totalMass = 0;

                using (Transaction t = new Transaction(doc, "Find COG"))
                {

                    t.Start();



                    #region CONCRETE COG

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

                        //XYZ centroid = new XYZ(cogX / totalMass, cogY / totalMass, cogZ / totalMass);
                        //doc.Create.NewFamilyInstance(centroid, sphereSymbol, StructuralType.NonStructural);

                    }

                    #endregion

                    TaskDialog.Show("r", totalMass.ToString());
                    
                    #region REINFORCEMENT

                    foreach (Reference barRef in selectedReoRef)
                    {
                        Element barElement = doc.GetElement(barRef);

                        double diameter = UnitUtils.ConvertFromInternalUnits(barElement.LookupParameter("Bar Diameter").AsDouble(), UnitTypeId.Millimeters);

                        string diameterName = "N" + Math.Round(diameter, 0).ToString();

                        double length = UnitUtils.ConvertFromInternalUnits(barElement.LookupParameter("Total Bar Length").AsDouble(), UnitTypeId.Meters);

                        //double rebarWeight = bbarWeight[diameterName] * length;


                        Rebar bar = barElement as Rebar;
                        IList<Curve> rebarCls = bar.GetCenterlineCurves(
                                        true, // adjustForSelfIntersection
                                        false, // suppress hooks
                                        false, // suppress bends
                                        MultiplanarOption.IncludeAllMultiplanarCurves, // or .CurrentPlaneOnly
                                        0); // if multiplar, index of the plane

                        List<Solid> sourceSolids = Helpers.GetRebarSolid(doc, barElement);


                        for (int i = 0; i < rebarCls.Count; i++)
                        {
                            Curve curve = rebarCls[i];

                            if (curve == null) continue;

                            // Calculate the weight of the current rebar segment
                            
                            double rebarWeight =  UnitUtils.ConvertFromInternalUnits(curve.Length, UnitTypeId.Meters) * bbarWeight[diameterName];

                            XYZ midPoint = curve.Evaluate(0.5, true);

                           
                            totalMass += rebarWeight;

                            cogX += rebarWeight * midPoint.X;
                            cogY += rebarWeight * midPoint.Y;
                            cogZ += rebarWeight * midPoint.Z;

                            double rebarVol = rebarWeight / rebarDensity;
                            double rebarAsConcreteWeight = rebarVol * concreteDensity;
                            totalMass -= rebarAsConcreteWeight;

                            //subtract the centroid of the rebar as concrete from the cog calculation
                            cogX -= rebarAsConcreteWeight * midPoint.X;
                            cogY -= rebarAsConcreteWeight * midPoint.Y;
                            cogZ -= rebarAsConcreteWeight * midPoint.Z;
                        }



                    }


                    #endregion

                        XYZ centroid = new XYZ(cogX / totalMass, cogY / totalMass, cogZ / totalMass);
                        doc.Create.NewFamilyInstance(centroid, sphereSymbol, StructuralType.NonStructural);


                    t.Commit();
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


        }//close execute


    }
}