using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Editing;
using Mapsui.Nts.Widgets;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.IO;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Editing;

public class EditingSample : IMapControlSample
{
    private EditManager _editManager = new();
    private WritableLayer? _targetLayer;
    private IMapControl? _mapControl;
    private List<IFeature>? _tempFeatures;

    public string Name => "Editing";
    public string Category => "Editing";
    public void Setup(IMapControl mapControl)
    {
        _editManager = InitEditMode(mapControl, EditMode.Modify);
        mapControl.Map.Navigator.ZoomToBox(_editManager.GetGrownExtent());
        InitEditWidgets(mapControl.Map);
        _mapControl = mapControl;
    }

    public static EditManager InitEditMode(IMapControl mapControl, EditMode editMode)
    {
        var map = CreateMap();
        var editManager = new EditManager
        {
            Layer = (WritableLayer)map.Layers.First(l => l.Name == "EditLayer")
        };
        var targetLayer = (WritableLayer)map.Layers.First(l => l.Name == "Layer 3");

        // Load the polygon layer on startup so you can start modifying right away
        editManager.Layer.AddRange(targetLayer.GetFeatures().Copy());
        targetLayer.Clear();

        editManager.EditMode = editMode;

        map.Widgets.Add(new EditingWidget(mapControl, editManager));
        mapControl.Map = map;
        return editManager;
    }

    private void InitEditWidgets(Map map)
    {
        _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 3") as WritableLayer;

        map.Widgets.Add(CreateBoxForEditWidgets());
        map.Widgets.Add(CreateSelectLayerToEditButton());
        map.Widgets.Add(CreateSelectLayer1Button(map));
        map.Widgets.Add(CreateSelectLayer2Button(map));
        map.Widgets.Add(CreateSelectLayer3Button(map));
        map.Widgets.Add(CreateSaveButton());
        map.Widgets.Add(CreateLoadButton());
        map.Widgets.Add(CreateCancelButton());
        map.Widgets.Add(CreateEditModesTextBox());
        map.Widgets.Add(CreateAddPointButton());
        map.Widgets.Add(CreateAddLineButton());
        map.Widgets.Add(CreateAddPolygonButton());
        map.Widgets.Add(CreateModifyButton());
        map.Widgets.Add(CreateRotateButton());
        map.Widgets.Add(CreateScaleButton());
        map.Widgets.Add(CreateNoneButton());
        map.Widgets.Add(CreateSelectButton());
        map.Widgets.Add(CreateDeleteButton());
        map.Widgets.Add(new MouseCoordinatesWidget());
    }

    private static BoxWidget CreateBoxForEditWidgets() => new BoxWidget
    {
        Width = 130,
        Height = 370,
        Position = new MPoint(2, 0),
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
    };

    private static TextBoxWidget CreateSelectLayerToEditButton() => new TextBoxWidget
    {
        Position = new MPoint(0, 5),
        Width = 120,
        Height = 18,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Select Layer To Edit:",
        BackColor = Color.Transparent,
    };

    private ButtonWidget CreateDeleteButton() => new()
    {
        Position = new MPoint(5, 340),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Delete",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            if (_editManager.SelectMode)
            {
                var selectedFeatures = _editManager.Layer?.GetFeatures().Where(f => (bool?)f["Selected"] == true) ?? [];
                foreach (var selectedFeature in selectedFeatures)
                    _editManager.Layer?.TryRemove(selectedFeature);
                _mapControl?.RefreshGraphics();
            }

            return true;
        }
    };

    private ButtonWidget CreateSelectButton() => new()
    {
        Position = new MPoint(5, 320),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Select (for delete)",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _editManager.SelectMode = !_editManager.SelectMode;
            return true;
        }
    };

    private ButtonWidget CreateNoneButton() => new()
    {
        Position = new MPoint(5, 290),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "None",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _editManager.EditMode = EditMode.None;
            return true;
        }
    };

    private ButtonWidget CreateScaleButton() => new()
    {
        Position = new MPoint(5, 270),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Scale",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _editManager.EditMode = EditMode.Scale;
            return true;
        }
    };

    private ButtonWidget CreateRotateButton() => new()
    {
        Position = new MPoint(5, 250),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Rotate",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _editManager.EditMode = EditMode.Rotate;
            return true;
        }
    };

    private ButtonWidget CreateModifyButton() => new()
    {
        Position = new MPoint(5, 230),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Modify",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _editManager.EditMode = EditMode.Modify;
            return true;
        }
    };

    private ButtonWidget CreateAddPolygonButton() => new()
    {
        Position = new MPoint(5, 210),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Add Polygon",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? [];
            foreach (var feature in features)
                feature.Modified();
            _tempFeatures = new List<IFeature>(features);
            _editManager.EditMode = EditMode.AddPolygon;
            return true;
        }
    };

    private ButtonWidget CreateAddLineButton() => new()
    {
        Position = new MPoint(5, 190),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Add Line",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? [];
            foreach (var feature in features)
                feature.Modified();
            _tempFeatures = new List<IFeature>(features);
            _editManager.EditMode = EditMode.AddLine;
            return true;
        }
    };

    private ButtonWidget CreateAddPointButton() => new()
    {
        Position = new MPoint(5, 170),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Add Point",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? [];
            foreach (var feature in features)
                feature.Modified();
            _tempFeatures = new List<IFeature>(features);
            _editManager.EditMode = EditMode.AddPoint;
            return true;
        }
    };

    private static TextBoxWidget CreateEditModesTextBox() => new()
    {
        Position = new MPoint(5, 150),
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Editing Modes:",
        BackColor = Color.Transparent,
    };

    private ButtonWidget CreateCancelButton() => new()
    {
        Position = new MPoint(5, 120),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Cancel",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            if (_targetLayer != null && _tempFeatures != null)
            {
                _targetLayer.Clear();
                _targetLayer.AddRange(_tempFeatures.Copy());
                _targetLayer.DataHasChanged();
                _mapControl?.RefreshGraphics();
            }

            _editManager.Layer?.Clear();
            _mapControl?.RefreshGraphics();
            _editManager.EditMode = EditMode.None;
            _tempFeatures = null;
            return true;
        }
    };

    private ButtonWidget CreateLoadButton() => new()
    {
        Position = new MPoint(5, 100),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Load",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? [];

            foreach (var feature in features)
                feature.Modified();

            _tempFeatures = new List<IFeature>(features);

            _editManager.Layer?.AddRange(features);
            _targetLayer?.Clear();

            _mapControl?.RefreshGraphics();
            return true;
        }
    };

    private ButtonWidget CreateSaveButton() => new()
    {
        Position = new MPoint(5, 80),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Save",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _targetLayer?.AddRange(_editManager.Layer?.GetFeatures().Copy() ?? []);
            _editManager.Layer?.Clear();

            _mapControl?.RefreshGraphics();
            return true;
        }
    };

    private ButtonWidget CreateSelectLayer3Button(Map map) => new()
    {
        Position = new MPoint(5, 60),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Layer 3",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 3") as WritableLayer;
            return true;
        }
    };

    private ButtonWidget CreateSelectLayer2Button(Map map) => new()
    {
        Position = new MPoint(5, 40),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Layer 2",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 2") as WritableLayer;
            return true;
        }
    };

    private ButtonWidget CreateSelectLayer1Button(Map map) => new()
    {
        Position = new MPoint(5, 20),
        Height = 18,
        Width = 120,
        CornerRadius = 2,
        HorizontalAlignment = HorizontalAlignment.Absolute,
        VerticalAlignment = VerticalAlignment.Absolute,
        Text = "Layer 1",
        BackColor = Color.LightGray,
        Tapped = (_, e) =>
        {
            _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 1") as WritableLayer;
            return true;
        }
    };

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Layers.Add(CreateLineLayer());
        map.Layers.Add(CreatePolygonLayer());
        var editLayer = CreateEditLayer();
        map.Layers.Add(editLayer);

        return map;
    }

    private static WritableLayer CreateEditLayer() => new()
    {
        Name = "EditLayer",
        Style = CreateEditLayerStyle(),
        IsMapInfoLayer = true
    };

    // The edit layer has two styles. That is why it needs to use a StyleCollection.
    // In a future version of Mapsui the ILayer will have a Styles collections just
    // as the GeometryFeature has right now.
    // The first style is the basic style of the features in edit mode.
    // The second style is the way to show a feature is selected.
    private static StyleCollection CreateEditLayerStyle() => new()
    {
        Styles =
        {
            CreateEditLayerBasicStyle(),
            CreateSelectedStyle(),
            CreateStyleToShowTheVertices(),
        }
    };

    private static SymbolStyle CreateStyleToShowTheVertices() => new() { SymbolScale = 0.5 };

    private static VectorStyle CreateEditLayerBasicStyle() => new()
    {
        Fill = new Brush(_editModeColor),
        Line = new Pen(_editModeColor, 3),
        Outline = new Pen(_editModeColor, 3)
    };

    private static readonly Color _editModeColor = new(124, 22, 111, 180);
    private static readonly Color _pointLayerColor = new(240, 240, 240, 240);
    private static readonly Color _lineLayerColor = new(150, 150, 150, 240);
    private static readonly Color _polygonLayerColor = new(20, 20, 20, 240);

    private static readonly SymbolStyle? _selectedStyle = new()
    {
        Fill = null,
        Outline = new Pen(Color.Red, 3),
        Line = new Pen(Color.Red, 3)
    };

    private static readonly SymbolStyle? _disableStyle = new() { Enabled = false };

    // To show the selected style a ThemeStyle is used which switches on and off the SelectedStyle
    // depending on a "Selected" attribute.
    private static ThemeStyle CreateSelectedStyle()
        => new(f => (bool?)f["Selected"] == true ? _selectedStyle : _disableStyle);

    private static WritableLayer CreatePointLayer() => new()
    {
        Name = "Layer 1",
        Style = CreatePointStyle()
    };

    private static WritableLayer CreateLineLayer()
    {
        var lineLayer = new WritableLayer
        {
            Name = "Layer 2",
            Style = CreateLineStyle()
        };

        // todo: add data

        return lineLayer;
    }

    private static WritableLayer CreatePolygonLayer()
    {
        var polygonLayer = new WritableLayer
        {
            Name = "Layer 3",
            Style = CreatePolygonStyle()
        };

        var wkt = "POLYGON ((1261416.17275404 5360656.05714234, 1261367.50386493 5360614.2556425, 1261353.47050427 5360599.62511755, 1261338.83997932 5360576.03712836, 1261337.34706862 5360570.6626498, 1261375.8641649 5360511.2448036, 1261383.92588273 5360483.17808227, 1261391.98760055 5360485.56673941, 1261393.48051126 5360480.490843, 1261411.99260405 5360487.6568144, 1261430.50469684 5360496.9128608, 1261450.21111819 5360507.06465361, 1261472.00761454 5360525.5767464, 1261488.13105019 5360544.98458561, 1261488.1310502 5360545.28316775, 1261481.26366093 5360549.76189988, 1261489.6239609 5360560.21227484, 1261495.59560374 5360555.13637843, 1261512.91336796 5360573.05130694, 1261535.00844645 5360598.43078898, 1261540.08434286 5360619.03295677, 1261535.90419287 5360621.12303176, 1261526.64814648 5360623.21310675, 1261489.32537876 5360644.41243881, 1261458.27283602 5360661.73020303, 1261438.26783253 5360662.02878517, 1261427.22029328 5360660.23729232, 1261416.17275404 5360656.05714234))";
        var polygon = new WKTReader().Read(wkt);
        IFeature feature = new GeometryFeature { Geometry = polygon };
        polygonLayer.Add(feature);

        return polygonLayer;
    }

    private static VectorStyle CreatePointStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(_pointLayerColor),
            Line = new Pen(_pointLayerColor, 3),
            Outline = new Pen(Color.Gray, 2)
        };
    }

    private static VectorStyle CreateLineStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(_lineLayerColor),
            Line = new Pen(_lineLayerColor, 3),
            Outline = new Pen(_lineLayerColor, 3)
        };
    }
    private static VectorStyle CreatePolygonStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(new Color(_polygonLayerColor)),
            Line = new Pen(_polygonLayerColor, 3),
            Outline = new Pen(_polygonLayerColor, 3)
        };
    }
}
