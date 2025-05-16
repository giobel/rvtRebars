
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


namespace rvtRebars
{
  [Transaction(TransactionMode.Manual)]
  public class InvertSliceLayers : IExternalCommand
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


      MyExternalEventHandler handler = new MyExternalEventHandler(uidoc);
      ExternalEvent exEvent = ExternalEvent.Create(handler);

      //these are bars in view, what happens when the view changes? need to update this
      IList<Element> rebars = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();

      var uniqueIds = rebars
      .Select(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString())
      .Where(id => !string.IsNullOrEmpty(id))
      .Distinct()
      .ToList();

      uniqueIds.Sort();
      // rebars.GroupBy(x=> x.LookupParameter("LOR_UniqueID (SRC_FBA)").AsValueString());

      // var form = new Window1(uidoc, doc, rebars, handler, exEvent);

      // form.UniqueIds = new List<string>(uniqueIds);

      // form.Show();

      App.thisApp.ShowForm(commandData.Application);

      return Result.Succeeded;
    }
  }
    

    public class MyExternalEventHandler : IExternalEventHandler
{
    private UIDocument _uidoc;
    public string Message { get; set; }

    public MyExternalEventHandler(UIDocument uidoc)
    {
        _uidoc = uidoc;
    }

    public void Execute(UIApplication app)
    {
        try
        {
            TaskDialog.Show("Revit API Call", $"Hello from ExternalEvent! Current view: {_uidoc.ActiveView.Name}");
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Error", ex.Message);
        }
    }

    public string GetName() => "My External Event Handler";
}
}