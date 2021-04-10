using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mapsui.Samples.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BenchmarkSettingsPage : ContentPage
    {
        public BenchmarkSettingsPage()
        {
            InitializeComponent();
            BenchmarkingToggle.IsToggled = Benchmark.Benchmarking.Enabled;
            BenchmarkingToggle.Toggled += BenchmarkingToggle_OnChanged;


            var allBenchmarks = Benchmark.Benchmarking.AllBenchmarks;
            benchMarkHistory.ItemsSource = allBenchmarks.Select(x => x[0]).Reverse().ToList();
        }

        private void BenchmarkingToggle_OnChanged(object sender, ToggledEventArgs e)
        {
            Benchmark.Benchmarking.Enabled = e.Value;
        }

        private void Erase_Clicked(object sender, System.EventArgs e)
        {
            Benchmark.Benchmarking.AllBenchmarks.Clear();
            benchMarkHistory.ItemsSource = null;

            var allBenchmarks = Benchmark.Benchmarking.AllBenchmarks;
            benchMarkHistory.ItemsSource = allBenchmarks.Select(x => x[0]).ToList();
        }

        private void benchMarkHistory_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var allBenchmarks = Benchmark.Benchmarking.AllBenchmarks;
            var idx = allBenchmarks.Count - e.SelectedItemIndex - 1;
            MainPage.NavigateToPage(new BenchmarkDetailsPage(allBenchmarks[idx]));
        }
    }
}