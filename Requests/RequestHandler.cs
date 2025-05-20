//
// (C) Copyright 2003-2019 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Security.Cryptography;
using System.Text;


namespace rvtRebars
{
    /// <summary>
    ///   A class with methods to execute requests made by the dialog user.
    /// </summary>
    /// 
    public class RequestHandler : IExternalEventHandler
    {
        public Window1 Window { get; set; }

        // A trivial delegate, but handy
        private delegate void DoorOperation(FamilyInstance e);

        // The value of the latest request made by the modeless form 
        private Request m_request = new Request();

        private IList<Element> VisibleRebars;

        /// <summary>
        /// A public property to access the current request value
        /// </summary>
        public Request Request
        {
            get { return m_request; }
        }

        /// <summary>
        ///   A method to identify this External Event Handler
        /// </summary>
        public String GetName()
        {
            return "R2014 External Event Sample";
        }


        /// <summary>
        ///   The top method of the event handler.
        /// </summary>
        /// <remarks>
        ///   This is called by Revit after the corresponding
        ///   external event was raised (by the modeless form)
        ///   and Revit reached the time at which it could call
        ///   the event's handler (i.e. this object)
        /// </remarks>
        /// 
        public void Execute(UIApplication uiapp)
        {
            try
            {
                switch (Request.Take())
                {
                    case RequestId.None:
                        {
                            return;  // no request at this time -> we can leave immediately
                        }
                    case RequestId.LoadRebars:
                        {
                            //Load all rebars in the model
                            LoadRebars(uiapp);
                            break;
                        }
                    case RequestId.UpdateSlices:
                        {
                            UpdateSlicesList(uiapp);
                            break;
                        }
                    case RequestId.Select:
                        {
                            SelectSliceBars(uiapp);
                            break;
                        }
                    case RequestId.InvertLayers:
                        {
                            InvertLayers(uiapp);
                            break;
                        }
                    case RequestId.ColorBySlice:
                        {
                            ColorBySlice(uiapp);
                            break;
                        }
                    case RequestId.ZoomTo:
                    {
                                ZoomTo(uiapp);
                        break;
                }
                    default:
                        {
                            // some kind of a warning here should
                            // notify us about an unexpected request 
                            break;
                        }
                }
            }
            finally
            {
                App.thisApp.WakeFormUp();
            }

            return;
        }

        private void ZoomTo(UIApplication uiapp)
        {
            string selectedSegment = Window.cboxUniqueIds.SelectedItem.ToString();
            string selectedSlice = Window.cboxSlices.SelectedItem.ToString();

            var SelectedBars = VisibleRebars
                .Where(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString() == selectedSegment &&
                x.LookupParameter("FBA_Slice")?.AsValueString() == selectedSlice)
                .Select(x => x.Id)
                .ToList();

            uiapp.ActiveUIDocument.ShowElements(SelectedBars);
        }

        private void ColorBySlice(UIApplication uiapp)
        {
            Document doc = uiapp.ActiveUIDocument.Document;

            FilteredElementCollector elementsInView = new FilteredElementCollector(doc);
            FillPatternElement solidFillPattern = elementsInView.OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);


            List<BuiltInCategory> builtInCats = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Rebar
            };

            ElementMulticategoryFilter filter1 = new ElementMulticategoryFilter(builtInCats);


            IList<Element> visibleElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(filter1).WhereElementIsNotElementType().ToElements();

            //string colorParam = "Type";
            //string colorParam = "Shape";
            string colorParam = "FBA_Slice";

            var grouped = visibleElements.GroupBy(x => x.LookupParameter(colorParam).AsValueString());

            //Random pRand = new Random();
            var md5 = MD5.Create();
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceForegroundPatternId(solidFillPattern.Id);


            string error = "";
            using (Transaction t = new Transaction(doc, "Override Colors"))
            {
                t.Start();
                foreach (var element in grouped)
                {


                    var firstElement = element.First();

                    string colorName = "xx";


                    colorName = firstElement.LookupParameter(colorParam).AsValueString();



                    if (colorName == null)
                    {
                        colorName = "null";
                    }
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(colorName));


                    Autodesk.Revit.DB.Color pcolor = new Autodesk.Revit.DB.Color(hash[0], hash[1], hash[2]);

                    //TaskDialog.Show("P", String.Format("{0}, {1},{2},{3}", colorName, hash[0], hash[1], hash[2]));

                    ogs.SetSurfaceForegroundPatternColor(pcolor);

                    ogs.SetProjectionLineColor(pcolor);


                    try
                    {
                        //foreach (FamilyInstance item in element)
                        foreach (var item in element)
                        {
                            doc.ActiveView.SetElementOverrides(item.Id, ogs);

                        }

                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }
                }

                t.Commit();
            }
        }

        private void InvertLayers(UIApplication uiapp)
        {
            Document doc = uiapp.ActiveUIDocument.Document;

            ICollection<ElementId> selectedBars = uiapp.ActiveUIDocument.Selection.GetElementIds();

            if (selectedBars.Count < 1)
            {
                TaskDialog.Show("Warning", "Please select some rebars");
            }

            var maxLayer = selectedBars
                    .Select(id => doc.GetElement(id))
                    .Where(e => e != null)
                    .Select(e => e.LookupParameter("FBA_SliceLayer").AsValueString())
                    .Where(val => !string.IsNullOrEmpty(val))
                    .Select(val => val.Replace("L", "")) // remove 'L'
                    .Select(val =>
                    {
                        int num;
                        return int.TryParse(val, out num) ? (int?)num : null;
                    })
                    .Where(num => num.HasValue)
                    .Max(num => num.Value);

            using (Transaction t = new Transaction(doc, "Invert slice layers"))
            {
                t.Start();

                foreach (ElementId eid in selectedBars)
                {

                    Element e = uiapp.ActiveUIDocument.Document.GetElement(eid);

                    int currentSlice = int.Parse(e.LookupParameter("FBA_SliceLayer").AsValueString().Replace("L", ""));

                    string newSlice = "L" + (maxLayer + 1 - currentSlice);

                    e.LookupParameter("FBA_SliceLayer").Set(newSlice);
                }


                t.Commit();

            }

            TaskDialog.Show("Invert Slice Layers", "Done");
        }

        private void SelectSliceBars(UIApplication uiapp)
        {
            string selectedSegment = Window.cboxUniqueIds.SelectedItem.ToString();
            string selectedSlice = Window.cboxSlices.SelectedItem.ToString();

            var SelectedBars = VisibleRebars
                            .Where(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString() == selectedSegment &&
                            x.LookupParameter("FBA_Slice")?.AsValueString() == selectedSlice)
                            .Select(x => x.Id)
                            .ToList();

            if (selectedSegment == null || selectedSlice == null)
            {
                TaskDialog.Show("Warning", "Please pick UniqueId and Slice first");
            }
            else
            {
                uiapp.ActiveUIDocument.Selection.SetElementIds(SelectedBars);
            }
            
        }

        private void LoadRebars(UIApplication uiapp)
        {
            Document doc = uiapp.ActiveUIDocument.Document;

            VisibleRebars = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();

            var uniqueIds = VisibleRebars
                    .Select(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

            //uniqueIds.Sort();


            Window?.Dispatcher.Invoke(() =>
            {
                Window.cboxUniqueIds.ItemsSource = uniqueIds.OrderBy(n => n).ToList();
            });
        }


        private void UpdateSlicesList(UIApplication uiapp)
        {
            string selectedSegment = Window.cboxUniqueIds.SelectedItem.ToString();
            string selectedSlice = Window.cboxSlices.SelectedIndex.ToString();

            var currentSegmentSlices = VisibleRebars
                .Where(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString() == selectedSegment)
                .Select(x => x.LookupParameter("FBA_Slice")?.AsValueString())
                .Where(slice => !string.IsNullOrEmpty(slice))
                .Distinct()
                .ToList();

            Window?.Dispatcher.Invoke(() =>
            {
                Window.cboxSlices.ItemsSource = currentSegmentSlices.OrderBy(n => n).ToList();
            });
        }



    }  // class

}  // namespace
