using Mapsui.Logging;
using Mapsui.Samples.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mapsui.Samples.Forms;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage : ContentPage
{
    static MainPage()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    IEnumerable<ISampleBase>? allSamples;
    Func<object?, EventArgs, bool>? clicker;

    public MainPage()
    {
        InitializeComponent();
        SetupToolbar();

        allSamples = AllSamples.GetSamples();

        var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c).ToList();
        categories.Insert(0, "All");
        picker.ItemsSource = categories;
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
            listView.ItemsSource = allSamples
                .Where(s => s.Category == selectedCategory)
                .Select(x => x.Name);
        }
    }

    private void PickerSelectedIndexChanged(object sender, EventArgs e)
    {
        FillListWithSamples();
    }

    private void LeakAction_Clicked(object sender, EventArgs e)
    {
        string report = Refs.Inspect();
        NavigateToPage(new LeaksPage(report));
    }

    private void SetupToolbar()
    {
        var leakButton = new ToolbarItem
        {
            Text = "Leaks",
            Order = ToolbarItemOrder.Secondary
        };
        leakButton.Clicked += LeakAction_Clicked;
        ToolbarItems.Add(leakButton);
    }

    void OnSelection(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
        {
            return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
        }

        var sampleName = e.SelectedItem.ToString();
        var sample = allSamples.Where(x => x.Name == sampleName).FirstOrDefault<ISampleBase>();

        clicker = null;
        if (sample is IFormsSample fsample)
            clicker = fsample.OnClick;

        NavigateToPage(new MapPage(sample, clicker));

        listView.SelectedItem = null;
    }

    public static async void NavigateToPage(Page page)
    {
        try
        {
            await ((NavigationPage)Application.Current.MainPage).PushAsync(page);
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, $"Error when navigating to page={page} exception={e}", e);
        }
    }
}
