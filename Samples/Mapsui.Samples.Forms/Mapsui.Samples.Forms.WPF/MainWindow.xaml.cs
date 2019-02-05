using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xamarin.Forms;
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
            // Never tested this. PDD.
            MbTilesHelper.DeployMbTilesFile(s => File.Create(System.IO.Path.Combine(MbTilesLocationOnWPF, s)));

            LoadApplication(new Mapsui.Samples.Forms.App());
        }

        private static string MbTilesLocationOnWPF => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
}
