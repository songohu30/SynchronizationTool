using Autodesk.Revit.DB;
using Functions;
using OpeningsModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI
{
    /// <summary>
    /// Interaction logic for SynchronizationWindow.xaml
    /// </summary>
    public partial class SynchronizationWindow : Window
    {
        public SynchronizationTool SynchronizationTool { get; set; }

        public SynchronizationWindow(SynchronizationTool synchronizationTool)
        {
            SynchronizationTool = synchronizationTool;
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            OpeningSynchronization.WindowHandleSearch search = OpeningSynchronization.WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void BtnCheckCloud_Click(object sender, RoutedEventArgs e)
        {
            SynchronizationTool.ToolAction = ToolAction.CheckWithCloud;
            SynchronizationTool.TheEvent.Raise();
            SynchronizationTool.SignalEvent.WaitOne();
            SynchronizationTool.SignalEvent.Reset();
            DataGridOpenings.ItemsSource = SynchronizationTool.OpeningViewModels;
            DataGridHosts.ItemsSource = SynchronizationTool.HostViewModels;
            SynchronizationTool.SetGenericModelTypeList();
            RoundTypeComBox.ItemsSource = SynchronizationTool.GenericFamilySymbols;
            RectTypeComBox.ItemsSource = SynchronizationTool.GenericFamilySymbols;
            for (int i = 0; i < SynchronizationTool.GenericFamilySymbols.Count - 1; i++)
            {
                FamilySymbol fs = SynchronizationTool.GenericFamilySymbols[i];
                if (fs.Name.ToUpper().Contains("XXX RECTANGULAR")) RectTypeComBox.SelectedIndex = i;
                if (fs.Name.ToUpper().Contains("XXX ROUND")) RoundTypeComBox.SelectedIndex = i;
            }
        }

        private void BtnUpdateProject_Click(object sender, RoutedEventArgs e)
        {
            FamilySymbol rectSymbol = RectTypeComBox.SelectedItem as FamilySymbol; 
            FamilySymbol roundSymbol = RoundTypeComBox.SelectedItem as FamilySymbol;

            if(rectSymbol == null || roundSymbol == null)
            {
                MessageBox.Show("You have to select opening types!");
                return;
            }

            SynchronizationTool.RectangularSymbol = rectSymbol;
            SynchronizationTool.RoundSymbol = roundSymbol;

            SynchronizationTool.ToolAction = ToolAction.UpdateProject;
            SynchronizationTool.TheEvent.Raise();
        }

        private void BtnCreateCloud_Click(object sender, RoutedEventArgs e)
        {
            SynchronizationTool.ToolAction = ToolAction.CreateCloudResource;
            SynchronizationTool.TheEvent.Raise();
        }

        private void BtnUpdateCloud_Click(object sender, RoutedEventArgs e)
        {
            SynchronizationTool.ToolAction = ToolAction.CreateCloudResource;
            SynchronizationTool.TheEvent.Raise();
        }

        private void TxtBoxToken_GotFocus(object sender, RoutedEventArgs e)
        {
            if(TxtBoxToken.Text == "Synchronization token")
            {
                TxtBoxToken.Clear();
                TxtBoxToken.Foreground = Brushes.Black;
            }
        }

        private void MainGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TxtBoxToken.IsFocused)
            {
                MainGrid.Focus();
                if (TxtBoxToken.Text == "")
                {
                    TxtBoxToken.Text = "Synchronization token";
                    TxtBoxToken.Foreground = Brushes.Gray;
                }
                else
                {
                    TxtBoxToken.Foreground = Brushes.Black;
                }
            }
        }

        private void DataGridOpenings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpeningViewModel model = DataGridOpenings.SelectedItem as OpeningViewModel;
            if(model != null)
            {
                SynchronizationTool.SelectedId = model.OpeningModel.ElementId;
                SynchronizationTool.ToolAction = ToolAction.ShowItem;
                SynchronizationTool.TheEvent.Raise();
            }
        }

        private void DataGridHosts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HostViewModel model = DataGridHosts.SelectedItem as HostViewModel;
            if (model != null)
            {
                SynchronizationTool.SelectedId = model.HostModel.ElementId;
                SynchronizationTool.ToolAction = ToolAction.ShowItem;
                SynchronizationTool.TheEvent.Raise();
            }
        }
    }
}
