using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapsui.Logging;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Maui;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables.

namespace Mapsui.Samples.Maui.ViewModel;

public partial class MainViewModel : ObservableObject
{
    static MainViewModel()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
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
    ISampleBase selectedSample;

    [ObservableProperty]
    Map? map;

    public ObservableCollection<ISampleBase> Samples { get; set; } = new();
    public ObservableCollection<string> Categories { get; } = new();

    // MapControl is needed in the samples. Mapsui's design should change so this is not needed anymore.
    public MapControl? MapControl { get; set; }

    public void Picker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        PopulateSamples(SelectedCategory);
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

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    public async void CollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (SelectedSample is null)
                return;

            if (SelectedSample is ISample sample)
                Map = await sample.CreateMapAsync();
            else if (SelectedSample is IMapControlSample mapControlSample && MapControl != null)
                mapControlSample.Setup(MapControl);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }
}
