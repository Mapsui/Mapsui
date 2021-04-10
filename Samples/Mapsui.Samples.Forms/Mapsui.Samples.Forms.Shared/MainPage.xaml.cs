using Mapsui.Samples.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Title = "Mapsui Samples";
            allSamples = AllSamples.GetSamples();

            var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
            picker.Items.Add("All");
            foreach (var category in categories)
            {
                picker.Items.Add(category);
            }
            listView.ItemsSource = allSamples.Select(k => k.Name).ToList();
            picker.SelectedIndexChanged += PickerSelectedIndexChanged;
            picker.SelectedItem = "All";
        }

        private void FillListWithSamples()
        {
            var selectedCategory = picker.SelectedItem?.ToString() ?? "";
            if (selectedCategory == "All")
            {
                listView.ItemsSource = allSamples.Select(x => x.Name);
            }
            else
            {
                listView.ItemsSource = allSamples.Where(s => s.Category == selectedCategory).Select(x => x.Name);
            }
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

            ((NavigationPage)Application.Current.MainPage).PushAsync(new MapPage(sampleName, sample.Setup, clicker));

            listView.SelectedItem = null;
        }
    }
}
