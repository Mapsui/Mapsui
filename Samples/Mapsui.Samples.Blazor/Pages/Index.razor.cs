using System.Diagnostics.CodeAnalysis;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.UI.Blazor;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using Microsoft.AspNetCore.Components;

namespace Mapsui.Samples.Blazor.Pages;

public partial class Index
{
    private string? _sourceCodeUrl = null;
    private int _activeTab = 0;
    private MapControl? _mapControl;
    private string? _categoryId;
    private string? _nameId;
    private bool _render;
    public List<string> SampleNames { get; set; } = [];
    public List<ISampleBase> MapSamples { get; set; } = [];
    public List<string> SampleCategories { get; set; } = [];

    [Inject] private NavigationManager Nav { get; set; } = default!;

    // Route parameters (bound from @page templates in Index.razor)
    [Parameter] public string? Category { get; set; }
    [Parameter] public string? Name { get; set; }

    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties")]
    public string? SampleCategory
    {
        get => _categoryId;
        set
        {
            if (_categoryId == value)
            {
                return;
            }

            _categoryId = value;
            // Update the available samples for the selected category
            FillSamples();
            // Automatically select the first sample from the new category
            SampleName = MapSamples.FirstOrDefault()?.Name;
        }
    }

    [Parameter]
    [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties")]
    public string? SampleName
    {
        get => _nameId;
        set
        {
            if (_nameId == value)
            {
                return;
            }

            _nameId = value;
            SampleBase = MapSamples.FirstOrDefault(f => f.Name == SampleName);
            FillMap();
            NavigateToCanonical(replace: false);
        }
    }

    [Parameter] public string? Title { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        LoggingWidget.ShowLoggingInMap = ActiveMode.Yes; // To show logging in release mode
        Performance.DefaultIsActive = ActiveMode.Yes; // To show performance in release mode
        FillComboBoxWithCategories();
        SampleCategory = SampleCategories[0];

        if (IsCategoryInRoute())
        {
            SampleCategory = Category;
            FillSamples();
            if (IsNameInRoute())
            {
                ThrowIfNameIsNotInRouteOrDoesNotExist();
                SampleName = Name; // Select the named sample
                NavigateToCanonical(replace: true); // ensure canonical casing/encoding
            }
            else
            {
                // Category-only path: pick first sample in category
                SampleName = MapSamples.FirstOrDefault()?.Name;
                NavigateToCanonical(replace: true);
            }
        }
        else
        {
            SampleCategory = SampleCategories[0]; // Set a default category
            FillSamples();
            SampleName = MapSamples.FirstOrDefault()?.Name;
            NavigateToCanonical(replace: true); // From "/" to "/{Category}/{Name}"
        }
    }

    private bool IsCategoryInRoute()
    {
        if (!string.IsNullOrEmpty(Category))
        {
            if (!SampleCategories.Contains(Category))
                throw new Exception($"Category '{Category}' does not exist. Choose from: '{string.Join(',', SampleCategories)}'");
            return true;
        }
        return false;
    }

    private bool IsNameInRoute()
    {
        return !string.IsNullOrEmpty(Name);
    }

    private void ThrowIfNameIsNotInRouteOrDoesNotExist()
    {
        if (string.IsNullOrEmpty(Name))
            throw new Exception("If a category is specified the name also needs to be specified.");
        if (!SampleNames.Contains(Name))
            throw new Exception($"The sample `{Name}` does not exist. Choose from: '{string.Join(',', SampleNames)}'");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (_activeTab == 0 && (firstRender || _render))
        {
            _render = false;
            FillMap();
            if (_mapControl != null)
            {
                _mapControl.UseContinuousMouseWheelZoom = true;
            }
        }
    }

    private void SetActiveTab(int tab)
    {
        _activeTab = tab;
        _render = true;
        StateHasChanged(); // Add this line
    }

    private void NavigateToCanonical(bool replace)
    {
        if (string.IsNullOrWhiteSpace(SampleCategory) || string.IsNullOrWhiteSpace(SampleName)) return;

        var categorySegment = Uri.EscapeDataString(SampleCategory);
        var nameSegment = Uri.EscapeDataString(SampleName);
        var target = $"/{categorySegment}/{nameSegment}";

        // Only navigate if different to avoid loops
        var current = new Uri(Nav.Uri);
        var currentPathAndQuery = current.PathAndQuery; // includes path and query
        if (!currentPathAndQuery.Equals(target, StringComparison.Ordinal))
        {
            Nav.NavigateTo(target, replace);
        }
    }

    private void FillComboBoxWithCategories()
    {
        Common.Samples.Register();

        var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c);
        foreach (var category in categories)
        {
            SampleCategories.Add(category);
        }
    }

    private void FillSamples()
    {
        var list = AllSamples.GetSamples().Where(s => s.Category == SampleCategory).OrderBy(c => c.Name);
        SampleNames.Clear();
        MapSamples.Clear();
        SampleNames.AddRange(list.Select(f => f.Name));
        MapSamples.AddRange(list);
    }

    private void FillMap()
    {
        Catch.Exceptions(async () =>
        {
            if (SampleBase != null && _mapControl != null)
            {
                var sample = SampleBase;
                Title = sample.Name;
                await sample.SetupAsync(_mapControl);
                _sourceCodeUrl = $@"./codesamples/{sample.GetType().Name}.html";
            }
        });
    }

    public ISampleBase? SampleBase { get; set; }
}
