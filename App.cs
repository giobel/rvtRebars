#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxButtons = System.Windows.Forms.MessageBoxButtons;
using MessageBoxIcon = System.Windows.Forms.MessageBoxIcon;
using System.Windows.Media.Imaging;
#endregion

namespace rvtRebars
{
  public class App : IExternalApplication
  {

    // class instance
    internal static App thisApp = null;

        // ModelessForm instance
        //private ModelessForm m_MyForm;
        private Window1 m_MyForm;

    public Result OnStartup( UIControlledApplication a )
    {



            m_MyForm = null;   // no dialog needed yet; the command will bring it
            thisApp = this;  // static access to this application instance
            
            //MessageBox.Show($"this app registered {thisApp.ToString()}");

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
                
                    
                    if (AddPushButton(cogs, assemblyPath, "btnCogConcreteReo", "Combined\nCOG", null, "rvtRebars.Resources.combination.png",
                        "rvtRebars.ConcreteWithReoCOG", "Refer to documentation") == false)
                {
                    MessageBox.Show("Failed to add combined cog", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


                    if (AddPushButton(cogs, assemblyPath, "btnCogConcrete", "Segment\nCOG", null, "rvtRebars.Resources.concrete.png", 
                    "rvtRebars.ConcreteCOG", "Calculates the centroid of the selected concrete element (must be a family instance, must have sphere family in the model)") == false)
                    {
                        MessageBox.Show("Failed to add rebars cog", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }



                if (AddPushButton(cogs, assemblyPath, "btnCogRebarsbyVol", "Rebars COG\nby Volume", null, "rvtRebars.Resources.rebars.png",
                "rvtRebars.GetRebarsCOGbyVolume", "Refer to documentation") == false)
                {
                    MessageBox.Show("Failed to add rebars cog", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


                    if (AddPushButton(cogs, assemblyPath, "btnCogRebarsbyKg", "Rebars COG\nby kg/m", null, "rvtRebars.Resources.bars.png", 
                    "rvtRebars.GetRebarsCOGbyKgm", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add rebars cog", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    #endregion


                    #region GROUPS

                    RibbonPanel groupsTab = GetSetRibbonPanel(a, tabName, "Groups");
                    
                                        if (AddPushButton(groupsTab, assemblyPath, "btnSliceLayers", "Invert Slices", null, "rvtRebars.Resources.invert.png",
                        "rvtRebars.InvertSliceLayers", "Refer to documentation") == false)
                {
                    MessageBox.Show("Failed to add rebars cog", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                

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
                    
                    if (AddPushButton(utilities,assemblyPath, "btnFaceCenter", "Face\nCentre", null, "rvtRebars.Resources.focus.png", 
                    "rvtRebars.FaceCenter", "Center of Face") == false)
                    {
                        MessageBox.Show("Failed to add button zoom", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                    if (AddPushButton(utilities,assemblyPath, "btnProject", "Project\nto Face", null, "rvtRebars.Resources.sling.png", 
                    "rvtRebars.ProjectToFace", "Center of Face") == false)
                    {
                        MessageBox.Show("Failed to add button center of face", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                if (AddPushButton(utilities, assemblyPath, "btnFaceSection", "Face Section", null, "rvtRebars.Resources.slice_icon.png",
                    "rvtRebars.SectionByFace", "Section aligned to selected face") == false)
                {
                    MessageBox.Show("Failed to add button face section", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                if (AddPushButton(utilities, assemblyPath, "btnAlign3D", "Align 3D View", null, "rvtRebars.Resources.slice_icon.png",
                    "rvtRebars.Align3dView", "Align 3d view to face") == false)
                {
                    MessageBox.Show("Failed to add button align", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                    
                    if (AddPushButton(utilities, assemblyPath, "btnSelectBars", "Select Rebars", null, "rvtRebars.Resources.selectArea.png",
    "rvtRebars.SelectRebars", "Select rebars only (works inside groups too)") == false)
                {
                    MessageBox.Show("Failed to add button zoom", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                    if (AddPushButton(utilities,assemblyPath, "btnZoom", "Zoom\nSelected", null, "rvtRebars.Resources.info.png",
            "rvtRebars.ZoomSelection", "Refer to documentation") == false)
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

                    if (AddPushButton(utilities,assemblyPath, "btnExport", "Export\nSheets", null, "rvtRebars.Resources.exportExcel.png", 
                    "rvtRebars.ExportSheets", "Refer to documentation") == false)
                    {
                        MessageBox.Show("Failed to add button export sheets", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                    
                    if (AddPushButton(utilities,assemblyPath, "btnExportSchedules", "Export\nSchedules", null, "rvtRebars.Resources.exportExcel.png", 
                    "rvtRebars.ExportSchedules", "Export all schedules on segment shop drawing indexes") == false)
                    {
                        MessageBox.Show("Failed to add button export sheets", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                    
                    if (AddPushButton(utilities, assemblyPath, "btnImport", "Update SegIds", null, "rvtRebars.Resources.import.png",
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
            //if (m_MyForm != null && m_MyForm.Visible)
            if (m_MyForm != null)
            {
                m_MyForm.Close();
            }

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

    /// <summary>
    ///   Waking up the dialog from its waiting state.
    /// </summary>
    /// 
    public void WakeFormUp()
    {
        if (m_MyForm != null)
        {
            m_MyForm.WakeUp();
        }
    }

        /// <summary>
        ///   This method creates and shows a modeless dialog, unless it already exists.
        /// </summary>
        /// <remarks>
        ///   The external command invokes this on the end-user's request
        /// </remarks>
        /// 
        public void ShowForm(UIApplication uiapp)
        {
            // If we do not have a dialog yet, create and show it
            //if (m_MyForm == null || m_MyForm.IsDisposed)           
            if (m_MyForm == null || !m_MyForm.IsVisible)
                {
                    // A new handler to handle request posting by the dialog
                    RequestHandler handler = new RequestHandler();

                    // External Event for the dialog to use (to post requests)
                    ExternalEvent exEvent = ExternalEvent.Create(handler);

                    // We give the objects to the new dialog;
                    // The dialog becomes the owner responsible fore disposing them, eventually.
                    //m_MyForm = new ModelessForm(exEvent, handler);
                    m_MyForm = new Window1(exEvent, handler);

                    handler.Window = m_MyForm;
                    m_MyForm.Show();
                }
        }
  }
}
