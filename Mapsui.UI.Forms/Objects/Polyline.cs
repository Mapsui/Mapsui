using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mapsui.UI.Forms
{
    public class Polyline : Drawable
    {
        private readonly ObservableRangeCollection<Position> _positions = new ObservableRangeCollection<Position>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Polyline"/> class.
        /// </summary>
        public Polyline()
        {
            _positions.CollectionChanged += OnCollectionChanged;

            CreateFeature();
        }

        /// <summary>
        /// Positions of line
        /// </summary>
        public IList<Position> Positions
        {
            get { return _positions; }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Positions):
                    Feature.Geometry = new LineString(Positions.Select(p => p.ToMapsui()).ToList());
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
                if (Feature == null)
                {
                    // Create a new one
                    Feature = new Feature
                    {
                        Geometry = new LineString(Positions.Select(p => p.ToMapsui()).ToList()),
                        ["Label"] = Label,
                    };
                    Feature.Styles.Clear();
                    Feature.Styles.Add(new VectorStyle
                    {
                        Line = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui()},
                        
                    });
                }
            }
        }
    }
}
