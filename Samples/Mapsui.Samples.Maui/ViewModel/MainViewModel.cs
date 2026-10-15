using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapsui.Samples.Common;
using Mapsui.UI.Maui;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables.

namespace Mapsui.Samples.Maui.ViewModel;

public partial class MainViewModel : ObservableObject
{
    static MainViewModel()
    {
        Common.Samples.Register();
    }

    public MainViewModel()
    {
        var allSamples = AllSamples.GetSamples() ?? new List<ISampleBase>();
        Categories = new ObservableCollection<string>(allSamples.Select(s => s.Category).Distinct().OrderBy(c => c));
        selectedCategory = Categories.First();
        PopulateSamples(selectedCategory);
        selectedSample = Samples.First();
    }

    [ObservableProperty]
    string selectedCategory;

    [ObservableProperty]
    ISampleBase? selectedSample;

    [ObservableProperty]
    Map? map;

    public ObservableCollection<ISampleBase> Samples { get; set; } = new();
    public ObservableCollection<string> Categories { get; } = new();

    // MapControl is needed in the samples. Mapsui's design should change so this is not needed anymore.
    public MapControl? MapControl { get; set; }

    public void Picker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var selection = SelectedSample;
        PopulateSamples(SelectedCategory);
        SelectedSample = selection?.Category == SelectedCategory ? selection : Samples.FirstOrDefault();
    }

    private void PopulateSamples(string selectedCategory)
    {
        var samples = AllSamples.GetSamples().OfType<ISampleBase>().Where(s => s.Category == selectedCategory);
        Samples.Clear();
        foreach (var sample in samples)
        {
            Samples.Add(sample);
        }
    }

    public event EventHandler? SampleChanged;

    public void CollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectedSample != null)
            SampleChanged?.Invoke(this, EventArgs.Empty);
    }
}
