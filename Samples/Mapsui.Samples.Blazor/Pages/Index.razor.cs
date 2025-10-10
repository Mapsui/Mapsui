using System.Diagnostics.CodeAnalysis;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.UI.Blazor;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Mapsui.Samples.Blazor.Pages;

public sealed partial class Index : IDisposable
{
    private string? _sourceCodeUrl = null;
    private int _activeTab = 0;
    private MapControl? _mapControl;
    private string? _categoryId;
    private string? _nameId;
    private bool _render;
    private bool _suppressHash;
    public List<string> SampleNames { get; set; } = [];
    public List<ISampleBase> MapSamples { get; set; } = [];
    public List<string> SampleCategories { get; set; } = [];

    [Inject] private NavigationManager Nav { get; set; } = default!;
    // no registration token needed; we'll unsubscribe in Dispose

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
            UpdateHashFromSelection();
        }
    }

    [Parameter] public string? Title { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        LoggingWidget.ShowLoggingInMap = ActiveMode.Yes; // To show logging in release mode
        Performance.DefaultIsActive = ActiveMode.Yes; // To show performance in release mode
        FillComboBoxWithCategories();
        // Subscribe to URL changes (hash changes will trigger LocationChanged)
        Nav.LocationChanged += OnLocationChanged;
        // Initialize from the current hash, or set the default
        InitializeFromHashOrDefault();
    }

    public void Dispose()
    {
        Nav.LocationChanged -= OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        // When the hash changes, update selection if needed
        if (TryParseHash(e.Location, out var category, out var name))
        {
            _suppressHash = true;
            try
            {
                if (!string.IsNullOrEmpty(category) && SampleCategories.Contains(category) && category != SampleCategory)
                {
                    SampleCategory = category; // setter updates samples
                }

                if (!string.IsNullOrEmpty(name) && SampleNames.Contains(name) && name != SampleName)
                {
                    SampleName = name; // setter updates map
                }
            }
            finally
            {
                _suppressHash = false;
            }

            StateHasChanged();
        }
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

    private void InitializeFromHashOrDefault()
    {
        if (TryParseHash(Nav.Uri, out var category, out var name))
        {
            _suppressHash = true;
            try
            {
                if (!string.IsNullOrEmpty(category) && SampleCategories.Contains(category))
                {
                    SampleCategory = category;
                }
                else
                {
                    SampleCategory = SampleCategories[0];
                }

                FillSamples();

                if (!string.IsNullOrEmpty(name) && SampleNames.Contains(name))
                {
                    SampleName = name;
                }
                else
                {
                    SampleName = MapSamples.FirstOrDefault()?.Name;
                }
            }
            finally
            {
                _suppressHash = false;
            }

            // Ensure canonical casing/encoding in hash
            UpdateHashFromSelection(replace: true);
        }
        else
        {
            // No hash: set defaults and write hash
            _suppressHash = true;
            try
            {
                SampleCategory = SampleCategories[0];
                FillSamples();
                SampleName = MapSamples.FirstOrDefault()?.Name;
            }
            finally
            {
                _suppressHash = false;
            }
            UpdateHashFromSelection(replace: true);
        }
    }

    private bool TryParseHash(string uri, out string? category, out string? name)
    {
        category = null;
        name = null;
        var u = new Uri(uri);
        var hash = u.Fragment; // includes leading '#'
        if (string.IsNullOrEmpty(hash)) return false;
        if (!hash.StartsWith("#")) return false;
        var path = hash.Length > 1 ? hash.Substring(1) : string.Empty; // remove '#'
        // Support both '#/Category/Name' and '#Category/Name'
        if (path.StartsWith("/")) path = path.Substring(1);
        if (string.IsNullOrEmpty(path)) return false;
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 1) category = Uri.UnescapeDataString(parts[0]);
        if (parts.Length >= 2) name = Uri.UnescapeDataString(parts[1]);
        return true;
    }

    private void UpdateHashFromSelection(bool replace = false)
    {
        if (_suppressHash) return;
        if (string.IsNullOrWhiteSpace(SampleCategory) || string.IsNullOrWhiteSpace(SampleName)) return;
        var categorySegment = Uri.EscapeDataString(SampleCategory);
        var nameSegment = Uri.EscapeDataString(SampleName);
        var hash = $"#/{categorySegment}/{nameSegment}";
        if (!string.Equals(new Uri(Nav.Uri).Fragment, hash, StringComparison.Ordinal))
        {
            // Navigate keeping us on the same page but updating the fragment
            Nav.NavigateTo(hash, forceLoad: false, replace: replace);
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
