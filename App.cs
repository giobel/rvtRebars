#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
#endregion

namespace rvtRebars
{
  class App : IExternalApplication
  {
    public Result OnStartup( UIControlledApplication a )
    {
                    string tabName = "LOR Rebars";

            try
            {
                    a.CreateRibbonTab(tabName);

                    #region COG

                     RibbonPanel cogs = GetSetRibbonPanel(a, tabName, "COG");

                    string assemblyPath = Assembly.GetExecutingAssembly().Location;
                    
                    if (AddPushButton(cogs, assemblyPath, "btnReoWeight", "Reo Weight", null, "rvtRebars.Resources.scale.png", 
                    "rvtRebars.ReoWeight", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add rebars cog", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }


                    if (AddPushButton(cogs, assemblyPath, "btnCogRebars", "Rebars COG", null, "rvtRebars.Resources.cogRebars.png", 
                    "rvtRebars.GetRebarsCOG", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add rebars cog", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }



                    #endregion


                    #region GROUPS

                    RibbonPanel groupsTab = GetSetRibbonPanel(a, tabName, "Groups");

                    if (AddPushButton(groupsTab, assemblyPath, "btnExportGroups", "Export Groups", null, "rvtRebars.Resources.exportExcel.png", 
                    "rvtRebars.ExportGroupsSubElements", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add Export Groups", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (AddPushButton(groupsTab, assemblyPath, "btnRegroupBars", "Regroup Bars", null, "rvtRebars.Resources.regroup.png", 
                    "rvtRebars.RegroupBars", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add Regroup Bars", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (AddPushButton(groupsTab, assemblyPath, "btnCombineGroups", "Combine Groups", null, "rvtRebars.Resources.clean.png", 
                    "rvtRebars.CombineGroups", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add Combine Groups", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    #endregion


                    #region UTILITIES

                    RibbonPanel utilities = GetSetRibbonPanel(a, tabName, "Utilities");
                    
                    if (AddPushButton(utilities,assemblyPath, "btnSelectBars", "Select Rebars", null, "rvtRebars.Resources.selectArea.png", 
                    "rvtRebars.SelectRebars", "Select rebars only (works inside groups too)") == false)
                    {
                        MessageBox.Show("Failed to add button zoom", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (AddPushButton(utilities,assemblyPath, "btnZoom", "Zoom\nSelected", null, "rvtRebars.Resources.info.png",
            "rvtRebars.Command", "Refer to documentation") == false)
                {
                    MessageBox.Show("Failed to add button zoom", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                   if (AddPushButton(utilities,assemblyPath, "btnSelect", "Select From\nClipboard", null, "rvtRebars.Resources.select.png", 
                    "rvtRebars.SelectFromRevizto", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add button select", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                   if (AddPushButton(utilities,assemblyPath, "btnClash", "Clash Check", null, "rvtRebars.Resources.clash.png", 
                    "rvtRebars.ClashDetection", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add button select", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (AddPushButton(utilities,assemblyPath, "btnExport", "Export Sheets", null, "rvtRebars.Resources.exportExcel.png", 
                    "rvtRebars.ExportSheets", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add button export sheets", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                    if (AddPushButton(utilities,assemblyPath, "btnImport", "Update SegIds", null, "rvtRebars.Resources.import.png", 
                    "rvtRebars.UpdateFromExcel", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add button export sheets", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                #endregion

                    
            //    var text = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();

            //    foreach (var itemu in text)
            //    {
            //     MessageBox.Show(itemu);
            //    }



            }
            catch { }

      return Result.Succeeded;
    }

    public Result OnShutdown( UIControlledApplication a )
    {
      return Result.Succeeded;
    }
    
private BitmapSource GetEmbeddedImage( string name )
{
  try
  {
    Assembly a = Assembly.GetExecutingAssembly();
    Stream s = a.GetManifestResourceStream( name );
    return BitmapFrame.Create( s );
  }
  catch
  {
    return null;
  }
}



        private RibbonPanel GetSetRibbonPanel(UIControlledApplication application, string tabName, string panelName)
        {
            List<RibbonPanel> tabList = new List<RibbonPanel>();

            tabList = application.GetRibbonPanels(tabName);

            RibbonPanel tab = null;

            foreach (RibbonPanel r in tabList)
            {
                if (r.Name.ToUpper() == panelName.ToUpper())
                {
                    tab = r;
                }
            }

            if (tab is null)
                tab = application.CreateRibbonPanel(tabName, panelName);

            return tab;
        }

        private Boolean AddPushButton(RibbonPanel Panel, string assemblyPath, string ButtonName, string ButtonText, string ImagePath16, string ImagePath32, string dllClass, string Tooltip)
        {

            try
            {
                PushButtonData m_pbData = new PushButtonData(ButtonName, ButtonText, assemblyPath, dllClass);

                BitmapSource bitmap16 = GetEmbeddedImage( ImagePath16 );

                BitmapSource bitmap32 = GetEmbeddedImage( ImagePath32 );

                m_pbData.Image = bitmap16;
                m_pbData.LargeImage = bitmap32;

                m_pbData.ToolTip = Tooltip;


                PushButton m_pb = Panel.AddItem(m_pbData) as PushButton;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public BitmapImage Convert(System.Drawing.Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }



  }
}
