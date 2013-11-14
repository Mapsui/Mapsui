using System;
using System.Windows;

namespace Mapsui.Silverlight
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            Application.Current.Host.Content.Resized += Content_Resized;
            Application.Current.Host.Content.FullScreenChanged += Content_FullScreenChanged;
            GUI.SetMap(map);
        }

        void Content_FullScreenChanged(object sender, EventArgs e)
        {
            Width = Application.Current.Host.Content.ActualWidth;
            Height = Application.Current.Host.Content.ActualHeight;
        }

        void Content_Resized(object sender, EventArgs e)
        {
            Width = Application.Current.Host.Content.ActualWidth;
            Height = Application.Current.Host.Content.ActualHeight;
        }
    }
}
