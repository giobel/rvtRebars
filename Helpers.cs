using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;


namespace rvtRebars
{
    class Helpers
    {


        public static Plane GetRebarSketchPlane(Rebar rebar)
        {
            // Get the centerline curves of the rebar
            IList<Curve> curves = rebar.GetCenterlineCurves(
                true,  // useGeometry
                false, // forVisualization
                false, // include hooks
                MultiplanarOption.IncludeOnlyPlanarCurves,
                0      // specific bar position (optional)
            );

            if (curves == null || curves.Count == 0)
                return null;

            // Use the first curve to define the plane
            Curve firstCurve = curves[0];

            // Get start and end point
            XYZ p0 = firstCurve.GetEndPoint(0);
            XYZ p1 = firstCurve.GetEndPoint(1);
            XYZ tangent = (p1 - p0).Normalize();

            // Try to get a second non-colinear curve for direction
            XYZ normal = null;
            for (int i = 1; i < curves.Count; i++)
            {
                XYZ pA = curves[i].GetEndPoint(0);
                XYZ pB = curves[i].GetEndPoint(1);
                XYZ dir = (pB - pA).Normalize();

                normal = tangent.CrossProduct(dir);
                if (normal.GetLength() > 1e-6)
                    break;
            }

            if (normal == null || normal.IsZeroLength())
            {
                // Fallback: try Rebar's normal if it's a single-plane rebar
                //8normal = rebar.Normal;
            }

            // Ensure normal is valid
            if (normal == null || normal.IsZeroLength())
                return null;

            // Plane origin is at p0, X = tangent, Y = tangent.Cross(normal), Normal = normal
            XYZ xDir = tangent;
            XYZ yDir = normal.CrossProduct(tangent);
            return Plane.CreateByOriginAndBasis(p0, xDir, yDir);
        }


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

        public static double SignedAngle(XYZ v1, XYZ v2, XYZ normal)
        {
            double angle = v1.AngleTo(v2);
            XYZ cross = v1.CrossProduct(v2);
            double sign = Math.Sign(cross.DotProduct(normal));
            return angle * sign; // radians, signed
        }

        public static UV GetCenterOfFace(Face myFace)
        {
            double uMin = double.MaxValue;
            double uMax = double.MinValue;
            double vMin = double.MaxValue;
            double vMax = double.MinValue;

            foreach (EdgeArray edgeLoop in myFace.EdgeLoops)
            {
                foreach (Edge edge in edgeLoop)
                {
                    IList<UV> edgeUVs = edge.TessellateOnFace(myFace);
                    foreach (UV uv in edgeUVs)
                    {
                        uMin = Math.Min(uMin, uv.U);
                        uMax = Math.Max(uMax, uv.U);
                        vMin = Math.Min(vMin, uv.V);
                        vMax = Math.Max(vMax, uv.V);
                    }
                }
            }

            // Compute center as midpoint (not size difference)
            UV center = new UV((uMin + uMax) / 2, (vMin + vMax) / 2);

            return center;
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

        

public static List<Curve> GetRebarCenterline(Rebar rebar)
{
    // Set up options to include centerline curves
    Options geomOptions = new Options
    {
        ComputeReferences = true,
        IncludeNonVisibleObjects = false,
        DetailLevel = ViewDetailLevel.Fine
    };

    // Get geometry
    GeometryElement geomElem = rebar.get_Geometry(geomOptions);

    List<Curve> centerlineCurves = new List<Curve>();

    foreach (GeometryObject obj in geomElem)
    {
        if (obj is GeometryInstance geomInstance)
        {
            GeometryElement instGeomElem = geomInstance.GetInstanceGeometry();

            foreach (GeometryObject instObj in instGeomElem)
            {
                if (instObj is Curve curve)
                {
                    centerlineCurves.Add(curve);
                }
            }
        }
    }

    return centerlineCurves;
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