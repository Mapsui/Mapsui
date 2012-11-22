using System;
using System.Windows.Controls;

namespace Mapsui.Silverlight
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            App.Current.Host.Content.Resized += Content_Resized;
            App.Current.Host.Content.FullScreenChanged += Content_FullScreenChanged;
            GUI.SetMap(map);
        }

        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            Width = App.Current.Host.Content.ActualWidth;
            Height = App.Current.Host.Content.ActualHeight;
        }

        void Content_Resized(object sender, EventArgs e)
        {
            Width = App.Current.Host.Content.ActualWidth;
            Height = App.Current.Host.Content.ActualHeight;
        }
    }
}
