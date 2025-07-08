using System.Windows;
using NanoTools2.Utils;
using Prism.Regions;


namespace NanoTools2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
        }


        public MainWindow(IRegionManager regionManager)
        {
                InitializeComponent();
                //view discovery
                regionManager.RegisterViewWithRegion("MainRegion", typeof(AnalysesView));

        }
    }
}
