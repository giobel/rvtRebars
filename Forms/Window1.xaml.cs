using System;
using System.Linq;
using System.Windows;
using Autodesk.Revit.UI;

namespace rvtRebars
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		private RequestHandler m_Handler;
		private ExternalEvent m_ExEvent;
		// public List<string> UniqueIds { get; set; }
		// private IList<Element> VisibleRebars { get; set; }
		// public ObservableCollection<string> SlicesInSegment { get; set; }
		// public List<ElementId> SelectedBars { get; set; }
		// Document _doc { get; set; }
		// UIDocument _uidoc { get; set; }


		public Window1(ExternalEvent exEvent, RequestHandler handler)
		{
			InitializeComponent();
			m_Handler = handler;
			m_ExEvent = exEvent;

			MakeRequest(RequestId.LoadRebars);

			// this.DataContext = this;
			// VisibleRebars = bars;
			// SelectedBars = new List<ElementId>();
			// SlicesInSegment = new ObservableCollection<string>();
			// _uidoc = uidoc;
			// _doc = doc;
		}


		/// <summary>
		///   A private helper method to make a request
		///   and put the dialog to sleep at the same time.
		/// </summary>
		/// <remarks>
		///   It is expected that the process which executes the request 
		///   (the Idling helper in this particular case) will also
		///   wake the dialog up after finishing the execution.
		/// </remarks>
		///
		private void MakeRequest(RequestId request)
		{
			m_Handler.Request.Make(request);
			m_ExEvent.Raise();
			DozeOff();
		}


		/// <summary>
		///   DozeOff -> disable all controls (but the Exit button)
		/// </summary>
		/// 
		private void DozeOff()
		{
			EnableCommands(false);
		}


		/// <summary>
		///   WakeUp -> enable all controls
		/// </summary>
		/// 
		public void WakeUp()
		{
			EnableCommands(true);
		}


		protected override void OnClosed(EventArgs e)
		{
			// we own both the event and the handler
			// we should dispose it before we are closed
			m_ExEvent.Dispose();
			m_ExEvent = null;
			m_Handler = null;

			// Your logic here
			base.OnClosed(e);
		}


		/// <summary>
		///   Making a door Left
		/// </summary>
		/// 
		private void BtnSelectClick(object sender, EventArgs e)
		{
			MakeRequest(RequestId.Select);
		}

		private void BtnInvertLayers(object sender, EventArgs e)
		{
			MakeRequest(RequestId.InvertLayers);
		}

		private void BtnColorBySlice(object sender, EventArgs e)
		{
			MakeRequest(RequestId.ColorBySlice);
		}


		private void BtnZoomTo(object sender, EventArgs e)
		{
		MakeRequest(RequestId.ZoomTo);
	}


		/// <summary>
		///   Control enabler / disabler 
		/// </summary>
		///
		private void EnableCommands(bool status)
		{
			foreach (UIElement element in LogicalTreeHelper.GetChildren(this).OfType<UIElement>())
			{
				element.IsEnabled = status;
			}
		}


		private void comboUniqueIds_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			MakeRequest(RequestId.UpdateSlices);
		}


		private void cboxSlices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			//TaskDialog.Show("R", "Not implemented yet");
		}


		/*
				private void cboxUniqueIds_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
				{
					try
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

						OutputText.Text = "Select a Slice";

					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}


				}

				private void cboxSlices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
				{
					try
					{
						if (cboxSlices.SelectedItem == null || cboxUniqueIds.SelectedItem == null)
							return;

						SelectedBars.Clear();

						SelectedBars = VisibleRebars
							.Where(x => x.LookupParameter("LOR_UniqueID (SRC_FBA)")?.AsValueString() == cboxUniqueIds.SelectedItem?.ToString() &&
							x.LookupParameter("FBA_Slice")?.AsValueString() == cboxSlices.SelectedItem?.ToString())
							.Select(x => x.Id)
							.ToList();


						OutputText.Text = $"{SelectedBars.Count().ToString()} rebars selected";
					}

					catch (Exception ex)
					{
						MessageBox.Show(ex.Message + "   " + cboxSlices.ToString());
					}
				}


				private void BtnSelectClick(object sender, RoutedEventArgs e)
				{
					_handler.Message = "Do something!";
					_externalEvent.Raise();

					if (SelectedBars != null)
					{
						_uidoc.Selection.SetElementIds(SelectedBars);
						_uidoc.ShowElements(SelectedBars);
					}
				}
		*/

	}
}