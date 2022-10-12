using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using System;
using System.IO;
using Xamarin.Forms.Platform.WPF;

namespace Mapsui.Samples.Forms.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();

            Xamarin.Forms.Forms.Init();

            // Hack to tell the platform independent samples where the files can be found on iOS.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnWPF;
            MbTilesHelper.DeployMbTilesFile(MbTilesLocationOnWPF);

            LoadApplication(new Mapsui.Samples.Forms.App());
        }

        private static string MbTilesLocationOnWPF => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
}
