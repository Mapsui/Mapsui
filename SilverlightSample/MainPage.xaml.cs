using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using SharpMap;
using SilverlightRendering;
using System.IO;
using System;

namespace SilverlightSample
{
    public partial class MainPage : UserControl
    {
        bool first = true;

        public MainPage()
        {
            InitializeComponent();
            this.UseLayoutRounding = true;
            this.Loaded += new System.Windows.RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (first)
            {
                InitializeMap();
                first = false;
            } 
        }
        void InitializeMap()
        {
            mapControl.Map = SharpMap.Samples.TileLayerSample.InitializeMap();
        }
    }
}
