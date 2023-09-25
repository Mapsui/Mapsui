using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Editing;
using Mapsui.Nts.Layers;
using Mapsui.Nts.Widgets;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidget;
using Mapsui.Widgets.ButtonWidget;
using Mapsui.Widgets.MouseCoordinatesWidget;
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

        var editManipulation = new EditManipulation();

        map.Home = _ =>
        {
            if (editManager.Layer.Extent != null)
            {
                var extent = editManager.Layer.Extent!.Grow(editManager.Layer.Extent.Width * 0.2);
                map.Navigator.ZoomToBox(extent);
            }
        };

        map.Widgets.Add(new EditingWidget(mapControl, editManager, editManipulation));
        mapControl.Map = map;
        return editManager;
    }

    private void InitEditWidgets(Map map)
    {
        _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 3") as WritableLayer;

        map.Widgets.Add(new BoxWidget
        {
            Width = 130,
            Height = 370,
            MarginY = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        });

        map.Widgets.Add(new TextBox
        {
            MarginY = 0,
            MarginX = 5,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Select Layer To Edit:",
            BackColor = Color.Transparent,
        });

        // Layers
        var layer1 = new ButtonWidget
        {
            MarginY = 20,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Layer 1",
            BackColor = Color.LightGray,
        };
        layer1.WidgetTouched += (_, e) =>
        {
            _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 1") as WritableLayer;
            e.Handled = true;
        };

        map.Widgets.Add(layer1);
        var layer2 = new ButtonWidget
        {
            MarginY = 40,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Layer 2",
            BackColor = Color.LightGray,
        };
        layer2.WidgetTouched += (_, e) =>
        {
            _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 2") as WritableLayer;
            e.Handled = true;
        };
        map.Widgets.Add(layer2);
        var layer3 = new ButtonWidget
        {
            MarginY = 60,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Layer 3",
            BackColor = Color.LightGray,
        };
        layer3.WidgetTouched += (_, e) =>
        {
            _targetLayer = map.Layers.FirstOrDefault(f => f.Name == "Layer 3") as WritableLayer;
            e.Handled = true;
        };
        map.Widgets.Add(layer3);
        // Persistence
        var save = new ButtonWidget
        {
            MarginY = 80,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Save",
            BackColor = Color.LightGray,
        };
        save.WidgetTouched += (_, e) =>
        {
            _targetLayer?.AddRange(_editManager.Layer?.GetFeatures().Copy() ?? new List<IFeature>());
            _editManager.Layer?.Clear();

            _mapControl?.RefreshGraphics();
            e.Handled = true;
        };
        map.Widgets.Add(save);
        var load = new ButtonWidget
        {
            MarginY = 100,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Load",
            BackColor = Color.LightGray,
        };
        load.WidgetTouched += (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }

            _tempFeatures = new List<IFeature>(features);

            _editManager.Layer?.AddRange(features);
            _targetLayer?.Clear();

            _mapControl?.RefreshGraphics();
            e.Handled = true;
        };
        map.Widgets.Add(load);
        var cancel = new ButtonWidget
        {
            MarginY = 120,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Cancel",
            BackColor = Color.LightGray,
        };
        cancel.WidgetTouched += (_, e) =>
        {
            if (_targetLayer != null && _tempFeatures != null)
            {
                _targetLayer.Clear();
                _targetLayer.AddRange(_tempFeatures.Copy());
                _mapControl?.RefreshGraphics();
            }

            _editManager.Layer?.Clear();

            _mapControl?.RefreshGraphics();

            _editManager.EditMode = EditMode.None;

            _tempFeatures = null;
            e.Handled = true;
        };
        map.Widgets.Add(cancel);

        map.Widgets.Add(new TextBox
        {
            MarginY = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Editing Modes:",
            BackColor = Color.Transparent,
        });
        // Editing Modes
        var addPoint = new ButtonWidget
        {
            MarginY = 170,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Add Point",
            BackColor = Color.LightGray,
        };
        addPoint.WidgetTouched += (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }

            _tempFeatures = new List<IFeature>(features);

            _editManager.EditMode = EditMode.AddPoint;
            e.Handled = true;
        };
        map.Widgets.Add(addPoint);
        var addLine = new ButtonWidget
        {
            MarginY = 190,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Add Line",
            BackColor = Color.LightGray,
        };
        addLine.WidgetTouched += (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }

            _tempFeatures = new List<IFeature>(features);

            _editManager.EditMode = EditMode.AddLine;
            e.Handled = true;
        };
        map.Widgets.Add(addLine);
        var addPolygon = new ButtonWidget
        {
            MarginY = 210,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Add Polygon",
            BackColor = Color.LightGray,
        };
        addPolygon.WidgetTouched += (_, e) =>
        {
            var features = _targetLayer?.GetFeatures().Copy() ?? Array.Empty<IFeature>();

            foreach (var feature in features)
            {
                feature.RenderedGeometry.Clear();
            }

            _tempFeatures = new List<IFeature>(features);

            _editManager.EditMode = EditMode.AddPolygon;
            e.Handled = true;
        };
        map.Widgets.Add(addPolygon);
        var modify = new ButtonWidget
        {
            MarginY = 230,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Modify",
            BackColor = Color.LightGray,
        };
        modify.WidgetTouched += (_, e) =>
        {
            _editManager.EditMode = EditMode.Modify;
            e.Handled = true;
        };
        map.Widgets.Add(modify);
        var rotate = new ButtonWidget
        {
            MarginY = 250,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Rotate",
            BackColor = Color.LightGray,
        };
        rotate.WidgetTouched += (_, e) =>
        {
            _editManager.EditMode = EditMode.Rotate;
            e.Handled = true;
            
        };
        map.Widgets.Add(rotate);
        var scale = new ButtonWidget
        {
            MarginY = 270,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Scale",
            BackColor = Color.LightGray,
        };
        scale.WidgetTouched += (_, e) =>
        {
            _editManager.EditMode = EditMode.Scale;
            e.Handled = true;
        };
        map.Widgets.Add(scale);
        var none = new ButtonWidget
        {
            MarginY = 290,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "None",
            BackColor = Color.LightGray,
        };
        none.WidgetTouched += (_, e) =>
        {
            _editManager.EditMode = EditMode.None;
            e.Handled = true;
        };
        map.Widgets.Add(none);

        // Deletion
        var selectForDelete = new ButtonWidget
        {
            MarginY = 320,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Select (for delete)",
            BackColor = Color.LightGray,
        };
        selectForDelete.WidgetTouched += (_, e) =>
        {
            _editManager.SelectMode = !_editManager.SelectMode;
            e.Handled = true;
        };
        map.Widgets.Add(selectForDelete);
        var delete = new ButtonWidget
        {
            MarginY = 340,
            MarginX = 5,
            Height = 18,
            Width = 120,
            CornerRadius = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Text = "Delete",
            BackColor = Color.LightGray,
        };
        delete.WidgetTouched += (_, e) =>
        {
            if (_editManager.SelectMode)
            {
                var selectedFeatures = _editManager.Layer?.GetFeatures().Where(f => (bool?)f["Selected"] == true) ??
                                       Array.Empty<IFeature>();

                foreach (var selectedFeature in selectedFeatures)
                {
                    _editManager.Layer?.TryRemove(selectedFeature);
                }

                _mapControl?.RefreshGraphics();
            }

            e.Handled = true;
        };
        map.Widgets.Add(delete);

        // Mouse Position Widget
        map.Widgets.Add(new MouseCoordinatesWidget(map));

    }

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Layers.Add(CreateLineLayer());
        map.Layers.Add(CreatePolygonLayer());
        var editLayer = CreateEditLayer();
        map.Layers.Add(editLayer);
        map.Layers.Add(new VertexOnlyLayer(editLayer) { Name = "VertexLayer" });
        return map;
    }

    private static WritableLayer CreateEditLayer()
    {
        return new WritableLayer
        {
            Name = "EditLayer",
            Style = CreateEditLayerStyle(),
            IsMapInfoLayer = true
        };
    }

    private static StyleCollection CreateEditLayerStyle()
    {
        // The edit layer has two styles. That is why it needs to use a StyleCollection.
        // In a future version of Mapsui the ILayer will have a Styles collections just
        // as the GeometryFeature has right now.
        // The first style is the basic style of the features in edit mode.
        // The second style is the way to show a feature is selected.
        return new StyleCollection
        {
            Styles = {
                CreateEditLayerBasicStyle(),
                CreateSelectedStyle()
            }
        };
    }

    private static IStyle CreateEditLayerBasicStyle()
    {
        var editStyle = new VectorStyle
        {
            Fill = new Brush(EditModeColor),
            Line = new Pen(EditModeColor, 3),
            Outline = new Pen(EditModeColor, 3)
        };
        return editStyle;
    }

    private static readonly Color EditModeColor = new Color(124, 22, 111, 180);
    private static readonly Color PointLayerColor = new Color(240, 240, 240, 240);
    private static readonly Color LineLayerColor = new Color(150, 150, 150, 240);
    private static readonly Color PolygonLayerColor = new Color(20, 20, 20, 240);


    private static readonly SymbolStyle? SelectedStyle = new SymbolStyle
    {
        Fill = null,
        Outline = new Pen(Color.Red, 3),
        Line = new Pen(Color.Red, 3)
    };

    private static readonly SymbolStyle? DisableStyle = new SymbolStyle { Enabled = false };

    private static IStyle CreateSelectedStyle()
    {
        // To show the selected style a ThemeStyle is used which switches on and off the SelectedStyle
        // depending on a "Selected" attribute.
        return new ThemeStyle(f => (bool?)f["Selected"] == true ? SelectedStyle : DisableStyle);
    }

    private static WritableLayer CreatePointLayer()
    {
        return new WritableLayer
        {
            Name = "Layer 1",
            Style = CreatePointStyle()
        };
    }

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

    private static IStyle CreatePointStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(PointLayerColor),
            Line = new Pen(PointLayerColor, 3),
            Outline = new Pen(Color.Gray, 2)
        };
    }

    private static IStyle CreateLineStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(LineLayerColor),
            Line = new Pen(LineLayerColor, 3),
            Outline = new Pen(LineLayerColor, 3)
        };
    }
    private static IStyle CreatePolygonStyle()
    {
        return new VectorStyle
        {
            Fill = new Brush(new Color(PolygonLayerColor)),
            Line = new Pen(PolygonLayerColor, 3),
            Outline = new Pen(PolygonLayerColor, 3)
        };
    }
}
