using System;
using System.Windows;
using System.Windows.Media;
using SharpMap;
using SharpMap.Samples;
using SilverlightRendering;
using SharpMap.Data;
using System.IO;
using System.Windows.Controls;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        bool first = true;

        public Window1()
        {
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            if (first)
            {
                InitializeMap();
                first = false;
            }
        }

        void InitializeMap()
        {
            mapControl.Map = SharpMap.Samples.GradiantThemeSample.InitializeMap();
        }
    }
}
