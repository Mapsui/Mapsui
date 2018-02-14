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
    public sealed class Polyline : BindableObject, IFeatureProvider
    {
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));
        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(Polyline), 1f);
        public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(nameof(StrokeColor), typeof(Xamarin.Forms.Color), typeof(Polyline), Xamarin.Forms.Color.Black);
        public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(nameof(IsClickable), typeof(bool), typeof(Polyline), false);
        public static readonly BindableProperty ZIndexProperty = BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(Polyline), 0);

        private readonly ObservableCollection<Position> _positions = new ObservableCollection<Position>();

        public Polyline()
        {
            _positions.CollectionChanged += OnCollectionChanged;

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
                    feature.Geometry = new LineString(Positions.Select(p => p.ToMapsui()).ToList());
                    break;
                case nameof(StrokeWidth):
                case nameof(StrokeColor):
                    var color = StrokeColor.ToMapsui();
                    ((VectorStyle)feature.Styles.First()).Line = new Pen(StrokeColor.ToMapsui(), StrokeWidth);
                    break;
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Positions));
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
                        Geometry = new LineString(Positions.Select(p => p.ToMapsui()).ToList()),
                        ["Label"] = Label,
                    };
                    feature.Styles.Clear();
                    feature.Styles.Add(new VectorStyle
                    {
                        Line = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui()},
                        
                    });
                }
            }
        }
    }
}
