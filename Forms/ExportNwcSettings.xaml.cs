using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace rvtRebars
{
public class ViewItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public View3D View { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
    /// <summary>
    /// Interaction logic for SettingsForm.xaml
    /// </summary>
    public partial class SettingsForm : Window
    {
        private DispatcherTimer _filterTimer;
        private ICollectionView _viewsCollectionView;
        public ObservableCollection<ViewItem> Views { get; set; }
        public string folderPath {get; private set;}

        public SettingsForm(List<View3D> views3D)
        {
            InitializeComponent();

                _filterTimer = new DispatcherTimer();
    _filterTimer.Interval = TimeSpan.FromMilliseconds(300);
    _filterTimer.Tick += (s, e) =>
    {
        _filterTimer.Stop();
        _viewsCollectionView?.Refresh();
    };

            Views = new ObservableCollection<ViewItem>(
                views3D
                    .Where(v => !v.IsTemplate)
                    .OrderBy(v => v.Name) // optional but recommended
                    .Select(v => new ViewItem 
                    { 
                        Name = v.Name, 
                        View = v,
                        IsSelected = false
                    })
            );

            ViewsList.ItemsSource = Views;

            _viewsCollectionView = CollectionViewSource.GetDefaultView(Views);
            _viewsCollectionView.Filter = FilterViews;
        }

private void CheckAllFiltered_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    foreach (ViewItem item in _viewsCollectionView)
    {
        item.IsSelected = true;
    }
}

private void UncheckAllFiltered_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    foreach (ViewItem item in _viewsCollectionView)
    {
        item.IsSelected = false;
    }
}

private bool FilterViews(object obj)
{
    if (string.IsNullOrWhiteSpace(txtFilter.Text))
        return true;

    if (obj is ViewItem view)
    {
        return view.Name.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    return false;
}

private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
{
    _filterTimer.Stop();
    _filterTimer.Start();
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

