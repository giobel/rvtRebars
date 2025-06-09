using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class SCheduleAddSeparator : IExternalCommand
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


            double scale = 304.8;
            ICollection<ElementId> selectedElementsId = uidoc.Selection.GetElementIds();



            using (Transaction t = new Transaction(doc, "Add separator"))
            {

                t.Start();


                foreach (ElementId eid in selectedElementsId)
                {


                    Element e = doc.GetElement(eid);
                    ViewSchedule v = e as ViewSchedule;

                    //TaskDialog.Show("R", v.Name);

                    ScheduleDefinition scheduleDef = v.Definition;

                    ScheduleField sf = scheduleDef.GetField(0);

                    IList<TableCellCombinedParameterData> combinedParams = sf.GetCombinedParameters();


                    combinedParams.First().Separator = "-";

                    scheduleDef.RemoveField(sf.FieldId);
                    scheduleDef.ShowGrandTotal = true;
                    scheduleDef.ShowGrandTotalCount = true;
                    scheduleDef.ShowGrandTotalTitle = true;
                    scheduleDef.GrandTotalTitle = "Count";



                    scheduleDef.InsertCombinedParameterField(combinedParams, "DRAWING NUMBER", 0);
                    scheduleDef.GetField(0).GridColumnWidth = 70 / scale;
                }




                t.Commit();


            }
            return Result.Succeeded;
        }
    }
}