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
        IEnumerable<ISample> allSamples;
        Func<object, EventArgs, bool> clicker;

        public MainPage()
		{
            InitializeComponent();

            allSamples = AllSamples.GetSamples();

            var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
            foreach (var category in categories)
            {
                picker.Items?.Add(category);
            }
            picker.SelectedIndexChanged += PickerSelectedIndexChanged;
            picker.SelectedItem = "Forms";

            listView.ItemsSource = allSamples.Select(k => k.Name).ToList();
        }

        private void FillListWithSamples()
        {
            var selectedCategory = picker.SelectedItem?.ToString() ?? "";
            listView.ItemsSource = allSamples.Where(s => s.Category == selectedCategory).Select(x => x.Name);
        }

        private void PickerSelectedIndexChanged(object sender, EventArgs e)
        {
            FillListWithSamples();
        }

        void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

            var sampleName = e.SelectedItem.ToString();
            var sample = allSamples.Where(x => x.Name == sampleName).FirstOrDefault<ISample>();

            clicker = null;
            if (sample is IFormsSample)
                clicker = ((IFormsSample)sample).OnClick;

            ((NavigationPage)Application.Current.MainPage).PushAsync(new MapPage(sample.Setup, clicker));

            listView.SelectedItem = null;
        }
    }
}
