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
	public class GetRebarsCOGbyKgm : IExternalCommand
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


			ICollection<Element> rebarsInView = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();

			double totalMass = 0;
			double xWeightedSum = 0;
			double yWeightedSum = 0;
			double zWeightedSum = 0;

			FamilySymbol fs = Helpers.GetFamilySymbolByName(doc, "SphereFamily", "SphereFamily");


			foreach (var element in rebarsInView)
			{

				double diameter = UnitUtils.ConvertFromInternalUnits(element.LookupParameter("Bar Diameter").AsDouble(), UnitTypeId.Millimeters);

				string diameterName = "N" + Math.Round(diameter, 0).ToString();

				//double length = UnitUtils.ConvertFromInternalUnits(element.LookupParameter("Total Bar Length").AsDouble(), UnitTypeId.Meters);

				//double rebarWeight = bbarWeight[diameterName] * length;

				//List<Solid> sourceSolids = Helpers.GetRebarSolid(doc, element);

				//BoundingBoxXYZ bbox = element.get_BoundingBox(doc.ActiveView);

				//XYZ pt1 = bbox.Min;				           	
				//XYZ pt3 = bbox.Max;

				//bbox center --> incorrect results
				//XYZ bboxCenter = (pt3 + pt1)/2;

				//if (sourceSolids.Count == 1){
 				Rebar bar = element as Rebar;
								IList<Curve> rebarCls = bar.GetCenterlineCurves(
											    true, // adjustForSelfIntersection
											    false, // suppress hooks
											    false, // suppress bends
											    MultiplanarOption.IncludeAllMultiplanarCurves, // or .CurrentPlaneOnly
											    0); // if multiplar, index of the plane


				foreach (Curve curve in rebarCls)
				{
					XYZ midPoint = curve.Evaluate(0.5, true);

						double tonnes = curve.Length * bbarWeight[diameterName];
						totalMass += tonnes;

						xWeightedSum += tonnes * midPoint.X;
						yWeightedSum += tonnes * midPoint.Y;
						zWeightedSum += tonnes * midPoint.Z;

				}


				// foreach (var solidBar in sourceSolids)
				// {


				// 	try
				// 	{

				// 		XYZ bboxCenter = solidBar.ComputeCentroid();



				// 		// doc.Create.NewFamilyInstance(
				// 		//         bboxCenter, // Location point
				// 		//         fs, // FamilySymbol

				// 		//         StructuralType.NonStructural // Specify if it's structural or non-structural
				// 		//     );

				// 		//double volume = element.LookupParameter("Reinforcement Volume").AsDouble();

				// 		//double volume = solidBar.Volume;


				// 		//double convertedVolume = UnitUtils.ConvertFromInternalUnits(volume, UnitTypeId.CubicMeters);
				// 		//double density = 7.850;
				// 		//double tonnes = convertedVolume*density;


				// 		//weight of current bar!!!
				// 		double tonnes = rebarWeight;

				// 		totalMass += tonnes;

				// 		xWeightedSum += tonnes * bboxCenter.X;
				// 		yWeightedSum += tonnes * bboxCenter.Y;
				// 		zWeightedSum += tonnes * bboxCenter.Z;


				// 	}

				// 	catch
				// 	{

				// 		//TaskDialog.Show("R", element.Id.ToString());

				// 		throw new Exception("Error");
				// 	}

				// }
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



			XYZ cogPt = new XYZ(xCog, yCog, zCog);

			using (Transaction t = new Transaction(doc, "Find COG"))
			{

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



	}
}