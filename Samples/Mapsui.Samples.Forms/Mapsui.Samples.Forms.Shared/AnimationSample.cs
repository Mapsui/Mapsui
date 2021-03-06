using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;

namespace Mapsui.Samples.Forms
{
    public class AnimationSample : IFormsSample
    {
        public string Name => "Animation Sample";

        public string Category => "Forms";

        Random random = new Random();

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var navigator = (Navigator)mapView.Navigator;

            var newRot = random.NextDouble() * 360.0;

            //navigator.RotateTo(newRot, 500);
            navigator.FlyTo(e.Point.ToMapsui(), mapView.Viewport.Resolution * 8, 5000);

                var entry3 = new AnimationEntry(
                    start: pin.Color,
                    end: pin.Color,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.Linear,
                    tick: (entry, value) => {
                        var color = (Xamarin.Forms.Color)entry.Start;
                        pin.Color = new Xamarin.Forms.Color(color.R, color.G, value < 0.5 ? (1.0 - 2.0 * value) : ((value - 0.5) * 2.0));
                    },
                    final: (entry) => { pin.Color = (Xamarin.Forms.Color)entry.End; }
                );
                animations.Add(entry3);

                animation = new Animation(5000);
                animation.Loop = true;
                animation.Entries.AddRange(animations);
                animation.Start();
            }

            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            var mapView = mapControl as MapView;

            mapControl.Map = OsmSample.CreateMap();
        }
    }
}
