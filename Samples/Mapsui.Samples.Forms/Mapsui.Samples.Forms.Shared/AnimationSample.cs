using System;
using System.Collections.Generic;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Mapsui.Utilities;

namespace Mapsui.Samples.Forms
{
    public class AnimationSample : IFormsSample
    {
        public string Name => "Animation Sample";

        public string Category => "Forms";

        Random random = new Random();
        Animation animation;
        Pin pin;

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var navigator = (Navigator)mapView.Navigator;

            var newRot = random.NextDouble() * 360.0;

            if (e.NumOfTaps == 2)
            {
                //navigator.RotateTo(newRot, 500);
                navigator.FlyTo(e.Point.ToMapsui(), mapView.Viewport.Resolution * 8, 5000);
            }
            else if (e.NumOfTaps == 1)
            {
                pin = new Pin(mapView)
                {
                    Label = $"AnimatedPin",
                    Position = e.Point,
                    Address = e.Point.ToString(),
                    Type = PinType.Pin,
                    Color = new Xamarin.Forms.Color(1, 0, 0),
                    Transparency = 0.5f,
                    Scale = 1,
                };
                mapView.Pins.Add(pin);

                var animations = new List<AnimationEntry>();

                var entry1 = new AnimationEntry(
                    start: pin.Scale,
                    end: pin.Scale * 2,
                    animationStart: 0,
                    animationEnd: .5,
                    easing: Easing.SinInOut,
                    tick: (entry, value) => { pin.Scale = (float)((float)entry.Start + ((float)entry.End - (float)entry.Start) * entry.Easing.Ease(value)); },
                    final: (entry) => { pin.Scale = (float)entry.End; }
                );
                animations.Add(entry1);

                var entry2 = new AnimationEntry(
                    start: pin.Scale * 2,
                    end: pin.Scale,
                    animationStart: .5,
                    animationEnd: 1,
                    easing: Easing.SinInOut,
                    tick: (entry, value) => { pin.Scale = (float)((float)entry.Start + ((float)entry.End - (float)entry.Start) * entry.Easing.Ease(value)); },
                    final: (entry) => { pin.Scale = (float)entry.End; }
                );
                animations.Add(entry2);

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

        private void PinFinal(AnimationEntry entry)
        {
            pin.Scale = (float)entry.End;
        }

        private void PinTick(AnimationEntry entry, double value)
        {
            pin.Scale = (float)((float)entry.Start + ((float)entry.End - (float)entry.Start) * entry.Easing.Ease(value));
        }

        public void Setup(IMapControl mapControl)
        {
            var mapView = mapControl as MapView;

            mapControl.Map = OsmSample.CreateMap();
        }
    }
}
