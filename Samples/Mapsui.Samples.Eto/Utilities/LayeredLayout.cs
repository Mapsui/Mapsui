
namespace Mapsui.Samples.Eto
{
    using System;
    using System.Linq;
    using global::Eto.Drawing;
    using global::Eto.Forms;
    public class LayeredLayout : PixelLayout
    {
        public void Add(Control control)
        {
            Add(control, Point.Empty);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            foreach (var control in Controls)
            {
                control.Size = this.Size;

                if (control is Layout layout)
                    layout.Update();
            }
        }
    }
}
