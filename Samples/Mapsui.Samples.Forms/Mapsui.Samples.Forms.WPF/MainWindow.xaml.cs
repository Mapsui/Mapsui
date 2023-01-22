using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Utilities;
using System;
using System.IO;
using Xamarin.Forms.Platform.WPF;

namespace Mapsui.Samples.Forms.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FormsApplicationPage
{
    public MainWindow()
    {
        InitializeComponent();

        Xamarin.Forms.Forms.Init();

        LoadApplication(new Forms.App());
    }
}
