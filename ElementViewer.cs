// // (C) Copyright 2003-2019 by Autodesk, Inc.
// // Converted from VB.NET to C# by OpenAI ChatGPT

// using System;
// using System.Collections.Generic;
// using Autodesk.Revit.UI;
// using Autodesk.Revit.DB;
// using Autodesk.Revit.ApplicationServices;

// [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
// [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
// [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
// public class ElementViewer : IExternalCommand
// {
//     private Options mOptions;
//     private List<Transform> mTransformations;
//     private Application mApplication;
//     private RevitViewer.VB.NET.Wireframe mViewer;

//     public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
//     {
//         mApplication = commandData.Application.Application;
//         mOptions = mApplication.Create.NewGeometryOptions();
//         mOptions.DetailLevel = ViewDetailLevel.Fine;
//         mViewer = new RevitViewer.VB.NET.Wireframe();

//         ElementSet selSet = new ElementSet();
//         foreach (ElementId elementId in commandData.Application.ActiveUIDocument.Selection.GetElementIds())
//         {
//             selSet.Insert(commandData.Application.ActiveUIDocument.Document.GetElement(elementId));
//         }

//         foreach (Element elem in selSet)
//         {
//             DrawElement(elem);
//         }

//         if (selSet.Size > 0)
//         {
//             mViewer.ShowModal();
//         }
//         else
//         {
//             TaskDialog.Show("Revit", "Please select some elements first.");
//         }

//         mApplication = null;
//         mViewer = null;

//         return Result.Succeeded;
//     }

//     private void PushTransformation(Transform transform)
//     {
//         if (mTransformations == null)
//             mTransformations = new List<Transform>();

//         mTransformations.Add(transform);
//     }

//     private void PopTransformation()
//     {
//         if (mTransformations == null || mTransformations.Count <= 1)
//         {
//             mTransformations = null;
//             return;
//         }

//         mTransformations = mTransformations.GetRange(0, mTransformations.Count - 1);
//     }

//     private void DrawElement(Element elem)
//     {
//         if (elem is Group group)
//         {
//             foreach (ElementId id in group.GetMemberIds())
//             {
//                 DrawElement(group.Document.GetElement(id));
//             }
//         }
//         else
//         {
//             GeometryElement geom = elem.get_Geometry(mOptions);
//             if (geom != null)
//             {
//                 DrawElement(geom);
//             }
//         }
//     }

//     private void DrawElement(GeometryElement elementGeom)
//     {
//         if (elementGeom == null) return;

//         foreach (GeometryObject geomObject in elementGeom)
//         {
//             switch (geomObject)
//             {
//                 case Curve curve:
//                     DrawCurve(curve);
//                     break;
//                 case GeometryInstance geomInstance:
//                     DrawInstance(geomInstance);
//                     break;
//                 case Mesh mesh:
//                     DrawMesh(mesh);
//                     break;
//                 case Solid solid:
//                     DrawSolid(solid);
//                     break;
//                 case PolyLine poly:
//                     DrawPoly(poly);
//                     break;
//             }
//         }
//     }

//     private void ViewerDrawLine(XYZ startPoint, XYZ endPoint)
//     {
//         if (mViewer == null) return;

//         XYZ transformedStart = startPoint;
//         XYZ transformedEnd = endPoint;

//         if (mTransformations != null)
//         {
//             for (int i = mTransformations.Count - 1; i >= 0; i--)
//             {
//                 transformedStart = TransformPoint(mTransformations[i], transformedStart);
//                 transformedEnd = TransformPoint(mTransformations[i], transformedEnd);
//             }
//         }

//         mViewer.Add(transformedStart.X, transformedStart.Y, transformedStart.Z,
//                     transformedEnd.X, transformedEnd.Y, transformedEnd.Z);
//     }

//     public XYZ TransformPoint(Transform transform, XYZ point)
//     {
//         return transform.OfPoint(point);
//     }

//     private void DrawCurve(Curve geomCurve)
//     {
//         DrawPoints(geomCurve.Tessellate());
//     }

//     private void DrawPoly(PolyLine polyLine)
//     {
//         DrawPoints(polyLine.GetCoordinates());
//     }

//     private void DrawPoints(IList<XYZ> points)
//     {
//         if (points.Count == 0) return;

//         XYZ previousPoint = points[0];
//         for (int i = 1; i < points.Count; i++)
//         {
//             XYZ point = points[i];
//             ViewerDrawLine(previousPoint, point);
//             previousPoint = point;
//         }
//     }

//     private void DrawInstance(GeometryInstance geomInstance)
//     {
//         PushTransformation(geomInstance.Transform);

//         GeometryElement geomSymbol = geomInstance.SymbolGeometry;
//         if (geomSymbol != null)
//         {
//             DrawElement(geomSymbol);
//         }

//         PopTransformation();
//     }

//     private void DrawMesh(Mesh geomMesh)
//     {
//         for (int i = 0; i < geomMesh.NumTriangles; i++)
//         {
//             MeshTriangle triangle = geomMesh.get_Triangle(i);
//             ViewerDrawLine(triangle.get_Vertex(0), triangle.get_Vertex(1));
//             ViewerDrawLine(triangle.get_Vertex(1), triangle.get_Vertex(2));
//             ViewerDrawLine(triangle.get_Vertex(2), triangle.get_Vertex(0));
//         }
//     }

//     private void DrawSolid(Solid geomSolid)
//     {
//         foreach (Face face in geomSolid.Faces)
//         {
//             DrawFace(face);
//         }

//         foreach (Edge edge in geomSolid.Edges)
//         {
//             DrawEdge(edge);
//         }
//     }

//     private void DrawEdge(Edge geomEdge)
//     {
//         DrawPoints(geomEdge.Tessellate());
//     }

//     private void DrawFace(Face geomFace)
//     {
//         DrawMesh(geomFace.Triangulate());
//     }
// }
