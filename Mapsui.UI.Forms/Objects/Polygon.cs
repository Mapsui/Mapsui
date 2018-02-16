using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public sealed class Polygon : BindableObject, IFeatureProvider
    {
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));
        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(Polygon), 1f);
        public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(nameof(StrokeColor), typeof(Xamarin.Forms.Color), typeof(Polygon), Xamarin.Forms.Color.Black);
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Xamarin.Forms.Color), typeof(Polygon), Xamarin.Forms.Color.DarkGray);
        public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(nameof(IsClickable), typeof(bool), typeof(Polygon), false);
        public static readonly BindableProperty ZIndexProperty = BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(Polygon), 0);

        private readonly ObservableCollection<Position> _positions = new ObservableCollection<Position>();
        private readonly ObservableCollection<Position[]> _holes = new ObservableCollection<Position[]>();

        private Action<Polygon, NotifyCollectionChangedEventArgs> _positionsChangedHandler = null;
        private Action<Polygon, NotifyCollectionChangedEventArgs> _holesChangedHandler = null;

        public Polygon()
        {
            _positions.CollectionChanged += OnPositionsCollectionChanged;
            _holes.CollectionChanged += OnHolesCollectionChanged;

            CreateFeature();
        }

        /// <summary>
        /// Label of polyline
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public float StrokeWidth
        {
            get { return (float)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        public Xamarin.Forms.Color StrokeColor
        {
            get { return (Xamarin.Forms.Color)GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }
        public Xamarin.Forms.Color FillColor
        {
            get { return (Xamarin.Forms.Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        public bool IsClickable
        {
            get { return (bool)GetValue(IsClickableProperty); }
            set { SetValue(IsClickableProperty, value); }
        }

        public int ZIndex
        {
            get { return (int)GetValue(ZIndexProperty); }
            set { SetValue(ZIndexProperty, value); }
        }

        public IList<Position> Positions
        {
            get { return _positions; }
        }

        public IList<Position[]> Holes
        {
            get { return _holes; }
        }

        public object Tag { get; set; }

        private Feature feature;

        public Feature Feature
        {
            get
            {
                return feature;
            }
            set
            {
                if (feature == null || !feature.Equals(value))
                    feature = value;
            }
        }

        public event EventHandler Clicked;

        internal bool SendTap()
        {
            EventHandler handler = Clicked;
            if (handler == null)
                return false;

            handler(this, EventArgs.Empty);
            return true;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Positions):
                    ((Mapsui.Geometries.Polygon)feature.Geometry).ExteriorRing = new LinearRing(Positions.Select(p => p.ToMapsui()).ToList());
                    break;
                case nameof(Holes):
                    ((Mapsui.Geometries.Polygon)feature.Geometry).InteriorRings = Holes.Select(h => new LinearRing(h.Select(p => p.ToMapsui()).ToList())).ToList();
                    break;
                case nameof(StrokeWidth):
                    ((VectorStyle)feature.Styles.First()).Line.Width = StrokeWidth;
                    break;
                case nameof(StrokeColor):
                    ((VectorStyle)feature.Styles.First()).Line.Color = StrokeColor.ToMapsui();
                    break;
                case nameof(FillColor):
                    ((VectorStyle)feature.Styles.First()).Fill = new Brush(FillColor.ToMapsui());
                    break;
            }
        }

        void OnPositionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Positions));
        }

        void OnHolesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Holes));
        }

        private object sync = new object();

        private void CreateFeature()
        {
            lock (sync)
            {
                if (feature == null)
                {
                    // Create a new one
                    feature = new Feature
                    {
                        Geometry = new Mapsui.Geometries.Polygon(), 
                        ["Label"] = Label,
                    };
                    feature.Styles.Clear();
                    feature.Styles.Add(new VectorStyle
                    {
                        Line = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui() },
                        Fill = new Brush { Color = FillColor.ToMapsui() }
                    });
                }
            }
        }
    }
}