using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace rvtRebars
{
    public class ViewItem
{
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public View3D View { get; set; }
}
    /// <summary>
    /// Interaction logic for SettingsForm.xaml
    /// </summary>
    public partial class SettingsForm : Window
    {
        public List<ViewItem> Views { get; private set; }
        public string folderPath {get; private set;}

        public SettingsForm(List<View3D> views3D)
        {
            InitializeComponent();

            Views = views3D
                .Where(v => !v.IsTemplate)
                .Select(v => new ViewItem { Name = v.Name, View = v })
                .ToList();

            ViewsList.ItemsSource = Views;
            //ExcelPathTextBox.Text = Properties.Settings.Default.ExcelFilePath;
            // MappingPathTextBox.Text = Properties.Settings.Default.Mapping;
            // ExportCsvPathTextBox.Text = Properties.Settings.Default.ExportedData;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder:";
                dialog.ShowNewFolderButton = true;

                // Preselect folder if TextBox has valid path
            if (System.IO.Directory.Exists(txtFolderPath.Text.Trim()))
                dialog.SelectedPath = txtFolderPath.Text.Trim();

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtFolderPath.Text = dialog.SelectedPath;
                }
            }
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            //Properties.Settings.Default.ExcelFilePath = ExcelPathTextBox.Text;
            //Properties.Settings.Default.Mapping = MappingPathTextBox.Text;
            //Properties.Settings.Default.ExportedData = ExportCsvPathTextBox.Text;

            //Properties.Settings.Default.Save(); // ← saves to user config
            folderPath = txtFolderPath.Text.Trim();
            if (!Views.Any(v => v.IsSelected))
            {
                TaskDialog.Show("Warning", "Please select at least one view.");
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

