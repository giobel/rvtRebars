using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;


namespace rvtRebars.ViewModel
{
    public class PanelEvent
    {
        ObservableCollection<string> _fields;
        public ObservableCollection<string> Fields { get; set; }

        public PanelEvent()
        {

            Fields = new ObservableCollection<string>();
            Fields.Add("Field1");
            Fields.Add("Field2");
            Fields.Add("Field3");
            Fields.Add("Field4");
            Fields.Add("Field5");
            Fields.Add("Field6");
            Fields.Add("Field7");
            Fields.Add("Field8");
            Fields.Add("Field9");
            Fields.Add("Field10");

        }
    }
}
