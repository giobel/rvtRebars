using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace rvtRebars
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public List<string> UniqueIds { get; set; }
		private IList<Element> VisibleRebars { get; set; }
		public ObservableCollection<string> SlicesInSegment { get; set; }
		public List<Element> SelectedBars { get; set; }
		Document _doc { get; set; }
		UIDocument _uidoc { get; set; }

		public Window1(UIDocument uidoc, Document doc, IList<Element> bars)
		{
			InitializeComponent();
			this.DataContext = this;
			VisibleRebars = bars;
			SlicesInSegment = new ObservableCollection<string>();
			_uidoc = uidoc;
			_doc = doc;
		}


		private void cboxUniqueIds_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			SlicesInSegment.Clear();
			//MessageBox.Show(cboxUniqueIds.SelectedItem.ToString());

			var currentSegment = VisibleRebars
				.Where(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString() == cboxUniqueIds.SelectedItem.ToString())
				.Select(x => x.LookupParameter("FBA_Slice")?.AsValueString())
				.Where(slice => !string.IsNullOrEmpty(slice))
				.Distinct()
				.ToList();

			currentSegment.Sort();

			foreach (var item in currentSegment)
			{
				SlicesInSegment.Add(item);
			}



		}

		private void cboxSlices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var currentSlices = VisibleRebars
				.Where(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString() == cboxUniqueIds.SelectedItem.ToString() &&
				x.LookupParameter("FBA_Slice").AsValueString() == cboxSlices.SelectedItem.ToString())
				.Select(x => x.Id)
				.ToList();
			
			_uidoc.Selection.SetElementIds(currentSlices);

        }
	}
}