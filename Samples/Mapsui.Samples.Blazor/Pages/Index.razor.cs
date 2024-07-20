using System.Diagnostics.CodeAnalysis;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.Widgets;
using Mapsui.UI.Blazor;
using Mapsui.Widgets.InfoWidgets;
using Microsoft.AspNetCore.Components;

namespace Mapsui.Samples.Blazor.Pages;

public partial class Index
{
    private string? _sourceCodeUrl = null;
    private int _activeTab = 0;
    private MapControl? _mapControl;
    private string? _categoryId;
    private string? _sampleId;
    private bool _render;
    public List<string> Samples { get; set; } = new();
    public List<ISampleBase> MapSamples { get; set; } = new();
    public List<string> Categories { get; set; } = new();

    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties")]
    public string? CategoryId
    {
        get => _categoryId;
        set
        {
            if (_categoryId == value)
            {
                return;
            }

            _categoryId = value;
            FillSamples();
        }
    }

    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties")]
    public string? SampleId
    {
        get => _sampleId;
        set
        {
            if (_sampleId == value)
            {
                return;
            }

            _sampleId = value;
            Sample = MapSamples.FirstOrDefault(f => f.Name == SampleId);
            FillMap();
        }
    }

    [Parameter] public string? Title { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        LoggingWidget.ShowLoggingInMap = ShowLoggingInMap.WhenLoggingWidgetIsEnabled; // To show logging in release mode
        FillComboBoxWithCategories();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (_activeTab == 0 && (firstRender || _render))
        {
            _render = false;
            FillMap();
            if (_mapControl != null)
                _mapControl.Renderer.WidgetRenders[typeof(CustomWidget)] = new CustomWidgetSkiaRenderer();
        }
    }

    private void SetActiveTab(int tab)
    {
        _activeTab = tab;
        _render = true;
        StateHasChanged(); // Add this line
    }

    private void FillComboBoxWithCategories()
    {
        // register Samples
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();

        var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c);
        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        CategoryId = Categories[0];

        FillSamples();
    }

    private void FillSamples()
    {
        var list = AllSamples.GetSamples().Where(s => s.Category == CategoryId).OrderBy(c => c.Name);
        Samples.Clear();
        MapSamples.Clear();
        Samples.AddRange(list.Select(f => f.Name));
        MapSamples.AddRange(list);
        SampleId = MapSamples.FirstOrDefault()?.Name;
    }

    private void FillMap()
    {
        Catch.Exceptions(async () =>
        {
            if (Sample != null && _mapControl != null)
            {
                var sample = Sample;
                Title = sample.Name;
                await sample.SetupAsync(_mapControl);
                _sourceCodeUrl = $@"../codesamples/{sample.GetType().Name}.html";
            }
        });
    }

    public ISampleBase? Sample { get; set; }
}
