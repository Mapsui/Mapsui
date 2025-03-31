﻿using System.Diagnostics.CodeAnalysis;
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
    private string? _sampleId;
    private bool _render;
    public List<string> Samples { get; set; } = [];
    public List<ISampleBase> MapSamples { get; set; } = [];
    public List<string> Categories { get; set; } = [];

    [Parameter][SupplyParameterFromQuery] public string? Category { get; set; }
    [Parameter][SupplyParameterFromQuery] public string? Sample { get; set; }

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
            if (!IsCategoryInRoute())
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
            SampleBase = MapSamples.FirstOrDefault(f => f.Name == SampleId);
            FillMap();
        }
    }

    [Parameter] public string? Title { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        LoggingWidget.ShowLoggingInMap = ActiveMode.Yes; // To show logging in release mode
        Performance.DefaultIsActive = ActiveMode.Yes; // To show performance in release mode
        FillComboBoxWithCategories();
        CategoryId = Categories[0];

        if (IsCategoryInRoute())
        {
            CategoryId = Category;
            FillSamples();
            ThrowIfSampleIsNotInRouteOrDoesNotExist();
            SampleId = Sample;
        }
        else
        {
            CategoryId = Categories[0]; // Set a default category
            FillSamples();
            SampleId = MapSamples.FirstOrDefault()?.Name;
        }
    }

    private bool IsCategoryInRoute()
    {
        if (!string.IsNullOrEmpty(Category))
        {
            if (!Categories.Contains(Category))
                throw new Exception($"Category '{Category}' does not exist. Choose from: '{string.Join(',', Categories)}'");
            return true;
        }
        return false;
    }

    private void ThrowIfSampleIsNotInRouteOrDoesNotExist()
    {
        if (string.IsNullOrEmpty(Sample))
            throw new Exception("If a category is specified the sample also needs to be specified.");
        if (!Samples.Contains(Sample))
            throw new Exception($"The sample `{Sample}` does not exist. Choose from: '{string.Join(',', Samples)}'");
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
    }

    private void FillSamples()
    {
        var list = AllSamples.GetSamples().Where(s => s.Category == CategoryId).OrderBy(c => c.Name);
        Samples.Clear();
        MapSamples.Clear();
        Samples.AddRange(list.Select(f => f.Name));
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
                _sourceCodeUrl = $@"../codesamples/{sample.GetType().Name}.html";
            }
        });
    }

    public ISampleBase? SampleBase { get; set; }
}
