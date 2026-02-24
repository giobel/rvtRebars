using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace rvtRebars
{
    [Transaction(TransactionMode.Manual)]
    public class ExportNwc : IExternalCommand
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

            // Collect all 3D views
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var views3D = collector.OfClass(typeof(View3D))
                                .Cast<View3D>()
                                .Where(v => !v.IsTemplate)  // Ignore view templates
                                .ToList();

            if (!views3D.Any())
            {
                TaskDialog.Show("Warning", "No 3D views found.");
                return Result.Cancelled;
            }

            SettingsForm window = new SettingsForm(views3D);
            
            window.ShowDialog();

            if (window.DialogResult == false)
            {
                return Result.Cancelled;
            }

            var selectedViews = window.Views.Where(v => v.IsSelected).Select(v => v.View).ToList();
            if (!selectedViews.Any())
            {
                TaskDialog.Show("Warning", "No views selected.");
                return Result.Cancelled;
            }

            string outputFolder = window.folderPath;

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);



            // string errors = "";
            int results = 0;

             foreach (var view in selectedViews)
            {
            // Build output file path
            string fileName = Path.Combine(outputFolder, $"{view.Name}.nwd");

            // Set up Navisworks export options
            NavisworksExportOptions options = new NavisworksExportOptions
            {
                ExportScope = NavisworksExportScope.View,
                ExportLinks = false,
                ExportRoomGeometry = false,
                ViewId = view.Id
            };

            try
            {
                doc.Export(outputFolder, $"{view.Name}",options);
                results ++;
                //TaskDialog.Show("Export", $"Exported view: {view.Name}");
            }
            catch (Exception ex)
            {
                // errors += ex.Message + Environment.NewLine;
                results --;
                //TaskDialog.Show("Error", $"Failed to export view {view.Name}:\n{ex.Message}");
            }
            }

            ShowExportFinishedDialog(outputFolder, results);

            return Result.Succeeded;
        }


private void ShowExportFinishedDialog(string outputFolder, int results)
{
    var td = new TaskDialog("Export Complete");

    td.MainInstruction = $"{results} view(s) exported.";
    td.MainContent = $"Output folder: {outputFolder}";

    // Add command link button
    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Open Output Folder");
    
    // Add a Close button
    td.CommonButtons = TaskDialogCommonButtons.Close;

    var result = td.Show();

    if (result == TaskDialogResult.CommandLink1)
    {
        // Open the folder
        if (System.IO.Directory.Exists(outputFolder))
            Process.Start(new ProcessStartInfo()
            {
                FileName = outputFolder,
                UseShellExecute = true,
                Verb = "open"
            });
    }
}
    }
}


