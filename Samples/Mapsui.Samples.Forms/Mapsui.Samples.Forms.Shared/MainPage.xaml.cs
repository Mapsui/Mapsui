using Mapsui.Layers;
using Mapsui.Samples.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mapsui.Samples.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
	{
        Dictionary<string, Func<Map>> allSamples;

        public MainPage()
		{
            InitializeComponent();

            allSamples = Samples.CreateList();

            listView.ItemsSource = allSamples.Select(k => k.Key).ToList();
        }

        void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

            var sample = e.SelectedItem.ToString();
            var call = allSamples[sample];

            ((NavigationPage)Application.Current.MainPage).PushAsync(new MapPage(call, Samples.GetClicker(sample)));

            listView.SelectedItem = null;
        }
    }
}
