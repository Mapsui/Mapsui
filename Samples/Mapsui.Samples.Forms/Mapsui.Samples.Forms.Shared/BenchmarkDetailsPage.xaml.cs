using Mapsui.Benchmark;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mapsui.Samples.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BenchmarkDetailsPage : ContentPage
    {
        public BenchmarkDetailsPage(List<RenderBenchmark> benchmarks)
        {
            InitializeComponent();
            Title = benchmarks[0].Name;
            benchmarksList.ItemsSource = benchmarks;
        }
    }
}